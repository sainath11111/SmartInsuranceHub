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
            ViewBag.Companies = _context.Companies.Where(c => c.status == "approved").ToList();
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

        // ========================================
        // Password Reset Token Storage (in-memory)
        // ========================================
        private static readonly Dictionary<string, (string Role, string Email, DateTime Expiry)> _resetTokens = new();

        // ========================================
        // Forgot Password (GET)
        // ========================================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // ========================================
        // Forgot Password (POST)
        // ========================================
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email, string companyName, string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "Please select your account type.";
                return View();
            }

            string? foundEmail = null;

            if (role == "Customer")
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    ViewBag.Error = "Please enter your email address.";
                    return View();
                }
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.email == email);
                if (customer != null) foundEmail = customer.email;
            }
            else if (role == "Agent")
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    ViewBag.Error = "Please enter your email address.";
                    return View();
                }
                var agent = await _context.Agents.FirstOrDefaultAsync(a => a.email == email);
                if (agent != null) foundEmail = agent.email;
            }
            else if (role == "Company")
            {
                if (string.IsNullOrWhiteSpace(companyName))
                {
                    ViewBag.Error = "Please enter your company name.";
                    return View();
                }
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.company_name == companyName);
                if (company != null) foundEmail = company.email;
            }

            if (foundEmail == null)
            {
                ViewBag.Error = role == "Company"
                    ? "No company found with that name. Please check and try again."
                    : "No account found with that email address. Please check and try again.";
                ViewBag.SelectedRole = role;
                ViewBag.EnteredEmail = email;
                ViewBag.EnteredCompanyName = companyName;
                return View();
            }

            // Generate a secure reset token
            var token = Guid.NewGuid().ToString("N");
            _resetTokens[token] = (role, foundEmail, DateTime.UtcNow.AddMinutes(15));

            // Clean up expired tokens
            var expired = _resetTokens.Where(t => t.Value.Expiry < DateTime.UtcNow).Select(t => t.Key).ToList();
            foreach (var key in expired) _resetTokens.Remove(key);

            return RedirectToAction("ResetPassword", new { token });
        }

        // ========================================
        // Reset Password (GET)
        // ========================================
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token) || !_resetTokens.ContainsKey(token))
            {
                TempData["Error"] = "Invalid or expired password reset link. Please try again.";
                return RedirectToAction("ForgotPassword");
            }

            var (role, email, expiry) = _resetTokens[token];
            if (DateTime.UtcNow > expiry)
            {
                _resetTokens.Remove(token);
                TempData["Error"] = "Password reset link has expired. Please try again.";
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Token = token;
            ViewBag.Email = email;
            ViewBag.Role = role;
            return View();
        }

        // ========================================
        // Reset Password (POST)
        // ========================================
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(token) || !_resetTokens.ContainsKey(token))
            {
                TempData["Error"] = "Invalid or expired password reset link. Please try again.";
                return RedirectToAction("ForgotPassword");
            }

            var (role, email, expiry) = _resetTokens[token];
            if (DateTime.UtcNow > expiry)
            {
                _resetTokens.Remove(token);
                TempData["Error"] = "Password reset link has expired. Please try again.";
                return RedirectToAction("ForgotPassword");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters.";
                ViewBag.Token = token;
                ViewBag.Email = email;
                ViewBag.Role = role;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                ViewBag.Token = token;
                ViewBag.Email = email;
                ViewBag.Role = role;
                return View();
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            if (role == "Customer")
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.email == email);
                if (customer != null) customer.password = hashedPassword;
            }
            else if (role == "Agent")
            {
                var agent = await _context.Agents.FirstOrDefaultAsync(a => a.email == email);
                if (agent != null) agent.password = hashedPassword;
            }
            else if (role == "Company")
            {
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.email == email);
                if (company != null) company.password = hashedPassword;
            }

            await _context.SaveChangesAsync();
            _resetTokens.Remove(token);

            TempData["Success"] = "Password reset successfully! You can now log in with your new password.";
            return RedirectToAction("Login");
        }
    }
}
