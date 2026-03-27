using System.ComponentModel.DataAnnotations;

namespace SmartInsuranceHub.Models
{
    public class City
    {
        [Key]
        public int city_id { get; set; }

        [Required, MaxLength(100)]
        public string city_name { get; set; } = "";

        [MaxLength(100)]
        public string? state { get; set; }

        public bool is_active { get; set; } = true;

        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
