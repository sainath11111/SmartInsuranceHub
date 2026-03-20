using System.ComponentModel.DataAnnotations;

namespace SmartInsuranceHub.Models
{
    public class InsuranceType
    {
        [Key]
        public int type_id { get; set; }

        [Required, MaxLength(150)]
        public string type_name { get; set; } = "";

        public string? description { get; set; }

        [MaxLength(100)]
        public string? icon { get; set; } = "bi-shield";

        [MaxLength(20)]
        public string status { get; set; } = "active";

        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
