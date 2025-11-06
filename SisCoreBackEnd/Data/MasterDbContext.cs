using Microsoft.EntityFrameworkCore;
using TimeControlApi.Domain.Master;

namespace TimeControlApi.Data
{
    public class MasterDbContext : DbContext
    {
        public MasterDbContext(DbContextOptions<MasterDbContext> opts) : base(opts) { }
        
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<MasterUser> MasterUsers => Set<MasterUser>();
        public DbSet<MasterUserSession> MasterUserSessions => Set<MasterUserSession>();
        public DbSet<MasterUserCompany> MasterUserCompanies => Set<MasterUserCompany>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Company>(e =>
            {
                e.HasIndex(x => x.Subdomain).IsUnique();
                e.Property(x => x.Name).HasMaxLength(255);
                e.Property(x => x.Subdomain).HasMaxLength(100);
                e.Property(x => x.DbHost).HasMaxLength(255);
                e.Property(x => x.DbName).HasMaxLength(100);
                e.Property(x => x.DbUser).HasMaxLength(100);
                e.Property(x => x.ConnectionOptions).HasMaxLength(500);
            });

            mb.Entity<MasterUser>(e =>
            {
                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.GoogleId).IsUnique();
                e.HasIndex(x => new { x.TenantUserId, x.TenantCompanyId }).IsUnique();
                e.Property(x => x.Email).HasMaxLength(255);
                e.Property(x => x.FullName).HasMaxLength(255);
                e.Property(x => x.PhoneNumber).HasMaxLength(20);
                e.Property(x => x.GoogleId).HasMaxLength(255);
                
                // Relación con Company (tenant company)
                e.HasOne(x => x.TenantCompany)
                    .WithMany()
                    .HasForeignKey(x => x.TenantCompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            mb.Entity<MasterUserSession>(e =>
            {
                e.HasIndex(x => x.RefreshTokenJti).IsUnique();
                e.Property(x => x.RefreshTokenJti).HasMaxLength(255);
                e.HasOne(x => x.MasterUser)
                    .WithMany(x => x.Sessions)
                    .HasForeignKey(x => x.MasterUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<MasterUserCompany>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.MasterUserId, x.CompanyId }).IsUnique();
                e.Property(x => x.Role).HasMaxLength(50);
                e.HasOne(x => x.MasterUser)
                    .WithMany(x => x.MasterUserCompanies)
                    .HasForeignKey(x => x.MasterUserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Company)
                    .WithMany(x => x.MasterUserCompanies)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
