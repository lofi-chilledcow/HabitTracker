using HabitCompletionService.Data;
using HabitCompletionService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitCompletionService.Queries.Handlers;

public class GetTodaysCompletionsQueryHandler(HabitCompletionDbContext db)
    : IRequestHandler<GetTodaysCompletionsQuery, IEnumerable<HabitCompletionDto>>
{
    public async Task<IEnumerable<HabitCompletionDto>> Handle(GetTodaysCompletionsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        return await db.HabitCompletions
            .AsNoTracking()
            .Where(c => c.UserId == request.UserId && c.CompletedDate.Date == today)
            .OrderBy(c => c.CompletedDate)
            .Select(c => new HabitCompletionDto(c.Id, c.HabitId, c.UserId, c.CompletedDate, c.Notes, c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
