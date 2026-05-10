using HabitService.Commands;
using HabitService.DTOs;
using HabitService.Infrastructure;
using HabitService.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitService.Controllers;

[Authorize]
[ApiController]
[Route("api/completions")]
public class CompletionsController(IMediator mediator) : ControllerBase
{
    [HttpGet("today")]
    [ProducesResponseType<IReadOnlyList<HabitCompletionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        if (User.GetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await mediator.Send(new GetTodaysCompletionsQuery(userId, today), cancellationToken);
        return Ok(result);
    }

    [HttpGet("~/api/habits/{habitId:guid}/completions")]
    [ProducesResponseType<IReadOnlyList<HabitCompletionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByHabit(Guid habitId, CancellationToken cancellationToken)
    {
        if (User.GetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var result = await mediator.Send(new GetHabitCompletionsQuery(habitId, userId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("~/api/habits/{habitId:guid}/completions/{date}")]
    [ProducesResponseType<HabitCompletionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upsert(Guid habitId, DateOnly date, [FromBody] UpsertHabitCompletionDto dto, CancellationToken cancellationToken)
    {
        if (User.GetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var result = await mediator.Send(
            new UpsertHabitCompletionCommand(habitId, userId, date, dto.Notes),
            cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("~/api/habits/{habitId:guid}/completions/{date}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid habitId, DateOnly date, CancellationToken cancellationToken)
    {
        if (User.GetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var result = await mediator.Send(
            new DeleteHabitCompletionCommand(habitId, userId, date),
            cancellationToken);

        return result == DeleteHabitCompletionResult.HabitNotFound
            ? NotFound()
            : NoContent();
    }
}
