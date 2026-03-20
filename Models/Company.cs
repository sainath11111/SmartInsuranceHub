using System.ComponentModel.DataAnnotations;

namespace SmartInsuranceHub.Models
{
    public class Company
    {
        [Key]
        public int company_id { get; set; }
        
        [Required, MaxLength(150)]
        public string company_name { get; set; } = "";
        
        [Required, EmailAddress, MaxLength(100)]
        public string email { get; set; } = "";
        
        [Required]
        public string password { get; set; } = "";
        
        public string address { get; set; } = "";
        
        [MaxLength(100)]
        public string c_agent { get; set; } = "";        // agent count
        
        public string c_information { get; set; } = "";
        
        [MaxLength(50)]
        public string license_number { get; set; } = "";
        
        [MaxLength(20)]
        public string status { get; set; } = "pending";
        
        public int? insurance_type_id { get; set; }
        
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
