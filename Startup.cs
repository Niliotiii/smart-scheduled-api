using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using SmartScheduledApi.Services;
using SmartScheduledApi.Interfaces;
using SmartScheduledApi.Models;
using SmartScheduledApi.DataContext;
using DotNetEnv;
using System.Text;

namespace SmartScheduledApi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        Env.Load();
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
                var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
                var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

                if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
                {
                    throw new ArgumentNullException("JWT configuration is missing");
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("User", policy => policy.RequireRole("User"));
        });

        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        // ...other services

        // Add DbContext and Seed Data
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        services.AddDbContext<SmartScheduleApiContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SmartScheduleApiContext context)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        // Seed the database
        context.Database.Migrate();
    }
}
