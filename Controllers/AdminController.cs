using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Models;
using SmartInsuranceHub.Services;

namespace SmartInsuranceHub.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalCompanies = await _context.Companies.CountAsync();
            ViewBag.TotalAgents = await _context.Agents.CountAsync();
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            ViewBag.TotalPolicies = await _context.Policies.CountAsync();
            ViewBag.PendingDocs = await _context.UserDocuments.Where(d => d.status == "pending").CountAsync();
            
            return View();
        }

        public async Task<IActionResult> Companies()
        {
            var companies = await _context.Companies.ToListAsync();
            return View(companies);
        }

        public async Task<IActionResult> ApproveCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                company.status = "approved";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Companies");
        }

        public async Task<IActionResult> RejectCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                company.status = "rejected";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Companies");
        }

        public async Task<IActionResult> Agents()
        {
            var agents = await _context.Agents.Include(a => a.Company).ToListAsync();
            return View(agents);
        }

        public async Task<IActionResult> ApproveAgent(int id)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent != null)
            {
                agent.approved_status = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Agents");
        }

        public async Task<IActionResult> Customers()
        {
            var customers = await _context.Customers.ToListAsync();
            return View(customers);
        }

        public async Task<IActionResult> InsuranceTypes()
        {
            var types = await _context.InsuranceTypes.OrderBy(t => t.type_id).ToListAsync();
            return View(types);
        }

        [HttpPost]
        public async Task<IActionResult> AddInsuranceType(string type_name, string description, string icon)
        {
            if (!string.IsNullOrWhiteSpace(type_name))
            {
                _context.InsuranceTypes.Add(new InsuranceType
                {
                    type_name = type_name.Trim(),
                    description = description?.Trim(),
                    icon = string.IsNullOrEmpty(icon) ? "bi-shield" : icon,
                    status = "active",
                    created_at = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("InsuranceTypes");
        }

        public async Task<IActionResult> DeleteInsuranceType(int id)
        {
            var type = await _context.InsuranceTypes.FindAsync(id);
            if (type != null)
            {
                _context.InsuranceTypes.Remove(type);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("InsuranceTypes");
        }

        public async Task<IActionResult> Queries()
        {
            var queries = await _context.Queries.Include(q => q.Customer).ToListAsync();
            return View(queries);
        }

        // ========================================
        // Document Verification Panel
        // ========================================
        public async Task<IActionResult> DocumentVerification()
        {
            var documents = await _context.UserDocuments
                .OrderByDescending(d => d.uploaded_at)
                .ToListAsync();

            // Build lookup dictionaries for user names
            var customerIds = documents.Where(d => d.user_type == "Customer").Select(d => d.user_id).Distinct().ToList();
            var agentIds = documents.Where(d => d.user_type == "Agent").Select(d => d.user_id).Distinct().ToList();

            var customerNames = await _context.Customers
                .Where(c => customerIds.Contains(c.customer_id))
                .ToDictionaryAsync(c => c.customer_id, c => c.full_name);
            
            var agentNames = await _context.Agents
                .Where(a => agentIds.Contains(a.agent_id))
                .ToDictionaryAsync(a => a.agent_id, a => a.full_name);

            ViewBag.CustomerNames = customerNames;
            ViewBag.AgentNames = agentNames;

            return View(documents);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveDocument(int id)
        {
            var doc = await _context.UserDocuments.FindAsync(id);
            if (doc != null)
            {
                var adminId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "0");
                doc.status = "approved";
                doc.reviewed_by = adminId;
                doc.reviewed_at = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Check if all docs are now approved for this user
                var docService = HttpContext.RequestServices.GetRequiredService<DocumentService>();
                await docService.UpdateVerificationStatusAsync(doc.user_type, doc.user_id);
            }
            return RedirectToAction("DocumentVerification");
        }

        [HttpPost]
        public async Task<IActionResult> RejectDocument(int id, string? reason)
        {
            var doc = await _context.UserDocuments.FindAsync(id);
            if (doc != null)
            {
                var adminId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "0");
                doc.status = "rejected";
                doc.rejection_reason = reason ?? "Document rejected by admin.";
                doc.reviewed_by = adminId;
                doc.reviewed_at = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("DocumentVerification");
        }

        // ========================================
        // Company Management (Add / Delete / Seed)
        // ========================================

        [HttpPost]
        public async Task<IActionResult> AddCompany(string company_name, string email, string? address, string? license_number, string? c_information)
        {
            if (!string.IsNullOrWhiteSpace(company_name) && !string.IsNullOrWhiteSpace(email))
            {
                var exists = await _context.Companies.AnyAsync(c => c.email == email);
                if (!exists)
                {
                    var slug = company_name.Trim().ToLower().Replace(" ", "").Replace("(", "").Replace(")", "");
                    _context.Companies.Add(new Company
                    {
                        company_name = company_name.Trim(),
                        email = email.Trim().ToLower(),
                        password = BCrypt.Net.BCrypt.HashPassword(slug + "@123"),
                        address = address?.Trim() ?? "",
                        license_number = license_number?.Trim() ?? "",
                        c_information = c_information?.Trim() ?? "",
                        c_agent = "0",
                        status = "approved",
                        created_at = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Companies");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            // Use raw SQL with fresh connection to delete company and dependents
            var connString = _context.Database.GetConnectionString();
            using var conn = new Npgsql.NpgsqlConnection(connString);
            await conn.OpenAsync();
            var sql = @"
                DELETE FROM ""Policies"" WHERE ""CompanyId"" = @id;
                DELETE FROM ""InsurancePlans"" WHERE ""CompanyId"" = @id;
                DELETE FROM ""ChatMessages"" WHERE ""CompanyId"" = @id;
                DELETE FROM ""Agents"" WHERE ""CompanyId"" = @id;
                DELETE FROM ""Companies"" WHERE ""CompanyId"" = @id;
            ";
            using var cmd = new Npgsql.NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
            return RedirectToAction("Companies");
        }

        [HttpPost]
        public async Task<IActionResult> SeedCompanies()
        {
            var connString = _context.Database.GetConnectionString();
            using var conn = new Npgsql.NpgsqlConnection(connString);
            await conn.OpenAsync();

            var sqlDelete = @"
                DELETE FROM ""Payments"";
                DELETE FROM ""Reviews"";
                DELETE FROM ""Policies"";
                DELETE FROM ""InsurancePlans"";
                DELETE FROM ""ChatMessages"";
                DELETE FROM ""Agents"";
                DELETE FROM ""Companies"";
            ";
            using var cmdDelete = new Npgsql.NpgsqlCommand(sqlDelete, conn);
            await cmdDelete.ExecuteNonQueryAsync();

            var companies = new (string Name, string Email, string Address, string AgentCount, string Info, string License, int TypeId)[]
            {
                // Life Insurance (TypeId = 1)
                ("LIC", "lic@gmail.com", "Mumbai, India", "500", "Government life insurance provider", "LIC001", 1),
                ("HDFC Life Insurance", "hdfc@gmail.com", "Mumbai, India", "300", "Private life insurance company", "HDFC001", 1),
                ("ICICI Prudential Life Insurance", "icici@gmail.com", "Mumbai, India", "250", "Joint venture insurance company", "ICICI001", 1),
                ("SBI Life Insurance", "sbi@gmail.com", "India", "400", "Bank-based life insurance", "SBI001", 1),
                ("Max Life Insurance", "max@gmail.com", "India", "200", "Private life insurance provider", "MAX001", 1),
                // Health Insurance (TypeId = 2)
                ("Star Health Insurance", "star@gmail.com", "Chennai, India", "220", "Health insurance specialist", "STAR001", 2),
                ("Niva Bupa Health Insurance", "niva@gmail.com", "Delhi, India", "180", "Global health insurance", "NIVA001", 2),
                ("Care Health Insurance", "care@gmail.com", "India", "160", "Affordable health plans", "CARE001", 2),
                ("Aditya Birla Health Insurance", "birla@gmail.com", "Mumbai, India", "210", "Wellness-based insurance", "BIRLA001", 2),
                ("HDFC ERGO Health", "ergohealth@gmail.com", "India", "190", "Comprehensive health insurance", "ERGOH001", 2),
                // Motor Insurance (TypeId = 3)
                ("Bajaj Allianz General Insurance", "bajaj@gmail.com", "Pune, India", "350", "Motor insurance leader", "BAJAJ001", 3),
                ("ICICI Lombard", "lombard@gmail.com", "Mumbai, India", "300", "General insurance provider", "LOMB001", 3),
                ("TATA AIG General Insurance", "tata@gmail.com", "India", "280", "Trusted motor insurance", "TATA001", 3),
                ("Reliance General Insurance", "reliance@gmail.com", "India", "260", "Affordable policies", "REL001", 3),
                ("HDFC ERGO", "ergo@gmail.com", "India", "240", "Motor and general insurance", "ERGO001", 3),
                // Property Insurance (TypeId = 4)
                ("New India Assurance", "new@gmail.com", "India", "150", "Public sector insurer", "NEW001", 4),
                ("United India Insurance", "united@gmail.com", "India", "140", "Government insurance", "UNIT001", 4),
                ("Oriental Insurance Company", "oriental@gmail.com", "India", "130", "Property insurance provider", "ORI001", 4),
                ("National Insurance Company", "national@gmail.com", "India", "120", "Public insurer", "NAT001", 4),
                // Travel Insurance (TypeId = 5)
                ("Tata AIG Travel Insurance", "tatatravel@gmail.com", "India", "170", "Travel insurance expert", "TATAT001", 5),
                ("ICICI Lombard Travel Insurance", "lombardtravel@gmail.com", "India", "160", "Travel coverage", "LOMBT001", 5),
                ("Reliance Travel Insurance", "reliancetravel@gmail.com", "India", "150", "Travel insurance services", "RELT001", 5),
                ("Bajaj Allianz Travel Insurance", "bajajtravel@gmail.com", "India", "180", "Global travel plans", "BAJT001", 5)
            };

            var sqlInsert = @"
                INSERT INTO ""Companies"" (""CompanyName"", ""Email"", ""Password"", ""Address"", ""CAgent"", ""CInformation"", ""LicenseNumber"", ""Status"", ""InsuranceTypeId"", ""CreatedAt"") 
                VALUES (@name, @email, @pwd, @address, @cagent, @info, @license, 'approved', @typeid, NOW())
            ";

            foreach (var comp in companies)
            {
                using var cmdInsert = new Npgsql.NpgsqlCommand(sqlInsert, conn);
                
                var prefix = comp.Email.Split('@')[0];
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(prefix + "@123");

                cmdInsert.Parameters.AddWithValue("name", comp.Name);
                cmdInsert.Parameters.AddWithValue("email", comp.Email);
                cmdInsert.Parameters.AddWithValue("pwd", passwordHash);
                cmdInsert.Parameters.AddWithValue("address", comp.Address);
                cmdInsert.Parameters.AddWithValue("cagent", comp.AgentCount);
                cmdInsert.Parameters.AddWithValue("info", comp.Info);
                cmdInsert.Parameters.AddWithValue("license", comp.License);
                cmdInsert.Parameters.AddWithValue("typeid", comp.TypeId);
                
                await cmdInsert.ExecuteNonQueryAsync();
            }

            return RedirectToAction("Companies");
        }
    }
}
