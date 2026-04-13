using ICMarkets.Application.Blockchains.Queries;
using ICMarkets.Domain.Entities;
using ICMarkets.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ICMarkets.UnitTests.Handlers;

public class GetAllBlockchainsQueryHandlerTests
{
    private readonly Mock<IBlockchainDataRepository> _repoMock = new();
    private readonly GetAllBlockchainsQueryHandler _handler;

    public GetAllBlockchainsQueryHandlerTests()
    {
        _handler = new GetAllBlockchainsQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllLatestBlockchains()
    {
        var data = new List<BlockchainData>
        {
            CreateSample("btc", "main", 800_000),
            CreateSample("eth", "main", 19_000_000)
        };

        _repoMock.Setup(r => r.GetLatestForAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        var result = await _handler.Handle(new GetAllBlockchainsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Network.Should().Be("btc");
        result[1].Network.Should().Be("eth");
    }

    [Fact]
    public async Task Handle_WhenNoData_ReturnsEmptyList()
    {
        _repoMock.Setup(r => r.GetLatestForAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BlockchainData>());

        var result = await _handler.Handle(new GetAllBlockchainsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    private static BlockchainData CreateSample(string network, string chain, long height) => new()
    {
        Network = network,
        Chain = chain,
        Name = $"{network.ToUpper()}.{chain}",
        Height = height,
        Hash = "abc123",
        PreviousHash = "prev123",
        LastForkHash = "fork123",
        BlockTime = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow
    };
}
