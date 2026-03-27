using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class Payment
    {
        [Key]
        public int payment_id { get; set; }
        
        public int policy_id { get; set; }
        [ForeignKey("policy_id")]
        public virtual Policy? Policy { get; set; }
        
        public int customer_id { get; set; }
        [ForeignKey("customer_id")]
        public virtual Customer? Customer { get; set; }
        
        public decimal amount { get; set; }
        
        public DateTime payment_date { get; set; } = DateTime.UtcNow;
        
        [MaxLength(50)]
        public string? method { get; set; }
        
        [MaxLength(20)]
        public string payment_status { get; set; } = "completed";
        
        [MaxLength(255)]
        public string? rejection_reason { get; set; }
        
        public int? received_by_agent { get; set; }
        [ForeignKey("received_by_agent")]
        public virtual Agent? Agent { get; set; }
    }
}
