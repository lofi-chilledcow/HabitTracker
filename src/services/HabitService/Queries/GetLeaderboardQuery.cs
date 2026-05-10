using HabitService.DTOs;
using MediatR;

namespace HabitService.Queries;

public record GetLeaderboardQuery(int Days = 30, int Limit = 50) : IRequest<IReadOnlyList<LeaderboardEntryDto>>;
