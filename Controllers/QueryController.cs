using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;

namespace SmartInsuranceHub.Controllers
{
    public class QueryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QueryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            var queries = await _context.Queries.OrderByDescending(q => q.send_date).ToListAsync();
            return View(queries);
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var query = await _context.Queries.FindAsync(id);
            if (query != null)
            {
                _context.Queries.Remove(query);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ReplyQuery(int id, string reply)
        {
            var query = await _context.Queries.FindAsync(id);
            if (query != null && !string.IsNullOrWhiteSpace(reply))
            {
                query.reply = reply;
                query.status = "Resolved";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Reply sent successfully.";
            }
            return RedirectToAction("Index");
        }
    }
}
