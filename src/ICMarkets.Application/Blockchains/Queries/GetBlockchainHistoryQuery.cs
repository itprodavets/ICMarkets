using ICMarkets.Application.Common;
using ICMarkets.Application.DTOs;
using ICMarkets.Domain.Interfaces;
using ICMarkets.Domain.ValueObjects;
using FluentValidation;
using Mapster;
using MediatR;

namespace ICMarkets.Application.Blockchains.Queries;

public record GetBlockchainHistoryQuery(
    string Network,
    string Chain,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<BlockchainDataDto>>;

public class GetBlockchainHistoryQueryHandler
    : IRequestHandler<GetBlockchainHistoryQuery, PagedResult<BlockchainDataDto>>
{
    private readonly IBlockchainDataRepository _repository;

    public GetBlockchainHistoryQueryHandler(IBlockchainDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<BlockchainDataDto>> Handle(
        GetBlockchainHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetHistoryAsync(
            request.Network, request.Chain, request.Page, request.PageSize, cancellationToken);

        return new PagedResult<BlockchainDataDto>
        {
            Items = items.Adapt<List<BlockchainDataDto>>(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

public class GetBlockchainHistoryQueryValidator : AbstractValidator<GetBlockchainHistoryQuery>
{
    public GetBlockchainHistoryQueryValidator()
    {
        RuleFor(x => x.Network)
            .NotEmpty()
            .Must(n => BlockchainEndpoint.Supported.Any(e => e.Network == n))
            .WithMessage("Unsupported blockchain network. Supported: eth, btc, dash, ltc");

        RuleFor(x => x.Chain).NotEmpty();

        RuleFor(x => x.Page).GreaterThan(0);

        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
