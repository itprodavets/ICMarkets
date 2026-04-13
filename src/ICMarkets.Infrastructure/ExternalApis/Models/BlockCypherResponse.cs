using System.Text.Json.Serialization;

namespace ICMarkets.Infrastructure.ExternalApis.Models;

public class BlockCypherResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("height")]
    public long Height { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    [JsonPropertyName("latest_url")]
    public string LatestUrl { get; set; } = string.Empty;

    [JsonPropertyName("previous_hash")]
    public string PreviousHash { get; set; } = string.Empty;

    [JsonPropertyName("previous_url")]
    public string PreviousUrl { get; set; } = string.Empty;

    [JsonPropertyName("peer_count")]
    public int PeerCount { get; set; }

    [JsonPropertyName("unconfirmed_count")]
    public int UnconfirmedCount { get; set; }

    [JsonPropertyName("high_fee_per_kb")]
    public long? HighFeePerKb { get; set; }

    [JsonPropertyName("medium_fee_per_kb")]
    public long? MediumFeePerKb { get; set; }

    [JsonPropertyName("low_fee_per_kb")]
    public long? LowFeePerKb { get; set; }

    [JsonPropertyName("high_gas_price")]
    public long? HighGasPrice { get; set; }

    [JsonPropertyName("medium_gas_price")]
    public long? MediumGasPrice { get; set; }

    [JsonPropertyName("low_gas_price")]
    public long? LowGasPrice { get; set; }

    [JsonPropertyName("high_priority_fee")]
    public long? HighPriorityFee { get; set; }

    [JsonPropertyName("medium_priority_fee")]
    public long? MediumPriorityFee { get; set; }

    [JsonPropertyName("low_priority_fee")]
    public long? LowPriorityFee { get; set; }

    [JsonPropertyName("base_fee")]
    public long? BaseFee { get; set; }

    [JsonPropertyName("last_fork_height")]
    public long LastForkHeight { get; set; }

    [JsonPropertyName("last_fork_hash")]
    public string LastForkHash { get; set; } = string.Empty;
}
