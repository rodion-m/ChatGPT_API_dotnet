# OpenAI Chat Completions (ChatGPT) client for .NET
## Preparation
First, you need to create an OpenAI account and get an API key. You can do this at https://beta.openai.com/account/api-keys.
## Installation
You can install the package via NuGet:
```
Install-Package OpenAI.ChatGPT
```
Create an instance of `OpenAIClient`:
```csharp
_client = new OpenAiClient("{YOUR_OPENAI_API_KEY}");
```
```
## Simple usage of the Chat Completions API
```csharp
string text = "Who are you?";
string response = await _client.GetChatCompletions(new UserMessage(text), 80);
Console.WriteLine(response);
```

## Streaming response with async streams (like ChatGPT)
```csharp
var text = "Write the world top 3 songs of Soul genre";
await foreach (string chunk in _client.StreamChatCompletions(new UserMessage(text), 80))
{
    Console.Write(chunk);
}
```

## Continue dialog with ChatGPT (send messages history)
Simply do that using `ThenAssistant` and `ThenUser` methods:
```csharp
ChatCompletionDialog dialog = 
    new UserMessage("How many meters are in a kilometer? Write just the number.")
    .ThenAssistant("1000")
    .ThenUser("Convert it to hex. Write just the number.");

await foreach (var chunk in _client.StreamChatCompletions(dialog, 80))
{
    Console.Write(chunk);
}
```

## OpenAI Images API (text-to-image)
### Generate image bytes
```csharp

```
### Generate images uris
```csharp
Uri[] uris = await _client.GenerateImagesUris("bicycle", "test", OpenAiImageSize._256, count: 2);
```
More examples see in the tests.