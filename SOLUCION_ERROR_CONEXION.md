# Solución al Error de Conexión

## Error Detectado

```
Access denied for user 'timecontrol'@'10.211.55.%' to database 'siscore'
```

## Problemas Identificados

1. **Usuario sin permisos**: El usuario `timecontrol` no tiene permisos para acceder a la BD `SisCore`
2. **Nombre de BD incorrecto**: El error muestra que intenta acceder a `siscore` (minúsculas) pero la BD puede llamarse `SisCore` (con mayúsculas)
3. **Configuración en BD maestra**: Necesita verificar que el nombre de la BD, usuario y contraseña sean correctos

## Soluciones

### Solución 1: Usar usuario root (Recomendado para desarrollo)

```sql
USE timecontrol_master;

UPDATE Companies 
SET 
    DbHost = '10.211.55.2',
    DbPort = 3306,
    DbName = 'SisCore',  -- IMPORTANTE: Nombre exacto (case-sensitive)
    DbUser = 'root',
    DbPassword = 'tu_password_root',
    ConnectionOptions = 'SslMode=None;ConnectionTimeout=30'
WHERE Subdomain = 'siscore';
```

### Solución 2: Dar permisos al usuario 'timecontrol'

Conecta al servidor MySQL en `10.211.55.2` y ejecuta:

```sql
-- Dar permisos al usuario timecontrol
GRANT ALL PRIVILEGES ON SisCore.* TO 'timecontrol'@'%' IDENTIFIED BY 'tu_password';
FLUSH PRIVILEGES;
```

O si el usuario ya existe:

```sql
GRANT ALL PRIVILEGES ON SisCore.* TO 'timecontrol'@'%';
FLUSH PRIVILEGES;
```

### Solución 3: Verificar nombre exacto de la BD

MySQL puede ser case-sensitive dependiendo del sistema operativo. Verifica el nombre exacto:

```sql
-- En el servidor MySQL
SHOW DATABASES LIKE '%core%';
SHOW DATABASES LIKE '%siscore%';
```

Luego actualiza en la BD maestra con el nombre exacto:

```sql
USE timecontrol_master;

UPDATE Companies 
SET DbName = 'SisCore'  -- O 'siscore' o 'SISCORE' según el nombre real
WHERE Subdomain = 'siscore';
```

## Pasos para Resolver

### Paso 1: Verificar configuración actual

```sql
USE timecontrol_master;

SELECT 
    DbHost,
    DbPort,
    DbName,
    DbUser
FROM Companies 
WHERE Subdomain = 'siscore';
```

### Paso 2: Verificar nombre de BD en MySQL

Conecta al servidor MySQL y verifica:

```sql
SHOW DATABASES;
-- Busca el nombre exacto de la BD (puede ser SisCore, siscore, SISCORE, etc.)
```

### Paso 3: Verificar permisos del usuario

```sql
-- En el servidor MySQL
SHOW GRANTS FOR 'timecontrol'@'%';
-- O
SHOW GRANTS FOR 'timecontrol'@'10.211.55.%';
```

### Paso 4: Actualizar configuración

Si el usuario es 'root':

```sql
USE timecontrol_master;

UPDATE Companies 
SET 
    DbUser = 'root',
    DbPassword = 'tu_password_root'
WHERE Subdomain = 'siscore';
```

Si quieres usar 'timecontrol':

1. Primero da permisos en MySQL:
```sql
GRANT ALL PRIVILEGES ON SisCore.* TO 'timecontrol'@'%' IDENTIFIED BY 'password';
FLUSH PRIVILEGES;
```

2. Luego actualiza en BD maestra:
```sql
USE timecontrol_master;

UPDATE Companies 
SET 
    DbUser = 'timecontrol',
    DbPassword = 'password'
WHERE Subdomain = 'siscore';
```

### Paso 5: Probar conexión

Después de actualizar, prueba el endpoint de diagnóstico:

```
GET https://localhost:7004/api/diagnostics/tenant?tenant=siscore
```

O intenta hacer login:

```
POST https://localhost:7004/api/auth/login?tenant=siscore
```

## Notas Importantes

1. **Case Sensitivity**: El nombre de la BD puede ser case-sensitive. Asegúrate de usar el nombre exacto.

2. **Permisos de Usuario**: El usuario debe tener permisos en la BD específica:
   ```sql
   GRANT ALL PRIVILEGES ON SisCore.* TO 'usuario'@'%';
   ```

3. **Contraseña**: Asegúrate de que la contraseña en la tabla `Companies` sea correcta.

4. **IP del Usuario**: Si el error muestra `'timecontrol'@'10.211.55.%'`, significa que MySQL está limitando el acceso por IP. Puedes usar `'%'` para permitir desde cualquier IP (solo en desarrollo).

## Verificación Rápida

Ejecuta este SQL para ver toda la configuración:

```sql
USE timecontrol_master;

SELECT 
    Name,
    Subdomain,
    DbHost,
    DbPort,
    DbName,
    DbUser,
    CONCAT('***', SUBSTRING(DbPassword, -4)) AS PasswordPreview
FROM Companies 
WHERE Subdomain = 'siscore';
```

Luego verifica en MySQL que:
- La BD existe con ese nombre exacto
- El usuario tiene permisos
- La contraseña es correcta

