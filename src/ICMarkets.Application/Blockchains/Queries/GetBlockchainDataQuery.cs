using ICMarkets.Application.DTOs;
using ICMarkets.Domain.Interfaces;
using Mapster;
using MediatR;

namespace ICMarkets.Application.Blockchains.Queries;

public record GetBlockchainDataQuery(string Network, string Chain) : IRequest<BlockchainDataDto?>;

public class GetBlockchainDataQueryHandler
    : IRequestHandler<GetBlockchainDataQuery, BlockchainDataDto?>
{
    private readonly IBlockchainDataRepository _repository;

    public GetBlockchainDataQueryHandler(IBlockchainDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<BlockchainDataDto?> Handle(
        GetBlockchainDataQuery request,
        CancellationToken cancellationToken)
    {
        var data = await _repository.GetLatestAsync(request.Network, request.Chain, cancellationToken);
        return data?.Adapt<BlockchainDataDto>();
    }
}
