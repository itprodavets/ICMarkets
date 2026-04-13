using ICMarkets.Domain.Entities;
using ICMarkets.Domain.Interfaces;
using ICMarkets.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ICMarkets.Infrastructure.Data.Repositories;

public sealed class BlockchainDataRepository : IBlockchainDataRepository
{
    private readonly AppDbContext _context;

    public BlockchainDataRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BlockchainData?> GetLatestAsync(
        string network, string chain, CancellationToken ct)
    {
        return await _context.BlockchainData
            .AsNoTracking()
            .Where(x => x.Network == network && x.Chain == chain)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<BlockchainData>> GetLatestForAllAsync(CancellationToken ct)
    {
        // 5 endpoints, each query hits the composite index - fine for this scale
        var tasks = BlockchainEndpoint.Supported.Select(ep =>
            _context.BlockchainData
                .AsNoTracking()
                .Where(x => x.Network == ep.Network && x.Chain == ep.Chain)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct));

        var results = await Task.WhenAll(tasks);
        return results.Where(x => x is not null).ToList()!;
    }

    public async Task<(IReadOnlyList<BlockchainData> Items, int TotalCount)> GetHistoryAsync(
        string network, string chain, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.BlockchainData
            .AsNoTracking()
            .Where(x => x.Network == network && x.Chain == chain);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(BlockchainData data, CancellationToken ct)
    {
        await _context.BlockchainData.AddAsync(data, ct);
    }

    public async Task AddRangeAsync(IEnumerable<BlockchainData> data, CancellationToken ct)
    {
        await _context.BlockchainData.AddRangeAsync(data, ct);
    }
}
