FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["src/ICMarkets.Domain/ICMarkets.Domain.csproj", "ICMarkets.Domain/"]
COPY ["src/ICMarkets.Application/ICMarkets.Application.csproj", "ICMarkets.Application/"]
COPY ["src/ICMarkets.Infrastructure/ICMarkets.Infrastructure.csproj", "ICMarkets.Infrastructure/"]
COPY ["src/ICMarkets.Api/ICMarkets.Api.csproj", "ICMarkets.Api/"]

RUN dotnet restore "ICMarkets.Api/ICMarkets.Api.csproj"

COPY src/ .
WORKDIR "/src/ICMarkets.Api"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ICMarkets.Api.dll"]
