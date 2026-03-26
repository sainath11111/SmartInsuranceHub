using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;

namespace SmartInsuranceHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Terms() => View();
        
        public IActionResult Privacy() => View();

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel();
            
            // Stats
            vm.TotalCompanies = await _context.Companies.Where(c => c.status == "approved").CountAsync();
            vm.TotalCustomers = await _context.Customers.CountAsync();
            vm.YearsExperience = 15; // Mocked stat as requested by generic "Years of experience"
            
            // Insurance Types
            vm.InsuranceTypes = await _context.InsuranceTypes.Where(t => t.status == "active").ToListAsync();
            
            // Companies logic (fetch top 6 for the grid)
            var companiesList = await _context.Companies
                .Where(c => c.status == "approved")
                .Take(6)
                .Select(c => new CompanyViewModel {
                    CompanyId = c.company_id,
                    Name = c.company_name,
                    Description = !string.IsNullOrEmpty(c.c_information) ? c.c_information : "Trusted insurance partner committed to securing your future.",
                    TotalPolicies = _context.InsurancePlans.Count(p => p.company_id == c.company_id)
                })
                .ToListAsync();
                
            vm.Companies = companiesList;

            // Active Advertisements (approved, within date range, ordered by amount for priority)
            var now = DateTime.UtcNow;
            vm.Advertisements = await _context.Advertisements
                .Include(a => a.Company)
                .Where(a => a.status == "approved" && a.start_date <= now && a.end_date >= now)
                .OrderByDescending(a => a.amount_paid)
                .Take(10)
                .ToListAsync();

            // Attach plan names to ViewBag for ads
            var adPlanIds = vm.Advertisements.Select(a => new { a.plan_id, a.company_id }).Distinct().ToList();
            var adPlanNames = new Dictionary<string, string>();
            foreach (var key in adPlanIds)
            {
                var plan = await _context.InsurancePlans
                    .FirstOrDefaultAsync(p => p.plan_id == key.plan_id && p.company_id == key.company_id);
                if (plan != null)
                    adPlanNames[$"{key.plan_id}_{key.company_id}"] = plan.plan_name;
            }
            ViewBag.AdPlanNames = adPlanNames;

            // Featured Plans for the homepage carousel (highest value scores)
            ViewBag.FeaturedPlans = await _context.InsurancePlans
                .Include(p => p.Company)
                .Where(p => p.status == "active" && p.premium_amount > 0)
                .OrderByDescending(p => p.coverage_amount / p.premium_amount)
                .Take(6)
                .ToListAsync();

            return View(vm);
        }

        [Route("about")]
        public async Task<IActionResult> About()
        {
            var vm = new HomeViewModel();
            vm.TotalCompanies = await _context.Companies.Where(c => c.status == "approved").CountAsync();
            vm.TotalCustomers = await _context.Customers.CountAsync();
            vm.YearsExperience = 15;
            return View(vm);
        }

        [Route("insurance")]
        public async Task<IActionResult> Insurance()
        {
            var types = await _context.InsuranceTypes.Where(t => t.status == "active").ToListAsync();
            return View(types);
        }

        [Route("companies")]
        public async Task<IActionResult> Companies()
        {
            var companiesList = await _context.Companies
                .Where(c => c.status == "approved")
                .Select(c => new CompanyViewModel {
                    CompanyId = c.company_id,
                    Name = c.company_name,
                    Description = !string.IsNullOrEmpty(c.c_information) ? c.c_information : "Trusted insurance partner committed to securing your future.",
                    TotalPolicies = _context.InsurancePlans.Count(p => p.company_id == c.company_id)
                })
                .ToListAsync();
            return View(companiesList);
        }
    }
}
