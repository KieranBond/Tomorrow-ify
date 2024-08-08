namespace Tomorrowify.Configuration;

public sealed class TomorrowifyConfiguration
{
    public string? ClientSecret { get; set; }
    public string WebsiteUri { get; set; } = "http://localhost:8080";
    public string CallbackUri { get; set; } = "http://localhost:8080/#cta";
    public Dynamo? Dynamo { get; set; } = null;
}

public sealed class Dynamo
{
    public string? ServiceUrl { get; set; } = null;
}