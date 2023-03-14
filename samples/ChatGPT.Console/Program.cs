using ChatGPT.Console;
using OpenAI.ChatCompletions.Chat;

ChatGPTFactory factory = ChatGPTFactory.CreateInMemory(Helpers.GetKeyFromEnvironment("openai_api_key_paid"));
OpenAI.ChatCompletions.Chat.ChatGPT chatGpt = factory.Create();
var chat = await chatGpt.StartNewChat();
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
