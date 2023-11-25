using Microsoft.Extensions.Hosting;

namespace OpenAI.ChatGpt.AspNetCore;

internal class AiClientStartupValidationBackgroundService : BackgroundService
{
    private readonly AiClientFromConfiguration _aiClient;

    public AiClientStartupValidationBackgroundService(AiClientFromConfiguration aiClient)
    {
        _aiClient = aiClient ?? throw new ArgumentNullException(nameof(aiClient));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}