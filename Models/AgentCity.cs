using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class AgentCity
    {
        [Key]
        public int id { get; set; }
        
        public int agent_id { get; set; }
        [ForeignKey("agent_id")]
        public virtual Agent? Agent { get; set; }
        
        [Required, MaxLength(100)]
        public string city_name { get; set; } = "";
    }
}
