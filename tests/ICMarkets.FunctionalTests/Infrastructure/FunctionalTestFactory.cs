using ICMarkets.Application.Interfaces;
using ICMarkets.Domain.Entities;
using ICMarkets.Domain.ValueObjects;
using ICMarkets.Infrastructure.Data;
using ICMarkets.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ICMarkets.FunctionalTests.Infrastructure;

public class FunctionalTestFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DbRoot = new();

    public Mock<IBlockCypherClient> BlockCypherClientMock { get; } = new();

    public FunctionalTestFactory()
    {
        BlockCypherClientMock
            .Setup(c => c.FetchAsync(It.IsAny<BlockchainEndpoint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BlockchainEndpoint ep, CancellationToken _) => new BlockchainData
            {
                Network = ep.Network,
                Chain = ep.Chain,
                Name = $"{ep.Network.ToUpper()}.{ep.Chain}",
                Height = 100_000,
                Hash = "testhash",
                PreviousHash = "prevhash",
                LastForkHash = "forkhash",
                BlockTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all EF/Npgsql registrations to avoid dual-provider conflict
            var toRemove = services.Where(d =>
            {
                var sn = d.ServiceType.FullName ?? "";
                var imp = d.ImplementationType?.FullName ?? "";
                return sn.Contains("DbContextOptions") || sn.Contains("Npgsql") ||
                       imp.Contains("Npgsql");
            }).ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            services.AddSingleton<DbContextOptions<AppDbContext>>(_ =>
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase("FuncTestDb", DbRoot)
                    .Options);

            // Remove background collector
            var collector = services.SingleOrDefault(
                d => d.ImplementationType == typeof(BlockchainDataCollector));
            if (collector != null)
                services.Remove(collector);

            // Replace BlockCypher client with mock
            var clientDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IBlockCypherClient));
            if (clientDescriptor != null)
                services.Remove(clientDescriptor);
            services.AddSingleton(BlockCypherClientMock.Object);

            // Replace health checks
            var hcDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("HealthCheck") == true)
                .ToList();
            foreach (var hc in hcDescriptors)
                services.Remove(hc);
            services.AddHealthChecks();
        });

        builder.UseEnvironment("Development");
    }
}
