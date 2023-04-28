using Microsoft.Extensions.DependencyInjection;
using OpenAI.ChatGpt.AspNetCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatGpt.TelegramBotExample;

public class TelegramChatBot
{
    private readonly ServiceProvider _serviceProvider;

    public TelegramChatBot(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if(update.Message is null) return;

        Message message = update.Message;

        // Only process text messages
        if (message.Text is not { } messageText)
            return;

        long chatId = message.Chat.Id;

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
        var typingTask = botClient.SendChatActionAsync(chatId, ChatAction.Typing, cancellationToken: cancellationToken);

        var chatGptFactory = _serviceProvider.GetRequiredService<ChatGPTFactory>();
        var chatGpt = await chatGptFactory.Create(message.Chat.Id.ToString(), cancellationToken: cancellationToken);
        var chat = await chatGpt.ContinueOrStartNewTopic(cancellationToken);
        var response = await chat.GetNextMessageResponse(messageText, cancellationToken: cancellationToken);
        await typingTask;
    
        _ = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: response,
            cancellationToken: cancellationToken);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        if(exception is ApiRequestException apiRequestException)
        {
            Console.WriteLine($"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}");
        } else
        {
            Console.WriteLine(exception);
        }

        return Task.CompletedTask;
    }
}