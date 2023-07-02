using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt;

public static class OpenAiClientExtensions
{
    /// <summary>
    /// Asynchronously sends a chat completion request to the OpenAI API and deserializes the response to a specific object type.
    /// </summary>
    /// <typeparam name="TObject">The type of object to deserialize the response to. The type must have a parameterless constructor.</typeparam>
    /// <param name="client">The OpenAI client.</param>
    /// <param name="dialog">The chat dialog, including a user message and any system messages that set the behavior of the assistant.</param>
    /// <param name="maxTokens">Optional. The maximum number of tokens in the response. Defaults to the limit of the model, minus the number of input tokens, minus 500.</param>
    /// <param name="model">Optional. The name of the model to use. Defaults to "text-davinci-002" unless the message input is longer than 6000 tokens, in which case it defaults to "text-davinci-003".</param>
    /// <param name="temperature">Controls the randomness of the assistant’s output. Ranges from 0.0 to 1.0, where 0.0 is deterministic and 1.0 is highly random. Default value is the default for the OpenAI API.</param>
    /// <param name="user">Optional. The user who is having the conversation. If not specified, defaults to "system".</param>
    /// <param name="requestModifier">Optional. A function that can modify the chat completion request before it is sent to the API.</param>
    /// <param name="rawResponseGetter">Optional. A function that can access the raw API response.</param>
    /// <param name="jsonSerializerOptions">Optional. Custom JSON serializer options for the response format. If not specified, default options are used.</param>
    /// <param name="jsonDeserializerOptions">Optional. Custom JSON deserializer options for the deserialization. If not specified, default options with case insensitive property names are used.</param>
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
        JsonSerializerOptions? jsonSerializerOptions = null,
        JsonSerializerOptions? jsonDeserializerOptions = null,
        CancellationToken cancellationToken = default) where TObject: new()
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(dialog);
        var responseFormat = CreateResponseFormatJson<TObject>(new TObject(), jsonSerializerOptions);

        return client.GetStructuredResponse<TObject>(
            dialog: dialog,
            responseFormat: responseFormat,
            maxTokens: maxTokens,
            model: model,
            temperature: temperature,
            user: user,
            requestModifier: requestModifier,
            rawResponseGetter: rawResponseGetter,
            jsonDeserializerOptions: jsonDeserializerOptions,
            cancellationToken: cancellationToken);
    }
    
    /// <summary>
    /// Asynchronously gets a response from the OpenAI API, and attempts to deserialize it into an instance of the specified type.
    /// </summary>
    /// <typeparam name="TObject">The type into which to deserialize the response.</typeparam>
    /// <param name="client">The OpenAI client.</param>
    /// <param name="dialog">The dialog to send to the OpenAI API.</param>
    /// <param name="responseExample">Is used to infer the expected structure of the response if no response format is explicitly specified.</param>
    /// <param name="maxTokens">(Optional) The maximum number of tokens for the model to generate. If null, the default is calculated.</param>
    /// <param name="model">(Optional) The model to use. If null, the default model is used.</param>
    /// <param name="temperature">(Optional) Controls randomness in the AI's output. Default is defined by ChatCompletionTemperatures.Default.</param>
    /// <param name="user">(Optional) User identifier. If null, the default user is used.</param>
    /// <param name="requestModifier">(Optional) Delegate for modifying the request.</param>
    /// <param name="rawResponseGetter">(Optional) Delegate for processing the raw response.</param>
    /// <param name="jsonSerializerOptions">(Optional) Options for the JSON serializer. If null, the default options are used.</param>
    /// <param name="jsonDeserializerOptions">(Optional) Options for the JSON deserializer. If null, case-insensitive property name matching is used.</param>
    /// <param name="cancellationToken">(Optional) A token that can be used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation, containing the deserialized response,
    /// or the default response if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> or <paramref name="dialog"/> or <paramref name="responseExample"/> is null.</exception>
    /// <remarks>
    /// This method modifies the content of the dialog to include a message instructing the AI to respond in a certain format.
    /// After the call to the API, the original content of the dialog is restored.
    /// </remarks>
    public static Task<TObject> GetStructuredResponse<TObject>(
        this IOpenAiClient client,
        UserOrSystemMessage dialog,
        TObject responseExample,
        int? maxTokens = null,
        string? model = null,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        JsonSerializerOptions? jsonSerializerOptions = null,
        JsonSerializerOptions? jsonDeserializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(dialog);
        ArgumentNullException.ThrowIfNull(responseExample);

        var responseFormat = CreateResponseFormatJson(responseExample, jsonSerializerOptions);
        return client.GetStructuredResponse<TObject>(
            dialog: dialog,
            responseFormat: responseFormat,
            maxTokens: maxTokens,
            model: model,
            temperature: temperature,
            user: user,
            requestModifier: requestModifier,
            rawResponseGetter: rawResponseGetter,
            jsonDeserializerOptions: jsonDeserializerOptions,
            cancellationToken: cancellationToken);
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
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(dialog);

        var editMsg = dialog.GetMessages().FirstOrDefault(it => it is SystemMessage)
                      ?? dialog.GetMessages().First();
        var originalContent = editMsg.Content;
        try
        {
            editMsg.Content += GetAdditionalJsonResponsePrompt(responseFormat);

            (model, maxTokens) = ChatCompletionMessage.FindOptimalModelAndMaxToken(dialog.GetMessages(), model, maxTokens);

            var response = await client.GetChatCompletions(
                dialog,
                maxTokens.Value,
                model,
                temperature,
                user,
                requestModifier,
                rawResponseGetter,
                cancellationToken);

            jsonDeserializerOptions ??= new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var deserialized = JsonSerializer.Deserialize<TObject>(response, jsonDeserializerOptions);
            if (deserialized is null)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize response to {typeof(TObject)}. Response: {response}");
            }

            return deserialized;
        }
        finally
        {
            editMsg.Content = originalContent;
        }
    }

    private static string GetAdditionalJsonResponsePrompt(string responseFormat)
    {
        return $"\n\nWrite your response in JSON format, which structure is enclosed within double backticks ``{responseFormat}``";
    }

    internal static string CreateResponseFormatJson<TObject>(
        TObject objectToDeserialize,
        JsonSerializerOptions? jsonSerializerOptions)
    {
        ArgumentNullException.ThrowIfNull(objectToDeserialize);
        if (jsonSerializerOptions is null)
        {
            jsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };
        }
        else
        {
            jsonSerializerOptions = new JsonSerializerOptions(jsonSerializerOptions)
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };
        }
        return JsonSerializer.Serialize(objectToDeserialize, jsonSerializerOptions);
    }
}