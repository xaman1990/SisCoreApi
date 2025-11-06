# Documentación de Flujos del Sistema TimeControl

Este documento describe los flujos completos del sistema TimeControl usando diagramas C4 y Mermaid.

---

## Diagrama C4 - Contexto del Sistema

### Nivel 1: Contexto

```mermaid
C4Context
    title Diagrama de Contexto - TimeControl API
    
    Person(user, "Usuario", "Usuario final de la aplicación")
    Person(admin, "Administrador", "Administrador del tenant")
    Person(masterAdmin, "Master Admin", "Administrador multi-empresa")
    
    System(api, "TimeControl API", "API REST multi-tenant para gestión de tiempo y formularios con IA")
    
    System_Ext(openai, "OpenAI API", "Servicio de IA para generación de formularios")
    System_Ext(redis, "Redis", "Cache distribuido")
    System_Ext(rabbitmq, "RabbitMQ", "Cola de mensajería para eventos")
    
    SystemDb(masterDb, "Master Database", "Base de datos maestra (empresas y usuarios maestros)")
    SystemDb(tenantDb, "Tenant Database", "Base de datos por tenant (datos de la empresa)")
    
    Rel(user, api, "Usa", "HTTPS")
    Rel(admin, api, "Administra", "HTTPS")
    Rel(masterAdmin, api, "Gestiona", "HTTPS")
    
    Rel(api, openai, "Genera formularios", "HTTPS")
    Rel(api, redis, "Cachea datos", "Redis Protocol")
    Rel(api, rabbitmq, "Publica eventos", "AMQP")
    
    Rel(api, masterDb, "Lee/Escribe", "MySQL")
    Rel(api, tenantDb, "Lee/Escribe", "MySQL")
```

---

## Diagrama C4 - Contenedores

### Nivel 2: Contenedores

```mermaid
C4Container
    title Diagrama de Contenedores - TimeControl API
    
    Person(user, "Usuario")
    Person(admin, "Administrador")
    
    System_Boundary(apiBoundary, "TimeControl API") {
        Container(webApi, "ASP.NET Core Web API", "C# .NET 8", "API REST con autenticación JWT")
        Container(tenantResolver, "Tenant Resolver", "C#", "Middleware de resolución de tenant")
        Container(authService, "Auth Service", "C#", "Servicio de autenticación y autorización")
        Container(moduleService, "Module Service", "C#", "Gestión de módulos y permisos")
        Container(formBuilderService, "Form Builder Service", "C#", "Generación de formularios con IA")
    }
    
    System_Ext(openai, "OpenAI API")
    SystemDb(masterDb, "Master DB")
    SystemDb(tenantDb, "Tenant DB")
    SystemDb_Ext(redis, "Redis Cache")
    SystemDb_Ext(rabbitmq, "RabbitMQ")
    
    Rel(user, webApi, "HTTPS")
    Rel(admin, webApi, "HTTPS")
    
    Rel(webApi, tenantResolver, "Usa")
    Rel(webApi, authService, "Usa")
    Rel(webApi, moduleService, "Usa")
    Rel(webApi, formBuilderService, "Usa")
    
    Rel(tenantResolver, masterDb, "Consulta")
    Rel(authService, tenantDb, "Lee/Escribe")
    Rel(moduleService, tenantDb, "Lee/Escribe")
    Rel(formBuilderService, tenantDb, "Lee/Escribe")
    Rel(formBuilderService, openai, "Llama API")
    
    Rel(authService, redis, "Cachea tokens")
    Rel(webApi, rabbitmq, "Publica eventos")
```

---

## Flujos de Casos de Uso

### 1. Flujo de Autenticación (Login)

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant TenantResolver
    participant AuthService
    participant TenantDb
    participant JwtService
    participant Redis
    
    Client->>API: POST /api/auth/login<br/>{email, password, deviceId}
    API->>TenantResolver: Resolver tenant (X-Tenant header)
    TenantResolver-->>API: TenantContext
    
    API->>AuthService: LoginAsync(request)
    AuthService->>TenantDb: Buscar usuario por email
    TenantDb-->>AuthService: Usuario encontrado
    
    AuthService->>AuthService: Validar password (Argon2id)
    
    alt Password válido
        AuthService->>JwtService: Generar Access Token
        AuthService->>JwtService: Generar Refresh Token
        JwtService-->>AuthService: Tokens generados
        
        AuthService->>TenantDb: Guardar RefreshToken
        AuthService->>Redis: Cachear sesión
        AuthService-->>API: LoginResponse {accessToken, refreshToken}
        API-->>Client: 200 OK + Tokens
    else Password inválido
        AuthService-->>API: null
        API-->>Client: 401 Unauthorized
    end
```

### 2. Flujo de Resolución de Tenant

```mermaid
sequenceDiagram
    participant Client
    participant TenantMiddleware
    participant TenantResolver
    participant MasterDb
    participant TenantContext
    
    Client->>TenantMiddleware: Request + X-Tenant header
    TenantMiddleware->>TenantResolver: ResolveAsync(httpContext)
    
    TenantResolver->>TenantResolver: Extraer subdomain (header/query/host)
    
    TenantResolver->>MasterDb: Buscar Company por subdomain
    MasterDb-->>TenantResolver: Company encontrada
    
    TenantResolver->>TenantResolver: Construir connection string
    TenantResolver->>TenantContext: Crear TenantContext
    TenantContext-->>TenantResolver: Context creado
    
    TenantResolver-->>TenantMiddleware: TenantContext
    TenantMiddleware->>TenantMiddleware: Asignar a HttpContext.Items
    TenantMiddleware-->>Client: Continuar pipeline
```

### 3. Flujo de Creación de Módulo con IA

```mermaid
sequenceDiagram
    participant Admin
    participant API
    participant ModuleController
    participant PermissionService
    participant AiFormService
    participant OpenAI
    participant TenantDb
    participant RabbitMQ
    
    Admin->>API: POST /api/modules/generate<br/>{prompt, aiModel}
    API->>ModuleController: GenerateModule(request)
    
    ModuleController->>PermissionService: Validar permiso "Module:Generate"
    
    alt Permiso válido
        ModuleController->>AiFormService: GenerateModuleFromPrompt(prompt, model)
        
        AiFormService->>OpenAI: POST /v1/chat/completions<br/>{prompt: "Genera módulo..."}
        OpenAI-->>AiFormService: JSON Schema del módulo
        
        AiFormService->>AiFormService: Parsear y validar JSON Schema
        AiFormService->>TenantDb: Crear Module
        AiFormService->>TenantDb: Crear ModulePrivileges (Create, Read, Update, Delete)
        AiFormService->>TenantDb: Guardar PromptHistory
        
        AiFormService->>RabbitMQ: Publicar evento "module.generated"
        AiFormService-->>ModuleController: ModuleResponse
        
        ModuleController-->>API: 201 Created + Module
        API-->>Admin: Módulo creado exitosamente
    else Sin permiso
        PermissionService-->>ModuleController: Forbidden
        ModuleController-->>API: 403 Forbidden
        API-->>Admin: Acceso denegado
    end
```

### 4. Flujo de Asignación Masiva de Permisos

```mermaid
sequenceDiagram
    participant Admin
    participant API
    participant PermissionController
    participant PermissionService
    participant TenantDb
    participant Redis
    
    Admin->>API: POST /api/permissions/assign-bulk<br/>{assignments: [...]}
    API->>PermissionController: AssignBulkPermissions(request)
    
    PermissionController->>PermissionService: Validar permiso "Permission:Assign"
    
    alt Permiso válido
        loop Para cada asignación
            PermissionController->>PermissionService: ValidatePermissionAssignment(assignment)
            
            alt Asignación válida
                PermissionService->>TenantDb: Verificar ModulePrivilege existe
                TenantDb-->>PermissionService: Privilege encontrado
                
                alt Asignar a Rol
                    PermissionService->>TenantDb: Crear PermissionAssignment (RoleId)
                    PermissionService->>TenantDb: Verificar herencia de permisos
                    PermissionService->>TenantDb: Actualizar permisos heredados
                else Asignar a Usuario
                    PermissionService->>TenantDb: Crear PermissionAssignment (UserId)
                end
                
                PermissionService->>Redis: Invalidar cache de permisos del usuario/rol
            else Asignación inválida
                PermissionService-->>PermissionController: Error
            end
        end
        
        PermissionService-->>PermissionController: BulkAssignmentResult
        PermissionController-->>API: 200 OK + Resultado
        API-->>Admin: Permisos asignados exitosamente
    else Sin permiso
        PermissionService-->>PermissionController: Forbidden
        PermissionController-->>API: 403 Forbidden
        API-->>Admin: Acceso denegado
    end
```

### 5. Flujo de Generación de Formulario con IA

```mermaid
sequenceDiagram
    participant User
    participant API
    participant FormBuilderController
    participant FormBuilderService
    participant AiService
    participant OpenAI
    participant TenantDb
    participant RabbitMQ
    
    User->>API: POST /api/forms/generate<br/>{prompt, aiModel}
    API->>FormBuilderController: GenerateForm(request)
    
    FormBuilderController->>FormBuilderService: Validar permiso "FormBuilder:Generate"
    
    alt Permiso válido
        FormBuilderService->>FormBuilderService: Validar rate limit por tenant
        FormBuilderService->>FormBuilderService: Buscar prompt similar en cache (PromptHash)
        
        alt Prompt similar encontrado
            FormBuilderService->>TenantDb: Obtener FormTemplate del cache
            TenantDb-->>FormBuilderService: FormTemplate existente
        else Prompt nuevo
            FormBuilderService->>AiService: GenerateFormSchema(prompt, model)
            
            AiService->>OpenAI: POST /v1/chat/completions<br/>{prompt: "Genera JSON Schema..."}
            OpenAI-->>AiService: JSON Schema del formulario
            
            AiService->>AiService: Validar JSON Schema
            AiService->>AiService: Generar UiSchema
            AiService-->>FormBuilderService: FormSchema + UiSchema
            
            FormBuilderService->>TenantDb: Crear FormTemplate (Status: Draft)
            FormBuilderService->>TenantDb: Crear FormFields
            FormBuilderService->>TenantDb: Crear BusinessRules
            FormBuilderService->>TenantDb: Guardar PromptHistory
        end
        
        FormBuilderService->>RabbitMQ: Publicar evento "form.generated"
        FormBuilderService-->>FormBuilderController: FormTemplateResponse
        
        FormBuilderController-->>API: 200 OK + FormTemplate
        API-->>User: Formulario generado exitosamente
    else Sin permiso
        FormBuilderService-->>FormBuilderController: Forbidden
        FormBuilderController-->>API: 403 Forbidden
        API-->>User: Acceso denegado
    else Rate limit excedido
        FormBuilderService-->>FormBuilderController: TooManyRequests
        FormBuilderController-->>API: 429 Too Many Requests
        API-->>User: Límite de requests excedido
    end
```

### 6. Flujo de Verificación de Permisos en Endpoint

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant AuthMiddleware
    participant PermissionMiddleware
    participant PermissionService
    participant TenantDb
    participant Redis
    participant Controller
    
    Client->>API: GET /api/users<br/>Authorization: Bearer token
    API->>AuthMiddleware: Validar JWT token
    
    AuthMiddleware->>AuthMiddleware: Validar firma y expiración
    AuthMiddleware->>AuthMiddleware: Extraer claims (UserId, TenantId, Roles)
    
    alt Token válido
        AuthMiddleware->>PermissionMiddleware: Continuar con request
        PermissionMiddleware->>PermissionService: GetUserEffectivePermissions(userId, endpoint)
        
        PermissionService->>Redis: Buscar permisos en cache
        
        alt Permisos en cache
            Redis-->>PermissionService: Permisos cacheados
        else Permisos no en cache
            PermissionService->>TenantDb: Obtener roles del usuario
            TenantDb-->>PermissionService: Roles encontrados
            
            PermissionService->>TenantDb: Obtener PermissionAssignments (roles + usuario)
            PermissionService->>TenantDb: Obtener permisos heredados (ParentRole)
            TenantDb-->>PermissionService: Permisos efectivos
            
            PermissionService->>PermissionService: Calcular permisos efectivos (directos + heredados - overrides)
            PermissionService->>Redis: Cachear permisos (TTL: 5 min)
            PermissionService-->>PermissionService: Permisos calculados
        end
        
        PermissionService->>PermissionService: Verificar si tiene permiso para endpoint
        PermissionService->>PermissionService: Verificar Scope (OwnTenant/SubTenant/System)
        
        alt Permiso válido
            PermissionService-->>PermissionMiddleware: Permiso concedido
            PermissionMiddleware->>Controller: Continuar a controller
            Controller->>TenantDb: Ejecutar query (con filtro global TenantId)
            TenantDb-->>Controller: Datos del tenant
            Controller-->>API: Response
            API-->>Client: 200 OK + Datos
        else Sin permiso
            PermissionService-->>PermissionMiddleware: Permiso denegado
            PermissionMiddleware-->>API: 403 Forbidden
            API-->>Client: Acceso denegado
        end
    else Token inválido
        AuthMiddleware-->>API: 401 Unauthorized
        API-->>Client: Token inválido
    end
```

### 7. Flujo de Publicación de Formulario

```mermaid
sequenceDiagram
    participant Admin
    participant API
    participant FormBuilderController
    participant FormBuilderService
    participant PermissionService
    participant TenantDb
    participant RabbitMQ
    
    Admin->>API: POST /api/forms/{id}/publish<br/>{versionId, publicationType, targetRoles}
    API->>FormBuilderController: PublishForm(id, request)
    
    FormBuilderController->>PermissionService: Validar permiso "FormBuilder:Publish"
    
    alt Permiso válido
        FormBuilderController->>FormBuilderService: PublishFormVersion(formId, versionId, publication)
        
        FormBuilderService->>TenantDb: Obtener FormTemplate
        FormBuilderService->>TenantDb: Obtener FormVersion
        TenantDb-->>FormBuilderService: FormTemplate y FormVersion
        
        FormBuilderService->>FormBuilderService: Validar que FormVersion existe
        FormBuilderService->>FormBuilderService: Validar targetRoles (si RoleBased)
        
        FormBuilderService->>TenantDb: Crear FormPublication
        FormBuilderService->>TenantDb: Actualizar FormVersion (IsPublished = true)
        FormBuilderService->>TenantDb: Actualizar FormTemplate (Status = Published)
        
        FormBuilderService->>RabbitMQ: Publicar evento "form.published"
        FormBuilderService-->>FormBuilderController: PublicationResponse
        
        FormBuilderController-->>API: 200 OK
        API-->>Admin: Formulario publicado exitosamente
    else Sin permiso
        PermissionService-->>FormBuilderController: Forbidden
        FormBuilderController-->>API: 403 Forbidden
        API-->>Admin: Acceso denegado
    end
```

### 8. Flujo de Herencia de Permisos

```mermaid
sequenceDiagram
    participant System
    participant PermissionService
    participant TenantDb
    participant Cache
    
    System->>PermissionService: GetEffectivePermissions(userId)
    
    PermissionService->>TenantDb: Obtener UserRoles del usuario
    TenantDb-->>PermissionService: Roles: [Supervisor, Employee]
    
    loop Para cada rol
        PermissionService->>TenantDb: Obtener PermissionAssignments del rol
        TenantDb-->>PermissionService: Permisos directos del rol
        
        PermissionService->>PermissionService: Verificar si tiene ParentRole
        alt Tiene ParentRole
            PermissionService->>TenantDb: Obtener permisos del ParentRole (recursivo)
            TenantDb-->>PermissionService: Permisos heredados
            
            PermissionService->>PermissionService: Combinar permisos (directos + heredados)
            PermissionService->>TenantDb: Verificar Overrides
            TenantDb-->>PermissionService: Overrides encontrados
            
            PermissionService->>PermissionService: Aplicar overrides (remover permisos sobreescritos)
        end
    end
    
    PermissionService->>TenantDb: Obtener PermissionAssignments directos al usuario
    TenantDb-->>PermissionService: Permisos directos del usuario
    
    PermissionService->>PermissionService: Combinar todos los permisos (roles + directos)
    PermissionService->>PermissionService: Eliminar duplicados
    PermissionService->>PermissionService: Validar fechas (ValidFrom, ValidTo)
    
    PermissionService->>Cache: Guardar permisos efectivos (TTL: 5 min)
    PermissionService-->>System: Permisos efectivos calculados
```

---

## Diagrama de Componentes (C4 Nivel 3)

```mermaid
C4Component
    title Diagrama de Componentes - TimeControl API
    
    Container(webApi, "ASP.NET Core Web API", "C# .NET 8")
    
    Component_Boundary(controllers, "Controllers") {
        Component(authCtrl, "AuthController", "Autenticación")
        Component(usersCtrl, "UsersController", "Gestión de usuarios")
        Component(rolesCtrl, "RolesController", "Gestión de roles")
        Component(modulesCtrl, "ModulesController", "Gestión de módulos")
        Component(permissionsCtrl, "PermissionsController", "Gestión de permisos")
        Component(formsCtrl, "FormBuilderController", "Form Builder con IA")
        Component(companiesCtrl, "CompaniesController", "Gestión de empresas")
    }
    
    Component_Boundary(services, "Services") {
        Component(authSvc, "AuthService", "Autenticación")
        Component(userSvc, "UserService", "Usuarios")
        Component(roleSvc, "RoleService", "Roles")
        Component(moduleSvc, "ModuleService", "Módulos")
        Component(permSvc, "PermissionService", "Permisos")
        Component(formSvc, "FormBuilderService", "Form Builder")
        Component(aiSvc, "AiFormService", "Integración IA")
    }
    
    Component_Boundary(middleware, "Middleware") {
        Component(tenantMw, "TenantMiddleware", "Resolución de tenant")
        Component(authMw, "AuthMiddleware", "Autenticación JWT")
        Component(permMw, "PermissionMiddleware", "Autorización")
    }
    
    Component_Boundary(data, "Data Access") {
        Component(tenantResolver, "TenantResolver", "Resolución de tenant")
        Component(dbContext, "TenantDbContext", "EF Core Context")
        Component(masterDb, "MasterDbContext", "EF Core Master")
    }
    
    System_Ext(openai, "OpenAI API")
    SystemDb(tenantDb, "Tenant DB")
    SystemDb(masterDb, "Master DB")
    
    Rel(authCtrl, authSvc, "Usa")
    Rel(usersCtrl, userSvc, "Usa")
    Rel(rolesCtrl, roleSvc, "Usa")
    Rel(modulesCtrl, moduleSvc, "Usa")
    Rel(permissionsCtrl, permSvc, "Usa")
    Rel(formsCtrl, formSvc, "Usa")
    
    Rel(formSvc, aiSvc, "Usa")
    Rel(aiSvc, openai, "Llama API")
    
    Rel(tenantMw, tenantResolver, "Usa")
    Rel(tenantResolver, masterDb, "Consulta")
    
    Rel(authSvc, dbContext, "Usa")
    Rel(userSvc, dbContext, "Usa")
    Rel(roleSvc, dbContext, "Usa")
    Rel(moduleSvc, dbContext, "Usa")
    Rel(permSvc, dbContext, "Usa")
    Rel(formSvc, dbContext, "Usa")
    
    Rel(dbContext, tenantDb, "Lee/Escribe")
```

---

## Flujo de Datos - Arquitectura Multi-Tenant

```mermaid
flowchart TD
    Start([Request HTTP]) --> TenantHeader{Tiene<br/>X-Tenant?}
    TenantHeader -->|No| ExtractSubdomain[Extraer de host<br/>subdomain.localhost]
    TenantHeader -->|Sí| GetSubdomain[Usar X-Tenant header]
    ExtractSubdomain --> GetSubdomain
    
    GetSubdomain --> QueryMaster[Consultar Master DB<br/>Companies por subdomain]
    QueryMaster --> Found{Company<br/>encontrada?}
    
    Found -->|No| Error404[404 Tenant Not Found]
    Found -->|Sí| BuildConn[Construir Connection String<br/>con datos de Company]
    
    BuildConn --> CreateContext[Crear TenantContext<br/>CompanyId + ConnectionString]
    CreateContext --> SetContext[Asignar a HttpContext.Items]
    
    SetContext --> AuthCheck{Requiere<br/>Autenticación?}
    AuthCheck -->|No| Continue[Continuar pipeline]
    AuthCheck -->|Sí| ValidateJWT[Validar JWT Token]
    
    ValidateJWT --> ValidToken{Token<br/>válido?}
    ValidToken -->|No| Error401[401 Unauthorized]
    ValidToken -->|Sí| ExtractClaims[Extraer Claims<br/>UserId, TenantId, Roles]
    
    ExtractClaims --> PermissionCheck{Requiere<br/>Permisos?}
    PermissionCheck -->|No| Continue
    PermissionCheck -->|Sí| CheckPermission[Verificar Permisos<br/>con PermissionService]
    
    CheckPermission --> HasPermission{Usuario tiene<br/>permiso?}
    HasPermission -->|No| Error403[403 Forbidden]
    HasPermission -->|Sí| Continue
    
    Continue --> CreateDbContext[Crear TenantDbContext<br/>con ConnectionString del Tenant]
    CreateDbContext --> ApplyFilters[Aplicar Filtros Globales<br/>WHERE TenantId = @CurrentTenantId]
    ApplyFilters --> ExecuteQuery[Ejecutar Query<br/>EF Core]
    ExecuteQuery --> ReturnResponse[Retornar Response]
    
    Error404 --> End([End])
    Error401 --> End
    Error403 --> End
    ReturnResponse --> End
```

---

## Resumen de Flujos Implementados

### Flujos Existentes (Implementados)

1. **Autenticación**
   - Login con email/password
   - Refresh token
   - Logout
   - Validación de token

2. **Gestión de Usuarios**
   - Registro de usuarios
   - Listado de usuarios
   - Actualización de usuarios
   - Eliminación (soft delete)
   - Asignación de roles

3. **Gestión de Roles**
   - Crear rol
   - Listar roles
   - Actualizar rol
   - Eliminar rol
   - Asignar permisos a rol

4. **Gestión de Empresas (Master)**
   - Crear empresa
   - Listar empresas
   - Actualizar empresa
   - Actualizar conexión de BD

5. **Usuarios Maestros (Master)**
   - Registrar usuario maestro
   - Asignar empresa a usuario maestro
   - Revocar acceso a empresa

### Flujos Nuevos (A Implementar)

1. **Gestión de Módulos**
   - Crear módulo
   - Generar módulo con IA
   - Listar módulos
   - Actualizar módulo
   - Eliminar módulo

2. **Gestión de Permisos**
   - Asignar permisos masivamente
   - Obtener permisos efectivos de usuario
   - Obtener permisos de módulo
   - Herencia de permisos

3. **Form Builder con IA**
   - Generar formulario desde prompt
   - Listar formularios
   - Actualizar formulario
   - Publicar formulario
   - Versionar formulario
   - Rollback de versión

---

**Última actualización**: 2024-01-15  
**Versión**: 1.0.0

