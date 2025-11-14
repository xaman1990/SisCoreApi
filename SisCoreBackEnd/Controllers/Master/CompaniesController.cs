using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SisCoreBackEnd.Data;
using SisCoreBackEnd.Domain.Master;
using SisCoreBackEnd.DTOs.Companies;

namespace SisCoreBackEnd.Controllers.Master
{
    [ApiController]
    [Route("api/master/[controller]")]
    [Authorize] // Requiere autenticación de super usuario
    public class CompaniesController : ControllerBase
    {
        private readonly MasterDbContext _db;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(MasterDbContext db, ILogger<CompaniesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las empresas
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                var companies = await _db.Companies
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Subdomain,
                        c.DbHost,
                        c.DbPort,
                        c.DbName,
                        c.DbUser,
                        HasPassword = !string.IsNullOrEmpty(c.DbPassword),
                        c.ConnectionOptions,
                        c.Status,
                        c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empresas");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener empresa por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompany(int id)
        {
            try
            {
                var company = await _db.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = "Empresa no encontrada" });
                }

                return Ok(new
                {
                    company.Id,
                    company.Name,
                    company.Subdomain,
                    company.DbHost,
                    company.DbPort,
                    company.DbName,
                    company.DbUser,
                    HasPassword = !string.IsNullOrEmpty(company.DbPassword),
                    company.ConnectionOptions,
                    company.BrandingJson,
                    company.SettingsJson,
                    company.Status,
                    company.CreatedAt,
                    company.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empresa");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crear nueva empresa
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Subdomain))
                {
                    return BadRequest(new { message = "Nombre y subdominio son requeridos" });
                }

                // Verificar que el subdominio no exista
                var exists = await _db.Companies.AnyAsync(c => c.Subdomain == request.Subdomain);
                if (exists)
                {
                    return BadRequest(new { message = "El subdominio ya está en uso" });
                }

                var company = new Company
                {
                    Name = request.Name,
                    Subdomain = request.Subdomain,
                    DbHost = request.DbHost,
                    DbPort = request.DbPort,
                    DbName = request.DbName,
                    DbUser = request.DbUser,
                    DbPassword = request.DbPassword,
                    ConnectionOptions = request.ConnectionOptions,
                    BrandingJson = request.BrandingJson,
                    SettingsJson = request.SettingsJson,
                    Status = 1
                };

                _db.Companies.Add(company);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, new
                {
                    company.Id,
                    company.Name,
                    company.Subdomain,
                    company.DbHost,
                    company.DbPort,
                    company.DbName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear empresa");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualizar empresa
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyRequest request)
        {
            try
            {
                var company = await _db.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = "Empresa no encontrada" });
                }

                if (!string.IsNullOrEmpty(request.Name))
                    company.Name = request.Name;

                if (!string.IsNullOrEmpty(request.DbHost))
                    company.DbHost = request.DbHost;

                if (request.DbPort.HasValue)
                    company.DbPort = request.DbPort;

                if (!string.IsNullOrEmpty(request.DbName))
                    company.DbName = request.DbName;

                if (!string.IsNullOrEmpty(request.DbUser))
                    company.DbUser = request.DbUser;

                if (!string.IsNullOrEmpty(request.DbPassword))
                    company.DbPassword = request.DbPassword;

                if (request.ConnectionOptions != null)
                    company.ConnectionOptions = request.ConnectionOptions;

                if (request.BrandingJson != null)
                    company.BrandingJson = request.BrandingJson;

                if (request.SettingsJson != null)
                    company.SettingsJson = request.SettingsJson;

                if (request.Status.HasValue)
                    company.Status = request.Status.Value;

                company.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    company.Id,
                    company.Name,
                    company.Subdomain,
                    company.DbHost,
                    company.DbPort,
                    company.DbName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar empresa");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualizar configuración de conexión de una empresa
        /// </summary>
        [HttpPut("{id}/connection")]
        public async Task<IActionResult> UpdateConnection(int id, [FromBody] UpdateConnectionRequest request)
        {
            try
            {
                var company = await _db.Companies.FindAsync(id);
                if (company == null)
                {
                    return NotFound(new { message = "Empresa no encontrada" });
                }

                if (!string.IsNullOrEmpty(request.DbHost))
                    company.DbHost = request.DbHost;

                if (request.DbPort.HasValue)
                    company.DbPort = request.DbPort;

                if (!string.IsNullOrEmpty(request.DbName))
                    company.DbName = request.DbName;

                if (!string.IsNullOrEmpty(request.DbUser))
                    company.DbUser = request.DbUser;

                if (!string.IsNullOrEmpty(request.DbPassword))
                    company.DbPassword = request.DbPassword;

                if (request.ConnectionOptions != null)
                    company.ConnectionOptions = request.ConnectionOptions;

                company.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Configuración de conexión actualizada",
                    DbHost = company.DbHost,
                    DbPort = company.DbPort ?? 3306,
                    DbName = company.DbName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar configuración de conexión");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }

    public class UpdateConnectionRequest
    {
        public string? DbHost { get; set; }
        public int? DbPort { get; set; }
        public string? DbName { get; set; }
        public string? DbUser { get; set; }
        public string? DbPassword { get; set; }
        public string? ConnectionOptions { get; set; }
    }
}

