using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace SisCoreBackEnd.Services
{
    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = CreateSalt(),
                DegreeOfParallelism = 8,
                Iterations = 4,
                MemorySize = 1024 * 1024 // 1 GB
            };

            var hash = argon2.GetBytes(32);
            var salt = argon2.Salt!;

            // Combinar salt y hash en formato: $argon2id$v=19$m=1048576,t=4,p=8$salt$hash
            return $"$argon2id$v=19$m=1048576,t=4,p=8${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                // Validar que el hash no esté vacío
                if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(password))
                    return false;

                // Parsear el hash
                var parts = hash.Split('$');
                
                // El formato esperado es: $argon2id$v=19$m=1048576,t=4,p=8$salt$hash
                // Después de Split('$') debería tener 6 partes: ["", "argon2id", "v=19", "m=1048576,t=4,p=8", "salt", "hash"]
                if (parts.Length != 6 || parts[1] != "argon2id")
                {
                    // Log para debugging (en producción usar ILogger)
                    System.Diagnostics.Debug.WriteLine($"Hash format invalid. Expected 6 parts, got {parts.Length}. Hash: {hash.Substring(0, Math.Min(50, hash.Length))}...");
                    return false;
                }

                // Validar que salt y hash no estén vacíos
                if (string.IsNullOrWhiteSpace(parts[4]) || string.IsNullOrWhiteSpace(parts[5]))
                {
                    System.Diagnostics.Debug.WriteLine("Salt or hash is empty in the password hash string.");
                    return false;
                }

                // Detectar placeholders/dummy hashes
                if (parts[4].Contains("placeholder", StringComparison.OrdinalIgnoreCase) || 
                    parts[4].Contains("dummy", StringComparison.OrdinalIgnoreCase) ||
                    parts[5].Contains("placeholder", StringComparison.OrdinalIgnoreCase) || 
                    parts[5].Contains("dummy", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Hash contains placeholder/dummy values. Salt: {parts[4]}, Hash: {parts[5]}. The hash must be regenerated using PasswordService.HashPassword().");
                    return false;
                }

                // Decodificar salt y hash desde Base64
                byte[] salt;
                byte[] storedHash;

                try
                {
                    salt = Convert.FromBase64String(parts[4]);
                }
                catch (FormatException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid Base64 salt. Salt part: '{parts[4]}'. This is not a valid Base64 string. Error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine("The hash in the database appears to be a placeholder. You need to generate a real hash using PasswordService.HashPassword() and update the database.");
                    return false;
                }

                try
                {
                    storedHash = Convert.FromBase64String(parts[5]);
                }
                catch (FormatException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid Base64 hash. Hash part: '{parts[5]}'. This is not a valid Base64 string. Error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine("The hash in the database appears to be a placeholder. You need to generate a real hash using PasswordService.HashPassword() and update the database.");
                    return false;
                }

                // Validar que salt tenga el tamaño esperado (16 bytes)
                if (salt.Length != 16)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid salt length. Expected 16 bytes, got {salt.Length}.");
                    return false;
                }

                // Validar que hash tenga el tamaño esperado (32 bytes)
                if (storedHash.Length != 32)
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid hash length. Expected 32 bytes, got {storedHash.Length}.");
                    return false;
                }

                // Crear hash con el mismo salt
                using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
                {
                    Salt = salt,
                    DegreeOfParallelism = 8,
                    Iterations = 4,
                    MemorySize = 1024 * 1024
                };

                var computedHash = argon2.GetBytes(32);

                // Comparar hashes
                return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
            }
            catch (Exception ex)
            {
                // Log el error completo para debugging
                System.Diagnostics.Debug.WriteLine($"Error verifying password: {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        private static byte[] CreateSalt()
        {
            var salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }
    }
}

