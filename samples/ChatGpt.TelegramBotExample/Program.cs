using ChatGpt.TelegramBotExample;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenAI.ChatGpt.AspNetCore;
using OpenAI.ChatGpt.AspNetCore.Models;
using OpenAI.ChatGpt.EntityFrameworkCore.Extensions;
using OpenAI.ChatGpt.Models;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

var accessToken = Helpers.GetKeyFromEnvironment("ONLINE_ASSISTANT_TG_BOT_TOKEN");
var botClient = new TelegramBotClient(accessToken);

var openAiKey = Helpers.GetKeyFromEnvironment("OPENAI_API_KEY");
await using var serviceProvider = CreateServiceProvider(
    openAiKey, 
    initialMessage: "You are ChatGPT helpful assistant worked inside Telegram.", 
    maxTokens: 300,
    host: "https://api.pawan.krd/v1/" //delete this line if you use default openAI host
);

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

var telegramChatBot = new TelegramChatBot(serviceProvider);

botClient.StartReceiving(
    updateHandler: telegramChatBot.HandleUpdateAsync,
    pollingErrorHandler: telegramChatBot.HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();
Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

ServiceProvider CreateServiceProvider(
    string openaikey, string initialMessage, int maxTokens, string? host = null)
{
    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
    services.AddOptions<OpenAICredentials>()
        .Configure(cred =>
        {
            cred.ApiKey = openaikey;
            if(host is not null) cred.ApiHost = host;
        });
    services.AddOptions<ChatGPTConfig>()
        .Configure(config =>
        {
            config.InitialSystemMessage = initialMessage;
            config.MaxTokens = maxTokens;
        });
    services.AddChatGptEntityFrameworkIntegration(
        options => options.UseSqlite("Data Source=dialogs.db"));
    services.RemoveAll<ChatGPTFactory>();
    services.AddTransient<ChatGPTFactory>();
    return services.BuildServiceProvider();
}