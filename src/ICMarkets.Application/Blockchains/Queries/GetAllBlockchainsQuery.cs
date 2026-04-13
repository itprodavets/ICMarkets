using ICMarkets.Application.DTOs;
using ICMarkets.Domain.Interfaces;
using Mapster;
using MediatR;

namespace ICMarkets.Application.Blockchains.Queries;

public record GetAllBlockchainsQuery : IRequest<IReadOnlyList<BlockchainDataDto>>;

public class GetAllBlockchainsQueryHandler
    : IRequestHandler<GetAllBlockchainsQuery, IReadOnlyList<BlockchainDataDto>>
{
    private readonly IBlockchainDataRepository _repository;

    public GetAllBlockchainsQueryHandler(IBlockchainDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<BlockchainDataDto>> Handle(
        GetAllBlockchainsQuery request,
        CancellationToken cancellationToken)
    {
        var data = await _repository.GetLatestForAllAsync(cancellationToken);
        return data.Adapt<List<BlockchainDataDto>>();
    }
}
