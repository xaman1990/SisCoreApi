using System.Collections.Immutable;
using SisCoreBackEnd.DTOs.Auth;
using SisCoreBackEnd.DTOs.Companies;
using SisCoreBackEnd.DTOs.MasterUsers;
using SisCoreBackEnd.DTOs.Modules;
using SisCoreBackEnd.DTOs.Permissions;
using SisCoreBackEnd.DTOs.Roles;

namespace SisCoreBackEnd.Swagger.Examples
{
    internal static class SwaggerExamplesCatalog
    {
        private static readonly ImmutableDictionary<string, EndpointExampleDefinition> Definitions = BuildDefinitions();

        public static bool TryGetExamples(string methodKey, out EndpointExampleDefinition definition) =>
            Definitions.TryGetValue(methodKey, out definition!);

        private static ImmutableDictionary<string, EndpointExampleDefinition> BuildDefinitions()
        {
            var builder = ImmutableDictionary.CreateBuilder<string, EndpointExampleDefinition>(StringComparer.OrdinalIgnoreCase);

            RegisterAuthExamples(builder);
            RegisterUsersExamples(builder);
            RegisterRolesExamples(builder);
            RegisterModulesExamples(builder);
            RegisterPermissionsExamples(builder);
            RegisterDiagnosticsExamples(builder);
            RegisterFormBuilderExamples(builder);
            RegisterMasterCompaniesExamples(builder);
            RegisterMasterUsersExamples(builder);

            return builder.ToImmutable();
        }

        private static void RegisterAuthExamples(IDictionary<string, EndpointExampleDefinition> builder)
        {
            builder["siscorebackend.controllers.authcontroller.login"] = new EndpointExampleDefinition()
                .WithRequest(new LoginRequest
                {
                    Email = "admin@timecontrol.com",
                    Password = "P@ssw0rd!",
                    DeviceId = "web-frontend",
                    DeviceName = "Browser Chrome"
                })
                .WithResponses(
                    (200, new LoginResponse
                    {
                        AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                        RefreshToken = "6c3b8f96-fd3c-4b9b-9e92-3c6e46cf3e09",
                        ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                        User = new UserInfo
                        {
                            Id = 1,
                            FullName = "Super Admin",
                            Email = "admin@timecontrol.com",
                            Roles = new List<string> { "SuperAdmin" },
                            MfaEnabled = false
                        }
                    }),
                    (400, new { message = "Debe proporcionar email o teléfono" }),
                    (401, new { message = "Credenciales inválidas" })
                );

            builder["siscorebackend.controllers.authcontroller.refreshtoken"] = new EndpointExampleDefinition()
                .WithRequest(new RefreshTokenRequest
                {
                    RefreshToken = "6c3b8f96-fd3c-4b9b-9e92-3c6e46cf3e09",
                    DeviceId = "web-frontend"
                })
                .WithResponses(
                    (200, new LoginResponse
                    {
                        AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                        RefreshToken = "9f9f16a8-4321-4a6c-9c8f-02b1f5c5d7ec",
                        ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                        User = new UserInfo
                        {
                            Id = 1,
                            FullName = "Super Admin",
                            Email = "admin@timecontrol.com",
                            Roles = new List<string> { "SuperAdmin" },
                            MfaEnabled = false
                        }
                    }),
                    (401, new { message = "Token de refresh inválido o expirado" })
                );

            builder["siscorebackend.controllers.authcontroller.logout"] = new EndpointExampleDefinition()
                .WithRequest(new RefreshTokenRequest
                {
                    RefreshToken = "9f9f16a8-4321-4a6c-9c8f-02b1f5c5d7ec"
                })
                .WithResponses(
                    (200, new { message = "Sesión cerrada exitosamente" }),
                    (400, new { message = "Token de refresh inválido" })
                );

            builder["siscorebackend.controllers.authcontroller.getcurrentuser"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new
                    {
                        id = "1",
                        email = "admin@timecontrol.com",
                        fullName = "Super Admin",
                        roles = new[] { "SuperAdmin", "Manager" }
                    })
                );

            builder["siscorebackend.controllers.authcontroller.validatetoken"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new
                    {
                        valid = true,
                        userId = "1",
                        email = "admin@timecontrol.com",
                        roles = new[] { "SuperAdmin" }
                    })
                );
        }

        private static void RegisterUsersExamples(IDictionary<string, EndpointExampleDefinition> builder)
        {
            builder["siscorebackend.controllers.userscontroller.registeruser"] = new EndpointExampleDefinition()
                .WithRequest(new RegisterUserRequest
                {
                    Email = "jane.doe@acme.com",
                    PhoneNumber = "+595971000000",
                    FullName = "Jane Doe",
                    Password = "P@ssw0rd!",
                    EmployeeNumber = "EMP-1024"
                })
                .WithResponses(
                    (201, new
                    {
                        id = 42,
                        email = "jane.doe@acme.com",
                        phoneNumber = "+595971000000",
                        fullName = "Jane Doe",
                        employeeNumber = "EMP-1024"
                    }),
                    (400, new { message = "Debe proporcionar email o teléfono" })
                );

            builder["siscorebackend.controllers.userscontroller.getusers"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new[]
                    {
                        new
                        {
                            id = 1,
                            email = "admin@timecontrol.com",
                            phoneNumber = "+595971123456",
                            fullName = "Super Admin",
                            employeeNumber = "EMP-0001",
                            status = "Active",
                            roles = new[]
                            {
                                new { id = 1, name = "SuperAdmin" }
                            }
                        },
                        new
                        {
                            id = 2,
                            email = "manager@acme.com",
                            phoneNumber = "+595971654321",
                            fullName = "Project Manager",
                            employeeNumber = "EMP-0020",
                            status = "Active",
                            roles = new[]
                            {
                                new { id = 2, name = "Manager" }
                            }
                        }
                    })
                );

            builder["siscorebackend.controllers.userscontroller.getuser"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new
                    {
                        id = 42,
                        email = "jane.doe@acme.com",
                        phoneNumber = "+595971000000",
                        fullName = "Jane Doe",
                        employeeNumber = "EMP-1024",
                        status = "Active",
                        roles = new[]
                        {
                            new { id = 2, name = "Manager" }
                        }
                    }),
                    (404, new { message = "Usuario no encontrado" })
                );

            builder["siscorebackend.controllers.userscontroller.updateuser"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    email = "jane.doe@acme.com",
                    phoneNumber = "+595971000000",
                    fullName = "Jane Doe",
                    employeeNumber = "EMP-1024"
                })
                .WithResponses(
                    (200, new
                    {
                        id = 42,
                        email = "jane.doe@acme.com",
                        phoneNumber = "+595971000000",
                        fullName = "Jane Doe",
                        employeeNumber = "EMP-1024"
                    })
                );

            builder["siscorebackend.controllers.userscontroller.deleteuser"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new { message = "Usuario eliminado exitosamente" }),
                    (404, new { message = "Usuario no encontrado" })
                );

            builder["siscorebackend.controllers.userscontroller.assignroles"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    roleIds = new[] { 1, 2 }
                })
                .WithResponses(
                    (200, new { message = "Roles asignados exitosamente" })
                );
        }

        private static void RegisterRolesExamples(IDictionary<string, EndpointExampleDefinition> builder)
        {
            builder["siscorebackend.controllers.rolescontroller.getroles"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new[]
                    {
                        new RoleResponse
                        {
                            Id = 1,
                            Name = "SuperAdmin",
                            Description = "Acceso completo al sistema",
                            IsSystem = true,
                            Permissions = new List<PermissionInfo>
                            {
                                new PermissionInfo { Id = 1, Code = "create", Name = "Crear", ModuleCode = "users" },
                                new PermissionInfo { Id = 2, Code = "read", Name = "Leer", ModuleCode = "users" },
                                new PermissionInfo { Id = 3, Code = "update", Name = "Actualizar", ModuleCode = "users" },
                                new PermissionInfo { Id = 4, Code = "delete", Name = "Eliminar", ModuleCode = "users" }
                            }
                        },
                        new RoleResponse
                        {
                            Id = 2,
                            Name = "Manager",
                            Description = "Gestión de proyectos y equipos",
                            IsSystem = false,
                            Permissions = new List<PermissionInfo>
                            {
                                new PermissionInfo { Id = 2, Code = "read", Name = "Leer", ModuleCode = "users" },
                                new PermissionInfo { Id = 3, Code = "update", Name = "Actualizar", ModuleCode = "users" }
                            }
                        }
                    })
                );

            builder["siscorebackend.controllers.rolescontroller.getrole"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new RoleResponse
                    {
                        Id = 2,
                        Name = "Manager",
                        Description = "Gestión de proyectos y equipos",
                        IsSystem = false,
                        Permissions = new List<PermissionInfo>
                        {
                            new PermissionInfo { Id = 2, Code = "read", Name = "Leer", ModuleCode = "users" },
                            new PermissionInfo { Id = 3, Code = "update", Name = "Actualizar", ModuleCode = "users" }
                        }
                    }),
                    (404, new { message = "Rol no encontrado" })
                );

            builder["siscorebackend.controllers.rolescontroller.createrole"] = new EndpointExampleDefinition()
                .WithRequest(new CreateRoleRequest
                {
                    Name = "Support",
                    Description = "Soporte a usuarios finales",
                    PermissionIds = new List<int> { 1, 2 }
                })
                .WithResponses(
                    (201, new
                    {
                        id = 5,
                        name = "Support",
                        description = "Soporte a usuarios finales"
                    }),
                    (400, new { message = "El nombre del rol es requerido" })
                );

            builder["siscorebackend.controllers.rolescontroller.updaterole"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    name = "Support",
                    description = "Soporte y seguimiento",
                    permissionIds = new[] { 1, 2, 5 }
                })
                .WithResponses(
                    (200, new
                    {
                        id = 5,
                        name = "Support",
                        description = "Soporte y seguimiento"
                    })
                );

            builder["siscorebackend.controllers.rolescontroller.deleterole"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new { message = "Rol eliminado exitosamente" }),
                    (404, new { message = "Rol no encontrado" })
                );
        }

        private static void RegisterModulesExamples(IDictionary<string, EndpointExampleDefinition> builder)
        {
            builder["siscorebackend.controllers.modulescontroller.getmodules"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new[]
                    {
                        new ModuleResponse
                        {
                            Id = 4,
                            Code = "users",
                            Name = "Usuarios",
                            Description = "Gestión de usuarios y roles",
                            Icon = "users",
                            MenuOrder = 1,
                            IsEnabled = true
                        },
                        new ModuleResponse
                        {
                            Id = 15,
                            Code = "modulos",
                            Name = "Módulos",
                            Description = "Administración de módulos",
                            Icon = "layers",
                            MenuOrder = 2,
                            IsEnabled = true
                        }
                    })
                );

            builder["siscorebackend.controllers.modulescontroller.getmodule"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new ModuleResponse
                    {
                        Id = 4,
                        Code = "users",
                        Name = "Usuarios",
                        Description = "Gestión de usuarios y roles",
                        Icon = "users",
                        MenuOrder = 1,
                        IsEnabled = true
                    }),
                    (404, new { message = "Módulo no encontrado" })
                );

            builder["siscorebackend.controllers.modulescontroller.createmodule"] = new EndpointExampleDefinition()
                .WithRequest(new CreateModuleRequest
                {
                    Code = "projects",
                    Name = "Proyectos",
                    Description = "Gestión de proyectos y actividades",
                    Icon = "clipboard-list",
                    MenuOrder = 3
                })
                .WithResponses(
                    (201, new
                    {
                        id = 21,
                        code = "projects",
                        name = "Proyectos",
                        description = "Gestión de proyectos y actividades"
                    }),
                    (400, new { message = "Code y Name son requeridos" })
                );

            builder["siscorebackend.controllers.modulescontroller.generatemodule"] = new EndpointExampleDefinition()
                .WithRequest(new GenerateModuleRequest
                {
                    Prompt = "Crear módulo para administrar inventario con acciones de alta, baja y ajustes.",
                    AiModel = "gpt-4o-mini"
                })
                .WithResponses(
                    (200, new
                    {
                        message = "Módulo generado exitosamente",
                        moduleId = 25
                    }),
                    (501, new { message = "En desarrollo" })
                );

            builder["siscorebackend.controllers.modulescontroller.updatemodule"] = new EndpointExampleDefinition()
                .WithRequest(new UpdateModuleRequest
                {
                    Name = "Gestión de Usuarios",
                    Description = "Administración de usuarios, roles y permisos",
                    Icon = "users",
                    MenuOrder = 1,
                    IsEnabled = true
                })
                .WithResponses(
                    (200, new
                    {
                        id = 4,
                        code = "users",
                        name = "Gestión de Usuarios",
                        description = "Administración de usuarios, roles y permisos"
                    })
                );

            builder["siscorebackend.controllers.modulescontroller.deletemodule"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new { message = "Módulo eliminado exitosamente" }),
                    (404, new { message = "Módulo no encontrado" })
                );

            builder["siscorebackend.controllers.modulescontroller.getmodulepermissions"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new ModulePermissionResponse
                    {
                        ModuleId = 4,
                        ModuleCode = "users",
                        ModuleName = "Usuarios",
                        Permissions = new List<PermissionDetail>
                        {
                            new PermissionDetail
                            {
                                ModulePrivilegeId = 7,
                                PermissionId = 1,
                                Code = "create",
                                Name = "Crear",
                                Description = "Crear usuarios",
                                HasPermission = true,
                                Source = "Role",
                                RoleId = 1,
                                RoleName = "SuperAdmin"
                            },
                            new PermissionDetail
                            {
                                ModulePrivilegeId = 17,
                                PermissionId = 2,
                                Code = "read",
                                Name = "Leer",
                                Description = "Consultar usuarios",
                                HasPermission = true,
                                Source = "Role",
                                RoleId = 1,
                                RoleName = "SuperAdmin"
                            }
                        }
                    })
                );
        }

        private static void RegisterPermissionsExamples(IDictionary<string, EndpointExampleDefinition> builder)
        {
            builder["siscorebackend.controllers.permissionscontroller.getmypermissions"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new PermissionResponse
                    {
                        UserId = 1,
                        Permissions = new List<ModulePermission>
                        {
                            new ModulePermission
                            {
                                Id = 4,
                                Code = "users",
                                Name = "Usuarios",
                                Description = "Gestión de usuarios y roles",
                                Privileges = new List<PermissionPrivilege>
                                {
                                    new PermissionPrivilege
                                    {
                                        ModulePrivilegeId = 7,
                                        PermissionId = 1,
                                        PermissionAssignmentId = 125,
                                        Code = "create",
                                        Name = "Crear",
                                        HasPermission = true
                                    },
                                    new PermissionPrivilege
                                    {
                                        ModulePrivilegeId = 17,
                                        PermissionId = 2,
                                        PermissionAssignmentId = 126,
                                        Code = "read",
                                        Name = "Leer",
                                        HasPermission = true
                                    }
                                }
                            }
                        }
                    })
                );

            builder["siscorebackend.controllers.permissionscontroller.getuserpermissions"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new UserPermissionResponse
                    {
                        UserId = 42,
                        UserName = "Jane Doe",
                        Permissions = new List<ModulePermission>
                        {
                            new ModulePermission
                            {
                                Id = 4,
                                Code = "users",
                                Name = "Usuarios",
                                Privileges = new List<PermissionPrivilege>
                                {
                                    new PermissionPrivilege
                                    {
                                        ModulePrivilegeId = 7,
                                        PermissionId = 1,
                                        PermissionAssignmentId = 225,
                                        Code = "create",
                                        Name = "Crear",
                                        HasPermission = true
                                    }
                                }
                            }
                        },
                        Roles = new List<RoleInfo>
                        {
                            new RoleInfo { Id = 2, Name = "Manager" }
                        }
                    })
                );

            builder["siscorebackend.controllers.permissionscontroller.checkpermission"] = new EndpointExampleDefinition()
                .WithRequest(new CheckPermissionRequest
                {
                    ModuleId = 4,
                    PermissionCode = "create",
                    UserId = 42
                })
                .WithResponses(
                    (200, new CheckPermissionResponse
                    {
                        HasPermission = true,
                        Source = "Role",
                        RoleId = 2,
                        RoleName = "Manager"
                    })
                );

            builder["siscorebackend.controllers.permissionscontroller.getpermissioncatalog"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    code = "user",
                    includeSystem = false
                })
                .WithResponses(
                    (200, new[]
                    {
                        new PermissionCatalogResponse
                        {
                            Id = 1,
                            Code = "create",
                            Name = "Crear",
                            Description = "Permite crear registros",
                            IsSystem = false,
                            IsDefaultForModule = true,
                            CreatedAt = DateTime.UtcNow.AddMonths(-6),
                            UpdatedAt = DateTime.UtcNow.AddDays(-2)
                        },
                        new PermissionCatalogResponse
                        {
                            Id = 2,
                            Code = "read",
                            Name = "Leer",
                            Description = "Permite leer registros",
                            IsSystem = false,
                            IsDefaultForModule = true,
                            CreatedAt = DateTime.UtcNow.AddMonths(-6),
                            UpdatedAt = DateTime.UtcNow.AddDays(-2)
                        }
                    })
                );

            builder["siscorebackend.controllers.permissionscontroller.getpermissioncatalogbyid"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new PermissionCatalogResponse
                    {
                        Id = 1,
                        Code = "create",
                        Name = "Crear",
                        Description = "Permite crear registros",
                        IsSystem = false,
                        IsDefaultForModule = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-6),
                        UpdatedAt = DateTime.UtcNow.AddDays(-2)
                    }),
                    (404, new { message = "Permiso no encontrado" })
                );

            builder["siscorebackend.controllers.permissionscontroller.createpermissioncatalog"] = new EndpointExampleDefinition()
                .WithRequest(new CreatePermissionRequest
                {
                    Code = "approve",
                    Name = "Aprobar",
                    Description = "Permite aprobar solicitudes",
                    IsSystem = false,
                    IsDefaultForModule = false
                })
                .WithResponses(
                    (201, new PermissionCatalogResponse
                    {
                        Id = 6,
                        Code = "approve",
                        Name = "Aprobar",
                        Description = "Permite aprobar solicitudes",
                        IsSystem = false,
                        IsDefaultForModule = false,
                        CreatedAt = DateTime.UtcNow
                    })
                );

            builder["siscorebackend.controllers.permissionscontroller.updatepermissioncatalog"] = new EndpointExampleDefinition()
                .WithRequest(new UpdatePermissionRequest
                {
                    Name = "Aprobar",
                    Description = "Permite aprobar y rechazar solicitudes",
                    IsDefaultForModule = true
                })
                .WithResponses(
                    (200, new PermissionCatalogResponse
                    {
                        Id = 6,
                        Code = "approve",
                        Name = "Aprobar",
                        Description = "Permite aprobar y rechazar solicitudes",
                        IsSystem = false,
                        IsDefaultForModule = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        UpdatedAt = DateTime.UtcNow
                    })
                );

            builder["siscorebackend.controllers.permissionscontroller.deletepermissioncatalog"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new { message = "Permiso eliminado exitosamente" }),
                    (404, new { message = "Permiso no encontrado" })
                );

            builder["siscorebackend.controllers.permissionscontroller.getmoduleprivileges"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    includeDeleted = false
                })
                .WithResponses(
                    (200, new[]
                    {
                        new ModulePrivilegeResponse
                        {
                            Id = 7,
                            ModuleId = 4,
                            ModuleCode = "users",
                            ModuleName = "Usuarios",
                            PermissionId = 1,
                            PermissionCode = "create",
                            PermissionName = "Crear",
                            Description = "Permite crear usuarios",
                            CreatedAt = DateTime.UtcNow.AddMonths(-6),
                            CreatedBy = 1,
                            IsDeleted = false,
                            IsDefault = true,
                            HasAssignments = true
                        },
                        new ModulePrivilegeResponse
                        {
                            Id = 17,
                            ModuleId = 4,
                            ModuleCode = "users",
                            ModuleName = "Usuarios",
                            PermissionId = 2,
                            PermissionCode = "read",
                            PermissionName = "Leer",
                            Description = "Permite consultar usuarios",
                            CreatedAt = DateTime.UtcNow.AddMonths(-6),
                            CreatedBy = 1,
                            IsDeleted = false,
                            IsDefault = true,
                            HasAssignments = true
                        }
                    })
                );

            builder["siscorebackend.controllers.permissionscontroller.getmoduleprivilege"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new ModulePrivilegeResponse
                    {
                        Id = 7,
                        ModuleId = 4,
                        ModuleCode = "users",
                        ModuleName = "Usuarios",
                        PermissionId = 1,
                        PermissionCode = "create",
                        PermissionName = "Crear",
                        Description = "Permite crear usuarios",
                        CreatedAt = DateTime.UtcNow.AddMonths(-6),
                        CreatedBy = 1,
                        UpdatedAt = DateTime.UtcNow.AddMonths(-1),
                        UpdatedBy = 2,
                        IsDeleted = false,
                        IsDefault = true,
                        HasAssignments = true
                    }),
                    (404, new { message = "Privilegio no encontrado" })
                );

            builder["siscorebackend.controllers.permissionscontroller.createmoduleprivilege"] = new EndpointExampleDefinition()
                .WithRequest(new CreateModulePrivilegeRequest
                {
                    PermissionId = 6,
                    SubModuleId = null,
                    NameOverride = "Aprobar usuario",
                    Description = "Permite aprobar usuarios",
                    IsDefault = false
                })
                .WithResponses(
                    (201, new ModulePrivilegeResponse
                    {
                        Id = 55,
                        ModuleId = 4,
                        ModuleCode = "users",
                        ModuleName = "Usuarios",
                        PermissionId = 6,
                        PermissionCode = "approve",
                        PermissionName = "Aprobar",
                        Description = "Permite aprobar usuarios",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = 1,
                        IsDeleted = false,
                        IsDefault = false,
                        HasAssignments = false
                    })
                );

            builder["siscorebackend.controllers.permissionscontroller.updatemoduleprivilege"] = new EndpointExampleDefinition()
                .WithRequest(new UpdateModulePrivilegeRequest
                {
                    NameOverride = "Aprobación de usuarios",
                    Description = "Permite aprobar o rechazar usuarios",
                    IsDefault = false
                })
                .WithResponses(
                    (200, new ModulePrivilegeResponse
                    {
                        Id = 55,
                        ModuleId = 4,
                        ModuleCode = "users",
                        ModuleName = "Usuarios",
                        PermissionId = 6,
                        PermissionCode = "approve",
                        PermissionName = "Aprobar",
                        Description = "Permite aprobar o rechazar usuarios",
                        CreatedAt = DateTime.UtcNow.AddHours(-1),
                        CreatedBy = 1,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = 1,
                        IsDeleted = false,
                        IsDefault = false,
                        HasAssignments = false
                    })
                );

            builder["siscorebackend.controllers.permissionscontroller.deletemoduleprivilege"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new { message = "Privilegio eliminado exitosamente" }),
                    (404, new { message = "Privilegio no encontrado" })
                );

            builder["siscorebackend.controllers.permissionscontroller.restoremoduleprivilege"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new { message = "Privilegio restaurado exitosamente" }),
                    (404, new { message = "Privilegio no encontrado" })
                );

            builder["siscorebackend.controllers.permissionscontroller.ensuredefaultprivileges"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new { message = "Privilegios por defecto sincronizados" })
                );

            builder["siscorebackend.controllers.permissionscontroller.getrolemodulepermissions"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new List<RoleModulePermissionsResponse>
                    {
                        new RoleModulePermissionsResponse
                        {
                            RoleId = 2,
                            ModuleId = 4,
                            ModuleCode = "users",
                            ModuleName = "Usuarios",
                            Privileges = new List<RoleModulePrivilegeStatus>
                            {
                                new RoleModulePrivilegeStatus
                                {
                                    ModulePrivilegeId = 7,
                                    PermissionId = 1,
                                    Code = "create",
                                    Name = "Crear",
                                    Description = "Crear usuarios",
                                    IsGranted = true
                                },
                                new RoleModulePrivilegeStatus
                                {
                                    ModulePrivilegeId = 17,
                                    PermissionId = 2,
                                    Code = "read",
                                    Name = "Leer",
                                    Description = "Leer usuarios",
                                    IsGranted = true
                                },
                                new RoleModulePrivilegeStatus
                                {
                                    ModulePrivilegeId = 27,
                                    PermissionId = 3,
                                    Code = "update",
                                    Name = "Actualizar",
                                    Description = "Actualizar usuarios",
                                    IsGranted = false
                                }
                            }
                        }
                    })
                );

            builder["siscorebackend.controllers.permissionscontroller.updaterolemodulepermissions"] = new EndpointExampleDefinition()
                .WithRequest(new UpdateRoleModulePermissionsRequest
                {
                    Modules = new List<RoleModulePermissionsUpdate>
                    {
                        new RoleModulePermissionsUpdate
                        {
                            ModuleId = 4,
                            Privileges = new List<RoleModulePrivilegeToggle>
                            {
                                new RoleModulePrivilegeToggle
                                {
                                    ModulePrivilegeId = 7,
                                    IsGranted = true
                                },
                                new RoleModulePrivilegeToggle
                                {
                                    ModulePrivilegeId = 27,
                                    IsGranted = true
                                }
                            }
                        }
                    }
                })
                .WithResponses(
                    (200, new { message = "Permisos actualizados correctamente" })
                );
        }

        private static void RegisterDiagnosticsExamples(IDictionary<string, EndpointExampleDefinition> builder)
        {
            builder["siscorebackend.controllers.diagnosticscontroller.gettenantdiagnostics"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new
                    {
                        headerName = "X-Tenant",
                        tenantHeaderValue = "siscore",
                        tenantQueryParam = (string?)null,
                        host = "siscore.localhost",
                        defaultDomain = "localhost",
                        tenantContext = new
                        {
                            companyId = 1,
                            subdomain = "siscore",
                            hasConnectionString = true,
                            connectionStringPreview = "Server=127.0.0.1;Database=timecontrol_siscore;User..."
                        },
                        companies = new[]
                        {
                            new
                            {
                                id = 1,
                                name = "SisCore",
                                subdomain = "siscore",
                                dbName = "timecontrol_siscore",
                                dbHost = "127.0.0.1",
                                dbPort = 3306,
                                connectionOptions = "?sslmode=none",
                                status = 1
                            }
                        },
                        message = "Tenant resuelto correctamente"
                    })
                );

            builder["siscorebackend.controllers.diagnosticscontroller.checkmasterdb"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new
                    {
                        connected = true,
                        companiesCount = 2,
                        companies = new[]
                        {
                            new { id = 1, name = "SisCore", subdomain = "siscore", dbName = "timecontrol_siscore", status = 1 },
                            new { id = 2, name = "Acme Labs", subdomain = "acme", dbName = "timecontrol_acme", status = 1 }
                        }
                    })
                );

            builder["siscorebackend.controllers.diagnosticscontroller.generatepasswordhash"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    password = "P@ssw0rd!"
                })
                .WithResponses(
                    (200, new
                    {
                        password = "P@ssw0rd!",
                        hash = "$argon2id$v=19$m=65536,t=3,p=2$...",
                        hashLength = 128,
                        instructions = "Copia el hash y actualiza el usuario en la BD..."
                    })
                );
        }

        private static void RegisterFormBuilderExamples(IDictionary<string, EndpointExampleDefinition> builder)
        {
            builder["siscorebackend.controllers.formbuildercontroller.generateform"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    prompt = "Generar formulario para registrar incidencias con campos de prioridad, categoría y descripción detallada.",
                    aiModel = "gpt-4o",
                    versionNotes = "Primer borrador generado por IA",
                    publish = false
                })
                .WithResponses(
                    (200, new
                    {
                        message = "En desarrollo",
                        formTemplateId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                        status = "Draft"
                    })
                );

            builder["siscorebackend.controllers.formbuildercontroller.getforms"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new
                    {
                        message = "En desarrollo",
                        forms = Array.Empty<object>()
                    })
                );

            builder["siscorebackend.controllers.formbuildercontroller.getform"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new
                    {
                        message = "En desarrollo",
                        formId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")
                    })
                );

            builder["siscorebackend.controllers.formbuildercontroller.createform"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    code = "incident_report",
                    name = "Reporte de Incidencias",
                    description = "Formulario para reportar incidencias internas",
                    schemaJson = "{ \"type\": \"object\", \"properties\": { \"title\": { \"type\": \"string\" } } }",
                    uiSchemaJson = "{ \"title\": { \"ui:placeholder\": \"Título de la incidencia\" } }"
                })
                .WithResponses(
                    (201, new
                    {
                        message = "En desarrollo"
                    })
                );

            builder["siscorebackend.controllers.formbuildercontroller.updateform"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    name = "Reporte de Incidencias",
                    description = "Formulario actualizado para incidencias internas",
                    schemaJson = "{ \"type\": \"object\", \"properties\": { \"title\": { \"type\": \"string\" } } }"
                })
                .WithResponses(
                    (200, new
                    {
                        message = "En desarrollo"
                    })
                );

            builder["siscorebackend.controllers.formbuildercontroller.deleteform"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new { message = "Formulario eliminado exitosamente" })
                );

            builder["siscorebackend.controllers.formbuildercontroller.publishform"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    versionId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                    publicationType = "RoleBased",
                    targetRoleIds = new[] { Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff") },
                    startDate = DateTime.Parse("2025-01-20T12:00:00Z"),
                    endDate = DateTime.Parse("2025-02-20T12:00:00Z")
                })
                .WithResponses(
                    (200, new
                    {
                        message = "Formulario publicado exitosamente",
                        publicationId = Guid.Parse("99999999-8888-7777-6666-555555555555")
                    })
                );

            builder["siscorebackend.controllers.formbuildercontroller.getformversions"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new
                    {
                        message = "En desarrollo",
                        versions = Array.Empty<object>()
                    })
                );

            builder["siscorebackend.controllers.formbuildercontroller.createformversion"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    versionNumber = "v1.1",
                    schemaJson = "{ \"type\": \"object\", \"properties\": { \"title\": { \"type\": \"string\" } } }",
                    changeNotes = "Se agregó campo de prioridad"
                })
                .WithResponses(
                    (201, new
                    {
                        message = "En desarrollo",
                        versionId = Guid.Parse("22222222-3333-4444-5555-666666666666")
                    })
                );

            builder["siscorebackend.controllers.formbuildercontroller.rollbackformversion"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new { message = "Rollback realizado exitosamente" })
                );
        }

        private static void RegisterMasterCompaniesExamples(IDictionary<string, EndpointExampleDefinition> builder)
        {
            builder["siscorebackend.controllers.master.companiescontroller.getcompanies"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new[]
                    {
                        new
                        {
                            id = 1,
                            name = "SisCore",
                            subdomain = "siscore",
                            dbHost = "127.0.0.1",
                            dbPort = 3306,
                            dbName = "timecontrol_siscore",
                            dbUser = "siscore_user",
                            hasPassword = true,
                            connectionOptions = "?sslmode=none",
                            status = 1,
                            createdAt = DateTime.Parse("2024-06-10T12:00:00Z")
                        }
                    })
                );

            builder["siscorebackend.controllers.master.companiescontroller.getcompany"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new
                    {
                        id = 1,
                        name = "SisCore",
                        subdomain = "siscore",
                        dbHost = "127.0.0.1",
                        dbPort = 3306,
                        dbName = "timecontrol_siscore",
                        dbUser = "siscore_user",
                        hasPassword = true,
                        connectionOptions = "?sslmode=none",
                        brandingJson = "{ \"primaryColor\": \"#0F62FE\" }",
                        settingsJson = "{ \"timezone\": \"America/Asuncion\" }",
                        status = 1,
                        createdAt = DateTime.Parse("2024-06-10T12:00:00Z"),
                        updatedAt = DateTime.Parse("2024-11-10T09:30:00Z")
                    }),
                    (404, new { message = "Empresa no encontrada" })
                );

            builder["siscorebackend.controllers.master.companiescontroller.createcompany"] = new EndpointExampleDefinition()
                .WithRequest(new CreateCompanyRequest
                {
                    Name = "Acme Labs",
                    Subdomain = "acme",
                    DbHost = "db.prod.internal",
                    DbPort = 3306,
                    DbName = "timecontrol_acme",
                    DbUser = "acme_admin",
                    DbPassword = "S3cret!",
                    ConnectionOptions = "SslMode=Required",
                    BrandingJson = "{ \"primaryColor\": \"#ff6f00\" }",
                    SettingsJson = "{ \"timezone\": \"America/New_York\" }"
                })
                .WithResponses(
                    (201, new
                    {
                        id = 2,
                        name = "Acme Labs",
                        subdomain = "acme",
                        dbHost = "db.prod.internal",
                        dbPort = 3306,
                        dbName = "timecontrol_acme"
                    })
                );

            builder["siscorebackend.controllers.master.companiescontroller.updatecompany"] = new EndpointExampleDefinition()
                .WithRequest(new UpdateCompanyRequest
                {
                    Name = "SisCore Cloud",
                    DbHost = "db.cluster.local",
                    DbPort = 3307,
                    DbName = "timecontrol_siscore",
                    DbUser = "siscore_admin",
                    ConnectionOptions = "SslMode=Preferred",
                    BrandingJson = "{ \"primaryColor\": \"#00539f\" }",
                    Status = 1
                })
                .WithResponses(
                    (200, new
                    {
                        id = 1,
                        name = "SisCore Cloud",
                        subdomain = "siscore",
                        dbHost = "db.cluster.local",
                        dbPort = 3307,
                        dbName = "timecontrol_siscore"
                    })
                );

            builder["siscorebackend.controllers.master.companiescontroller.updateconnection"] = new EndpointExampleDefinition()
                .WithRequest(new
                {
                    dbHost = "db.cluster.local",
                    dbPort = 3307,
                    dbName = "timecontrol_siscore",
                    dbUser = "siscore_admin",
                    dbPassword = "N3wS3cret!",
                    connectionOptions = "SslMode=Preferred"
                })
                .WithResponses(
                    (200, new
                    {
                        message = "Configuración de conexión actualizada",
                        dbHost = "db.cluster.local",
                        dbPort = 3307,
                        dbName = "timecontrol_siscore"
                    })
                );
        }

        private static void RegisterMasterUsersExamples(IDictionary<string, EndpointExampleDefinition> builder)
        {
            builder["siscorebackend.controllers.master.masteruserscontroller.registermasteruser"] = new EndpointExampleDefinition()
                .WithRequest(new RegisterMasterUserRequest
                {
                    TenantUserId = 12,
                    TenantSubdomain = "siscore",
                    IsGod = false
                })
                .WithResponses(
                    (201, new MasterUserResponse
                    {
                        Id = 5,
                        Email = "jane.doe@acme.com",
                        FullName = "Jane Doe",
                        PhoneNumber = "+595971000000",
                        TenantUserId = 12,
                        TenantCompanyId = 1,
                        TenantCompanyName = "SisCore",
                        TenantSubdomain = "siscore",
                        IsGod = false,
                        Status = 1,
                        CreatedAt = DateTime.Parse("2025-01-10T09:30:00Z"),
                        Companies = new List<MasterUserCompanyResponse>
                        {
                            new MasterUserCompanyResponse
                            {
                                Id = 10,
                                CompanyId = 1,
                                CompanyName = "SisCore",
                                Subdomain = "siscore",
                                Role = "admin",
                                GrantedAt = DateTime.Parse("2025-01-10T09:30:00Z")
                            }
                        }
                    }),
                    (403, new { message = "Solo usuarios con rol God pueden asignar el rol God a otros usuarios." })
                );

            builder["siscorebackend.controllers.master.masteruserscontroller.getmasteruser"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new MasterUserResponse
                    {
                        Id = 5,
                        Email = "jane.doe@acme.com",
                        FullName = "Jane Doe",
                        PhoneNumber = "+595971000000",
                        TenantUserId = 12,
                        TenantCompanyId = 1,
                        TenantCompanyName = "SisCore",
                        TenantSubdomain = "siscore",
                        IsGod = false,
                        Status = 1,
                        LastLoginAt = DateTime.Parse("2025-01-18T16:45:00Z"),
                        CreatedAt = DateTime.Parse("2025-01-10T09:30:00Z"),
                        Companies = new List<MasterUserCompanyResponse>
                        {
                            new MasterUserCompanyResponse
                            {
                                Id = 10,
                                CompanyId = 1,
                                CompanyName = "SisCore",
                                Subdomain = "siscore",
                                Role = "admin",
                                GrantedAt = DateTime.Parse("2025-01-10T09:30:00Z")
                            }
                        }
                    }),
                    (404, new { message = "Usuario maestro no encontrado" })
                );

            builder["siscorebackend.controllers.master.masteruserscontroller.getmasterusers"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new[]
                    {
                        new MasterUserResponse
                        {
                            Id = 1,
                            Email = "god@timecontrol.com",
                            FullName = "God Admin",
                            TenantUserId = 1,
                            TenantCompanyId = 1,
                            TenantCompanyName = "SisCore",
                            TenantSubdomain = "siscore",
                            IsGod = true,
                            Status = 1,
                            CreatedAt = DateTime.Parse("2024-05-01T10:00:00Z")
                        },
                        new MasterUserResponse
                        {
                            Id = 5,
                            Email = "jane.doe@acme.com",
                            FullName = "Jane Doe",
                            TenantUserId = 12,
                            TenantCompanyId = 1,
                            TenantCompanyName = "SisCore",
                            TenantSubdomain = "siscore",
                            IsGod = false,
                            Status = 1,
                            CreatedAt = DateTime.Parse("2025-01-10T09:30:00Z")
                        }
                    })
                );

            builder["siscorebackend.controllers.master.masteruserscontroller.assigncompanytomasteruser"] = new EndpointExampleDefinition()
                .WithRequest(new AssignCompanyToMasterUserRequest
                {
                    MasterUserId = 5,
                    CompanyId = 2,
                    Role = "admin"
                })
                .WithResponses(
                    (200, new MasterUserResponse
                    {
                        Id = 5,
                        Email = "jane.doe@acme.com",
                        FullName = "Jane Doe",
                        TenantUserId = 12,
                        TenantCompanyId = 1,
                        TenantCompanyName = "SisCore",
                        TenantSubdomain = "siscore",
                        IsGod = false,
                        Status = 1,
                        CreatedAt = DateTime.Parse("2025-01-10T09:30:00Z"),
                        Companies = new List<MasterUserCompanyResponse>
                        {
                            new MasterUserCompanyResponse
                            {
                                Id = 10,
                                CompanyId = 1,
                                CompanyName = "SisCore",
                                Subdomain = "siscore",
                                Role = "admin",
                                GrantedAt = DateTime.Parse("2025-01-10T09:30:00Z")
                            },
                            new MasterUserCompanyResponse
                            {
                                Id = 21,
                                CompanyId = 2,
                                CompanyName = "Acme Labs",
                                Subdomain = "acme",
                                Role = "admin",
                                GrantedAt = DateTime.Parse("2025-01-18T10:15:00Z")
                            }
                        }
                    }),
                    (403, new { message = "Solo usuarios con rol God pueden asignar empresas." })
                );

            builder["siscorebackend.controllers.master.masteruserscontroller.revokecompanyfrommasteruser"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new { message = "Acceso revocado exitosamente" }),
                    (404, new { message = "No se encontró la relación entre el usuario maestro y la empresa" })
                );

            builder["siscorebackend.controllers.master.masteruserscontroller.checkifuserisgod"] = new EndpointExampleDefinition()
                .WithResponses(
                    (200, new { isGod = true })
                );
        }
    }
}


