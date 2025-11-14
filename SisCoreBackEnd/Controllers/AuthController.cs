using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SisCoreBackEnd.DTOs.Auth;
using SisCoreBackEnd.Services;

namespace SisCoreBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Iniciar sesión con email/password o teléfono/password
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
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

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var response = await _authService.LoginAsync(request, ipAddress, userAgent);

                if (response == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Refrescar token de acceso
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var response = await _authService.RefreshTokenAsync(request.RefreshToken, request.DeviceId, ipAddress, userAgent);

                if (response == null)
                {
                    return Unauthorized(new { message = "Token de refresh inválido o expirado" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al refrescar token");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cerrar sesión
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.LogoutAsync(request.RefreshToken);
                if (!result)
                {
                    return BadRequest(new { message = "Token de refresh inválido" });
                }

                return Ok(new { message = "Sesión cerrada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener información del usuario actual
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = User.FindFirstValue(ClaimTypes.Email);
                var name = User.FindFirstValue(ClaimTypes.Name);
                var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                return Ok(new
                {
                    Id = userId,
                    Email = email,
                    FullName = name,
                    Roles = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario actual");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Validar token de acceso
        /// </summary>
        [HttpGet("validate")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = User.FindFirstValue(ClaimTypes.Email);
                var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                return Ok(new
                {
                    Valid = true,
                    UserId = userId,
                    Email = email,
                    Roles = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar token");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}

