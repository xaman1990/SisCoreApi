using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeControlApi.DTOs.Auth;
using TimeControlApi.Services;

namespace TimeControlApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IAuthService authService,
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _authService = authService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Registrar nuevo usuario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.PhoneNumber))
                {
                    return BadRequest(new { message = "Debe proporcionar email o teléfono" });
                }

                if (string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "La contraseña es requerida" });
                }

                if (string.IsNullOrEmpty(request.FullName))
                {
                    return BadRequest(new { message = "El nombre completo es requerido" });
                }

                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var user = await _authService.RegisterUserAsync(request, currentUserId);

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FullName = user.FullName,
                    EmployeeNumber = user.EmployeeNumber
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener todos los usuarios
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userService.GetUsersAsync();
                var result = users.Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.PhoneNumber,
                    u.FullName,
                    u.EmployeeNumber,
                    u.Status,
                    Roles = u.UserRoles.Select(ur => new
                    {
                        ur.Role.Id,
                        ur.Role.Name
                    }).ToList()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(new
                {
                    user.Id,
                    user.Email,
                    user.PhoneNumber,
                    user.FullName,
                    user.EmployeeNumber,
                    user.Status,
                    Roles = user.UserRoles.Select(ur => new
                    {
                        ur.Role.Id,
                        ur.Role.Name
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualizar usuario
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(
                    id,
                    request.Email,
                    request.PhoneNumber,
                    request.FullName,
                    request.EmployeeNumber
                );

                return Ok(new
                {
                    user.Id,
                    user.Email,
                    user.PhoneNumber,
                    user.FullName,
                    user.EmployeeNumber
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Eliminar usuario (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(new { message = "Usuario eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Asignar roles a usuario
        /// </summary>
        [HttpPost("{id}/roles")]
        public async Task<IActionResult> AssignRoles(int id, [FromBody] AssignRolesRequest request)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var result = await _userService.AssignRolesAsync(id, request.RoleIds, currentUserId);
                if (!result)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(new { message = "Roles asignados exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar roles");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }

    public class UpdateUserRequest
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? EmployeeNumber { get; set; }
    }

    public class AssignRolesRequest
    {
        public List<int> RoleIds { get; set; } = new();
    }
}

