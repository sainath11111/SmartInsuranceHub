using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class InsurancePlan
    {
        [Key]
        [Column(Order = 1)]
        public int plan_id { get; set; }
        
        [Key]
        [Column(Order = 2)]
        public int company_id { get; set; }
        
        [ForeignKey("company_id")]
        public virtual Company? Company { get; set; }
        
        public int type_id { get; set; }
        
        public int? agent_id { get; set; }
        [ForeignKey("agent_id")]
        public virtual Agent? Agent { get; set; }
        
        [MaxLength(150)]
        public string plan_name { get; set; } = "";
        
        public decimal premium_amount { get; set; }
        
        public decimal coverage_amount { get; set; }
        
        public int duration_months { get; set; }
        
        public string? description { get; set; }
        
        public DateTime created_date { get; set; } = DateTime.UtcNow;
        
        [MaxLength(20)]
        public string status { get; set; } = "active";
    }
}
