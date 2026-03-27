using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;
using System.Security.Claims;
using SmartInsuranceHub.Services;

namespace SmartInsuranceHub.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly R2StorageService _r2Storage;

        public AuthController(ApplicationDbContext context, R2StorageService r2Storage)
        {
            _context = context;
            _r2Storage = r2Storage;
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
                    
                    if (!string.IsNullOrEmpty(agent.profile_photo))
                    {
                        claims.Add(new Claim("ProfilePhoto", agent.profile_photo));
                    }
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
        public async Task<IActionResult> RegisterCustomer()
        {
            ViewBag.Cities = await _context.Cities.Where(c => c.is_active).OrderBy(c => c.city_name).ToListAsync();
            return View(new Customer());
        }

        private async Task<bool> IsEmailUniqueAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            email = email.ToLower().Trim();
            return !(await _context.Admins.AnyAsync(x => x.email.ToLower() == email) ||
                     await _context.Companies.AnyAsync(x => x.email.ToLower() == email) ||
                     await _context.Agents.AnyAsync(x => x.email.ToLower() == email) ||
                     await _context.Customers.AnyAsync(x => x.email.ToLower() == email));
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCustomer(Customer model, string[] SelectedCities)
        {
            if (SelectedCities != null && SelectedCities.Length > 0)
            {
                model.city = string.Join(", ", SelectedCities);
            }
            // Unique Email Validation
            if (!await IsEmailUniqueAsync(model.email))
            {
                ModelState.AddModelError("email", "This email is already registered.");
            }

            // Age Validation
            var age = DateTime.Today.Year - model.dob.Year;
            if (model.dob.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < 18)
            {
                ModelState.AddModelError("dob", "You must be 18+ to register.");
            }
            model.age = age;

            // Aadhaar Validation
            if (!string.IsNullOrEmpty(model.c_adhar))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.c_adhar, @"^\d{12}$"))
                    ModelState.AddModelError("c_adhar", "Invalid Aadhaar number. Must be exactly 12 digits.");
                else if (await _context.Customers.AnyAsync(c => c.c_adhar == model.c_adhar) || await _context.Agents.AnyAsync(a => a.aadhaar == model.c_adhar))
                    ModelState.AddModelError("c_adhar", "Aadhaar already exists.");
            }

            // PAN Validation
            if (!string.IsNullOrEmpty(model.c_pancard))
            {
                model.c_pancard = model.c_pancard.ToUpper();
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.c_pancard, @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$"))
                    ModelState.AddModelError("c_pancard", "Invalid PAN number.");
                else if (await _context.Customers.AnyAsync(c => c.c_pancard == model.c_pancard) || await _context.Agents.AnyAsync(a => a.pan == model.c_pancard))
                    ModelState.AddModelError("c_pancard", "PAN number already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Cities = await _context.Cities.Where(c => c.is_active).OrderBy(c => c.city_name).ToListAsync();
                return View(model);
            }

            model.password = BCrypt.Net.BCrypt.HashPassword(model.password);
            model.created_at = DateTime.UtcNow;
            model.dob = DateTime.SpecifyKind(model.dob, DateTimeKind.Utc);
            model.status = "active";

            _context.Customers.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registration successful! You can now log in.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> RegisterAgent()
        {
            ViewBag.Companies = _context.Companies.Where(c => c.status == "approved").ToList();
            ViewBag.Cities = await _context.Cities.Where(c => c.is_active).OrderBy(c => c.city_name).ToListAsync();
            return View(new Agent());
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAgent(Agent model, IFormFile? profilePhotoFile, string[] SelectedCities)
        {
            if (SelectedCities != null && SelectedCities.Length > 0)
            {
                model.city = string.Join(", ", SelectedCities);
            }
            // Profile Photo Validation (Before DB Save)
            if (profilePhotoFile != null)
            {
                var valResult = _r2Storage.ValidateFile(profilePhotoFile);
                if (!valResult.IsValid)
                {
                    ModelState.AddModelError("profile_photo", valResult.Error);
                }
            }

            // Unique Email Validation
            if (!await IsEmailUniqueAsync(model.email))
            {
                ModelState.AddModelError("email", "This email is already registered.");
            }

            // Age Validation
            var age = DateTime.Today.Year - model.dob.Year;
            if (model.dob.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < 18)
            {
                ModelState.AddModelError("dob", "You must be 18+ to register.");
            }

            // Aadhaar Validation
            if (!string.IsNullOrEmpty(model.aadhaar))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.aadhaar, @"^\d{12}$"))
                    ModelState.AddModelError("aadhaar", "Invalid Aadhaar number. Must be exactly 12 digits.");
                else if (await _context.Agents.AnyAsync(a => a.aadhaar == model.aadhaar) || await _context.Customers.AnyAsync(c => c.c_adhar == model.aadhaar))
                    ModelState.AddModelError("aadhaar", "Aadhaar already exists.");
            }

            // PAN Validation
            if (!string.IsNullOrEmpty(model.pan))
            {
                model.pan = model.pan.ToUpper();
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.pan, @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$"))
                    ModelState.AddModelError("pan", "Invalid PAN number.");
                else if (await _context.Agents.AnyAsync(a => a.pan == model.pan) || await _context.Customers.AnyAsync(c => c.c_pancard == model.pan))
                    ModelState.AddModelError("pan", "PAN number already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Companies = _context.Companies.Where(c => c.status == "approved").ToList();
                ViewBag.Cities = await _context.Cities.Where(c => c.is_active).OrderBy(c => c.city_name).ToListAsync();
                return View(model);
            }

            model.password = BCrypt.Net.BCrypt.HashPassword(model.password);
            model.created_at = DateTime.UtcNow;
            model.dob = DateTime.SpecifyKind(model.dob, DateTimeKind.Utc);
            model.approved_status = false;

            _context.Agents.Add(model);
            await _context.SaveChangesAsync();

            // Profile Photo Upload (After DB Save to get agent_id)
            if (profilePhotoFile != null)
            {
                var uploadResult = await _r2Storage.UploadFileAsync(profilePhotoFile, "Agent", model.agent_id, "profile_photo");
                if (uploadResult.FileUrl != null)
                {
                    model.profile_photo = uploadResult.FileUrl;
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = "Registration successful! Awaiting Admin approval.";
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
