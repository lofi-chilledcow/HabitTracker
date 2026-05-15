using AuthService.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Commands;

public class DeleteUserCommandHandler(AppDbContext db) : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return false;

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
