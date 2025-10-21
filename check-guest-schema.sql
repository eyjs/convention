-- Guests 테이블의 현재 컬럼 구조 확인
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Guests'
ORDER BY ORDINAL_POSITION;

-- EnglishName 컬럼이 존재하는지 확인
SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Guests' 
            AND COLUMN_NAME = 'EnglishName'
        )
        THEN 'EnglishName 컬럼이 존재합니다.'
        ELSE 'EnglishName 컬럼이 존재하지 않습니다. 마이그레이션이 필요합니다!'
    END AS Status;

-- 적용된 마이그레이션 목록 확인
SELECT * FROM __EFMigrationsHistory
ORDER BY MigrationId DESC;
