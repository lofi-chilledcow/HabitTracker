namespace HabitCompletionService.Models;

public class HabitCompletion : BaseEntity
{
    public Guid HabitId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CompletedDate { get; set; }
    public string? Notes { get; set; }
}
