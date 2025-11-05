-- =====================================================
-- Script para Actualizar Hash de Usuario Admin
-- =====================================================
-- Este script actualiza el hash de contraseña del usuario admin
-- 
-- PASO 1: Genera el hash usando el endpoint:
-- POST https://localhost:7004/api/diagnostics/generate-password-hash
-- Body: { "password": "Admin123!" }
--
-- PASO 2: Copia el hash generado y reemplaza 'TU_HASH_AQUI' abajo
--
-- PASO 3: Ejecuta este script
--
-- =====================================================

USE SisCore;

-- Actualizar el hash del usuario admin
-- IMPORTANTE: Reemplaza 'TU_HASH_AQUI' con el hash generado por el endpoint
UPDATE `Users` 
SET 
    `PasswordHash` = 'TU_HASH_AQUI',  -- <-- Pega el hash aquí
    `UpdatedAt` = NOW()
WHERE `Email` = 'admin@timecontrol.com';

-- Verificar que se actualizó correctamente
SELECT 
    `Id`,
    `Email`,
    `FullName`,
    CASE 
        WHEN `PasswordHash` LIKE '$argon2id$%' THEN 'OK - Hash válido'
        WHEN `PasswordHash` LIKE '%placeholder%' OR `PasswordHash` LIKE '%dummy%' THEN 'ERROR - Hash placeholder'
        ELSE 'VERIFICAR - Formato desconocido'
    END AS `EstadoHash`,
    LEFT(`PasswordHash`, 50) AS `InicioHash`,
    LENGTH(`PasswordHash`) AS `LongitudHash`,
    `UpdatedAt`
FROM `Users`
WHERE `Email` = 'admin@timecontrol.com';

