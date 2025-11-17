BEGIN TRANSACTION;
GO

ALTER TABLE [PersonalTrips] ADD [CoverImageUrl] nvarchar(500) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251114084744_AddCoverImageUrlToPersonalTrip', N'8.0.8');
GO

COMMIT;
GO

