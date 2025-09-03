using hospitalAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace hospitalAPI.StartupServices;

public static class AuthService
{
    public static IServiceCollection AddAuthService(this IServiceCollection services, IConfiguration configuration)
    {
        // 🔐 Get JWT settings
        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
        {
            throw new InvalidOperationException("Missing JWT configuration. Check 'Jwt:Key', 'Jwt:Issuer', 'Jwt:Audience'.");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,

                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = signingKey,

                        // Ensure claims are correctly mapped
                        NameClaimType = JwtRegisteredClaimNames.UniqueName,
                        RoleClaimType = ClaimTypes.Role
                    };

                    // Optional: Support custom header like "X-Auth-Token"
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            //var token = context.Request.Headers["Authorization"].FirstOrDefault();

                            var token = context.Request.Headers["X-MyCustomToken"].FirstOrDefault();
                            // Also check for custom header (optional)


                            if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = token["Bearer ".Length..].Trim();
                            }
                            else if (!string.IsNullOrEmpty(token) && !token.Contains(" "))
                            {
                                // Assume raw token (if trusted source)
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = context =>
                        {
                            // Log failures (use ILogger in production)
                            Console.WriteLine("Auth failed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine($"[Token Valid] User: {context.Principal?.Identity?.Name}");
                            return Task.CompletedTask;
                        }
                    };
                });

        // ✅ Authorization Policies
        services.AddAuthorization(options =>
        {
            // Fallback: require auth for all endpoints unless marked [AllowAnonymous]
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // ✅ Use exact role names from RoleType enum: Doctor, Nurse, Client, Admin
            options.AddPolicy("adminOnly", policy =>
                policy.RequireRole(RoleType.Admin.ToString()));

            options.AddPolicy("doctorsOnly", policy =>
                policy.RequireRole(RoleType.Doctor.ToString()));

            options.AddPolicy("nursesOnly", policy =>
                policy.RequireRole(RoleType.Nurse.ToString()));

            options.AddPolicy("clientsOnly", policy =>
                policy.RequireRole(RoleType.Client.ToString()));

            //for testign

            options.AddPolicy("clientsOrAdmins", policy =>
    policy.RequireRole(
        RoleType.Client.ToString(),
        RoleType.Admin.ToString()
    ));


        });

        return services;
    }
}