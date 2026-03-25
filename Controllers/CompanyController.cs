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
        
    }
}
