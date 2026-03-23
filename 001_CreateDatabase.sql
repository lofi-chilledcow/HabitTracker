-- ============================================================================
-- HabitTracker Database Setup Script
-- Step 1: Database, Schemas, Tables, Indexes, Seed Data
-- Target: SQL Server (2019+)
-- ============================================================================

-- ============================================================================
-- 1. CREATE DATABASE
-- ============================================================================
USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'HabitTracker')
BEGIN
    CREATE DATABASE [HabitTracker];
END
GO

USE [HabitTracker];
GO

-- ============================================================================
-- 2. CREATE SCHEMAS (service-owned boundaries)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'auth')
    EXEC('CREATE SCHEMA [auth]');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'habit')
    EXEC('CREATE SCHEMA [habit]');
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'monitor')
    EXEC('CREATE SCHEMA [monitor]');
GO

-- ============================================================================
-- 3. TABLES — Auth Schema
-- ============================================================================

-- Users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('auth'))
BEGIN
    CREATE TABLE [auth].[Users]
    (
        [UserId]        INT             IDENTITY(1,1)   NOT NULL,
        [Email]         NVARCHAR(256)   NOT NULL,
        [PasswordHash]  NVARCHAR(MAX)   NOT NULL,
        [DisplayName]   NVARCHAR(100)   NOT NULL,
        [Role]          NVARCHAR(20)    NOT NULL        DEFAULT 'user',      -- 'user' | 'admin'
        [IsActive]      BIT             NOT NULL        DEFAULT 1,
        [CreatedAt]     DATETIME2(3)    NOT NULL        DEFAULT GETUTCDATE(),
        [UpdatedAt]     DATETIME2(3)    NOT NULL        DEFAULT GETUTCDATE(),

        CONSTRAINT [PK_Users]           PRIMARY KEY CLUSTERED ([UserId]),
        CONSTRAINT [UQ_Users_Email]     UNIQUE ([Email]),
        CONSTRAINT [CK_Users_Role]      CHECK ([Role] IN ('user', 'admin'))
    );
END
GO

-- Refresh Tokens (for JWT refresh flow)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens' AND schema_id = SCHEMA_ID('auth'))
BEGIN
    CREATE TABLE [auth].[RefreshTokens]
    (
        [TokenId]       INT             IDENTITY(1,1)   NOT NULL,
        [UserId]        INT             NOT NULL,
        [Token]         NVARCHAR(500)   NOT NULL,
        [ExpiresAt]     DATETIME2(3)    NOT NULL,
        [CreatedAt]     DATETIME2(3)    NOT NULL        DEFAULT GETUTCDATE(),
        [RevokedAt]     DATETIME2(3)    NULL,

        CONSTRAINT [PK_RefreshTokens]           PRIMARY KEY CLUSTERED ([TokenId]),
        CONSTRAINT [FK_RefreshTokens_Users]     FOREIGN KEY ([UserId]) REFERENCES [auth].[Users]([UserId])
    );
END
GO

-- ============================================================================
-- 4. TABLES — Habit Schema
-- ============================================================================

-- Habits
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Habits' AND schema_id = SCHEMA_ID('habit'))
BEGIN
    CREATE TABLE [habit].[Habits]
    (
        [HabitId]           INT             IDENTITY(1,1)   NOT NULL,
        [UserId]            INT             NOT NULL,
        [Name]              NVARCHAR(200)   NOT NULL,
        [Description]       NVARCHAR(500)   NULL,
        [Frequency]         NVARCHAR(10)    NOT NULL        DEFAULT 'daily',    -- 'daily' | 'weekly'
        [TargetDaysPerWeek] INT             NULL,                                -- for weekly habits
        [IsPublic]          BIT             NOT NULL        DEFAULT 0,           -- competition page visibility
        [IsActive]          BIT             NOT NULL        DEFAULT 1,           -- soft delete
        [CreatedAt]         DATETIME2(3)    NOT NULL        DEFAULT GETUTCDATE(),
        [UpdatedAt]         DATETIME2(3)    NOT NULL        DEFAULT GETUTCDATE(),

        CONSTRAINT [PK_Habits]              PRIMARY KEY CLUSTERED ([HabitId]),
        CONSTRAINT [FK_Habits_Users]        FOREIGN KEY ([UserId]) REFERENCES [auth].[Users]([UserId]),
        CONSTRAINT [CK_Habits_Frequency]    CHECK ([Frequency] IN ('daily', 'weekly')),
        CONSTRAINT [CK_Habits_TargetDays]   CHECK ([TargetDaysPerWeek] IS NULL OR ([TargetDaysPerWeek] BETWEEN 1 AND 7))
    );
END
GO

-- Habit Completions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HabitCompletions' AND schema_id = SCHEMA_ID('habit'))
BEGIN
    CREATE TABLE [habit].[HabitCompletions]
    (
        [CompletionId]  INT             IDENTITY(1,1)   NOT NULL,
        [HabitId]       INT             NOT NULL,
        [CompletedDate] DATE            NOT NULL,
        [CreatedAt]     DATETIME2(3)    NOT NULL        DEFAULT GETUTCDATE(),

        CONSTRAINT [PK_HabitCompletions]            PRIMARY KEY CLUSTERED ([CompletionId]),
        CONSTRAINT [FK_HabitCompletions_Habits]     FOREIGN KEY ([HabitId]) REFERENCES [habit].[Habits]([HabitId]),
        CONSTRAINT [UQ_HabitCompletions_PerDay]     UNIQUE ([HabitId], [CompletedDate])  -- one check per habit per day
    );
END
GO

-- ============================================================================
-- 5. TABLES — Monitor Schema (for admin dashboard)
-- ============================================================================

-- API Request Logs (lightweight monitoring)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RequestLogs' AND schema_id = SCHEMA_ID('monitor'))
BEGIN
    CREATE TABLE [monitor].[RequestLogs]
    (
        [LogId]         BIGINT          IDENTITY(1,1)   NOT NULL,
        [ServiceName]   NVARCHAR(50)    NOT NULL,           -- 'AuthService' | 'HabitService'
        [Endpoint]      NVARCHAR(200)   NOT NULL,
        [Method]        NVARCHAR(10)    NOT NULL,           -- GET, POST, PUT, DELETE
        [StatusCode]    INT             NOT NULL,
        [DurationMs]    INT             NOT NULL,
        [UserId]        INT             NULL,
        [CreatedAt]     DATETIME2(3)    NOT NULL        DEFAULT GETUTCDATE(),

        CONSTRAINT [PK_RequestLogs]     PRIMARY KEY CLUSTERED ([LogId])
    );
END
GO

-- Health Check Logs
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HealthChecks' AND schema_id = SCHEMA_ID('monitor'))
BEGIN
    CREATE TABLE [monitor].[HealthChecks]
    (
        [CheckId]       BIGINT          IDENTITY(1,1)   NOT NULL,
        [ServiceName]   NVARCHAR(50)    NOT NULL,
        [Status]        NVARCHAR(20)    NOT NULL,           -- 'healthy' | 'degraded' | 'unhealthy'
        [ResponseMs]    INT             NOT NULL,
        [Details]       NVARCHAR(MAX)   NULL,
        [CheckedAt]     DATETIME2(3)    NOT NULL        DEFAULT GETUTCDATE(),

        CONSTRAINT [PK_HealthChecks]    PRIMARY KEY CLUSTERED ([CheckId])
    );
END
GO

-- ============================================================================
-- 6. INDEXES (query performance)
-- ============================================================================

-- Auth indexes
CREATE NONCLUSTERED INDEX [IX_Users_Email]
    ON [auth].[Users]([Email])
    WHERE [IsActive] = 1;
GO

CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId]
    ON [auth].[RefreshTokens]([UserId])
    WHERE [RevokedAt] IS NULL;
GO

-- Habit indexes
CREATE NONCLUSTERED INDEX [IX_Habits_UserId]
    ON [habit].[Habits]([UserId])
    INCLUDE ([Name], [Frequency], [IsPublic], [IsActive])
    WHERE [IsActive] = 1;
GO

CREATE NONCLUSTERED INDEX [IX_Habits_Public]
    ON [habit].[Habits]([IsPublic])
    INCLUDE ([UserId], [Name], [Frequency])
    WHERE [IsPublic] = 1 AND [IsActive] = 1;
GO

CREATE NONCLUSTERED INDEX [IX_HabitCompletions_HabitId_Date]
    ON [habit].[HabitCompletions]([HabitId], [CompletedDate] DESC);
GO

CREATE NONCLUSTERED INDEX [IX_HabitCompletions_Date]
    ON [habit].[HabitCompletions]([CompletedDate])
    INCLUDE ([HabitId]);
GO

-- Monitor indexes
CREATE NONCLUSTERED INDEX [IX_RequestLogs_CreatedAt]
    ON [monitor].[RequestLogs]([CreatedAt] DESC)
    INCLUDE ([ServiceName], [StatusCode], [DurationMs]);
GO

CREATE NONCLUSTERED INDEX [IX_RequestLogs_StatusCode]
    ON [monitor].[RequestLogs]([StatusCode])
    WHERE [StatusCode] >= 400;
GO

CREATE NONCLUSTERED INDEX [IX_HealthChecks_ServiceName]
    ON [monitor].[HealthChecks]([ServiceName], [CheckedAt] DESC);
GO

-- ============================================================================
-- 7. SEED DATA
-- ============================================================================

-- Admin user (password: Admin@123 — change in production!)
-- This is a BCrypt hash placeholder. Replace with actual hash from your Auth Service.
IF NOT EXISTS (SELECT 1 FROM [auth].[Users] WHERE [Email] = 'admin@habittracker.local')
BEGIN
    INSERT INTO [auth].[Users] ([Email], [PasswordHash], [DisplayName], [Role])
    VALUES ('admin@habittracker.local', '$REPLACE_WITH_BCRYPT_HASH$', 'Admin', 'admin');
END
GO

-- Demo user (password: Demo@123 — for testing only)
IF NOT EXISTS (SELECT 1 FROM [auth].[Users] WHERE [Email] = 'demo@habittracker.local')
BEGIN
    INSERT INTO [auth].[Users] ([Email], [PasswordHash], [DisplayName], [Role])
    VALUES ('demo@habittracker.local', '$REPLACE_WITH_BCRYPT_HASH$', 'Demo User', 'user');
END
GO

-- Sample habits for demo user
DECLARE @DemoUserId INT = (SELECT [UserId] FROM [auth].[Users] WHERE [Email] = 'demo@habittracker.local');

IF @DemoUserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [habit].[Habits] WHERE [UserId] = @DemoUserId)
BEGIN
    INSERT INTO [habit].[Habits] ([UserId], [Name], [Description], [Frequency], [TargetDaysPerWeek], [IsPublic])
    VALUES
        (@DemoUserId, 'Drink 8 glasses of water',   'Stay hydrated throughout the day',     'daily',  NULL, 1),
        (@DemoUserId, 'Read for 30 minutes',         'Read a book before bed',               'daily',  NULL, 1),
        (@DemoUserId, 'Exercise',                    'Gym, run, or home workout',            'weekly', 4,    1),
        (@DemoUserId, 'Meditate',                    '10 minutes of mindfulness',            'daily',  NULL, 0),
        (@DemoUserId, 'Learn something new',         'Watch a tutorial or read docs',        'weekly', 3,    0);
END
GO

-- ============================================================================
-- 8. USEFUL VIEWS (for common queries)
-- ============================================================================

-- Today's habits for a user (used by Dashboard)
CREATE OR ALTER VIEW [habit].[vw_TodayHabits]
AS
    SELECT
        h.[HabitId],
        h.[UserId],
        h.[Name],
        h.[Description],
        h.[Frequency],
        h.[IsPublic],
        CAST(GETUTCDATE() AS DATE) AS [Today],
        CASE
            WHEN c.[CompletionId] IS NOT NULL THEN 1
            ELSE 0
        END AS [IsCompletedToday]
    FROM [habit].[Habits] h
    LEFT JOIN [habit].[HabitCompletions] c
        ON c.[HabitId] = h.[HabitId]
        AND c.[CompletedDate] = CAST(GETUTCDATE() AS DATE)
    WHERE h.[IsActive] = 1;
GO

-- Competition page view (public habits with streaks)
CREATE OR ALTER VIEW [habit].[vw_Competition]
AS
    SELECT
        u.[DisplayName],
        h.[HabitId],
        h.[Name] AS [HabitName],
        h.[Frequency],
        COUNT(c.[CompletionId]) AS [TotalCompletions],
        MAX(c.[CompletedDate]) AS [LastCompletedDate]
    FROM [habit].[Habits] h
    INNER JOIN [auth].[Users] u ON u.[UserId] = h.[UserId]
    LEFT JOIN [habit].[HabitCompletions] c ON c.[HabitId] = h.[HabitId]
    WHERE h.[IsPublic] = 1
      AND h.[IsActive] = 1
      AND u.[IsActive] = 1
    GROUP BY u.[DisplayName], h.[HabitId], h.[Name], h.[Frequency];
GO

-- Admin monitoring: API health summary
CREATE OR ALTER VIEW [monitor].[vw_ApiHealthSummary]
AS
    SELECT
        [ServiceName],
        COUNT(*) AS [TotalRequests],
        AVG([DurationMs]) AS [AvgDurationMs],
        MAX([DurationMs]) AS [MaxDurationMs],
        SUM(CASE WHEN [StatusCode] >= 400 THEN 1 ELSE 0 END) AS [ErrorCount],
        CAST(
            SUM(CASE WHEN [StatusCode] >= 400 THEN 1.0 ELSE 0.0 END) /
            NULLIF(COUNT(*), 0) * 100
        AS DECIMAL(5,2)) AS [ErrorRatePercent],
        MIN([CreatedAt]) AS [PeriodStart],
        MAX([CreatedAt]) AS [PeriodEnd]
    FROM [monitor].[RequestLogs]
    WHERE [CreatedAt] >= DATEADD(HOUR, -24, GETUTCDATE())
    GROUP BY [ServiceName];
GO

-- ============================================================================
-- 9. STORED PROCEDURE: Calculate Streaks
-- ============================================================================
CREATE OR ALTER PROCEDURE [habit].[sp_GetCurrentStreak]
    @HabitId INT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH DateSequence AS (
        SELECT
            [CompletedDate],
            DATEDIFF(DAY, [CompletedDate], CAST(GETUTCDATE() AS DATE)) AS DaysAgo,
            ROW_NUMBER() OVER (ORDER BY [CompletedDate] DESC) AS RowNum
        FROM [habit].[HabitCompletions]
        WHERE [HabitId] = @HabitId
    ),
    StreakCalc AS (
        SELECT
            [CompletedDate],
            DaysAgo,
            RowNum,
            DaysAgo - RowNum AS GroupId  -- consecutive days have same GroupId
        FROM DateSequence
    )
    SELECT
        COUNT(*) AS [CurrentStreak],
        MIN([CompletedDate]) AS [StreakStart],
        MAX([CompletedDate]) AS [StreakEnd]
    FROM StreakCalc
    WHERE GroupId = (
        SELECT TOP 1 GroupId
        FROM StreakCalc
        WHERE DaysAgo <= 1  -- streak must include today or yesterday
        ORDER BY RowNum
    );
END
GO

PRINT '========================================';
PRINT ' HabitTracker database setup complete!';
PRINT '========================================';
GO
