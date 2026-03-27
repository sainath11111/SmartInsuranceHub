using Microsoft.EntityFrameworkCore;
using SmartInsuranceHub.Models;

namespace SmartInsuranceHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<InsuranceType> InsuranceTypes { get; set; }
        public DbSet<InsurancePlan> InsurancePlans { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Query> Queries { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<UserDocument> UserDocuments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<AdPayment> AdPayments { get; set; }
        public DbSet<CustomerAgentMessage> CustomerAgentMessages { get; set; }
        public DbSet<PolicyRequest> PolicyRequests { get; set; }
        public DbSet<AgentCity> AgentCities { get; set; }
        public DbSet<City> Cities { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // InsuranceType: simple admin-managed, single PK
            modelBuilder.Entity<InsuranceType>()
                .HasKey(it => it.type_id);

            // InsurancePlan: composite key
            modelBuilder.Entity<InsurancePlan>()
                .HasKey(ip => new { ip.plan_id, ip.company_id });

            // Relationships
            modelBuilder.Entity<AgentCity>()
                .HasOne(ac => ac.Agent)
                .WithMany(a => a.AgentCities)
                .HasForeignKey(ac => ac.agent_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Agent>()
                .HasOne(a => a.Company)
                .WithMany()
                .HasForeignKey(a => a.company_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Policy>()
                .HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.customer_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Policy>()
                .HasOne(p => p.Agent)
                .WithMany()
                .HasForeignKey(p => p.agent_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Policy>()
                .HasOne(p => p.InsurancePlan)
                .WithMany()
                .HasForeignKey(p => new { p.plan_id, p.company_id })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Policy)
                .WithMany()
                .HasForeignKey(p => p.policy_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.customer_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.customer_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.InsurancePlan)
                .WithMany()
                .HasForeignKey(r => new { r.plan_id, r.company_id })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InsurancePlan>()
                .HasOne(ip => ip.Company)
                .WithMany()
                .HasForeignKey(ip => ip.company_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(c => c.Company)
                .WithMany()
                .HasForeignKey(c => c.company_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(c => c.Agent)
                .WithMany()
                .HasForeignKey(c => c.agent_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerAgentMessage>()
                .HasOne(c => c.Customer)
                .WithMany()
                .HasForeignKey(c => c.customer_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerAgentMessage>()
                .HasOne(c => c.Agent)
                .WithMany()
                .HasForeignKey(c => c.agent_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Advertisement>()
                .HasOne(a => a.Company)
                .WithMany()
                .HasForeignKey(a => a.company_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdPayment>()
                .HasOne(p => p.Company)
                .WithMany()
                .HasForeignKey(p => p.company_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdPayment>()
                .HasOne(p => p.Advertisement)
                .WithMany()
                .HasForeignKey(p => p.advertisement_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PolicyRequest>()
                .HasOne(pr => pr.Customer)
                .WithMany()
                .HasForeignKey(pr => pr.customer_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PolicyRequest>()
                .HasOne(pr => pr.Agent)
                .WithMany()
                .HasForeignKey(pr => pr.agent_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PolicyRequest>()
                .HasOne(pr => pr.InsurancePlan)
                .WithMany()
                .HasForeignKey(pr => new { pr.plan_id, pr.company_id })
                .OnDelete(DeleteBehavior.Restrict);

            // Dynamically map snake_case properties to PascalCase DB columns
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    var name = property.Name;
                    var parts = name.Split('_');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (parts[i].Length > 0)
                            parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
                    }
                    property.SetColumnName(string.Join("", parts));
                }
            }
        }
    }
}
