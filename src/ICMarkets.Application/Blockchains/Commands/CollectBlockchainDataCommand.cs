using ICMarkets.Application.Interfaces;
using ICMarkets.Domain.Interfaces;
using ICMarkets.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ICMarkets.Application.Blockchains.Commands;

public record CollectBlockchainDataCommand : IRequest<int>;

public class CollectBlockchainDataHandler
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
            .Select(async endpoint =>
            {
                try
                {
                    return await _client.FetchAsync(endpoint, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch data for {Endpoint}", endpoint);
                    return null;
                }
            });

        var results = await Task.WhenAll(fetchTasks);
        var succeeded = results.Where(r => r is not null).ToList();

        if (succeeded.Count > 0)
        {
            await _repository.AddRangeAsync(succeeded!, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        _logger.LogInformation("Collected {Count}/{Total} blockchain snapshots",
            succeeded.Count, BlockchainEndpoint.Supported.Count);

        return succeeded.Count;
    }
}
