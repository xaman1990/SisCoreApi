using Microsoft.EntityFrameworkCore;
using SisCoreBackEnd.Data;
using SisCoreBackEnd.DTOs.Auth;
using SisCoreBackEnd.Domain.Tenant;
using SisCoreBackEnd.Tenancy;

namespace SisCoreBackEnd.Services
{
    public class AuthService : IAuthService
    {
        private readonly TenantDbContextFactory _dbFactory;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            TenantDbContextFactory dbFactory,
            IPasswordService passwordService,
            IJwtService jwtService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbFactory = dbFactory;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private TenantDbContext GetDbContext()
        {
            // El TenantDbContextFactory ya valida el tenant context
            return _dbFactory.CreateDbContext();
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent)
        {
            using var db = GetDbContext();

            User? user = null;

            // Buscar usuario por email o teléfono
            // Entity Framework abrirá la conexión automáticamente cuando se ejecute la consulta
            if (!string.IsNullOrEmpty(request.Email))
            {
                user = await db.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.Status == 1);
            }
            else if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user = await db.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber && u.Status == 1);
            }

            if (user == null)
                return null;

            // Verificar contraseña
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                    return null;
            }

            // Obtener roles
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            // Generar tokens
            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshTokenJti = Guid.NewGuid().ToString(); // JTI único para el refresh token

            // Calcular expiración
            var jwtConfig = _configuration.GetSection("Jwt");
            var refreshDays = int.Parse(jwtConfig["RefreshDays"] ?? "14");
            var expiresAt = DateTime.UtcNow.AddDays(refreshDays);

            // Guardar refresh token
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Jti = refreshTokenJti,
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ExpiresAt = expiresAt
            };

            db.RefreshTokens.Add(refreshToken);
            user.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenJti,
                ExpiresAt = expiresAt,
                User = new UserInfo
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles,
                    MfaEnabled = user.MfaEnabled
                }
            };
        }

        public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken, string? deviceId, string? ipAddress, string? userAgent)
        {
            using var db = GetDbContext();

            // Buscar el refresh token (simplificado - en producción usar JWT)
            var token = await db.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Jti == refreshToken && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow);

            if (token == null || token.User == null)
                return null;

            // Revocar el token anterior
            token.RevokedAt = DateTime.UtcNow;

            // Generar nuevos tokens
            var user = token.User;
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
            var newRefreshTokenJti = Guid.NewGuid().ToString();

            // Guardar nuevo refresh token
            var jwtConfig = _configuration.GetSection("Jwt");
            var refreshDays = int.Parse(jwtConfig["RefreshDays"] ?? "14");
            var expiresAt = DateTime.UtcNow.AddDays(refreshDays);

            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Jti = newRefreshTokenJti,
                DeviceId = deviceId ?? token.DeviceId,
                DeviceName = token.DeviceName,
                IpAddress = ipAddress ?? token.IpAddress,
                UserAgent = userAgent ?? token.UserAgent,
                ExpiresAt = expiresAt,
                ReplacedByJti = token.Jti
            };

            db.RefreshTokens.Add(newRefreshToken);
            await db.SaveChangesAsync();

            return new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenJti,
                ExpiresAt = expiresAt,
                User = new UserInfo
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles,
                    MfaEnabled = user.MfaEnabled
                }
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            using var db = GetDbContext();

            var token = await db.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Jti == refreshToken && rt.RevokedAt == null);

            if (token == null)
                return false;

            token.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<User> RegisterUserAsync(RegisterUserRequest request, int? createdBy)
        {
            using var db = GetDbContext();

            // Validar que email o teléfono no existan
            if (!string.IsNullOrEmpty(request.Email))
            {
                var exists = await db.Users.AnyAsync(u => u.Email == request.Email);
                if (exists)
                    throw new InvalidOperationException("El email ya está registrado");
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                var exists = await db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber);
                if (exists)
                    throw new InvalidOperationException("El teléfono ya está registrado");
            }

            var user = new User
            {
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = _passwordService.HashPassword(request.Password),
                FullName = request.FullName,
                EmployeeNumber = request.EmployeeNumber,
                Status = 1,
                CreatedBy = createdBy
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Asignar roles
            if (request.RoleIds.Any())
            {
                var roles = await db.Roles
                    .Where(r => request.RoleIds.Contains(r.Id))
                    .ToListAsync();

                foreach (var role in roles)
                {
                    db.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                        AssignedBy = createdBy
                    });
                }

                await db.SaveChangesAsync();
            }

            return user;
        }
    }
}

