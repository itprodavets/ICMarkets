using System.Text.Json;
using ICMarkets.Application.Interfaces;
using ICMarkets.Domain.Entities;
using ICMarkets.Domain.ValueObjects;
using ICMarkets.Infrastructure.ExternalApis.Models;
using Microsoft.Extensions.Logging;

namespace ICMarkets.Infrastructure.ExternalApis;

public sealed class BlockCypherClient : IBlockCypherClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BlockCypherClient> _logger;

    public BlockCypherClient(HttpClient httpClient, ILogger<BlockCypherClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<BlockchainData> FetchAsync(BlockchainEndpoint endpoint, CancellationToken ct)
    {
        _logger.LogDebug("Requesting {Endpoint} from BlockCypher", endpoint);

        // Stream directly from socket → JSON parser, no intermediate byte[] buffer
        using var httpResponse = await _httpClient.GetAsync(
            endpoint.ToApiPath(), HttpCompletionOption.ResponseHeadersRead, ct);
        httpResponse.EnsureSuccessStatusCode();

        await using var stream = await httpResponse.Content.ReadAsStreamAsync(ct);
        var response = await JsonSerializer.DeserializeAsync(
            stream, BlockCypherJsonContext.Default.BlockCypherResponse, ct);

        if (response is null)
            throw new InvalidOperationException($"Empty response from BlockCypher for {endpoint}");

        return MapToEntity(response, endpoint);
    }

    private static BlockchainData MapToEntity(BlockCypherResponse src, BlockchainEndpoint endpoint)
    {
        return new BlockchainData
        {
            Name = src.Name,
            Network = endpoint.Network,
            Chain = endpoint.Chain,
            Height = src.Height,
            Hash = src.Hash,
            BlockTime = src.Time,
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
            CreatedAt = DateTime.UtcNow
        };
    }
}
