-- =============================================================================
-- Script SQL para crear tablas de Módulos y Permisos
-- Base de datos: siscore (tenant database)
-- =============================================================================

USE siscore;

-- -----------------------------------------------------------------------------
-- Tabla: SubModules
-- Descripción: Submódulos dentro de un módulo para organización jerárquica
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS SubModules (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ModuleId INT NOT NULL,
    Code VARCHAR(50) NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    MenuOrder INT NOT NULL DEFAULT 0,
    IsEnabled BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedBy INT,
    UpdatedAt DATETIME,
    UpdatedBy INT,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT FK_SubModules_Modules FOREIGN KEY (ModuleId) REFERENCES Modules(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Índice único para ModuleId y Code
CREATE UNIQUE INDEX IF NOT EXISTS IX_SubModules_ModuleId_Code ON SubModules (ModuleId, Code);

-- -----------------------------------------------------------------------------
-- Tabla: ModulePrivileges
-- Descripción: Asociación de permisos del catálogo a módulos específicos
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ModulePrivileges (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ModuleId INT NOT NULL,
    PermissionId INT NOT NULL,
    SubModuleId INT,
    Code VARCHAR(100) NOT NULL,
    Name VARCHAR(150) NOT NULL,
    Description VARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedBy INT,
    UpdatedAt DATETIME,
    UpdatedBy INT,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    IsDefault BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT FK_ModulePrivileges_Modules FOREIGN KEY (ModuleId) REFERENCES Modules(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ModulePrivileges_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ModulePrivileges_SubModules FOREIGN KEY (SubModuleId) REFERENCES SubModules(Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE UNIQUE INDEX IF NOT EXISTS IX_ModulePrivileges_Module_Permission ON ModulePrivileges (ModuleId, PermissionId);
CREATE INDEX IF NOT EXISTS IX_ModulePrivileges_Module_Code ON ModulePrivileges (ModuleId, Code);

-- -----------------------------------------------------------------------------
-- Tabla: Permissions
-- Descripción: Catálogo global de permisos reutilizables
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Permissions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Code VARCHAR(100) NOT NULL UNIQUE,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    IsSystem BOOLEAN NOT NULL DEFAULT FALSE,
    IsDefaultForModule BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedBy INT,
    UpdatedAt DATETIME,
    UpdatedBy INT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Tabla: PermissionAssignments
-- Descripción: Asignación de permisos a roles o usuarios
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PermissionAssignments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ModulePrivilegeId INT NOT NULL,
    RoleId INT,
    UserId INT,
    GrantedBy INT,
    GrantedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ValidFrom DATETIME NOT NULL,
    ValidTo DATETIME,
    IsInherited BOOLEAN NOT NULL DEFAULT FALSE,
    OverrideParentId INT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedBy INT,
    UpdatedAt DATETIME ,
    UpdatedBy INT,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT FK_PermissionAssignments_ModulePrivileges FOREIGN KEY (ModulePrivilegeId) REFERENCES ModulePrivileges(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PermissionAssignments_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PermissionAssignments_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PermissionAssignments_OverrideParent FOREIGN KEY (OverrideParentId) REFERENCES PermissionAssignments(Id) ON DELETE SET NULL,
    CONSTRAINT CHK_PermissionAssignments_RoleOrUser CHECK (RoleId IS NOT NULL OR UserId IS NOT NULL)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Índice compuesto para búsquedas
CREATE INDEX IF NOT EXISTS IX_PermissionAssignments_ModulePrivilegeId_RoleId_UserId ON PermissionAssignments (ModulePrivilegeId, RoleId, UserId);

-- =============================================================================
-- Notas
-- =============================================================================
-- Adapta este script según tus necesidades antes de desplegarlo en producción.