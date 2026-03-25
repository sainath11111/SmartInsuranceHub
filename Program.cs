using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartInsuranceHub.ApiClients;
using SmartInsuranceHub.Configuration;
using SmartInsuranceHub.Data;
using SmartInsuranceHub.Services;
using System.Text;
using SmartInsuranceHub.Models;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<JwtAuthService>();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

// News API Configuration & Services
builder.Services.AddSingleton<NewsApiConfig>();
builder.Services.AddScoped<GNewsApiClient>();
builder.Services.AddScoped<NewsService>();

// Document Verification Services
builder.Services.AddScoped<R2StorageService>();
builder.Services.AddScoped<DocumentService>();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));

// Use Cookie Authentication for MVC (much simpler and more reliable for web apps)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/Login";
    options.Cookie.Name = "AuthCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP in dev
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Add Role-Based Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CompanyOnly", policy => policy.RequireRole("Company"));
    options.AddPolicy("AgentOnly", policy => policy.RequireRole("Agent"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});

var app = builder.Build();

app.MapHub<SmartInsuranceHub.Hubs.ChatHub>("/chatHub");

// ==========================================
// Database Seeding: Top 25 Insurance Companies
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Run schema migrations for new columns and tables
    try
    {
        context.Database.ExecuteSqlRaw(@"
            ALTER TABLE ""Customers"" ADD COLUMN IF NOT EXISTS ""VerificationStatus"" VARCHAR(20) DEFAULT 'unverified';
        ");
        context.Database.ExecuteSqlRaw(@"
            ALTER TABLE ""Agents"" ADD COLUMN IF NOT EXISTS ""VerificationStatus"" VARCHAR(20) DEFAULT 'unverified';
        ");
        context.Database.ExecuteSqlRaw(@"
            ALTER TABLE ""Companies"" ADD COLUMN IF NOT EXISTS ""InsuranceTypeId"" INTEGER;
        ");
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""UserDocuments"" (
                ""DocumentId"" SERIAL PRIMARY KEY,
                ""UserType"" VARCHAR(20) NOT NULL,
                ""UserId"" INTEGER NOT NULL,
                ""Category"" VARCHAR(100) NOT NULL,
                ""DocumentName"" VARCHAR(150) NOT NULL,
                ""FileUrl"" TEXT NOT NULL,
                ""FileName"" VARCHAR(255) DEFAULT '',
                ""FileSize"" BIGINT DEFAULT 0,
                ""Status"" VARCHAR(20) DEFAULT 'pending',
                ""RejectionReason"" TEXT,
                ""ReviewedBy"" INTEGER,
                ""UploadedAt"" TIMESTAMP DEFAULT NOW(),
                ""ReviewedAt"" TIMESTAMP
            );
        ");
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""AuditLogs"" (
                ""AuditLogId"" SERIAL PRIMARY KEY,
                ""ActionType"" VARCHAR(50) NOT NULL,
                ""EntityType"" VARCHAR(50) NOT NULL,
                ""EntityId"" INTEGER NOT NULL,
                ""PerformedBy"" INTEGER NOT NULL,
                ""PerformedByName"" VARCHAR(100),
                ""Details"" TEXT,
                ""CreatedAt"" TIMESTAMP DEFAULT NOW()
            );
        ");
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""Advertisements"" (
                ""AdId"" SERIAL PRIMARY KEY,
                ""CompanyId"" INTEGER NOT NULL,
                ""PlanId"" INTEGER NOT NULL,
                ""Title"" VARCHAR(200) NOT NULL,
                ""Description"" TEXT,
                ""BannerUrl"" TEXT,
                ""AmountPaid"" DECIMAL(12,2) DEFAULT 0,
                ""DurationDays"" INTEGER DEFAULT 7,
                ""StartDate"" TIMESTAMP,
                ""EndDate"" TIMESTAMP,
                ""Status"" VARCHAR(20) DEFAULT 'pending',
                ""CreatedAt"" TIMESTAMP DEFAULT NOW()
            );
        ");
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""AdPayments"" (
                ""AdPaymentId"" SERIAL PRIMARY KEY,
                ""CompanyId"" INTEGER NOT NULL,
                ""AdvertisementId"" INTEGER NOT NULL,
                ""Amount"" DECIMAL(12,2) DEFAULT 0,
                ""PaymentStatus"" VARCHAR(20) DEFAULT 'completed',
                ""PaymentDate"" TIMESTAMP DEFAULT NOW()
            );
        ");
        context.Database.ExecuteSqlRaw(@"
            ALTER TABLE ""Customers"" ADD COLUMN IF NOT EXISTS ""Dob"" TIMESTAMP NOT NULL DEFAULT '2000-01-01';
            DO $$ BEGIN IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Customers' AND column_name = 'dob') THEN ALTER TABLE ""Customers"" DROP COLUMN ""dob""; END IF; END $$;
        ");
        context.Database.ExecuteSqlRaw(@"
            ALTER TABLE ""Agents"" ADD COLUMN IF NOT EXISTS ""Dob"" TIMESTAMP NOT NULL DEFAULT '2000-01-01';
            DO $$ BEGIN IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Agents' AND column_name = 'dob') THEN ALTER TABLE ""Agents"" DROP COLUMN ""dob""; END IF; END $$;
        ");
        context.Database.ExecuteSqlRaw(@"
            ALTER TABLE ""Agents"" ADD COLUMN IF NOT EXISTS ""Aadhaar"" VARCHAR(20);
            DO $$ BEGIN IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Agents' AND column_name = 'aadhaar') THEN ALTER TABLE ""Agents"" DROP COLUMN ""aadhaar""; END IF; END $$;
        ");
        context.Database.ExecuteSqlRaw(@"
            ALTER TABLE ""Agents"" ADD COLUMN IF NOT EXISTS ""Pan"" VARCHAR(20);
            DO $$ BEGIN IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Agents' AND column_name = 'pan') THEN ALTER TABLE ""Agents"" DROP COLUMN ""pan""; END IF; END $$;
        ");
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""CustomerAgentMessages"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""CustomerId"" INTEGER NOT NULL,
                ""AgentId"" INTEGER NOT NULL,
                ""SenderType"" VARCHAR(20) NOT NULL,
                ""MessageText"" TEXT NOT NULL,
                ""SentAt"" TIMESTAMP DEFAULT NOW(),
                FOREIGN KEY (""CustomerId"") REFERENCES ""Customers""(""CustomerId"") ON DELETE RESTRICT,
                FOREIGN KEY (""AgentId"") REFERENCES ""Agents""(""AgentId"") ON DELETE RESTRICT
            );
        ");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration note: {ex.Message}");
    }

    // Ensure database is created/migrated
    context.Database.EnsureCreated();

    if (!context.Companies.Any())
    {
        var connString = context.Database.GetConnectionString();
        using var conn = new Npgsql.NpgsqlConnection(connString);
        conn.Open();

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
        cmdDelete.ExecuteNonQuery();

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
            
            cmdInsert.ExecuteNonQuery();
        }

        Console.WriteLine("✅ Seeded 24 Indian insurance companies successfully.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
