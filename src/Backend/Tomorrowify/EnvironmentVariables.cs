namespace Tomorrowify;

public static class EnvironmentVariables
{
    public static string? ASPNETCORE_ENVIRONMENT => Environment.GetEnvironmentVariable(nameof(ASPNETCORE_ENVIRONMENT));
}
