-- =====================================================
-- Script para Insertar Usuario Administrador en SisCore
-- =====================================================
-- Este script inserta un usuario administrador en la BD tenant SisCore
-- Email: admin@timecontrol.com
-- Password: Admin123!
--
-- IMPORTANTE: El hash de la contraseña debe generarse usando el servicio PasswordService
-- del backend. Este script incluye un hash de ejemplo que DEBE SER REEMPLAZADO.
-- 
-- Para generar el hash correcto:
-- 1. Ejecuta el endpoint POST /api/auth/register (si está disponible)
-- 2. O usa el servicio PasswordService desde el código
-- 3. O ejecuta este script y luego actualiza el hash manualmente
--
-- =====================================================

USE SisCore;

-- =====================================================
-- OPCIÓN 1: Insertar usuario con hash temporal (ACTUALIZAR DESPUÉS)
-- =====================================================
-- Este hash es TEMPORAL y debe ser reemplazado
-- Hash temporal para "Admin123!" - NO SEGURO, SOLO PARA PRUEBAS
INSERT INTO `Users` (
    `Email`, 
    `PasswordHash`, 
    `FullName`, 
    `Status`, 
    `EmailVerified`,
    `CreatedAt`
)
VALUES (
    'admin@timecontrol.com',
    '$argon2id$v=19$m=1048576,t=4,p=8$dummy_salt_placeholder$dummy_hash_placeholder',
    'Administrador',
    1,
    TRUE,
    NOW()
)
ON DUPLICATE KEY UPDATE 
    `Email` = `Email`,
    `FullName` = 'Administrador',
    `UpdatedAt` = NOW();

-- =====================================================
-- OPCIÓN 2: Obtener el ID del usuario insertado y asignar rol de Administrador
-- =====================================================
-- Primero verificar si existe el rol "Administrador"
SET @admin_role_id = (SELECT `Id` FROM `Roles` WHERE `Name` = 'Administrador' LIMIT 1);

-- Si el rol no existe, crear uno básico (debería existir por el script 02_tenant_template.sql)
-- Si no existe, descomentar estas líneas:
-- INSERT INTO `Roles` (`Name`, `Description`, `IsSystem`, `Status`, `CreatedAt`)
-- VALUES ('Administrador', 'Administrador del sistema', TRUE, 1, NOW())
-- ON DUPLICATE KEY UPDATE `Name` = `Name`;
-- SET @admin_role_id = LAST_INSERT_ID();

-- Asignar rol de Administrador al usuario
SET @admin_user_id = (SELECT `Id` FROM `Users` WHERE `Email` = 'admin@timecontrol.com' LIMIT 1);

IF @admin_user_id IS NOT NULL AND @admin_role_id IS NOT NULL THEN
    INSERT INTO `UserRoles` (`UserId`, `RoleId`, `AssignedAt`)
    VALUES (@admin_user_id, @admin_role_id, NOW())
    ON DUPLICATE KEY UPDATE `AssignedAt` = NOW();
END IF;

-- =====================================================
-- VERIFICAR INSERCIÓN
-- =====================================================
SELECT 
    u.`Id`,
    u.`Email`,
    u.`FullName`,
    u.`Status`,
    u.`EmailVerified`,
    u.`CreatedAt`,
    GROUP_CONCAT(r.`Name` SEPARATOR ', ') AS `Roles`
FROM `Users` u
LEFT JOIN `UserRoles` ur ON u.`Id` = ur.`UserId`
LEFT JOIN `Roles` r ON ur.`RoleId` = r.`Id`
WHERE u.`Email` = 'admin@timecontrol.com'
GROUP BY u.`Id`;

-- =====================================================
-- NOTA IMPORTANTE:
-- =====================================================
-- El hash de la contraseña en este script es un PLACEHOLDER.
-- Para generar el hash correcto de Argon2id para "Admin123!", tienes dos opciones:
--
-- OPCIÓN A: Usar el endpoint de registro (si está disponible)
-- POST /api/auth/register?tenant=siscore
-- Body: {
--   "email": "admin@timecontrol.com",
--   "password": "Admin123!",
--   "fullName": "Administrador",
--   "roleIds": [1]  // ID del rol Administrador
-- }
--
-- OPCIÓN B: Generar el hash manualmente desde el código C#
-- Usa el servicio PasswordService.HashPassword("Admin123!")
-- y luego actualiza el registro:
--
-- UPDATE `Users` 
-- SET `PasswordHash` = 'HASH_GENERADO_AQUI'
-- WHERE `Email` = 'admin@timecontrol.com';
--
-- =====================================================

