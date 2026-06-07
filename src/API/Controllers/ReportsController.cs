using ExpenseTracker.Application.Features.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _mediator.Send(new GetMonthlySummaryQuery(month, year));
        return Ok(new { success = true, data = result });
    }

    [HttpGet("category-breakdown")]
    public async Task<IActionResult> GetCategoryBreakdown([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _mediator.Send(new GetCategoryBreakdownQuery(month, year));
        return Ok(new { success = true, data = result });
    }

    [HttpGet("trend")]
    public async Task<IActionResult> GetTrend([FromQuery] int months = 6)
    {
        var result = await _mediator.Send(new GetTrendQuery(months));
        return Ok(new { success = true, data = result });
    }

    [HttpGet("top-categories")]
    public async Task<IActionResult> GetTopCategories([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _mediator.Send(new GetTopCategoriesQuery(month, year));
        return Ok(new { success = true, data = result });
    }
}
