-- 마이그레이션 검증 쿼리

-- 1. Guests 테이블에 GroupName 컬럼 확인
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Guests' AND COLUMN_NAME = 'GroupName';

-- 2. ConventionActions 테이블에 Description 컬럼 확인
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ConventionActions' AND COLUMN_NAME = 'Description';

-- 3. ConventionActions 테이블의 ConfigJson 컬럼 확인
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ConventionActions' AND COLUMN_NAME = 'ConfigJson';

-- 4. 최근 마이그레이션 확인
SELECT TOP 3
    MigrationId,
    ProductVersion
FROM __EFMigrationsHistory
ORDER BY MigrationId DESC;
