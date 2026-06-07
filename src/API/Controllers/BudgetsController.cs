using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Features.Budgets.Commands;
using ExpenseTracker.Application.Features.Budgets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/v1/budgets")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BudgetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetBudgets([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _mediator.Send(new GetBudgetsQuery(month, year));
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> SetBudget([FromBody] SetBudgetRequest request)
    {
        await _mediator.Send(new SetBudgetCommand(request.CategoryId, request.Amount, request.MonthYear));
        return Ok(new { success = true, message = "Budget saved" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteBudgetCommand(id));
            return Ok(new { success = true, message = "Budget deleted" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "Budget not found" });
        }
    }
}
