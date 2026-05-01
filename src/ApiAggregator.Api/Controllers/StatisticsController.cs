using ApiAggregator.Application.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class StatisticsController : ControllerBase {
    private IRequestStatisticsStore _statisticsStore { get; init; }

    public StatisticsController(IRequestStatisticsStore statisticsStore) {
        _statisticsStore = statisticsStore;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<ApiStatisticsDto>> GetApiProviderStatistics() {
        return Ok(_statisticsStore.GetStatistics());
    }
}
