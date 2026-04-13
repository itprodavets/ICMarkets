using ICMarkets.Application.Blockchains.Commands;
using ICMarkets.Application.Blockchains.Queries;
using ICMarkets.Application.Common;
using ICMarkets.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ICMarkets.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BlockchainsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BlockchainsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BlockchainDataDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllBlockchainsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{network}/{chain}")]
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
        return Ok(new { CollectedCount = count });
    }
}
