using System.Text.Json.Serialization;
using ICMarkets.Infrastructure.ExternalApis.Models;

namespace ICMarkets.Infrastructure.ExternalApis;

[JsonSerializable(typeof(BlockCypherResponse))]
internal partial class BlockCypherJsonContext : JsonSerializerContext;
