using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0618

namespace OpenAI.ChatGpt.Models.ChatCompletion;

/// <summary>
/// Provides access to OpenAI GPT models as string constants.
/// </summary>
/// <remarks>
/// Training data for all models is up to Sep 2021.
/// See:
/// https://platform.openai.com/docs/models/gpt-3-5
/// https://platform.openai.com/docs/models/gpt-4
/// </remarks>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class ChatCompletionModels
{
    public const string Default = Gpt3_5_Turbo;

    /// <summary>
    /// IMPORTANT: This model is available only by request. Link for joining waitlist: https://openai.com/waitlist/gpt-4-api
    /// More capable than any GPT-3.5 model, able to do more complex tasks, and optimized for chat.
    /// Will be updated with OpenAI's latest model iteration 2 weeks after it is released.
    /// This model has a maximum token limit of 8,192.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt4 = "gpt-4";

    /// <summary>
    /// IMPORTANT: This model is available only by request. Link for joining waitlist: https://openai.com/waitlist/gpt-4-api
    /// Snapshot of gpt-4 from June 13th 2023 with function calling data.
    /// Unlike gpt-4, this model will not receive updates, and will be deprecated 3 months after a new version is released.
    /// This model has a maximum token limit of 8,192.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt4_0613 = "gpt-4-0613";


    /// <summary>
    /// Same capabilities as the base gpt-4 mode but with 4x the context length.
    /// Will be updated with our latest model iteration.
    /// This model has a maximum token limit of 32,768.
    /// The model was trained with data up to September 2021.
    /// </summary>
    [Obsolete("This model is available only by request. " +
                  "Link for joining waitlist: https://openai.com/waitlist/gpt-4-api")]
    public const string Gpt4_32k = "gpt-4-32k";

    /// <summary>
    /// Snapshot of gpt-4-32 from June 13th 2023.
    /// Unlike gpt-4-32k, this model will not receive updates, and will be deprecated 3 months after a new version is released.
    /// This model has a maximum token limit of 32,768.
    /// The model was trained with data up to September 2021.
    /// </summary>
    [Obsolete("This model is available only by request. " +
              "Link for joining waitlist: https://openai.com/waitlist/gpt-4-api")]
    public const string Gpt4_32k_0613 = "gpt-4-32k-0613";

    /// <summary>
    /// Most capable GPT-3.5 model and optimized for chat at 1/10th the cost of text-davinci-003.
    /// Will be updated with our latest model iteration 2 weeks after it is released.
    /// This model has a maximum token limit of 4,096.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt3_5_Turbo = "gpt-3.5-turbo";

    /// <summary>
    /// Same capabilities as the standard gpt-3.5-turbo model but with 4 times the context.
    /// This model has a maximum token limit of 16,384.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt3_5_Turbo_16k = "gpt-3.5-turbo-16k";

    /// <summary>
    /// Snapshot of gpt-3.5-turbo from June 13th 2023 with function calling data.
    /// Unlike gpt-3.5-turbo, this model will not receive updates, and will be deprecated 3 months after a new version is released.
    /// This model has a maximum token limit of 4,096.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt3_5_Turbo_0613 = "gpt-3.5-turbo-0613";

    /// <summary>
    /// Snapshot of gpt-3.5-turbo-16k from June 13th 2023.
    /// Unlike gpt-3.5-turbo-16k, this model will not receive updates, and will be deprecated 3 months after a new version is released.
    /// This model has a maximum token limit of 16,384.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt3_5_Turbo_16k_0613 = "gpt-3.5-turbo-16k-0613";
    
    /// <summary>
    /// IMPORTANT: This model is available only by request. Link for joining waitlist: https://openai.com/waitlist/gpt-4-api
    /// Snapshot of gpt-4 from March 14th 2023.
    /// Unlike gpt-4, this model will not receive updates,
    /// and will only be supported for a three month period ending on June 14th 2023.
    /// </summary>
    [Obsolete("DISCONTINUATION DATE 09/13/2023")]
    public const string Gpt4_0314 = "gpt-4-0314";

    /// <summary>
    /// Snapshot of gpt-4-32 from March 14th 2023.
    /// Unlike gpt-4-32k, this model will not receive updates,
    /// and will only be supported for a three month period ending on June 14th 2023.
    /// </summary>
    [Obsolete("DISCONTINUATION DATE 09/13/2023. This model is available only by request. " +
              "Link for joining waitlist: https://openai.com/waitlist/gpt-4-api")]
    public const string Gpt4_32k_0314 = "gpt-4-32k-0314";

    /// <summary>
    /// Snapshot of gpt-3.5-turbo from March 1st 2023.
    /// Unlike gpt-3.5-turbo, this model will not receive updates,
    /// and will only be supported for a three month period ending on June 1st 2023.
    /// </summary>
    [Obsolete("DISCONTINUATION DATE 09/13/2023")]
    public const string Gpt3_5_Turbo_0301 = "gpt-3.5-turbo-0301";

    /// <summary>
    /// The maximum number of tokens that can be processed by the model.
    /// </summary>
    private static readonly Dictionary<string, int> MaxTokensLimits = new()
    {
        { Gpt4, 8192 },
        { Gpt4_0613, 8192 },
        { Gpt4_32k, 32768 },
        { Gpt4_32k_0613, 32768 },
        { Gpt3_5_Turbo, 4096 },
        { Gpt3_5_Turbo_16k, 16384 },
        { Gpt3_5_Turbo_0613, 4096 },
        { Gpt3_5_Turbo_16k_0613, 16384 },
        { Gpt4_0314, 8192 },
        { Gpt4_32k_0314, 32768 },
        { Gpt3_5_Turbo_0301, 4096 },
    };
    
    private static volatile bool _validateModelName = true;
    
    
    /// <summary>
    /// Gets the maximum number of tokens that can be processed by the model
    /// </summary>
    /// <param name="model">GPT model</param>
    public static int GetMaxTokensLimitForModel(string model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (!MaxTokensLimits.ContainsKey(model))
        {
            throw new ArgumentException($"Invalid model: {model}", nameof(model));
        }
        return MaxTokensLimits[model];
    }
    
    /// <summary>
    /// Checks if the model name is supported
    /// </summary>
    /// <param name="model">GPT model name</param>
    public static bool IsModelSupported(string model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(model));
        }
        return MaxTokensLimits.ContainsKey(model);
    }

    public static string FromString(string model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(model));
        }
        if (_validateModelName && !MaxTokensLimits.ContainsKey(model))
        {
            throw new ArgumentException($"Invalid model: {model}", nameof(model));
        }
        return model;
    }

    public static void DisableModelNameValidation()
    {
        _validateModelName = false;
    }
    
    public static void EnableModelNameValidation()
    {
        _validateModelName = true;
    }

    public static void EnsureMaxTokensIsSupported(string model, int maxTokens)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (maxTokens < 1) throw new ArgumentOutOfRangeException(nameof(maxTokens));
        if (!MaxTokensLimits.TryGetValue(model, out var limit))
        {
            throw new ArgumentException($"Invalid model: {model}", nameof(model));
        }
        if (maxTokens > limit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxTokens), 
                $"Max tokens must be less than or equal to {limit} for model {model}"
            );
        }
    }

    public static void EnsureMaxTokensIsSupportedByAnyModel(int maxTokens)
    {
        var limit = MaxTokensLimits.Values.Max();
        if (maxTokens > limit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxTokens), $"Max tokens must be less than or equal to {limit}");
        }
    }
}