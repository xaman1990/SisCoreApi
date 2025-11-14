using Microsoft.EntityFrameworkCore;
using SisCoreBackEnd.Data;

namespace SisCoreBackEnd.Tenancy
{
    public class TenantDbContextFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public TenantDbContextFactory(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public TenantDbContext CreateDbContext()
        {
            var tenantContext = _httpContextAccessor.HttpContext?.GetTenant();
            
            if (tenantContext == null)
            {
                throw new InvalidOperationException("Tenant context not found. Asegúrate de incluir el header 'X-Tenant' en la petición.");
            }

            var connectionString = tenantContext.ConnectionString 
                ?? throw new InvalidOperationException("No se pudo determinar la cadena de conexión del tenant");

            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            
            // NO usar ServerVersion.AutoDetect() porque abre una conexión que puede interferir
            // Usar una versión por defecto compatible con MySQL 5.7+ y 8.0+
            // Si necesitas una versión específica, puedes usar ServerVersion.Parse("8.0.33-mysql")
            var serverVersion = ServerVersion.Parse("8.0.33-mysql");

            optionsBuilder.UseMySql(connectionString, serverVersion);

            // Crear contexto sin validar inmediatamente
            // La validación se hará cuando se use el contexto por primera vez
            var dbContext = new TenantDbContext(optionsBuilder.Options);
            
            return dbContext;
        }

        private string GetHostFromConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';');
            var serverPart = parts.FirstOrDefault(p => p.StartsWith("Server=", StringComparison.OrdinalIgnoreCase));
            return serverPart?.Substring(7) ?? "unknown";
        }

        private string GetDatabaseFromConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';');
            var dbPart = parts.FirstOrDefault(p => p.StartsWith("Database=", StringComparison.OrdinalIgnoreCase));
            return dbPart?.Substring(9) ?? "unknown";
        }

        private string GetPortFromConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';');
            var portPart = parts.FirstOrDefault(p => p.StartsWith("Port=", StringComparison.OrdinalIgnoreCase));
            return portPart != null ? portPart.Substring(5) : "3306 (por defecto)";
        }
    }
}
