using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SisCoreBackEnd.DTOs.Permissions;
using SisCoreBackEnd.Services;

namespace SisCoreBackEnd.Controllers
{
    /// <summary>
    /// API para gestión de permisos y privilegios por módulo/rol.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return userId;
        }

        #region Effective permissions

        [HttpGet("me")]
        public async Task<IActionResult> GetMyPermissions()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Usuario no identificado" });
                }

                var permissions = await _permissionService.GetMyPermissionsAsync(userId.Value);
                return Ok(permissions);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del usuario actual");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            try
            {
                var response = await _permissionService.GetUserPermissionsAsync(userId, includeInherited: true);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckPermission([FromBody] CheckPermissionRequest request)
        {
            try
            {
                var userId = request.UserId ?? GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return BadRequest(new { message = "Debe proporcionar UserId o estar autenticado" });
                }

                var result = await _permissionService.CheckPermissionAsync(userId.Value, request.ModuleId, request.PermissionCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar permiso");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Permission catalog

        [HttpGet("catalog")]
        public async Task<IActionResult> GetPermissionCatalog([FromQuery] PermissionCatalogFilter filter)
        {
            try
            {
                var result = await _permissionService.GetPermissionCatalogAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener catálogo de permisos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("catalog/{id:int}")]
        public async Task<IActionResult> GetPermissionCatalogById(int id)
        {
            try
            {
                var permission = await _permissionService.GetPermissionCatalogByIdAsync(id);
                if (permission == null)
                    return NotFound(new { message = "Permiso no encontrado" });

                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permiso del catálogo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("catalog")]
        public async Task<IActionResult> CreatePermissionCatalog([FromBody] CreatePermissionRequest request)
        {
            try
            {
                var createdBy = GetCurrentUserId() ?? 0;
                var result = await _permissionService.CreatePermissionAsync(request, createdBy);
                return CreatedAtAction(nameof(GetPermissionCatalogById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear permiso de catálogo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("catalog/{id:int}")]
        public async Task<IActionResult> UpdatePermissionCatalog(int id, [FromBody] UpdatePermissionRequest request)
        {
            try
            {
                var updatedBy = GetCurrentUserId() ?? 0;
                var result = await _permissionService.UpdatePermissionAsync(id, request, updatedBy);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permiso de catálogo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("catalog/{id:int}")]
        public async Task<IActionResult> DeletePermissionCatalog(int id)
        {
            try
            {
                var deletedBy = GetCurrentUserId();
                var result = await _permissionService.DeletePermissionAsync(id, deletedBy);
                if (!result)
                    return NotFound(new { message = "Permiso no encontrado" });

                return Ok(new { message = "Permiso eliminado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar permiso de catálogo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Module privileges

        [HttpGet("modules/{moduleId:int}/privileges")]
        public async Task<IActionResult> GetModulePrivileges(int moduleId, [FromQuery] ModulePrivilegeFilter filter)
        {
            try
            {
                filter ??= new ModulePrivilegeFilter();
                filter.ModuleId = moduleId;
                var privileges = await _permissionService.GetModulePrivilegesAsync(moduleId, filter);
                return Ok(privileges);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener privilegios del módulo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("modules/{moduleId:int}/privileges/{privilegeId:int}")]
        public async Task<IActionResult> GetModulePrivilege(int moduleId, int privilegeId)
        {
            try
            {
                var privilege = await _permissionService.GetModulePrivilegeByIdAsync(privilegeId);
                if (privilege == null || privilege.ModuleId != moduleId)
                    return NotFound(new { message = "Privilegio no encontrado" });

                return Ok(privilege);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener privilegio del módulo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("modules/{moduleId:int}/privileges")]
        public async Task<IActionResult> CreateModulePrivilege(int moduleId, [FromBody] CreateModulePrivilegeRequest request)
        {
            try
            {
                var createdBy = GetCurrentUserId() ?? 0;
                var privilege = await _permissionService.CreateModulePrivilegeAsync(moduleId, request, createdBy);
                return CreatedAtAction(nameof(GetModulePrivilege), new { moduleId, privilegeId = privilege.Id }, privilege);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear privilegio de módulo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("modules/{moduleId:int}/privileges/{privilegeId:int}")]
        public async Task<IActionResult> UpdateModulePrivilege(int moduleId, int privilegeId, [FromBody] UpdateModulePrivilegeRequest request)
        {
            try
            {
                var updatedBy = GetCurrentUserId() ?? 0;
                var privilege = await _permissionService.UpdateModulePrivilegeAsync(privilegeId, request, updatedBy);
                if (privilege.ModuleId != moduleId)
                    return BadRequest(new { message = "El privilegio no pertenece al módulo indicado" });

                return Ok(privilege);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar privilegio de módulo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("modules/{moduleId:int}/privileges/{privilegeId:int}")]
        public async Task<IActionResult> DeleteModulePrivilege(int moduleId, int privilegeId)
        {
            try
            {
                var updatedBy = GetCurrentUserId();
                var result = await _permissionService.DeleteModulePrivilegeAsync(privilegeId, updatedBy);
                if (!result)
                    return NotFound(new { message = "Privilegio no encontrado" });

                return Ok(new { message = "Privilegio eliminado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar privilegio de módulo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("modules/{moduleId:int}/privileges/{privilegeId:int}/restore")]
        public async Task<IActionResult> RestoreModulePrivilege(int moduleId, int privilegeId)
        {
            try
            {
                var updatedBy = GetCurrentUserId();
                var result = await _permissionService.RestoreModulePrivilegeAsync(privilegeId, updatedBy);
                if (!result)
                    return NotFound(new { message = "Privilegio no encontrado" });

                return Ok(new { message = "Privilegio restaurado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restaurar privilegio de módulo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("modules/{moduleId:int}/privileges/defaults")]
        public async Task<IActionResult> EnsureDefaultPrivileges(int moduleId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _permissionService.EnsureDefaultModulePrivilegesAsync(moduleId, userId);
                return Ok(new { message = "Privilegios por defecto sincronizados" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sincronizar privilegios por defecto");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion

        #region Role permissions

        [HttpGet("roles/{roleId:int}/modules")]
        public async Task<IActionResult> GetRoleModulePermissions(int roleId, [FromQuery] int? moduleId)
        {
            try
            {
                var result = await _permissionService.GetRolePermissionsMatrixAsync(roleId, moduleId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener matriz de permisos del rol");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("roles/{roleId:int}/modules")]
        public async Task<IActionResult> UpdateRoleModulePermissions(int roleId, [FromBody] UpdateRoleModulePermissionsRequest request)
        {
            try
            {
                var updatedBy = GetCurrentUserId() ?? 0;
                await _permissionService.UpdateRoleModulePermissionsAsync(roleId, request, updatedBy);
                return Ok(new { message = "Permisos actualizados correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permisos del rol");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        #endregion
    }
}

