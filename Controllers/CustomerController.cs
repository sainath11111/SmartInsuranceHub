using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;
using SmartInsuranceHub.Services;

namespace SmartInsuranceHub.Controllers
{
    [Authorize(Policy = "CustomerOnly")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCustomerId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return id != null ? int.Parse(id) : 0;
        }

        public async Task<IActionResult> Index()
        {
            var cid = GetCustomerId();
            var customer = await _context.Customers.FindAsync(cid);

            ViewBag.CustomerName = customer?.full_name ?? "User";
            ViewBag.MyPolicies = await _context.Policies.Where(p => p.customer_id == cid).CountAsync();
            ViewBag.ActivePolicies = await _context.Policies.Where(p => p.customer_id == cid && p.policy_status == "active").CountAsync();
            ViewBag.TotalPayments = await _context.Payments.Where(p => p.customer_id == cid).SumAsync(p => p.amount);
            ViewBag.IsVerified = customer?.verification_status == "verified";
            ViewBag.VerificationStatus = customer?.verification_status ?? "unverified";

            // Pass document progress for the dashboard banner
            var docService = HttpContext.RequestServices.GetRequiredService<DocumentService>();
            var (completed, total, percentage) = await docService.GetCompletionAsync("Customer", cid);
            ViewBag.Completed = completed;
            ViewBag.Total = total;
            ViewBag.Percentage = percentage;

            ViewBag.PendingRequests = await _context.PolicyRequests.Where(r => r.customer_id == cid && r.status == "pending").CountAsync();

            // Recent payment history (last 5)
            ViewBag.RecentPayments = await _context.Payments.AsNoTracking()
                .Include(p => p.Policy).ThenInclude(p => p!.InsurancePlan)
                .Where(p => p.customer_id == cid)
                .OrderByDescending(p => p.payment_date)
                .Take(5)
                .ToListAsync();

            // Personalized Recommendations based on age
            var age = customer?.age ?? 25;
            IQueryable<InsurancePlan> recQuery = _context.InsurancePlans.AsNoTracking()
                .Include(p => p.Company)
                .Where(p => p.status == "active");

            if (age < 30)
            {
                // Young adults: prioritize affordable health & auto plans
                recQuery = recQuery.OrderBy(p => p.premium_amount);
                ViewBag.RecReason = "Based on your age, we recommend affordable health & auto plans to get you started.";
            }
            else if (age <= 50)
            {
                // Mid-life: prioritize high coverage term life & family health
                recQuery = recQuery.OrderByDescending(p => p.coverage_amount);
                ViewBag.RecReason = "At your life stage, high-coverage term life & family plans offer the best protection.";
            }
            else
            {
                // Seniors: prioritize comprehensive, moderate-premium plans
                recQuery = recQuery.OrderByDescending(p => p.coverage_amount / p.premium_amount);
                ViewBag.RecReason = "We recommend plans with the best value-to-premium ratio for comprehensive senior coverage.";
            }

            ViewBag.Recommendations = await recQuery.Take(4).ToListAsync();

            return View();
        }

        public async Task<IActionResult> BrowsePlans()
        {
            var cid = GetCustomerId();
            var customer = await _context.Customers.FindAsync(cid);
            ViewBag.IsVerified = customer?.verification_status == "verified";

            var plans = await _context.InsurancePlans.AsNoTracking().Include(p => p.Company).Where(p => p.status == "active").ToListAsync();
            
            // Pass filter data
            ViewBag.InsuranceTypes = await _context.InsuranceTypes.AsNoTracking().Where(t => t.status == "active").ToListAsync();
            ViewBag.Companies = await _context.Companies.AsNoTracking().Where(c => c.status == "approved").ToListAsync();
            ViewBag.MaxPremium = plans.Any() ? plans.Max(p => p.premium_amount) : 50000;
            ViewBag.MaxCoverage = plans.Any() ? plans.Max(p => p.coverage_amount) : 5000000;
            
            return View(plans);
        }

        public async Task<IActionResult> PlanDetails(int id)
        {
            var plan = await _context.InsurancePlans.AsNoTracking()
                .Include(p => p.Company)
                .FirstOrDefaultAsync(p => p.plan_id == id);

            if (plan == null) return NotFound();

            var cid = GetCustomerId();
            var customer = await _context.Customers.FindAsync(cid);
            ViewBag.IsVerified = customer?.verification_status == "verified";

            return View(plan);
        }

        public async Task<IActionResult> ComparePlans(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return RedirectToAction("BrowsePlans");

            var idPairs = ids.Split(',')
                .Select(s => s.Split('_'))
                .Where(p => p.Length == 2)
                .Select(p => new { PlanId = int.Parse(p[0]), CompanyId = int.Parse(p[1]) })
                .ToList();

            var plans = new List<InsurancePlan>();
            foreach (var pair in idPairs.Take(3))
            {
                var plan = await _context.InsurancePlans.AsNoTracking()
                    .Include(p => p.Company)
                    .FirstOrDefaultAsync(p => p.plan_id == pair.PlanId && p.company_id == pair.CompanyId);
                if (plan != null) plans.Add(plan);
            }

            if (plans.Count < 2)
            {
                TempData["Error"] = "Please select at least 2 plans to compare.";
                return RedirectToAction("BrowsePlans");
            }

            // Pass insurance type names
            var typeIds = plans.Select(p => p.type_id).Distinct().ToList();
            var types = await _context.InsuranceTypes.Where(t => typeIds.Contains(t.type_id)).ToDictionaryAsync(t => t.type_id, t => t.type_name);
            ViewBag.TypeNames = types;

            return View(plans);
        }

        // ========================================
        // Agent-Assisted Purchase Flow
        // ========================================
        public async Task<IActionResult> SelectAgent(int planId, int companyId)
        {
            var plan = await _context.InsurancePlans.FirstOrDefaultAsync(p => p.plan_id == planId && p.company_id == companyId);
            if (plan == null) return NotFound();

            var cid = GetCustomerId();
            var customer = await _context.Customers.FindAsync(cid);

            if (customer == null || customer.verification_status != "verified")
            {
                TempData["Error"] = "Complete document verification before requesting insurance.";
                return RedirectToAction("BrowsePlans");
            }

            var city = customer.city ?? "";

            // Fetch ALL approved agents for this company
            var agents = await _context.Agents.AsNoTracking()
                .Include(a => a.AgentCities)
                .Where(a => a.company_id == companyId && a.approved_status)
                .ToListAsync();

            ViewBag.Plan = plan;
            ViewBag.City = city;
            ViewBag.CustomerCity = city;
            return View(agents);
        }

        public async Task<IActionResult> AgentProfile(int agentId)
        {
            var agent = await _context.Agents.AsNoTracking().Include(a => a.Company).FirstOrDefaultAsync(a => a.agent_id == agentId);
            if (agent == null) return NotFound();

            var plans = await _context.InsurancePlans.AsNoTracking()
                .Where(p => p.company_id == agent.company_id && p.status == "active")
                .ToListAsync();

            ViewBag.Plans = plans;
            return View(agent);
        }

        [HttpPost]
        public async Task<IActionResult> RequestPolicy(int planId, int companyId, int agentId)
        {
            var cid = GetCustomerId();
            
            // Check if customer already owns this policy
            var alreadyOwns = await _context.Policies.AnyAsync(p => p.customer_id == cid && p.plan_id == planId && p.company_id == companyId);
            if (alreadyOwns)
            {
                TempData["Error"] = "You already own this policy. Multiple identical policies are not allowed.";
                return RedirectToAction("BrowsePlans");
            }

            // Check if there's already a pending request
            var existingRequest = await _context.PolicyRequests.AnyAsync(r => r.customer_id == cid && r.plan_id == planId && r.company_id == companyId && r.status == "pending");
            if (existingRequest)
            {
                TempData["Error"] = "You already have a pending request for this policy.";
                return RedirectToAction("MyRequests");
            }

            var req = new PolicyRequest
            {
                customer_id = cid,
                agent_id = agentId,
                plan_id = planId,
                company_id = companyId,
                status = "pending",
                created_at = DateTime.UtcNow
            };

            _context.PolicyRequests.Add(req);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your policy request has been sent to the agent successfully!";
            return RedirectToAction("MyRequests");
        }

        public async Task<IActionResult> MyRequests()
        {
            var cid = GetCustomerId();
            var requests = await _context.PolicyRequests.AsNoTracking()
                .Include(r => r.InsurancePlan)
                .Include(r => r.Agent).ThenInclude(a => a!.Company)
                .Where(r => r.customer_id == cid)
                .OrderByDescending(r => r.created_at)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> MyPolicies()
        {
            var cid = GetCustomerId();
            var policies = await _context.Policies.AsNoTracking().Include(p => p.InsurancePlan).Include(p => p.Agent)
                .Where(p => p.customer_id == cid).ToListAsync();
            return View(policies);
        }

        [HttpGet]
        public async Task<IActionResult> SubmitQuery()
        {
            var cid = GetCustomerId();
            ViewBag.Queries = await _context.Queries.AsNoTracking().Where(q => q.customer_id == cid).OrderByDescending(q => q.send_date).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitQuery(Query model)
        {
            var cid = GetCustomerId();
            var customer = await _context.Customers.FindAsync(cid);
            if (customer == null) return NotFound();

            model.customer_id = cid;
            model.name = customer.full_name;
            model.email = customer.email;
            model.phone = customer.phone;
            model.send_date = DateTime.UtcNow;

            _context.Queries.Add(model);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Query submitted successfully!";
            ViewBag.Queries = await _context.Queries.AsNoTracking().Where(q => q.customer_id == cid).OrderByDescending(q => q.send_date).ToListAsync();
            return View(new Query());
        }
    }
}
