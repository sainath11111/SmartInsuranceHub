using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInsuranceHub.Models
{
    public class AdPayment
    {
        [Key]
        public int ad_payment_id { get; set; }

        public int company_id { get; set; }
        [ForeignKey("company_id")]
        public virtual Company? Company { get; set; }

        public int advertisement_id { get; set; }
        [ForeignKey("advertisement_id")]
        public virtual Advertisement? Advertisement { get; set; }

        public decimal amount { get; set; }

        [MaxLength(20)]
        public string payment_status { get; set; } = "completed";

        public DateTime payment_date { get; set; } = DateTime.UtcNow;
    }
}
