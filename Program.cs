using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using dotenv.net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OmgtuPortal;
using OmgtuPortal.Data;
using Scalar.AspNetCore;

DotEnv.Load();

var appSettings = AppSettings.FromEnvironment();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(appSettings);
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
        options.UseSqlite("Data Source=app.db");
    else
        options.UseNpgsql(appSettings.DatabaseUrl);
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = appSettings.CorsOrigins
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Authentication
if (appSettings.AutoLogin && builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication("DevAutoLogin")
        .AddScheme<AuthenticationSchemeOptions, DevAutoLoginHandler>("DevAutoLogin", null);
}
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = appSettings.Authority;
            options.Audience = appSettings.KeycloakClientId;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = appSettings.Authority,
                ValidateAudience = true,
                ValidAudience = appSettings.KeycloakClientId,
                ValidateLifetime = true,
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    if (context.Principal?.Identity is not ClaimsIdentity identity)
                        return Task.CompletedTask;

                    var realmAccess = identity.FindFirst("realm_access")?.Value;
                    if (realmAccess is null)
                        return Task.CompletedTask;

                    try
                    {
                        using var doc = JsonDocument.Parse(realmAccess);
                        if (doc.RootElement.TryGetProperty("roles", out var roles))
                        {
                            foreach (var role in roles.EnumerateArray())
                            {
                                var value = role.GetString();
                                if (!string.IsNullOrEmpty(value))
                                    identity.AddClaim(new Claim(ClaimTypes.Role, value));
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Ignore malformed claim
                    }

                    return Task.CompletedTask;
                }
            };
        });
}

builder.Services.AddAuthorization();

var app = builder.Build();

// Auto-apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs");
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, ".frontend", "dist")),
    RequestPath = ""
});

// Endpoints

app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy" }))
    .Produces<object>(StatusCodes.Status200OK);

app.MapGet("/api/me", (ClaimsPrincipal user) => Results.Ok(new
    {
        Sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub"),
        PreferredUsername = user.FindFirstValue("preferred_username"),
        Email = user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email"),
        GivenName = user.FindFirstValue(ClaimTypes.GivenName) ?? user.FindFirstValue("given_name"),
        FamilyName = user.FindFirstValue(ClaimTypes.Surname) ?? user.FindFirstValue("family_name"),
        Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
    }))
    .RequireAuthorization()
    .Produces<object>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized);

// SPA fallback

app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(new { Error = "Not Found" });
        return;
    }

    var indexPath = Path.Combine(builder.Environment.ContentRootPath, ".frontend", "dist", "index.html");
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(indexPath);
});

app.Run();

// Dev-only auth handler: auto-authenticates every request with all roles
sealed class DevAutoLoginHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "dev-user-id"),
            new("sub", "dev-user-id"),
            new("preferred_username", "developer"),
            new(ClaimTypes.Email, "dev@localhost"),
            new("email", "dev@localhost"),
            new(ClaimTypes.GivenName, "Dev"),
            new("given_name", "Dev"),
            new(ClaimTypes.Surname, "User"),
            new("family_name", "User"),
            new(ClaimTypes.Role, "admin"),
            new(ClaimTypes.Role, "user"),
            new(ClaimTypes.Role, "student"),
            new(ClaimTypes.Role, "teacher"),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
