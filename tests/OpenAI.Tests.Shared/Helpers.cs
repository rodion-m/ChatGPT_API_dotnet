using Microsoft.Extensions.Configuration;

namespace OpenAI.Tests.Shared;

public static class Helpers
{
    private static IConfiguration Configuration { get; set; }

    static Helpers()
    {
        Configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(typeof(Helpers).Assembly)
            .Build();
    }

    public static string GetOpenAiKey() 
        => GetRequiredValueFromConfiguration("OPENAI_API_KEY");
    public static string GetOpenRouterKey() 
        => GetRequiredValueFromConfiguration("OPENROUTER_API_KEY");


    public static string? NullIfEmpty(this string? str)
    {
        return string.IsNullOrEmpty(str) ? null : str;
    }

    public static string GetRequiredValueFromConfiguration(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        var value = Configuration[key];
        if (value is null or { Length: 0 })
        {
            throw new InvalidOperationException($"{key} is not set in configuration");
        }

        return value;
    }
}