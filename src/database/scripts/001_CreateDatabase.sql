-- ============================================================
-- 001_CreateDatabase.sql
-- Creates the HabitTracker database, schemas, and all tables.
-- Run once against your local SQL Server instance.
-- ============================================================

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'HabitTracker')
BEGIN
    CREATE DATABASE HabitTracker;
END
GO

USE HabitTracker;
GO

-- ============================================================
-- Schemas
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'auth')
    EXEC('CREATE SCHEMA auth');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'habit')
    EXEC('CREATE SCHEMA habit');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'monitor')
    EXEC('CREATE SCHEMA monitor');
GO

-- ============================================================
-- auth.Users
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('auth.Users'))
BEGIN
    CREATE TABLE auth.Users (
        UserId       INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
        Email        NVARCHAR(256)  NOT NULL,
        PasswordHash NVARCHAR(512)  NOT NULL,
        DisplayName  NVARCHAR(100)  NOT NULL,
        Role         NVARCHAR(20)   NOT NULL DEFAULT 'user'
                         CONSTRAINT chk_users_role CHECK (Role IN ('user', 'admin')),
        IsActive     BIT            NOT NULL DEFAULT 1,
        CreatedAt    DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt    DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT uq_users_email UNIQUE (Email)
    );
END
GO

-- ============================================================
-- auth.RefreshTokens
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('auth.RefreshTokens'))
BEGIN
    CREATE TABLE auth.RefreshTokens (
        TokenId   INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        UserId    INT           NOT NULL,
        Token     NVARCHAR(512) NOT NULL,
        ExpiresAt DATETIME2     NOT NULL,
        CreatedAt DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
        RevokedAt DATETIME2     NULL,
        CONSTRAINT fk_refreshtokens_user FOREIGN KEY (UserId)
            REFERENCES auth.Users (UserId) ON DELETE CASCADE,
        CONSTRAINT uq_refreshtokens_token UNIQUE (Token)
    );
END
GO

-- ============================================================
-- habit.Habits
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('habit.Habits'))
BEGIN
    CREATE TABLE habit.Habits (
        HabitId           INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        UserId            INT           NOT NULL,
        Name              NVARCHAR(200) NOT NULL,
        Description       NVARCHAR(1000) NULL,
        Frequency         NVARCHAR(10)  NOT NULL
                              CONSTRAINT chk_habits_frequency CHECK (Frequency IN ('daily', 'weekly')),
        TargetDaysPerWeek TINYINT       NULL,
        IsPublic          BIT           NOT NULL DEFAULT 0,
        IsActive          BIT           NOT NULL DEFAULT 1,
        CreatedAt         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- ============================================================
-- habit.HabitCompletions
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('habit.HabitCompletions'))
BEGIN
    CREATE TABLE habit.HabitCompletions (
        CompletionId  INT      NOT NULL IDENTITY(1,1) PRIMARY KEY,
        HabitId       INT      NOT NULL,
        CompletedDate DATE     NOT NULL,
        CreatedAt     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT fk_completions_habit FOREIGN KEY (HabitId)
            REFERENCES habit.Habits (HabitId) ON DELETE CASCADE,
        CONSTRAINT uq_completions_habit_date UNIQUE (HabitId, CompletedDate)
    );
END
GO

-- ============================================================
-- monitor.RequestLogs
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('monitor.RequestLogs'))
BEGIN
    CREATE TABLE monitor.RequestLogs (
        LogId       INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        ServiceName NVARCHAR(100) NOT NULL,
        Endpoint    NVARCHAR(500) NOT NULL,
        Method      NVARCHAR(10)  NOT NULL,
        StatusCode  INT           NOT NULL,
        DurationMs  INT           NOT NULL,
        UserId      INT           NULL,
        CreatedAt   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- ============================================================
-- monitor.HealthChecks
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('monitor.HealthChecks'))
BEGIN
    CREATE TABLE monitor.HealthChecks (
        CheckId     INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
        ServiceName NVARCHAR(100)  NOT NULL,
        Status      NVARCHAR(20)   NOT NULL
                        CONSTRAINT chk_health_status CHECK (Status IN ('healthy', 'degraded', 'unhealthy')),
        ResponseMs  INT            NOT NULL,
        Details     NVARCHAR(1000) NULL,
        CheckedAt   DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- ============================================================
-- Indexes
-- ============================================================

-- Speed up token lookups for refresh flow
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_refreshtokens_userid' AND object_id = OBJECT_ID('auth.RefreshTokens'))
    CREATE INDEX ix_refreshtokens_userid ON auth.RefreshTokens (UserId);
GO

-- Speed up habit list queries by user
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_habits_userid' AND object_id = OBJECT_ID('habit.Habits'))
    CREATE INDEX ix_habits_userid ON habit.Habits (UserId);
GO

-- Speed up completion history lookups
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_completions_habitid_date' AND object_id = OBJECT_ID('habit.HabitCompletions'))
    CREATE INDEX ix_completions_habitid_date ON habit.HabitCompletions (HabitId, CompletedDate DESC);
GO

-- Speed up request log queries by service and time
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_requestlogs_service_created' AND object_id = OBJECT_ID('monitor.RequestLogs'))
    CREATE INDEX ix_requestlogs_service_created ON monitor.RequestLogs (ServiceName, CreatedAt DESC);
GO

-- Speed up health check history queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_healthchecks_service_checked' AND object_id = OBJECT_ID('monitor.HealthChecks'))
    CREATE INDEX ix_healthchecks_service_checked ON monitor.HealthChecks (ServiceName, CheckedAt DESC);
GO

PRINT 'HabitTracker database setup complete.';
GO
