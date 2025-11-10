using System.Text;
using DrumBuddy.Endpoint.Configuration;
using DrumBuddy.Endpoint.Services;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DrumBuddy.Endpoint.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = GetConnectionString(configuration);

        services.AddDbContext<DrumBuddyDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddApplicationIdentity(
        this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<DrumBuddyDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var secretKey = GetJwtSecret(configuration, "JwtSettings:SecretKey");
        var issuer = GetJwtSecret(configuration, "JwtSettings:Issuer");
        var audience = GetJwtSecret(configuration, "JwtSettings:Audience");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "Bearer";
            options.DefaultChallengeScheme = "Bearer";
        })
        .AddJwtBearer("Bearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();
        services.AddScoped<TokenService>();

        return services;
    }

    public static IServiceCollection AddApplicationCors(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        var env = configuration["ASPNETCORE_ENVIRONMENT"];
        
        if (env == "Development")
        {
            return configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");
        }
        else
        {
            return Secrets.DatabaseConnectionString;
        }
    }

    private static string GetJwtSecret(IConfiguration configuration, string key)
    {
        var env = configuration["ASPNETCORE_ENVIRONMENT"];
        
        if (env == "Development")
        {
            return configuration[key]
                ?? throw new InvalidOperationException($"JWT setting '{key}' not found in appsettings.json");
        }
        else
        {
            return key switch
            {
                "JwtSettings:SecretKey" => Secrets.JwtSecretKey,
                "JwtSettings:Issuer" => Secrets.JwtIssuer,
                "JwtSettings:Audience" => Secrets.JwtAudience,
                _ => throw new InvalidOperationException($"Unknown JWT setting: {key}")
            };
        }
    }
}
