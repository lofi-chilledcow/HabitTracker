using HabitService.Data;
using HabitService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Queries.Handlers;

public class GetTodaysCompletionsQueryHandler(HabitDbContext db)
    : IRequestHandler<GetTodaysCompletionsQuery, IReadOnlyList<HabitCompletionDto>>
{
    public async Task<IReadOnlyList<HabitCompletionDto>> Handle(GetTodaysCompletionsQuery request, CancellationToken cancellationToken)
    {
        return await db.HabitCompletions
            .AsNoTracking()
            .Where(c => c.UserId == request.UserId && c.CompletedDate == request.Today)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new HabitCompletionDto(
                c.Id,
                c.HabitId,
                c.CompletedDate,
                c.Notes,
                c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
