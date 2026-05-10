using AuthService.Data;
using AuthService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Commands;

public class SetUserStatusCommandHandler(AppDbContext db) : IRequestHandler<SetUserStatusCommand, AdminUserDto?>
{
    public async Task<AdminUserDto?> Handle(SetUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return null;

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return new AdminUserDto(
            user.Id,
            user.Username,
            user.Email,
            user.PhoneNumber,
            user.Role.Name,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
