using OpenAI.ChatGpt;
using OpenAI.ChatGpt.Models;

Console.InputEncoding = System.Text.Encoding.Unicode;
Console.OutputEncoding = System.Text.Encoding.Unicode;

Console.WriteLine("Welcome to ChatGPT Console!");

var apiKey = LoadApiKey();
var config = new ChatCompletionsConfig() { MaxTokens = 300 };
await using Chat chat = await ChatGPT.CreateInMemoryChat(apiKey, config);

Console.Write("User: ");
while (Console.ReadLine() is { } userMessage)
{
    var response = await chat.GetNextMessageResponse(userMessage);
    Console.WriteLine($"ChatGPT: {response.Trim()}");
    Console.Write("User: ");
}


string LoadApiKey()
{
    var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    if (key is null)
    {
        Console.WriteLine("Please enter your OpenAI API key " +
                            "(you can get it from https://platform.openai.com/account/api-keys): ");
        key = Console.ReadLine();
        if (key is null)
        {
            throw new Exception("API key is not provided");
        }
    }

    return key;
}