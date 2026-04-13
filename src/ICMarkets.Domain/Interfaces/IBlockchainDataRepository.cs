using ICMarkets.Domain.Entities;

namespace ICMarkets.Domain.Interfaces;

public interface IBlockchainDataRepository
{
    Task<BlockchainData?> GetLatestAsync(string network, string chain, CancellationToken ct = default);

    Task<IReadOnlyList<BlockchainData>> GetLatestForAllAsync(CancellationToken ct = default);

    Task<(IReadOnlyList<BlockchainData> Items, int TotalCount)> GetHistoryAsync(
        string network, string chain, int page, int pageSize, CancellationToken ct = default);

    Task AddAsync(BlockchainData data, CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<BlockchainData> data, CancellationToken ct = default);
}
