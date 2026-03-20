using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;

namespace SmartInsuranceHub.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> MakePayment(int policyId)
        {
            var policy = await _context.Policies.FindAsync(policyId);
            if (policy == null) return NotFound();
            
            return View(policy);
        }

        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> ProcessPayment(int policy_id, decimal amount, string method)
        {
            var policy = await _context.Policies.FindAsync(policy_id);
            if (policy == null) return NotFound();
            
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var payment = new Payment
            {
                policy_id = policy_id,
                customer_id = cid,
                amount = amount,
                method = method,
                payment_date = DateTime.UtcNow,
                payment_status = "completed",
                received_by_agent = policy.agent_id
            };
            
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Index", "Customer");
        }
    }
}
