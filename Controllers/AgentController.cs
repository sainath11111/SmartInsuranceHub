using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;

namespace SmartInsuranceHub.Controllers
{
    [Authorize(Policy = "AgentOnly")]
    public class AgentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgentController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetAgentId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return id != null ? int.Parse(id) : 0;
        }

        public async Task<IActionResult> Index()
        {
            var aid = GetAgentId();
            ViewBag.TotalPolicies = await _context.Policies.Where(p => p.agent_id == aid).CountAsync();
            ViewBag.TotalPayments = await _context.Payments.Where(p => p.received_by_agent == aid).SumAsync(p => p.amount);
            
            return View();
        }

        public async Task<IActionResult> Policies()
        {
            var aid = GetAgentId();
            var policies = await _context.Policies.Include(p => p.Customer).Include(p => p.InsurancePlan).Where(p => p.agent_id == aid).ToListAsync();
            return View(policies);
        }
    }
}
