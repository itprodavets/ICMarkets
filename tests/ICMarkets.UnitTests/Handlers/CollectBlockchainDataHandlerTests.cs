using ICMarkets.Application.Blockchains.Commands;
using ICMarkets.Application.Interfaces;
using ICMarkets.Domain.Entities;
using ICMarkets.Domain.Interfaces;
using ICMarkets.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ICMarkets.UnitTests.Handlers;

public class CollectBlockchainDataHandlerTests
{
    private readonly Mock<IBlockCypherClient> _clientMock = new();
    private readonly Mock<IBlockchainDataRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CollectBlockchainDataHandler _handler;

    public CollectBlockchainDataHandlerTests()
    {
        _handler = new CollectBlockchainDataHandler(
            _clientMock.Object,
            _repoMock.Object,
            _uowMock.Object,
            Mock.Of<ILogger<CollectBlockchainDataHandler>>());
    }

    [Fact]
    public async Task Handle_CollectsFromAllEndpoints()
    {
        _clientMock
            .Setup(c => c.FetchAsync(It.IsAny<BlockchainEndpoint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BlockchainEndpoint ep, CancellationToken _) => new BlockchainData
            {
                Network = ep.Network,
                Chain = ep.Chain,
                Name = $"{ep.Network.ToUpper()}.{ep.Chain}",
                Hash = "abc",
                PreviousHash = "prev",
                LastForkHash = "fork",
                CreatedAt = DateTime.UtcNow
            });

        var count = await _handler.Handle(new CollectBlockchainDataCommand(), CancellationToken.None);

        count.Should().Be(BlockchainEndpoint.Supported.Count);
        _repoMock.Verify(r => r.AddAsync(
            It.IsAny<BlockchainData>(),
            It.IsAny<CancellationToken>()), Times.Exactly(BlockchainEndpoint.Supported.Count));
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PartialFailure_SavesSuccessful()
    {
        var callCount = 0;
        _clientMock
            .Setup(c => c.FetchAsync(It.IsAny<BlockchainEndpoint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BlockchainEndpoint ep, CancellationToken _) =>
            {
                if (Interlocked.Increment(ref callCount) % 2 == 0)
                    throw new HttpRequestException("timeout");

                return new BlockchainData
                {
                    Network = ep.Network, Chain = ep.Chain,
                    Name = $"{ep.Network}.{ep.Chain}",
                    Hash = "x", PreviousHash = "y", LastForkHash = "z",
                    CreatedAt = DateTime.UtcNow
                };
            });

        var count = await _handler.Handle(new CollectBlockchainDataCommand(), CancellationToken.None);

        count.Should().BeGreaterThan(0);
        count.Should().BeLessThan(BlockchainEndpoint.Supported.Count);
    }
}
