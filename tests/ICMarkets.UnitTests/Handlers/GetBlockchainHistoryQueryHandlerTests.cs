using ICMarkets.Application.Blockchains.Queries;
using ICMarkets.Domain.Entities;
using ICMarkets.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ICMarkets.UnitTests.Handlers;

public class GetBlockchainHistoryQueryHandlerTests
{
    private readonly Mock<IBlockchainDataRepository> _repoMock = new();
    private readonly GetBlockchainHistoryQueryHandler _handler;

    public GetBlockchainHistoryQueryHandlerTests()
    {
        _handler = new GetBlockchainHistoryQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsPagedHistory()
    {
        var items = Enumerable.Range(1, 5)
            .Select(i => new BlockchainData
            {
                Network = "btc",
                Chain = "main",
                Name = "BTC.main",
                Height = 800_000 + i,
                Hash = $"hash_{i}",
                PreviousHash = "prev",
                LastForkHash = "fork",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            })
            .ToList();

        _repoMock.Setup(r => r.GetHistoryAsync("btc", "main", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items.AsReadOnly(), 25));

        var query = new GetBlockchainHistoryQuery("btc", "main", 1, 20);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_LastPage_HasNoNextPage()
    {
        _repoMock.Setup(r => r.GetHistoryAsync("btc", "main", 2, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<BlockchainData>().AsReadOnly(), 25));

        var query = new GetBlockchainHistoryQuery("btc", "main", 2, 20);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeTrue();
    }
}
