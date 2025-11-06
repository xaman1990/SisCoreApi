-- =====================================================
-- Script para Corregir la Configuración de Conexión de SisCore
-- =====================================================
-- Basado en el error: Access denied for user 'timecontrol'@'10.211.55.%' to database 'siscore'
-- 
-- Problemas detectados:
-- 1. El usuario en la BD maestra es 'timecontrol' pero necesita permisos
-- 2. El nombre de la BD puede estar en minúsculas ('siscore') en lugar de 'SisCore'
-- 3. Necesita verificar/actualizar la configuración

USE timecontrol_master;

-- Ver configuración actual
SELECT 
    Id,
    Name,
    Subdomain,
    DbHost,
    DbPort,
    DbName,
    DbUser,
    Status
FROM Companies 
WHERE Subdomain = 'siscore';

-- =====================================================
-- OPCIÓN 1: Usar usuario root (si tiene acceso)
-- =====================================================
UPDATE Companies 
SET 
    DbHost = '10.211.55.2',
    DbPort = 3306,  -- O tu puerto si no es 3306
    DbName = 'SisCore',  -- IMPORTANTE: Nombre exacto de la BD (puede ser case-sensitive)
    DbUser = 'root',  -- O el usuario que tenga permisos
    DbPassword = 'tu_password_root',  -- Contraseña del usuario root
    ConnectionOptions = 'SslMode=None;ConnectionTimeout=30'
WHERE Subdomain = 'siscore';

-- =====================================================
-- OPCIÓN 2: Dar permisos al usuario 'timecontrol'
-- =====================================================
-- Si prefieres usar el usuario 'timecontrol', ejecuta esto en el servidor MySQL:
/*
-- Conectar al servidor MySQL en 10.211.55.2
USE SisCore;

-- Dar permisos al usuario timecontrol
GRANT ALL PRIVILEGES ON SisCore.* TO 'timecontrol'@'10.211.55.%' IDENTIFIED BY 'tu_password';
FLUSH PRIVILEGES;

-- O si el usuario ya existe:
GRANT ALL PRIVILEGES ON SisCore.* TO 'timecontrol'@'%';
FLUSH PRIVILEGES;
*/

-- =====================================================
-- OPCIÓN 3: Crear usuario específico para SisCore
-- =====================================================
/*
-- En el servidor MySQL:
CREATE USER 'siscore_user'@'%' IDENTIFIED BY 'password_segura';
GRANT ALL PRIVILEGES ON SisCore.* TO 'siscore_user'@'%';
FLUSH PRIVILEGES;

-- Luego actualizar en la BD maestra:
UPDATE Companies 
SET 
    DbUser = 'siscore_user',
    DbPassword = 'password_segura'
WHERE Subdomain = 'siscore';
*/

-- =====================================================
-- VERIFICAR DESPUÉS DE ACTUALIZAR
-- =====================================================
SELECT 
    Id,
    Name,
    Subdomain,
    DbHost,
    DbPort,
    DbName,
    DbUser,
    ConnectionOptions
FROM Companies 
WHERE Subdomain = 'siscore';

