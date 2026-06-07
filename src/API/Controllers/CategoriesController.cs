using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Features.Categories.Commands;
using ExpenseTracker.Application.Features.Categories.Queries;
using ExpenseTracker.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/v1/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetCategoriesQuery());
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var result = await _mediator.Send(new CreateCategoryCommand(request));
            return Ok(new { success = true, data = result, message = "Category created" });
        }
        catch (DomainException ex)
        {
            return Conflict(new { success = false, error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdateCategoryCommand(id, request));
            return Ok(new { success = true, data = result, message = "Category updated" });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "Category not found" });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteCategoryCommand(id));
            return Ok(new { success = true, message = "Category deleted" });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "Category not found" });
        }
    }
}
