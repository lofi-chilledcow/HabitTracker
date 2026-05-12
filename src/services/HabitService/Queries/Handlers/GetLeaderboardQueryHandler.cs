using HabitService.Data;
using HabitService.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Queries.Handlers;

public class GetLeaderboardQueryHandler(HabitDbContext db)
    : IRequestHandler<GetLeaderboardQuery, IReadOnlyList<LeaderboardEntryDto>>
{
    public async Task<IReadOnlyList<LeaderboardEntryDto>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var days = Math.Clamp(request.Days, 1, 365);
        var limit = Math.Clamp(request.Limit, 1, 100);
        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-(days - 1)));

        var rows = await db.Habits
            .AsNoTracking()
            .Where(h => h.IsActive && h.IsPublic)
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.Description,
                h.Frequency,
                h.TargetDaysPerWeek,
                CompletionCount = h.Completions.Count(c => c.CompletedDate >= fromDate),
                h.CreatedAt
            })
            .OrderByDescending(h => h.CompletionCount)
            .ThenBy(h => h.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return rows
            .Select(h => new LeaderboardEntryDto(
                h.Id,
                h.Name,
                h.Description,
                h.Frequency,
                h.TargetDaysPerWeek,
                h.CompletionCount,
                h.CreatedAt))
            .ToList();
    }
}
