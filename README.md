[![example gif...](assets/chatgpt_console_spectre_example.gif)](samples/ChatGpt.SpectreConsoleExample/Program.cs)
# ChatGPT integration for .NET
[![Nuget](https://img.shields.io/nuget/v/OpenAI.ChatGPT.EntityFrameworkCore)](https://www.nuget.org/packages/OpenAI.ChatGPT.EntityFrameworkCore/)[![.NET](https://github.com/rodion-m/ChatGPT_API_dotnet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/rodion-m/ChatGPT_API_dotnet/actions/workflows/dotnet.yml) \
OpenAI Chat Completions API (ChatGPT) integration with DI and EF Core supporting. It allows you to use the API in your .NET applications. Also, the client supports streaming responses (like ChatGPT) via async streams.

## Preparation
First, you need to create an OpenAI account and get an API key. You can do this at https://platform.openai.com/account/api-keys.

## Installation
The easiest way to use ChatGPT service in your .NET project with DI and persistence (EF Core) supporting is to install the NuGet package [OpenAI.ChatGPT.EntityFrameworkCore](https://www.nuget.org/packages/OpenAI.ChatGPT.EntityFrameworkCore/):
```
Install-Package OpenAI.ChatGPT.EntityFrameworkCore
```
If you don't want to use EF Core, you can install the package [OpenAI.ChatGPT.AspNetCore](https://www.nuget.org/packages/OpenAI.ChatGPT.AspNetCore/) and implement your own storage for chat history, using `IChatHistoryStorage` interface.

## Usage
1. Set the OpenAI API key or even host (optional) in your project user secrets, or the `appsettings.json` file (not safe):
```json
{
  "OpenAICredentials": {
    "ApiKey": "your-api-key-from-openai",
    "ApiHost": "https://api.openai.com/v1/"
  }
}
```
Also, you can specify OpenAI API key as environment variable `ASPNETCORE_OpenAICredentials:ApiKey`.

2. Add ChatGPT integration with EF to your DI container:
```csharp
builder.Services.AddChatGptEntityFrameworkIntegration(
    options => options.UseSqlite("Data Source=chats.db"));
```
Instead of `options.UseSqlite("Data Source=chats.db")` use your own db and connection string.

3. Inject `ChatGPTFactory` to your service and use it to create `ChatGPT` instance:
```csharp
public class YourService
{
    private readonly ChatGPTFactory _chatGptFactory;

    public YourService(ChatGPTFactory chatGptFactory)
    {
        _chatGptFactory = chatGptFactory;
    }

    public async Task<string> GetAnswer(string text)
    {
        ChatGPT chatGpt = await _chatGptFactory.Create(userId);
        var chatService = await chatGpt.ContinueOrStartNewTopic();
        response = await _chatService.GetNextMessageResponse(_prompt);
        return response;
    }
}
```
See [Blazor Example](samples/ChatGpt.BlazorExample/Pages/Index.razor).

If you want to configure request parameters, you can do it in `appsettings.json` configuration or in `ChatGPTFactory.Create` or in `ChatGPT.CreateChat` methods.
```json
{
  "ChatGPTConfig": {
      "InitialSystemMessage": null,
      "InitialUserMessage": null,
      "MaxTokens": null,
      "Model": null,
      "Temperature": null,
      "PassUserIdToOpenAiRequests": true
  }
}
```
See parameters description inside [ChatGPTConfig](OpenAI.ChatGpt/Models/ChatGPTConfig.cs).

## Exceptions
If the server response is not a success status code, the client will throw a [NotExpectedResponseException](OpenAI.ChatGpt/Exceptions/NotExpectedResponseException.cs). The exception will contain the error message from the OpenAI API. \
By default, requesting cancellation or `ChatService.Stop()` method calling will throw `OperationCanceledException`. If you don't want to throw it (relevant for streaming responses), you can set `throwOnCancellation` parameter to `false`:
```csharp
await foreach (string chunk in chatService.StreamNextMessageResponse(text, throwOnCancellation: false))
{
    //...
}
```

## Thread safety and async
`ChatGPTFactory`, `ChatGPT` classes thread-safety is depend on the `IChatHistoryStorage` implementation. If you use `ChatGPTFactory` with entity framework, it's NOT thread-safe. `ChatService` class is not thread-safe. \
Anyways, this services are designed to be used safely with DI, so you don't need to worry about it. \
All the methods from all the packages are designed to be used in async context and use `ConfigureAwait(false)` (thanks for the `ConfigureAwait.Fody` package).

## Retries, timeouts and other policies
Since `ChatGPTFactory` depends on `IHttClientFactory`, you can easily use any of the available policies for it, like Polly.

## Examples
* [Blazor Example](samples/ChatGpt.BlazorExample)
* [Console Example](samples/ChatGpt.ConsoleExample/Program.cs) (simple)
* [Spectre Console Example](samples/ChatGpt.SpectreConsoleExample/Program.cs) (advanced)

## API Parameters
Here is a list of the main parameters that can be used in the ChatCompletions (ChatGPT) API request (OpenAI.ChatGpt/Models/ChatCompletion/ChatCompletionRequest.cs).
Some of them are taken from this article: https://towardsdatascience.com/gpt-3-parameters-and-prompt-design-1a595dc5b405 \
Below listed parameters for ChatCompletions API.

### Model
The prediction-generating AI model is specified by the engine parameter. The available models are:
*   `ChatCompletionModels.Gpt3_5_Turbo` (Default): Most capable GPT-3.5 model and optimized for chat at 1/10th the cost of text-davinci-003. Will be updated with OpenAI's latest model iteration.
*   `ChatCompletionModels.Gpt3_5_Turbo_0301`: Snapshot of gpt-3.5-turbo from March 1st 2023. Unlike gpt-3.5-turbo, this model will not receive updates, and will only be supported for a three month period ending on June 1st 2023.
*   `ChatCompletionModels.Gpt4`: More capable than any GPT-3.5 model, able to do more complex tasks, and optimized for chat. Will be updated with OpenAI's latest model iteration. \*
*   `ChatCompletionModels.Gpt4_0314`: Snapshot of gpt-4 from March 14th 2023. Unlike gpt-4, this model will not receive updates, and will only be supported for a three month period ending on June 14th 2023. \*
*   `ChatCompletionModels.Gpt4_32k`: Same capabilities as the base gpt-4 mode but with 4x the context length. Will be updated with OpenAI's latest model iteration. \*
*   `ChatCompletionModels.Gpt4_32k_0314`: Snapshot of gpt-4-32 from March 14th 2023. Unlike gpt-4-32k, this model will not receive updates, and will only be supported for a three month period ending on June 14th 2023. \* \
    Note that training data for all models is up to Sep 2021. \
    \* These models are currently in beta and are not yet available to all users. Here is the link for joining waitlist: https://openai.com/waitlist/gpt-4-api

### MaxTokens
The maximum number of tokens allowed for the generated answer. Defaults to `ChatCompletionRequest.MaxTokensDefault` (64).
* This value is validated and limited with `ChatCompletionModels.GetMaxTokensLimitForModel` method.
* It's possible to calculate approximately tokens count using `ChatCompletionMessage.CalculateApproxTotalTokenCount` method
* The number of tokens can be retrieved from the API response: `ChatCompletionResponse.Usage.TotalTokens`.
  As a rule of thumb for English, 1 token is around 4 characters (so 100 tokens ≈ 75 words). See tokenizer from OpenAI: https://platform.openai.com/tokenizer
* Encoding algorithm can be found here: https://github.com/latitudegames/GPT-3-Encoder

### Temperature
What sampling temperature to use, between 0 and 2.
* Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
* Predefined values are available in `ChatCompletionTemperatures`.
* Default value is: `ChatCompletionTemperatures.Balanced` (0.5).

Description:
Before being mapped into probabilities, the model outputs unnormalized values (logits). The logits are typically used with a function such as softmax to convert them into probabilities.

But, before applying the softmax function, we can use a trick inspired by thermodynamics and scale the logits with the temperature parameter, i.e. softmax(logits/temperature).

A temperature parameter close to 1 would mean that the logits are passed through the softmax function without modification. If the temperature is close to zero, the highest probable tokens will become very likely compared to the other tokens, i.e. the model becomes more deterministic and will always output the same set of tokens after a given sequence of words.

More parameters description can be found here: Some of them are taken from this article: https://towardsdatascience.com/gpt-3-parameters-and-prompt-design-1a595dc5b405

# Using raw client without DI
If you don't need DI and chat history, you can use only the NuGet package [OpenAI.ChatGPT](https://www.nuget.org/packages/OpenAI.ChatGPT):
```
Install-Package OpenAI.ChatGPT
```
Then create an instance of `OpenAIClient`:
```csharp
_client = new OpenAiClient("{YOUR_OPENAI_API_KEY}");
```

## Simple usage of the Chat Completions API (raw client)
```csharp
string text = "Who are you?";
string response = await _client.GetChatCompletions(new UserMessage(text), maxTokens: 80);
Console.WriteLine(response);
```

## Streaming response with async streams (like ChatGPT)
```csharp
var text = "Write the world top 3 songs of Soul genre";
await foreach (string chunk in _client.StreamChatCompletions(new UserMessage(text), maxTokens: 80))
{
    Console.Write(chunk);
}
```

## Continue dialog with ChatGPT (message history)
Use `ThenAssistant` and `ThenUser` methods to create a dialog:
```csharp
var dialog = Dialog.StartAsUser("How many meters are in a kilometer? Write just the number.") //the message from user
          .ThenAssistant("1000") // response from the assistant
          .ThenUser("Convert it to hex. Write just the number."); // the next message from user

await foreach (var chunk in _client.StreamChatCompletions(dialog, maxTokens: 80))
{
    Console.Write(chunk);
}
```
Or just send message history as a collection.
