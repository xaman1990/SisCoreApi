using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeControlApi.Data;
using TimeControlApi.Domain.Master;

namespace TimeControlApi.Tenancy
{
    public class TenantResolver : ITenantResolver
    {
        private readonly MasterDbContext _master;
        private readonly IConfiguration _cfg;
        private readonly ILogger<TenantResolver>? _logger;

        public TenantResolver(MasterDbContext master, IConfiguration cfg, ILogger<TenantResolver>? logger = null)
        {
            _master = master;
            _cfg = cfg;
            _logger = logger;
        }

        public async Task<TenantContext?> ResolveAsync(HttpContext http)
        {
            var header = _cfg["Tenancy:HeaderName"] ?? "X-Tenant";
            string? subdomain = http.Request.Headers[header].FirstOrDefault();

            // Log para debugging
            _logger?.LogDebug("Resolviendo tenant. Header '{Header}': '{Value}'", header, subdomain);

            // Si no hay header, intentar con query parameter (útil para Swagger/testing)
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                subdomain = http.Request.Query["tenant"].FirstOrDefault();
                _logger?.LogDebug("No se encontró header, intentando query parameter 'tenant': '{Value}'", subdomain);
            }

            if (string.IsNullOrWhiteSpace(subdomain))
            {
                var host = http.Request.Host.Host; // ej: acme.localhost
                var defaultDomain = _cfg["Tenancy:DefaultDomain"] ?? "localhost";
                _logger?.LogDebug("No se encontró header, intentando por subdominio. Host: '{Host}', DefaultDomain: '{DefaultDomain}'", host, defaultDomain);
                
                if (host.EndsWith(defaultDomain, StringComparison.OrdinalIgnoreCase))
                {
                    var part = host[..^(defaultDomain.Length)];
                    subdomain = part.TrimEnd('.'); // "acme"
                    _logger?.LogDebug("Subdominio extraído: '{Subdomain}'", subdomain);
                }
            }

            if (string.IsNullOrWhiteSpace(subdomain))
            {
                _logger?.LogWarning("No se pudo determinar el subdominio del tenant");
                return null;
            }

            // Buscar empresa (comparación case-insensitive)
            var company = await _master.Companies.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Subdomain.ToLower() == subdomain.ToLower() && c.Status == 1);

            if (company == null)
            {
                _logger?.LogWarning("No se encontró empresa con subdominio '{Subdomain}'", subdomain);
                // Log todas las empresas disponibles para debugging
                var allCompanies = await _master.Companies.AsNoTracking()
                    .Where(c => c.Status == 1)
                    .Select(c => c.Subdomain)
                    .ToListAsync();
                _logger?.LogDebug("Empresas disponibles: {Companies}", string.Join(", ", allCompanies));
                return null;
            }

            _logger?.LogDebug("Tenant resuelto: {CompanyName} (ID: {CompanyId}, BD: {DbName}, Host: {DbHost}, Port: {DbPort}, HasPort: {HasPort})", 
                company.Name, company.Id, company.DbName, company.DbHost, company.DbPort ?? 3306, company.DbPort.HasValue);

            // Construir connection string con IP, puerto y opciones
            var connectionString = BuildConnectionString(company);
            
            return new TenantContext { CompanyId = company.Id, ConnectionString = connectionString, Subdomain = subdomain };
        }

        private string BuildConnectionString(Company company)
        {
            var parts = new List<string>
            {
                $"Server={company.DbHost}"
            };

            // Agregar puerto si está especificado
            // Si es NULL, MySQL usará el puerto por defecto 3306
            // IMPORTANTE: Siempre agregar el puerto si está especificado, incluso si es 3306
            // Esto asegura que la conexión use el puerto correcto cuando hay múltiples instancias
            if (company.DbPort.HasValue)
            {
                parts.Add($"Port={company.DbPort.Value}");
                _logger?.LogDebug("Puerto especificado en connection string: {Port}", company.DbPort.Value);
            }
            else
            {
                // Si no se especifica puerto, MySQL usará 3306 por defecto
                // No agregamos el puerto explícitamente en este caso
                _logger?.LogDebug("Puerto no especificado, MySQL usará puerto por defecto 3306");
            }

            parts.Add($"Database={company.DbName}");
            parts.Add($"User={company.DbUser}");
            parts.Add($"Password={company.DbPassword}");

            // Agregar opciones adicionales si están especificadas
            if (!string.IsNullOrWhiteSpace(company.ConnectionOptions))
            {
                // Si las opciones vienen como "SslMode=None;ConnectionTimeout=30"
                var options = company.ConnectionOptions.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var option in options)
                {
                    var trimmed = option.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        parts.Add(trimmed);
                    }
                }
                _logger?.LogDebug("Opciones de conexión agregadas: {Options}", company.ConnectionOptions);
            }
            else
            {
                // Opciones por defecto si no se especifican
                parts.Add("SslMode=None");
            }

            var connectionString = string.Join(";", parts);
            _logger?.LogDebug("Connection string construida: {ConnectionString}", 
                connectionString.Replace(company.DbPassword, "***"));

            return connectionString;
        }
    }
}
