-- ============================================================
-- 002_drop_habit_schema.sql
-- Drops the habit schema and all objects within it.
--
-- Drop order:
--   1. Indexes        (on habit.HabitCompletions, habit.Habits)
--   2. Tables         (child first: HabitCompletions, then Habits)
--   3. Schema         (habit)
--
-- Safe to re-run: each step is guarded by an existence check.
-- ============================================================

USE HabitTracker;
GO

-- ============================================================
-- 1. Indexes
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_completions_habitid_date'
           AND object_id = OBJECT_ID('habit.HabitCompletions'))
    DROP INDEX ix_completions_habitid_date ON habit.HabitCompletions;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_habits_userid'
           AND object_id = OBJECT_ID('habit.Habits'))
    DROP INDEX ix_habits_userid ON habit.Habits;
GO

-- ============================================================
-- 2. Tables
-- ============================================================

-- Child first (FK: fk_completions_habit → habit.Habits)
IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('habit.HabitCompletions'))
    DROP TABLE habit.HabitCompletions;
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('habit.Habits'))
    DROP TABLE habit.Habits;
GO

-- ============================================================
-- 3. Schema
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'habit')
    DROP SCHEMA habit;
GO

PRINT '002_drop_habit_schema.sql complete.';
GO
