using HabitCompletionService.Data;
using HabitCompletionService.DTOs;
using HabitCompletionService.Models;
using MediatR;

namespace HabitCompletionService.Commands.Handlers;

public class CreateHabitCompletionCommandHandler(HabitCompletionDbContext db)
    : IRequestHandler<CreateHabitCompletionCommand, HabitCompletionDto>
{
    public async Task<HabitCompletionDto> Handle(CreateHabitCompletionCommand request, CancellationToken cancellationToken)
    {
        var completion = new HabitCompletion
        {
            HabitId = request.HabitId,
            UserId = request.UserId,
            CompletedDate = request.CompletedDate,
            Notes = request.Notes
        };

        db.HabitCompletions.Add(completion);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(completion);
    }

    private static HabitCompletionDto ToDto(HabitCompletion c) =>
        new(c.Id, c.HabitId, c.UserId, c.CompletedDate, c.Notes, c.CreatedAt);
}
