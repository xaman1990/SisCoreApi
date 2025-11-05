# Cómo Funciona el Sistema Multi-Tenant

## ¿Qué es el Tenant?

El **tenant** es la **empresa** o **organización** que está usando el sistema. Cada empresa tiene su propia base de datos aislada.

## Cómo Funciona

### 1. Estructura de Bases de Datos

- **BD Maestra** (`timecontrol_master`): Contiene todas las empresas registradas
- **BD por Tenant**: Cada empresa tiene su propia BD (ej: `empresa1`, `empresa2`, etc.)

### 2. Resolución del Tenant

El sistema resuelve qué empresa (tenant) está haciendo la petición mediante:

1. **Header HTTP `X-Tenant`** (recomendado)
   ```
   X-Tenant: empresa1
   ```

2. **Subdominio** (alternativa)
   ```
   http://empresa1.localhost:5000/api/auth/login
   ```

### 3. Flujo de Resolución

```
1. Cliente hace petición → Incluye header X-Tenant: "empresa1"
2. TenantMiddleware intercepta la petición
3. TenantResolver busca la empresa en BD maestra
4. Si encuentra la empresa, guarda el TenantContext en HttpContext.Items
5. Los servicios usan el TenantContext para conectarse a la BD correcta
```

## Solución al Error "Tenant context not found"

Este error ocurre cuando:

1. **No se está enviando el header `X-Tenant`**
2. **La empresa no existe en la BD maestra**
3. **El subdominio no coincide con ninguna empresa registrada**

## Cómo Configurar y Usar

### Paso 1: Crear la BD Maestra

```sql
-- Ejecutar el script
mysql -u root -p < TimeControlApi/Data/Scripts/01_master_database.sql
```

### Paso 2: Registrar SisCore en la BD Maestra

```sql
USE timecontrol_master;

-- Registrar la empresa SisCore
INSERT INTO Companies (Name, Subdomain, DbHost, DbName, DbUser, DbPassword, Status)
VALUES (
    'SisCore',
    'siscore',
    'localhost',
    'SisCore',
    'root',
    'tu_password_mysql',
    1
);
```

**Nota:** Si ya tienes la BD `SisCore` creada, solo necesitas registrar la empresa en la BD maestra con el subdominio `siscore`.

### Paso 3: Crear/Ejecutar la BD del Tenant (SisCore)

Si ya tienes la BD `SisCore` creada, verifica que tenga todas las tablas necesarias ejecutando el script de plantilla:

```sql
-- Si ya existe la BD SisCore, úsala
USE SisCore;

-- Ejecutar el script de plantilla (copiar y pegar el contenido de 02_tenant_template.sql)
-- O ejecutar directamente:
SOURCE TimeControlApi/Data/Scripts/02_tenant_template.sql;
```

Si no tienes la BD creada:

```sql
-- Crear la BD del tenant
CREATE DATABASE SisCore CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE SisCore;

-- Ejecutar el script de plantilla
SOURCE TimeControlApi/Data/Scripts/02_tenant_template.sql;
```

### Paso 4: Usar los Endpoints

**En Postman:**
1. Configura la variable de colección `tenant` con el valor `siscore`
2. Todos los requests incluyen automáticamente el header `X-Tenant`

**En Swagger:**
1. Agrega el header `X-Tenant` manualmente con valor `siscore`

**En código (ejemplo con HttpClient):**

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-Tenant", "siscore");
var response = await client.PostAsync("https://localhost:5001/api/auth/login", content);
```

**En cURL:**
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -H "X-Tenant: siscore" \
  -d '{"email":"usuario@ejemplo.com","password":"password123"}'
```

## Ejemplo Completo

### 1. Estructura de BD después de la configuración:

```
timecontrol_master (BD Maestra)
├── Companies
│   └── [siscore, empresa2, ...]

SisCore (BD del Tenant)
├── Users
├── Roles
├── Timesheets
└── ... (todas las tablas del sistema)

otra_empresa (BD del Tenant)
├── Users
├── Roles
└── ... (aislada de SisCore)
```

### 2. Petición de Login:

```http
POST /api/auth/login
Headers:
  Content-Type: application/json
  X-Tenant: siscore
Body:
{
  "email": "admin@siscore.com",
  "password": "password123"
}
```

### 3. El sistema:

1. Lee el header `X-Tenant: siscore`
2. Busca en `timecontrol_master.Companies` donde `Subdomain = 'siscore'`
3. Obtiene la cadena de conexión: `Server=localhost;Database=SisCore;...`
4. Se conecta a la BD `SisCore`
5. Busca el usuario en esa BD específica
6. Retorna el token JWT

## Verificación

Para verificar que todo está configurado correctamente:

1. **Verificar BD maestra:**
```sql
USE timecontrol_master;
SELECT * FROM Companies;
```

2. **Verificar BD del tenant:**
```sql
USE SisCore;
SELECT * FROM Users;
SELECT * FROM Roles;
```

3. **Probar endpoint de login:**
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -H "X-Tenant: siscore" \
  -d '{"email":"tu_email","password":"tu_password"}'
```

## Troubleshooting

### Error: "Tenant context not found"

**Solución:**
- Verifica que estás enviando el header `X-Tenant`
- Verifica que la empresa existe en `timecontrol_master.Companies`
- Verifica que el `Subdomain` coincide exactamente

### Error: "No se pudo determinar la cadena de conexión"

**Solución:**
- Verifica que la BD del tenant existe
- Verifica que las credenciales en `Companies` son correctas
- Verifica que MySQL está corriendo

### Error: "Table 'SisCore.Users' doesn't exist"

**Solución:**
- Ejecuta el script `02_tenant_template.sql` en la BD `SisCore`
- Verifica que las tablas se crearon correctamente

## Notas Importantes

1. **Cada empresa tiene datos completamente aislados**
2. **El header `X-Tenant` es OBLIGATORIO** en todas las peticiones
3. **El subdominio debe coincidir exactamente** con el registro en `Companies.Subdomain`
4. **En producción**, usa HTTPS y considera usar AWS Secrets Manager para las credenciales de BD

