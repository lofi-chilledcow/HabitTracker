using AuthService.Commands;
using AuthService.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AdminUserDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListUsersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<AdminUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetStatus(Guid id, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SetUserStatusCommand(id, request.IsActive), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/role")]
    [ProducesResponseType<AdminUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetRole(Guid id, UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new SetUserRoleCommand(id, request.Role), cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
