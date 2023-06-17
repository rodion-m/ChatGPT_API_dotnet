namespace OpenAI.Tests.Shared;

public static class Helpers
{
    public static string GetOpenAiKey() => GetKeyFromEnvironment("OPENAI_API_KEY");
    
    public static string? NullIfEmpty(this string? str)
    {
        return string.IsNullOrEmpty(str) ? null : str;
    }
    
    public static string GetKeyFromEnvironment(string keyName)
    {
        if (keyName == null) throw new ArgumentNullException(nameof(keyName));
        var value = Environment.GetEnvironmentVariable(keyName);
        if (value is null)
        {
            throw new InvalidOperationException($"{keyName} is not set as environment variable");
        }

        return value;
    }
}