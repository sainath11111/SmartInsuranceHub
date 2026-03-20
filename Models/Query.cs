using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class Query
    {
        [Key]
        public int query_id { get; set; }
        
        public int? customer_id { get; set; }
        [ForeignKey("customer_id")]
        public virtual Customer? Customer { get; set; }
        
        [MaxLength(100)]
        public string name { get; set; } = "";
        
        [EmailAddress, MaxLength(100)]
        public string email { get; set; } = "";
        
        [MaxLength(20)]
        public string? phone { get; set; }
        
        [MaxLength(200)]
        public string? subject { get; set; }
        
        public string? message { get; set; }
        
        public DateTime send_date { get; set; } = DateTime.UtcNow;
    }
}
