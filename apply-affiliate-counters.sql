BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723135300_AddSignupPendingCountersAndEventLog'
)
BEGIN
    ALTER TABLE [AffiliateRequests] ADD [PendingCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723135300_AddSignupPendingCountersAndEventLog'
)
BEGIN
    ALTER TABLE [AffiliateRequests] ADD [SignupCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723135300_AddSignupPendingCountersAndEventLog'
)
BEGIN
    CREATE TABLE [AffiliateEventLogs] (
        [Id] int NOT NULL IDENTITY,
        [AffiliateRequestId] int NOT NULL,
        [Event] nvarchar(32) NOT NULL,
        [ExternalKey] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AffiliateEventLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AffiliateEventLogs_AffiliateRequests_AffiliateRequestId] FOREIGN KEY ([AffiliateRequestId]) REFERENCES [AffiliateRequests] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723135300_AddSignupPendingCountersAndEventLog'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AffiliateEventLogs_AffiliateRequestId_Event_ExternalKey] ON [AffiliateEventLogs] ([AffiliateRequestId], [Event], [ExternalKey]) WHERE [ExternalKey] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723135300_AddSignupPendingCountersAndEventLog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723135300_AddSignupPendingCountersAndEventLog', N'8.0.29');
END;
GO

COMMIT;
GO

