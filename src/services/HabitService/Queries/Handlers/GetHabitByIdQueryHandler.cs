using HabitService.Data;
using HabitService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Queries.Handlers;

public class GetHabitByIdQueryHandler(HabitDbContext db) : IRequestHandler<GetHabitByIdQuery, HabitDto?>
{
    public async Task<HabitDto?> Handle(GetHabitByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.Habits
            .AsNoTracking()
            .Where(h => h.Id == request.Id)
            .Select(h => new HabitDto(h.Id, h.Name, h.Description, h.Frequency, h.CreatedAt, h.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
