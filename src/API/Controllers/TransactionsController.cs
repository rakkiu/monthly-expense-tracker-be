using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Features.Transactions.Commands;
using ExpenseTracker.Application.Features.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/v1/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int? month, [FromQuery] int? year,
        [FromQuery] Guid? categoryId, [FromQuery] string? type,
        [FromQuery] string? keyword, [FromQuery] string? sortBy,
        [FromQuery] string? sortDir, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetTransactionsQuery(
            month, year, categoryId, type, keyword, sortBy, sortDir, page, pageSize));

        return Ok(new
        {
            success = true,
            data = result.Items,
            pagination = new
            {
                page = result.Page,
                pageSize = result.PageSize,
                totalItems = result.TotalItems,
                totalPages = result.TotalPages
            }
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetTransactionByIdQuery(id));
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "Transaction not found" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        var result = await _mediator.Send(new CreateTransactionCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            new { success = true, data = result, message = "Transaction created" });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdateTransactionCommand(id, request));
            return Ok(new { success = true, data = result, message = "Transaction updated" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "Transaction not found" });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteTransactionCommand(id));
            return Ok(new { success = true, message = "Transaction deleted" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "Transaction not found" });
        }
    }
}
