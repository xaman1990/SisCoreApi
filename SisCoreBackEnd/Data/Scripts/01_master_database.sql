-- =====================================================
-- TimeControl - Base de Datos Maestra
-- =====================================================
-- Esta BD contiene la información de todas las empresas (tenants)
-- y usuarios maestros (super usuarios)

CREATE DATABASE IF NOT EXISTS `timecontrol_master` 
CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE `timecontrol_master`;

-- =====================================================
-- Tabla: Companies
-- Almacena todas las empresas registradas en el sistema
-- =====================================================
CREATE TABLE IF NOT EXISTS `Companies` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(255) NOT NULL,
    `Subdomain` VARCHAR(100) NOT NULL UNIQUE,
    `DbHost` VARCHAR(255) NOT NULL COMMENT 'IP o hostname de la BD (ej: localhost, 192.168.1.100, mysql.example.com)',
    `DbPort` INT NULL COMMENT 'Puerto de MySQL (por defecto 3306 si es null)',
    `DbName` VARCHAR(100) NOT NULL COMMENT 'Nombre de la BD del tenant',
    `DbUser` VARCHAR(100) NOT NULL,
    `DbPassword` VARCHAR(500) NOT NULL COMMENT 'Encriptado, usar Secrets Manager en producción',
    `ConnectionOptions` VARCHAR(500) NULL COMMENT 'Opciones adicionales de conexión (ej: SslMode=None;ConnectionTimeout=30)',
    `BrandingJson` JSON NULL COMMENT 'Configuración de branding (logo, colores, etc.)',
    `SettingsJson` JSON NULL COMMENT 'Configuraciones generales de la empresa',
    `Status` TINYINT NOT NULL DEFAULT 1 COMMENT '1=Activo, 0=Inactivo',
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_subdomain` (`Subdomain`),
    INDEX `idx_status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: MasterUsers
-- Usuarios maestros (super usuarios) que pueden acceder
-- a múltiples empresas y crear nuevas empresas
-- =====================================================
CREATE TABLE IF NOT EXISTS `MasterUsers` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Email` VARCHAR(255) NOT NULL UNIQUE,
    `PasswordHash` VARCHAR(255) NOT NULL COMMENT 'Argon2id hash',
    `FullName` VARCHAR(255) NOT NULL,
    `PhoneNumber` VARCHAR(20) NULL,
    `GoogleId` VARCHAR(255) NULL UNIQUE COMMENT 'ID de Google OAuth',
    `PhoneVerified` BOOLEAN NOT NULL DEFAULT FALSE,
    `EmailVerified` BOOLEAN NOT NULL DEFAULT FALSE,
    `MfaSecret` VARCHAR(255) NULL COMMENT 'TOTP secret para MFA',
    `MfaEnabled` BOOLEAN NOT NULL DEFAULT FALSE,
    `Status` TINYINT NOT NULL DEFAULT 1 COMMENT '1=Activo, 0=Inactivo, 2=Bloqueado',
    `LastLoginAt` DATETIME NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_email` (`Email`),
    INDEX `idx_google_id` (`GoogleId`),
    INDEX `idx_status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: MasterUserSessions
-- Sesiones de usuarios maestros con device binding
-- =====================================================
CREATE TABLE IF NOT EXISTS `MasterUserSessions` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `MasterUserId` INT NOT NULL,
    `RefreshTokenJti` VARCHAR(255) NOT NULL UNIQUE COMMENT 'JWT ID del refresh token',
    `DeviceId` VARCHAR(255) NULL COMMENT 'ID del dispositivo',
    `DeviceName` VARCHAR(255) NULL,
    `IpAddress` VARCHAR(45) NULL,
    `UserAgent` TEXT NULL,
    `ExpiresAt` DATETIME NOT NULL,
    `RevokedAt` DATETIME NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`MasterUserId`) REFERENCES `MasterUsers`(`Id`) ON DELETE CASCADE,
    INDEX `idx_master_user_id` (`MasterUserId`),
    INDEX `idx_refresh_token_jti` (`RefreshTokenJti`),
    INDEX `idx_expires_at` (`ExpiresAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: MasterUserCompanies
-- Relación entre usuarios maestros y empresas
-- Permite que un super usuario acceda a múltiples empresas
-- =====================================================
CREATE TABLE IF NOT EXISTS `MasterUserCompanies` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `MasterUserId` INT NOT NULL,
    `CompanyId` INT NOT NULL,
    `Role` VARCHAR(50) NOT NULL DEFAULT 'viewer' COMMENT 'owner, admin, viewer',
    `GrantedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `GrantedBy` INT NULL COMMENT 'ID del usuario que otorgó el acceso',
    FOREIGN KEY (`MasterUserId`) REFERENCES `MasterUsers`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`CompanyId`) REFERENCES `Companies`(`Id`) ON DELETE CASCADE,
    UNIQUE KEY `uk_master_user_company` (`MasterUserId`, `CompanyId`),
    INDEX `idx_company_id` (`CompanyId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: AuditLogs
-- Logs de auditoría a nivel maestro
-- =====================================================
CREATE TABLE IF NOT EXISTS `AuditLogs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `MasterUserId` INT NULL COMMENT 'Usuario que realizó la acción',
    `CompanyId` INT NULL COMMENT 'Empresa afectada (si aplica)',
    `Action` VARCHAR(100) NOT NULL COMMENT 'create, update, delete, login, etc.',
    `EntityType` VARCHAR(100) NOT NULL COMMENT 'Company, MasterUser, etc.',
    `EntityId` INT NULL,
    `OldValues` JSON NULL,
    `NewValues` JSON NULL,
    `IpAddress` VARCHAR(45) NULL,
    `UserAgent` TEXT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_master_user_id` (`MasterUserId`),
    INDEX `idx_company_id` (`CompanyId`),
    INDEX `idx_action` (`Action`),
    INDEX `idx_created_at` (`CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Datos iniciales
-- =====================================================
-- Insertar usuario maestro inicial (password: Admin123! - cambiar en producción)
-- Hash Argon2id para "Admin123!" - DEBE SER CAMBIADO EN PRODUCCIÓN
INSERT INTO `MasterUsers` (`Email`, `PasswordHash`, `FullName`, `EmailVerified`, `Status`)
VALUES ('admin@timecontrol.com', '$argon2id$v=19$m=65536,t=3,p=4$dummy_hash_change_in_production', 'Super Administrador', TRUE, 1)
ON DUPLICATE KEY UPDATE `Email` = `Email`;

