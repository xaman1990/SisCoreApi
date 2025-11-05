# Arquitectura del Sistema TimeControl

## Resumen

TimeControl es un sistema multi-tenant para gestión de control de tiempo (timesheets) diseñado para soportar múltiples empresas, cada una con su propia base de datos y configuración.

## Arquitectura Multi-Tenant

### Patrón de Tenancy

El sistema utiliza el patrón **"Una BD por Tenant"**:
- **BD Maestra** (`timecontrol_master`): Contiene información de todas las empresas y usuarios maestros
- **BD por Tenant**: Cada empresa tiene su propia base de datos con todas sus tablas

### Resolución de Tenant

El sistema resuelve el tenant mediante:
1. **Header HTTP**: `X-Tenant` con el subdominio
2. **Subdominio**: Ejemplo `acme.localhost` → tenant "acme"
3. **Middleware**: `TenantMiddleware` intercepta las peticiones y resuelve el tenant

## Estructura de Bases de Datos

### Base de Datos Maestra (`timecontrol_master`)

**Tablas principales:**
- `Companies`: Información de todas las empresas
- `MasterUsers`: Usuarios maestros (super usuarios)
- `MasterUserSessions`: Sesiones de usuarios maestros
- `MasterUserCompanies`: Relación entre usuarios maestros y empresas
- `AuditLogs`: Logs de auditoría a nivel maestro

**Script:** `Data/Scripts/01_master_database.sql`

### Plantilla de Base de Datos por Tenant

**Tablas principales:**
- `Users`: Usuarios de la empresa
- `Roles` / `UserRoles`: Sistema de roles y permisos
- `Modules` / `Permissions` / `RolePermissions`: Sistema de permisos granular (ACL)
- `Projects` / `Activities`: Proyectos y actividades
- `Timesheets`: Registro de horas trabajadas
- `CompanySettings`: Configuraciones parametrizables
- `RefreshTokens`: Tokens de refresh para JWT
- `Notifications`: Inbox de notificaciones
- `Tickets`: Sistema de tickets de soporte
- `AuditLogs` / `SecurityLogs`: Logs de auditoría y seguridad

**Script:** `Data/Scripts/02_tenant_template.sql`

## Sistema de Autenticación

### Usuarios Maestros (Super Usuarios)

- Almacenados en la BD maestra
- Pueden crear empresas
- Pueden acceder a múltiples empresas
- Roles: `owner`, `admin`, `viewer`

### Usuarios de Tenant

- Almacenados en la BD del tenant
- Múltiples métodos de autenticación:
  - Email + Password
  - Teléfono + OTP
  - Google OAuth
- Sistema de roles configurables
- MFA (Multi-Factor Authentication) opcional

### JWT Tokens

- **Access Token**: Vida corta (20 minutos por defecto)
- **Refresh Token**: Vida larga (14 días por defecto)
- **Rotación de tokens**: Los refresh tokens se rotan automáticamente
- **Device binding**: Los tokens están vinculados a dispositivos

## Sistema de Permisos (ACL)

### Arquitectura Modular

1. **Modules**: Módulos del sistema (timesheet, reports, settings, etc.)
2. **Permissions**: Permisos por módulo (create, read, update, delete, approve, etc.)
3. **Roles**: Roles de usuario (SuperAdmin, Admin, Supervisor, Employee, etc.)
4. **RolePermissions**: Asignación de permisos a roles
5. **UserRoles**: Asignación de roles a usuarios

### Configurabilidad

- Los módulos se pueden habilitar/deshabilitar por empresa
- Los permisos se pueden configurar por rol
- Los roles se pueden crear personalizados
- Todo es configurable desde el panel de administración

## Modelos de Dominio

### Master Domain

- `Company`: Empresa
- `MasterUser`: Usuario maestro
- `MasterUserSession`: Sesión de usuario maestro
- `MasterUserCompany`: Relación usuario-empresa

### Tenant Domain

- `User`: Usuario del tenant
- `Role` / `UserRole`: Roles
- `Module` / `Permission` / `RolePermission`: Permisos
- `Project` / `Activity` / `ProjectUser`: Proyectos
- `Timesheet`: Registro de horas
- `CompanySettings`: Configuraciones
- `RefreshToken`: Tokens de refresh
- `Notification`: Notificaciones
- `Ticket`: Tickets de soporte

## Características Principales

### Gestión de Timesheets

- Registro de horas por usuario, proyecto y actividad
- Geolocalización automática (GPS)
- Validaciones configurables:
  - Ventana de registro (días hábiles)
  - Horas por día (configurable por proyecto)
  - Fracción mínima de horas (0.25h)
- Estados: Pendiente, Aprobado, Rechazado
- Aprobación por supervisores

### Configuraciones Parametrizables

Todas las reglas de negocio son configurables por empresa:
- Días máximos para registrar
- Días máximos para cerrar período
- Días para bloquear registro
- Horas por defecto por día
- Fracción mínima de horas
- Timeouts de sesión
- Políticas de contraseña
- Notificaciones de omisos

### Notificaciones

- Inbox en la aplicación
- Correo electrónico
- WhatsApp (stub para implementación)
- Notificaciones push (preparado para Firebase)

### Seguridad

- Encriptación de contraseñas (Argon2id)
- MFA (TOTP)
- Device binding
- Rotación de tokens
- Logs de seguridad y auditoría
- Timeouts de sesión configurables
- Políticas de contraseña configurables

## Próximos Pasos

### 1. Servicios de Dominio

Crear servicios para:
- `AuthService`: Autenticación y autorización
- `UserService`: Gestión de usuarios
- `TimesheetService`: Lógica de negocio de timesheets
- `PermissionService`: Verificación de permisos
- `CompanyService`: Gestión de empresas
- `NotificationService`: Envío de notificaciones

### 2. Repositorios

Implementar patrones Repository para:
- Acceso a datos abstracto
- Facilidad de testing
- Separación de responsabilidades

### 3. Controladores

Crear endpoints para:
- `/api/auth/*`: Autenticación
- `/api/users/*`: Gestión de usuarios
- `/api/timesheets/*`: Timesheets
- `/api/projects/*`: Proyectos
- `/api/settings/*`: Configuraciones
- `/api/master/*`: Endpoints maestros (solo super usuarios)

### 4. Validaciones de Negocio

- Validar ventanas de registro
- Validar horas máximas por día
- Validar permisos antes de operaciones
- Validar estados de proyectos y actividades

### 5. Integraciones

- Google OAuth
- Servicio de SMS/OTP (Twilio, AWS SNS, etc.)
- WhatsApp Business API
- Firebase Cloud Messaging (notificaciones push)
- Servicios de geocodificación (reverse geocoding)

### 6. Testing

- Unit tests (xUnit)
- Integration tests
- Tests de seguridad

### 7. Documentación

- OpenAPI/Swagger completo
- Documentación de endpoints
- Guías de integración

## Configuración Inicial

### 1. Crear Base de Datos Maestra

```bash
mysql -u root -p < TimeControlApi/Data/Scripts/01_master_database.sql
```

### 2. Configurar appsettings.json

Actualizar la cadena de conexión de la BD maestra:

```json
{
  "ConnectionStrings": {
    "MasterDatabase": "Server=localhost;Database=timecontrol_master;User=root;Password=TU_PASSWORD;SslMode=None"
  }
}
```

### 3. Crear Primera Empresa

Ejecutar el script de plantilla para crear la BD del tenant:

```bash
# Reemplazar {TENANT_DB_NAME} con el nombre de la BD
mysql -u root -p < TimeControlApi/Data/Scripts/02_tenant_template.sql
```

### 4. Registrar Empresa en BD Maestra

Insertar el registro en la tabla `Companies` con la información de la BD creada.

## Consideraciones de Seguridad

1. **Contraseñas**: Cambiar el hash del usuario maestro inicial
2. **JWT Key**: Generar una clave secreta segura (32+ caracteres)
3. **Connection Strings**: Usar AWS Secrets Manager en producción
4. **HTTPS**: Obligatorio en producción
5. **CORS**: Configurar solo los orígenes permitidos
6. **Rate Limiting**: Implementar límites de tasa
7. **SQL Injection**: EF Core previene automáticamente
8. **XSS**: Validar y sanitizar inputs

## Deployment

### AWS Elastic Beanstalk

- Configurar variables de entorno
- Usar AWS RDS para MySQL
- Configurar Secrets Manager para credenciales
- Configurar CloudWatch para logs

### CI/CD con GitHub Actions

- Build automático
- Tests automáticos
- Deployment a QA y Producción

