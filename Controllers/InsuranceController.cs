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
            var plans = await _context.InsurancePlans.Include(p => p.Agent).AsNoTracking().Where(p => p.company_id == cid).ToListAsync();
            return View(plans);
        }

        [Authorize(Policy = "CompanyOnly")]
        public async Task<IActionResult> CreatePlan()
        {
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            ViewBag.Types = await _context.InsuranceTypes.AsNoTracking().Where(t => t.status == "active").ToListAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CompanyOnly")]
        public async Task<IActionResult> CreatePlan(InsurancePlan model)
        {
            model.company_id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            model.agent_id = null;
            
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
            var plan = await _context.InsurancePlans.AsNoTracking().FirstOrDefaultAsync(p => p.plan_id == id && p.company_id == cid);
            if (plan == null) return NotFound();

            ViewBag.Types = await _context.InsuranceTypes.AsNoTracking().Where(t => t.status == "active").ToListAsync();
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


    }
}
