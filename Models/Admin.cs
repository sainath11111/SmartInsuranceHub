using System.ComponentModel.DataAnnotations;

namespace SmartInsuranceHub.Models
{
    public class Admin
    {
        [Key]
        public int admin_id { get; set; }
        
        [Required, MaxLength(100)]
        public string full_name { get; set; } = string.Empty;
        
        [Required, EmailAddress, MaxLength(100)]
        public string email { get; set; } = string.Empty;
        
        [Required]
        public string password { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string phone { get; set; } = string.Empty;
        
        public DateTime? last_login { get; set; }
        
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
