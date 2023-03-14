using ChatGPT_Console;
using OpenAI.ChatCompletions.Chat;
using OpenAI.ChatCompletions.Chat.Models;
using OpenAI.Models.ChatCompletion;

ChatGPTFactory factory = ChatGPTFactory.CreateInMemory(Helpers.GetKeyFromEnvironment("openai_api_key_paid"),
    new ChatCompletionsConfig()
    {
        Temperature = ChatCompletionTemperatures.Balanced
    });
ChatGPT chatGpt = factory.Create(new ChatCompletionsConfig() {MaxTokens = 1000});
Chat chat = await chatGpt.StartNewChat();
Console.Write("User: ");
while (Console.ReadLine() is { } userMessage)
{
    Console.Write("ChatGPT: ");
    await foreach (string chunk in chat.StreamNextMessageResponse(userMessage))
    {
        Console.Write(chunk);
    }
    
    Console.WriteLine();
    Console.WriteLine();
    Console.Write("User: ");
}
