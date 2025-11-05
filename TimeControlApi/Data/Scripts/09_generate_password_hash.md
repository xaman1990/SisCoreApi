# Generar Hash de Contraseña para Usuario Admin

## Problema

El hash de Argon2id debe generarse usando el servicio `PasswordService` del backend. No se puede generar directamente desde SQL.

## Soluciones

### Opción 1: Usar el Endpoint de Registro (Recomendado)

Si el endpoint de registro está disponible, úsalo directamente:

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

### Opción 2: Generar Hash desde C# (Para Scripts)

Si necesitas generar el hash desde código C#, puedes crear un pequeño script:

```csharp
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

var password = "Admin123!";

using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
{
    Salt = CreateSalt(),
    DegreeOfParallelism = 8,
    Iterations = 4,
    MemorySize = 1024 * 1024 // 1 GB
};

var hash = argon2.GetBytes(32);
var salt = argon2.Salt!;

var hashString = $"$argon2id$v=19$m=1048576,t=4,p=8${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
Console.WriteLine(hashString);

static byte[] CreateSalt()
{
    var salt = new byte[16];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(salt);
    return salt;
}
```

### Opción 3: Usar el Servicio del Backend Temporalmente

1. Ejecuta el proyecto
2. Crea un endpoint temporal o usa el endpoint de registro
3. Obtén el hash generado
4. Actualiza el usuario en la BD:

```sql
USE SisCore;

UPDATE `Users` 
SET `PasswordHash` = 'HASH_GENERADO_DEL_BACKEND'
WHERE `Email` = 'admin@timecontrol.com';
```

### Opción 4: Usar un Script C# de Consola

Crea un archivo `GeneratePasswordHash.cs`:

```csharp
// dotnet run GeneratePasswordHash.cs
using System;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

string password = "Admin123!";

byte[] salt = new byte[16];
using (var rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(salt);
}

using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
{
    Salt = salt,
    DegreeOfParallelism = 8,
    Iterations = 4,
    MemorySize = 1024 * 1024
};

var hash = argon2.GetBytes(32);
var hashString = $"$argon2id$v=19$m=1048576,t=4,p=8${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";

Console.WriteLine($"Hash generado para '{password}':");
Console.WriteLine(hashString);
```

Ejecuta:
```bash
dotnet run GeneratePasswordHash.cs
```

Luego copia el hash y actualiza el usuario en la BD.

## Actualizar el Usuario en la BD

Una vez que tengas el hash correcto:

```sql
USE SisCore;

UPDATE `Users` 
SET 
    `PasswordHash` = 'TU_HASH_GENERADO_AQUI',
    `UpdatedAt` = NOW()
WHERE `Email` = 'admin@timecontrol.com';
```

## Verificar

Después de actualizar el hash, prueba el login:

```bash
POST https://localhost:7004/api/auth/login?tenant=siscore
Content-Type: application/json

{
  "email": "admin@timecontrol.com",
  "password": "Admin123!"
}
```

