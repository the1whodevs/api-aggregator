using ApiAggregator.Application.Aggregation;
using ApiAggregator.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AggregationController : ControllerBase {
    private readonly IAggregationService _aggregationService;

    public AggregationController(IAggregationService aggregationService) {
        _aggregationService = aggregationService;
    }

    [HttpGet]
    public async Task<ActionResult<AggregatedResponseDto>> GetAggregatedData(
        [FromQuery] AggregationQuery query,
        CancellationToken cancellationToken) {
        var response = await _aggregationService.GetAggregatedDataAsync(
            query, cancellationToken);

        return Ok(response);
    }
}