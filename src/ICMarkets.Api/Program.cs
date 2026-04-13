using System.IO.Compression;
using ICMarkets.Api.Middleware;
using ICMarkets.Api.Serialization;
using ICMarkets.Application;
using ICMarkets.Infrastructure;
using ICMarkets.Infrastructure.Data;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

// Application & Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Output caching for read-heavy endpoints
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(b => b.NoCache());
    options.AddPolicy("BlockchainData", b =>
        b.Expire(TimeSpan.FromSeconds(30))
         .Tag("blockchain"));
});

// Response compression — Brotli first, GZip fallback
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o =>
    o.Level = CompressionLevel.Fastest);

// Controllers + source-generated JSON serializer (zero-reflection)
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, ApiJsonContext.Default);
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ICMarkets API", Version = "v1" });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

// Auto-migrate on startup (skip for InMemory in tests)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();
}

app.UseResponseCompression();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ICMarkets API v1"));
}

app.UseCors("Default");
app.UseOutputCache();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
