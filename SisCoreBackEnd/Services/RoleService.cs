using Microsoft.EntityFrameworkCore;
using SisCoreBackEnd.Data;
using SisCoreBackEnd.DTOs.Roles;
using SisCoreBackEnd.Domain.Tenant;
using SisCoreBackEnd.Tenancy;

namespace SisCoreBackEnd.Services
{
    public class RoleService : IRoleService
    {
        private readonly TenantDbContextFactory _dbFactory;
        private readonly IMasterUserService _masterUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoleService(
            TenantDbContextFactory dbFactory, 
            IMasterUserService masterUserService,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbFactory = dbFactory;
            _masterUserService = masterUserService;
            _httpContextAccessor = httpContextAccessor;
        }

        private TenantDbContext GetDbContext()
        {
            // El TenantDbContextFactory ya valida el tenant context
            return _dbFactory.CreateDbContext();
        }

        public async Task<List<RoleResponse>> GetRolesAsync()
        {
            using var db = GetDbContext();
            
            // Obtener email del usuario actual desde el token JWT
            var email = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            
            // Verificar si el usuario es God (solo God puede ver el rol God)
            var isGod = false;
            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    isGod = await _masterUserService.IsUserGodByEmailAsync(email);
                }
                catch
                {
                    // Si hay error al verificar, asumimos que no es God
                    isGod = false;
                }
            }
            
            var roles = await db.Roles
                .Where(r => r.Status == 1)
                .ToListAsync();

            // Filtrar rol "God" si el usuario no es God
            var filteredRoles = roles;
            if (!isGod)
            {
                filteredRoles = roles.Where(r => !r.Name.Equals("God", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return filteredRoles.Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystem = r.IsSystem,
                Status = r.Status,
                Permissions = new List<PermissionInfo>()
            }).ToList();
        }

        public async Task<RoleResponse?> GetRoleByIdAsync(int id)
        {
            using var db = GetDbContext();
            var role = await db.Roles
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                return null;

            return new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystem = role.IsSystem,
                Status = role.Status,
                Permissions = new List<PermissionInfo>()
            };
        }

        public async Task<Role> CreateRoleAsync(CreateRoleRequest request, int? createdBy)
        {
            using var db = GetDbContext();

            // Verificar que el nombre no exista
            var exists = await db.Roles.AnyAsync(r => r.Name == request.Name);
            if (exists)
                throw new InvalidOperationException("El nombre del rol ya existe");

            var role = new Role
            {
                Name = request.Name,
                Description = request.Description,
                IsSystem = false,
                Status = 1
            };

            db.Roles.Add(role);
            await db.SaveChangesAsync();

            return role;
        }

        public async Task<Role> UpdateRoleAsync(int id, string? name, string? description, List<int>? permissionIds)
        {
            using var db = GetDbContext();
            var role = await db.Roles
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                throw new InvalidOperationException("Rol no encontrado");

            if (role.IsSystem)
                throw new InvalidOperationException("No se pueden modificar roles del sistema");

            if (!string.IsNullOrEmpty(name) && name != role.Name)
            {
                var exists = await db.Roles.AnyAsync(r => r.Name == name && r.Id != id);
                if (exists)
                    throw new InvalidOperationException("El nombre del rol ya existe");
                role.Name = name;
            }

            if (description != null)
                role.Description = description;

            await db.SaveChangesAsync();
            return role;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            using var db = GetDbContext();
            var role = await db.Roles.FindAsync(id);
            if (role == null)
                return false;

            if (role.IsSystem)
                throw new InvalidOperationException("No se pueden eliminar roles del sistema");

            role.Status = 0; // Soft delete
            await db.SaveChangesAsync();

            return true;
        }
    }
}

