using HabitService.DTOs;
using HabitService.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitService.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/competition")]
public class CompetitionController(IMediator mediator) : ControllerBase
{
    [HttpGet("leaderboard")]
    [ProducesResponseType<IReadOnlyList<LeaderboardEntryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaderboard(
        [FromQuery] int days = 30,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetLeaderboardQuery(days, limit), cancellationToken);
        return Ok(result);
    }
}
