-- =============================================================================
-- Script de Verificación para Tablas de Módulos y Permisos
-- Base de datos: siscore (tenant database)
-- =============================================================================
-- Este script verifica que todas las tablas y relaciones estén correctamente creadas
-- =============================================================================

USE siscore;

-- =============================================================================
-- 1. Verificar Existencia de Tablas
-- =============================================================================

SELECT 
    'Verificación de Tablas' AS Seccion,
    TABLE_NAME AS NombreTabla,
    TABLE_ROWS AS Filas,
    CASE 
        WHEN TABLE_NAME IN ('Permissions', 'SubModules', 'ModulePrivileges', 'PermissionAssignments') 
        THEN '✓ Creada' 
        ELSE '✗ Faltante' 
    END AS Estado
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME IN ('Permissions', 'SubModules', 'ModulePrivileges', 'PermissionAssignments')
ORDER BY TABLE_NAME;

-- =============================================================================
-- 2. Verificar Estructura de Columnas
-- =============================================================================

-- Verificar Permissions
SELECT 
    'Permissions' AS Tabla,
    COLUMN_NAME AS Columna,
    DATA_TYPE AS TipoDato,
    IS_NULLABLE AS PermiteNull,
    COLUMN_KEY AS Clave
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Permissions'
ORDER BY ORDINAL_POSITION;

-- Verificar SubModules
SELECT 
    'SubModules' AS Tabla,
    COLUMN_NAME AS Columna,
    DATA_TYPE AS TipoDato,
    IS_NULLABLE AS PermiteNull,
    COLUMN_KEY AS Clave
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'SubModules'
ORDER BY ORDINAL_POSITION;

-- Verificar ModulePrivileges
SELECT 
    'ModulePrivileges' AS Tabla,
    COLUMN_NAME AS Columna,
    DATA_TYPE AS TipoDato,
    IS_NULLABLE AS PermiteNull,
    COLUMN_KEY AS Clave
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'ModulePrivileges'
ORDER BY ORDINAL_POSITION;

-- Verificar PermissionAssignments
SELECT 
    'PermissionAssignments' AS Tabla,
    COLUMN_NAME AS Columna,
    DATA_TYPE AS TipoDato,
    IS_NULLABLE AS PermiteNull,
    COLUMN_KEY AS Clave
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'PermissionAssignments'
ORDER BY ORDINAL_POSITION;

-- =============================================================================
-- 3. Verificar Índices
-- =============================================================================

SELECT 
    TABLE_NAME AS Tabla,
    INDEX_NAME AS Indice,
    GROUP_CONCAT(COLUMN_NAME ORDER BY SEQ_IN_INDEX) AS Columnas
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME IN ('Permissions', 'SubModules', 'ModulePrivileges', 'PermissionAssignments')
    AND INDEX_NAME != 'PRIMARY'
GROUP BY TABLE_NAME, INDEX_NAME
ORDER BY TABLE_NAME, INDEX_NAME;

-- =============================================================================
-- 4. Verificar Foreign Keys
-- =============================================================================

SELECT 
    TABLE_NAME AS TablaOrigen,
    CONSTRAINT_NAME AS NombreFK,
    COLUMN_NAME AS ColumnaOrigen,
    REFERENCED_TABLE_NAME AS TablaReferenciada,
    REFERENCED_COLUMN_NAME AS ColumnaReferenciada
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = DATABASE()
    AND REFERENCED_TABLE_NAME IS NOT NULL
    AND TABLE_NAME IN ('Permissions', 'SubModules', 'ModulePrivileges', 'PermissionAssignments')
ORDER BY TABLE_NAME, CONSTRAINT_NAME;

-- =============================================================================
-- 5. Verificar Datos
-- =============================================================================

-- Contar registros en cada tabla
SELECT 
    'Permissions' AS Tabla,
    COUNT(*) AS TotalRegistros,
    COUNT(*) AS Activos
FROM Permissions

UNION ALL

SELECT 
    'SubModules' AS Tabla,
    COUNT(*) AS TotalRegistros,
    COUNT(CASE WHEN IsDeleted = FALSE THEN 1 END) AS Activos
FROM SubModules

UNION ALL

SELECT 
    'ModulePrivileges' AS Tabla,
    COUNT(*) AS TotalRegistros,
    COUNT(CASE WHEN IsDeleted = FALSE THEN 1 END) AS Activos
FROM ModulePrivileges

UNION ALL

SELECT 
    'PermissionAssignments' AS Tabla,
    COUNT(*) AS TotalRegistros,
    COUNT(CASE WHEN IsDeleted = FALSE THEN 1 END) AS Activos
FROM PermissionAssignments;

-- =============================================================================
-- 6. Verificar Integridad Referencial
-- =============================================================================

-- Verificar ModulePrivileges sin módulo válido
SELECT 
    'ModulePrivileges con ModuleId inválido' AS Problema,
    COUNT(*) AS Cantidad
FROM ModulePrivileges mp
LEFT JOIN Modules m ON mp.ModuleId = m.Id
WHERE m.Id IS NULL

UNION ALL

SELECT 
    'ModulePrivileges con PermissionId inválido' AS Problema,
    COUNT(*) AS Cantidad
FROM ModulePrivileges mp
LEFT JOIN Permissions p ON mp.PermissionId = p.Id
WHERE p.Id IS NULL

UNION ALL

-- Verificar PermissionAssignments sin ModulePrivilege válido
SELECT 
    'PermissionAssignments con ModulePrivilegeId inválido' AS Problema,
    COUNT(*) AS Cantidad
FROM PermissionAssignments pa
LEFT JOIN ModulePrivileges mp ON pa.ModulePrivilegeId = mp.Id
WHERE mp.Id IS NULL

UNION ALL

-- Verificar PermissionAssignments sin RoleId ni UserId
SELECT 
    'PermissionAssignments sin RoleId ni UserId' AS Problema,
    COUNT(*) AS Cantidad
FROM PermissionAssignments
WHERE (RoleId IS NULL AND UserId IS NULL)
    AND IsDeleted = FALSE;

-- =============================================================================
-- 7. Resumen de Estado
-- =============================================================================

SELECT 
    'RESUMEN' AS Tipo,
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Permissions')
            AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'SubModules')
            AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ModulePrivileges')
            AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'PermissionAssignments')
        THEN '✓ Todas las tablas existen'
        ELSE '✗ Faltan tablas - Ejecuta el script 13_create_modules_permissions_tables.sql'
    END AS Estado;

