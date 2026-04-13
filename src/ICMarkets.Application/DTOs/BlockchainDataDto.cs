namespace ICMarkets.Application.DTOs;

public class BlockchainDataDto
{
    public string Name { get; init; } = string.Empty;
    public string Network { get; init; } = string.Empty;
    public string Chain { get; init; } = string.Empty;
    public long Height { get; init; }
    public string Hash { get; init; } = string.Empty;
    public DateTime BlockTime { get; init; }
    public string LatestUrl { get; init; } = string.Empty;
    public string PreviousHash { get; init; } = string.Empty;
    public string PreviousUrl { get; init; } = string.Empty;
    public int PeerCount { get; init; }
    public int UnconfirmedCount { get; init; }

    // BTC, DASH, LTC
    public long? HighFeePerKb { get; init; }
    public long? MediumFeePerKb { get; init; }
    public long? LowFeePerKb { get; init; }

    // ETH
    public long? HighGasPrice { get; init; }
    public long? MediumGasPrice { get; init; }
    public long? LowGasPrice { get; init; }
    public long? HighPriorityFee { get; init; }
    public long? MediumPriorityFee { get; init; }
    public long? LowPriorityFee { get; init; }
    public long? BaseFee { get; init; }

    public long LastForkHeight { get; init; }
    public string LastForkHash { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
