using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SisCoreBackEnd.Services;
using SisCoreBackEnd.DTOs.Modules;

namespace SisCoreBackEnd.Controllers
{
    /// <summary>
    /// Controlador para gestión de módulos del sistema
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ModulesController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<ModulesController> _logger;

        public ModulesController(IModuleService moduleService, IPermissionService permissionService, ILogger<ModulesController> logger)
        {
            _moduleService = moduleService;
            _permissionService = permissionService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los módulos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetModules()
        {
            try
            {
                var modules = await _moduleService.GetModulesAsync();
                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener módulo por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetModule(int id)
        {
            try
            {
                var module = await _moduleService.GetModuleByIdAsync(id);
                if (module == null)
                {
                    return NotFound(new { message = "Módulo no encontrado" });
                }

                return Ok(module);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crear nuevo módulo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateModule([FromBody] CreateModuleRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.Name))
                {
                    return BadRequest(new { message = "Code y Name son requeridos" });
                }
                if (request.MenuOrder < 0)
                {
                    return BadRequest(new { message = "MenuOrder debe ser mayor o igual a cero" });
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int? createdBy = null;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    createdBy = userId;
                }

                var module = await _moduleService.CreateModuleAsync(request, createdBy);
                await _permissionService.EnsureDefaultModulePrivilegesAsync(module.Id, createdBy);
                return CreatedAtAction(nameof(GetModule), new { id = module.Id }, new
                {
                    module.Id,
                    module.Code,
                    module.Name,
                    module.Description
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear módulo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Generar módulo con IA desde lenguaje natural
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateModule([FromBody] GenerateModuleRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Prompt))
                {
                    return BadRequest(new { message = "El prompt es requerido" });
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int? createdBy = null;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    createdBy = userId;
                }

                var module = await _moduleService.GenerateModuleWithAIAsync(request, createdBy);
                return Ok(new { message = "Módulo generado exitosamente", moduleId = module.Id });
            }
            catch (NotImplementedException ex)
            {
                return StatusCode(501, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar módulo con IA");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualizar módulo
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModule(int id, [FromBody] UpdateModuleRequest request)
        {
            try
            {
                if (request.MenuOrder.HasValue && request.MenuOrder.Value < 0)
                {
                    return BadRequest(new { message = "MenuOrder debe ser mayor o igual a cero" });
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int? updatedBy = null;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    updatedBy = userId;
                }

                var module = await _moduleService.UpdateModuleAsync(id, request, updatedBy);
                return Ok(new
                {
                    module.Id,
                    module.Code,
                    module.Name,
                    module.Description
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar módulo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Eliminar módulo (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModule(int id)
        {
            try
            {
                var result = await _moduleService.DeleteModuleAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Módulo no encontrado" });
                }

                return Ok(new { message = "Módulo eliminado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar módulo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener permisos de un módulo
        /// </summary>
        [HttpGet("{id}/permissions")]
        public async Task<IActionResult> GetModulePermissions(int id, [FromQuery] bool includeInherited = true, [FromQuery] int? userId = null)
        {
            try
            {
                var permissions = await _moduleService.GetModulePermissionsAsync(id, includeInherited, userId);
                return Ok(permissions);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del módulo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}

