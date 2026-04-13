using ICMarkets.Application.Blockchains.Commands;
using ICMarkets.Application.Blockchains.Queries;
using ICMarkets.Application.Common;
using ICMarkets.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace ICMarkets.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BlockchainsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IOutputCacheStore _cacheStore;

    public BlockchainsController(IMediator mediator, IOutputCacheStore cacheStore)
    {
        _mediator = mediator;
        _cacheStore = cacheStore;
    }

    [HttpGet]
    [OutputCache(PolicyName = "BlockchainData")]
    [ProducesResponseType(typeof(IReadOnlyList<BlockchainDataDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllBlockchainsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{network}/{chain}")]
    [OutputCache(PolicyName = "BlockchainData", VaryByRouteValueNames = ["network", "chain"])]
    [ProducesResponseType(typeof(BlockchainDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatest(string network, string chain, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBlockchainDataQuery(network, chain), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("{network}/{chain}/history")]
    [ProducesResponseType(typeof(PagedResult<BlockchainDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(
        string network,
        string chain,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetBlockchainHistoryQuery(network, chain, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("collect")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Collect(CancellationToken ct)
    {
        var count = await _mediator.Send(new CollectBlockchainDataCommand(), ct);

        // invalidate cached responses so subsequent reads reflect fresh data
        await _cacheStore.EvictByTagAsync("blockchain", ct);

        return Ok(new { CollectedCount = count });
    }
}
