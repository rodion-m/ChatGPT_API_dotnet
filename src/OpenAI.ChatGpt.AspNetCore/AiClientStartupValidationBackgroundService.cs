using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenAI.ChatGpt.AspNetCore;

internal class AiClientStartupValidationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AiClientStartupValidationBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        _ = scope.ServiceProvider.GetRequiredService<IAiClient>();
    }
}