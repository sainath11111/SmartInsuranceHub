using System.ComponentModel.DataAnnotations;

namespace SmartInsuranceHub.Models
{
    /// <summary>
    /// Records every admin action (approve/reject) for audit trail.
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int audit_log_id { get; set; }

        [Required, MaxLength(50)]
        public string action_type { get; set; } = ""; // e.g. "ApproveDocument", "RejectDocument"

        [Required, MaxLength(50)]
        public string entity_type { get; set; } = ""; // e.g. "UserDocument"

        public int entity_id { get; set; } // document_id

        public int performed_by { get; set; } // Admin ID

        [MaxLength(100)]
        public string? performed_by_name { get; set; } // Admin full_name

        public string? details { get; set; } // JSON or human-readable description

        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
