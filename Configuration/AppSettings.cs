namespace OmgtuPortal.Configuration;

public class AppSettings
{
    public required string DatabaseUrl { get; init; }
    public required string JwtSecret { get; init; }

    private static readonly Dictionary<string, string> EnvMap = new()
    {
        [nameof(DatabaseUrl)] = "DATABASE_URL",
        [nameof(JwtSecret)] = "JWT_SECRET",
    };

    public static AppSettings Load()
    {
        var missing = new List<string>();

        string Require(string propertyName)
        {
            var envName = EnvMap[propertyName];
            var value = Environment.GetEnvironmentVariable(envName);
            if (string.IsNullOrWhiteSpace(value))
                missing.Add(envName);
            return value ?? string.Empty;
        }

        var settings = new AppSettings
        {
            DatabaseUrl = Require(nameof(DatabaseUrl)),
            JwtSecret = Require(nameof(JwtSecret)),
        };

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing required environment variables: {string.Join(", ", missing)}. " +
                "Copy .env.example to .env and fill in the values.");
        }

        return settings;
    }
}
