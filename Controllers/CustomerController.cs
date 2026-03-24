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

            ViewBag.MyPolicies = await _context.Policies.Where(p => p.customer_id == cid).CountAsync();
            ViewBag.TotalPayments = await _context.Payments.Where(p => p.customer_id == cid).SumAsync(p => p.amount);
            ViewBag.IsVerified = customer?.verification_status == "verified";
            ViewBag.VerificationStatus = customer?.verification_status ?? "unverified";

            // Pass document progress for the dashboard banner
            var docService = HttpContext.RequestServices.GetRequiredService<DocumentService>();
            var (completed, total, percentage) = await docService.GetCompletionAsync("Customer", cid);
            ViewBag.Completed = completed;
            ViewBag.Total = total;
            ViewBag.Percentage = percentage;

            return View();
        }

        public async Task<IActionResult> BrowsePlans()
        {
            var cid = GetCustomerId();
            var customer = await _context.Customers.FindAsync(cid);
            ViewBag.IsVerified = customer?.verification_status == "verified";

            var plans = await _context.InsurancePlans.Include(p => p.Company).Where(p => p.status == "active").ToListAsync();
            return View(plans);
        }

        public async Task<IActionResult> MyPolicies()
        {
            var cid = GetCustomerId();
            var policies = await _context.Policies.Include(p => p.InsurancePlan).Include(p => p.Agent)
                .Where(p => p.customer_id == cid).ToListAsync();
            return View(policies);
        }

        [HttpGet]
        public async Task<IActionResult> SubmitQuery()
        {
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
            return View();
        }
    }
}
