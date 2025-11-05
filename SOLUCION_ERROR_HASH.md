# Solución al Error de Hash Base64

## Error

```
System.FormatException: 'The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.'
```

## Causas Posibles

1. **Hash placeholder/dummy**: El hash en la BD es un placeholder (como `dummy_hash_placeholder`) que no es Base64 válido
2. **Hash truncado**: El hash fue cortado al guardarse en la BD
3. **Caracteres inválidos**: El hash contiene espacios, saltos de línea u otros caracteres inválidos
4. **Formato incorrecto**: El hash no sigue el formato esperado de Argon2id

## Solución

### Paso 1: Verificar el hash en la BD

Ejecuta el script de diagnóstico:

```sql
USE SisCore;

SELECT 
    `Email`,
    `PasswordHash`,
    LENGTH(`PasswordHash`) AS `Longitud`,
    LEFT(`PasswordHash`, 50) AS `Inicio`,
    RIGHT(`PasswordHash`, 50) AS `Fin`
FROM `Users`
WHERE `Email` = 'admin@timecontrol.com';
```

### Paso 2: Generar un hash correcto

El hash debe generarse usando el servicio `PasswordService` del backend. Tienes dos opciones:

#### Opción A: Usar el endpoint de registro

```bash
POST https://localhost:7004/api/auth/register?tenant=siscore
Content-Type: application/json

{
  "email": "admin@timecontrol.com",
  "password": "Admin123!",
  "fullName": "Administrador",
  "roleIds": [1]
}
```

#### Opción B: Generar hash manualmente y actualizar

1. **Crea un pequeño programa C#** para generar el hash:

```csharp
// GenerateHash.cs
using System;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

var password = "Admin123!";
var salt = new byte[16];
using (var rng = RandomNumberGenerator.Create())
    rng.GetBytes(salt);

using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
{
    Salt = salt,
    DegreeOfParallelism = 8,
    Iterations = 4,
    MemorySize = 1024 * 1024
};

var hash = argon2.GetBytes(32);
var hashString = $"$argon2id$v=19$m=1048576,t=4,p=8${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";

Console.WriteLine("Hash generado:");
Console.WriteLine(hashString);
Console.WriteLine();
Console.WriteLine("Longitud: " + hashString.Length);
```

2. **Ejecuta el programa** y copia el hash generado

3. **Actualiza el usuario en la BD**:

```sql
USE SisCore;

UPDATE `Users` 
SET 
    `PasswordHash` = 'HASH_GENERADO_AQUI',  -- Pega el hash completo aquí
    `UpdatedAt` = NOW()
WHERE `Email` = 'admin@timecontrol.com';
```

### Paso 3: Limpiar el hash si tiene caracteres inválidos

Si el hash tiene espacios o saltos de línea, puedes limpiarlo:

```sql
USE SisCore;

UPDATE `Users` 
SET 
    `PasswordHash` = TRIM(REPLACE(REPLACE(REPLACE(`PasswordHash`, '\n', ''), '\r', ''), ' ', '')),
    `UpdatedAt` = NOW()
WHERE `Email` = 'admin@timecontrol.com';
```

### Paso 4: Verificar que el hash sea válido

El hash debe tener este formato:

```
$argon2id$v=19$m=1048576,t=4,p=8$[SALT_BASE64]$[HASH_BASE64]
```

Ejemplo válido:
```
$argon2id$v=19$m=1048576,t=4,p=8$abcdefghijklmnop$ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/==
```

## Validaciones Agregadas

He mejorado el código de `PasswordService.VerifyPassword()` para:

1. ✅ Validar que el hash no esté vacío
2. ✅ Validar el formato del hash (debe tener 6 partes separadas por `$`)
3. ✅ Capturar errores específicos de Base64 con mensajes descriptivos
4. ✅ Validar que salt y hash tengan el tamaño correcto
5. ✅ Agregar logs de debug para identificar problemas

## Próximos Pasos

1. **Ejecuta el script de diagnóstico** (`10_verificar_hash_usuario.sql`) para ver el estado actual del hash
2. **Si el hash es un placeholder**, genera uno nuevo usando el backend
3. **Actualiza el hash** en la BD con el hash correcto
4. **Prueba el login** nuevamente

## Debugging

Si el error persiste, revisa los mensajes de debug en la consola. El código ahora mostrará:
- Qué parte del hash está mal (salt o hash)
- El valor exacto de la parte problemática
- El tipo de error específico

Esto te ayudará a identificar exactamente qué está mal con el hash almacenado.

