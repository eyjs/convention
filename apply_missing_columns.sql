-- 누락된 컬럼만 추가
ALTER TABLE Guests ADD EnglishName NVARCHAR(100) NULL;
ALTER TABLE Guests ADD PassportNumber NVARCHAR(50) NULL;
ALTER TABLE Guests ADD PassportExpiryDate DATE NULL;
ALTER TABLE Guests ADD VisaDocumentAttachmentId INT NULL;

-- 마이그레이션 히스토리 업데이트
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20251020060651_AddDynamicActionSystem', '8.0.0');