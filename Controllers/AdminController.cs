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
        private readonly DocumentService _docService;

        public AdminController(ApplicationDbContext context, DocumentService docService)
        {
            _context = context;
            _docService = docService;
        }

        // ========================================
        // Dashboard
        // ========================================
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalCompanies = await _context.Companies.CountAsync();
            ViewBag.TotalAgents = await _context.Agents.CountAsync();
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            ViewBag.TotalPolicies = await _context.Policies.CountAsync();
            ViewBag.PendingDocs = await _context.UserDocuments.Where(d => d.status == "pending").CountAsync();
            ViewBag.VerifiedCustomers = await _context.Customers.Where(c => c.verification_status == "verified").CountAsync();
            ViewBag.VerifiedAgents = await _context.Agents.Where(a => a.verification_status == "verified").CountAsync();
            ViewBag.RejectedDocs = await _context.UserDocuments.Where(d => d.status == "rejected").CountAsync();
            return View();
        }

        // ========================================
        // Company, Agent, Customer Management
        // ========================================
        public async Task<IActionResult> Companies()
        {
            var companies = await _context.Companies.ToListAsync();
            return View(companies);
        }

        public async Task<IActionResult> ApproveCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null) { company.status = "approved"; await _context.SaveChangesAsync(); }
            return RedirectToAction("Companies");
        }

        public async Task<IActionResult> RejectCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null) { company.status = "rejected"; await _context.SaveChangesAsync(); }
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
            if (agent != null) { agent.approved_status = true; await _context.SaveChangesAsync(); }
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
            if (type != null) { _context.InsuranceTypes.Remove(type); await _context.SaveChangesAsync(); }
            return RedirectToAction("InsuranceTypes");
        }

        public async Task<IActionResult> Queries()
        {
            var queries = await _context.Queries.Include(q => q.Customer).OrderByDescending(q => q.send_date).ToListAsync();
            return View(queries);
        }

        [HttpPost]
        public async Task<IActionResult> ReplyQuery(int id, string reply)
        {
            var query = await _context.Queries.FindAsync(id);
            if (query != null && !string.IsNullOrWhiteSpace(reply))
            {
                query.reply = reply;
                query.status = "Resolved";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Reply sent successfully.";
            }
            return RedirectToAction("Queries");
        }

        // ========================================
        // Document Verification Panel (with filter)
        // ========================================
        public async Task<IActionResult> DocumentVerification(string? filter, string? search)
        {
            var query = _context.UserDocuments.AsQueryable();

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(filter) && filter != "all")
                query = query.Where(d => d.status == filter);

            var documents = await query.OrderByDescending(d => d.uploaded_at).ToListAsync();

            // Apply name search (post-query since names are in separate tables)
            var customerIds = documents.Where(d => d.user_type == "Customer").Select(d => d.user_id).Distinct().ToList();
            var agentIds = documents.Where(d => d.user_type == "Agent").Select(d => d.user_id).Distinct().ToList();

            var customerNames = await _context.Customers
                .Where(c => customerIds.Contains(c.customer_id))
                .ToDictionaryAsync(c => c.customer_id, c => c.full_name);

            var agentNames = await _context.Agents
                .Where(a => agentIds.Contains(a.agent_id))
                .ToDictionaryAsync(a => a.agent_id, a => a.full_name);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var sl = search.ToLower();
                documents = documents.Where(d =>
                {
                    var name = d.user_type == "Customer"
                        ? (customerNames.TryGetValue(d.user_id, out var cn) ? cn : "")
                        : (agentNames.TryGetValue(d.user_id, out var an) ? an : "");
                    return name.ToLower().Contains(sl);
                }).ToList();
            }

            ViewBag.CustomerNames = customerNames;
            ViewBag.AgentNames = agentNames;
            ViewBag.Filter = filter ?? "all";
            ViewBag.Search = search ?? "";
            ViewBag.AllCount = await _context.UserDocuments.CountAsync();
            ViewBag.PendingCount = await _context.UserDocuments.CountAsync(d => d.status == "pending");
            ViewBag.ApprovedCount = await _context.UserDocuments.CountAsync(d => d.status == "approved");
            ViewBag.RejectedCount = await _context.UserDocuments.CountAsync(d => d.status == "rejected");

            return View(documents);
        }

        // ========================================
        // Per-User Document View
        // ========================================
        public async Task<IActionResult> UserDocuments(int userId, string userType)
        {
            string userName;
            string verificationStatus;

            if (userType == "Customer")
            {
                var customer = await _context.Customers.FindAsync(userId);
                if (customer == null) return NotFound();
                userName = customer.full_name;
                verificationStatus = customer.verification_status;
                ViewBag.UserEmail = customer.email;
            }
            else
            {
                var agent = await _context.Agents.FindAsync(userId);
                if (agent == null) return NotFound();
                userName = agent.full_name;
                verificationStatus = agent.verification_status;
                ViewBag.UserEmail = agent.email;
            }

            var documents = await _context.UserDocuments
                .Where(d => d.user_type == userType && d.user_id == userId)
                .OrderByDescending(d => d.uploaded_at)
                .ToListAsync();

            var (completed, total, percentage) = await _docService.GetCompletionAsync(userType, userId);

            ViewBag.UserName = userName;
            ViewBag.UserType = userType;
            ViewBag.UserId = userId;
            ViewBag.VerificationStatus = verificationStatus;
            ViewBag.Completed = completed;
            ViewBag.Total = total;
            ViewBag.Percentage = percentage;

            return View(documents);
        }

        // ========================================
        // Approve Document (with Audit Log)
        // ========================================
        [HttpPost]
        public async Task<IActionResult> ApproveDocument(int id, string? returnUserId, string? returnUserType)
        {
            var doc = await _context.UserDocuments.FindAsync(id);
            if (doc != null)
            {
                var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var adminName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";

                doc.status = "approved";
                doc.reviewed_by = adminId;
                doc.reviewed_at = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Write audit log via raw SQL to avoid DbContext state issues
                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO ""AuditLogs"" (""ActionType"", ""EntityType"", ""EntityId"", ""PerformedBy"", ""PerformedByName"", ""Details"", ""CreatedAt"")
                          VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                        "ApproveDocument", "UserDocument", doc.document_id, adminId, adminName,
                        $"Approved '{doc.document_name}' ({doc.category}) for {doc.user_type} #{doc.user_id}",
                        DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    // Audit log failure should not block the approval
                    System.Diagnostics.Debug.WriteLine($"Audit log write failed: {ex.Message}");
                }

                // Recompute verification status
                await _docService.UpdateVerificationStatusAsync(doc.user_type, doc.user_id);
            }

            // Return to per-user view if we came from there
            if (!string.IsNullOrEmpty(returnUserId) && !string.IsNullOrEmpty(returnUserType))
                return RedirectToAction("UserDocuments", new { userId = returnUserId, userType = returnUserType });

            return RedirectToAction("DocumentVerification");
        }

        // ========================================
        // Reject Document (with Audit Log)
        // ========================================
        [HttpPost]
        public async Task<IActionResult> RejectDocument(int id, string? reason, string? returnUserId, string? returnUserType)
        {
            var doc = await _context.UserDocuments.FindAsync(id);
            if (doc != null)
            {
                var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var adminName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
                var rejectionReason = reason ?? "Document rejected by admin.";

                doc.status = "rejected";
                doc.rejection_reason = rejectionReason;
                doc.reviewed_by = adminId;
                doc.reviewed_at = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Write audit log via raw SQL to avoid DbContext state issues
                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO ""AuditLogs"" (""ActionType"", ""EntityType"", ""EntityId"", ""PerformedBy"", ""PerformedByName"", ""Details"", ""CreatedAt"")
                          VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                        "RejectDocument", "UserDocument", doc.document_id, adminId, adminName,
                        $"Rejected '{doc.document_name}' ({doc.category}) for {doc.user_type} #{doc.user_id}: {rejectionReason}",
                        DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Audit log write failed: {ex.Message}");
                }

                // Recompute verification status
                await _docService.UpdateVerificationStatusAsync(doc.user_type, doc.user_id);
            }

            if (!string.IsNullOrEmpty(returnUserId) && !string.IsNullOrEmpty(returnUserType))
                return RedirectToAction("UserDocuments", new { userId = returnUserId, userType = returnUserType });

            return RedirectToAction("DocumentVerification");
        }

        // ========================================
        // Audit Log View
        // ========================================
        public async Task<IActionResult> AuditLog(int page = 1)
        {
            const int pageSize = 30;
            var totalCount = await _context.AuditLogs.CountAsync();
            var logs = await _context.AuditLogs
                .OrderByDescending(a => a.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;
            return View(logs);
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
                ("LIC", "lic@gmail.com", "Mumbai, India", "500", "Government life insurance provider", "LIC001", 1),
                ("HDFC Life Insurance", "hdfc@gmail.com", "Mumbai, India", "300", "Private life insurance company", "HDFC001", 1),
                ("ICICI Prudential Life Insurance", "icici@gmail.com", "Mumbai, India", "250", "Joint venture insurance company", "ICICI001", 1),
                ("SBI Life Insurance", "sbi@gmail.com", "India", "400", "Bank-based life insurance", "SBI001", 1),
                ("Max Life Insurance", "max@gmail.com", "India", "200", "Private life insurance provider", "MAX001", 1),
                ("Star Health Insurance", "star@gmail.com", "Chennai, India", "220", "Health insurance specialist", "STAR001", 2),
                ("Niva Bupa Health Insurance", "niva@gmail.com", "Delhi, India", "180", "Global health insurance", "NIVA001", 2),
                ("Care Health Insurance", "care@gmail.com", "India", "160", "Affordable health plans", "CARE001", 2),
                ("Aditya Birla Health Insurance", "birla@gmail.com", "Mumbai, India", "210", "Wellness-based insurance", "BIRLA001", 2),
                ("HDFC ERGO Health", "ergohealth@gmail.com", "India", "190", "Comprehensive health insurance", "ERGOH001", 2),
                ("Bajaj Allianz General Insurance", "bajaj@gmail.com", "Pune, India", "350", "Motor insurance leader", "BAJAJ001", 3),
                ("ICICI Lombard", "lombard@gmail.com", "Mumbai, India", "300", "General insurance provider", "LOMB001", 3),
                ("TATA AIG General Insurance", "tata@gmail.com", "India", "280", "Trusted motor insurance", "TATA001", 3),
                ("Reliance General Insurance", "reliance@gmail.com", "India", "260", "Affordable policies", "REL001", 3),
                ("HDFC ERGO", "ergo@gmail.com", "India", "240", "Motor and general insurance", "ERGO001", 3),
                ("New India Assurance", "new@gmail.com", "India", "150", "Public sector insurer", "NEW001", 4),
                ("United India Insurance", "united@gmail.com", "India", "140", "Government insurance", "UNIT001", 4),
                ("Oriental Insurance Company", "oriental@gmail.com", "India", "130", "Property insurance provider", "ORI001", 4),
                ("National Insurance Company", "national@gmail.com", "India", "120", "Public insurer", "NAT001", 4),
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

        // ========================================
        // Advertisement Management
        // ========================================
        public async Task<IActionResult> ManageAds(string? filter)
        {
            var query = _context.Advertisements.Include(a => a.Company).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter) && filter != "all")
                query = query.Where(a => a.status == filter);

            var ads = await query.OrderByDescending(a => a.created_at).ToListAsync();

            // Get plan names
            var planKeys = ads.Select(a => new { a.plan_id, a.company_id }).Distinct().ToList();
            var planNames = new Dictionary<string, string>();
            foreach (var key in planKeys)
            {
                var plan = await _context.InsurancePlans
                    .FirstOrDefaultAsync(p => p.plan_id == key.plan_id && p.company_id == key.company_id);
                if (plan != null)
                    planNames[$"{key.plan_id}_{key.company_id}"] = plan.plan_name;
            }

            // Get payments
            var adIds = ads.Select(a => a.ad_id).ToList();
            var payments = await _context.AdPayments
                .Where(p => adIds.Contains(p.advertisement_id))
                .ToDictionaryAsync(p => p.advertisement_id, p => p);

            ViewBag.PlanNames = planNames;
            ViewBag.Payments = payments;
            ViewBag.Filter = filter ?? "all";
            ViewBag.AllCount = await _context.Advertisements.CountAsync();
            ViewBag.PendingCount = await _context.Advertisements.CountAsync(a => a.status == "pending");
            ViewBag.ApprovedCount = await _context.Advertisements.CountAsync(a => a.status == "approved");
            ViewBag.RejectedCount = await _context.Advertisements.CountAsync(a => a.status == "rejected");

            return View(ads);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAd(int id)
        {
            var ad = await _context.Advertisements.FindAsync(id);
            if (ad != null)
            {
                ad.status = "approved";
                ad.start_date = DateTime.UtcNow;
                ad.end_date = DateTime.UtcNow.AddDays(ad.duration_days);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageAds");
        }

        [HttpPost]
        public async Task<IActionResult> RejectAd(int id)
        {
            var ad = await _context.Advertisements.FindAsync(id);
            if (ad != null)
            {
                ad.status = "rejected";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageAds");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var connString = _context.Database.GetConnectionString();
            using var conn = new Npgsql.NpgsqlConnection(connString);
            await conn.OpenAsync();
            var sql = @"
                DELETE FROM ""AdPayments"" WHERE ""AdvertisementId"" = @id;
                DELETE FROM ""Advertisements"" WHERE ""AdId"" = @id;
            ";
            
            // Note: Postgres columns were defined with PascalCase via raw SQL in Program.cs
            // EF Core translates them via snake_case, but raw SQL must use exact quoted PascalCase.
            using var cmd = new Npgsql.NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            
            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                // Fallback to snake_case column names if the DB table structure uses snake_case
                var fallbackSql = @"
                    DELETE FROM ""AdPayments"" WHERE advertisement_id = @id;
                    DELETE FROM ""Advertisements"" WHERE ad_id = @id;
                ";
                using var cmdFallback = new Npgsql.NpgsqlCommand(fallbackSql, conn);
                cmdFallback.Parameters.AddWithValue("id", id);
                await cmdFallback.ExecuteNonQueryAsync();
            }
            
            return RedirectToAction("ManageAds");
        }
    }
}
