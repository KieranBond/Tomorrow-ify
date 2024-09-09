namespace Tomorrowify;

public static class EnvironmentVariables
{
    public static string? ASPNETCORE_ENVIRONMENT => Environment.GetEnvironmentVariable(nameof(ASPNETCORE_ENVIRONMENT));
    public static bool IsDevelopment(this string environment) => environment.ToLowerInvariant() == Environments.Development.ToLowerInvariant();
}
