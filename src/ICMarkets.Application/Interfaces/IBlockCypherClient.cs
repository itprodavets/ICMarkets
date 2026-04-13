using ICMarkets.Domain.Entities;
using ICMarkets.Domain.ValueObjects;

namespace ICMarkets.Application.Interfaces;

public interface IBlockCypherClient
{
    Task<BlockchainData> FetchAsync(BlockchainEndpoint endpoint, CancellationToken ct = default);
}
