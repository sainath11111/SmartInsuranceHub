using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;

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
            ViewBag.PendingRequests = await _context.PolicyRequests.Where(r => r.agent_id == aid && r.status == "pending").CountAsync();
            
            return View();
        }

        public async Task<IActionResult> Policies()
        {
            var aid = GetAgentId();
            var policies = await _context.Policies.Include(p => p.Customer).Include(p => p.InsurancePlan).Where(p => p.agent_id == aid).ToListAsync();
            return View(policies);
        }

        // ========================================
        // Agent Profile
        // ========================================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var aid = GetAgentId();
            var agent = await _context.Agents
                .Include(a => a.AgentCities)
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.agent_id == aid);

            if (agent == null) return NotFound();
            
            // Re-fetch documents to show in profile
            ViewBag.Documents = await _context.UserDocuments
                .Where(d => d.user_type == "Agent" && d.user_id == aid)
                .ToListAsync();

            return View(agent);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(string full_name, string email, string phone, int experience_years, 
            string aadhaar, string pan, DateTime dob, List<string> cities, IFormFile? profilePhoto)
        {
            var aid = GetAgentId();
            var agent = await _context.Agents
                .Include(a => a.AgentCities)
                .FirstOrDefaultAsync(a => a.agent_id == aid);

            if (agent == null) return NotFound();

            agent.full_name = full_name;
            agent.email = email;
            agent.phone = phone;
            agent.experience_years = experience_years;
            agent.aadhaar = aadhaar;
            agent.pan = pan;
            if (dob != default) agent.dob = dob;

            // Handle cities update using raw SQL to bypass Npgsql batching bug
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"AgentCities\" WHERE \"AgentId\" = {0}", aid);

            if (cities != null && cities.Any())
            {
                foreach (var city in cities)
                {
                    if (!string.IsNullOrWhiteSpace(city))
                    {
                        var c = city.Trim();
                        await _context.Database.ExecuteSqlRawAsync("INSERT INTO \"AgentCities\" (\"AgentId\", \"CityName\") VALUES ({0}, {1})", aid, c);
                    }
                }
            }
            
            // Handle Profile Photo upload
            if (profilePhoto != null && profilePhoto.Length > 0)
            {
                var r2Service = HttpContext.RequestServices.GetRequiredService<SmartInsuranceHub.Services.R2StorageService>();
                var (url, error) = await r2Service.UploadFileAsync(profilePhoto, "Agent", aid, "profile_photo");
                
                if (url != null)
                {
                    agent.profile_photo = url;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully!";
            
            return RedirectToAction("Profile");
        }

        // ========================================
        // Policy Requests Management
        // ========================================
        public async Task<IActionResult> Requests()
        {
            var aid = GetAgentId();
            var requests = await _context.PolicyRequests
                .Include(r => r.Customer)
                .Include(r => r.InsurancePlan)
                .Where(r => r.agent_id == aid)
                .OrderByDescending(r => r.created_at)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> RequestDetails(int id)
        {
            var aid = GetAgentId();
            var req = await _context.PolicyRequests
                .Include(r => r.Customer)
                .Include(r => r.InsurancePlan)
                .FirstOrDefaultAsync(r => r.request_id == id && r.agent_id == aid);

            if (req == null) return NotFound();

            if (req.status == "approved")
            {
                ViewBag.CustomerDocs = await _context.UserDocuments
                    .Where(d => d.user_type == "Customer" && d.user_id == req.customer_id)
                    .ToListAsync();
            }

            return View(req);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var aid = GetAgentId();
            var req = await _context.PolicyRequests
                .Include(r => r.InsurancePlan)
                .FirstOrDefaultAsync(r => r.request_id == id && r.agent_id == aid && r.status == "pending");

            if (req != null)
            {
                req.status = "approved";
                req.reviewed_at = DateTime.UtcNow;

                // Auto-create the policy via Raw SQL to completely bypass Npgsql batching bug
                var policyNo = "POL" + DateTime.UtcNow.Ticks.ToString().Substring(8);
                var startDate = DateTime.UtcNow;
                var endDate = DateTime.UtcNow.AddMonths(req.InsurancePlan!.duration_months);
                
                await _context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO ""Policies"" 
                    (""PolicyNo"", ""CustomerId"", ""AgentId"", ""PlanId"", ""CompanyId"", ""StartDate"", ""EndDate"", ""PolicyStatus"", ""PremiumAmount"", ""CreatedBy"")
                    VALUES 
                    ({0}, {1}, {2}, {3}, {4}, {5}, {6}, 'pending_payment', {7}, 'Agent')",
                    policyNo, req.customer_id, req.agent_id, req.plan_id, req.company_id, startDate, endDate, req.InsurancePlan.premium_amount);
                
                try
                {
                    await _context.SaveChangesAsync(); // Save the PolicyRequest update
                    TempData["Success"] = "Request approved and Policy has been issued successfully.";
                }
                catch (Exception ex)
                {
                    var inner = ex;
                    while (inner.InnerException != null) inner = inner.InnerException;
                    return BadRequest("FAILED TO SAVE: " + inner.Message + "\n\n" + ex.ToString());
                }
            }

            return RedirectToAction("RequestDetails", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(int id, string reason)
        {
            var aid = GetAgentId();
            var req = await _context.PolicyRequests
                .FirstOrDefaultAsync(r => r.request_id == id && r.agent_id == aid && r.status == "pending");

            if (req != null)
            {
                req.status = "rejected";
                req.rejection_reason = reason;
                req.reviewed_at = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Request rejected.";
            }

            return RedirectToAction("Requests");
        }
    }
}
