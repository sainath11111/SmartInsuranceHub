using System.ComponentModel.DataAnnotations;

namespace SmartInsuranceHub.Models
{
    public class UserDocument
    {
        [Key]
        public int document_id { get; set; }

        [Required, MaxLength(20)]
        public string user_type { get; set; } = ""; // "Customer" or "Agent"

        [Required]
        public int user_id { get; set; }

        [Required, MaxLength(100)]
        public string category { get; set; } = ""; // e.g., "Identity Proof", "Address Proof"

        [Required, MaxLength(150)]
        public string document_name { get; set; } = ""; // e.g., "Aadhaar Card", "PAN Card"

        [Required]
        public string file_url { get; set; } = ""; // R2 object key

        [MaxLength(255)]
        public string file_name { get; set; } = ""; // Original filename

        public long file_size { get; set; } // Size in bytes

        [MaxLength(20)]
        public string status { get; set; } = "pending"; // pending, approved, rejected

        public string? rejection_reason { get; set; }

        public int? reviewed_by { get; set; } // Admin ID

        public DateTime uploaded_at { get; set; } = DateTime.UtcNow;

        public DateTime? reviewed_at { get; set; }
    }
}
