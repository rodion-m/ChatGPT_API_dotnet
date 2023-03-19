namespace OpenAI.Models.ChatCompletion;

public static class ChatCompletionRoles
{
    public const string System = "system";
    public const string User = "user";
    public const string Assistant = "assistant";
    
    public static bool IsValid(string role) => role is System or User or Assistant;
    
    public static void ThrowIfInvalid(string role)
    {
        if (!IsValid(role))
            throw new ArgumentException($"Invalid role: {role}");
    }
}