using System.ComponentModel.DataAnnotations;

namespace SmartInsuranceHub.Models
{
    public class Customer
    {
        [Key]
        public int customer_id { get; set; }
        
        public int? policy_id { get; set; }
        
        [Required, MaxLength(100)]
        public string full_name { get; set; } = "";
        
        [Required, EmailAddress, MaxLength(100)]
        public string email { get; set; } = "";
        
        [Required]
        public string password { get; set; } = "";
        
        [MaxLength(20)]
        public string? phone { get; set; }
        
        public string? address { get; set; }
        
        [MaxLength(100)]
        public string? city { get; set; }
        
        public int age { get; set; }
        
        public DateTime dob { get; set; }
        
        [MaxLength(20)]
        public string? c_pancard { get; set; }
        
        [MaxLength(20)]
        public string? c_adhar { get; set; }
        
        public string? family_info { get; set; }
        
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        
        [MaxLength(20)]
        public string status { get; set; } = "active";

        [MaxLength(20)]
        public string verification_status { get; set; } = "unverified";
    }
}
