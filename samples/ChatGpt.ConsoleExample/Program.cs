using OpenAI.ChatGpt;

Console.WriteLine("Welcome to ChatGPT Console!");

var apiKey = LoadApiKey();
Chat chat = await ChatGPT.CreateInMemoryChat(apiKey);

Console.Write("User: ");
while (Console.ReadLine() is { } userMessage)
{
    var response = await chat.GetNextMessageResponse(userMessage);
    Console.WriteLine($"ChatGPT: {response.Trim()}");
    Console.Write("User: ");
}


string LoadApiKey()
{
    var s = Environment.GetEnvironmentVariable("openai_api_key_paid");
    if (s is null)
    {
        Console.WriteLine("Please enter your OpenAI API key " +
                            "(you can get it from https://platform.openai.com/account/api-keys): ");
        s = Console.ReadLine();
    }

    return s;
}