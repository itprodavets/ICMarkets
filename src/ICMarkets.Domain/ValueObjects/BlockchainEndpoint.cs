using System.Collections.Frozen;

namespace ICMarkets.Domain.ValueObjects;

public sealed record BlockchainEndpoint(string Network, string Chain)
{
    public static readonly FrozenSet<BlockchainEndpoint> Supported =
        ((BlockchainEndpoint[])
        [
            new("eth", "main"),
            new("dash", "main"),
            new("btc", "main"),
            new("btc", "test3"),
            new("ltc", "main")
        ]).ToFrozenSet();

    public string ToApiPath() => $"{Network}/{Chain}";

    public static bool IsValid(string network, string chain) =>
        Supported.Contains(new BlockchainEndpoint(network, chain));

    public override string ToString() => $"{Network}/{Chain}";
}
