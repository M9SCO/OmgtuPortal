using System.ComponentModel.DataAnnotations;

namespace OmgtuPortal;

public sealed class AppSettings
{
    [Required]
    public required string DatabaseUrl { get; init; }

    [Required]
    public required string KeycloakUrl { get; init; }

    [Required]
    public required string KeycloakRealm { get; init; }

    [Required]
    public required string KeycloakClientId { get; init; }

    [Required]
    public required string CorsOrigins { get; init; }

    public bool AutoLogin { get; init; }

    public string Authority => $"{KeycloakUrl.TrimEnd('/')}/realms/{KeycloakRealm}";

    public static AppSettings FromEnvironment()
    {
        var settings = new AppSettings
        {
            DatabaseUrl = GetRequired("DATABASE_URL"),
            KeycloakUrl = GetRequired("KEYCLOAK_URL"),
            KeycloakRealm = GetRequired("KEYCLOAK_REALM"),
            KeycloakClientId = GetRequired("KEYCLOAK_CLIENT_ID"),
            CorsOrigins = GetRequired("CORS_ORIGINS"),
            AutoLogin = Environment.GetEnvironmentVariable("AUTOLOGIN")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false,
        };

        Validate(settings);
        return settings;
    }

    private static string GetRequired(string name)
    {
        return Environment.GetEnvironmentVariable(name)
               ?? throw new InvalidOperationException(
                   $"Required environment variable '{name}' is not set. Check your .env file.");
    }

    private static void Validate(AppSettings settings)
    {
        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(settings, context, results, validateAllProperties: true))
        {
            var errors = string.Join(Environment.NewLine, results.Select(r => r.ErrorMessage));
            throw new InvalidOperationException(
                $"AppSettings validation failed:{Environment.NewLine}{errors}");
        }
    }
}
