using HabitService.Models;

namespace HabitService.Data;

public static class HabitSeeder
{
    public static async Task SeedAsync(HabitDbContext db)
    {
        if (db.Habits.Any()) return;

        var demoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var habits = new List<Habit>
        {
            new() {
                UserId      = demoUserId,
                Name        = "Morning Run",
                Description = "Run 5km before breakfast to start the day with energy",
                Frequency   = "daily",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            },
            new() {
                UserId      = demoUserId,
                Name        = "Read a Book",
                Description = "Read at least 20 pages of a non-fiction or technical book",
                Frequency   = "daily",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            },
            new() {
                UserId      = demoUserId,
                Name        = "Weekly Review",
                Description = "Review goals, reflect on the past week, and plan the next one",
                Frequency   = "weekly",
                TargetDaysPerWeek = 1,
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            },
            new() {
                UserId      = demoUserId,
                Name        = "Drink 2L of Water",
                Description = "Stay hydrated throughout the day by tracking water intake",
                Frequency   = "daily",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            }
        };

        await db.Habits.AddRangeAsync(habits);
        await db.SaveChangesAsync();
    }
}
