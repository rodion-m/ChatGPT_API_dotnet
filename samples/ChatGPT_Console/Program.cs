using ChatGPT_Console;
using OpenAI;
using OpenAI.ChatCompletions.Chat;
using OpenAI.ChatCompletions.Chat.Models;
using OpenAI.Models.ChatCompletion;
using Spectre.Console;

using Console = Spectre.Console.AnsiConsole;

Console.MarkupLine("[underline yellow]Welcome to ChatGPT Console![/]");
Console.MarkupLine("[underline red]Press Ctrl+C to stop writing[/]");

Console.WriteLine();

Chat chat = await CreateChat();
SetupCancellation();

var name = Console.Ask<string?>("What's your [green]name[/]?") ?? "Me";
Console.MarkupLine("[underline yellow]Start chat. Now ask something ChatGPT...[/]");
while (AnsiConsole.Ask<string>($"[underline green]{name}[/]: ") is { } userMessage)
{
    AnsiConsole.Markup("[underline red]ChatGPT[/]: ");
    var stream = chat.StreamNextMessageResponse(userMessage)
                                        .ThrowOnCancellation(false);
    await foreach (string chunk in stream.SkipWhile(string.IsNullOrWhiteSpace))
    {
        if(!chat.IsCancelled) Console.Write(chunk);
    }

    Console.WriteLine();
}

async Task<Chat> CreateChat()
{
    var key = Environment.GetEnvironmentVariable("openai_api_key_paid");
    if (key is null)
    {
        key = Console.Ask<string>(
            "Please enter your [green]OpenAI API key[/] (you can get it from https://platform.openai.com/account/api-keys): ");
    }
    ChatGPTFactory factory = ChatGPTFactory.CreateInMemory(
        key,
        new ChatCompletionsConfig()
        {
            Temperature = ChatCompletionTemperatures.Balanced
        });
    ChatGPT chatGpt = factory.Create();
    Chat chat = await chatGpt.StartNewChat();
    return chat;
}

void SetupCancellation()
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