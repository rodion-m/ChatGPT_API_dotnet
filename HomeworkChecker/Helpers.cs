using System;

namespace HomeworkChecker;

internal static class Helpers
{
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