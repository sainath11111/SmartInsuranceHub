using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;
using SmartInsuranceHub.Services;

namespace SmartInsuranceHub.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalCompanies = await _context.Companies.CountAsync();
            ViewBag.TotalAgents = await _context.Agents.CountAsync();
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            ViewBag.TotalPolicies = await _context.Policies.CountAsync();
            ViewBag.PendingDocs = await _context.UserDocuments.Where(d => d.status == "pending").CountAsync();
            
            return View();
        }

        public async Task<IActionResult> Companies()
        {
            var companies = await _context.Companies.ToListAsync();
            return View(companies);
        }

        public async Task<IActionResult> ApproveCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                company.status = "approved";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Companies");
        }

        public async Task<IActionResult> RejectCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                company.status = "rejected";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Companies");
        }

        public async Task<IActionResult> Agents()
        {
            var agents = await _context.Agents.Include(a => a.Company).ToListAsync();
            return View(agents);
        }

        public async Task<IActionResult> ApproveAgent(int id)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent != null)
            {
                agent.approved_status = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Agents");
        }

        public async Task<IActionResult> Customers()
        {
            var customers = await _context.Customers.ToListAsync();
            return View(customers);
        }

        public async Task<IActionResult> InsuranceTypes()
        {
            var types = await _context.InsuranceTypes.OrderBy(t => t.type_id).ToListAsync();
            return View(types);
        }

        [HttpPost]
        public async Task<IActionResult> AddInsuranceType(string type_name, string description, string icon)
        {
            if (!string.IsNullOrWhiteSpace(type_name))
            {
                _context.InsuranceTypes.Add(new InsuranceType
                {
                    type_name = type_name.Trim(),
                    description = description?.Trim(),
                    icon = string.IsNullOrEmpty(icon) ? "bi-shield" : icon,
                    status = "active",
                    created_at = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("InsuranceTypes");
        }

        public async Task<IActionResult> DeleteInsuranceType(int id)
        {
            var type = await _context.InsuranceTypes.FindAsync(id);
            if (type != null)
            {
                _context.InsuranceTypes.Remove(type);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("InsuranceTypes");
        }

        public async Task<IActionResult> Queries()
        {
            var queries = await _context.Queries.Include(q => q.Customer).ToListAsync();
            return View(queries);
        }

        // ========================================
        // Document Verification Panel
        // ========================================
        public async Task<IActionResult> DocumentVerification()
        {
            var documents = await _context.UserDocuments
                .OrderByDescending(d => d.uploaded_at)
                .ToListAsync();

            // Build lookup dictionaries for user names
            var customerIds = documents.Where(d => d.user_type == "Customer").Select(d => d.user_id).Distinct().ToList();
            var agentIds = documents.Where(d => d.user_type == "Agent").Select(d => d.user_id).Distinct().ToList();

            var customerNames = await _context.Customers
                .Where(c => customerIds.Contains(c.customer_id))
                .ToDictionaryAsync(c => c.customer_id, c => c.full_name);
            
            var agentNames = await _context.Agents
                .Where(a => agentIds.Contains(a.agent_id))
                .ToDictionaryAsync(a => a.agent_id, a => a.full_name);

            ViewBag.CustomerNames = customerNames;
            ViewBag.AgentNames = agentNames;

            return View(documents);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveDocument(int id)
        {
            var doc = await _context.UserDocuments.FindAsync(id);
            if (doc != null)
            {
                var adminId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "0");
                doc.status = "approved";
                doc.reviewed_by = adminId;
                doc.reviewed_at = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Check if all docs are now approved for this user
                var docService = HttpContext.RequestServices.GetRequiredService<DocumentService>();
                await docService.UpdateVerificationStatusAsync(doc.user_type, doc.user_id);
            }
            return RedirectToAction("DocumentVerification");
        }

        [HttpPost]
        public async Task<IActionResult> RejectDocument(int id, string? reason)
        {
            var doc = await _context.UserDocuments.FindAsync(id);
            if (doc != null)
            {
                var adminId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "0");
                doc.status = "rejected";
                doc.rejection_reason = reason ?? "Document rejected by admin.";
                doc.reviewed_by = adminId;
                doc.reviewed_at = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("DocumentVerification");
        }
    }
}
