using HabitCompletionService.Commands;
using HabitCompletionService.DTOs;
using HabitCompletionService.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitCompletionService.Controllers;

[Authorize]
[ApiController]
[Route("api/habit-completions")]
public class HabitCompletionsController(IMediator mediator) : ControllerBase
{
    private Guid? TryGetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : null;
    }

    [HttpPost]
    [ProducesResponseType<HabitCompletionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateHabitCompletionDto dto, CancellationToken cancellationToken)
    {
        if (TryGetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var result = await mediator.Send(
            new CreateHabitCompletionCommand(
                dto.HabitId,
                userId,
                dto.CompletedDate?.Date ?? DateTime.UtcNow.Date,
                dto.Notes),
            cancellationToken);

        return CreatedAtAction(nameof(GetByHabitId), new { habitId = result.HabitId }, result);
    }

    [HttpGet("habit/{habitId:guid}")]
    [ProducesResponseType<IEnumerable<HabitCompletionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByHabitId(Guid habitId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCompletionsByHabitIdQuery(habitId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("today")]
    [ProducesResponseType<IEnumerable<HabitCompletionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        if (TryGetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var result = await mediator.Send(new GetTodaysCompletionsQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (TryGetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var deleted = await mediator.Send(new DeleteHabitCompletionCommand(id, userId), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
