# ICMarkets Blockchain Data API

Web API application that collects and stores blockchain data from [BlockCypher](https://www.blockcypher.com/) for ETH, BTC, DASH and LTC networks.

## Tech Stack

- **.NET 10** / ASP.NET Core
- **PostgreSQL** (via EF Core)
- **MediatR** (CQRS pattern)
- **FluentValidation** (request validation pipeline)
- **Mapster** (object mapping)
- **Serilog** (structured logging)
- **Polly** (HTTP retry policies)
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

- **CQRS** - separate query/command handlers via MediatR with validation and logging pipelines
- **Repository** - `IBlockchainDataRepository` abstracts data access
- **Unit of Work** - `IUnitOfWork` wraps EF Core transaction management

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/) (for PostgreSQL)

### Run with Docker Compose (recommended)

```bash
docker compose up --build
```

The API will be available at `http://localhost:8080`.  
Swagger UI: `http://localhost:8080/swagger`

### Run locally

1. Start PostgreSQL:

```bash
docker compose up postgres -d
```

2. Run the API:

```bash
dotnet run --project src/ICMarkets.Api
```

The application will apply database migrations automatically on startup.

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/blockchains` | Latest snapshot for each blockchain |
| GET | `/api/blockchains/{network}/{chain}` | Latest data for a specific network |
| GET | `/api/blockchains/{network}/{chain}/history?page=1&pageSize=20` | Paginated history (CreatedAt DESC) |
| POST | `/api/blockchains/collect` | Manually trigger data collection |
| GET | `/health` | Health check |

### Supported Networks

| Network | Chain | Source |
|---------|-------|--------|
| eth | main | `api.blockcypher.com/v1/eth/main` |
| btc | main | `api.blockcypher.com/v1/btc/main` |
| btc | test3 | `api.blockcypher.com/v1/btc/test3` |
| dash | main | `api.blockcypher.com/v1/dash/main` |
| ltc | main | `api.blockcypher.com/v1/ltc/main` |

## Data Collection

A background service (`BlockchainDataCollector`) polls all BlockCypher endpoints every 5 minutes (configurable via `BlockchainCollector:IntervalMinutes` in `appsettings.json`). Each snapshot is stored with a `CreatedAt` timestamp.

Data can also be collected on demand via `POST /api/blockchains/collect`.

## Running Tests

```bash
dotnet test
```

Tests use EF Core InMemory provider and don't require a running database.

## Configuration

Key settings in `appsettings.json`:

| Setting | Default | Description |
|---------|---------|-------------|
| `ConnectionStrings:DefaultConnection` | `Host=localhost;...` | PostgreSQL connection |
| `BlockchainCollector:IntervalMinutes` | `5` | Polling interval |

Serilog is configured to log to console and rolling files (`logs/` directory).

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
      ExternalApis/BlockCypherClient.cs
      Services/BlockchainDataCollector.cs
    ICMarkets.Api/
      Controllers/BlockchainsController.cs
      Middleware/ExceptionHandlingMiddleware.cs
      Program.cs
  tests/
    ICMarkets.UnitTests/
    ICMarkets.IntegrationTests/
    ICMarkets.FunctionalTests/
  Dockerfile
  docker-compose.yml
```
