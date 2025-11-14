# Scripts SQL para Módulos y Permisos

Este directorio contiene los scripts SQL necesarios para crear las tablas de Módulos y Permisos en la base de datos del tenant.

## Orden de Ejecución

Ejecuta los scripts en el siguiente orden:

### 1. Crear Tablas (OBLIGATORIO)
```bash
mysql -u [usuario] -p siscore < 13_create_modules_permissions_tables.sql
```

O desde MySQL Workbench/HeidiSQL:
- Abre el archivo `13_create_modules_permissions_tables.sql`
- Ejecuta el script completo

Este script crea las siguientes tablas:
- `SubModules` - Submódulos dentro de un módulo
- `ModulePrivileges` - Privilegios o acciones dentro de un módulo
- `PermissionGroups` - Grupos de permisos para asignación masiva
- `PermissionGroupAssignments` - Relación entre grupos y privilegios
- `PermissionAssignments` - Asignación de permisos a roles o usuarios

### 2. Insertar Datos de Ejemplo (OPCIONAL)
```bash
mysql -u [usuario] -p siscore < 14_seed_modules_permissions_data.sql
```

**IMPORTANTE:** Antes de ejecutar este script, ajusta la variable `@admin_user_id` en la línea 10 del script con el ID de tu usuario administrador.

Este script:
- Crea privilegios CRUD estándar para todos los módulos existentes
- Crea grupos de permisos base (ADMIN_FULL, EDITOR, VIEWER)
- Asigna privilegios a los grupos
- Opcionalmente asigna permisos a roles existentes

### 3. Verificar Instalación (RECOMENDADO)
```bash
mysql -u [usuario] -p siscore < 15_verify_modules_permissions_tables.sql
```

Este script verifica:
- Que todas las tablas existan
- Que las columnas estén correctamente definidas
- Que los índices y foreign keys estén creados
- Que no haya problemas de integridad referencial

## Solución de Problemas

### Error: "Table 'siscore.moduleprivileges' doesn't exist"

Este error puede ocurrir por dos razones:

1. **Las tablas no se han creado:**
   - Ejecuta el script `13_create_modules_permissions_tables.sql`
   - Verifica con el script `15_verify_modules_permissions_tables.sql`

2. **Problema de case sensitivity en MySQL:**
   - En Linux, MySQL es case-sensitive para nombres de tablas
   - En Windows, MySQL no es case-sensitive por defecto
   - Si estás en Linux y el error persiste, verifica que las tablas se crearon con el nombre correcto:
     ```sql
     SHOW TABLES LIKE '%Module%';
     SHOW TABLES LIKE '%Permission%';
     ```

### Verificar Nombres de Tablas

Ejecuta este query para ver todas las tablas relacionadas:
```sql
USE siscore;
SHOW TABLES WHERE Tables_in_siscore LIKE '%Module%' 
   OR Tables_in_siscore LIKE '%Permission%'
   OR Tables_in_siscore LIKE '%Sub%';
```

Las tablas deberían aparecer como:
- `SubModules` (o `submodules` en sistemas case-insensitive)
- `ModulePrivileges` (o `moduleprivileges`)
- `PermissionGroups` (o `permissiongroups`)
- `PermissionGroupAssignments` (o `permissiongroupassignments`)
- `PermissionAssignments` (o `permissionassignments`)

### Si las Tablas Tienen Nombres en Minúsculas

Si tu MySQL está configurado para usar nombres en minúsculas y las tablas se crearon así, pero EF Core busca nombres con mayúsculas, puedes:

**Opción 1:** Configurar EF Core para usar nombres en minúsculas (recomendado)

Agrega esto en `TenantDbContext.OnModelCreating`:
```csharp
// Al inicio del método OnModelCreating
foreach (var entityType in mb.Model.GetEntityTypes())
{
    var tableName = entityType.GetTableName();
    if (tableName != null)
    {
        entityType.SetTableName(tableName.ToLowerInvariant());
    }
}
```

**Opción 2:** Renombrar las tablas en MySQL
```sql
RENAME TABLE moduleprivileges TO ModulePrivileges;
RENAME TABLE permissionassignments TO PermissionAssignments;
-- etc.
```

## Estructura de Datos

### ModulePrivileges
Define los privilegios (acciones) disponibles en cada módulo:
- `Action`: CREATE, READ, UPDATE, DELETE, EXECUTE, etc.
- `Scope`: OwnTenant, SubTenant, System
- `ValidFrom` / `ValidTo`: Fechas de vigencia

### PermissionAssignments
Asigna privilegios a roles o usuarios:
- Puede asignarse a un `RoleId` (para todos los usuarios del rol)
- O a un `UserId` (permiso directo al usuario)
- Soporta herencia y overrides

### PermissionGroups
Agrupa múltiples privilegios para asignación masiva:
- Útil para crear "perfiles" de permisos
- Ejemplo: "Editor Completo", "Solo Lectura"

## Próximos Pasos

Después de ejecutar los scripts:

1. **Verifica que la aplicación funcione:**
   - Intenta obtener permisos: `GET /api/permissions/me`
   - Lista módulos: `GET /api/modules`

2. **Crea privilegios para tus módulos existentes:**
   - Usa la API: `POST /api/modules/{id}/permissions`
   - O ejecuta queries SQL personalizados

3. **Asigna permisos a roles:**
   - Usa la API: `POST /api/permissions/assign-bulk`
   - O asigna manualmente desde la base de datos

## Notas Importantes

- Los scripts usan `INSERT IGNORE` para evitar errores si los datos ya existen
- Ajusta los valores de `CreatedBy` según tu sistema
- Los datos de seed son ejemplos - personalízalos según tus necesidades
- Las tablas usan soft delete (`IsDeleted`) para mantener historial

