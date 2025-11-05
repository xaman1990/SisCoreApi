# Resumen: Sistema de Usuarios Maestros (Multi-Empresa)

## Objetivo

Integrar un sistema de usuarios maestros que permite que usuarios de empresas individuales (tenants) puedan acceder a múltiples empresas y tener privilegios especiales, incluyendo el rol "God" con todos los privilegios del sistema.

## Cambios Realizados

### 1. Modificación de Entidades

#### MasterUser (Modificado)
- **Eliminado**: `PasswordHash`, `PhoneVerified`, `EmailVerified`, `MfaSecret`, `MfaEnabled`
  - **Razón**: Estos datos ya están en la BD tenant, no necesitamos duplicarlos
- **Agregado**: 
  - `TenantUserId`: ID del usuario en la BD tenant
  - `TenantCompanyId`: ID de la empresa (Companies.Id)
  - `IsGod`: Boolean para indicar si tiene rol God
  - `CreatedBy`: ID del usuario que creó este registro maestro

#### MasterUserCompany (Sin cambios estructurales)
- El campo `Role` ahora acepta: `god`, `owner`, `admin`, `viewer`
- Solo usuarios God pueden asignar el rol `god`

### 2. Servicios Creados

#### IMasterUserService / MasterUserService
- `RegisterMasterUserAsync`: Registra un usuario tenant como usuario maestro
- `GetMasterUserByTenantUserAsync`: Obtiene usuario maestro por TenantUserId y TenantCompanyId
- `GetMasterUserByEmailAsync`: Obtiene usuario maestro por email
- `IsUserGodAsync`: Verifica si un usuario es God
- `IsUserGodByEmailAsync`: Verifica si un usuario es God por email
- `GetMasterUsersAsync`: Lista usuarios maestros (con filtro opcional por IsGod)
- `AssignCompanyToMasterUserAsync`: Asigna empresa a usuario maestro
- `RevokeCompanyFromMasterUserAsync`: Revoca acceso a empresa

### 3. Controladores Creados

#### MasterUsersController
- `POST /api/master/masterusers/register`: Registrar usuario maestro
- `GET /api/master/masterusers`: Listar usuarios maestros
- `GET /api/master/masterusers/{id}`: Obtener usuario maestro por ID
- `POST /api/master/masterusers/assign-company`: Asignar empresa a usuario maestro
- `DELETE /api/master/masterusers/{masterUserId}/companies/{companyId}`: Revocar acceso
- `GET /api/master/masterusers/check-god`: Verificar si usuario actual es God

### 4. Modificaciones en Servicios Existentes

#### RoleService
- **Modificado**: `GetRolesAsync()` ahora oculta el rol "God" si el usuario no es God
- Solo usuarios con `IsGod = true` pueden ver y asignar el rol "God"

### 5. DTOs Creados

- `RegisterMasterUserRequest`: Para registrar usuarios maestros
- `MasterUserResponse`: Respuesta con información del usuario maestro
- `MasterUserCompanyResponse`: Información de empresas asignadas
- `AssignCompanyToMasterUserRequest`: Para asignar empresas

## Reglas de Negocio

### Rol "God"

1. **Asignación**: Solo usuarios God pueden asignar el rol God a otros usuarios
2. **Visibilidad**: Solo usuarios God pueden ver el rol "God" en la lista de roles
3. **Privilegios**: Usuarios God tienen todos los privilegios del sistema
4. **Creación**: El rol God solo se puede asignar durante el registro del usuario maestro (`RegisterMasterUserAsync` con `IsGod = true`)

### Usuarios Maestros

1. **Registro**: Un usuario debe existir primero en una BD tenant antes de ser registrado como usuario maestro
2. **Validación**: El servicio valida que el usuario existe en la BD tenant antes de crear el registro maestro
3. **Referencias**: No duplicamos datos (contraseñas, etc.), solo referencias
4. **Empresas**: Un usuario maestro puede tener acceso a múltiples empresas a través de `MasterUserCompanies`

## Script SQL de Migración

El script `12_migrate_master_users.sql` realiza:

1. Elimina columnas obsoletas de `MasterUsers`
2. Agrega nuevas columnas (`TenantUserId`, `TenantCompanyId`, `IsGod`, `CreatedBy`)
3. Crea índices necesarios
4. Actualiza foreign keys
5. Agrega constraint para valores válidos de `Role` en `MasterUserCompanies`

## Uso

### Registrar un Usuario Maestro

```bash
POST /api/master/masterusers/register
Authorization: Bearer {token}
Content-Type: application/json

{
  "tenantUserId": 1,
  "tenantSubdomain": "siscore",
  "isGod": false  // Solo usuarios God pueden poner true
}
```

### Verificar si Usuario es God

```bash
GET /api/master/masterusers/check-god
Authorization: Bearer {token}
```

### Asignar Empresa a Usuario Maestro

```bash
POST /api/master/masterusers/assign-company
Authorization: Bearer {token}  // Debe ser God
Content-Type: application/json

{
  "masterUserId": 1,
  "companyId": 2,
  "role": "admin"  // god, owner, admin, viewer
}
```

## Notas Importantes

1. **Seguridad**: Solo usuarios God pueden asignar el rol God
2. **Datos Duplicados**: No duplicamos contraseñas ni datos sensibles, solo referencias
3. **Validación**: El sistema valida que el usuario existe en la BD tenant antes de crear el registro maestro
4. **Migración**: Ejecutar el script SQL antes de usar el sistema

## Próximos Pasos

1. Ejecutar el script SQL de migración (`12_migrate_master_users.sql`)
2. Crear el primer usuario God manualmente (si es necesario)
3. Probar los endpoints con Postman
4. Integrar la verificación de rol God en otros servicios según sea necesario

