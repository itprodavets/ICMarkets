namespace ICMarkets.Domain.Entities;

public class BlockchainData
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
    public string Chain { get; set; } = string.Empty;

    public long Height { get; set; }
    public string Hash { get; set; } = string.Empty;
    public DateTime BlockTime { get; set; }

    public string LatestUrl { get; set; } = string.Empty;
    public string PreviousHash { get; set; } = string.Empty;
    public string PreviousUrl { get; set; } = string.Empty;

    public int PeerCount { get; set; }
    public int UnconfirmedCount { get; set; }

    // BTC, DASH, LTC fee fields
    public long? HighFeePerKb { get; set; }
    public long? MediumFeePerKb { get; set; }
    public long? LowFeePerKb { get; set; }

    // ETH gas fields
    public long? HighGasPrice { get; set; }
    public long? MediumGasPrice { get; set; }
    public long? LowGasPrice { get; set; }
    public long? HighPriorityFee { get; set; }
    public long? MediumPriorityFee { get; set; }
    public long? LowPriorityFee { get; set; }
    public long? BaseFee { get; set; }

    public long LastForkHeight { get; set; }
    public string LastForkHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
