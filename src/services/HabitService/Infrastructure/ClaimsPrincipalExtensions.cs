using System.Security.Claims;

namespace HabitService.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetCurrentUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue("sub");

        return Guid.TryParse(value, out var id) ? id : null;
    }
}
