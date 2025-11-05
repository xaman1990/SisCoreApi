-- =====================================================
-- Script de Migración: Modificar Tabla MasterUsers
-- =====================================================
-- Este script modifica la tabla MasterUsers para:
-- 1. Eliminar campos duplicados (PasswordHash, PhoneVerified, EmailVerified, MfaSecret, MfaEnabled)
-- 2. Agregar referencias al usuario en BD tenant (TenantUserId, TenantCompanyId)
-- 3. Agregar campo IsGod para rol especial
-- 4. Agregar campo CreatedBy
--
-- IMPORTANTE: Este script elimina datos. Asegúrate de tener backup.
-- =====================================================

USE timecontrol_master;

-- =====================================================
-- PASO 1: Eliminar datos existentes de MasterUsers (si los hay)
-- =====================================================
-- IMPORTANTE: Solo ejecutar si no hay datos importantes
-- Si ya tienes usuarios maestros, primero necesitas migrarlos manualmente
-- DELETE FROM MasterUserSessions;
-- DELETE FROM MasterUserCompanies;
-- DELETE FROM MasterUsers;

-- =====================================================
-- PASO 2: Eliminar índices y foreign keys que cambiarán
-- =====================================================
-- Eliminar foreign key de MasterUserSessions (si existe)
ALTER TABLE `MasterUserSessions` 
DROP FOREIGN KEY IF EXISTS `FK_MasterUserSessions_MasterUsers_MasterUserId`;

-- Eliminar foreign key de MasterUserCompanies (si existe)
ALTER TABLE `MasterUserCompanies` 
DROP FOREIGN KEY IF EXISTS `FK_MasterUserCompanies_MasterUsers_MasterUserId`;

-- Eliminar índice único de GoogleId (ya no será único)
ALTER TABLE `MasterUsers` 
DROP INDEX IF EXISTS `idx_google_id`;

-- =====================================================
-- PASO 3: Eliminar columnas obsoletas
-- =====================================================
ALTER TABLE `MasterUsers`
DROP COLUMN IF EXISTS `PasswordHash`,
DROP COLUMN IF EXISTS `PhoneVerified`,
DROP COLUMN IF EXISTS `EmailVerified`,
DROP COLUMN IF EXISTS `MfaSecret`,
DROP COLUMN IF EXISTS `MfaEnabled`;

-- =====================================================
-- PASO 4: Agregar nuevas columnas
-- =====================================================
ALTER TABLE `MasterUsers`
ADD COLUMN IF NOT EXISTS `TenantUserId` INT NOT NULL AFTER `FullName` COMMENT 'ID del usuario en la BD tenant',
ADD COLUMN IF NOT EXISTS `TenantCompanyId` INT NOT NULL AFTER `TenantUserId` COMMENT 'ID de la empresa (Companies.Id)',
ADD COLUMN IF NOT EXISTS `IsGod` BOOLEAN NOT NULL DEFAULT FALSE AFTER `GoogleId` COMMENT 'Rol God: tiene todos los privilegios',
ADD COLUMN IF NOT EXISTS `CreatedBy` INT NULL AFTER `UpdatedAt` COMMENT 'ID del usuario que creó este registro maestro';

-- =====================================================
-- PASO 5: Crear nuevos índices
-- =====================================================
-- Índice único para TenantUserId + TenantCompanyId (un usuario solo puede estar una vez por empresa)
ALTER TABLE `MasterUsers`
ADD UNIQUE INDEX IF NOT EXISTS `idx_tenant_user_company` (`TenantUserId`, `TenantCompanyId`);

-- Índice para IsGod (para búsquedas rápidas)
ALTER TABLE `MasterUsers`
ADD INDEX IF NOT EXISTS `idx_is_god` (`IsGod`);

-- Índice para CreatedBy
ALTER TABLE `MasterUsers`
ADD INDEX IF NOT EXISTS `idx_created_by` (`CreatedBy`);

-- =====================================================
-- PASO 6: Agregar foreign key a Companies
-- =====================================================
ALTER TABLE `MasterUsers`
ADD CONSTRAINT IF NOT EXISTS `FK_MasterUsers_Companies_TenantCompanyId`
FOREIGN KEY (`TenantCompanyId`) REFERENCES `Companies`(`Id`) 
ON DELETE RESTRICT ON UPDATE CASCADE;

-- =====================================================
-- PASO 7: Agregar foreign key de CreatedBy (self-reference)
-- =====================================================
ALTER TABLE `MasterUsers`
ADD CONSTRAINT IF NOT EXISTS `FK_MasterUsers_MasterUsers_CreatedBy`
FOREIGN KEY (`CreatedBy`) REFERENCES `MasterUsers`(`Id`) 
ON DELETE SET NULL ON UPDATE CASCADE;

-- =====================================================
-- PASO 8: Recrear foreign keys de tablas relacionadas
-- =====================================================
-- MasterUserSessions
ALTER TABLE `MasterUserSessions`
ADD CONSTRAINT IF NOT EXISTS `FK_MasterUserSessions_MasterUsers_MasterUserId`
FOREIGN KEY (`MasterUserId`) REFERENCES `MasterUsers`(`Id`) 
ON DELETE CASCADE ON UPDATE CASCADE;

-- MasterUserCompanies
ALTER TABLE `MasterUserCompanies`
ADD CONSTRAINT IF NOT EXISTS `FK_MasterUserCompanies_MasterUsers_MasterUserId`
FOREIGN KEY (`MasterUserId`) REFERENCES `MasterUsers`(`Id`) 
ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `MasterUserCompanies`
ADD CONSTRAINT IF NOT EXISTS `FK_MasterUserCompanies_Companies_CompanyId`
FOREIGN KEY (`CompanyId`) REFERENCES `Companies`(`Id`) 
ON DELETE CASCADE ON UPDATE CASCADE;

-- =====================================================
-- PASO 9: Actualizar MasterUserCompanies para permitir rol "god"
-- =====================================================
-- El campo Role ya acepta cualquier string, pero podemos agregar un CHECK constraint
-- MySQL 8.0+ soporta CHECK constraints
ALTER TABLE `MasterUserCompanies`
DROP CHECK IF EXISTS `chk_role_values`;

ALTER TABLE `MasterUserCompanies`
ADD CONSTRAINT `chk_role_values` 
CHECK (`Role` IN ('god', 'owner', 'admin', 'viewer'));

-- =====================================================
-- PASO 10: Verificar cambios
-- =====================================================
DESCRIBE `MasterUsers`;
DESCRIBE `MasterUserCompanies`;

SELECT 
    'Migración completada. Verifica que las columnas estén correctas.' AS Message;

