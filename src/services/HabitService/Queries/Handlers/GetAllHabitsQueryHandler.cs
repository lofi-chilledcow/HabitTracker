using HabitService.Data;
using HabitService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Queries.Handlers;

public class GetAllHabitsQueryHandler(HabitDbContext db) : IRequestHandler<GetAllHabitsQuery, IEnumerable<HabitDto>>
{
    public async Task<IEnumerable<HabitDto>> Handle(GetAllHabitsQuery request, CancellationToken cancellationToken)
    {
        return await db.Habits
            .AsNoTracking()
            .Where(h => h.UserId == request.UserId && h.IsActive)
            .Select(h => new HabitDto(
                h.Id,
                h.Name,
                h.Description,
                h.Frequency,
                h.TargetDaysPerWeek,
                h.IsPublic,
                h.CreatedAt,
                h.UpdatedAt,
                h.IsActive))
            .ToListAsync(cancellationToken);
    }
}
