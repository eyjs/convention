-- 현재 적용된 마이그레이션 확인
SELECT * FROM __EFMigrationsHistory;

-- Guest 테이블 컬럼 확인
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Guests' 
AND COLUMN_NAME IN ('EnglishName', 'PassportNumber', 'PassportExpiryDate', 'VisaDocumentAttachmentId');