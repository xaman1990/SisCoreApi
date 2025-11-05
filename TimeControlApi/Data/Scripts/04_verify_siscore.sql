-- =====================================================
-- Script para Verificar la Configuración de SisCore
-- =====================================================
-- Ejecuta este script para verificar que todo está configurado correctamente

-- 1. Verificar BD Maestra
USE timecontrol_master;

SELECT '=== BD MAESTRA ===' AS Info;
SELECT 'Total de empresas registradas:' AS Info, COUNT(*) AS Count FROM Companies;
SELECT 'Empresas activas:' AS Info, COUNT(*) AS Count FROM Companies WHERE Status = 1;

-- 2. Verificar registro de SisCore
SELECT '=== REGISTRO DE SISCORE ===' AS Info;
SELECT 
    Id,
    Name,
    Subdomain,
    DbHost,
    DbName,
    DbUser,
    Status,
    CreatedAt
FROM Companies 
WHERE Subdomain = 'siscore' OR Name LIKE '%SisCore%' OR DbName = 'SisCore';

-- 3. Verificar que la BD SisCore existe (esto debe ejecutarse manualmente)
-- USE SisCore;
-- SHOW TABLES;

-- 4. Si no aparece SisCore, ejecuta este INSERT
SELECT '=== SI NO APARECE SISCORE, EJECUTA ESTE INSERT ===' AS Info;
-- Descomenta y actualiza la contraseña:
/*
INSERT INTO Companies (Name, Subdomain, DbHost, DbName, DbUser, DbPassword, Status)
VALUES (
    'SisCore',
    'siscore',
    'localhost',
    'SisCore',
    'root',
    'tu_password_mysql',  -- CAMBIAR
    1
);
*/

