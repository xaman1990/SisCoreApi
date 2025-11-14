using Microsoft.EntityFrameworkCore;
using SisCoreBackEnd.Data;
using SisCoreBackEnd.Domain.Master;
using SisCoreBackEnd.DTOs.MasterUsers;
using SisCoreBackEnd.Tenancy;

namespace SisCoreBackEnd.Services
{
    public class MasterUserService : IMasterUserService
    {
        private readonly MasterDbContext _masterDb;
        private readonly TenantDbContextFactory _tenantDbFactory;
        private readonly ITenantResolver _tenantResolver;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MasterUserService(
            MasterDbContext masterDb,
            TenantDbContextFactory tenantDbFactory,
            ITenantResolver tenantResolver,
            IHttpContextAccessor httpContextAccessor)
        {
            _masterDb = masterDb;
            _tenantDbFactory = tenantDbFactory;
            _tenantResolver = tenantResolver;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MasterUserResponse> RegisterMasterUserAsync(RegisterMasterUserRequest request, int? createdBy)
        {
            // Validar que el usuario que crea es God (si se está asignando rol God)
            if (request.IsGod && createdBy.HasValue)
            {
                var creator = await _masterDb.MasterUsers
                    .FirstOrDefaultAsync(mu => mu.Id == createdBy.Value);
                
                if (creator == null || !creator.IsGod)
                {
                    throw new UnauthorizedAccessException("Solo usuarios con rol God pueden asignar el rol God a otros usuarios.");
                }
            }

            // Obtener la empresa por subdomain
            var company = await _masterDb.Companies
                .FirstOrDefaultAsync(c => c.Subdomain == request.TenantSubdomain.ToLower());

            if (company == null)
            {
                throw new InvalidOperationException($"No se encontró la empresa con subdomain '{request.TenantSubdomain}'");
            }

            // Validar que el usuario existe en la BD tenant
            // Necesitamos construir el contexto del tenant para acceder a su BD
            var connectionString = BuildConnectionString(company);
            var tempContext = new TenantContext
            {
                CompanyId = company.Id,
                Subdomain = company.Subdomain,
                ConnectionString = connectionString
            };

            // Guardar contexto temporal en HttpContext para que TenantDbContextFactory lo use
            var originalContext = _httpContextAccessor.HttpContext?.GetTenant();
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Items["TENANT"] = tempContext;
                }

                using var tenantDb = _tenantDbFactory.CreateDbContext();
                var tenantUser = await tenantDb.Users
                    .FirstOrDefaultAsync(u => u.Id == request.TenantUserId && u.Status == 1);

                if (tenantUser == null)
                {
                    throw new InvalidOperationException($"No se encontró el usuario con ID {request.TenantUserId} en la empresa '{request.TenantSubdomain}'");
                }

                // Validar que el email coincida
                if (!string.IsNullOrEmpty(tenantUser.Email))
                {
                    // Verificar si ya existe un MasterUser con este email
                    var existingMasterUser = await _masterDb.MasterUsers
                        .FirstOrDefaultAsync(mu => mu.Email == tenantUser.Email);

                    if (existingMasterUser != null)
                    {
                        throw new InvalidOperationException($"Ya existe un usuario maestro con el email '{tenantUser.Email}'");
                    }
                }

                // Verificar si ya existe un MasterUser para este TenantUserId y TenantCompanyId
                var existing = await _masterDb.MasterUsers
                    .FirstOrDefaultAsync(mu => mu.TenantUserId == request.TenantUserId && mu.TenantCompanyId == company.Id);

                if (existing != null)
                {
                    throw new InvalidOperationException($"El usuario ya está registrado como usuario maestro para esta empresa.");
                }

                // Crear MasterUser
                var masterUser = new MasterUser
                {
                    Email = tenantUser.Email ?? throw new InvalidOperationException("El usuario debe tener un email"),
                    FullName = tenantUser.FullName,
                    PhoneNumber = tenantUser.PhoneNumber,
                    GoogleId = tenantUser.GoogleId,
                    TenantUserId = request.TenantUserId,
                    TenantCompanyId = company.Id,
                    IsGod = request.IsGod,
                    Status = 1,
                    CreatedBy = createdBy
                };

                _masterDb.MasterUsers.Add(masterUser);
                await _masterDb.SaveChangesAsync();

                // Asignar acceso a la empresa de origen
                var masterUserCompany = new MasterUserCompany
                {
                    MasterUserId = masterUser.Id,
                    CompanyId = company.Id,
                    Role = request.IsGod ? "god" : "owner",
                    GrantedBy = createdBy
                };

                _masterDb.MasterUserCompanies.Add(masterUserCompany);
                await _masterDb.SaveChangesAsync();

                return await GetMasterUserByIdAsync(masterUser.Id);
            }
            finally
            {
                // Restaurar contexto original si existía
                if (_httpContextAccessor.HttpContext != null)
                {
                    if (originalContext != null)
                    {
                        _httpContextAccessor.HttpContext.Items["TENANT"] = originalContext;
                    }
                    else
                    {
                        _httpContextAccessor.HttpContext.Items.Remove("TENANT");
                    }
                }
            }
        }

        public async Task<MasterUserResponse?> GetMasterUserByTenantUserAsync(int tenantUserId, int tenantCompanyId)
        {
            var masterUser = await _masterDb.MasterUsers
                .Include(mu => mu.TenantCompany)
                .Include(mu => mu.MasterUserCompanies)
                    .ThenInclude(muc => muc.Company)
                .FirstOrDefaultAsync(mu => mu.TenantUserId == tenantUserId && mu.TenantCompanyId == tenantCompanyId);

            if (masterUser == null)
                return null;

            return MapToResponse(masterUser);
        }

        public async Task<MasterUserResponse?> GetMasterUserByEmailAsync(string email)
        {
            var masterUser = await _masterDb.MasterUsers
                .Include(mu => mu.TenantCompany)
                .Include(mu => mu.MasterUserCompanies)
                    .ThenInclude(muc => muc.Company)
                .FirstOrDefaultAsync(mu => mu.Email == email.ToLower());

            if (masterUser == null)
                return null;

            return MapToResponse(masterUser);
        }

        public async Task<bool> IsUserGodAsync(int tenantUserId, int tenantCompanyId)
        {
            var masterUser = await _masterDb.MasterUsers
                .FirstOrDefaultAsync(mu => mu.TenantUserId == tenantUserId && mu.TenantCompanyId == tenantCompanyId);

            return masterUser?.IsGod == true && masterUser.Status == 1;
        }

        public async Task<bool> IsUserGodByEmailAsync(string email)
        {
            var masterUser = await _masterDb.MasterUsers
                .FirstOrDefaultAsync(mu => mu.Email == email.ToLower());

            return masterUser?.IsGod == true && masterUser.Status == 1;
        }

        public async Task<List<MasterUserResponse>> GetMasterUsersAsync(bool? isGod = null)
        {
            var query = _masterDb.MasterUsers
                .Include(mu => mu.TenantCompany)
                .Include(mu => mu.MasterUserCompanies)
                    .ThenInclude(muc => muc.Company)
                .AsQueryable();

            if (isGod.HasValue)
            {
                query = query.Where(mu => mu.IsGod == isGod.Value);
            }

            var masterUsers = await query
                .Where(mu => mu.Status == 1)
                .ToListAsync();

            return masterUsers.Select(MapToResponse).ToList();
        }

        public async Task<MasterUserResponse> AssignCompanyToMasterUserAsync(AssignCompanyToMasterUserRequest request, int grantedBy)
        {
            // Validar que el usuario que asigna es God
            var granter = await _masterDb.MasterUsers
                .FirstOrDefaultAsync(mu => mu.Id == grantedBy);

            if (granter == null || !granter.IsGod)
            {
                throw new UnauthorizedAccessException("Solo usuarios con rol God pueden asignar empresas a usuarios maestros.");
            }

            // Validar que si se asigna rol "god", solo God puede hacerlo
            if (request.Role == "god")
            {
                throw new UnauthorizedAccessException("El rol 'god' solo se puede asignar durante el registro del usuario maestro. Use RegisterMasterUserAsync con IsGod=true.");
            }

            var masterUser = await _masterDb.MasterUsers
                .FirstOrDefaultAsync(mu => mu.Id == request.MasterUserId);

            if (masterUser == null)
            {
                throw new InvalidOperationException($"No se encontró el usuario maestro con ID {request.MasterUserId}");
            }

            var company = await _masterDb.Companies
                .FirstOrDefaultAsync(c => c.Id == request.CompanyId);

            if (company == null)
            {
                throw new InvalidOperationException($"No se encontró la empresa con ID {request.CompanyId}");
            }

            // Verificar si ya existe la relación
            var existing = await _masterDb.MasterUserCompanies
                .FirstOrDefaultAsync(muc => muc.MasterUserId == request.MasterUserId && muc.CompanyId == request.CompanyId);

            if (existing != null)
            {
                existing.Role = request.Role;
                existing.GrantedBy = grantedBy;
                existing.GrantedAt = DateTime.UtcNow;
            }
            else
            {
                var masterUserCompany = new MasterUserCompany
                {
                    MasterUserId = request.MasterUserId,
                    CompanyId = request.CompanyId,
                    Role = request.Role,
                    GrantedBy = grantedBy
                };

                _masterDb.MasterUserCompanies.Add(masterUserCompany);
            }

            await _masterDb.SaveChangesAsync();

            return await GetMasterUserByIdAsync(request.MasterUserId);
        }

        public async Task<bool> RevokeCompanyFromMasterUserAsync(int masterUserId, int companyId)
        {
            var masterUserCompany = await _masterDb.MasterUserCompanies
                .FirstOrDefaultAsync(muc => muc.MasterUserId == masterUserId && muc.CompanyId == companyId);

            if (masterUserCompany == null)
                return false;

            _masterDb.MasterUserCompanies.Remove(masterUserCompany);
            await _masterDb.SaveChangesAsync();

            return true;
        }

        private async Task<MasterUserResponse> GetMasterUserByIdAsync(int id)
        {
            var masterUser = await _masterDb.MasterUsers
                .Include(mu => mu.TenantCompany)
                .Include(mu => mu.MasterUserCompanies)
                    .ThenInclude(muc => muc.Company)
                .FirstOrDefaultAsync(mu => mu.Id == id);

            if (masterUser == null)
                throw new InvalidOperationException($"No se encontró el usuario maestro con ID {id}");

            return MapToResponse(masterUser);
        }

        private MasterUserResponse MapToResponse(MasterUser masterUser)
        {
            return new MasterUserResponse
            {
                Id = masterUser.Id,
                Email = masterUser.Email,
                FullName = masterUser.FullName,
                PhoneNumber = masterUser.PhoneNumber,
                TenantUserId = masterUser.TenantUserId,
                TenantCompanyId = masterUser.TenantCompanyId,
                TenantCompanyName = masterUser.TenantCompany.Name,
                TenantSubdomain = masterUser.TenantCompany.Subdomain,
                IsGod = masterUser.IsGod,
                Status = masterUser.Status,
                LastLoginAt = masterUser.LastLoginAt,
                CreatedAt = masterUser.CreatedAt,
                Companies = masterUser.MasterUserCompanies.Select(muc => new MasterUserCompanyResponse
                {
                    Id = muc.Id,
                    CompanyId = muc.CompanyId,
                    CompanyName = muc.Company.Name,
                    Subdomain = muc.Company.Subdomain,
                    Role = muc.Role,
                    GrantedAt = muc.GrantedAt
                }).ToList()
            };
        }

        private string BuildConnectionString(Company company)
        {
            var parts = new List<string>();
            parts.Add($"Server={company.DbHost}");

            if (company.DbPort.HasValue)
            {
                parts.Add($"Port={company.DbPort}");
            }

            parts.Add($"Database={company.DbName}");
            parts.Add($"User={company.DbUser}");
            parts.Add($"Password={company.DbPassword}");

            // Agregar opciones adicionales si están especificadas
            if (!string.IsNullOrWhiteSpace(company.ConnectionOptions))
            {
                var options = company.ConnectionOptions.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var option in options)
                {
                    var trimmed = option.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        parts.Add(trimmed);
                    }
                }
            }
            else
            {
                parts.Add("SslMode=None");
            }

            return string.Join(";", parts);
        }
    }
}

