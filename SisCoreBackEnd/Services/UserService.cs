using Microsoft.EntityFrameworkCore;
using SisCoreBackEnd.Data;
using SisCoreBackEnd.Domain.Tenant;
using SisCoreBackEnd.Tenancy;

namespace SisCoreBackEnd.Services
{
    public class UserService : IUserService
    {
        private readonly TenantDbContextFactory _dbFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(TenantDbContextFactory dbFactory, IHttpContextAccessor httpContextAccessor)
        {
            _dbFactory = dbFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private TenantDbContext GetDbContext()
        {
            // El TenantDbContextFactory ya valida el tenant context
            return _dbFactory.CreateDbContext();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var db = GetDbContext();
            return await db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            using var db = GetDbContext();
            return await db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.Status == 1)
                .ToListAsync();
        }

        public async Task<User> UpdateUserAsync(int id, string? email, string? phoneNumber, string? fullName, string? employeeNumber)
        {
            using var db = GetDbContext();
            var user = await db.Users.FindAsync(id);
            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado");

            if (!string.IsNullOrEmpty(email) && email != user.Email)
            {
                var exists = await db.Users.AnyAsync(u => u.Email == email && u.Id != id);
                if (exists)
                    throw new InvalidOperationException("El email ya está en uso");
                user.Email = email;
            }

            if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber != user.PhoneNumber)
            {
                var exists = await db.Users.AnyAsync(u => u.PhoneNumber == phoneNumber && u.Id != id);
                if (exists)
                    throw new InvalidOperationException("El teléfono ya está en uso");
                user.PhoneNumber = phoneNumber;
            }

            if (!string.IsNullOrEmpty(fullName))
                user.FullName = fullName;

            if (!string.IsNullOrEmpty(employeeNumber))
                user.EmployeeNumber = employeeNumber;

            user.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using var db = GetDbContext();
            var user = await db.Users.FindAsync(id);
            if (user == null)
                return false;

            user.Status = 0; // Soft delete
            user.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AssignRolesAsync(int userId, List<int> roleIds, int? assignedBy)
        {
            using var db = GetDbContext();
            var user = await db.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Eliminar roles actuales
            var currentRoles = await db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            db.UserRoles.RemoveRange(currentRoles);

            // Asignar nuevos roles
            var roles = await db.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync();
            foreach (var role in roles)
            {
                db.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id,
                    AssignedBy = assignedBy
                });
            }

            await db.SaveChangesAsync();
            return true;
        }
    }
}

