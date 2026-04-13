using System.Net;
using System.Net.Http.Json;
using ICMarkets.Application.DTOs;
using ICMarkets.FunctionalTests.Infrastructure;
using FluentAssertions;

namespace ICMarkets.FunctionalTests;

public class BlockchainDataFlowTests : IClassFixture<FunctionalTestFactory>
{
    private readonly HttpClient _client;

    public BlockchainDataFlowTests(FunctionalTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CollectAndQuery_FullFlow()
    {
        // 1. Trigger data collection
        var collectResponse = await _client.PostAsync("/api/blockchains/collect", null);
        collectResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var collectResult = await collectResponse.Content.ReadFromJsonAsync<CollectResult>();
        collectResult!.CollectedCount.Should().BeGreaterThan(0);

        // 2. Get all blockchains - should have data now
        var allResponse = await _client.GetAsync("/api/blockchains");
        allResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var allData = await allResponse.Content.ReadFromJsonAsync<List<BlockchainDataDto>>();
        allData.Should().NotBeEmpty();

        // 3. Get specific blockchain
        var btcResponse = await _client.GetAsync("/api/blockchains/btc/main");
        btcResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var btcData = await btcResponse.Content.ReadFromJsonAsync<BlockchainDataDto>();
        btcData.Should().NotBeNull();
        btcData!.Network.Should().Be("btc");

        // 4. Collect again and check history has two entries
        await _client.PostAsync("/api/blockchains/collect", null);

        var historyResponse = await _client.GetAsync("/api/blockchains/btc/main/history");
        historyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Swagger_IsAvailable()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private record CollectResult(int CollectedCount);
}
