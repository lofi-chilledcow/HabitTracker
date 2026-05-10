using AuthService.Data;
using AuthService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Commands;

public class SetUserRoleCommandHandler(AppDbContext db) : IRequestHandler<SetUserRoleCommand, AdminUserDto?>
{
    public async Task<AdminUserDto?> Handle(SetUserRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = request.Role.Trim();
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken)
            ?? throw new ArgumentException("Role must be User or Admin.", nameof(request.Role));

        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return null;

        user.RoleId = role.Id;
        user.Role = role;
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
