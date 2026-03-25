using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;

namespace SmartInsuranceHub.Controllers
{
    [Authorize]
    public class InsuranceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InsuranceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "CompanyOnly")]
        public async Task<IActionResult> Index()
        {
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var plans = await _context.InsurancePlans.Include(p => p.Agent).Where(p => p.company_id == cid).ToListAsync();
            return View(plans);
        }

        [Authorize(Policy = "CompanyOnly")]
        public async Task<IActionResult> CreatePlan()
        {
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            ViewBag.Agents = await _context.Agents.Where(a => a.company_id == cid && a.approved_status).ToListAsync();
            ViewBag.Types = await _context.InsuranceTypes.Where(t => t.status == "active").ToListAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CompanyOnly")]
        public async Task<IActionResult> CreatePlan(InsurancePlan model)
        {
            model.company_id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            var maxPlanId = await _context.InsurancePlans.AnyAsync() 
                ? await _context.InsurancePlans.MaxAsync(p => p.plan_id) 
                : 0;
            model.plan_id = maxPlanId + 1;

            model.created_date = DateTime.UtcNow;
            model.status = "active";
            
            _context.InsurancePlans.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Insurance plan created successfully!";
            return Redirect("/Insurance/Index");
        }

        // ========================================
        // Edit Plan (GET)
        // ========================================
        [Authorize(Policy = "CompanyOnly")]
        public async Task<IActionResult> EditPlan(int id)
        {
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var plan = await _context.InsurancePlans.FirstOrDefaultAsync(p => p.plan_id == id && p.company_id == cid);
            if (plan == null) return NotFound();

            ViewBag.Agents = await _context.Agents.Where(a => a.company_id == cid && a.approved_status).ToListAsync();
            ViewBag.Types = await _context.InsuranceTypes.Where(t => t.status == "active").ToListAsync();
            return View(plan);
        }

        // ========================================
        // Edit Plan (POST)
        // ========================================
        [HttpPost]
        [Authorize(Policy = "CompanyOnly")]
        public async Task<IActionResult> EditPlan(InsurancePlan model)
        {
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var plan = await _context.InsurancePlans.FirstOrDefaultAsync(p => p.plan_id == model.plan_id && p.company_id == cid);
            if (plan == null) return NotFound();

            plan.plan_name = model.plan_name;
            plan.agent_id = model.agent_id;
            plan.type_id = model.type_id;
            plan.premium_amount = model.premium_amount;
            plan.coverage_amount = model.coverage_amount;
            plan.duration_months = model.duration_months;
            plan.description = model.description;
            plan.status = model.status;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Insurance plan updated successfully!";
            return Redirect("/Insurance/Index");
        }

        // ========================================
        // Delete Plan (POST)
        // ========================================
        [HttpPost]
        [Authorize(Policy = "CompanyOnly")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var plan = await _context.InsurancePlans.FirstOrDefaultAsync(p => p.plan_id == id && p.company_id == cid);
            if (plan == null) return NotFound();

            // Check if any policies exist for this plan
            var hasPolicies = await _context.Policies.AnyAsync(p => p.plan_id == id && p.company_id == cid);
            if (hasPolicies)
            {
                TempData["Error"] = "Cannot delete this plan because active policies are linked to it.";
                return Redirect("/Insurance/Index");
            }

            _context.InsurancePlans.Remove(plan);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Insurance plan deleted successfully.";
            return Redirect("/Insurance/Index");
        }

        // ========================================
        // Purchase - VERIFICATION GATE (GET)
        // ========================================
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> Purchase(int planId, int companyId)
        {
            var plan = await _context.InsurancePlans.FirstOrDefaultAsync(p => p.plan_id == planId && p.company_id == companyId);
            if (plan == null) return NotFound();

            // --- CRITICAL: Verify customer before showing purchase page ---
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var customer = await _context.Customers.FindAsync(cid);

            if (customer == null || customer.verification_status != "verified")
            {
                // Pass flag to view — view will show the blocked state
                ViewBag.IsVerified = false;
                ViewBag.VerificationStatus = customer?.verification_status ?? "unverified";
                return View(plan);
            }

            ViewBag.IsVerified = true;
            return View(plan);
        }

        // ========================================
        // ConfirmPurchase - VERIFICATION GATE (POST)
        // ========================================
        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> ConfirmPurchase(int plan_id, int company_id, int duration)
        {
            var plan = await _context.InsurancePlans.FirstOrDefaultAsync(p => p.plan_id == plan_id && p.company_id == company_id);
            if (plan == null) return NotFound();
            
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // --- CRITICAL: Server-side verification check (cannot be bypassed from frontend) ---
            var customer = await _context.Customers.FindAsync(cid);
            if (customer == null || customer.verification_status != "verified")
            {
                TempData["Error"] = "You must complete document verification before purchasing insurance.";
                return RedirectToAction("BrowsePlans", "Customer");
            }

            var policy = new Policy
            {
                policy_no = "POL" + DateTime.UtcNow.Ticks.ToString().Substring(8),
                customer_id = cid,
                agent_id = plan.agent_id,
                plan_id = plan.plan_id,
                company_id = plan.company_id,
                start_date = DateTime.UtcNow,
                end_date = DateTime.UtcNow.AddMonths(duration),
                policy_status = "active",
                premium_amount = plan.premium_amount,
                created_by = "Customer"
            };
            
            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Policy purchased successfully! Welcome aboard.";
            return RedirectToAction("MyPolicies", "Customer");
        }
    }
}
