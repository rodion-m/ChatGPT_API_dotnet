﻿using OpenAI;
using OpenAI.ChatGpt;
using OpenAI.ChatGpt.Models;
using Spectre.Console;
using Console = Spectre.Console.AnsiConsole;

Console.MarkupLine("[underline yellow]Welcome to ChatGPT Console![/]");
Console.MarkupLine("[underline red]Press Ctrl+C to stop writing[/]");

Console.WriteLine();


var name = Console.Ask<string?>("What's your [green]name[/]?") ?? "Me";
var apiKey = LoadApiKey();
Chat chat = await ChatGPT.CreateInMemoryChat(
    apiKey,
    config: new ChatCompletionsConfig() { MaxTokens = 200 },
    initialDialog: Dialog.StartAsSystem($"You are helpful assistant for a person named {name}.")
);
SetupCancellation(chat);

Console.MarkupLine("[underline yellow]Start chat. Now ask something ChatGPT...[/]");
while (AnsiConsole.Ask<string>($"[underline green]{name}[/]: ") is { } userMessage)
{
    AnsiConsole.Markup("[underline red]ChatGPT[/]: ");
    var stream = chat.StreamNextMessageResponse(userMessage)
        .ThrowOnCancellation(false);
    await foreach (string chunk in stream.SkipWhile(string.IsNullOrWhiteSpace))
    {
        if (!chat.IsCancelled) Console.Write(chunk);
    }

    Console.WriteLine();
}

string LoadApiKey()
{
    var s = Environment.GetEnvironmentVariable("openai_api_key_paid")
            ?? Console.Ask<string>("Please enter your [green]OpenAI API key[/] " +
                                   "(you can get it from https://platform.openai.com/account/api-keys): ");

    return s;
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