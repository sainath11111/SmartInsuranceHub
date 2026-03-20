using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;

namespace SmartInsuranceHub.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("myid")]
        public IActionResult GetMyId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole != "Agent" || string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int agentId = int.Parse(userIdStr);
            return Ok(new { agentId });
        }

        [HttpGet("{agentId}")]
        public async Task<IActionResult> GetMessages(int agentId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);
            
            int companyId = 0;
            
            if (userRole == "Company")
            {
                companyId = userId;
            }
            else if (userRole == "Agent")
            {
                if (userId != agentId) return Unauthorized();
                
                var agent = await _context.Agents.FindAsync(agentId);
                if (agent == null) return NotFound();
                companyId = agent.company_id;
            }
            else
            {
                return Forbid();
            }

            var messages = await _context.ChatMessages
                .Where(m => m.company_id == companyId && m.agent_id == agentId)
                .OrderBy(m => m.sent_at)
                .Select(m => new {
                    m.id,
                    m.sender_type,
                    m.message_text,
                    m.sent_at
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("{agentId}")]
        public async Task<IActionResult> SendMessage(int agentId, [FromBody] ChatMessageDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(dto.MessageText)) return BadRequest();
            int userId = int.Parse(userIdStr);
            
            int companyId = 0;
            string senderType = "";
            
            if (userRole == "Company")
            {
                companyId = userId;
                senderType = "Company";
                
                var agentExists = await _context.Agents.AnyAsync(a => a.agent_id == agentId && a.company_id == companyId);
                if (!agentExists) return BadRequest("Agent not found or doesn't belong to your company.");
            }
            else if (userRole == "Agent")
            {
                if (userId != agentId) return Unauthorized();
                senderType = "Agent";
                
                var agent = await _context.Agents.FindAsync(agentId);
                if (agent == null) return NotFound();
                companyId = agent.company_id;
            }
            else
            {
                return Forbid();
            }

            var message = new ChatMessage
            {
                company_id = companyId,
                agent_id = agentId,
                sender_type = senderType,
                message_text = dto.MessageText,
                sent_at = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new {
                message.id,
                message.sender_type,
                message.message_text,
                message.sent_at
            });
        }
    }

    public class ChatMessageDto
    {
        public string MessageText { get; set; } = "";
    }
}
