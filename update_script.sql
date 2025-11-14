BEGIN TRANSACTION;
GO

ALTER TABLE [SurveyResponses] DROP CONSTRAINT [FK_SurveyResponses_ConventionActions_ConventionActionId];
GO

DROP TABLE [SurveyResponseAnswers];
GO

EXEC sp_rename N'[SurveyResponses].[ConventionActionId]', N'SurveyId', N'COLUMN';
GO

EXEC sp_rename N'[SurveyResponses].[IX_SurveyResponse_UserId]', N'IX_SurveyResponses_UserId', N'INDEX';
GO

EXEC sp_rename N'[SurveyResponses].[IX_SurveyResponse_ConventionActionId]', N'IX_SurveyResponses_SurveyId', N'INDEX';
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SurveyResponses]') AND [c].[name] = N'SubmittedAt');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [SurveyResponses] DROP CONSTRAINT [' + @var0 + '];');
GO

CREATE TABLE [Surveys] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(255) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ConventionId] int NULL,
    CONSTRAINT [PK_Surveys] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Surveys_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id])
);
GO

CREATE TABLE [SurveyQuestions] (
    [Id] int NOT NULL IDENTITY,
    [SurveyId] int NOT NULL,
    [QuestionText] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [IsRequired] bit NOT NULL,
    [OrderIndex] int NOT NULL,
    CONSTRAINT [PK_SurveyQuestions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SurveyQuestions_Surveys_SurveyId] FOREIGN KEY ([SurveyId]) REFERENCES [Surveys] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [QuestionOptions] (
    [Id] int NOT NULL IDENTITY,
    [QuestionId] int NOT NULL,
    [OptionText] nvarchar(max) NOT NULL,
    [OrderIndex] int NOT NULL,
    CONSTRAINT [PK_QuestionOptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuestionOptions_SurveyQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [SurveyQuestions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ResponseDetails] (
    [Id] int NOT NULL IDENTITY,
    [ResponseId] int NOT NULL,
    [QuestionId] int NOT NULL,
    [AnswerText] nvarchar(max) NULL,
    [SelectedOptionId] int NULL,
    CONSTRAINT [PK_ResponseDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ResponseDetails_QuestionOptions_SelectedOptionId] FOREIGN KEY ([SelectedOptionId]) REFERENCES [QuestionOptions] ([Id]),
    CONSTRAINT [FK_ResponseDetails_SurveyQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [SurveyQuestions] ([Id]),
    CONSTRAINT [FK_ResponseDetails_SurveyResponses_ResponseId] FOREIGN KEY ([ResponseId]) REFERENCES [SurveyResponses] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_QuestionOptions_QuestionId] ON [QuestionOptions] ([QuestionId]);
GO

CREATE INDEX [IX_ResponseDetails_QuestionId] ON [ResponseDetails] ([QuestionId]);
GO

CREATE INDEX [IX_ResponseDetails_ResponseId] ON [ResponseDetails] ([ResponseId]);
GO

CREATE INDEX [IX_ResponseDetails_SelectedOptionId] ON [ResponseDetails] ([SelectedOptionId]);
GO

CREATE INDEX [IX_SurveyQuestions_SurveyId] ON [SurveyQuestions] ([SurveyId]);
GO

CREATE INDEX [IX_Surveys_ConventionId] ON [Surveys] ([ConventionId]);
GO

ALTER TABLE [SurveyResponses] ADD CONSTRAINT [FK_SurveyResponses_Surveys_SurveyId] FOREIGN KEY ([SurveyId]) REFERENCES [Surveys] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251106073407_AddSurveyModule', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ConventionActions] ADD [BehaviorType] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [ConventionActions] ADD [TargetModuleId] int NULL;
GO

CREATE TABLE [ActionSubmissions] (
    [Id] int NOT NULL IDENTITY,
    [ConventionActionId] int NOT NULL,
    [UserId] int NOT NULL,
    [SubmissionDataJson] nvarchar(max) NOT NULL,
    [SubmittedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ActionSubmissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ActionSubmissions_ConventionActions_ConventionActionId] FOREIGN KEY ([ConventionActionId]) REFERENCES [ConventionActions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ActionSubmissions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);
GO

CREATE INDEX [IX_ActionSubmission_ConventionActionId] ON [ActionSubmissions] ([ConventionActionId]);
GO

CREATE INDEX [IX_ActionSubmission_UserId] ON [ActionSubmissions] ([UserId]);
GO

CREATE UNIQUE INDEX [UQ_ActionSubmission_UserId_ConventionActionId] ON [ActionSubmissions] ([UserId], [ConventionActionId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107033942_AddActionBehaviorTypeAndSubmissions', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107052029_RemoveActionTypeRequired', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [ResponseDetails];
GO

DROP INDEX [IX_ConventionAction_ActionType] ON [ConventionActions];
GO

DROP INDEX [UQ_ConventionAction_ConventionId_ActionType] ON [ConventionActions];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConventionActions]') AND [c].[name] = N'ActionType');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ConventionActions] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [ConventionActions] DROP COLUMN [ActionType];
GO

CREATE TABLE [SurveyResponseDetails] (
    [Id] int NOT NULL IDENTITY,
    [ResponseId] int NOT NULL,
    [QuestionId] int NOT NULL,
    [AnswerText] nvarchar(max) NULL,
    [SelectedOptionId] int NULL,
    CONSTRAINT [PK_SurveyResponseDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SurveyResponseDetails_QuestionOptions_SelectedOptionId] FOREIGN KEY ([SelectedOptionId]) REFERENCES [QuestionOptions] ([Id]),
    CONSTRAINT [FK_SurveyResponseDetails_SurveyQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [SurveyQuestions] ([Id]),
    CONSTRAINT [FK_SurveyResponseDetails_SurveyResponses_ResponseId] FOREIGN KEY ([ResponseId]) REFERENCES [SurveyResponses] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_SurveyResponseDetails_QuestionId] ON [SurveyResponseDetails] ([QuestionId]);
GO

CREATE INDEX [IX_SurveyResponseDetails_ResponseId] ON [SurveyResponseDetails] ([ResponseId]);
GO

CREATE INDEX [IX_SurveyResponseDetails_SelectedOptionId] ON [SurveyResponseDetails] ([SelectedOptionId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107063418_RenameResponseDetailToSurveyResponseDetail', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ConventionActions] ADD [FrontendRoute] nvarchar(max) NULL;
GO

ALTER TABLE [ConventionActions] ADD [ModuleIdentifier] nvarchar(max) NULL;
GO

ALTER TABLE [ConventionActions] ADD [TargetId] int NULL;
GO

CREATE TABLE [FormDefinitions] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [ConventionId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_FormDefinitions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FormDefinitions_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [FormFields] (
    [Id] int NOT NULL IDENTITY,
    [FormDefinitionId] int NOT NULL,
    [Key] nvarchar(max) NOT NULL,
    [Label] nvarchar(max) NOT NULL,
    [FieldType] nvarchar(max) NOT NULL,
    [OrderIndex] int NOT NULL,
    [IsRequired] bit NOT NULL,
    [Placeholder] nvarchar(max) NULL,
    [OptionsJson] nvarchar(max) NULL,
    [ValidationRules] nvarchar(max) NULL,
    CONSTRAINT [PK_FormFields] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FormFields_FormDefinitions_FormDefinitionId] FOREIGN KEY ([FormDefinitionId]) REFERENCES [FormDefinitions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [FormSubmissions] (
    [Id] int NOT NULL IDENTITY,
    [FormDefinitionId] int NOT NULL,
    [UserId] int NOT NULL,
    [SubmissionDataJson] nvarchar(max) NOT NULL,
    [SubmittedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_FormSubmissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FormSubmissions_FormDefinitions_FormDefinitionId] FOREIGN KEY ([FormDefinitionId]) REFERENCES [FormDefinitions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_FormSubmissions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_FormDefinitions_ConventionId] ON [FormDefinitions] ([ConventionId]);
GO

CREATE INDEX [IX_FormFields_FormDefinitionId] ON [FormFields] ([FormDefinitionId]);
GO

CREATE INDEX [IX_FormSubmissions_FormDefinitionId] ON [FormSubmissions] ([FormDefinitionId]);
GO

CREATE INDEX [IX_FormSubmissions_UserId] ON [FormSubmissions] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251110022001_RefactorActionSystemToFormBuilder', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251110071259_AddTargetIdToConventionActions', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251110072549_FinalActionModelRefactor', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [FirstName] nvarchar(50) NULL;
GO

ALTER TABLE [Users] ADD [LastName] nvarchar(50) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251111033030_AddFirstNameLastNameToUser', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [PersonalTrips] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    [Destination] nvarchar(100) NULL,
    [City] nvarchar(100) NULL,
    [UserId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_PersonalTrips] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PersonalTrips_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Accommodations] (
    [Id] int NOT NULL IDENTITY,
    [PersonalTripId] int NOT NULL,
    [Name] nvarchar(200) NULL,
    [Type] nvarchar(50) NULL,
    [Address] nvarchar(500) NULL,
    [CheckInTime] datetime2 NULL,
    [CheckOutTime] datetime2 NULL,
    [BookingReference] nvarchar(100) NULL,
    [ContactNumber] nvarchar(50) NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Accommodations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Accommodations_PersonalTrips_PersonalTripId] FOREIGN KEY ([PersonalTripId]) REFERENCES [PersonalTrips] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Flights] (
    [Id] int NOT NULL IDENTITY,
    [PersonalTripId] int NOT NULL,
    [Airline] nvarchar(100) NULL,
    [FlightNumber] nvarchar(50) NULL,
    [DepartureLocation] nvarchar(200) NULL,
    [ArrivalLocation] nvarchar(200) NULL,
    [DepartureTime] datetime2 NULL,
    [ArrivalTime] datetime2 NULL,
    [BookingReference] nvarchar(100) NULL,
    [SeatNumber] nvarchar(20) NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Flights] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Flights_PersonalTrips_PersonalTripId] FOREIGN KEY ([PersonalTripId]) REFERENCES [PersonalTrips] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Accommodation_CheckInTime] ON [Accommodations] ([CheckInTime]);
GO

CREATE INDEX [IX_Accommodation_PersonalTripId] ON [Accommodations] ([PersonalTripId]);
GO

CREATE INDEX [IX_Flight_DepartureTime] ON [Flights] ([DepartureTime]);
GO

CREATE INDEX [IX_Flight_PersonalTripId] ON [Flights] ([PersonalTripId]);
GO

CREATE INDEX [IX_PersonalTrip_StartDate] ON [PersonalTrips] ([StartDate]);
GO

CREATE INDEX [IX_PersonalTrip_UserId] ON [PersonalTrips] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251111062802_AddPersonalTripSystem', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [PersonalTrips] ADD [CountryCode] nvarchar(2) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251111082430_AddCountryCodeToPersonalTrip', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ItineraryItems] (
    [Id] int NOT NULL IDENTITY,
    [DayNumber] int NOT NULL,
    [LocationName] nvarchar(200) NOT NULL,
    [Address] nvarchar(500) NULL,
    [Latitude] float NULL,
    [Longitude] float NULL,
    [GooglePlaceId] nvarchar(300) NULL,
    [StartTime] time NULL,
    [EndTime] time NULL,
    [PersonalTripId] int NOT NULL,
    CONSTRAINT [PK_ItineraryItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ItineraryItems_PersonalTrips_PersonalTripId] FOREIGN KEY ([PersonalTripId]) REFERENCES [PersonalTrips] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ItineraryItem_DayNumber] ON [ItineraryItems] ([DayNumber]);
GO

CREATE INDEX [IX_ItineraryItem_PersonalTripId] ON [ItineraryItems] ([PersonalTripId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251113010153_AddItineraryItemEntity', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [PersonalTrips] ADD [Latitude] float NULL;
GO

ALTER TABLE [PersonalTrips] ADD [Longitude] float NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251113012331_AddCoordsToPersonalTrip', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Accommodations] ADD [PostalCode] nvarchar(20) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251113031441_AddPostalCodeToAccommodation', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Accommodations] ADD [Latitude] float NULL;
GO

ALTER TABLE [Accommodations] ADD [Longitude] float NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251113054433_AddCoordsToAccommodation', N'8.0.8');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251114012202_BaselineExistingDb', N'8.0.8');
GO

COMMIT;
GO

