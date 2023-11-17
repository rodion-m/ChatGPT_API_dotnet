using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema;
using Json.Schema.Generation;
using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;
using static OpenAI.ChatGpt.Models.ChatCompletion.Messaging.ChatCompletionMessage;

namespace OpenAI.ChatGpt.Modules.StructuredResponse;

[Fody.ConfigureAwait(false)]
public static class OpenAiClientExtensions
{
    private static readonly SchemaGeneratorConfiguration SchemaGeneratorConfiguration = new()
    {
        Nullability = Nullability.Disabled,
        PropertyOrder = PropertyOrder.AsDeclared,
        PropertyNameResolver = PropertyNameResolvers.AsDeclared
    };
    private static readonly JsonSerializerOptions JsonSchemaSerializerOptions = new()
    {
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions JsonDefaultSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false
    };

    /// <summary>
    /// Asynchronously sends a chat completion request to the OpenAI API and deserializes the response to a specific object type.
    /// </summary>
    /// <typeparam name="TObject">The type of object to deserialize the response to. The type must have a parameterless constructor.</typeparam>
    /// <param name="client">The OpenAI client.</param>
    /// <param name="dialog">The chat dialog, including a user message and any system messages that set the behavior of the assistant.</param>
    /// <param name="maxTokens">Optional. The maximum number of tokens in the response. Defaults to the limit of the model, minus the number of input tokens, minus 500.</param>
    /// <param name="model">Optional. The name of the model to use. Defaults to <see cref="ChatCompletionModels.Gpt4"/>. It's recommended to use GPT4+.</param>
    /// <param name="temperature">Controls the randomness of the assistant’s output. Ranges from 0.0 to 1.0, where 0.0 is deterministic and 1.0 is highly random. Default value is the default for the OpenAI API.</param>
    /// <param name="user">Optional. The user ID who is having the conversation.</param>
    /// <param name="requestModifier">Optional. A function that can modify the chat completion request before it is sent to the API.</param>
    /// <param name="rawResponseGetter">Optional. A function that can access the raw API response.</param>
    /// <param name="jsonDeserializerOptions">Optional. Custom JSON deserializer options for the deserialization. If not specified, default options with case insensitive property names are used.</param>
    /// <param name="jsonSerializerOptions">Optional. Custom JSON serializer options for the serialization.</param>
    /// <param name="examples">Optional. Example of the models those will be serialized using <paramref name="jsonSerializerOptions"/>.</param>
    /// <param name="cancellationToken">Optional. A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the deserialized object from the API response.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="client"/> or <paramref name="dialog"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response from the API cannot be deserialized to the specified object type.</exception>
    /// <remarks>
    /// The method modifies the content of the message in the dialog to include a request for a JSON-formatted response.
    /// The original message content is restored after the API call.
    /// </remarks>
    public static Task<TObject> GetStructuredResponse<TObject>(
        this IOpenAiClient client,
        UserOrSystemMessage dialog,
        int? maxTokens = null,
        string? model = null,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        JsonSerializerOptions? jsonDeserializerOptions = null,
        JsonSerializerOptions? jsonSerializerOptions = null,
        IEnumerable<TObject>? examples = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(dialog);
        var responseFormat = CreateResponseFormatJson<TObject>();

        return GetStructuredResponse<TObject>(
            client,
            dialog: dialog,
            responseFormat: responseFormat,
            maxTokens: maxTokens,
            model: model,
            temperature: temperature,
            user: user,
            requestModifier: requestModifier,
            rawResponseGetter: rawResponseGetter,
            jsonDeserializerOptions: jsonDeserializerOptions,
            jsonSerializerOptions: jsonSerializerOptions,
            examples: examples,
            cancellationToken: cancellationToken
        );
    }

    internal static async Task<TObject> GetStructuredResponse<TObject>(
        this IOpenAiClient client,
        UserOrSystemMessage dialog,
        string responseFormat,
        int? maxTokens = null,
        string? model = null,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        JsonSerializerOptions? jsonDeserializerOptions = null,
        JsonSerializerOptions? jsonSerializerOptions = null,
        IEnumerable<TObject>? examples = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(dialog);

        var editMsg = dialog.GetMessages()
                          .FirstOrDefault(it => it is SystemMessage)
                      ?? dialog.GetMessages()[0];
        var originalContent = editMsg.Content;
        try
        {
            editMsg.Content += GetAdditionalJsonResponsePrompt(responseFormat, examples, jsonSerializerOptions);

            (model, maxTokens) = FindOptimalModelAndMaxToken(
                dialog.GetMessages(), 
                model, 
                maxTokens,
                smallModel: ChatCompletionModels.Gpt4,
                bigModel: ChatCompletionModels.Gpt4
            );

            var response = await client.GetChatCompletions(
                dialog,
                maxTokens.Value,
                model,
                temperature,
                user,
                ChatCompletionModels.IsJsonModeSupported(model),
                null,
                requestModifier,
                rawResponseGetter,
                cancellationToken
            );

            var deserialized = DeserializeOrThrow<TObject>(jsonDeserializerOptions, response);
            return deserialized;
        }
        finally
        {
            editMsg.Content = originalContent;
        }
    }

    private static TObject DeserializeOrThrow<TObject>(JsonSerializerOptions? jsonDeserializerOptions, string response)
    {
        ArgumentNullException.ThrowIfNull(response);
        response = response.Trim();
        if (response[0] == '`')
        {
            if (response.StartsWith("```json") && response.EndsWith("```"))
            {
                response = response[7..^3];
            }
            else if (response.StartsWith("```") && response.EndsWith("```"))
            {
                response = response[3..^3];
            }
        }

        if(!response.StartsWith('{') || !response.EndsWith('}'))
        {
            var (openBracketIndex, closeBracketIndex) = FindFirstAndLastBracket(response);
            response = response[openBracketIndex..(closeBracketIndex + 1)];
        }

        jsonDeserializerOptions ??= new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        TObject? deserialized;
        try
        {
            deserialized = JsonSerializer.Deserialize<TObject>(response, jsonDeserializerOptions);
            if (deserialized is null)
            {
                throw new InvalidJsonException(
                    $"Failed to deserialize response to {typeof(TObject)}. Response: {response}.", response);
            }
        }
        catch (JsonException jsonException)
        {
            throw new InvalidJsonException(
                $"Failed to deserialize response to {typeof(TObject)}. Response: {response}.", response, jsonException);
        }

        return deserialized;
        
        static (int openBracketIndex, int closeBracketIndex) FindFirstAndLastBracket(string response)
        {
            ArgumentNullException.ThrowIfNull(response);
            var openBracketIndex = response.IndexOf('{');
            var closeBracketIndex = response.LastIndexOf('}');
            if (openBracketIndex < 0 || closeBracketIndex < 0)
            {
                throw new InvalidJsonException(
                    $"Failed to deserialize response to {typeof(TObject)}. Response: {response}.", response);
            }
            return (openBracketIndex, closeBracketIndex);
        }
    }

    private static string GetAdditionalJsonResponsePrompt<TObject>(
        string responseFormat, IEnumerable<TObject>? examples, JsonSerializerOptions? jsonSerializerOptions)
    {
        var res = $"\n\nYour output must be strictly in valid, readable, iterable RFC8259 compliant JSON without any extra text. " +
              $"Here is the output structure (JSON Schema):\n```json\n{responseFormat}\n```";
        
        if (examples is not null) 
        {
            jsonSerializerOptions ??= JsonDefaultSerializerOptions;
            var examplesString = string.Join("\n", examples.Select(it => JsonSerializer.Serialize(it, jsonSerializerOptions)));
            res += $"\n\nExamples:\n```json\n{examplesString}\n```";
        }

        return res;
    }

    internal static string CreateResponseFormatJson<TObject>()
    {
        var schemaBuilder = new JsonSchemaBuilder();
        var schema = schemaBuilder.FromType<TObject>(SchemaGeneratorConfiguration).Build();
        var schemaString = JsonSerializer.Serialize(schema, JsonSchemaSerializerOptions);
        return schemaString;
    }
}