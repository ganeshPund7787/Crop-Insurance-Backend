using System.Text;
using System.Threading.RateLimiting;
using Authentication.Configuration;
using Authentication.Helpers;
using Authentication.Interfaces;
using Authentication.Models;
using Authentication.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── JWT Settings ───────────────────────────────────────────────────
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        // ── Cookie Settings ────────────────────────────────────────────────
        services.Configure<CookieSettings>(
            configuration.GetSection("CookieSettings"));

        var jwtSettings = configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>()!;

        var cookieSettings = configuration
            .GetSection("CookieSettings")
            .Get<CookieSettings>()!;

        // ── CORS ───────────────────────────────────────────────────────────
        var allowedOrigins = configuration
            .GetSection("CorsSettings:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("CropInsurancePolicy", policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // Required for cookies
            });
        });

        // ── Rate Limiting ──────────────────────────────────────────────────
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("GlobalPolicy", opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromSeconds(60);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 5;
            });

            options.AddFixedWindowLimiter("AuthPolicy", opt =>
            {
                opt.PermitLimit = 10;
                opt.Window = TimeSpan.FromSeconds(60);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(
                    """{"success":false,"message":"Too many requests. Please try again later."}""",
                    cancellationToken);
            };
        });

        // ── Authentication — read JWT from HttpOnly cookie ─────────────────
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };

                // ── Read token from cookie instead of Authorization header ──
                // This is the key change — JWT middleware reads cookie
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Read access token from HttpOnly cookie
                        var accessToken = context.Request.Cookies
                            .TryGetValue(
                                cookieSettings.AccessTokenCookieName,
                                out var token)
                            ? token
                            : null;

                        if (!string.IsNullOrEmpty(accessToken))
                            context.Token = accessToken;

                        return Task.CompletedTask;
                    },

                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(
                            """{"success":false,"message":"Unauthorized. Please login."}""");
                    },

                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(
                            """{"success":false,"message":"Forbidden. You do not have permission."}""");
                    }
                };
            });

        services.AddAuthorization();

        // ── Repositories ───────────────────────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRepository<FarmerProfile>,
            BaseRepository<FarmerProfile>>();
        services.AddScoped<IRepository<AgentProfile>,
            BaseRepository<AgentProfile>>();
        services.AddScoped<IRepository<Farm>,
            BaseRepository<Farm>>();
        services.AddScoped<IRepository<Crop>,
            BaseRepository<Crop>>();
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IInspectionRepository, InspectionRepository>();
        services.AddScoped<IRepository<InsuranceClaim>,
            BaseRepository<InsuranceClaim>>();

        // ── Services ───────────────────────────────────────────────────────
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFarmerService, FarmerService>();
        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<IClaimRepository, ClaimRepository>();

        // ── HTTP Context + Cookie Helper ───────────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<CookieHelper>();  // ← registered here

        // ── AutoMapper ─────────────────────────────────────────────────────
        services.AddAutoMapper(typeof(AutoMapperProfile));

        // ── FluentValidation ───────────────────────────────────────────────
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }
}