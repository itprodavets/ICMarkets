using ICMarkets.Application.Interfaces;
using ICMarkets.Domain.Interfaces;
using ICMarkets.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ICMarkets.Application.Blockchains.Commands;

public record CollectBlockchainDataCommand : IRequest<int>;

public sealed class CollectBlockchainDataHandler
    : IRequestHandler<CollectBlockchainDataCommand, int>
{
    private readonly IBlockCypherClient _client;
    private readonly IBlockchainDataRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CollectBlockchainDataHandler> _logger;

    public CollectBlockchainDataHandler(
        IBlockCypherClient client,
        IBlockchainDataRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CollectBlockchainDataHandler> logger)
    {
        _client = client;
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> Handle(CollectBlockchainDataCommand request, CancellationToken ct)
    {
        var fetchTasks = BlockchainEndpoint.Supported
            .Select(endpoint => FetchSafeAsync(endpoint, ct))
            .ToList();

        // Process results as they arrive — fast endpoints don't wait for slow ones
        var count = 0;
        await foreach (var completedTask in Task.WhenEach(fetchTasks))
        {
            var result = await completedTask;
            if (result is not null)
            {
                await _repository.AddAsync(result, ct);
                count++;
            }
        }

        if (count > 0)
            await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Collected {Count}/{Total} blockchain snapshots",
            count, BlockchainEndpoint.Supported.Count);

        return count;
    }

    private async Task<Domain.Entities.BlockchainData?> FetchSafeAsync(
        BlockchainEndpoint endpoint, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        try
        {
            return await _client.FetchAsync(endpoint, cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Request timed out for {Endpoint}", endpoint);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch data for {Endpoint}", endpoint);
            return null;
        }
    }
}
