using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class ChatMessage
    {
        [Key]
        public int id { get; set; }
        
        public int company_id { get; set; }
        [ForeignKey("company_id")]
        public virtual Company? Company { get; set; }
        
        public int agent_id { get; set; }
        [ForeignKey("agent_id")]
        public virtual Agent? Agent { get; set; }
        
        [Required, MaxLength(20)]
        public string sender_type { get; set; } = ""; // "Company" or "Agent"
        
        [Required]
        public string message_text { get; set; } = "";
        
        public DateTime sent_at { get; set; } = DateTime.UtcNow;
    }
}
