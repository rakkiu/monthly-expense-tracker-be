using ExpenseTracker.Application.Features.Export.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/v1/export")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] int? month, [FromQuery] int? year)
    {
        var data = await _mediator.Send(new ExportPdfCommand(month, year));
        return File(data, "application/pdf", $"expense-report-{month ?? DateTime.UtcNow.Month}-{year ?? DateTime.UtcNow.Year}.pdf");
    }
}
