-- 빠른 마이그레이션 (데이터 손실 주의!)
-- 기존 Features 데이터가 중요하지 않은 경우 사용

-- 1. 기존 테이블 삭제
DROP TABLE IF EXISTS Features;

-- 2. 새 테이블 생성
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

-- 3. 테스트 데이터 추가 (Convention ID를 실제 값으로 변경)
INSERT INTO Features (ConventionId, MenuName, MenuUrl, IsActive, IconUrl)
VALUES 
    (1, '테스트 기능', 'test', 1, '📦'),
    (1, '설문조사', 'survey', 1, '📋');

-- 확인
SELECT * FROM Features;
