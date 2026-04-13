using System.Text.Json.Serialization;
using ICMarkets.Application.Common;
using ICMarkets.Application.DTOs;

namespace ICMarkets.Api.Serialization;

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(BlockchainDataDto))]
[JsonSerializable(typeof(List<BlockchainDataDto>))]
[JsonSerializable(typeof(PagedResult<BlockchainDataDto>))]
[JsonSerializable(typeof(CollectResult))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(ValidationErrorResponse))]
internal partial class ApiJsonContext : JsonSerializerContext;

public sealed record CollectResult(int CollectedCount);

public sealed record ErrorResponse(string Title, int Status);

public sealed record ValidationErrorResponse(
    string Title,
    int Status,
    IEnumerable<ValidationFieldError> Errors);

public sealed record ValidationFieldError(string PropertyName, string ErrorMessage);
