using ICMarkets.Infrastructure.Data;
using ICMarkets.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace ICMarkets.IntegrationTests.Infrastructure;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    // shared root ensures all scopes see the same in-memory data
    private static readonly InMemoryDatabaseRoot DbRoot = new();

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

            // Manually register options to bypass TryAdd in AddDbContext
            services.AddSingleton<DbContextOptions<AppDbContext>>(_ =>
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase("IntTestDb", DbRoot)
                    .Options);

            // Remove the background collector
            var collector = services.SingleOrDefault(
                d => d.ImplementationType == typeof(BlockchainDataCollector));
            if (collector != null)
                services.Remove(collector);

            // Replace health checks (remove NpgSql health check)
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
