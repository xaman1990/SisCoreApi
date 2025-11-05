-- =====================================================
-- Script para Verificar Hash de Usuario
-- =====================================================
-- Este script ayuda a diagnosticar problemas con el hash de contraseña
-- Ejecuta este script para verificar el formato del hash almacenado

USE SisCore;

-- Ver el hash almacenado (primeros y últimos caracteres para no mostrar todo)
SELECT 
    `Id`,
    `Email`,
    `FullName`,
    CASE 
        WHEN `PasswordHash` IS NULL THEN 'NULL'
        WHEN LENGTH(`PasswordHash`) = 0 THEN 'VACÍO'
        WHEN `PasswordHash` NOT LIKE '$argon2id$%' THEN 'FORMATO INCORRECTO (no empieza con $argon2id$)'
        WHEN LENGTH(`PasswordHash`) < 100 THEN 'MUY CORTO (posiblemente truncado)'
        ELSE 'OK'
    END AS `EstadoHash`,
    LENGTH(`PasswordHash`) AS `LongitudHash`,
    LEFT(`PasswordHash`, 50) AS `InicioHash`,
    RIGHT(`PasswordHash`, 50) AS `FinHash`,
    `PasswordHash` AS `HashCompleto`  -- Comentar esta línea si no quieres ver el hash completo
FROM `Users`
WHERE `Email` = 'admin@timecontrol.com';

-- Verificar si el hash tiene caracteres inválidos para Base64
SELECT 
    `Email`,
    `PasswordHash`,
    CASE 
        WHEN `PasswordHash` LIKE '% %' THEN 'TIENE ESPACIOS'
        WHEN `PasswordHash` LIKE '%\n%' OR `PasswordHash` LIKE '%\r%' THEN 'TIENE SALTOS DE LÍNEA'
        WHEN `PasswordHash` LIKE '%\t%' THEN 'TIENE TABS'
        WHEN `PasswordHash` REGEXP '[^A-Za-z0-9+/=]' THEN 'TIENE CARACTERES INVÁLIDOS'
        ELSE 'OK'
    END AS `Problemas`
FROM `Users`
WHERE `Email` = 'admin@timecontrol.com';

-- Verificar el formato del hash (debe tener exactamente 6 partes separadas por $)
SELECT 
    `Email`,
    `PasswordHash`,
    (LENGTH(`PasswordHash`) - LENGTH(REPLACE(`PasswordHash`, '$', ''))) AS `NumeroDeSignosDollar`,
    CASE 
        WHEN (LENGTH(`PasswordHash`) - LENGTH(REPLACE(`PasswordHash`, '$', ''))) = 5 THEN 'OK (5 $ = 6 partes)'
        ELSE 'ERROR: Formato incorrecto'
    END AS `ValidacionFormato`
FROM `Users`
WHERE `Email` = 'admin@timecontrol.com';

