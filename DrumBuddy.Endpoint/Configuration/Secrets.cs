namespace DrumBuddy.Endpoint.Configuration;

public static class Secrets
{
    public static string DatabaseConnectionString => 
        Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
        ?? throw new InvalidOperationException("DATABASE_CONNECTION_STRING environment variable not found.");

    public static string JwtSecretKey => 
        Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
        ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable not found.");

    public static string JwtIssuer => 
        Environment.GetEnvironmentVariable("JWT_ISSUER")
        ?? throw new InvalidOperationException("JWT_ISSUER environment variable not found.");

    public static string JwtAudience => 
        Environment.GetEnvironmentVariable("JWT_AUDIENCE")
        ?? throw new InvalidOperationException("JWT_AUDIENCE environment variable not found.");

    public static int JwtExpirationMinutes => 
        int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"), out var minutes)
            ? minutes
            : throw new InvalidOperationException("JWT_EXPIRATION_MINUTES environment variable not found or is not a valid integer.");
}