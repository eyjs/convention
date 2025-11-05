IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ActionTemplates] (
    [Id] int NOT NULL IDENTITY,
    [TemplateType] nvarchar(max) NOT NULL,
    [TemplateName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Category] nvarchar(max) NOT NULL,
    [IconClass] nvarchar(max) NULL,
    [DefaultRoute] nvarchar(max) NOT NULL,
    [DefaultConfigJson] nvarchar(max) NULL,
    [RequiredFields] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [OrderNum] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ActionTemplates] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Conventions] (
    [Id] int NOT NULL IDENTITY,
    [MemberId] nvarchar(max) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [ConventionType] nvarchar(450) NOT NULL DEFAULT N'DOMESTIC',
    [RenderType] nvarchar(max) NOT NULL DEFAULT N'STANDARD',
    [StartDate] datetime2 NULL,
    [EndDate] datetime2 NULL,
    [ConventionImg] nvarchar(max) NULL,
    [BrandColor] nvarchar(max) NULL,
    [ThemePreset] nvarchar(max) NULL,
    [RegDtm] datetime2 NOT NULL DEFAULT (getdate()),
    [DeleteYn] nvarchar(max) NOT NULL DEFAULT N'N',
    [CompleteYn] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Conventions] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [LlmSettings] (
    [Id] int NOT NULL IDENTITY,
    [ProviderName] nvarchar(50) NOT NULL,
    [ApiKey] nvarchar(500) NULL,
    [BaseUrl] nvarchar(200) NULL,
    [ModelName] nvarchar(100) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [AdditionalSettings] nvarchar(max) NULL,
    CONSTRAINT [PK_LlmSettings] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [LoginId] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(256) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NULL,
    [Phone] nvarchar(20) NULL,
    [Role] nvarchar(20) NOT NULL DEFAULT N'Guest',
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [EmailVerified] bit NOT NULL,
    [PhoneVerified] bit NOT NULL,
    [RefreshToken] nvarchar(256) NULL,
    [RefreshTokenExpiresAt] datetime2 NULL,
    [LastLoginAt] datetime2 NULL,
    [ProfileImageUrl] nvarchar(512) NULL,
    [CorpName] nvarchar(100) NULL,
    [CorpPart] nvarchar(100) NULL,
    [ResidentNumber] nvarchar(50) NULL,
    [Affiliation] nvarchar(100) NULL,
    [EnglishName] nvarchar(100) NULL,
    [PassportNumber] nvarchar(50) NULL,
    [PassportExpiryDate] date NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [VectorDataEntries] (
    [Id] nvarchar(36) NOT NULL,
    [ConventionId] int NOT NULL,
    [SourceType] nvarchar(50) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [EmbeddingData] varbinary(max) NOT NULL,
    [MetadataJson] nvarchar(2048) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_VectorDataEntries] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AttributeDefinitions] (
    [Id] int NOT NULL IDENTITY,
    [ConventionId] int NOT NULL,
    [AttributeKey] nvarchar(450) NOT NULL,
    [Options] nvarchar(max) NULL,
    [OrderNum] int NOT NULL,
    [IsRequired] bit NOT NULL,
    CONSTRAINT [PK_AttributeDefinitions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AttributeDefinitions_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AttributeTemplates] (
    [Id] int NOT NULL IDENTITY,
    [ConventionId] int NOT NULL,
    [AttributeKey] nvarchar(450) NOT NULL,
    [AttributeValues] nvarchar(max) NULL,
    [OrderNum] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AttributeTemplates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AttributeTemplates_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ConventionActions] (
    [Id] int NOT NULL IDENTITY,
    [ConventionId] int NOT NULL,
    [ActionType] nvarchar(100) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(4000) NULL,
    [Deadline] datetime2 NULL,
    [MapsTo] nvarchar(200) NOT NULL,
    [ConfigJson] nvarchar(4000) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [OrderNum] int NOT NULL DEFAULT 0,
    [TemplateId] int NULL,
    [IsRequired] bit NOT NULL,
    [IconClass] nvarchar(max) NULL,
    [Category] nvarchar(max) NULL,
    [ActionCategory] nvarchar(50) NULL,
    [TargetLocation] nvarchar(100) NULL,
    CONSTRAINT [PK_ConventionActions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConventionActions_ActionTemplates_TemplateId] FOREIGN KEY ([TemplateId]) REFERENCES [ActionTemplates] ([Id]),
    CONSTRAINT [FK_ConventionActions_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Features] (
    [Id] int NOT NULL IDENTITY,
    [ConventionId] int NOT NULL,
    [MenuName] nvarchar(100) NOT NULL,
    [MenuUrl] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL,
    [IconUrl] nvarchar(500) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Features] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Features_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Menus] (
    [Id] int NOT NULL IDENTITY,
    [ConventionId] int NOT NULL,
    [ItemName] nvarchar(max) NULL,
    [RegDtm] datetime2 NOT NULL,
    [DeleteYn] nvarchar(max) NOT NULL,
    [OrderNum] int NOT NULL,
    CONSTRAINT [PK_Menus] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Menus_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [NoticeCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [DisplayOrder] int NOT NULL DEFAULT 0,
    [ConventionId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_NoticeCategories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_NoticeCategories_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Owners] (
    [Id] int NOT NULL IDENTITY,
    [ConventionId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Telephone] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Owners] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Owners_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Schedules] (
    [Id] int NOT NULL IDENTITY,
    [ConventionId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [ScheduleDate] datetime2 NOT NULL,
    [StartTime] time NULL,
    [EndTime] time NULL,
    [Group] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [OrderNum] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Schedules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Schedules_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ScheduleTemplates] (
    [Id] int NOT NULL IDENTITY,
    [ConventionId] int NOT NULL,
    [CourseName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [OrderNum] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ScheduleTemplates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ScheduleTemplates_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ConventionChatMessages] (
    [Id] int NOT NULL IDENTITY,
    [ConventionId] int NOT NULL,
    [UserId] int NOT NULL,
    [UserName] nvarchar(100) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [IsAdmin] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_ConventionChatMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConventionChatMessages_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ConventionChatMessages_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [Galleries] (
    [Id] int NOT NULL IDENTITY,
    [ConventionId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [AuthorId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Galleries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Galleries_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Galleries_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [GuestAttributes] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [AttributeKey] nvarchar(450) NOT NULL,
    [AttributeValue] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_GuestAttributes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GuestAttributes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserConventions] (
    [UserId] int NOT NULL,
    [ConventionId] int NOT NULL,
    [GroupName] nvarchar(100) NULL,
    [AccessToken] nvarchar(64) NULL,
    [LastChatReadTimestamp] datetime2 NULL,
    [VisaDocumentAttachmentId] int NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_UserConventions] PRIMARY KEY ([UserId], [ConventionId]),
    CONSTRAINT [FK_UserConventions_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserConventions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [SurveyResponses] (
    [Id] int NOT NULL IDENTITY,
    [ConventionActionId] int NOT NULL,
    [UserId] int NOT NULL,
    [SubmittedAt] datetime2 NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_SurveyResponses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SurveyResponses_ConventionActions_ConventionActionId] FOREIGN KEY ([ConventionActionId]) REFERENCES [ConventionActions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SurveyResponses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [UserActionStatuses] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ConventionActionId] int NOT NULL,
    [IsComplete] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CompletedAt] datetime2 NULL,
    [ResponseDataJson] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [UpdatedAt] datetime2 NULL DEFAULT (getdate()),
    CONSTRAINT [PK_UserActionStatuses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserActionStatuses_ConventionActions_ConventionActionId] FOREIGN KEY ([ConventionActionId]) REFERENCES [ConventionActions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserActionStatuses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [Sections] (
    [Id] int NOT NULL IDENTITY,
    [MenuId] int NOT NULL,
    [Title] nvarchar(max) NULL,
    [Contents] nvarchar(max) NULL,
    [OrderNum] int NOT NULL,
    [RegDtm] datetime2 NOT NULL,
    [DeleteYn] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Sections] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Sections_Menus_MenuId] FOREIGN KEY ([MenuId]) REFERENCES [Menus] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Notices] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [IsPinned] bit NOT NULL DEFAULT CAST(0 AS bit),
    [ViewCount] int NOT NULL DEFAULT 0,
    [AuthorId] int NOT NULL,
    [ConventionId] int NOT NULL,
    [NoticeCategoryId] int NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Notices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notices_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Notices_NoticeCategories_NoticeCategoryId] FOREIGN KEY ([NoticeCategoryId]) REFERENCES [NoticeCategories] ([Id]),
    CONSTRAINT [FK_Notices_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [GuestScheduleTemplates] (
    [UserId] int NOT NULL,
    [ScheduleTemplateId] int NOT NULL,
    [AssignedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_GuestScheduleTemplates] PRIMARY KEY ([UserId], [ScheduleTemplateId]),
    CONSTRAINT [FK_GuestScheduleTemplates_ScheduleTemplates_ScheduleTemplateId] FOREIGN KEY ([ScheduleTemplateId]) REFERENCES [ScheduleTemplates] ([Id]),
    CONSTRAINT [FK_GuestScheduleTemplates_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ScheduleItems] (
    [Id] int NOT NULL IDENTITY,
    [ScheduleTemplateId] int NOT NULL,
    [ScheduleDate] datetime2 NOT NULL,
    [StartTime] nvarchar(max) NOT NULL,
    [EndTime] nvarchar(max) NULL,
    [Title] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NULL,
    [Location] nvarchar(max) NULL,
    [OrderNum] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ScheduleItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ScheduleItems_ScheduleTemplates_ScheduleTemplateId] FOREIGN KEY ([ScheduleTemplateId]) REFERENCES [ScheduleTemplates] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [GalleryImages] (
    [Id] int NOT NULL IDENTITY,
    [GalleryId] int NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    [Caption] nvarchar(max) NULL,
    [OrderNum] int NOT NULL,
    [UploadedAt] datetime2 NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_GalleryImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GalleryImages_Galleries_GalleryId] FOREIGN KEY ([GalleryId]) REFERENCES [Galleries] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [SurveyResponseAnswers] (
    [Id] int NOT NULL IDENTITY,
    [SurveyResponseId] int NOT NULL,
    [Question] nvarchar(max) NOT NULL,
    [Answer] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_SurveyResponseAnswers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SurveyResponseAnswers_SurveyResponses_SurveyResponseId] FOREIGN KEY ([SurveyResponseId]) REFERENCES [SurveyResponses] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Comments] (
    [Id] int NOT NULL IDENTITY,
    [NoticeId] int NOT NULL,
    [AuthorId] int NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getdate()),
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Comments_Notices_NoticeId] FOREIGN KEY ([NoticeId]) REFERENCES [Notices] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Comments_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [FileAttachments] (
    [Id] int NOT NULL IDENTITY,
    [OriginalName] nvarchar(max) NOT NULL,
    [SavedName] nvarchar(max) NOT NULL,
    [FilePath] nvarchar(max) NOT NULL,
    [Size] bigint NOT NULL,
    [ContentType] nvarchar(max) NOT NULL,
    [Category] nvarchar(450) NOT NULL,
    [NoticeId] int NULL,
    [BoardPostId] int NULL,
    [UploadedAt] datetime2 NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_FileAttachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FileAttachments_Notices_NoticeId] FOREIGN KEY ([NoticeId]) REFERENCES [Notices] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AttributeDefinition_ConventionId] ON [AttributeDefinitions] ([ConventionId]);
GO

CREATE UNIQUE INDEX [UQ_AttributeDefinition_ConventionId_AttributeKey] ON [AttributeDefinitions] ([ConventionId], [AttributeKey]);
GO

CREATE INDEX [IX_AttributeTemplate_ConventionId] ON [AttributeTemplates] ([ConventionId]);
GO

CREATE UNIQUE INDEX [UQ_AttributeTemplate_ConventionId_AttributeKey] ON [AttributeTemplates] ([ConventionId], [AttributeKey]);
GO

CREATE INDEX [IX_Comments_AuthorId] ON [Comments] ([AuthorId]);
GO

CREATE INDEX [IX_Comments_NoticeId] ON [Comments] ([NoticeId]);
GO

CREATE INDEX [IX_ConventionAction_ActionType] ON [ConventionActions] ([ActionType]);
GO

CREATE INDEX [IX_ConventionAction_ConventionId] ON [ConventionActions] ([ConventionId]);
GO

CREATE INDEX [IX_ConventionActions_TemplateId] ON [ConventionActions] ([TemplateId]);
GO

CREATE UNIQUE INDEX [UQ_ConventionAction_ConventionId_ActionType] ON [ConventionActions] ([ConventionId], [ActionType]);
GO

CREATE INDEX [IX_ChatMessage_ConventionId] ON [ConventionChatMessages] ([ConventionId]);
GO

CREATE INDEX [IX_ChatMessage_CreatedAt] ON [ConventionChatMessages] ([CreatedAt]);
GO

CREATE INDEX [IX_ChatMessage_UserId] ON [ConventionChatMessages] ([UserId]);
GO

CREATE INDEX [IX_Convention_ConventionType] ON [Conventions] ([ConventionType]);
GO

CREATE INDEX [IX_Convention_StartDate] ON [Conventions] ([StartDate]);
GO

CREATE INDEX [IX_Feature_ConventionId] ON [Features] ([ConventionId]);
GO

CREATE INDEX [IX_FileAttachment_Category] ON [FileAttachments] ([Category]);
GO

CREATE INDEX [IX_FileAttachment_NoticeId] ON [FileAttachments] ([NoticeId]);
GO

CREATE INDEX [IX_Galleries_AuthorId] ON [Galleries] ([AuthorId]);
GO

CREATE INDEX [IX_Gallery_ConventionId] ON [Galleries] ([ConventionId]);
GO

CREATE INDEX [IX_Gallery_CreatedAt] ON [Galleries] ([CreatedAt]);
GO

CREATE INDEX [IX_GalleryImage_GalleryId] ON [GalleryImages] ([GalleryId]);
GO

CREATE UNIQUE INDEX [UQ_GuestAttributes_UserId_AttributeKey] ON [GuestAttributes] ([UserId], [AttributeKey]);
GO

CREATE INDEX [IX_GuestScheduleTemplates_ScheduleTemplateId] ON [GuestScheduleTemplates] ([ScheduleTemplateId]);
GO

CREATE INDEX [IX_Menu_ConventionId] ON [Menus] ([ConventionId]);
GO

CREATE INDEX [IX_NoticeCategory_ConventionId] ON [NoticeCategories] ([ConventionId]);
GO

CREATE INDEX [IX_NoticeCategory_ConventionId_Name] ON [NoticeCategories] ([ConventionId], [Name]);
GO

CREATE INDEX [IX_Notice_AuthorId] ON [Notices] ([AuthorId]);
GO

CREATE INDEX [IX_Notice_ConventionId] ON [Notices] ([ConventionId]);
GO

CREATE INDEX [IX_Notice_CreatedAt] ON [Notices] ([CreatedAt]);
GO

CREATE INDEX [IX_Notice_IsPinned] ON [Notices] ([IsPinned]);
GO

CREATE INDEX [IX_Notice_NoticeCategoryId] ON [Notices] ([NoticeCategoryId]);
GO

CREATE INDEX [IX_Owner_ConventionId] ON [Owners] ([ConventionId]);
GO

CREATE INDEX [IX_ScheduleItem_ScheduleTemplateId] ON [ScheduleItems] ([ScheduleTemplateId]);
GO

CREATE INDEX [IX_Schedule_ConventionId] ON [Schedules] ([ConventionId]);
GO

CREATE INDEX [IX_ScheduleTemplate_ConventionId] ON [ScheduleTemplates] ([ConventionId]);
GO

CREATE INDEX [IX_Section_MenuId] ON [Sections] ([MenuId]);
GO

CREATE INDEX [IX_SurveyResponseAnswers_SurveyResponseId] ON [SurveyResponseAnswers] ([SurveyResponseId]);
GO

CREATE INDEX [IX_SurveyResponse_ConventionActionId] ON [SurveyResponses] ([ConventionActionId]);
GO

CREATE INDEX [IX_SurveyResponse_UserId] ON [SurveyResponses] ([UserId]);
GO

CREATE INDEX [IX_GuestActionStatus_ConventionActionId] ON [UserActionStatuses] ([ConventionActionId]);
GO

CREATE INDEX [IX_GuestActionStatus_UserId] ON [UserActionStatuses] ([UserId]);
GO

CREATE UNIQUE INDEX [UQ_GuestActionStatus_UserId_ConventionActionId] ON [UserActionStatuses] ([UserId], [ConventionActionId]);
GO

CREATE INDEX [IX_UserConvention_ConventionId] ON [UserConventions] ([ConventionId]);
GO

CREATE INDEX [IX_UserConvention_UserId] ON [UserConventions] ([UserId]);
GO

CREATE UNIQUE INDEX [UQ_UserConvention_AccessToken] ON [UserConventions] ([AccessToken]) WHERE [AccessToken] IS NOT NULL;
GO

CREATE UNIQUE INDEX [UQ_UserConvention_UserId_ConventionId] ON [UserConventions] ([UserId], [ConventionId]);
GO

CREATE INDEX [IX_User_Email] ON [Users] ([Email]);
GO

CREATE INDEX [IX_User_Name] ON [Users] ([Name]);
GO

CREATE INDEX [IX_User_Phone] ON [Users] ([Phone]);
GO

CREATE INDEX [IX_User_Role] ON [Users] ([Role]);
GO

CREATE UNIQUE INDEX [UQ_User_LoginId] ON [Users] ([LoginId]);
GO

CREATE INDEX [IX_VectorDataEntries_ConventionId] ON [VectorDataEntries] ([ConventionId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251103020236_Initial', N'8.0.8');
GO

COMMIT;
GO

