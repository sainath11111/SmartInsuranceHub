using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class Review
    {
        [Key]
        public int review_id { get; set; }
        
        public int customer_id { get; set; }
        [ForeignKey("customer_id")]
        public virtual Customer? Customer { get; set; }
        
        public int plan_id { get; set; }
        public int company_id { get; set; }
        [ForeignKey("plan_id, company_id")]
        public virtual InsurancePlan? InsurancePlan { get; set; }
        
        [Range(1, 5)]
        public int rating { get; set; }
        
        public string? comment { get; set; }
        
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
