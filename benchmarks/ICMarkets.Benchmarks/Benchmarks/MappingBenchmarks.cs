using BenchmarkDotNet.Attributes;
using ICMarkets.Application.DTOs;
using ICMarkets.Domain.Entities;
using Mapster;

namespace ICMarkets.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class MappingBenchmarks
{
    private BlockchainData _entity = null!;
    private List<BlockchainData> _entities = null!;

    [GlobalSetup]
    public void Setup()
    {
        _entity = new BlockchainData
        {
            Id = 1,
            Name = "BTC.main",
            Network = "btc",
            Chain = "main",
            Height = 840000,
            Hash = "0000000000000000000320283a032748cef8227873ff4872689bf23f1cda83a5",
            BlockTime = DateTime.UtcNow,
            LatestUrl = "https://api.blockcypher.com/v1/btc/main/blocks/00000000",
            PreviousHash = "00000000000000000001dcce6ce7c8a45872cafd1fb04732b447a14a91832591",
            PreviousUrl = "https://api.blockcypher.com/v1/btc/main/blocks/00000001",
            PeerCount = 244,
            UnconfirmedCount = 12543,
            HighFeePerKb = 150000,
            MediumFeePerKb = 75000,
            LowFeePerKb = 25000,
            LastForkHeight = 839990,
            LastForkHash = "00000000000000000002a7c4c1e48d76c5a37902165a270156b7a8d72f9a4670",
            CreatedAt = DateTime.UtcNow
        };

        _entities = Enumerable.Range(0, 100).Select(i =>
        {
            var e = new BlockchainData
            {
                Id = i,
                Name = $"BTC.main",
                Network = "btc",
                Chain = "main",
                Height = 840000 + i,
                Hash = $"hash_{i:D64}",
                BlockTime = DateTime.UtcNow.AddMinutes(-i * 5),
                LatestUrl = $"https://api.blockcypher.com/v1/btc/main/blocks/{i}",
                PreviousHash = $"prev_{i:D64}",
                PreviousUrl = $"https://api.blockcypher.com/v1/btc/main/blocks/{i - 1}",
                PeerCount = 244,
                UnconfirmedCount = 12543 - i,
                HighFeePerKb = 150000,
                MediumFeePerKb = 75000,
                LowFeePerKb = 25000,
                LastForkHeight = 839990,
                LastForkHash = $"fork_{i:D64}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i * 5)
            };
            return e;
        }).ToList();
    }

    [Benchmark(Description = "Mapster: single entity → DTO")]
    public BlockchainDataDto MapSingle()
    {
        return _entity.Adapt<BlockchainDataDto>();
    }

    [Benchmark(Description = "Mapster: 100 entities → DTOs")]
    public List<BlockchainDataDto> MapBatch()
    {
        return _entities.Adapt<List<BlockchainDataDto>>();
    }

    [Benchmark(Description = "Manual: single entity → DTO")]
    public BlockchainDataDto ManualMapSingle()
    {
        return ManualMap(_entity);
    }

    [Benchmark(Description = "Manual: 100 entities → DTOs")]
    public List<BlockchainDataDto> ManualMapBatch()
    {
        var result = new List<BlockchainDataDto>(_entities.Count);
        foreach (var e in _entities)
            result.Add(ManualMap(e));
        return result;
    }

    private static BlockchainDataDto ManualMap(BlockchainData src) => new()
    {
        Name = src.Name,
        Network = src.Network,
        Chain = src.Chain,
        Height = src.Height,
        Hash = src.Hash,
        BlockTime = src.BlockTime,
        LatestUrl = src.LatestUrl,
        PreviousHash = src.PreviousHash,
        PreviousUrl = src.PreviousUrl,
        PeerCount = src.PeerCount,
        UnconfirmedCount = src.UnconfirmedCount,
        HighFeePerKb = src.HighFeePerKb,
        MediumFeePerKb = src.MediumFeePerKb,
        LowFeePerKb = src.LowFeePerKb,
        HighGasPrice = src.HighGasPrice,
        MediumGasPrice = src.MediumGasPrice,
        LowGasPrice = src.LowGasPrice,
        HighPriorityFee = src.HighPriorityFee,
        MediumPriorityFee = src.MediumPriorityFee,
        LowPriorityFee = src.LowPriorityFee,
        BaseFee = src.BaseFee,
        LastForkHeight = src.LastForkHeight,
        LastForkHash = src.LastForkHash,
        CreatedAt = src.CreatedAt
    };
}
