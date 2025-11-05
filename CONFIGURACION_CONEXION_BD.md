# Configuración de Conexión a Base de Datos por Empresa

## Campos de Configuración

Cada empresa en la tabla `Companies` puede tener una configuración personalizada de conexión a su base de datos:

### Campos Disponibles

- **DbHost**: IP o hostname del servidor MySQL
  - Ejemplos: `localhost`, `192.168.1.100`, `mysql.example.com`
- **DbPort**: Puerto de MySQL (opcional, por defecto 3306)
  - Si es `NULL`, se usa el puerto por defecto 3306
  - Ejemplos: `3306`, `3307`, `33060`
- **DbName**: Nombre de la base de datos
- **DbUser**: Usuario de MySQL
- **DbPassword**: Contraseña de MySQL
- **ConnectionOptions**: Opciones adicionales de conexión (opcional)
  - Formato: `SslMode=None;ConnectionTimeout=30;CharSet=utf8mb4`
  - Si no se especifica, se usa `SslMode=None` por defecto

## Ejemplos de Configuración

### Ejemplo 1: BD Local (localhost)
```sql
INSERT INTO Companies (Name, Subdomain, DbHost, DbPort, DbName, DbUser, DbPassword, ConnectionOptions, Status)
VALUES (
    'SisCore',
    'siscore',
    'localhost',
    NULL,  -- NULL = puerto por defecto 3306
    'SisCore',
    'root',
    'password123',
    'SslMode=None',
    1
);
```

**Connection String generada:**
```
Server=localhost;Database=SisCore;User=root;Password=password123;SslMode=None
```

### Ejemplo 2: BD Remota con IP y Puerto Personalizado
```sql
INSERT INTO Companies (Name, Subdomain, DbHost, DbPort, DbName, DbUser, DbPassword, ConnectionOptions, Status)
VALUES (
    'SisCore',
    'siscore',
    '192.168.1.100',  -- IP del servidor
    3307,             -- Puerto personalizado
    'SisCore',
    'root',
    'password123',
    'SslMode=None;ConnectionTimeout=30',
    1
);
```

**Connection String generada:**
```
Server=192.168.1.100;Port=3307;Database=SisCore;User=root;Password=password123;SslMode=None;ConnectionTimeout=30
```

### Ejemplo 3: BD en Servidor con Hostname
```sql
INSERT INTO Companies (Name, Subdomain, DbHost, DbPort, DbName, DbUser, DbPassword, ConnectionOptions, Status)
VALUES (
    'SisCore',
    'siscore',
    'mysql.produccion.com',
    NULL,  -- Puerto por defecto
    'SisCore',
    'dbuser',
    'secure_password',
    'SslMode=Required;ConnectionTimeout=60',
    1
);
```

**Connection String generada:**
```
Server=mysql.produccion.com;Database=SisCore;User=dbuser;Password=secure_password;SslMode=Required;ConnectionTimeout=60
```

### Ejemplo 4: BD con Múltiples Opciones
```sql
INSERT INTO Companies (Name, Subdomain, DbHost, DbPort, DbName, DbUser, DbPassword, ConnectionOptions, Status)
VALUES (
    'SisCore',
    'siscore',
    '192.168.1.100',
    3307,
    'SisCore',
    'root',
    'password123',
    'SslMode=None;ConnectionTimeout=30;CharSet=utf8mb4;Pooling=true',
    1
);
```

**Connection String generada:**
```
Server=192.168.1.100;Port=3307;Database=SisCore;User=root;Password=password123;SslMode=None;ConnectionTimeout=30;CharSet=utf8mb4;Pooling=true
```

## Actualizar Configuración Existente

### Si ya tienes una empresa registrada y necesitas cambiar la IP/Puerto:

```sql
USE timecontrol_master;

-- Actualizar IP y puerto
UPDATE Companies 
SET 
    DbHost = '192.168.1.100',
    DbPort = 3307,
    ConnectionOptions = 'SslMode=None;ConnectionTimeout=30'
WHERE Subdomain = 'siscore';
```

### Agregar campos a BD existente (si ejecutaste el script 01 antes de esta actualización):

```sql
-- Ejecutar el script de migración
mysql -u root -p < TimeControlApi/Data/Scripts/05_add_port_and_options_to_companies.sql
```

O manualmente:

```sql
USE timecontrol_master;

ALTER TABLE Companies 
ADD COLUMN IF NOT EXISTS DbPort INT NULL COMMENT 'Puerto de MySQL (por defecto 3306 si es null)' AFTER DbHost;

ALTER TABLE Companies 
ADD COLUMN IF NOT EXISTS ConnectionOptions VARCHAR(500) NULL COMMENT 'Opciones adicionales de conexión' AFTER DbPassword;
```

## Opciones de Conexión Comunes

### Para Desarrollo Local
```
ConnectionOptions: 'SslMode=None'
```

### Para Producción
```
ConnectionOptions: 'SslMode=Required;ConnectionTimeout=60;CharSet=utf8mb4'
```

### Para Conexiones Lentas
```
ConnectionOptions: 'SslMode=None;ConnectionTimeout=120;CommandTimeout=60'
```

### Para Pooling de Conexiones
```
ConnectionOptions: 'SslMode=None;Pooling=true;MinimumPoolSize=5;MaximumPoolSize=100'
```

## Verificar Configuración

```sql
USE timecontrol_master;

SELECT 
    Id,
    Name,
    Subdomain,
    DbHost,
    DbPort,
    DbName,
    DbUser,
    ConnectionOptions,
    Status
FROM Companies
WHERE Subdomain = 'siscore';
```

## Notas Importantes

1. **Seguridad**: En producción, NO almacenes contraseñas en texto plano. Usa AWS Secrets Manager o Azure Key Vault.

2. **Puerto por Defecto**: Si `DbPort` es `NULL`, el sistema usa el puerto 3306 (puerto estándar de MySQL).

3. **ConnectionOptions**: Las opciones deben estar separadas por `;` (punto y coma).

4. **Validación**: El sistema valida que la conexión funcione al crear la empresa, pero no valida continuamente.

5. **Cambios en Tiempo Real**: Los cambios en la configuración de conexión se aplican inmediatamente sin necesidad de reiniciar la aplicación.

## Troubleshooting

### Error: "Unable to connect to any of the specified MySQL hosts"

**Solución:**
- Verifica que la IP/hostname sea correcta
- Verifica que el puerto sea correcto
- Verifica que el firewall permita conexiones en ese puerto
- Verifica que MySQL esté escuchando en esa IP y puerto

### Error: "Access denied for user"

**Solución:**
- Verifica que el usuario y contraseña sean correctos
- Verifica que el usuario tenga permisos para acceder desde esa IP
- Verifica que el usuario tenga permisos en la base de datos

### Error: "Unknown database"

**Solución:**
- Verifica que el nombre de la BD sea correcto
- Verifica que la BD exista en el servidor MySQL

