using ApiAggregator.Application.Aggregation;
using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.Statistics;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AggregationController : ControllerBase {
    private readonly IAggregationService _aggregationService;

    public AggregationController(IAggregationService aggregationService) {
        _aggregationService = aggregationService;
    }
    
    // Keep query binding explicit so the public API contract is clear in Swagger.
    [HttpGet]
    public async Task<ActionResult<AggregatedResponseDto>> GetAggregatedData(
        [FromQuery] string? query,
        [FromQuery] string? category,
        [FromQuery] string sortBy = "date",
        [FromQuery] string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var aggregationQuery = new AggregationQuery
        {
            Query = query,
            Category = category,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var response = await _aggregationService.GetAggregatedDataAsync(
            aggregationQuery,
            cancellationToken);

        return Ok(response);
    }
}
