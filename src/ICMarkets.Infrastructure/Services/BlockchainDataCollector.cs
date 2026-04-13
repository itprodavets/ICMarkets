using ICMarkets.Application.Interfaces;
using ICMarkets.Domain.Interfaces;
using ICMarkets.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ICMarkets.Infrastructure.Services;

public class BlockchainDataCollector : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BlockchainDataCollector> _logger;
    private readonly TimeSpan _interval;

    public BlockchainDataCollector(
        IServiceScopeFactory scopeFactory,
        ILogger<BlockchainDataCollector> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var minutes = configuration.GetValue("BlockchainCollector:IntervalMinutes", 5);
        _interval = TimeSpan.FromMinutes(minutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Blockchain data collector started, interval: {Interval}", _interval);

        await CollectAllAsync(stoppingToken);

        using var timer = new PeriodicTimer(_interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CollectAllAsync(stoppingToken);
        }
    }

    private async Task CollectAllAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<IBlockCypherClient>();
            var repository = scope.ServiceProvider.GetRequiredService<IBlockchainDataRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var fetchTasks = BlockchainEndpoint.Supported
                .Select(ep => FetchSafeAsync(client, ep, ct));

            var results = await Task.WhenAll(fetchTasks);
            var succeeded = results.Where(r => r is not null).ToList();

            if (succeeded.Count > 0)
            {
                await repository.AddRangeAsync(succeeded!, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }

            _logger.LogInformation("Collected {Success}/{Total} blockchain snapshots",
                succeeded.Count, BlockchainEndpoint.Supported.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error in blockchain data collector");
        }
    }

    private async Task<Domain.Entities.BlockchainData?> FetchSafeAsync(
        IBlockCypherClient client,
        BlockchainEndpoint endpoint,
        CancellationToken ct)
    {
        try
        {
            return await client.FetchAsync(endpoint, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to fetch {Endpoint}", endpoint);
            return null;
        }
    }
}
