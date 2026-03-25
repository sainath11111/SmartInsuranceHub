using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class CustomerAgentMessage
    {
        [Key]
        public int id { get; set; }
        
        public int customer_id { get; set; }
        [ForeignKey("customer_id")]
        public virtual Customer? Customer { get; set; }
        
        public int agent_id { get; set; }
        [ForeignKey("agent_id")]
        public virtual Agent? Agent { get; set; }
        
        [Required, MaxLength(20)]
        public string sender_type { get; set; } = ""; // "Customer" or "Agent"
        
        [Required]
        public string message_text { get; set; } = "";
        
        public DateTime sent_at { get; set; } = DateTime.UtcNow;
    }
}
