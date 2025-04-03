using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartScheduledApi.DataContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SmartScheduledApi.Services;
using DotNetEnv;
using SmartScheduledApi.Interfaces;
using SmartScheduledApi.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Carregar variáveis de ambiente do arquivo .env
Env.Load();

// Adicionar serviços ao contêiner
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Smart Scheduled API",
        Version = "v1",
        Description = "API for Smart Scheduled system"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Scheme = "Bearer",
        BearerFormat = "JWT",
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
});

builder.Services.AddAutoMapper(typeof(Program));
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
builder.Services.AddDbContext<SmartScheduleApiContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

// Adicionar autenticação
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configurar autenticação JWT Bearer
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer ?? throw new Exception("JWT_ISSUER not found"),
            ValidAudience = jwtAudience ?? throw new Exception("JWT_AUDIENCE not found"),
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey ?? throw new Exception("JWT_KEY not found"))
            )
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

// Adicionar configuração do CORS
builder.Services.AddCors(options =>
{
    var corsOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")?.Split(',')
        ?? new[] { "http://localhost:61798" };

    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());

    // Em ambiente de desenvolvimento, podemos ter uma política mais permissiva
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("Development",
            policy => policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
    }
});

// Adicionar serviços
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserContextService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Register custom services
builder.Services.AddScoped<AuthorizationService>();
builder.Services.AddScoped<TeamRulePermissionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Adicionar middleware CORS
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");  // Use política mais permissiva em desenvolvimento
}
else
{
    app.UseCors("AllowSpecificOrigin");  // Use política restritiva em produção
}

app.MapControllers();

app.Run();

namespace SmartScheduledApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
