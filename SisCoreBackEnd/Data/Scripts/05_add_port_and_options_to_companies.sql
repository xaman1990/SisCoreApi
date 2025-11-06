-- =====================================================
-- Script de Migración: Agregar campos DbPort y ConnectionOptions
-- =====================================================
-- Ejecuta este script si ya tienes la BD maestra creada
-- para agregar los nuevos campos de configuración de conexión

USE timecontrol_master;

-- Agregar campo DbPort (puerto de MySQL)
ALTER TABLE `Companies` 
ADD COLUMN IF NOT EXISTS `DbPort` INT NULL COMMENT 'Puerto de MySQL (por defecto 3306 si es null)' AFTER `DbHost`;

-- Agregar campo ConnectionOptions (opciones adicionales)
ALTER TABLE `Companies` 
ADD COLUMN IF NOT EXISTS `ConnectionOptions` VARCHAR(500) NULL COMMENT 'Opciones adicionales de conexión (ej: SslMode=None;ConnectionTimeout=30)' AFTER `DbPassword`;

-- Actualizar registros existentes para usar puerto por defecto si es necesario
-- (Opcional: solo si quieres establecer un puerto específico para empresas existentes)
-- UPDATE `Companies` SET `DbPort` = 3306 WHERE `DbPort` IS NULL;

-- Verificar cambios
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'timecontrol_master' 
AND TABLE_NAME = 'Companies'
ORDER BY ORDINAL_POSITION;

