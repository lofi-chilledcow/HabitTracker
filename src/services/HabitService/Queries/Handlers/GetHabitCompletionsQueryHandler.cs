using HabitService.Data;
using HabitService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Queries.Handlers;

public class GetHabitCompletionsQueryHandler(HabitDbContext db)
    : IRequestHandler<GetHabitCompletionsQuery, IReadOnlyList<HabitCompletionDto>?>
{
    public async Task<IReadOnlyList<HabitCompletionDto>?> Handle(GetHabitCompletionsQuery request, CancellationToken cancellationToken)
    {
        var habitExists = await db.Habits.AnyAsync(
            h => h.Id == request.HabitId && h.UserId == request.UserId,
            cancellationToken);

        if (!habitExists)
            return null;

        return await db.HabitCompletions
            .AsNoTracking()
            .Where(c => c.HabitId == request.HabitId && c.UserId == request.UserId)
            .OrderByDescending(c => c.CompletedDate)
            .Select(c => new HabitCompletionDto(
                c.Id,
                c.HabitId,
                c.CompletedDate,
                c.Notes,
                c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
