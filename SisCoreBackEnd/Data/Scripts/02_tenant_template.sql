-- =====================================================
-- TimeControl - Plantilla de Base de Datos por Tenant
-- =====================================================
-- Este script se ejecuta al crear una nueva empresa
-- Reemplazar {TENANT_DB_NAME} con el nombre real de la BD

-- CREATE DATABASE IF NOT EXISTS `{TENANT_DB_NAME}` 
-- CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
-- USE `{TENANT_DB_NAME}`;

-- =====================================================
-- Tabla: Users
-- Usuarios de la empresa (tenant)
-- =====================================================
CREATE TABLE IF NOT EXISTS `Users` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Email` VARCHAR(255) NULL UNIQUE COMMENT 'NULL si solo usa teléfono',
    `PhoneNumber` VARCHAR(20) NULL UNIQUE COMMENT 'NULL si solo usa email',
    `PasswordHash` VARCHAR(255) NULL COMMENT 'NULL si solo usa OAuth/teléfono',
    `GoogleId` VARCHAR(255) NULL UNIQUE COMMENT 'ID de Google OAuth',
    `FullName` VARCHAR(255) NOT NULL,
    `EmployeeNumber` VARCHAR(50) NULL UNIQUE,
    `PhoneVerified` BOOLEAN NOT NULL DEFAULT FALSE,
    `EmailVerified` BOOLEAN NOT NULL DEFAULT FALSE,
    `MfaSecret` VARCHAR(255) NULL COMMENT 'TOTP secret para MFA',
    `MfaEnabled` BOOLEAN NOT NULL DEFAULT FALSE,
    `Status` TINYINT NOT NULL DEFAULT 1 COMMENT '1=Activo, 0=Inactivo, 2=Bloqueado',
    `LastLoginAt` DATETIME NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    `CreatedBy` INT NULL COMMENT 'ID del usuario que creó este registro',
    INDEX `idx_email` (`Email`),
    INDEX `idx_phone` (`PhoneNumber`),
    INDEX `idx_google_id` (`GoogleId`),
    INDEX `idx_status` (`Status`),
    INDEX `idx_employee_number` (`EmployeeNumber`),
    CONSTRAINT `chk_auth_method` CHECK (
        (`Email` IS NOT NULL) OR 
        (`PhoneNumber` IS NOT NULL) OR 
        (`GoogleId` IS NOT NULL)
    )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: Roles
-- Roles predefinidos del sistema
-- =====================================================
CREATE TABLE IF NOT EXISTS `Roles` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(100) NOT NULL UNIQUE,
    `Description` VARCHAR(500) NULL,
    `IsSystem` BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'Roles del sistema no se pueden eliminar',
    `Status` TINYINT NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: UserRoles
-- Relación muchos a muchos entre usuarios y roles
-- =====================================================
CREATE TABLE IF NOT EXISTS `UserRoles` (
    `UserId` INT NOT NULL,
    `RoleId` INT NOT NULL,
    `AssignedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `AssignedBy` INT NULL,
    PRIMARY KEY (`UserId`, `RoleId`),
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`) ON DELETE CASCADE,
    INDEX `idx_role_id` (`RoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: Modules
-- Módulos del sistema (configurables por empresa)
-- =====================================================
CREATE TABLE IF NOT EXISTS `Modules` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Code` VARCHAR(50) NOT NULL UNIQUE COMMENT 'timesheet, reports, settings, etc.',
    `Name` VARCHAR(100) NOT NULL,
    `Description` VARCHAR(500) NULL,
    `Icon` VARCHAR(100) NULL COMMENT 'Nombre del icono',
    `MenuOrder` INT NOT NULL DEFAULT 0,
    `IsEnabled` BOOLEAN NOT NULL DEFAULT TRUE,
    `IsSystem` BOOLEAN NOT NULL DEFAULT FALSE,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_code` (`Code`),
    INDEX `idx_menu_order` (`MenuOrder`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: Permissions
-- Catálogo global de permisos reutilizables por módulos
-- =====================================================
CREATE TABLE IF NOT EXISTS `Permissions` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Code` VARCHAR(100) NOT NULL UNIQUE,
    `Name` VARCHAR(100) NOT NULL,
    `Description` VARCHAR(500) NULL,
    `IsSystem` BOOLEAN NOT NULL DEFAULT FALSE,
    `IsDefaultForModule` BOOLEAN NOT NULL DEFAULT TRUE,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `CreatedBy` INT NULL,
    `UpdatedAt` DATETIME NULL,
    `UpdatedBy` INT NULL,
    INDEX `idx_permissions_created_at` (`CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: RefreshTokens
-- Tokens de refresh para autenticación JWT
-- =====================================================
CREATE TABLE IF NOT EXISTS `RefreshTokens` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `UserId` INT NOT NULL,
    `Jti` VARCHAR(255) NOT NULL UNIQUE COMMENT 'JWT ID',
    `DeviceId` VARCHAR(255) NULL,
    `DeviceName` VARCHAR(255) NULL,
    `IpAddress` VARCHAR(45) NULL,
    `UserAgent` TEXT NULL,
    `ExpiresAt` DATETIME NOT NULL,
    `RevokedAt` DATETIME NULL,
    `ReplacedByJti` VARCHAR(255) NULL COMMENT 'Token que reemplazó este (rotación)',
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE,
    INDEX `idx_user_id` (`UserId`),
    INDEX `idx_jti` (`Jti`),
    INDEX `idx_expires_at` (`ExpiresAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: CompanySettings
-- Configuraciones parametrizables por empresa
-- =====================================================
CREATE TABLE IF NOT EXISTS `CompanySettings` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Key` VARCHAR(100) NOT NULL UNIQUE,
    `Value` JSON NOT NULL,
    `Description` VARCHAR(500) NULL,
    `Category` VARCHAR(50) NOT NULL DEFAULT 'general' COMMENT 'timesheet, security, notifications, etc.',
    `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    `UpdatedBy` INT NULL,
    INDEX `idx_key` (`Key`),
    INDEX `idx_category` (`Category`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: Projects (Ordenes de Venta / OV)
-- Proyectos/Ordenes de Venta donde se registran horas
-- =====================================================
CREATE TABLE IF NOT EXISTS `Projects` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Code` VARCHAR(50) NOT NULL UNIQUE,
    `Name` VARCHAR(255) NOT NULL,
    `Description` TEXT NULL,
    `ClientId` INT NULL COMMENT 'FK a tabla Clients si existe',
    `CostCenterId` INT NULL COMMENT 'FK a tabla CostCenters si existe',
    `DefaultHoursPerDay` DECIMAL(5,2) NOT NULL DEFAULT 9.00 COMMENT 'Horas por defecto configurable por proyecto',
    `StartDate` DATE NULL,
    `EndDate` DATE NULL,
    `Status` TINYINT NOT NULL DEFAULT 1 COMMENT '1=Activo, 0=Inactivo',
    `SupervisorId` INT NULL COMMENT 'Supervisor asignado',
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    `CreatedBy` INT NULL,
    INDEX `idx_code` (`Code`),
    INDEX `idx_status` (`Status`),
    INDEX `idx_supervisor_id` (`SupervisorId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: Activities
-- Actividades dentro de los proyectos
-- =====================================================
CREATE TABLE IF NOT EXISTS `Activities` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `ProjectId` INT NOT NULL,
    `Code` VARCHAR(50) NOT NULL,
    `Name` VARCHAR(255) NOT NULL,
    `Description` TEXT NULL,
    `Status` TINYINT NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY `uk_project_code` (`ProjectId`, `Code`),
    FOREIGN KEY (`ProjectId`) REFERENCES `Projects`(`Id`) ON DELETE CASCADE,
    INDEX `idx_project_id` (`ProjectId`),
    INDEX `idx_status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: ProjectUsers
-- Usuarios asignados a proyectos
-- =====================================================
CREATE TABLE IF NOT EXISTS `ProjectUsers` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `ProjectId` INT NOT NULL,
    `UserId` INT NOT NULL,
    `AssignedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `AssignedBy` INT NULL,
    UNIQUE KEY `uk_project_user` (`ProjectId`, `UserId`),
    FOREIGN KEY (`ProjectId`) REFERENCES `Projects`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE,
    INDEX `idx_user_id` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: Timesheets
-- Registro de horas trabajadas
-- =====================================================
CREATE TABLE IF NOT EXISTS `Timesheets` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `UserId` INT NOT NULL,
    `ProjectId` INT NOT NULL,
    `ActivityId` INT NOT NULL,
    `Date` DATE NOT NULL COMMENT 'Fecha del trabajo realizado',
    `Hours` DECIMAL(5,2) NOT NULL COMMENT 'Horas trabajadas (mínimo 0.25)',
    `Note` VARCHAR(500) NULL,
    `LocationLatitude` DECIMAL(10,8) NULL COMMENT 'Latitud GPS',
    `LocationLongitude` DECIMAL(11,8) NULL COMMENT 'Longitud GPS',
    `LocationAddress` VARCHAR(500) NULL COMMENT 'Dirección derivada de GPS',
    `Status` TINYINT NOT NULL DEFAULT 0 COMMENT '0=Pendiente, 1=Aprobado, 2=Rechazado',
    `ApprovedBy` INT NULL COMMENT 'Supervisor que aprobó',
    `ApprovedAt` DATETIME NULL,
    `RejectionReason` VARCHAR(500) NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    `CreatedBy` INT NOT NULL COMMENT 'Usuario que registró',
    `UpdatedBy` INT NULL COMMENT 'Usuario que modificó',
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE RESTRICT,
    FOREIGN KEY (`ProjectId`) REFERENCES `Projects`(`Id`) ON DELETE RESTRICT,
    FOREIGN KEY (`ActivityId`) REFERENCES `Activities`(`Id`) ON DELETE RESTRICT,
    FOREIGN KEY (`ApprovedBy`) REFERENCES `Users`(`Id`) ON DELETE SET NULL,
    INDEX `idx_user_date` (`UserId`, `Date`),
    INDEX `idx_project_date` (`ProjectId`, `Date`),
    INDEX `idx_status` (`Status`),
    INDEX `idx_date` (`Date`),
    CONSTRAINT `chk_hours_min` CHECK (`Hours` >= 0.25),
    CONSTRAINT `chk_hours_max` CHECK (`Hours` <= 24.00)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: SupervisorReassignments
-- Historial de reasignaciones de proyectos entre supervisores
-- =====================================================
CREATE TABLE IF NOT EXISTS `SupervisorReassignments` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `ProjectId` INT NOT NULL,
    `OldSupervisorId` INT NULL,
    `NewSupervisorId` INT NOT NULL,
    `Reason` VARCHAR(500) NULL,
    `EffectiveDate` DATE NOT NULL,
    `ReassignedBy` INT NOT NULL,
    `ReassignedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`ProjectId`) REFERENCES `Projects`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`OldSupervisorId`) REFERENCES `Users`(`Id`) ON DELETE SET NULL,
    FOREIGN KEY (`NewSupervisorId`) REFERENCES `Users`(`Id`) ON DELETE RESTRICT,
    FOREIGN KEY (`ReassignedBy`) REFERENCES `Users`(`Id`) ON DELETE RESTRICT,
    INDEX `idx_project_id` (`ProjectId`),
    INDEX `idx_new_supervisor` (`NewSupervisorId`),
    INDEX `idx_effective_date` (`EffectiveDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: Omissions
-- Registro de omisos (días sin registro de horas)
-- =====================================================
CREATE TABLE IF NOT EXISTS `Omissions` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `UserId` INT NOT NULL,
    `ProjectId` INT NOT NULL,
    `Date` DATE NOT NULL,
    `NotificationSent` BOOLEAN NOT NULL DEFAULT FALSE,
    `NotificationChannel` VARCHAR(50) NULL COMMENT 'email, whatsapp, push',
    `NotificationSentAt` DATETIME NULL,
    `ResolvedAt` DATETIME NULL COMMENT 'Cuando se registró la hora faltante',
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`ProjectId`) REFERENCES `Projects`(`Id`) ON DELETE CASCADE,
    UNIQUE KEY `uk_user_project_date` (`UserId`, `ProjectId`, `Date`),
    INDEX `idx_user_date` (`UserId`, `Date`),
    INDEX `idx_date` (`Date`),
    INDEX `idx_notification_sent` (`NotificationSent`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: Notifications
-- Inbox de notificaciones para usuarios
-- =====================================================
CREATE TABLE IF NOT EXISTS `Notifications` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `UserId` INT NOT NULL,
    `Type` VARCHAR(50) NOT NULL COMMENT 'omission, approval, rejection, system, etc.',
    `Title` VARCHAR(255) NOT NULL,
    `Message` TEXT NOT NULL,
    `Read` BOOLEAN NOT NULL DEFAULT FALSE,
    `ReadAt` DATETIME NULL,
    `RelatedEntityType` VARCHAR(100) NULL COMMENT 'Timesheet, Omission, etc.',
    `RelatedEntityId` INT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE,
    INDEX `idx_user_read` (`UserId`, `Read`),
    INDEX `idx_created_at` (`CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: Tickets
-- Sistema de tickets de soporte
-- =====================================================
CREATE TABLE IF NOT EXISTS `Tickets` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Number` VARCHAR(50) NOT NULL UNIQUE COMMENT 'TICKET-0001',
    `UserId` INT NOT NULL COMMENT 'Usuario que crea el ticket',
    `AssignedTo` INT NULL COMMENT 'Usuario asignado para resolver',
    `Category` VARCHAR(50) NOT NULL COMMENT 'bug, feature, support, etc.',
    `Priority` VARCHAR(20) NOT NULL DEFAULT 'medium' COMMENT 'low, medium, high, critical',
    `Status` VARCHAR(20) NOT NULL DEFAULT 'open' COMMENT 'open, in_progress, resolved, closed',
    `Subject` VARCHAR(255) NOT NULL,
    `Description` TEXT NOT NULL,
    `Resolution` TEXT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    `ResolvedAt` DATETIME NULL,
    `ClosedAt` DATETIME NULL,
    FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE RESTRICT,
    FOREIGN KEY (`AssignedTo`) REFERENCES `Users`(`Id`) ON DELETE SET NULL,
    INDEX `idx_number` (`Number`),
    INDEX `idx_user_id` (`UserId`),
    INDEX `idx_status` (`Status`),
    INDEX `idx_priority` (`Priority`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: TicketAttachments
-- Adjuntos de tickets
-- =====================================================
CREATE TABLE IF NOT EXISTS `TicketAttachments` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `TicketId` INT NOT NULL,
    `FileName` VARCHAR(255) NOT NULL,
    `FilePath` VARCHAR(500) NOT NULL,
    `FileSize` BIGINT NOT NULL COMMENT 'En bytes',
    `MimeType` VARCHAR(100) NOT NULL,
    `UploadedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UploadedBy` INT NOT NULL,
    FOREIGN KEY (`TicketId`) REFERENCES `Tickets`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`UploadedBy`) REFERENCES `Users`(`Id`) ON DELETE RESTRICT,
    INDEX `idx_ticket_id` (`TicketId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: AuditLogs
-- Logs de auditoría del tenant
-- =====================================================
CREATE TABLE IF NOT EXISTS `AuditLogs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `UserId` INT NULL COMMENT 'Usuario que realizó la acción',
    `Action` VARCHAR(100) NOT NULL COMMENT 'create, update, delete, approve, etc.',
    `EntityType` VARCHAR(100) NOT NULL COMMENT 'Timesheet, Project, User, etc.',
    `EntityId` INT NULL,
    `OldValues` JSON NULL,
    `NewValues` JSON NULL,
    `IpAddress` VARCHAR(45) NULL,
    `UserAgent` TEXT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_user_id` (`UserId`),
    INDEX `idx_action` (`Action`),
    INDEX `idx_entity` (`EntityType`, `EntityId`),
    INDEX `idx_created_at` (`CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Tabla: SecurityLogs
-- Logs de seguridad (intentos de login, etc.)
-- =====================================================
CREATE TABLE IF NOT EXISTS `SecurityLogs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `UserId` INT NULL,
    `Email` VARCHAR(255) NULL COMMENT 'Email usado en intento de login',
    `PhoneNumber` VARCHAR(20) NULL COMMENT 'Teléfono usado en intento de login',
    `EventType` VARCHAR(50) NOT NULL COMMENT 'login_success, login_failed, password_change, etc.',
    `IpAddress` VARCHAR(45) NULL,
    `UserAgent` TEXT NULL,
    `DeviceId` VARCHAR(255) NULL,
    `Details` JSON NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_user_id` (`UserId`),
    INDEX `idx_event_type` (`EventType`),
    INDEX `idx_created_at` (`CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Datos iniciales (Seed)
-- =====================================================

-- Roles del sistema
INSERT INTO `Roles` (`Name`, `Description`, `IsSystem`, `Status`) VALUES
('SuperAdmin', 'Administrador con todos los permisos', TRUE, 1),
('Admin', 'Administrador de la empresa', TRUE, 1),
('Supervisor', 'Supervisor de proyectos', TRUE, 1),
('Employee', 'Empleado/Usuario final', TRUE, 1),
('HR', 'Recursos Humanos', TRUE, 1),
('Accounting', 'Contabilidad/Costos', TRUE, 1),
('Auditor', 'Auditor', TRUE, 1)
ON DUPLICATE KEY UPDATE `Name` = `Name`;

-- Módulos del sistema
INSERT INTO `Modules` (`Code`, `Name`, `Description`, `Icon`, `MenuOrder`, `IsEnabled`, `IsSystem`) VALUES
('timesheet', 'Registro de Horas', 'Registro de horas trabajadas', 'clock', 1, TRUE, TRUE),
('reports', 'Reportes', 'Visualización y exportación de reportes', 'chart', 2, TRUE, TRUE),
('projects', 'Proyectos', 'Gestión de proyectos y actividades', 'folder', 3, TRUE, TRUE),
('users', 'Usuarios', 'Gestión de usuarios y roles', 'users', 4, TRUE, TRUE),
('settings', 'Configuración', 'Configuración del sistema', 'settings', 5, TRUE, TRUE),
('tickets', 'Soporte', 'Sistema de tickets de soporte', 'help-circle', 6, TRUE, TRUE)
ON DUPLICATE KEY UPDATE `Code` = `Code`;

-- Permisos básicos por módulo
-- Timesheet
INSERT INTO `Permissions` (`ModuleId`, `Code`, `Name`, `Description`)
SELECT m.Id, 'create', 'Crear registro', 'Crear nuevo registro de horas'
FROM `Modules` m WHERE m.Code = 'timesheet'
ON DUPLICATE KEY UPDATE `Name` = `Name`;

INSERT INTO `Permissions` (`ModuleId`, `Code`, `Name`, `Description`)
SELECT m.Id, 'read', 'Ver registros', 'Ver registros de horas'
FROM `Modules` m WHERE m.Code = 'timesheet'
ON DUPLICATE KEY UPDATE `Name` = `Name`;

INSERT INTO `Permissions` (`ModuleId`, `Code`, `Name`, `Description`)
SELECT m.Id, 'update', 'Editar registro', 'Editar registro de horas'
FROM `Modules` m WHERE m.Code = 'timesheet'
ON DUPLICATE KEY UPDATE `Name` = `Name`;

INSERT INTO `Permissions` (`ModuleId`, `Code`, `Name`, `Description`)
SELECT m.Id, 'delete', 'Eliminar registro', 'Eliminar registro de horas'
FROM `Modules` m WHERE m.Code = 'timesheet'
ON DUPLICATE KEY UPDATE `Name` = `Name`;

INSERT INTO `Permissions` (`ModuleId`, `Code`, `Name`, `Description`)
SELECT m.Id, 'approve', 'Aprobar registro', 'Aprobar registros de horas'
FROM `Modules` m WHERE m.Code = 'timesheet'
ON DUPLICATE KEY UPDATE `Name` = `Name`;

-- Configuraciones por defecto
INSERT INTO `CompanySettings` (`Key`, `Value`, `Description`, `Category`) VALUES
('timesheet.max_days_to_register', '{"value": 2}', 'Días hábiles máximos para registrar horas', 'timesheet'),
('timesheet.max_days_to_close', '{"value": 5}', 'Días hábiles máximos para cerrar período', 'timesheet'),
('timesheet.block_after_days', '{"value": 10}', 'Bloquear registro después de X días hábiles', 'timesheet'),
('timesheet.default_hours_per_day', '{"value": 9.0}', 'Horas por defecto por día', 'timesheet'),
('timesheet.min_hours_fraction', '{"value": 0.25}', 'Fracción mínima de horas', 'timesheet'),
('security.session_timeout_minutes', '{"value": 30}', 'Timeout de sesión en minutos', 'security'),
('security.password_min_length', '{"value": 8}', 'Longitud mínima de contraseña', 'security'),
('notifications.omission_day_6', '{"enabled": true}', 'Notificación de omiso día 6', 'notifications'),
('notifications.omission_day_8', '{"enabled": true}', 'Notificación de omiso día 8', 'notifications'),
('notifications.omission_day_10', '{"enabled": true}', 'Notificación de omiso día 10', 'notifications')
ON DUPLICATE KEY UPDATE `Key` = `Key`;

-- Asignar permisos al rol SuperAdmin (todos los permisos)
INSERT INTO `RolePermissions` (`RoleId`, `PermissionId`)
SELECT r.Id, p.Id
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.Name = 'SuperAdmin'
ON DUPLICATE KEY UPDATE `RoleId` = `RoleId`;

-- Asignar permisos al rol Employee (solo crear y ver sus propios registros)
INSERT INTO `RolePermissions` (`RoleId`, `PermissionId`)
SELECT r.Id, p.Id
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.Name = 'Employee' AND p.Code IN ('create', 'read')
ON DUPLICATE KEY UPDATE `RoleId` = `RoleId`;

-- Asignar permisos al rol Supervisor (crear, leer, actualizar, aprobar)
INSERT INTO `RolePermissions` (`RoleId`, `PermissionId`)
SELECT r.Id, p.Id
FROM `Roles` r
CROSS JOIN `Permissions` p
WHERE r.Name = 'Supervisor' AND p.Code IN ('create', 'read', 'update', 'approve')
ON DUPLICATE KEY UPDATE `RoleId` = `RoleId`;

