# Verificar Configuración de Conexión

## Paso 1: Verificar que el puerto está en la BD

Ejecuta este SQL para verificar la configuración de SisCore:

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

**Verifica que:**
- `DbHost` tenga tu IP (ej: `192.168.1.100`)
- `DbPort` tenga tu puerto (ej: `3307`) - **NO debe ser NULL si tu puerto no es 3306**
- `ConnectionOptions` tenga opciones si las necesitas

## Paso 2: Si el puerto está NULL y necesitas especificar uno

```sql
USE timecontrol_master;

UPDATE Companies 
SET DbPort = 3307  -- Tu puerto aquí
WHERE Subdomain = 'siscore';
```

## Paso 3: Probar el endpoint de diagnóstico

```
GET https://localhost:7004/api/diagnostics/tenant?tenant=siscore
```

Deberías ver en la respuesta:
```json
{
  "tenantContext": {
    "connectionStringPreview": "Server=192.168.1.100;Port=3307;Database=SisCore;..."
  }
}
```

Si ves `Port=3307` en el connection string, entonces está funcionando correctamente.

## Paso 4: Verificar en los logs

Con el logging habilitado, deberías ver en la consola:
```
Puerto especificado en connection string: 3307
Connection string construida: Server=192.168.1.100;Port=3307;Database=SisCore;User=root;Password=***;SslMode=None
```

## Ejemplo de UPDATE completo

Si necesitas actualizar SisCore con IP y puerto:

```sql
USE timecontrol_master;

UPDATE Companies 
SET 
    DbHost = '192.168.1.100',  -- Tu IP
    DbPort = 3307,              -- Tu puerto (SIEMPRE especificar, incluso si es 3306)
    DbPassword = 'tu_password',
    ConnectionOptions = 'SslMode=None;ConnectionTimeout=30'
WHERE Subdomain = 'siscore';
```

**Importante:** Especifica el puerto SIEMPRE, incluso si es 3306. Esto asegura que la conexión use el puerto correcto.

