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

// News API Configuration & Services
builder.Services.AddSingleton<NewsApiConfig>();
builder.Services.AddScoped<GNewsApiClient>();
builder.Services.AddScoped<NewsService>();

// Document Verification Services
builder.Services.AddScoped<R2StorageService>();
builder.Services.AddScoped<DocumentService>();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

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
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration note: {ex.Message}");
    }

    // Ensure database is created/migrated
    context.Database.EnsureCreated();

    if (!context.Companies.Any())
    {
        var topCompanies = new List<string>
        {
            "State Farm", "Berkshire Hathaway", "Progressive", "Allstate", "Liberty Mutual",
            "Travelers", "Chubb", "USAA", "Farmers Insurance", "Nationwide",
            "American International Group (AIG)", "Geico", "MetLife", "Prudential Financial", "New York Life",
            "MassMutual", "Northwestern Mutual", "Lincoln National", "Principal Financial", "Aflac",
            "Life Insurance Corporation of India (LIC)", "HDFC Life", "SBI Life", "ICICI Prudential", "Max Life"
        };

        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Company@123");

        foreach (var name in topCompanies)
        {
            var emailPrefix = name.ToLower().Replace(" ", "").Replace("(", "").Replace(")", "");
            context.Companies.Add(new Company
            {
                company_name = name,
                email = $"info@{emailPrefix}.com",
                password = defaultPasswordHash,
                status = "approved",
                created_at = DateTime.UtcNow
            });
        }
        
        context.SaveChanges();
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
