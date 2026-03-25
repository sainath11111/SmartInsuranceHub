using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;
using SmartInsuranceHub.Services;

namespace SmartInsuranceHub.Controllers
{
    [Authorize(Policy = "CompanyOnly")]
    public class AdvertisementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly R2StorageService _r2Storage;
        private const decimal RATE_PER_DAY = 100m;

        public AdvertisementController(ApplicationDbContext context, R2StorageService r2Storage)
        {
            _context = context;
            _r2Storage = r2Storage;
        }

        private int GetCompanyId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        // ========================================
        // My Ads - List all ads by this company
        // ========================================
        public async Task<IActionResult> MyAds()
        {
            var cid = GetCompanyId();
            var ads = await _context.Advertisements
                .Where(a => a.company_id == cid)
                .OrderByDescending(a => a.created_at)
                .ToListAsync();

            // Get plan names for display
            var planIds = ads.Select(a => a.plan_id).Distinct().ToList();
            var plans = await _context.InsurancePlans
                .Where(p => p.company_id == cid && planIds.Contains(p.plan_id))
                .ToDictionaryAsync(p => p.plan_id, p => p.plan_name);

            ViewBag.PlanNames = plans;
            return View(ads);
        }

        // ========================================
        // Create Ad (GET)
        // ========================================
        public async Task<IActionResult> CreateAd()
        {
            var cid = GetCompanyId();
            var plans = await _context.InsurancePlans
                .Where(p => p.company_id == cid && p.status == "active")
                .ToListAsync();

            ViewBag.Plans = plans;
            ViewBag.RatePerDay = RATE_PER_DAY;
            return View();
        }

        // ========================================
        // Create Ad (POST)
        // ========================================
        [HttpPost]
        public async Task<IActionResult> CreateAd(string title, string? description, IFormFile? bannerImage, int plan_id, int duration_days)
        {
            var cid = GetCompanyId();

            if (bannerImage != null)
            {
                var valResult = _r2Storage.ValidateFile(bannerImage);
                if (!valResult.IsValid)
                {
                    TempData["Error"] = valResult.Error;
                    return Redirect("/Advertisement/CreateAd");
                }
            }

            if (string.IsNullOrWhiteSpace(title) || duration_days < 1)
            {
                TempData["Error"] = "Please fill all required fields. Duration must be at least 1 day.";
                return Redirect("/Advertisement/CreateAd");
            }

            if (duration_days > 365)
            {
                TempData["Error"] = "Maximum duration is 365 days.";
                return Redirect("/Advertisement/CreateAd");
            }

            // Verify the plan belongs to this company
            var plan = await _context.InsurancePlans.FirstOrDefaultAsync(p => p.plan_id == plan_id && p.company_id == cid);
            if (plan == null)
            {
                TempData["Error"] = "Invalid plan selected.";
                return Redirect("/Advertisement/CreateAd");
            }

            var totalAmount = RATE_PER_DAY * duration_days;

            // Create advertisement
            var ad = new Advertisement
            {
                company_id = cid,
                plan_id = plan_id,
                title = title.Trim(),
                description = description?.Trim(),
                amount_paid = totalAmount,
                duration_days = duration_days,
                status = "pending",
                created_at = DateTime.UtcNow
            };

            _context.Advertisements.Add(ad);
            await _context.SaveChangesAsync();
            
            // Upload Banner Photo (After DB Save to get ad_id)
            if (bannerImage != null)
            {
                var uploadResult = await _r2Storage.UploadFileAsync(bannerImage, "Advertisement", ad.ad_id, "banner_url");
                if (uploadResult.FileUrl != null)
                {
                    ad.banner_url = uploadResult.FileUrl;
                    await _context.SaveChangesAsync();
                }
            }

            // Create payment record
            var payment = new AdPayment
            {
                company_id = cid,
                advertisement_id = ad.ad_id,
                amount = totalAmount,
                payment_status = "completed",
                payment_date = DateTime.UtcNow
            };

            _context.AdPayments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Advertisement created! ₹{totalAmount:N0} paid. Awaiting admin approval.";
            return Redirect("/Advertisement/MyAds");
        }
    }
}
