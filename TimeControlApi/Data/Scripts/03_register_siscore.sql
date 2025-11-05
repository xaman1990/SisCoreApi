-- =====================================================
-- Script para Registrar SisCore en la BD Maestra
-- =====================================================
-- Ejecutar este script después de crear la BD maestra
-- y asegurarse de que la BD SisCore existe

USE timecontrol_master;

-- Registrar la empresa SisCore
-- IMPORTANTE: Actualizar la contraseña de MySQL, IP y puerto según tu configuración

-- Opción 1: BD en localhost (puerto por defecto 3306)
INSERT INTO Companies (Name, Subdomain, DbHost, DbPort, DbName, DbUser, DbPassword, ConnectionOptions, Status)
VALUES (
    'SisCore',
    'siscore',
    'localhost',        -- O tu IP: '192.168.1.100'
    NULL,               -- NULL = usar puerto por defecto 3306, o especifica: 3306, 3307, etc.
    'SisCore',
    'root',
    'tu_password_mysql',  -- CAMBIAR POR TU CONTRASEÑA DE MYSQL
    'SslMode=None',     -- Opciones adicionales: 'SslMode=None;ConnectionTimeout=30;CharSet=utf8mb4'
    1
)
ON DUPLICATE KEY UPDATE 
    Name = 'SisCore',
    DbHost = 'localhost',
    DbPort = NULL,
    DbName = 'SisCore',
    DbUser = 'root',
    DbPassword = 'tu_password_mysql',  -- CAMBIAR POR TU CONTRASEÑA DE MYSQL
    ConnectionOptions = 'SslMode=None',
    Status = 1;

-- Opción 2: BD en IP remota con puerto personalizado (ejemplo)
-- Descomenta y ajusta según tu configuración:
/*
INSERT INTO Companies (Name, Subdomain, DbHost, DbPort, DbName, DbUser, DbPassword, ConnectionOptions, Status)
VALUES (
    'SisCore',
    'siscore',
    '192.168.1.100',    -- IP del servidor MySQL
    3307,               -- Puerto personalizado (si no es 3306)
    'SisCore',
    'root',
    'tu_password_mysql',
    'SslMode=None;ConnectionTimeout=30',
    1
)
ON DUPLICATE KEY UPDATE 
    Name = 'SisCore',
    DbHost = '192.168.1.100',
    DbPort = 3307,
    DbName = 'SisCore',
    DbUser = 'root',
    DbPassword = 'tu_password_mysql',
    ConnectionOptions = 'SslMode=None;ConnectionTimeout=30',
    Status = 1;
*/

-- Verificar que se registró correctamente
SELECT * FROM Companies WHERE Subdomain = 'siscore';

