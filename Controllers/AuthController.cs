using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;
using System.Security.Claims;

namespace SmartInsuranceHub.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Companies = _context.Companies.Where(c => c.status == "approved").ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string role, int? company_id)
        {
            List<Claim>? claims = null;

            if (role == "Admin")
            {
                var admin = await _context.Admins.FirstOrDefaultAsync(x => x.email == email);
                if (admin != null && BCrypt.Net.BCrypt.Verify(password, admin.password))
                {
                    admin.last_login = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, admin.admin_id.ToString()),
                        new Claim(ClaimTypes.Name, admin.full_name),
                        new Claim(ClaimTypes.Email, admin.email),
                        new Claim(ClaimTypes.Role, "Admin")
                    };
                }
            }
            else if (role == "Company")
            {
                Company? company = null;
                
                // If the dropdown was used, find by ID
                if (company_id.HasValue && company_id.Value > 0)
                {
                    company = await _context.Companies.FirstOrDefaultAsync(x => x.company_id == company_id.Value);
                }
                // Fallback to email if for some reason the dropdown wasn't used and email was provided
                else if (!string.IsNullOrEmpty(email))
                {
                    company = await _context.Companies.FirstOrDefaultAsync(x => x.email == email);
                }

                if (company != null && BCrypt.Net.BCrypt.Verify(password, company.password))
                {
                    if (company.status != "approved")
                    {
                        ViewBag.Error = "Your company account is pending Admin approval.";
                        ViewBag.Companies = _context.Companies.Where(c => c.status == "approved").ToList();
                        return View();
                    }
                    claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, company.company_id.ToString()),
                        new Claim(ClaimTypes.Name, company.company_name),
                        new Claim(ClaimTypes.Email, company.email),
                        new Claim(ClaimTypes.Role, "Company")
                    };
                }
            }
            else if (role == "Agent")
            {
                var agent = await _context.Agents.FirstOrDefaultAsync(x => x.email == email);
                if (agent != null && BCrypt.Net.BCrypt.Verify(password, agent.password))
                {
                    if (!agent.approved_status)
                    {
                        ViewBag.Error = "Your agent account is pending Admin approval.";
                        return View();
                    }
                    claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, agent.agent_id.ToString()),
                        new Claim(ClaimTypes.Name, agent.full_name),
                        new Claim(ClaimTypes.Email, agent.email),
                        new Claim(ClaimTypes.Role, "Agent")
                    };
                }
            }
            else if (role == "Customer")
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(x => x.email == email);
                if (customer != null && BCrypt.Net.BCrypt.Verify(password, customer.password))
                {
                    claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, customer.customer_id.ToString()),
                        new Claim(ClaimTypes.Name, customer.full_name),
                        new Claim(ClaimTypes.Email, customer.email),
                        new Claim(ClaimTypes.Role, "Customer")
                    };
                }
            }

            if (claims != null)
            {
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                    new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

                return role switch
                {
                    "Admin"    => RedirectToAction("Index", "Admin"),
                    "Company"  => RedirectToAction("Index", "Company"),
                    "Agent"    => RedirectToAction("Index", "Agent"),
                    "Customer" => RedirectToAction("Index", "Customer"),
                    _          => RedirectToAction("Index", "Home")
                };
            }

            ViewBag.Error = "Invalid email or password. Please try again.";
            return View();
        }

        [HttpGet]
        public IActionResult RegisterCustomer() => View(new Customer());

        [HttpPost]
        public async Task<IActionResult> RegisterCustomer(Customer model)
        {
            model.password = BCrypt.Net.BCrypt.HashPassword(model.password);
            model.created_at = DateTime.UtcNow;
            model.status = "active";

            _context.Customers.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult RegisterAgent()
        {
            ViewBag.Companies = _context.Companies.Where(c => c.status == "approved").ToList();
            return View(new Agent());
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAgent(Agent model)
        {
            model.password = BCrypt.Net.BCrypt.HashPassword(model.password);
            model.created_at = DateTime.UtcNow;
            model.approved_status = false;

            _context.Agents.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
