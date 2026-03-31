using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;

namespace SmartInsuranceHub.Services
{
    /// <summary>
    /// Manages document requirements, completion tracking, and verification status.
    /// </summary>
    public class DocumentService
    {
        private readonly ApplicationDbContext _context;

        public DocumentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // Required Document Definitions
        // ============================================================

        public static List<(string Category, List<string> Documents)> GetRequiredDocuments(string userType)
        {
            if (userType == "Customer")
            {
                return new List<(string, List<string>)>
                {
                    ("Identity Proof", new List<string> { "Aadhaar Card", "PAN Card" }),
                    ("Address Proof", new List<string> { "Aadhaar Card", "Electricity Bill" }),
                    ("Age Proof", new List<string> { "Birth Certificate", "Passport" }),
                    ("Income Proof", new List<string> { "Salary Slip", "ITR", "Bank Statement" }),
                    ("Photograph", new List<string> { "Passport Size Photo" }),
                    ("Bank Details", new List<string> { "Cancelled Cheque", "Bank Passbook" }),
                    ("Medical Documents", new List<string> { "Medical Report" }),
                };
            }
            else // Agent
            {
                return new List<(string, List<string>)>
                {
                    ("Identity Proof", new List<string> { "Aadhaar Card", "PAN Card" }),
                    ("Address Proof", new List<string> { "Aadhaar Card", "Electricity Bill" }),
                    ("Professional License", new List<string> { "IRDAI Agent License" }),
                    ("Education", new List<string> { "SSC/HSC Certificate", "Graduation Certificate" }),
                    ("Photograph", new List<string> { "Passport Size Photo" }),
                    ("Bank Details", new List<string> { "Cancelled Cheque", "Bank Passbook" }),
                    ("Company Authorization", new List<string> { "Appointment Letter" }),
                };
            }
        }

        /// <summary>
        /// Returns all required category names (one doc per category is sufficient).
        /// </summary>
        public static List<string> GetRequiredCategories(string userType)
        {
            return GetRequiredDocuments(userType).Select(d => d.Category).ToList();
        }

        /// <summary>
        /// Gets the minimum number of categories that must have at least one approved document.
        /// </summary>
        public static int GetRequiredCategoryCount(string userType)
        {
            return GetRequiredCategories(userType).Count;
        }

        // ============================================================
        // Upload Completion (for "Verify to Request" gate)
        // ============================================================

        /// <summary>
        /// Checks if all required categories have at least one uploaded document (any status).
        /// Used to gate the "Request via Agent" button — customer must upload all 7 before requesting.
        /// </summary>
        public async Task<(int Uploaded, int Total, bool AllUploaded)> GetUploadCompletionAsync(string userType, int userId)
        {
            var requiredCategories = GetRequiredCategories(userType);
            var total = requiredCategories.Count;

            var uploadedCategories = await _context.UserDocuments
                .Where(d => d.user_type == userType && d.user_id == userId)
                .Select(d => d.category)
                .Distinct()
                .CountAsync();

            return (uploadedCategories, total, uploadedCategories >= total);
        }

        // ============================================================
        // Progress & Verification
        // ============================================================

        /// <summary>
        /// Gets documents uploaded by a user, grouped by category.
        /// </summary>
        public async Task<List<UserDocument>> GetUserDocumentsAsync(string userType, int userId)
        {
            return await _context.UserDocuments
                .Where(d => d.user_type == userType && d.user_id == userId)
                .OrderByDescending(d => d.uploaded_at)
                .ToListAsync();
        }

        /// <summary>
        /// Returns (approved category count, total required categories, percentage).
        /// </summary>
        public async Task<(int Completed, int Total, int Percentage)> GetCompletionAsync(string userType, int userId)
        {
            var requiredCategories = GetRequiredCategories(userType);
            var total = requiredCategories.Count;

            var approvedCategories = await _context.UserDocuments
                .Where(d => d.user_type == userType && d.user_id == userId && d.status == "approved")
                .Select(d => d.category)
                .Distinct()
                .CountAsync();

            var percentage = total > 0 ? (int)Math.Round((double)approvedCategories / total * 100) : 0;
            return (approvedCategories, total, percentage);
        }

        /// <summary>
        /// Checks if all required categories have at least one approved document.
        /// If so, updates the user's verification_status to "verified".
        /// </summary>
        public async Task UpdateVerificationStatusAsync(string userType, int userId)
        {
            var requiredCategories = GetRequiredCategories(userType);

            var approvedCategories = await _context.UserDocuments
                .Where(d => d.user_type == userType && d.user_id == userId && d.status == "approved")
                .Select(d => d.category)
                .Distinct()
                .ToListAsync();

            var allApproved = requiredCategories.All(c => approvedCategories.Contains(c));
            var newStatus = allApproved ? "verified" : "unverified";

            if (userType == "Customer")
            {
                var customer = await _context.Customers.FindAsync(userId);
                if (customer != null && customer.verification_status != newStatus)
                {
                    customer.verification_status = newStatus;
                    await _context.SaveChangesAsync();
                }
            }
            else if (userType == "Agent")
            {
                var agent = await _context.Agents.FindAsync(userId);
                if (agent != null && agent.verification_status != newStatus)
                {
                    agent.verification_status = newStatus;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
