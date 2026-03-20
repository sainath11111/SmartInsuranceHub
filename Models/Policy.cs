using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class Policy
    {
        [Key]
        public int policy_id { get; set; }
        
        [MaxLength(50)]
        public string policy_no { get; set; } = "";
        
        public int customer_id { get; set; }
        [ForeignKey("customer_id")]
        public virtual Customer? Customer { get; set; }
        
        public int? agent_id { get; set; }
        [ForeignKey("agent_id")]
        public virtual Agent? Agent { get; set; }
        
        public int plan_id { get; set; }
        public int company_id { get; set; }
        [ForeignKey("plan_id, company_id")]
        public virtual InsurancePlan? InsurancePlan { get; set; }
        
        public DateTime start_date { get; set; }
        
        public DateTime end_date { get; set; }
        
        [MaxLength(20)]
        public string policy_status { get; set; } = "active";
        
        public decimal premium_amount { get; set; }
        
        [MaxLength(100)]
        public string? created_by { get; set; }
    }
}
