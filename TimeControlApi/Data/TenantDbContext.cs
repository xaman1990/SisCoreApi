using Microsoft.EntityFrameworkCore;
using TimeControlApi.Domain.Tenant;

namespace TimeControlApi.Data
{
    public class TenantDbContext : DbContext
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> opts) : base(opts) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectUser> ProjectUsers => Set<ProjectUser>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<Timesheet> Timesheets => Set<Timesheet>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // User
            mb.Entity<User>(e =>
            {
                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.PhoneNumber).IsUnique();
                e.HasIndex(x => x.GoogleId).IsUnique();
                e.HasIndex(x => x.EmployeeNumber).IsUnique();
                e.Property(x => x.Email).HasMaxLength(255);
                e.Property(x => x.PhoneNumber).HasMaxLength(20);
                e.Property(x => x.GoogleId).HasMaxLength(255);
                e.Property(x => x.FullName).HasMaxLength(255);
                e.Property(x => x.EmployeeNumber).HasMaxLength(50);
            });

            // Role
            mb.Entity<Role>(e =>
            {
                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.Description).HasMaxLength(500);
            });

            // UserRole
            mb.Entity<UserRole>(e =>
            {
                e.HasKey(k => new { k.UserId, k.RoleId });
                e.HasOne(x => x.User)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Role)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Module
            mb.Entity<Module>(e =>
            {
                e.HasIndex(x => x.Code).IsUnique();
                e.Property(x => x.Code).HasMaxLength(50);
                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.Description).HasMaxLength(500);
                e.Property(x => x.Icon).HasMaxLength(100);
            });

            // Permission
            mb.Entity<Permission>(e =>
            {
                e.HasIndex(x => new { x.ModuleId, x.Code }).IsUnique();
                e.Property(x => x.Code).HasMaxLength(100);
                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.Description).HasMaxLength(500);
                e.HasOne(x => x.Module)
                    .WithMany(x => x.Permissions)
                    .HasForeignKey(x => x.ModuleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RolePermission
            mb.Entity<RolePermission>(e =>
            {
                e.HasKey(k => new { k.RoleId, k.PermissionId });
                e.HasOne(x => x.Role)
                    .WithMany(x => x.RolePermissions)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Permission)
                    .WithMany(x => x.RolePermissions)
                    .HasForeignKey(x => x.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RefreshToken
            mb.Entity<RefreshToken>(e =>
            {
                e.HasIndex(x => x.Jti).IsUnique();
                e.Property(x => x.Jti).HasMaxLength(255);
                e.Property(x => x.DeviceId).HasMaxLength(255);
                e.Property(x => x.DeviceName).HasMaxLength(255);
                e.Property(x => x.IpAddress).HasMaxLength(45);
                e.Property(x => x.ReplacedByJti).HasMaxLength(255);
                e.HasOne(x => x.User)
                    .WithMany(x => x.RefreshTokens)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CompanySettings
            mb.Entity<CompanySettings>(e =>
            {
                e.HasIndex(x => x.Key).IsUnique();
                e.Property(x => x.Key).HasMaxLength(100);
                e.Property(x => x.Category).HasMaxLength(50);
                e.Property(x => x.Description).HasMaxLength(500);
            });

            // Project
            mb.Entity<Project>(e =>
            {
                e.HasIndex(x => x.Code).IsUnique();
                e.Property(x => x.Code).HasMaxLength(50);
                e.Property(x => x.Name).HasMaxLength(255);
                e.Property(x => x.DefaultHoursPerDay).HasPrecision(5, 2);
                e.HasOne(x => x.Supervisor)
                    .WithMany()
                    .HasForeignKey(x => x.SupervisorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ProjectUser
            mb.Entity<ProjectUser>(e =>
            {
                e.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
                e.HasOne(x => x.Project)
                    .WithMany(x => x.ProjectUsers)
                    .HasForeignKey(x => x.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Activity
            mb.Entity<Activity>(e =>
            {
                e.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique();
                e.Property(x => x.Code).HasMaxLength(50);
                e.Property(x => x.Name).HasMaxLength(255);
                e.HasOne(x => x.Project)
                    .WithMany(x => x.Activities)
                    .HasForeignKey(x => x.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Timesheet
            mb.Entity<Timesheet>(e =>
            {
                e.Property(p => p.Note).HasMaxLength(500);
                e.Property(p => p.Hours).HasPrecision(5, 2);
                e.Property(p => p.LocationAddress).HasMaxLength(500);
                e.Property(p => p.RejectionReason).HasMaxLength(500);
                e.Property(p => p.LocationLatitude).HasPrecision(10, 8);
                e.Property(p => p.LocationLongitude).HasPrecision(11, 8);
                e.HasIndex(x => new { x.UserId, x.Date });
                e.HasIndex(x => new { x.ProjectId, x.Date });
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.Date);
                e.HasOne(x => x.User)
                    .WithMany(x => x.Timesheets)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Project)
                    .WithMany(x => x.Timesheets)
                    .HasForeignKey(x => x.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Activity)
                    .WithMany(x => x.Timesheets)
                    .HasForeignKey(x => x.ActivityId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
