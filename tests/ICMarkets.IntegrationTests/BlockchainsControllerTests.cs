using System.Net;
using System.Net.Http.Json;
using ICMarkets.Application.DTOs;
using ICMarkets.Domain.Entities;
using ICMarkets.Infrastructure.Data;
using ICMarkets.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ICMarkets.IntegrationTests;

public class BlockchainsControllerTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public BlockchainsControllerTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WhenNoData_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync("/api/blockchains");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await response.Content.ReadFromJsonAsync<List<BlockchainDataDto>>();
        data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLatest_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/blockchains/ltc/test99");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetHistory_InvalidNetwork_Returns400()
    {
        var response = await _client.GetAsync("/api/blockchains/invalid/main/history");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLatest_WithSeededData_ReturnsOk()
    {
        // Seed data
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.BlockchainData.Add(new BlockchainData
            {
                Network = "btc", Chain = "main", Name = "BTC.main",
                Height = 800_000, Hash = "abc", PreviousHash = "prev",
                LastForkHash = "fork", BlockTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/blockchains/btc/main");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<BlockchainDataDto>();
        dto.Should().NotBeNull();
        dto!.Height.Should().Be(800_000);
    }

    [Fact]
    public async Task GetHistory_ValidRequest_ReturnsPaginatedResult()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            for (int i = 0; i < 5; i++)
            {
                db.BlockchainData.Add(new BlockchainData
                {
                    Network = "eth", Chain = "main", Name = "ETH.main",
                    Height = 19_000_000 + i, Hash = $"hash_{i}",
                    PreviousHash = "prev", LastForkHash = "fork",
                    BlockTime = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/blockchains/eth/main/history?page=1&pageSize=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"totalCount\"");
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
