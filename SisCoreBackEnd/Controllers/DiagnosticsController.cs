using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeControlApi.Data;
using TimeControlApi.Services;
using TimeControlApi.Tenancy;

namespace TimeControlApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class DiagnosticsController : ControllerBase
    {
        private readonly MasterDbContext _masterDb;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(
            MasterDbContext masterDb,
            IConfiguration configuration,
            ILogger<DiagnosticsController> logger)
        {
            _masterDb = masterDb;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint de diagnóstico para verificar la configuración del tenant
        /// </summary>
        [HttpGet("tenant")]
        public async Task<IActionResult> GetTenantDiagnostics()
        {
            try
            {
                var headerName = _configuration["Tenancy:HeaderName"] ?? "X-Tenant";
                var tenantHeader = Request.Headers[headerName].FirstOrDefault();
                var tenantQueryParam = Request.Query["tenant"].FirstOrDefault();
                var host = Request.Host.Host;
                var defaultDomain = _configuration["Tenancy:DefaultDomain"] ?? "localhost";

                // Obtener tenant context
                var tenantContext = HttpContext.GetTenant();

                // Obtener todas las empresas
                var companies = await _masterDb.Companies
                    .Where(c => c.Status == 1)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Subdomain,
                        c.DbName,
                        c.DbHost,
                        c.DbPort,
                        c.ConnectionOptions,
                        c.Status
                    })
                    .ToListAsync();

                return Ok(new
                {
                    HeaderName = headerName,
                    TenantHeaderValue = tenantHeader,
                    TenantQueryParam = tenantQueryParam,
                    Host = host,
                    DefaultDomain = defaultDomain,
                    TenantContext = tenantContext != null ? new
                    {
                        tenantContext.CompanyId,
                        tenantContext.Subdomain,
                        HasConnectionString = !string.IsNullOrEmpty(tenantContext.ConnectionString),
                        ConnectionStringPreview = tenantContext.ConnectionString != null 
                            ? tenantContext.ConnectionString.Replace("Password=", "Password=***").Substring(0, Math.Min(200, tenantContext.ConnectionString.Length))
                            : null
                    } : null,
                    Companies = companies,
                    Message = tenantContext == null 
                        ? $"No se pudo resolver el tenant. Verifica que el header '{headerName}' esté presente O que el query parameter 'tenant' esté presente. Ejemplo: ?tenant=siscore"
                        : "Tenant resuelto correctamente",
                    Instructions = tenantContext == null ? new
                    {
                        Option1 = $"Agregar header: {headerName}: siscore",
                        Option2 = "Agregar query parameter: ?tenant=siscore",
                        Option3 = "Usar subdominio: siscore.localhost:7004"
                    } : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en diagnóstico de tenant");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Verificar conexión a la BD maestra
        /// </summary>
        [HttpGet("master-db")]
        public async Task<IActionResult> CheckMasterDb()
        {
            try
            {
                var companiesCount = await _masterDb.Companies.CountAsync();
                var companies = await _masterDb.Companies
                    .Select(c => new { c.Id, c.Name, c.Subdomain, c.DbName, c.Status })
                    .ToListAsync();

                return Ok(new
                {
                    Connected = true,
                    CompaniesCount = companiesCount,
                    Companies = companies
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con BD maestra");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Generar hash de contraseña (SOLO PARA DESARROLLO - ELIMINAR EN PRODUCCIÓN)
        /// </summary>
        [HttpPost("generate-password-hash")]
        [AllowAnonymous]
        public IActionResult GeneratePasswordHash([FromBody] GeneratePasswordHashRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { message = "La contraseña es requerida" });
                }

                var passwordService = new PasswordService();
                var hash = passwordService.HashPassword(request.Password);

                return Ok(new
                {
                    Password = request.Password,
                    Hash = hash,
                    HashLength = hash.Length,
                    Instructions = "Copia el hash y actualiza el usuario en la BD con: UPDATE Users SET PasswordHash = 'HASH_AQUI' WHERE Email = 'admin@timecontrol.com';"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar hash de contraseña");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class GeneratePasswordHashRequest
    {
        public string Password { get; set; } = string.Empty;
    }
}

