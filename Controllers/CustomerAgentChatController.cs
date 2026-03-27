using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;
using System.Security.Claims;

namespace SmartInsuranceHub.Controllers
{
    [Authorize(Roles = "Customer,Agent")]
    public class CustomerAgentChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerAgentChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Main Chat Interface
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(userIdStr, out int currentUserId) || string.IsNullOrEmpty(role))
                return Unauthorized();
                
            if (role != "Customer" && role != "Agent")
                return Forbid();

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentUserRole = role;

            if (role == "Customer")
            {
                var customer = await _context.Customers.FindAsync(currentUserId);
                var city = customer?.city ?? "";
                var contacts = await _context.Agents
                    .AsNoTracking()
                    .Include(a => a.Company)
                    .Where(a => a.approved_status)
                    .Select(a => new { 
                        Id = a.agent_id, 
                        Name = a.full_name, 
                        Role = "Agent", 
                        Avatar = a.profile_photo,
                        City = a.city,
                        CompanyName = a.Company != null ? a.Company.company_name : ""
                    })
                    .ToListAsync();
                ViewBag.Contacts = contacts;
                ViewBag.CustomerCity = city;
                ViewBag.OnlineAgentIds = SmartInsuranceHub.Hubs.ChatHub.OnlineAgents.Keys.ToList();
            }
            else if (role == "Agent")
            {
                var agent = await _context.Agents.FindAsync(currentUserId);
                var city = agent?.city ?? "";
                var contacts = await _context.Customers
                    .Where(c => c.status == "active" && (c.city ?? "").ToLower() == city.ToLower())
                    .Select(c => new { 
                        Id = c.customer_id, 
                        Name = c.full_name, 
                        Role = "Customer", 
                        Avatar = "",
                        City = c.city,
                        CompanyName = ""
                    })
                    .ToListAsync();
                ViewBag.Contacts = contacts;
            }

            return View();
        }

        [HttpGet("api/customeragentchat/history/{contactId}")]
        public async Task<IActionResult> GetChatHistory(int contactId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(userIdStr, out int currentUserId) || string.IsNullOrEmpty(role))
                return Unauthorized();

            int customerId = role == "Customer" ? currentUserId : contactId;
            int agentId = role == "Agent" ? currentUserId : contactId;

            var messages = await _context.CustomerAgentMessages
                .Where(m => m.customer_id == customerId && m.agent_id == agentId)
                .OrderBy(m => m.sent_at)
                .Select(m => new {
                    m.id,
                    m.sender_type,
                    m.message_text,
                    m.sent_at
                })
                .ToListAsync();

            return Json(messages);
        }

        [HttpPost("api/customeragentchat/send/{contactId}")]
        public async Task<IActionResult> SendMessage(int contactId, [FromBody] CAChatMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MessageText)) return BadRequest("Message is required.");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(userIdStr, out int currentUserId) || string.IsNullOrEmpty(role))
                return Unauthorized();

            int customerId = role == "Customer" ? currentUserId : contactId;
            int agentId = role == "Agent" ? currentUserId : contactId;

            var msg = new CustomerAgentMessage
            {
                customer_id = customerId,
                agent_id = agentId,
                sender_type = role,
                message_text = dto.MessageText,
                sent_at = DateTime.UtcNow
            };

            _context.CustomerAgentMessages.Add(msg);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, msg.id, msg.sent_at });
        }
    }

    public class CAChatMessageDto
    {
        public string MessageText { get; set; } = "";
    }
}
