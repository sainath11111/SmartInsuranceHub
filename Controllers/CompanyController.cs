using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;

namespace SmartInsuranceHub.Controllers
{
    [Authorize(Policy = "CompanyOnly")]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCompanyId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return id != null ? int.Parse(id) : 0;
        }

        public async Task<IActionResult> Index()
        {
            var cid = GetCompanyId();
            ViewBag.TotalAgents = await _context.Agents.Where(a => a.company_id == cid).CountAsync();
            ViewBag.TotalPlans = await _context.InsurancePlans.Where(p => p.company_id == cid).CountAsync();
            ViewBag.TotalPolicies = await _context.Policies.Where(p => p.company_id == cid).CountAsync();
            ViewBag.TotalSales = await _context.Policies.Where(p => p.company_id == cid).SumAsync(p => p.premium_amount);
            
            return View();
        }

        public async Task<IActionResult> Agents()
        {
            var cid = GetCompanyId();
            var agents = await _context.Agents.Where(a => a.company_id == cid).ToListAsync();
            
            var performance = await _context.Policies
                .Where(p => p.company_id == cid && p.agent_id != null && p.policy_status == "active")
                .GroupBy(p => p.agent_id)
                .Select(g => new { AgentId = g.Key ?? 0, Count = g.Count() })
                .ToDictionaryAsync(x => x.AgentId, x => x.Count);
                
            ViewBag.AgentPerformance = performance;
            
            return View(agents);
        }

        public async Task<IActionResult> Dashboard()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAgent(int id)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent != null && agent.company_id == GetCompanyId())
            {
                _context.Agents.Remove(agent);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Agent successfully removed from your company.";
            }
            return RedirectToAction("Agents");
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAgent(int id)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent != null && agent.company_id == GetCompanyId())
            {
                agent.approved_status = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Agent successfully approved and is now Active.";
            }
            return RedirectToAction("Agents");
        }
        
        [HttpGet]
        public async Task<IActionResult> PendingPayments()
        {
            var cid = GetCompanyId();
            var payments = await _context.Payments
                .Include(p => p.Policy).ThenInclude(p => p!.InsurancePlan)
                .Include(p => p.Policy!).ThenInclude(p => p.Customer)
                .Include(p => p.Agent)
                .Where(p => p.Policy!.company_id == cid && p.payment_status == "pending")
                .OrderByDescending(p => p.payment_date)
                .ToListAsync();

            return View(payments);
        }

        [HttpPost]
        public async Task<IActionResult> ApprovePayment(int id)
        {
            var cid = GetCompanyId();
            var payment = await _context.Payments
                .Include(p => p.Policy).ThenInclude(p => p!.InsurancePlan)
                .FirstOrDefaultAsync(p => p.payment_id == id && p.Policy!.company_id == cid && p.payment_status == "pending");

            if (payment?.Policy != null && payment.Policy.InsurancePlan != null)
            {
                // Approve Payment
                payment.payment_status = "completed";
                await _context.SaveChangesAsync();

                // Activate Policy
                var plan = payment.Policy.InsurancePlan;
                payment.Policy.policy_status = "active";
                payment.Policy.start_date = DateTime.UtcNow;
                payment.Policy.end_date = DateTime.UtcNow.AddMonths(plan.duration_months);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Payment approved! The policy is now active.";
            }

            return RedirectToAction("PendingPayments");
        }

        [HttpPost]
        public async Task<IActionResult> RejectPayment(int id, string reason)
        {
            var cid = GetCompanyId();
            var payment = await _context.Payments
                .Include(p => p.Policy)
                .FirstOrDefaultAsync(p => p.payment_id == id && p.Policy!.company_id == cid && p.payment_status == "pending");

            if (payment?.Policy != null)
            {
                var rejectionMessage = string.IsNullOrWhiteSpace(reason) ? "Payment was rejected by the company." : reason;
                
                await _context.Database.ExecuteSqlRawAsync(
                    @"UPDATE ""Payments"" SET ""PaymentStatus"" = 'rejected', ""RejectionReason"" = {0} WHERE ""PaymentId"" = {1};
                      UPDATE ""Policies"" SET ""PolicyStatus"" = 'pending_payment' WHERE ""PolicyId"" = {2};",
                    rejectionMessage, payment.payment_id, payment.policy_id);

                TempData["Success"] = "Payment has been rejected. The policy remains strictly pending.";
            }

            return RedirectToAction("PendingPayments");
        }
        // ========================================
        // Analytics & Details Dashboard
        // ========================================
        public async Task<IActionResult> Analytics()
        {
            var cid = GetCompanyId();
            
            var policies = await _context.Policies
                .Include(p => p.Customer)
                .Include(p => p.Agent)
                .Include(p => p.InsurancePlan)
                .Where(p => p.company_id == cid)
                .OrderByDescending(p => p.start_date)
                .ToListAsync();

            var monthlySales = await _context.Policies
                .Where(p => p.company_id == cid && p.policy_status == "active")
                .GroupBy(p => new { p.start_date.Year, p.start_date.Month })
                .Select(g => new { 
                    Month = g.Key.Month, 
                    Year = g.Key.Year, 
                    Revenue = g.Sum(x => x.premium_amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            ViewBag.MonthlySales = monthlySales;

            var totalRequests = await _context.PolicyRequests.Where(r => r.company_id == cid).CountAsync();
            var approvedReqs = await _context.PolicyRequests.Where(r => r.company_id == cid && r.status == "approved").CountAsync();
            var rejectedReqs = await _context.PolicyRequests.Where(r => r.company_id == cid && r.status == "rejected").CountAsync();

            ViewBag.TotalRequests = totalRequests;
            ViewBag.ApprovedRequests = approvedReqs;
            ViewBag.RejectedRequests = rejectedReqs;

            var topAgents = await _context.Policies
                .Include(p => p.Agent)
                .Where(p => p.company_id == cid && p.policy_status == "active")
                .GroupBy(p => new { ID = p.agent_id, Name = p.Agent!.full_name })
                .Select(g => new {
                    Name = g.Key.Name,
                    PoliciesSold = g.Count(),
                    Revenue = g.Sum(x => x.premium_amount)
                })
                .OrderByDescending(x => x.PoliciesSold)
                .Take(5)
                .ToListAsync();
            
            ViewBag.TopAgents = topAgents;

            return View(policies);
        }

        public async Task<IActionResult> AgentDetails(int id)
        {
            var cid = GetCompanyId();
            var agent = await _context.Agents
                .Include(a => a.AgentCities)
                .FirstOrDefaultAsync(a => a.agent_id == id && a.company_id == cid);

            if (agent == null) return NotFound();

            var policies = await _context.Policies
                .Include(p => p.Customer)
                .Include(p => p.InsurancePlan)
                .Where(p => p.agent_id == id && p.company_id == cid)
                .ToListAsync();

            ViewBag.TotalPoliciesSold = policies.Count(p => p.policy_status == "active");
            ViewBag.TotalRevenue = policies.Where(p => p.policy_status == "active").Sum(p => (decimal?)p.premium_amount) ?? 0;
            
            var monthlySales = policies
                .Where(p => p.policy_status == "active")
                .GroupBy(p => new { p.start_date.Year, p.start_date.Month })
                .Select(g => new { 
                    Month = g.Key.Month, 
                    Year = g.Key.Year, 
                    Revenue = g.Sum(x => x.premium_amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            ViewBag.MonthlySales = monthlySales;
            ViewBag.ActivePoliciesCount = policies.Count(p => p.policy_status == "active");
            ViewBag.PendingPoliciesCount = policies.Count(p => p.policy_status == "pending_approval" || p.policy_status == "pending_payment");
            ViewBag.ExpiredPoliciesCount = policies.Count(p => p.policy_status == "expired");
            ViewBag.AgentPolicies = policies.OrderByDescending(p => p.start_date).ToList();

            ViewBag.Documents = await _context.UserDocuments.Where(d => d.user_type == "Agent" && d.user_id == id).ToListAsync();

            return View(agent);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAgentStatus(int id)
        {
            var cid = GetCompanyId();
            var agent = await _context.Agents.FirstOrDefaultAsync(a => a.agent_id == id && a.company_id == cid);
            if (agent != null)
            {
                agent.approved_status = !agent.approved_status;
                await _context.SaveChangesAsync();
                TempData["Success"] = agent.approved_status ? "Agent activated successfully." : "Agent deactivated successfully.";
            }
            return RedirectToAction("AgentDetails", new { id = id });
        }

        public async Task<IActionResult> CustomerDetails(int id)
        {
            var cid = GetCompanyId();
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            var policies = await _context.Policies
                .Include(p => p.InsurancePlan)
                .Include(p => p.Agent)
                .Where(p => p.customer_id == id && p.company_id == cid)
                .ToListAsync();

            ViewBag.Policies = policies;
            ViewBag.Documents = await _context.UserDocuments.Where(d => d.user_type == "Customer" && d.user_id == id).ToListAsync();

            return View(customer);
        }
    }
}
