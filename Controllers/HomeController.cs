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
