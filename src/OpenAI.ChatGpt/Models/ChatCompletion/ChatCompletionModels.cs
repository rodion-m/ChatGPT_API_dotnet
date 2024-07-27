using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0618

namespace OpenAI.ChatGpt.Models.ChatCompletion;

/// <summary>
/// Provides access to OpenAI GPT models as string constants.
/// </summary>
/// <remarks>
/// See:
/// https://platform.openai.com/docs/models/
/// </remarks>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class ChatCompletionModels
{
    public const string Default = Gpt4o;

    /// <summary>
    /// GPT-4o ("o" for "omni") is the most advanced model. It is multimodal, accepting text or image inputs and outputting text.
    /// It has the same high intelligence as GPT-4 Turbo but is more efficient, generating text 2x faster and being 50% cheaper.
    /// This model has a maximum token limit of 128,000.
    /// The model was trained with data up to October 2023.
    /// </summary>
    public const string Gpt4o = "gpt-4o";

    /// <summary>
    /// The specific version of GPT-4o that gpt-4o currently points to.
    /// This model has a maximum token limit of 128,000.
    /// The model was trained with data up to October 2023.
    /// </summary>
    public const string Gpt4o_2024_05_13 = "gpt-4o-2024-05-13";

    /// <summary>
    /// GPT-4o mini is the most advanced model in the small models category and the cheapest model.
    /// It is multimodal, accepting text or image inputs and outputting text.
    /// It has higher intelligence than gpt-3.5-turbo but is just as fast.
    /// This model has a maximum token limit of 128,000.
    /// The model was trained with data up to October 2023.
    /// </summary>
    public const string Gpt4o_Mini = "gpt-4o-mini";

    /// <summary>
    /// The specific version of GPT-4o mini that gpt-4o-mini currently points to.
    /// This model has a maximum token limit of 128,000.
    /// The model was trained with data up to October 2023.
    /// </summary>
    public const string Gpt4o_Mini_2024_07_18 = "gpt-4o-mini-2024-07-18";

    /// <summary>
    /// The latest GPT-4 Turbo model with vision capabilities.
    /// Vision requests can now use JSON mode and function calling.
    /// This model has a maximum token limit of 128,000.
    /// The model was trained with data up to December 2023.
    /// </summary>
    public const string Gpt4Turbo = "gpt-4-turbo";

    /// <summary>
    /// GPT-4 Turbo with Vision model. Vision requests can now use JSON mode and function calling.
    /// gpt-4-turbo currently points to this version.
    /// This model has a maximum token limit of 128,000.
    /// The model was trained with data up to December 2023.
    /// </summary>
    public const string Gpt4Turbo_2024_04_09 = "gpt-4-turbo-2024-04-09";

    /// <summary>
    /// GPT-4 Turbo preview model. Currently points to gpt-4-0125-preview.
    /// This model has a maximum token limit of 128,000.
    /// The model was trained with data up to December 2023.
    /// </summary>
    public const string Gpt4TurboPreview = "gpt-4-turbo-preview";

    /// <summary>
    /// GPT-4 Turbo preview model intended to reduce cases of "laziness" where the model doesn't complete a task.
    /// Returns a maximum of 4,096 output tokens.
    /// This model has a maximum token limit of 128,000.
    /// The model was trained with data up to December 2023.
    /// </summary>
    public const string Gpt4_0125_Preview = "gpt-4-0125-preview";

    /// <summary>
    /// GPT-4 Turbo preview model featuring improved instruction following, JSON mode, reproducible outputs, parallel function calling, and more.
    /// Returns a maximum of 4,096 output tokens.
    /// This model has a maximum token limit of 128,000.
    /// The model was trained with data up to April 2023.
    /// </summary>
    public const string Gpt4_1106_Preview = "gpt-4-1106-preview";

    /// <summary>
    /// Currently points to gpt-4-0613.
    /// This model has a maximum token limit of 8,192.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt4 = "gpt-4";

    /// <summary>
    /// Snapshot of gpt-4 from June 13th 2023 with improved function calling support.
    /// This model has a maximum token limit of 8,192.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt4_0613 = "gpt-4-0613";

    /// <summary>
    /// Legacy Snapshot of gpt-4 from March 14th 2023.
    /// This model has a maximum token limit of 8,192.
    /// The model was trained with data up to September 2021.
    /// </summary>
    [Obsolete("Legacy. Snapshot of gpt-4 from March 14th 2023. Use Gpt4 instead.")]
    public const string Gpt4_0314 = "gpt-4-0314";

    /// <summary>
    /// Currently points to gpt-3.5-turbo-0125.
    /// This model has a maximum token limit of 16,385.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt3_5_Turbo = "gpt-3.5-turbo";

    /// <summary>
    /// The latest GPT-3.5 Turbo model with higher accuracy at responding in requested formats and a fix for a bug which caused a text encoding issue for non-English language function calls.
    /// Returns a maximum of 4,096 output tokens.
    /// This model has a maximum token limit of 16,385.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt3_5_Turbo_0125 = "gpt-3.5-turbo-0125";

    /// <summary>
    /// GPT-3.5 Turbo model with improved instruction following, JSON mode, reproducible outputs, parallel function calling, and more.
    /// Returns a maximum of 4,096 output tokens.
    /// This model has a maximum token limit of 16,385.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt3_5_Turbo_1106 = "gpt-3.5-turbo-1106";

    /// <summary>
    /// Similar capabilities as GPT-3 era models. Compatible with legacy Completions endpoint and not Chat Completions.
    /// This model has a maximum token limit of 4,096.
    /// The model was trained with data up to September 2021.
    /// </summary>
    public const string Gpt3_5_Turbo_Instruct = "gpt-3.5-turbo-instruct";

    private static readonly string[] ModelsSupportedJson = {
        Gpt4Turbo, Gpt4Turbo_2024_04_09, Gpt4TurboPreview, Gpt4_0125_Preview, Gpt4_1106_Preview,
        Gpt3_5_Turbo_1106, Gpt3_5_Turbo_0125
    };
    
    /// <summary>
    /// The maximum number of tokens that can be processed by the model.
    /// </summary>
    private static readonly Dictionary<string, int> MaxTokensLimits = new()
    {
        { Gpt4o, 128_000 },
        { Gpt4o_2024_05_13, 128_000 },
        { Gpt4o_Mini, 128_000 },
        { Gpt4o_Mini_2024_07_18, 128_000 },
        { Gpt4Turbo, 128_000 },
        { Gpt4Turbo_2024_04_09, 128_000 },
        { Gpt4TurboPreview, 128_000 },
        { Gpt4_0125_Preview, 128_000 },
        { Gpt4_1106_Preview, 128_000 },
        { Gpt4, 8_192 },
        { Gpt4_0613, 8_192 },
        { Gpt4_0314, 8_192 },
        { Gpt3_5_Turbo, 16_385 },
        { Gpt3_5_Turbo_0125, 16_385 },
        { Gpt3_5_Turbo_1106, 16_385 },
        { Gpt3_5_Turbo_Instruct, 4_096 },
    };
    
    private static int _validateModelName = 0;
    
    
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
            throw new ArgumentException("Model cannot be empty or whitespace.", nameof(model));
        }
        if (_validateModelName == 1 && !MaxTokensLimits.ContainsKey(model))
        {
            throw new ArgumentException($"Invalid model: {model}", nameof(model));
        }
        return model;
    }

    public static void EnsureMaxTokensIsSupported(string model, int maxTokens)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (maxTokens < 1) throw new ArgumentOutOfRangeException(nameof(maxTokens));
        if (!MaxTokensLimits.TryGetValue(model, out var limit))
        {
            if (_validateModelName == 1)
            {
                throw new ArgumentException($"Invalid model: {model}", nameof(model));
            }
            return;
        }

        if (maxTokens > limit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxTokens), 
                $"Max tokens must be less than or equal to {limit} for model {model} but was {maxTokens}"
            );
        }
    }

    public static void EnsureMaxTokensIsSupportedByAnyModel(int maxTokens)
    {
        var limit = MaxTokensLimits.Values.Max();
        if (maxTokens > limit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxTokens), $"Max tokens must be less than or equal to {limit} but was {maxTokens}");
        }
    }

    /// <summary>
    /// Checks if the model name is supported for JSON mode
    /// </summary>
    /// <param name="model">GPT model name</param>
    /// <returns>True if the model is supported for JSON mode</returns>
    public static bool IsJsonModeSupported(string model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return Array.IndexOf(ModelsSupportedJson, model) != -1;
    }

    internal static IReadOnlyList<string> GetModelsThatSupportJsonMode()
    {
        return ModelsSupportedJson;
    }
}