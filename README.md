# OpenAI Chat Completions (ChatGPT) client for .NET
This is a .NET client for the OpenAI Chat Completions API (ChatGPT). It allows you to use the API in your .NET applications. Also, the client supports streaming responses (like ChatGPT) via async streams.

## Preparation
First, you need to create an OpenAI account and get an API key. You can do this at https://platform.openai.com/account/api-keys.

## Installation
You can install the package via NuGet:
```
Install-Package OpenAI.ChatGPT
```
Then create an instance of `OpenAIClient`:
```csharp
_client = new OpenAiClient("{YOUR_OPENAI_API_KEY}");
```

## Simple usage of the Chat Completions API
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
ChatCompletionDialog dialog = 
    Dialog.StartAsUser("How many meters are in a kilometer? Write just the number.") //the message from user
          .ThenAssistant("1000") // response from the assistant
          .ThenUser("Convert it to hex. Write just the number."); // the next message from user

await foreach (var chunk in _client.StreamChatCompletions(dialog, maxTokens: 80))
{
    Console.Write(chunk);
}
```
Or just send message history as a collection.

## OpenAI Images API (text-to-image)
### Generate image bytes
```csharp
byte[] image = await _client.GenerateImageBytes("bicycle", "test", OpenAiImageSize._256);
```
### Generate images uris
```csharp
Uri[] uris = await _client.GenerateImagesUris("bicycle", "test", OpenAiImageSize._256, count: 2);
```
More examples see in the tests.