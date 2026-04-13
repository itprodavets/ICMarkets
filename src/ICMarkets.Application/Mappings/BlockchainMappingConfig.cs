using ICMarkets.Application.DTOs;
using ICMarkets.Domain.Entities;
using Mapster;

namespace ICMarkets.Application.Mappings;

public class BlockchainMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BlockchainData, BlockchainDataDto>();
    }
}
