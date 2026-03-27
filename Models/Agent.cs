using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class Agent
    {
        [Key]
        public int agent_id { get; set; }
        
        public int company_id { get; set; }
        [ForeignKey("company_id")]
        public virtual Company? Company { get; set; }
        
        [Required, MaxLength(100)]
        public string full_name { get; set; } = "";
        
        [Required, EmailAddress, MaxLength(100)]
        public string email { get; set; } = "";
        
        [Required]
        public string password { get; set; } = "";
        
        [MaxLength(20)]
        public string? phone { get; set; }
        
        [MaxLength(50)]
        public string? license_number { get; set; }
        
        public int experience_years { get; set; }
        
        public string? address { get; set; }
        
        [MaxLength(100)]
        public string? city { get; set; }
        
        [MaxLength(20)]
        public string? pincode { get; set; }
        
        public string? profile_photo { get; set; }
        
        public DateTime dob { get; set; }
        
        [MaxLength(20)]
        public string? aadhaar { get; set; }
        
        [MaxLength(20)]
        public string? pan { get; set; }
        
        public bool approved_status { get; set; } = false;
        
        public DateTime created_at { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string verification_status { get; set; } = "unverified";
        
        public virtual ICollection<AgentCity> AgentCities { get; set; } = new List<AgentCity>();
    }
}
