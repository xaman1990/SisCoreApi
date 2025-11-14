-- =============================================================================
-- Script SQL para insertar datos base del modelo de permisos
-- Base de datos: siscore (tenant database)
-- =============================================================================
-- IMPORTANTE: Ejecuta este script DESPUÉS de crear las tablas con el script 13
-- Ajusta los valores de CreatedBy según el ID del usuario administrador de tu sistema
-- =============================================================================

USE siscore;

-- Usuario administrador responsable del seed
SET @admin_user_id = 1; -- Cambia por el Id real del usuario administrador

-- =============================================================================
-- 1. Catálogo de permisos base
-- =============================================================================

INSERT INTO Permissions (Code, Name, Description, IsSystem, IsDefaultForModule, CreatedAt, CreatedBy)
VALUES
    ('create', 'Crear', 'Permite crear registros en módulos', TRUE, TRUE, NOW(), @admin_user_id),
    ('read', 'Leer', 'Permite consultar registros en módulos', TRUE, TRUE, NOW(), @admin_user_id),
    ('update', 'Actualizar', 'Permite editar registros en módulos', TRUE, TRUE, NOW(), @admin_user_id),
    ('delete', 'Eliminar', 'Permite eliminar registros en módulos', TRUE, TRUE, NOW(), @admin_user_id),
    ('execute', 'Ejecutar', 'Permite ejecutar acciones especiales del módulo', TRUE, FALSE, NOW(), @admin_user_id),
    ('export', 'Exportar', 'Permite exportar información del módulo', TRUE, FALSE, NOW(), @admin_user_id)
ON DUPLICATE KEY UPDATE
    Name = VALUES(Name),
    Description = VALUES(Description),
    IsDefaultForModule = VALUES(IsDefaultForModule),
    IsSystem = VALUES(IsSystem);

-- =============================================================================
-- 2. Privilegios por módulo (se crean por defecto para todos los módulos activos)
-- =============================================================================

INSERT INTO ModulePrivileges (ModuleId, PermissionId, Code, Name, Description, CreatedAt, CreatedBy, IsDefault, IsDeleted)
SELECT
    m.Id,
    p.Id,
    p.Code,
    p.Name,
    CONCAT(p.Name, ' en ', m.Name),
    NOW(),
    @admin_user_id,
    p.IsDefaultForModule,
    FALSE
FROM Modules m
CROSS JOIN Permissions p
WHERE m.IsEnabled = TRUE
  AND p.IsDefaultForModule = TRUE
  AND NOT EXISTS (
        SELECT 1 FROM ModulePrivileges mp
        WHERE mp.ModuleId = m.Id AND mp.PermissionId = p.Id
    );

-- Crear privilegios adicionales opcionales (no marcados como default)
INSERT INTO ModulePrivileges (ModuleId, PermissionId, Code, Name, Description, CreatedAt, CreatedBy, IsDefault, IsDeleted)
SELECT
    m.Id,
    p.Id,
    p.Code,
    p.Name,
    CONCAT(p.Name, ' en ', m.Name),
    NOW(),
    @admin_user_id,
    FALSE,
    FALSE
FROM Modules m
JOIN Permissions p ON p.Code IN ('crud.execute', 'crud.export')
WHERE m.IsEnabled = TRUE
  AND NOT EXISTS (
        SELECT 1 FROM ModulePrivileges mp
        WHERE mp.ModuleId = m.Id AND mp.PermissionId = p.Id
    );

-- =============================================================================
-- 3. Verificación rápida
-- =============================================================================

SELECT 'Permissions' AS Tabla, COUNT(*) AS TotalRegistros FROM Permissions
UNION ALL
SELECT 'ModulePrivileges' AS Tabla, COUNT(*) FROM ModulePrivileges WHERE IsDeleted = FALSE;