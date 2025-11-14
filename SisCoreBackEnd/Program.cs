using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using SisCoreBackEnd.Data;
using SisCoreBackEnd.Services;
using SisCoreBackEnd.Tenancy;
using SisCoreBackEnd.Swagger.Examples;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Master Database (BD maestra)
var masterConnectionString = builder.Configuration.GetConnectionString("MasterDatabase")
    ?? builder.Configuration["MasterDatabase:ConnectionString"]
    ?? throw new InvalidOperationException("MasterDatabase connection string is required");

builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseMySql(masterConnectionString, ServerVersion.AutoDetect(masterConnectionString)));

// Tenant Resolver
builder.Services.AddScoped<ITenantResolver, TenantResolver>();
builder.Services.AddScoped<TenantDbContextFactory>();

// Services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IMasterUserService, MasterUserService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// HttpContextAccessor para servicios
builder.Services.AddHttpContextAccessor();

// JWT Authentication
var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtConfig["SigningKey"] ?? throw new InvalidOperationException("JWT SigningKey is required");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "TimeControl API", 
        Version = "v1",
        Description = "API para gestión de control de tiempo multiempresa"
    });

    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.OperationFilter<RequestResponseExamplesOperationFilter>();
    c.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<RequestResponseExamplesOperationFilter>();
builder.WebHost.UseUrls("http://0.0.0.0:7004");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeControl API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors();

// Tenant Resolution Middleware (antes de Authentication)
app.UseTenantResolution();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
