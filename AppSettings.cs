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

    public string? UniversityProvider { get; init; }
    public string? UniversityBaseUrl { get; init; }

    public string UploadsPath { get; init; } = "Uploads";
    public long MaxFileSizeMb { get; init; } = 50;

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
            UniversityProvider = Environment.GetEnvironmentVariable("UNIVERSITY_PROVIDER"),
            UniversityBaseUrl = Environment.GetEnvironmentVariable("UNIVERSITY_BASE_URL"),
            UploadsPath = Environment.GetEnvironmentVariable("UPLOADS_PATH") ?? "Uploads",
            MaxFileSizeMb = long.TryParse(Environment.GetEnvironmentVariable("MAX_FILE_SIZE_MB"), out var maxSize) ? maxSize : 50,
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
