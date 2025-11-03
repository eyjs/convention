-- =====================================================
-- Data Migration Script: Guest → User + UserConvention
-- =====================================================
-- 이 스크립트는 Guest 엔티티를 User + UserConvention으로 분리하는
-- 데이터 마이그레이션을 수행합니다.
--
-- 실행 전 주의사항:
-- 1. 반드시 데이터베이스 백업을 먼저 수행하세요
-- 2. 개발/테스트 환경에서 먼저 검증하세요
-- 3. 마이그레이션 중 애플리케이션을 중단하세요
--
-- 실행 순서:
-- 1. 백업 수행
-- 2. 이 스크립트 실행
-- 3. Entity Framework migration 실행 (dotnet ef database update)
-- =====================================================

BEGIN TRANSACTION;

PRINT '========================================';
PRINT 'Guest → User + UserConvention Migration';
PRINT '========================================';
PRINT '';

-- =====================================================
-- STEP 1: User 테이블에 Guest 속성 컬럼 추가 (이미 존재하지 않는 경우)
-- =====================================================
PRINT 'STEP 1: Adding Guest-specific columns to Users table...';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'CorpName')
BEGIN
    ALTER TABLE [Users] ADD [CorpName] NVARCHAR(200) NULL;
    PRINT '  - Added CorpName column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'CorpPart')
BEGIN
    ALTER TABLE [Users] ADD [CorpPart] NVARCHAR(200) NULL;
    PRINT '  - Added CorpPart column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'ResidentNumber')
BEGIN
    ALTER TABLE [Users] ADD [ResidentNumber] NVARCHAR(20) NULL;
    PRINT '  - Added ResidentNumber column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Affiliation')
BEGIN
    ALTER TABLE [Users] ADD [Affiliation] NVARCHAR(200) NULL;
    PRINT '  - Added Affiliation column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'EnglishName')
BEGIN
    ALTER TABLE [Users] ADD [EnglishName] NVARCHAR(200) NULL;
    PRINT '  - Added EnglishName column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PassportNumber')
BEGIN
    ALTER TABLE [Users] ADD [PassportNumber] NVARCHAR(50) NULL;
    PRINT '  - Added PassportNumber column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PassportExpiryDate')
BEGIN
    ALTER TABLE [Users] ADD [PassportExpiryDate] DATETIME2 NULL;
    PRINT '  - Added PassportExpiryDate column';
END

PRINT 'STEP 1: Completed';
PRINT '';

-- =====================================================
-- STEP 2: UserConventions 테이블 생성 (이미 존재하지 않는 경우)
-- =====================================================
PRINT 'STEP 2: Creating UserConventions table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserConventions')
BEGIN
    CREATE TABLE [UserConventions] (
        [UserId] INT NOT NULL,
        [ConventionId] INT NOT NULL,
        [GroupName] NVARCHAR(100) NULL,
        [AccessToken] NVARCHAR(64) NULL,
        [LastChatReadTimestamp] DATETIME2 NULL,
        [VisaDocumentAttachmentId] INT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_UserConventions] PRIMARY KEY ([UserId], [ConventionId]),
        CONSTRAINT [FK_UserConventions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserConventions_Conventions_ConventionId] FOREIGN KEY ([ConventionId]) REFERENCES [Conventions]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_UserConvention_ConventionId] ON [UserConventions]([ConventionId]);
    CREATE INDEX [IX_UserConvention_AccessToken] ON [UserConventions]([AccessToken]);
    CREATE INDEX [IX_UserConvention_GroupName] ON [UserConventions]([GroupName]);

    PRINT '  - Created UserConventions table';
END
ELSE
BEGIN
    PRINT '  - UserConventions table already exists';
END

PRINT 'STEP 2: Completed';
PRINT '';

-- =====================================================
-- STEP 3: Guests 테이블이 존재하는 경우 데이터 마이그레이션
-- =====================================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Guests')
BEGIN
    PRINT 'STEP 3: Migrating Guest data...';

    DECLARE @guestCount INT;
    SELECT @guestCount = COUNT(*) FROM [Guests];
    PRINT '  - Found ' + CAST(@guestCount AS NVARCHAR) + ' guests to migrate';

    -- =====================================================
    -- STEP 3.1: Guest → User 마이그레이션
    -- =====================================================
    PRINT 'STEP 3.1: Migrating Guests to Users...';

    -- 전화번호로 기존 User와 매칭, 없으면 새로 생성
    MERGE [Users] AS target
    USING (
        SELECT
            g.[Id] AS GuestId,
            g.[ConventionId],
            g.[Name],
            g.[Phone],
            g.[CorpName],
            g.[CorpPart],
            g.[ResidentNumber],
            g.[Affiliation],
            g.[EnglishName],
            g.[PassportNumber],
            g.[PassportExpiryDate],
            g.[GroupName],
            g.[AccessToken],
            g.[LastChatReadTimestamp],
            g.[VisaDocumentAttachmentId],
            g.[CreatedAt]
        FROM [Guests] g
    ) AS source
    ON (target.[Phone] = source.[Phone])
    WHEN MATCHED THEN
        UPDATE SET
            target.[Name] = source.[Name],
            target.[CorpName] = source.[CorpName],
            target.[CorpPart] = source.[CorpPart],
            target.[ResidentNumber] = source.[ResidentNumber],
            target.[Affiliation] = source.[Affiliation],
            target.[EnglishName] = source.[EnglishName],
            target.[PassportNumber] = source.[PassportNumber],
            target.[PassportExpiryDate] = source.[PassportExpiryDate],
            target.[UpdatedAt] = GETDATE()
    WHEN NOT MATCHED THEN
        INSERT ([LoginId], [PasswordHash], [Name], [Phone], [CorpName], [CorpPart], [ResidentNumber], [Affiliation], [EnglishName], [PassportNumber], [PassportExpiryDate], [CreatedAt], [UpdatedAt], [IsDeleted])
        VALUES ('', '', source.[Name], source.[Phone], source.[CorpName], source.[CorpPart], source.[ResidentNumber], source.[Affiliation], source.[EnglishName], source.[PassportNumber], source.[PassportExpiryDate], source.[CreatedAt], GETDATE(), 0);

    PRINT '  - Merged Guest data into Users table';

    -- =====================================================
    -- STEP 3.2: Guest → UserConvention 마이그레이션
    -- =====================================================
    PRINT 'STEP 3.2: Creating UserConvention records...';

    INSERT INTO [UserConventions] ([UserId], [ConventionId], [GroupName], [AccessToken], [LastChatReadTimestamp], [VisaDocumentAttachmentId], [CreatedAt])
    SELECT
        u.[Id] AS UserId,
        g.[ConventionId],
        g.[GroupName],
        g.[AccessToken],
        g.[LastChatReadTimestamp],
        g.[VisaDocumentAttachmentId],
        g.[CreatedAt]
    FROM [Guests] g
    INNER JOIN [Users] u ON u.[Phone] = g.[Phone]
    WHERE NOT EXISTS (
        SELECT 1 FROM [UserConventions] uc
        WHERE uc.[UserId] = u.[Id] AND uc.[ConventionId] = g.[ConventionId]
    );

    DECLARE @userConventionCount INT;
    SELECT @userConventionCount = @@ROWCOUNT;
    PRINT '  - Created ' + CAST(@userConventionCount AS NVARCHAR) + ' UserConvention records';

    -- =====================================================
    -- STEP 3.3: 관련 테이블의 GuestId → UserId 업데이트
    -- =====================================================
    PRINT 'STEP 3.3: Updating related tables...';

    -- GuestAttributes 테이블 업데이트
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'GuestAttributes')
    BEGIN
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GuestAttributes]') AND name = 'GuestId')
        BEGIN
            -- 임시 UserId 컬럼 추가
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GuestAttributes]') AND name = 'UserId')
            BEGIN
                ALTER TABLE [GuestAttributes] ADD [UserId] INT NULL;
            END

            -- UserId 값 설정
            UPDATE ga
            SET ga.[UserId] = u.[Id]
            FROM [GuestAttributes] ga
            INNER JOIN [Guests] g ON ga.[GuestId] = g.[Id]
            INNER JOIN [Users] u ON u.[Phone] = g.[Phone];

            PRINT '  - Updated GuestAttributes.UserId';

            -- 이후 제약조건은 EF Migration에서 처리됩니다
        END
    END

    -- GuestScheduleTemplates 테이블 업데이트
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'GuestScheduleTemplates')
    BEGIN
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GuestScheduleTemplates]') AND name = 'GuestId')
        BEGIN
            -- 임시 UserId 컬럼 추가
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GuestScheduleTemplates]') AND name = 'UserId')
            BEGIN
                ALTER TABLE [GuestScheduleTemplates] ADD [UserId] INT NULL;
            END

            -- UserId 값 설정
            UPDATE gst
            SET gst.[UserId] = u.[Id]
            FROM [GuestScheduleTemplates] gst
            INNER JOIN [Guests] g ON gst.[GuestId] = g.[Id]
            INNER JOIN [Users] u ON u.[Phone] = g.[Phone];

            PRINT '  - Updated GuestScheduleTemplates.UserId';
        END
    END

    -- GuestActionStatuses 테이블 업데이트 (UserActionStatuses로 리네임 예정)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'GuestActionStatuses')
    BEGIN
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GuestActionStatuses]') AND name = 'GuestId')
        BEGIN
            -- 임시 UserId 컬럼 추가
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GuestActionStatuses]') AND name = 'UserId')
            BEGIN
                ALTER TABLE [GuestActionStatuses] ADD [UserId] INT NULL;
            END

            -- UserId 값 설정
            UPDATE gas
            SET gas.[UserId] = u.[Id]
            FROM [GuestActionStatuses] gas
            INNER JOIN [Guests] g ON gas.[GuestId] = g.[Id]
            INNER JOIN [Users] u ON u.[Phone] = g.[Phone];

            PRINT '  - Updated GuestActionStatuses.UserId';
        END
    END

    -- ConventionChatMessages 테이블 업데이트
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ConventionChatMessages')
    BEGIN
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ConventionChatMessages]') AND name = 'GuestId')
        BEGIN
            -- 임시 UserId 컬럼 추가
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ConventionChatMessages]') AND name = 'UserId')
            BEGIN
                ALTER TABLE [ConventionChatMessages] ADD [UserId] INT NULL;
            END

            -- UserId 값 설정
            UPDATE ccm
            SET ccm.[UserId] = u.[Id]
            FROM [ConventionChatMessages] ccm
            INNER JOIN [Guests] g ON ccm.[GuestId] = g.[Id]
            INNER JOIN [Users] u ON u.[Phone] = g.[Phone];

            PRINT '  - Updated ConventionChatMessages.UserId';

            -- GuestName → UserName 컬럼 리네임 (이후 migration에서 처리)
        END
    END

    -- SurveyResponses 테이블 업데이트
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SurveyResponses')
    BEGIN
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SurveyResponses]') AND name = 'GuestId')
        BEGIN
            -- 임시 UserId 컬럼 추가
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SurveyResponses]') AND name = 'UserId')
            BEGIN
                ALTER TABLE [SurveyResponses] ADD [UserId] INT NULL;
            END

            -- UserId 값 설정
            UPDATE sr
            SET sr.[UserId] = u.[Id]
            FROM [SurveyResponses] sr
            INNER JOIN [Guests] g ON sr.[GuestId] = g.[Id]
            INNER JOIN [Users] u ON u.[Phone] = g.[Phone];

            PRINT '  - Updated SurveyResponses.UserId';
        END
    END

    PRINT 'STEP 3.3: Completed';
    PRINT '';

    PRINT 'STEP 3: Migration completed successfully';
    PRINT '';
END
ELSE
BEGIN
    PRINT 'STEP 3: Guests table not found - skipping data migration';
    PRINT '';
END

-- =====================================================
-- STEP 4: 마이그레이션 결과 요약
-- =====================================================
PRINT 'STEP 4: Migration Summary';
PRINT '========================';

DECLARE @userCount INT, @ucCount INT;
SELECT @userCount = COUNT(*) FROM [Users];
SELECT @ucCount = COUNT(*) FROM [UserConventions];

PRINT '  - Total Users: ' + CAST(@userCount AS NVARCHAR);
PRINT '  - Total UserConventions: ' + CAST(@ucCount AS NVARCHAR);

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Guests')
BEGIN
    PRINT '';
    PRINT 'IMPORTANT: Guests 테이블이 아직 존재합니다.';
    PRINT '           데이터를 확인한 후, Entity Framework migration을 실행하여';
    PRINT '           스키마 변경을 완료하세요:';
    PRINT '           dotnet ef database update';
END

PRINT '';
PRINT '========================================';
PRINT 'Migration Script Completed';
PRINT '========================================';

-- 트랜잭션 커밋 (모든 작업이 성공한 경우)
COMMIT TRANSACTION;

PRINT 'Transaction committed successfully.';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Verify the migrated data in Users and UserConventions tables';
PRINT '2. Run Entity Framework migration: dotnet ef database update';
PRINT '3. Test the application thoroughly';
