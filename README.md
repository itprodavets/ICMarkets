# ICMarkets Blockchain Data API

Web API application that collects and stores blockchain data from [BlockCypher](https://www.blockcypher.com/) for ETH, BTC, DASH and LTC networks.

## Tech Stack

- **.NET 10** / ASP.NET Core
- **PostgreSQL** (via EF Core + Npgsql)
- **MediatR** (CQRS pattern)
- **FluentValidation** (request validation pipeline)
- **Mapster** (object mapping — faster than AutoMapper, less allocations)
- **Serilog** (structured logging)
- **Polly** (retry + circuit breaker)
- **Swagger / OpenAPI**
- **Docker**

## Architecture

Clean Architecture with CQRS:

```
src/
  ICMarkets.Domain           Entities, interfaces, value objects
  ICMarkets.Application      CQRS handlers, DTOs, validators, behaviors
  ICMarkets.Infrastructure   EF Core, repositories, BlockCypher client, background service
  ICMarkets.Api              Controllers, middleware, configuration

tests/
  ICMarkets.UnitTests            Handler and validator tests (Moq, FluentAssertions)
  ICMarkets.IntegrationTests     API endpoint tests (WebApplicationFactory)
  ICMarkets.FunctionalTests      End-to-end data flow tests
```

### Design Patterns

- **CQRS** — separate query/command handlers via MediatR with validation and logging pipelines
- **Repository + Unit of Work** — `IBlockchainDataRepository` + `IUnitOfWork` abstracts data access
- **Value Object** — `BlockchainEndpoint` as a sealed record with `FrozenSet` for O(1) lookups

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/) (for PostgreSQL)

### Run with Docker Compose (recommended)

```bash
docker compose up --build
```

API: `http://localhost:8080`  
Swagger UI: `http://localhost:8080/swagger`  
Health check: `http://localhost:8080/health`

### Run locally

```bash
# Start PostgreSQL
docker compose up postgres -d

# Run the API (migrations apply automatically)
dotnet run --project src/ICMarkets.Api
```

### Run Tests

```bash
dotnet test
```

Tests use EF Core InMemory provider — no database required.

## API Endpoints

| Method | Route | Description | Rate Limit |
|--------|-------|-------------|------------|
| GET | `/api/blockchains` | Latest snapshot for each blockchain | 100/min |
| GET | `/api/blockchains/{network}/{chain}` | Latest data for specific network | 100/min |
| GET | `/api/blockchains/{network}/{chain}/history?page=1&pageSize=20` | Paginated history | 100/min |
| POST | `/api/blockchains/collect` | Manually trigger data collection | 5/min |
| GET | `/health` | Health check (PostgreSQL connectivity) | — |

### Supported Networks

| Network | Chain | BlockCypher Endpoint |
|---------|-------|----------------------|
| eth | main | `/v1/eth/main` |
| btc | main | `/v1/btc/main` |
| btc | test3 | `/v1/btc/test3` |
| dash | main | `/v1/dash/main` |
| ltc | main | `/v1/ltc/main` |

## Performance & Reliability

This API is designed for high-throughput, low-latency operation typical of crypto exchange infrastructure.

### Zero-Reflection Serialization

System.Text.Json source generators eliminate all reflection-based serialization at runtime. Two source-generated contexts:

- `BlockCypherJsonContext` — upstream API deserialization (Infrastructure layer)
- `ApiJsonContext` — response serialization (API layer)

This removes a major source of Gen0 GC allocations in the hot path.

### Streaming HTTP

`BlockCypherClient` uses `HttpCompletionOption.ResponseHeadersRead` to stream JSON directly from the socket to the parser. No intermediate `byte[]` buffer is allocated for the response body.

### Response Pipeline

```
Request → Rate Limiter → Controller → MediatR Pipeline → EF Core (AsNoTracking)
                                          ↓
                                   Output Cache (30s TTL)
                                          ↓
Response ← Brotli/GZip Compression ← Source-Gen JSON Serializer
```

### Memory & GC Optimizations

| Technique | Impact |
|-----------|--------|
| JSON source generators | Eliminates reflection; no `System.Type` lookups at runtime |
| Streaming deserialization | Zero-copy from socket → parser; no response body buffering |
| `sealed` classes everywhere | JIT devirtualization, method inlining |
| `FrozenSet<BlockchainEndpoint>` | Pre-computed hash table; O(1) Contains |
| `AsNoTracking()` on all reads | No change tracker overhead |
| Brotli compression (Fastest) | ~70% response size reduction |
| Output cache with tag invalidation | Eliminates DB hits for repeated reads |
| Concrete response types | No anonymous type reflection in middleware |

### Resilience

| Layer | Mechanism |
|-------|-----------|
| HTTP client → BlockCypher | Polly retry (3× exponential backoff) + circuit breaker (5 failures → 30s break) |
| Background collector | `SemaphoreSlim(3)` to respect free-tier rate limit (~3 req/s) |
| API endpoints | Fixed-window rate limiter (100 req/min reads, 5 req/min writes) |
| Database | `EnableRetryOnFailure(3)`, 15s command timeout |
| Connection pool | Npgsql multiplexing, 5–100 pool size |

### What Happens Under Load

1. **Read requests** hit the output cache (30s TTL). Cache miss triggers an `AsNoTracking` query that hits the composite index `(Network, Chain, CreatedAt DESC)`.
2. **GET /api/blockchains** runs 5 parallel queries (one per endpoint), each using the composite index.
3. **POST /api/blockchains/collect** is rate-limited to 5/min. Fetches all 5 endpoints in parallel with `SemaphoreSlim(3)` throttling. If BlockCypher is degraded, the circuit breaker opens after 5 failures and short-circuits for 30s.
4. **Background collector** runs every 5 minutes. Same throttling + circuit breaker as manual collect.

## Data Collection

A background service (`BlockchainDataCollector`) polls all BlockCypher endpoints every 5 minutes (configurable via `BlockchainCollector:IntervalMinutes`). Each snapshot is stored with a UTC timestamp.

Data can also be collected on demand via `POST /api/blockchains/collect`, which evicts the output cache after saving.

## Configuration

| Setting | Default | Description |
|---------|---------|-------------|
| `ConnectionStrings:DefaultConnection` | see appsettings | PostgreSQL (credentials via env vars in production) |
| `BlockchainCollector:IntervalMinutes` | `5` | Background polling interval |

**Security note**: Database credentials are not stored in `appsettings.json`. For local development, they come from `appsettings.Development.json`. In Docker/production, use the `ConnectionStrings__DefaultConnection` environment variable.

Serilog logs to console and rolling files (`logs/` directory, 14-day retention).

## Project Structure

```
ICMarkets/
  src/
    ICMarkets.Domain/
      Entities/BlockchainData.cs
      Interfaces/IBlockchainDataRepository.cs
      ValueObjects/BlockchainEndpoint.cs
    ICMarkets.Application/
      Behaviors/ValidationBehavior.cs, LoggingBehavior.cs
      Blockchains/Commands/, Queries/
      Common/PagedResult.cs
      DTOs/BlockchainDataDto.cs
      Mappings/BlockchainMappingConfig.cs
    ICMarkets.Infrastructure/
      Data/AppDbContext.cs, Configurations/, Repositories/
      ExternalApis/BlockCypherClient.cs, BlockCypherJsonContext.cs
      Services/BlockchainDataCollector.cs
    ICMarkets.Api/
      Controllers/BlockchainsController.cs
      Middleware/ExceptionHandlingMiddleware.cs
      Serialization/ApiJsonContext.cs
      Program.cs
  tests/
    ICMarkets.UnitTests/        (16 tests)
    ICMarkets.IntegrationTests/ (6 tests)
    ICMarkets.FunctionalTests/  (2 tests)
  Dockerfile
  docker-compose.yml
```
