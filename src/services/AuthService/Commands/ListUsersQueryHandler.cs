using AuthService.Data;
using AuthService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Commands;

public class ListUsersQueryHandler(AppDbContext db) : IRequestHandler<ListUsersQuery, IReadOnlyList<AdminUserDto>>
{
    public async Task<IReadOnlyList<AdminUserDto>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .OrderBy(u => u.Username)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Username,
                u.Email,
                u.PhoneNumber,
                u.Role.Name,
                u.IsActive,
                u.CreatedAt,
                u.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
