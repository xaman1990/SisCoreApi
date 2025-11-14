# Inventario de Endpoints Activos

Resumen de rutas expuestas en la API y los contratos de entrada/salida para alimentar los ejemplos de Swagger.

## AuthController (`/api/auth`)

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| POST | `/login` | `LoginRequest` | `LoginResponse` (200), `{ message }` en 400/401 |
| POST | `/refresh` | `RefreshTokenRequest` | `LoginResponse` (200), `{ message }` en 401 |
| POST | `/logout` | `RefreshTokenRequest` | `{ message }` |
| GET | `/me` | — | `{ id, email, fullName, roles[] }` |
| GET | `/validate` | — | `{ valid, userId, email, roles[] }` |

## UsersController (`/api/users`)

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| POST | `/` | `RegisterUserRequest` | `{ id, email, phoneNumber, fullName, employeeNumber }` |
| GET | `/` | — | `List<{ id, email, phoneNumber, fullName, employeeNumber, status, roles[] }>` |
| GET | `/{id}` | — | `{ id, email, phoneNumber, fullName, employeeNumber, status, roles[] }` |
| PUT | `/{id}` | `UpdateUserRequest` | `{ id, email, phoneNumber, fullName, employeeNumber }` |
| DELETE | `/{id}` | — | `{ message }` |
| POST | `/{id}/roles` | `AssignRolesRequest` | `{ message }` |

## RolesController (`/api/roles`)

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| GET | `/` | — | `List<RoleResponse>` |
| GET | `/{id}` | — | `RoleResponse` |
| POST | `/` | `CreateRoleRequest` | `{ id, name, description }` |
| PUT | `/{id}` | `UpdateRoleRequest` | `{ id, name, description }` |
| DELETE | `/{id}` | — | `{ message }` |

## ModulesController (`/api/modules`)

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| GET | `/` | — | `List<ModuleResponse>` |
| GET | `/{id}` | — | `ModuleResponse` |
| POST | `/` | `CreateModuleRequest` | `{ id, code, name, description }` |
| POST | `/generate` | `GenerateModuleRequest` | `{ message, moduleId }` |
| PUT | `/{id}` | `UpdateModuleRequest` | `{ id, code, name, description }` |
| DELETE | `/{id}` | — | `{ message }` |
| GET | `/{id}/permissions` | `includeInherited`, `userId` (query) | `ModulePermissionResponse` |

## PermissionsController (`/api/permissions`)

### Permisos efectivos

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| GET | `/me` | — | `PermissionResponse` |
| GET | `/user/{userId}` | — | `UserPermissionResponse` |
| POST | `/check` | `CheckPermissionRequest` | `CheckPermissionResponse` |

### Catálogo de permisos

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| GET | `/catalog` | `PermissionCatalogFilter` (query) | `List<PermissionCatalogResponse>` |
| GET | `/catalog/{id}` | — | `PermissionCatalogResponse` |
| POST | `/catalog` | `CreatePermissionRequest` | `PermissionCatalogResponse` |
| PUT | `/catalog/{id}` | `UpdatePermissionRequest` | `PermissionCatalogResponse` |
| DELETE | `/catalog/{id}` | — | `{ message }` |

### Privilegios por módulo

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| GET | `/modules/{moduleId}/privileges` | `ModulePrivilegeFilter` (query) | `List<ModulePrivilegeResponse>` |
| GET | `/modules/{moduleId}/privileges/{privilegeId}` | — | `ModulePrivilegeResponse` |
| POST | `/modules/{moduleId}/privileges` | `CreateModulePrivilegeRequest` | `ModulePrivilegeResponse` |
| PUT | `/modules/{moduleId}/privileges/{privilegeId}` | `UpdateModulePrivilegeRequest` | `ModulePrivilegeResponse` |
| DELETE | `/modules/{moduleId}/privileges/{privilegeId}` | — | `{ message }` |
| POST | `/modules/{moduleId}/privileges/{privilegeId}/restore` | — | `{ message }` |
| POST | `/modules/{moduleId}/privileges/defaults` | — | `{ message }` |

### Permisos por rol

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| GET | `/roles/{roleId}/modules` | `moduleId` (query) | `List<RoleModulePermissionsResponse>` |
| PUT | `/roles/{roleId}/modules` | `UpdateRoleModulePermissionsRequest` | `{ message }` |

## DiagnosticsController (`/api/diagnostics`)

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| GET | `/tenant` | headers/query (`X-Tenant` / `tenant`) | `{ headerName, tenantHeaderValue, tenantContext, companies[], message }` |
| GET | `/master-db` | — | `{ connected, companiesCount, companies[] }` |
| POST | `/generate-password-hash` | `GeneratePasswordHashRequest` | `{ password, hash, hashLength, instructions }` |

## FormBuilderController (`/api/formbuilder`)

> Actualmente endpoints retornan respuestas de placeholder “En desarrollo”.

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| POST | `/generate` | `GenerateFormRequest` | `{ message, formTemplateId, status }` |
| GET | `/` | `status` (query) | `{ message, forms[] }` |
| GET | `/{id}` | — | `{ message, formId }` |
| POST | `/` | `CreateFormRequest` | `{ message }` (201) |
| PUT | `/{id}` | `UpdateFormRequest` | `{ message }` |
| DELETE | `/{id}` | — | `{ message }` |
| POST | `/{id}/publish` | `PublishFormRequest` | `{ message, publicationId }` |
| GET | `/{id}/versions` | — | `{ message, versions[] }` |
| POST | `/{id}/versions` | `CreateFormVersionRequest` | `{ message, versionId }` (201) |
| POST | `/{id}/rollback/{versionId}` | — | `{ message }` |

## Master Companies (`/api/master/companies`)

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| GET | `/` | — | `List<{ id, name, subdomain, dbHost, dbPort, dbName, dbUser, hasPassword, status }>` |
| GET | `/{id}` | — | `{ id, name, subdomain, dbHost, dbPort, dbName, dbUser, hasPassword, brandingJson, settingsJson, status }` |
| POST | `/` | `CreateCompanyRequest` | `{ id, name, subdomain, dbHost, dbPort, dbName }` |
| PUT | `/{id}` | `UpdateCompanyRequest` | `{ id, name, subdomain, dbHost, dbPort, dbName }` |
| PUT | `/{id}/connection` | `UpdateConnectionRequest` | `{ message, dbHost, dbPort, dbName }` |

## Master Users (`/api/master/masterusers`)

| Método | Ruta | Request DTO | Response principal |
|--------|------|-------------|--------------------|
| POST | `/register` | `RegisterMasterUserRequest` | `MasterUserResponse` |
| GET | `/{id}` | — | `MasterUserResponse` |
| GET | `/` | `isGod` (query) | `List<MasterUserResponse>` |
| POST | `/assign-company` | `AssignCompanyToMasterUserRequest` | `MasterUserResponse` |
| DELETE | `/{masterUserId}/companies/{companyId}` | — | `{ message }` |
| GET | `/check-god` | — | `{ isGod }` |


