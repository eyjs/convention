-- Feature 테이블 구조 변경 마이그레이션
-- 실행 전 백업 필수!

-- 1. 백업 테이블 생성
SELECT * INTO Features_Backup FROM Features;

-- 2. 기존 테이블 삭제
DROP TABLE Features;

-- 3. 새 구조로 테이블 재생성
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

-- 4. 기존 데이터 마이그레이션 (구조에 맞게 조정)
INSERT INTO Features (ConventionId, MenuName, MenuUrl, IsActive, IconUrl, CreatedAt, UpdatedAt)
SELECT 
    ConventionId,
    FeatureName AS MenuName,
    LOWER(REPLACE(FeatureName, ' ', '-')) AS MenuUrl,
    CASE WHEN IsEnabled = 'true' THEN 1 ELSE 0 END AS IsActive,
    '/icons/default.svg' AS IconUrl,
    GETUTCDATE() AS CreatedAt,
    GETUTCDATE() AS UpdatedAt
FROM Features_Backup;

-- 5. 백업 테이블 삭제 (확인 후 주석 해제)
-- DROP TABLE Features_Backup;

-- 6. 테스트 데이터 삽입 (선택사항)
-- INSERT INTO Features (ConventionId, MenuName, MenuUrl, IsActive, IconUrl)
-- VALUES 
-- (1, '테스트 기능', 'test', 1, '/icons/test.svg'),
-- (1, '설문조사', 'survey', 1, '/icons/survey.svg'),
-- (1, '미니게임', 'game', 0, '/icons/game.svg');
