using OpenAI.ChatGpt;
using OpenAI.ChatGpt.Models;

Console.InputEncoding = System.Text.Encoding.Unicode;
Console.OutputEncoding = System.Text.Encoding.Unicode;

Console.WriteLine("Welcome to ChatGPT Console!");

var apiKey = LoadApiKey();
var config = new ChatCompletionsConfig() { MaxTokens = 300 };
Chat chat = await ChatGPT.CreateInMemoryChat(apiKey, config,
    initialDialog: Dialog.StartAsSystem(
        "Ты русскоязычный бот, который предоставляет шпаргалки для пользователя. Контекст: IT." +
        "Пользователь пишет тебе слово или слосочетание, а ты должен ответить что это слово означает. " +
        "Отвечай коротко и понятно. А еще пользователь может добавлять список определений, которые ты должен запомнить."
        )
    );

Console.Write("User: ");
while (Console.ReadLine() is { } userMessage)
{
    var response = await chat.GetNextMessageResponse(userMessage);
    Console.WriteLine($"ChatGPT: {response.Trim()}");
    Console.Write("User: ");
}


string LoadApiKey()
{
    var key = Environment.GetEnvironmentVariable("openai_api_key");
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