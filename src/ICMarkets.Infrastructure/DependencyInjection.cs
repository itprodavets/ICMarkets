using ICMarkets.Application.Interfaces;
using ICMarkets.Domain.Interfaces;
using ICMarkets.Infrastructure.Data;
using ICMarkets.Infrastructure.Data.Repositories;
using ICMarkets.Infrastructure.ExternalApis;
using ICMarkets.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace ICMarkets.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.EnableRetryOnFailure(3)));

        services.AddScoped<IBlockchainDataRepository, BlockchainDataRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpClient<IBlockCypherClient, BlockCypherClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.blockcypher.com/v1/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

        services.AddHostedService<BlockchainDataCollector>();

        return services;
    }
}
