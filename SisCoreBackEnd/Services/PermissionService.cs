using Microsoft.EntityFrameworkCore;
using SisCoreBackEnd.Data;
using SisCoreBackEnd.Domain.Tenant;
using SisCoreBackEnd.DTOs.Permissions;
using SisCoreBackEnd.Tenancy;

namespace SisCoreBackEnd.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly TenantDbContextFactory _dbFactory;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(TenantDbContextFactory dbFactory, ILogger<PermissionService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        private TenantDbContext GetDbContext() => _dbFactory.CreateDbContext();

        private static string NormalizePermissionCode(string value) => value.Trim().ToLowerInvariant();

        private static string? NormalizeOptionalText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static PermissionCatalogResponse MapPermission(Permission permission) => new()
        {
            Id = permission.Id,
            Code = permission.Code,
            Name = permission.Name,
            Description = permission.Description,
            IsSystem = permission.IsSystem,
            IsDefaultForModule = permission.IsDefaultForModule,
            CreatedAt = permission.CreatedAt,
            UpdatedAt = permission.UpdatedAt
        };

        private static ModulePrivilegeResponse MapPrivilege(ModulePrivilege privilege) => new()
        {
            Id = privilege.Id,
            ModuleId = privilege.ModuleId,
            ModuleCode = privilege.Module?.Code ?? string.Empty,
            ModuleName = privilege.Module?.Name ?? string.Empty,
            PermissionId = privilege.PermissionId,
            PermissionCode = privilege.Permission?.Code ?? privilege.Code,
            PermissionName = privilege.Permission?.Name ?? privilege.Name,
            SubModuleId = privilege.SubModuleId,
            SubModuleCode = privilege.SubModule?.Code,
            SubModuleName = privilege.SubModule?.Name,
            Description = privilege.Description,
            CreatedAt = privilege.CreatedAt,
            CreatedBy = privilege.CreatedBy,
            UpdatedAt = privilege.UpdatedAt,
            UpdatedBy = privilege.UpdatedBy,
            IsDeleted = privilege.IsDeleted,
            IsDefault = privilege.IsDefault,
            HasAssignments = privilege.PermissionAssignments.Any(pa => !pa.IsDeleted)
        };

        #region Permission catalog

        public async Task<IReadOnlyList<PermissionCatalogResponse>> GetPermissionCatalogAsync(PermissionCatalogFilter filter)
        {
            using var db = GetDbContext();

            filter ??= new PermissionCatalogFilter();

            var query = db.Permissions.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Code))
            {
                var code = NormalizePermissionCode(filter.Code);
                query = query.Where(p => p.Code.Contains(code));
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var name = filter.Name.Trim();
                query = query.Where(p => p.Name.Contains(name));
            }

            if (!filter.IncludeSystem)
            {
                query = query.Where(p => !p.IsSystem);
            }

            if (filter.OnlyDefaults.HasValue)
            {
                query = query.Where(p => p.IsDefaultForModule == filter.OnlyDefaults.Value);
            }

            var permissions = await query
                .OrderBy(p => p.Code)
                .ToListAsync();

            return permissions.Select(MapPermission).ToList();
        }

        public async Task<PermissionCatalogResponse?> GetPermissionCatalogByIdAsync(int id)
        {
            using var db = GetDbContext();
            var permission = await db.Permissions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            return permission == null ? null : MapPermission(permission);
        }

        public async Task<PermissionCatalogResponse> CreatePermissionAsync(CreatePermissionRequest request, int createdBy)
        {
            using var db = GetDbContext();

            if (string.IsNullOrWhiteSpace(request.Code))
                throw new InvalidOperationException("El código del permiso es requerido.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("El nombre del permiso es requerido.");

            var normalizedCode = NormalizePermissionCode(request.Code);

            var exists = await db.Permissions.AnyAsync(p => p.Code == normalizedCode);
            if (exists)
                throw new InvalidOperationException("Ya existe un permiso con el mismo código.");

            var permission = new Permission
            {
                Code = normalizedCode,
                Name = request.Name.Trim(),
                Description = NormalizeOptionalText(request.Description),
                IsSystem = request.IsSystem,
                IsDefaultForModule = request.IsDefaultForModule,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            db.Permissions.Add(permission);
            await db.SaveChangesAsync();

            if (request.IsDefaultForModule)
            {
                var modules = await db.Modules
                    .AsNoTracking()
                    .Select(m => m.Id)
                    .ToListAsync();

                if (modules.Count > 0)
                {
                    var now = DateTime.UtcNow;
                    var modulePrivileges = modules.Select(moduleId => new ModulePrivilege
                    {
                        ModuleId = moduleId,
                        PermissionId = permission.Id,
                        Code = permission.Code,
                        Name = permission.Name,
                        Description = permission.Description,
                        CreatedAt = now,
                        CreatedBy = createdBy,
                        IsDefault = true,
                        IsDeleted = false
                    }).ToList();

                    db.ModulePrivileges.AddRange(modulePrivileges);
                    await db.SaveChangesAsync();
                }
            }

            return MapPermission(permission);
        }

        public async Task<PermissionCatalogResponse> UpdatePermissionAsync(int id, UpdatePermissionRequest request, int updatedBy)
        {
            using var db = GetDbContext();

            var permission = await db.Permissions.FirstOrDefaultAsync(p => p.Id == id);
            if (permission == null)
                throw new InvalidOperationException("Permiso no encontrado.");

            if (request.Code != null)
            {
                if (string.IsNullOrWhiteSpace(request.Code))
                    throw new InvalidOperationException("El código no puede estar vacío.");

                var normalizedCode = NormalizePermissionCode(request.Code);
                var exists = await db.Permissions.AnyAsync(p => p.Id != id && p.Code == normalizedCode);
                if (exists)
                    throw new InvalidOperationException("Ya existe otro permiso con el mismo código.");

                permission.Code = normalizedCode;
            }

            if (request.Name != null)
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    throw new InvalidOperationException("El nombre no puede estar vacío.");
                permission.Name = request.Name.Trim();
            }

            if (request.Description != null)
            {
                permission.Description = NormalizeOptionalText(request.Description);
            }

            if (request.IsSystem.HasValue)
            {
                permission.IsSystem = request.IsSystem.Value;
            }

            if (request.IsDefaultForModule.HasValue)
            {
                permission.IsDefaultForModule = request.IsDefaultForModule.Value;
            }

            permission.UpdatedAt = DateTime.UtcNow;
            permission.UpdatedBy = updatedBy;

            await db.SaveChangesAsync();

            return MapPermission(permission);
        }

        public async Task<bool> DeletePermissionAsync(int id, int? deletedBy)
        {
            using var db = GetDbContext();

            var permission = await db.Permissions
                .Include(p => p.ModulePrivileges)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null)
                return false;

            if (permission.IsSystem)
                throw new InvalidOperationException("No se puede eliminar un permiso del sistema.");

            if (permission.ModulePrivileges.Any())
                throw new InvalidOperationException("No se puede eliminar el permiso porque está asociado a módulos.");

            db.Permissions.Remove(permission);
            await db.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Module privileges

        public async Task<IReadOnlyList<ModulePrivilegeResponse>> GetModulePrivilegesAsync(int moduleId, ModulePrivilegeFilter filter)
        {
            using var db = GetDbContext();

            filter ??= new ModulePrivilegeFilter();

            var query = db.ModulePrivileges
                .AsNoTracking()
                .Include(mp => mp.Module)
                .Include(mp => mp.Permission)
                .Include(mp => mp.SubModule)
                .Include(mp => mp.PermissionAssignments)
                .Where(mp => mp.ModuleId == moduleId);

            if (!filter.IncludeDeleted)
            {
                query = query.Where(mp => !mp.IsDeleted);
            }

            if (filter.SubModuleId.HasValue)
            {
                query = query.Where(mp => mp.SubModuleId == filter.SubModuleId.Value);
            }

            if (filter.PermissionId.HasValue)
            {
                query = query.Where(mp => mp.PermissionId == filter.PermissionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.PermissionCode))
            {
                var code = NormalizePermissionCode(filter.PermissionCode);
                query = query.Where(mp => mp.Code == code || mp.Permission.Code == code);
            }

            var privileges = await query
                .OrderBy(mp => mp.Code)
                .ThenBy(mp => mp.Id)
                .ToListAsync();

            return privileges.Select(MapPrivilege).ToList();
        }

        public async Task<ModulePrivilegeResponse?> GetModulePrivilegeByIdAsync(int id)
        {
            using var db = GetDbContext();

            var privilege = await db.ModulePrivileges
                .AsNoTracking()
                .Include(mp => mp.Module)
                .Include(mp => mp.Permission)
                .Include(mp => mp.SubModule)
                .Include(mp => mp.PermissionAssignments)
                .FirstOrDefaultAsync(mp => mp.Id == id);

            return privilege == null ? null : MapPrivilege(privilege);
        }

        public async Task<ModulePrivilegeResponse> CreateModulePrivilegeAsync(int moduleId, CreateModulePrivilegeRequest request, int createdBy)
        {
            using var db = GetDbContext();

            var module = await db.Modules.FirstOrDefaultAsync(m => m.Id == moduleId);
            if (module == null)
                throw new InvalidOperationException("Módulo no encontrado.");

            var permission = await db.Permissions.FirstOrDefaultAsync(p => p.Id == request.PermissionId);
            if (permission == null)
                throw new InvalidOperationException("Permiso de catálogo no encontrado.");

            if (request.SubModuleId.HasValue)
            {
                var validSubModule = await db.SubModules.AnyAsync(sm =>
                    sm.Id == request.SubModuleId.Value &&
                    sm.ModuleId == moduleId &&
                    !sm.IsDeleted);
                if (!validSubModule)
                    throw new InvalidOperationException("El submódulo indicado no pertenece al módulo.");
            }

            var exists = await db.ModulePrivileges
                .AnyAsync(mp => mp.ModuleId == moduleId && mp.PermissionId == request.PermissionId && !mp.IsDeleted);
            if (exists)
                throw new InvalidOperationException("El módulo ya cuenta con ese privilegio.");

            var privilege = new ModulePrivilege
            {
                ModuleId = moduleId,
                PermissionId = permission.Id,
                SubModuleId = request.SubModuleId,
                Code = permission.Code,
                Name = request.NameOverride?.Trim() ?? permission.Name,
                Description = NormalizeOptionalText(request.Description) ?? permission.Description,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsDefault = request.IsDefault ?? permission.IsDefaultForModule,
                IsDeleted = false
            };

            db.ModulePrivileges.Add(privilege);
            await db.SaveChangesAsync();

            await db.Entry(privilege).Reference(p => p.Permission).LoadAsync();
            await db.Entry(privilege).Reference(p => p.Module).LoadAsync();
            await db.Entry(privilege).Reference(p => p.SubModule).LoadAsync();

            return MapPrivilege(privilege);
        }

        public async Task<ModulePrivilegeResponse> UpdateModulePrivilegeAsync(int id, UpdateModulePrivilegeRequest request, int updatedBy)
        {
            using var db = GetDbContext();

            var privilege = await db.ModulePrivileges
                .Include(mp => mp.Permission)
                .FirstOrDefaultAsync(mp => mp.Id == id);

            if (privilege == null)
                throw new InvalidOperationException("Privilegio no encontrado.");

            if (request.PermissionId.HasValue && request.PermissionId.Value != privilege.PermissionId)
            {
                var permission = await db.Permissions.FirstOrDefaultAsync(p => p.Id == request.PermissionId.Value);
                if (permission == null)
                    throw new InvalidOperationException("Permiso de catálogo no encontrado.");

                var exists = await db.ModulePrivileges
                    .AnyAsync(mp => mp.ModuleId == privilege.ModuleId &&
                                    mp.PermissionId == permission.Id &&
                                    mp.Id != privilege.Id &&
                                    !mp.IsDeleted);
                if (exists)
                    throw new InvalidOperationException("El módulo ya cuenta con ese privilegio.");

                privilege.PermissionId = permission.Id;
                privilege.Code = permission.Code;
                privilege.Name = permission.Name;
                privilege.Description = permission.Description;
                privilege.Permission = permission;
            }

            if (request.SubModuleId.HasValue)
            {
                var validSubModule = await db.SubModules.AnyAsync(sm =>
                    sm.Id == request.SubModuleId.Value &&
                    sm.ModuleId == privilege.ModuleId &&
                    !sm.IsDeleted);
                if (!validSubModule)
                    throw new InvalidOperationException("El submódulo indicado no pertenece al módulo.");

                privilege.SubModuleId = request.SubModuleId;
            }
            else if (request.RemoveSubModule)
            {
                privilege.SubModuleId = null;
            }

            if (request.NameOverride != null)
            {
                privilege.Name = request.NameOverride.Trim();
            }

            if (request.Description != null)
            {
                privilege.Description = NormalizeOptionalText(request.Description);
            }

            if (request.IsDefault.HasValue)
            {
                privilege.IsDefault = request.IsDefault.Value;
            }

            privilege.UpdatedAt = DateTime.UtcNow;
            privilege.UpdatedBy = updatedBy;

            await db.SaveChangesAsync();

            await db.Entry(privilege).Reference(p => p.Permission).LoadAsync();
            await db.Entry(privilege).Reference(p => p.Module).LoadAsync();
            await db.Entry(privilege).Reference(p => p.SubModule).LoadAsync();

            return MapPrivilege(privilege);
        }

        public async Task<bool> DeleteModulePrivilegeAsync(int id, int? updatedBy)
        {
            using var db = GetDbContext();

            var privilege = await db.ModulePrivileges.FirstOrDefaultAsync(mp => mp.Id == id);
            if (privilege == null)
                return false;

            if (privilege.IsDeleted)
                return true;

            var hasAssignments = await db.PermissionAssignments.AnyAsync(pa => pa.ModulePrivilegeId == id && !pa.IsDeleted);
            if (hasAssignments)
                throw new InvalidOperationException("No se puede eliminar el privilegio porque está asignado a roles o usuarios.");

            privilege.IsDeleted = true;
            privilege.UpdatedAt = DateTime.UtcNow;
            privilege.UpdatedBy = updatedBy;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreModulePrivilegeAsync(int id, int? updatedBy)
        {
            using var db = GetDbContext();

            var privilege = await db.ModulePrivileges.FirstOrDefaultAsync(mp => mp.Id == id);
            if (privilege == null)
                return false;

            if (!privilege.IsDeleted)
                return true;

            var exists = await db.ModulePrivileges.AnyAsync(mp =>
                mp.ModuleId == privilege.ModuleId &&
                mp.PermissionId == privilege.PermissionId &&
                mp.Id != privilege.Id &&
                !mp.IsDeleted);

            if (exists)
                throw new InvalidOperationException("Ya existe un privilegio activo con el mismo permiso para el módulo.");

            privilege.IsDeleted = false;
            privilege.UpdatedAt = DateTime.UtcNow;
            privilege.UpdatedBy = updatedBy;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task EnsureDefaultModulePrivilegesAsync(int moduleId, int? createdBy)
        {
            using var db = GetDbContext();

            var module = await db.Modules.FirstOrDefaultAsync(m => m.Id == moduleId);
            if (module == null)
                throw new InvalidOperationException("Módulo no encontrado.");

            var defaultPermissions = await db.Permissions
                .Where(p => p.IsDefaultForModule)
                .ToListAsync();

            if (!defaultPermissions.Any())
                return;

            var existing = await db.ModulePrivileges
                .Where(mp => mp.ModuleId == moduleId)
                .ToListAsync();

            foreach (var permission in defaultPermissions)
            {
                var privilege = existing.FirstOrDefault(mp => mp.PermissionId == permission.Id);
                if (privilege == null)
                {
                    db.ModulePrivileges.Add(new ModulePrivilege
                    {
                        ModuleId = moduleId,
                        PermissionId = permission.Id,
                        Code = permission.Code,
                        Name = permission.Name,
                        Description = permission.Description,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        IsDefault = true,
                        IsDeleted = false
                    });
                }
                else if (privilege.IsDeleted)
                {
                    privilege.IsDeleted = false;
                    privilege.IsDefault = true;
                    privilege.UpdatedAt = DateTime.UtcNow;
                    privilege.UpdatedBy = createdBy;
                }
            }

            await db.SaveChangesAsync();
        }

        #endregion

        #region Role permissions

        public async Task<IReadOnlyList<RoleModulePermissionsResponse>> GetRolePermissionsMatrixAsync(int roleId, int? moduleId = null)
        {
            using var db = GetDbContext();

            var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
                throw new InvalidOperationException("Rol no encontrado.");

            IQueryable<Module> moduleQuery = db.Modules
                .AsNoTracking()
                .Where(m => m.IsEnabled);

            if (moduleId.HasValue)
            {
                moduleQuery = moduleQuery.Where(m => m.Id == moduleId.Value);
            }

            var modules = await moduleQuery
                .Include(m => m.ModulePrivileges.Where(mp => !mp.IsDeleted))
                    .ThenInclude(mp => mp.Permission)
                .OrderBy(m => m.MenuOrder)
                .ThenBy(m => m.Name)
                .ToListAsync();

            var privilegeIds = modules.SelectMany(m => m.ModulePrivileges).Select(mp => mp.Id).Distinct().ToList();

            var assignments = await db.PermissionAssignments
                .Where(pa => pa.RoleId == roleId && privilegeIds.Contains(pa.ModulePrivilegeId))
                .ToDictionaryAsync(pa => pa.ModulePrivilegeId, pa => pa);

            var response = new List<RoleModulePermissionsResponse>();

            foreach (var module in modules)
            {
                var moduleResponse = new RoleModulePermissionsResponse
                {
                    RoleId = roleId,
                    ModuleId = module.Id,
                    ModuleCode = module.Code,
                    ModuleName = module.Name
                };

                foreach (var privilege in module.ModulePrivileges.OrderBy(mp => mp.Code))
                {
                    assignments.TryGetValue(privilege.Id, out var assignment);

                    moduleResponse.Privileges.Add(new RoleModulePrivilegeStatus
                    {
                        ModulePrivilegeId = privilege.Id,
                        PermissionId = privilege.PermissionId,
                        Code = privilege.Code,
                        Name = privilege.Name,
                        Description = privilege.Description,
                        IsGranted = assignment != null && !assignment.IsDeleted,
                        IsDefault= privilege.IsDefault
                    });
                }

                response.Add(moduleResponse);
            }

            return response;
        }

        public async Task UpdateRoleModulePermissionsAsync(int roleId, UpdateRoleModulePermissionsRequest request, int updatedBy)
        {
            using var db = GetDbContext();

            var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
                throw new InvalidOperationException("Rol no encontrado.");

            if (request.Modules == null || !request.Modules.Any())
                return;

            foreach (var moduleUpdate in request.Modules)
            {
                var privilegeIds = moduleUpdate.Privileges.Select(p => p.ModulePrivilegeId).ToList();

                var modulePrivileges = await db.ModulePrivileges
                    .Where(mp => mp.ModuleId == moduleUpdate.ModuleId && privilegeIds.Contains(mp.Id))
                    .ToDictionaryAsync(mp => mp.Id);

                if (modulePrivileges.Count != privilegeIds.Count)
                    throw new InvalidOperationException("Se intentó actualizar un privilegio que no pertenece al módulo.");

                var assignments = await db.PermissionAssignments
                    .Where(pa => pa.RoleId == roleId && privilegeIds.Contains(pa.ModulePrivilegeId))
                    .ToDictionaryAsync(pa => pa.ModulePrivilegeId);

                foreach (var privilegeToggle in moduleUpdate.Privileges)
                {
                    assignments.TryGetValue(privilegeToggle.ModulePrivilegeId, out var assignment);

                    if (privilegeToggle.IsGranted)
                    {
                        if (assignment == null)
                        {
                            db.PermissionAssignments.Add(new PermissionAssignment
                            {
                                ModulePrivilegeId = privilegeToggle.ModulePrivilegeId,
                                RoleId = roleId,
                                GrantedAt = DateTime.UtcNow,
                                GrantedBy = updatedBy,
                                ValidFrom = DateTime.UtcNow,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = updatedBy,
                                IsInherited = false,
                                IsDeleted = false
                            });
                        }
                        else if (assignment.IsDeleted)
                        {
                            assignment.IsDeleted = false;
                            assignment.UpdatedAt = DateTime.UtcNow;
                            assignment.UpdatedBy = updatedBy;
                            assignment.GrantedAt = DateTime.UtcNow;
                            assignment.GrantedBy = updatedBy;
                        }
                    }
                    else if (assignment != null && !assignment.IsDeleted)
                    {
                        assignment.IsDeleted = true;
                        assignment.UpdatedAt = DateTime.UtcNow;
                        assignment.UpdatedBy = updatedBy;
                    }
                }
            }

            await db.SaveChangesAsync();
        }

        #endregion

        #region Effective permissions

        public async Task<PermissionResponse> GetMyPermissionsAsync(int userId) =>
            await GetEffectivePermissionsAsync(userId);

        public async Task<UserPermissionResponse> GetUserPermissionsAsync(int userId, bool includeInherited)
        {
            using var db = GetDbContext();

            var user = await db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            var effective = await GetEffectivePermissionsAsync(userId);

            return new UserPermissionResponse
            {
                UserId = userId,
                UserName = user.FullName,
                Permissions = effective.Permissions,
                Roles = user.UserRoles
                    .Select(ur => new RoleInfo { Id = ur.Role.Id, Name = ur.Role.Name })
                    .ToList()
            };
        }

        public async Task<PermissionResponse> GetEffectivePermissionsAsync(int userId)
        {
            using var db = GetDbContext();

            var user = await db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

            var modulePrivileges = await db.ModulePrivileges
                .AsNoTracking()
                .Include(mp => mp.Module)
                .Include(mp => mp.Permission)
                .Where(mp => !mp.IsDeleted && mp.Module.IsEnabled)
                .OrderBy(mp => mp.Module.MenuOrder)
                .ThenBy(mp => mp.Code)
                .ToListAsync();

            var privilegeIds = modulePrivileges.Select(mp => mp.Id).ToList();

            var assignments = await db.PermissionAssignments
                .Where(pa => !pa.IsDeleted &&
                             privilegeIds.Contains(pa.ModulePrivilegeId) &&
                             (pa.UserId == userId ||
                              (pa.RoleId.HasValue && roleIds.Contains(pa.RoleId.Value))))
                .Include(pa => pa.Role)
                .ToListAsync();

            var assignmentsByPrivilege = assignments
                .GroupBy(pa => pa.ModulePrivilegeId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var response = new PermissionResponse
            {
                UserId = userId,
                Permissions = new List<ModulePermission>()
            };

            foreach (var moduleGroup in modulePrivileges.GroupBy(mp => new
                     {
                         ModuleId = mp.ModuleId,
                         ModuleCode = mp.Module.Code,
                         ModuleName = mp.Module.Name,
                         ModuleDescription = mp.Module.Description
                     }))
            {
                var modulePermission = new ModulePermission
                {
                    Id = moduleGroup.Key.ModuleId,
                    Code = moduleGroup.Key.ModuleCode,
                    Name = moduleGroup.Key.ModuleName,
                    Description = moduleGroup.Key.ModuleDescription
                };

                foreach (var privilege in moduleGroup.OrderBy(mp => mp.Code))
                {
                    if (!assignmentsByPrivilege.TryGetValue(privilege.Id, out var privilegeAssignments))
                    {
                        continue;
                    }

                    var direct = privilegeAssignments
                        .FirstOrDefault(pa => pa.UserId == userId && !pa.IsDeleted);

                    var roleAssignment = privilegeAssignments
                        .Where(pa => pa.RoleId.HasValue && !pa.IsDeleted && roleIds.Contains(pa.RoleId.Value))
                        .OrderBy(pa => pa.RoleId)
                        .FirstOrDefault();

                    var effectiveAssignment = direct ?? roleAssignment;

                    if (effectiveAssignment == null)
                    {
                        continue;
                    }

                    modulePermission.Privileges.Add(new PermissionPrivilege
                    {
                        ModulePrivilegeId = privilege.Id,
                        PermissionId = privilege.PermissionId,
                        PermissionAssignmentId = effectiveAssignment.Id,
                        Code = privilege.Code,
                        Name = privilege.Name,
                        HasPermission = true
                    });
                }

                if (modulePermission.Privileges.Any())
                {
                    response.Permissions.Add(modulePermission);
                }
            }

            return response;
        }

        public async Task<CheckPermissionResponse> CheckPermissionAsync(int userId, int moduleId, string permissionCode)
        {
            using var db = GetDbContext();

            var user = await db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new CheckPermissionResponse
                {
                    HasPermission = false,
                    Message = "Usuario no encontrado."
                };
            }

            var normalizedCode = NormalizePermissionCode(permissionCode);

            var privilege = await db.ModulePrivileges
                .Include(mp => mp.Module)
                .FirstOrDefaultAsync(mp =>
                    mp.ModuleId == moduleId &&
                    !mp.IsDeleted &&
                    mp.Code == normalizedCode);

            if (privilege == null)
            {
                return new CheckPermissionResponse
                {
                    HasPermission = false,
                    Message = "Privilegio no encontrado."
                };
            }

            var direct = await db.PermissionAssignments
                .FirstOrDefaultAsync(pa =>
                    !pa.IsDeleted &&
                    pa.UserId == userId &&
                    pa.ModulePrivilegeId == privilege.Id);

            if (direct != null)
            {
                return new CheckPermissionResponse
                {
                    HasPermission = true,
                    Source = "Direct"
                };
            }

            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

            var roleAssignment = await db.PermissionAssignments
                .Include(pa => pa.Role)
                .FirstOrDefaultAsync(pa =>
                    !pa.IsDeleted &&
                    pa.RoleId.HasValue &&
                    roleIds.Contains(pa.RoleId.Value) &&
                    pa.ModulePrivilegeId == privilege.Id);

            if (roleAssignment != null)
            {
                return new CheckPermissionResponse
                {
                    HasPermission = true,
                    Source = "Role",
                    RoleId = roleAssignment.RoleId,
                    RoleName = roleAssignment.Role?.Name
                };
            }

            return new CheckPermissionResponse
            {
                HasPermission = false,
                Message = "El usuario no tiene el permiso solicitado."
            };
        }

        #endregion
    }
}

