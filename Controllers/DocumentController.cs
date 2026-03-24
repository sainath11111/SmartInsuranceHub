using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;
using SmartInsuranceHub.Services;

namespace SmartInsuranceHub.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly R2StorageService _r2;
        private readonly DocumentService _docService;

        public DocumentController(ApplicationDbContext context, R2StorageService r2, DocumentService docService)
        {
            _context = context;
            _r2 = r2;
            _docService = docService;
        }

        private (string UserType, int UserId) GetCurrentUser()
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var userType = role == "Agent" ? "Agent" : "Customer";
            return (userType, id);
        }

        // ========================================
        // Profile Page with Documents Tab
        // ========================================
        public async Task<IActionResult> Profile()
        {
            var (userType, userId) = GetCurrentUser();

            if (userType == "Customer")
            {
                var customer = await _context.Customers.FindAsync(userId);
                if (customer == null) return RedirectToAction("Login", "Auth");
                ViewBag.User = customer;
                ViewBag.UserName = customer.full_name;
                ViewBag.UserEmail = customer.email;
                ViewBag.UserPhone = customer.phone;
                ViewBag.VerificationStatus = customer.verification_status;
            }
            else
            {
                var agent = await _context.Agents.FindAsync(userId);
                if (agent == null) return RedirectToAction("Login", "Auth");
                ViewBag.User = agent;
                ViewBag.UserName = agent.full_name;
                ViewBag.UserEmail = agent.email;
                ViewBag.UserPhone = agent.phone;
                ViewBag.VerificationStatus = agent.verification_status;
            }

            ViewBag.UserType = userType;
            ViewBag.UserId = userId;

            var documents = await _docService.GetUserDocumentsAsync(userType, userId);
            var (completed, total, percentage) = await _docService.GetCompletionAsync(userType, userId);
            var requiredDocs = DocumentService.GetRequiredDocuments(userType);

            ViewBag.Documents = documents;
            ViewBag.Completed = completed;
            ViewBag.Total = total;
            ViewBag.Percentage = percentage;
            ViewBag.RequiredDocs = requiredDocs;

            return View();
        }

        // ========================================
        // Upload Document (with deduplication)
        // ========================================
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, string category, string documentName)
        {
            var (userType, userId) = GetCurrentUser();

            // Validate file type & size
            var (isValid, error) = _r2.ValidateFile(file);
            if (!isValid)
            {
                TempData["Error"] = error;
                return RedirectToAction("Profile");
            }

            // --- DEDUPLICATION: Remove any existing pending/rejected doc for same slot ---
            var existingDoc = await _context.UserDocuments
                .FirstOrDefaultAsync(d =>
                    d.user_type == userType &&
                    d.user_id == userId &&
                    d.category == category &&
                    d.document_name == documentName &&
                    d.status != "approved"); // never touch approved docs

            if (existingDoc != null)
            {
                // Delete old file from R2 storage before replacing
                await _r2.DeleteFileAsync(existingDoc.file_url);
                _context.UserDocuments.Remove(existingDoc);
                await _context.SaveChangesAsync();
            }

            // Upload new file to R2 (or local fallback)
            var fileUrl = await _r2.UploadFileAsync(file, userType, userId, category);
            if (fileUrl == null)
            {
                TempData["Error"] = "Failed to upload file. Please try again.";
                return RedirectToAction("Profile");
            }

            // Save metadata
            var doc = new UserDocument
            {
                user_type = userType,
                user_id = userId,
                category = category,
                document_name = documentName,
                file_url = fileUrl,
                file_name = file.FileName,
                file_size = file.Length,
                status = "pending",
                uploaded_at = DateTime.UtcNow
            };
            _context.UserDocuments.Add(doc);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{documentName} uploaded successfully and is pending review.";
            return RedirectToAction("Profile");
        }

        // ========================================
        // Delete / Re-upload a Document
        // ========================================
        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var (userType, userId) = GetCurrentUser();
            var doc = await _context.UserDocuments.FindAsync(id);

            if (doc == null || doc.user_type != userType || doc.user_id != userId)
            {
                TempData["Error"] = "Document not found.";
                return RedirectToAction("Profile");
            }

            if (doc.status == "approved")
            {
                TempData["Error"] = "Cannot delete an approved document.";
                return RedirectToAction("Profile");
            }

            await _r2.DeleteFileAsync(doc.file_url);
            _context.UserDocuments.Remove(doc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Document removed. You can now re-upload.";
            return RedirectToAction("Profile");
        }

        // ========================================
        // Preview Document (Presigned URL)
        // ========================================
        public async Task<IActionResult> Preview(int id)
        {
            var doc = await _context.UserDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            var url = await _r2.GetPresignedUrlAsync(doc.file_url);
            if (url == null) return NotFound();

            return Redirect(url);
        }
    }
}
