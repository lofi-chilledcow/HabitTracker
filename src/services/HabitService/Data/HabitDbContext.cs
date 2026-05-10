using HabitService.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitService.Data;

public class HabitDbContext(DbContextOptions<HabitDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitCompletion> HabitCompletions => Set<HabitCompletion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habit>(entity =>
        {
            entity.ToTable("Habits", "habit", table =>
            {
                table.HasCheckConstraint("CK_Habits_Frequency", "[Frequency] IN ('daily', 'weekly')");
                table.HasCheckConstraint("CK_Habits_TargetDaysPerWeek", "[TargetDaysPerWeek] IS NULL OR ([TargetDaysPerWeek] >= 1 AND [TargetDaysPerWeek] <= 7)");
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Frequency).IsRequired().HasMaxLength(20);
            entity.Property(e => e.IsPublic).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(e => new { e.UserId, e.IsActive });
            entity.HasIndex(e => new { e.IsPublic, e.IsActive });
        });

        modelBuilder.Entity<HabitCompletion>(entity =>
        {
            entity.ToTable("HabitCompletions", "habit");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(e => e.HabitId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CompletedDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(e => new { e.HabitId, e.CompletedDate }).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.CompletedDate });
            entity.HasIndex(e => e.HabitId);

            entity.HasOne(e => e.Habit)
                  .WithMany(h => h.Completions)
                  .HasForeignKey(e => e.HabitId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
