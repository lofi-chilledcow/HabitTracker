using HabitCompletionService.Data;
using HabitCompletionService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitCompletionService.Queries.Handlers;

public class GetCompletionsByHabitIdQueryHandler(HabitCompletionDbContext db)
    : IRequestHandler<GetCompletionsByHabitIdQuery, IEnumerable<HabitCompletionDto>>
{
    public async Task<IEnumerable<HabitCompletionDto>> Handle(GetCompletionsByHabitIdQuery request, CancellationToken cancellationToken)
    {
        return await db.HabitCompletions
            .AsNoTracking()
            .Where(c => c.HabitId == request.HabitId)
            .OrderByDescending(c => c.CompletedDate)
            .Select(c => new HabitCompletionDto(c.Id, c.HabitId, c.UserId, c.CompletedDate, c.Notes, c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
