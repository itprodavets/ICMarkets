namespace ICMarkets.Domain.ValueObjects;

public sealed record BlockchainEndpoint(string Network, string Chain)
{
    public static readonly IReadOnlyList<BlockchainEndpoint> Supported =
    [
        new("eth", "main"),
        new("dash", "main"),
        new("btc", "main"),
        new("btc", "test3"),
        new("ltc", "main")
    ];

    public string ToApiPath() => $"{Network}/{Chain}";

    public static bool IsValid(string network, string chain) =>
        Supported.Any(e => e.Network == network && e.Chain == chain);

    public override string ToString() => $"{Network}/{Chain}";
}
