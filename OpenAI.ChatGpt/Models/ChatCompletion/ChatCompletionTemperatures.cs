namespace OpenAI.ChatGpt.Models.ChatCompletion;

public static class ChatCompletionTemperatures
{
    public const float Default = Balanced;

    public const float Deterministic = 0.2f;
    public const float Balanced = 0.5f;
    public const float Random = 0.8f;
    public const float Creative = 1f;
    public const float Insane = 2f;
    
    public const float Minimum = 0f;
    public const float Maximum = 2f;

    /// <summary>
    /// Validates temperature and returns value (between 0 and 2)
    /// </summary>
    /// <param name="temperature">Must be between 0 and 2</param>
    /// <throws>
    /// <see cref="ArgumentOutOfRangeException"/> if temperature is not between 0 and 2
    /// </throws>
    /// <returns>Validated temperature</returns>
    public static float Custom(float temperature) => VaidateTemperature(temperature);

    internal static float VaidateTemperature(float temperature)
    {
        if (temperature is < Minimum or > Maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(temperature), 
                $"Temperature must be between {Minimum} and {Maximum}, but was {temperature}");
        }
        
        return temperature;
    }
}