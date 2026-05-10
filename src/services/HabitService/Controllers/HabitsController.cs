using HabitService.Commands;
using HabitService.DTOs;
using HabitService.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HabitsController(IMediator mediator) : ControllerBase
{
    private Guid? TryGetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : null;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<HabitDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        if (TryGetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var result = await mediator.Send(new GetAllHabitsQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<HabitDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (TryGetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var result = await mediator.Send(new GetHabitByIdQuery(id, userId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<HabitDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateHabitDto dto, CancellationToken cancellationToken)
    {
        if (TryGetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var result = await mediator.Send(
            new CreateHabitCommand(userId, dto.Name, dto.Description, dto.Frequency, dto.TargetDaysPerWeek, dto.IsPublic),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<HabitDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHabitDto dto, CancellationToken cancellationToken)
    {
        if (TryGetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var result = await mediator.Send(
            new UpdateHabitCommand(id, userId, dto.Name, dto.Description, dto.Frequency, dto.TargetDaysPerWeek, dto.IsPublic),
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (TryGetCurrentUserId() is not Guid userId)
            return Unauthorized();

        var deleted = await mediator.Send(new DeleteHabitCommand(id, userId), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
