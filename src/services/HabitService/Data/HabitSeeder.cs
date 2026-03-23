using HabitService.Models;

namespace HabitService.Data;

public static class HabitSeeder
{
    public static async Task SeedAsync(HabitDbContext db)
    {
        if (db.Habits.Any()) return;

        var habits = new List<Habit>
        {
            new() {
                Name        = "Morning Run",
                Description = "Run 5km before breakfast to start the day with energy",
                Frequency   = "Daily",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            },
            new() {
                Name        = "Read a Book",
                Description = "Read at least 20 pages of a non-fiction or technical book",
                Frequency   = "Daily",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            },
            new() {
                Name        = "Weekly Review",
                Description = "Review goals, reflect on the past week, and plan the next one",
                Frequency   = "Weekly",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            },
            new() {
                Name        = "Drink 2L of Water",
                Description = "Stay hydrated throughout the day by tracking water intake",
                Frequency   = "Daily",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            },
            new() {
                Name        = "Monthly Budget Check",
                Description = "Review spending, update budget categories, and set savings targets",
                Frequency   = "Monthly",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = false
            }
        };

        await db.Habits.AddRangeAsync(habits);
        await db.SaveChangesAsync();
    }
}
