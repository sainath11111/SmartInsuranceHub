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
            
            // Temporary strategy for getting a unique plan_id for composite keys
            var maxPlanId = await _context.InsurancePlans.AnyAsync() 
                ? await _context.InsurancePlans.MaxAsync(p => p.plan_id) 
                : 0;
            model.plan_id = maxPlanId + 1;

            model.created_date = DateTime.UtcNow;
            model.status = "active";
            
            _context.InsurancePlans.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> Purchase(int planId, int companyId)
        {
            var plan = await _context.InsurancePlans.FirstOrDefaultAsync(p => p.plan_id == planId && p.company_id == companyId);
            if (plan == null) return NotFound();
            
            return View(plan);
        }

        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> ConfirmPurchase(int plan_id, int company_id, int duration)
        {
            var plan = await _context.InsurancePlans.FirstOrDefaultAsync(p => p.plan_id == plan_id && p.company_id == company_id);
            if (plan == null) return NotFound();
            
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
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
            
            return RedirectToAction("MyPolicies", "Customer");
        }
    }
}
