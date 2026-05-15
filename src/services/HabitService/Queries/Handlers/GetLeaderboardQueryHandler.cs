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
            .Join(
                db.UserProfiles.AsNoTracking(),
                habit => habit.UserId,
                user => user.Id,
                (habit, user) => new { Habit = habit, User = user })
            .Select(h => new
            {
                h.Habit.Id,
                h.User.Username,
                h.Habit.Name,
                h.Habit.Description,
                h.Habit.Frequency,
                h.Habit.TargetDaysPerWeek,
                CompletionCount = h.Habit.Completions.Count(c => c.CompletedDate >= fromDate),
                h.Habit.CreatedAt
            })
            .OrderByDescending(h => h.CompletionCount)
            .ThenBy(h => h.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return rows
            .Select(h => new LeaderboardEntryDto(
                h.Id,
                h.Username,
                h.Name,
                h.Description,
                h.Frequency,
                h.TargetDaysPerWeek,
                h.CompletionCount,
                h.CreatedAt))
            .ToList();
    }
}
