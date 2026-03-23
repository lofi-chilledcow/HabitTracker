using HabitCompletionService.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitCompletionService.Data;

public class HabitCompletionDbContext(DbContextOptions<HabitCompletionDbContext> options) : DbContext(options)
{
    public DbSet<HabitCompletion> HabitCompletions => Set<HabitCompletion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HabitCompletion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HabitId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CompletedDate).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasIndex(e => e.HabitId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.CompletedDate });
        });
    }
}
