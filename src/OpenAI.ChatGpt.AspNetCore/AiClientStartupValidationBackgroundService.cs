using Microsoft.Extensions.Hosting;

namespace OpenAI.ChatGpt.AspNetCore;

internal class AiClientStartupValidationBackgroundService : BackgroundService
{

    public AiClientStartupValidationBackgroundService(AiClientFromConfiguration _)
    {
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}