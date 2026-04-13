using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using BenchmarkDotNet.Attributes;
using ICMarkets.Application.DTOs;
using ICMarkets.Infrastructure.ExternalApis.Models;

namespace ICMarkets.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class JsonSerializationBenchmarks
{
    private BlockchainDataDto _dto = null!;
    private byte[] _dtoJsonBytes = null!;
    private byte[] _blockCypherJsonBytes = null!;

    private JsonSerializerOptions _reflectionOptions = null!;
    private JsonSerializerOptions _sourceGenOptions = null!;

    private static readonly string BlockCypherPayload = """
        {
            "name": "BTC.main",
            "height": 840000,
            "hash": "0000000000000000000320283a032748cef8227873ff4872689bf23f1cda83a5",
            "time": "2024-04-20T00:09:30.442688429Z",
            "latest_url": "https://api.blockcypher.com/v1/btc/main/blocks/0000000000000000000320283a032748cef8227873ff4872689bf23f1cda83a5",
            "previous_hash": "00000000000000000001dcce6ce7c8a45872cafd1fb04732b447a14a91832591",
            "previous_url": "https://api.blockcypher.com/v1/btc/main/blocks/00000000000000000001dcce6ce7c8a45872cafd1fb04732b447a14a91832591",
            "peer_count": 244,
            "unconfirmed_count": 12543,
            "high_fee_per_kb": 150000,
            "medium_fee_per_kb": 75000,
            "low_fee_per_kb": 25000,
            "last_fork_height": 839990,
            "last_fork_hash": "00000000000000000002a7c4c1e48d76c5a37902165a270156b7a8d72f9a4670"
        }
        """;

    [GlobalSetup]
    public void Setup()
    {
        _dto = new BlockchainDataDto
        {
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

        _reflectionOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _sourceGenOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = BenchmarkJsonContext.Default
        };

        _dtoJsonBytes = JsonSerializer.SerializeToUtf8Bytes(_dto, _sourceGenOptions);
        _blockCypherJsonBytes = System.Text.Encoding.UTF8.GetBytes(BlockCypherPayload);
    }

    // --- Serialization: DTO → JSON bytes ---

    [Benchmark(Description = "Serialize DTO (reflection)")]
    public byte[] Serialize_Dto_Reflection()
    {
        return JsonSerializer.SerializeToUtf8Bytes(_dto, _reflectionOptions);
    }

    [Benchmark(Description = "Serialize DTO (source-gen)")]
    public byte[] Serialize_Dto_SourceGen()
    {
        return JsonSerializer.SerializeToUtf8Bytes(_dto, BenchmarkJsonContext.Default.BlockchainDataDto);
    }

    // --- Deserialization: JSON bytes → DTO ---

    [Benchmark(Description = "Deserialize DTO (reflection)")]
    public BlockchainDataDto? Deserialize_Dto_Reflection()
    {
        return JsonSerializer.Deserialize<BlockchainDataDto>(_dtoJsonBytes, _reflectionOptions);
    }

    [Benchmark(Description = "Deserialize DTO (source-gen)")]
    public BlockchainDataDto? Deserialize_Dto_SourceGen()
    {
        return JsonSerializer.Deserialize(_dtoJsonBytes, BenchmarkJsonContext.Default.BlockchainDataDto);
    }

    // --- Deserialization: BlockCypher API response ---

    [Benchmark(Description = "Deserialize BlockCypher (reflection)")]
    public BlockCypherResponse? Deserialize_BlockCypher_Reflection()
    {
        return JsonSerializer.Deserialize<BlockCypherResponse>(_blockCypherJsonBytes);
    }

    [Benchmark(Description = "Deserialize BlockCypher (source-gen)")]
    public BlockCypherResponse? Deserialize_BlockCypher_SourceGen()
    {
        return JsonSerializer.Deserialize(_blockCypherJsonBytes,
            BenchmarkJsonContext.Default.BlockCypherResponse);
    }
}

[System.Text.Json.Serialization.JsonSourceGenerationOptions(
    PropertyNamingPolicy = System.Text.Json.Serialization.JsonKnownNamingPolicy.CamelCase)]
[System.Text.Json.Serialization.JsonSerializable(typeof(BlockchainDataDto))]
[System.Text.Json.Serialization.JsonSerializable(typeof(BlockCypherResponse))]
internal partial class BenchmarkJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
