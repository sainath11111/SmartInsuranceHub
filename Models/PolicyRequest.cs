using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class PolicyRequest
    {
        [Key]
        public int request_id { get; set; }

        public int customer_id { get; set; }
        [ForeignKey("customer_id")]
        public virtual Customer? Customer { get; set; }

        public int agent_id { get; set; }
        [ForeignKey("agent_id")]
        public virtual Agent? Agent { get; set; }

        public int plan_id { get; set; }
        public int company_id { get; set; }
        [ForeignKey("plan_id, company_id")]
        public virtual InsurancePlan? InsurancePlan { get; set; }

        [MaxLength(20)]
        public string status { get; set; } = "pending";

        public string? rejection_reason { get; set; }

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public DateTime? reviewed_at { get; set; }
    }
}
