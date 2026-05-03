using System.ComponentModel.DataAnnotations;

namespace OmgtuPortal;

public sealed class AppSettings
{
    [Required]
    public required string DatabaseUrl { get; init; }

    [Required]
    [MinLength(32)]
    public required string JwtSecret { get; init; }

    [Required]
    public required string JwtIssuer { get; init; }

    [Required]
    public required string CorsOrigins { get; init; }

    /// <summary>
    /// Creates AppSettings from environment variables.
    /// Throws if any required variable is missing.
    /// </summary>
    public static AppSettings FromEnvironment()
    {
        var settings = new AppSettings
        {
            DatabaseUrl = GetRequired("DATABASE_URL"),
            JwtSecret = GetRequired("JWT_SECRET"),
            JwtIssuer = GetRequired("JWT_ISSUER"),
            CorsOrigins = GetRequired("CORS_ORIGINS"),
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
