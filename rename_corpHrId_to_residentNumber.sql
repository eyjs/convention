-- Manual migration to rename CorpHrId to ResidentNumber

-- Step 1: Add new column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Guests]') AND name = 'ResidentNumber')
BEGIN
    ALTER TABLE Guests ADD ResidentNumber NVARCHAR(50) NULL;
END

-- Step 2: Copy data from old column to new column (if old column exists)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Guests]') AND name = 'CorpHrId')
BEGIN
    UPDATE Guests SET ResidentNumber = CorpHrId WHERE CorpHrId IS NOT NULL;
    
    -- Step 3: Drop old column
    ALTER TABLE Guests DROP COLUMN CorpHrId;
END

PRINT 'Migration completed: CorpHrId renamed to ResidentNumber';
