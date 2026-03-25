using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class Advertisement
    {
        [Key]
        public int ad_id { get; set; }

        public int company_id { get; set; }
        [ForeignKey("company_id")]
        public virtual Company? Company { get; set; }

        public int plan_id { get; set; }

        [MaxLength(200)]
        public string title { get; set; } = "";

        public string? description { get; set; }

        public string? banner_url { get; set; }

        public decimal amount_paid { get; set; }

        public int duration_days { get; set; }

        public DateTime? start_date { get; set; }

        public DateTime? end_date { get; set; }

        [MaxLength(20)]
        public string status { get; set; } = "pending";

        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
