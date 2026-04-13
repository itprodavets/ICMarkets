using System.Collections.Frozen;
using BenchmarkDotNet.Attributes;
using ICMarkets.Domain.ValueObjects;

namespace ICMarkets.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class EndpointLookupBenchmarks
{
    private static readonly FrozenSet<BlockchainEndpoint> FrozenEndpoints =
        BlockchainEndpoint.Supported;

    private static readonly List<BlockchainEndpoint> ListEndpoints =
        BlockchainEndpoint.Supported.ToList();

    private BlockchainEndpoint _existing;
    private BlockchainEndpoint _missing;

    [GlobalSetup]
    public void Setup()
    {
        _existing = new BlockchainEndpoint("btc", "main");
        _missing = new BlockchainEndpoint("xrp", "main");
    }

    [Benchmark(Description = "FrozenSet.Contains (hit)")]
    public bool FrozenSet_Hit() => FrozenEndpoints.Contains(_existing);

    [Benchmark(Description = "FrozenSet.Contains (miss)")]
    public bool FrozenSet_Miss() => FrozenEndpoints.Contains(_missing);

    [Benchmark(Description = "List.Any (hit)")]
    public bool List_Hit() => ListEndpoints.Any(
        e => e.Network == _existing.Network && e.Chain == _existing.Chain);

    [Benchmark(Description = "List.Any (miss)")]
    public bool List_Miss() => ListEndpoints.Any(
        e => e.Network == _missing.Network && e.Chain == _missing.Chain);
}
