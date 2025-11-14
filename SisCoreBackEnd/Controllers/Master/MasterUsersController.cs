using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SisCoreBackEnd.DTOs.MasterUsers;
using SisCoreBackEnd.Services;

namespace SisCoreBackEnd.Controllers.Master
{
    [ApiController]
    [Route("api/master/[controller]")]
    [Authorize]
    public class MasterUsersController : ControllerBase
    {
        private readonly IMasterUserService _masterUserService;
        private readonly ILogger<MasterUsersController> _logger;

        public MasterUsersController(
            IMasterUserService masterUserService,
            ILogger<MasterUsersController> logger)
        {
            _masterUserService = masterUserService;
            _logger = logger;
        }

        /// <summary>
        /// Registrar un usuario como usuario maestro (multi-empresa)
        /// Solo usuarios God pueden asignar rol God
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterMasterUser([FromBody] RegisterMasterUserRequest request)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var currentUserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "";

                // Verificar si el usuario actual es God (necesario para asignar rol God)
                if (request.IsGod)
                {
                    var isCurrentUserGod = await _masterUserService.IsUserGodByEmailAsync(currentUserEmail);
                    if (!isCurrentUserGod)
                    {
                        return Forbid("Solo usuarios con rol God pueden asignar el rol God a otros usuarios.");
                    }
                }

                var masterUser = await _masterUserService.RegisterMasterUserAsync(request, currentUserId);
                return CreatedAtAction(nameof(GetMasterUser), new { id = masterUser.Id }, masterUser);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario maestro");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener usuario maestro por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMasterUser(int id)
        {
            try
            {
                var masterUsers = await _masterUserService.GetMasterUsersAsync();
                var masterUser = masterUsers.FirstOrDefault(mu => mu.Id == id);

                if (masterUser == null)
                {
                    return NotFound(new { message = "Usuario maestro no encontrado" });
                }

                return Ok(masterUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario maestro");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener todos los usuarios maestros
        /// Solo usuarios God pueden ver otros usuarios God
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMasterUsers([FromQuery] bool? isGod = null)
        {
            try
            {
                var currentUserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "";
                var isCurrentUserGod = await _masterUserService.IsUserGodByEmailAsync(currentUserEmail);

                // Si el usuario no es God, no puede filtrar por isGod=true
                if (isGod == true && !isCurrentUserGod)
                {
                    return Forbid("Solo usuarios con rol God pueden ver otros usuarios God.");
                }

                var masterUsers = await _masterUserService.GetMasterUsersAsync(isGod);
                return Ok(masterUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios maestros");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Asignar empresa a usuario maestro
        /// Solo usuarios God pueden hacer esto
        /// </summary>
        [HttpPost("assign-company")]
        public async Task<IActionResult> AssignCompanyToMasterUser([FromBody] AssignCompanyToMasterUserRequest request)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var masterUser = await _masterUserService.AssignCompanyToMasterUserAsync(request, currentUserId);
                return Ok(masterUser);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar empresa a usuario maestro");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Revocar acceso a empresa de usuario maestro
        /// Solo usuarios God pueden hacer esto
        /// </summary>
        [HttpDelete("{masterUserId}/companies/{companyId}")]
        public async Task<IActionResult> RevokeCompanyFromMasterUser(int masterUserId, int companyId)
        {
            try
            {
                var result = await _masterUserService.RevokeCompanyFromMasterUserAsync(masterUserId, companyId);
                if (!result)
                {
                    return NotFound(new { message = "No se encontró la relación entre el usuario maestro y la empresa" });
                }

                return Ok(new { message = "Acceso revocado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar acceso a empresa");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Verificar si el usuario actual es God
        /// </summary>
        [HttpGet("check-god")]
        public async Task<IActionResult> CheckIfUserIsGod()
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? "";
                var isGod = await _masterUserService.IsUserGodByEmailAsync(email);

                return Ok(new { IsGod = isGod });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si usuario es God");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}

