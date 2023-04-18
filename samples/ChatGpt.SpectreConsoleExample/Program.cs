using OpenAI.ChatGpt;
using OpenAI.ChatGpt.Models;
using Console = Spectre.Console.AnsiConsole;

Console.MarkupLine("[underline yellow]Welcome to ChatGPT Console![/]");
Console.MarkupLine("[underline red]Press Ctrl+C to stop writing[/]");

Console.WriteLine();


var name = Console.Ask<string?>("What's your [green]name[/]?") ?? "Me";
var apiKey = LoadApiKey();
await using Chat chat = await ChatGPT.CreateInMemoryChat(
    apiKey,
    config: new ChatCompletionsConfig() { MaxTokens = 200 },
    initialDialog: Dialog.StartAsSystem($"You are helpful assistant for a person named {name}.")
);
SetupCancellation(chat);

Console.MarkupLine("[underline yellow]Start chat. Now ask something ChatGPT...[/]");
while (Console.Ask<string>($"[underline green]{name}[/]: ") is { } userMessage)
{
    Console.Markup("[underline red]ChatGPT[/]: ");
    var stream = chat.StreamNextMessageResponse(userMessage, throwOnCancellation: false);
    await foreach (string chunk in stream.SkipWhile(string.IsNullOrWhiteSpace))
    {
        if (!chat.IsCancelled) Console.Write(chunk);
    }

    Console.WriteLine();
}

string LoadApiKey()
{
    var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? Console.Ask<string>("Please enter your [green]OpenAI API key[/] " +
                                   "(you can get it from https://platform.openai.com/account/api-keys): ");

    return key;
}

void SetupCancellation(Chat chat)
{
    System.Console.CancelKeyPress += (_, args) =>
    {
        if (chat.IsWriting && !chat.IsCancelled)
        {
            chat.Stop();
            args.Cancel = true;
            Console.Write($"...{Environment.NewLine}Stopped. Press Ctrl+C again to exit.");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("Bye!");
        }
    };
}