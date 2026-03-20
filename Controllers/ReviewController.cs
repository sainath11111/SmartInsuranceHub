using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;

namespace SmartInsuranceHub.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> WriteReview(int planId, int companyId)
        {
            var plan = await _context.InsurancePlans.FirstOrDefaultAsync(p => p.plan_id == planId && p.company_id == companyId);
            if (plan == null) return NotFound();
            
            ViewBag.Plan = plan;
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> SubmitReview(int plan_id, int company_id, int rating, string comment)
        {
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            var review = new Review
            {
                customer_id = cid,
                plan_id = plan_id,
                company_id = company_id,
                rating = rating,
                comment = comment,
                created_at = DateTime.UtcNow
            };
            
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("MyPolicies", "Customer");
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> PlanReviews(int planId, int companyId)
        {
            var reviews = await _context.Reviews.Include(r => r.Customer).Where(r => r.plan_id == planId && r.company_id == companyId).ToListAsync();
            return View(reviews);
        }
    }
}
