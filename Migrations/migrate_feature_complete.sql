-- Features 테이블 마이그레이션 스크립트
-- 실행 전 백업 권장!

USE [YourDatabaseName]; -- 실제 DB 이름으로 변경
GO

-- 1. 기존 데이터 백업
SELECT * INTO Features_Backup FROM Features;
GO

-- 2. 외래키 제약조건 확인 및 제거
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
              ' DROP CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.foreign_keys
WHERE referenced_object_id = OBJECT_ID('Features');

IF LEN(@sql) > 0
BEGIN
    PRINT 'Dropping foreign keys...';
    EXEC sp_executesql @sql;
END
GO

-- 3. Features 테이블 삭제
DROP TABLE IF EXISTS Features;
GO

-- 4. 새 구조로 Features 테이블 생성
CREATE TABLE Features (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ConventionId INT NOT NULL,
    MenuName NVARCHAR(100) NOT NULL,
    MenuUrl NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 0,
    IconUrl NVARCHAR(500) NOT NULL DEFAULT '',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Features_Conventions FOREIGN KEY (ConventionId) 
        REFERENCES Conventions(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Features_ConventionUrl UNIQUE (ConventionId, MenuUrl)
);
GO

-- 5. 기존 데이터 마이그레이션 (있는 경우)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Features_Backup')
BEGIN
    PRINT 'Migrating old data...';
    
    INSERT INTO Features (ConventionId, MenuName, MenuUrl, IsActive, IconUrl, CreatedAt, UpdatedAt)
    SELECT 
        ConventionId,
        COALESCE(FeatureName, 'Unknown') AS MenuName,
        LOWER(REPLACE(COALESCE(FeatureName, 'unknown'), ' ', '-')) AS MenuUrl,
        CASE 
            WHEN IsEnabled = 'Y' OR IsEnabled = 'true' OR IsEnabled = '1' THEN 1 
            ELSE 0 
        END AS IsActive,
        '📦' AS IconUrl,
        GETUTCDATE() AS CreatedAt,
        GETUTCDATE() AS UpdatedAt
    FROM Features_Backup;
    
    PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' rows migrated.';
END
GO

-- 6. 각 Convention에 기본 Features 추가 (기존 Convention이 있는 경우)
PRINT 'Adding default features to existing conventions...';

INSERT INTO Features (ConventionId, MenuName, MenuUrl, IsActive, IconUrl)
SELECT 
    c.Id,
    feature.MenuName,
    feature.MenuUrl,
    1 AS IsActive,
    feature.IconUrl
FROM Conventions c
CROSS JOIN (
    VALUES 
        ('일정', 'schedule', '📅'),
        ('채팅', 'chat', '💬'),
        ('갤러리', 'gallery', '📷'),
        ('게시판', 'board', '📋')
) AS feature(MenuName, MenuUrl, IconUrl)
WHERE NOT EXISTS (
    SELECT 1 FROM Features f 
    WHERE f.ConventionId = c.Id AND f.MenuUrl = feature.MenuUrl
);

PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' default features added.';
GO

-- 7. 테스트 기능 추가 (ConventionId = 1인 경우, 실제 ID로 변경)
-- 실제 Convention ID 확인 후 주석 해제하여 사용
/*
INSERT INTO Features (ConventionId, MenuName, MenuUrl, IsActive, IconUrl)
VALUES 
    (1, '테스트 기능', 'test', 1, '📦'),
    (1, '설문조사', 'survey', 1, '📋');
GO
*/

-- 8. 백업 테이블 삭제 (확인 후 주석 해제)
-- DROP TABLE Features_Backup;
-- GO

-- 9. 결과 확인
SELECT 
    c.Id AS ConventionId,
    c.Title AS ConventionTitle,
    f.Id AS FeatureId,
    f.MenuName,
    f.MenuUrl,
    f.IsActive,
    f.IconUrl,
    f.CreatedAt
FROM Conventions c
LEFT JOIN Features f ON c.Id = f.ConventionId
ORDER BY c.Id, f.Id;
GO

PRINT 'Migration completed successfully!';
