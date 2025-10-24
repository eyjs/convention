-- Migration: Upload System Refactoring
-- Date: 2025-10-24
-- Description: Adds GroupName to Guests and Description to ConventionActions

-- 1. Add GroupName column to Guests
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Guests]')
    AND name = 'GroupName'
)
BEGIN
    ALTER TABLE [dbo].[Guests]
    ADD [GroupName] NVARCHAR(100) NULL;

    PRINT 'GroupName column added to Guests table';
END
ELSE
BEGIN
    PRINT 'GroupName column already exists';
END

-- 2. Add index for GroupName for better query performance
IF NOT EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'IX_Guest_GroupName'
    AND object_id = OBJECT_ID(N'[dbo].[Guests]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Guest_GroupName]
    ON [dbo].[Guests] ([GroupName])
    WHERE [GroupName] IS NOT NULL;

    PRINT 'Index IX_Guest_GroupName created';
END
ELSE
BEGIN
    PRINT 'Index IX_Guest_GroupName already exists';
END

-- 3. Add Description column to ConventionActions
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[ConventionActions]')
    AND name = 'Description'
)
BEGIN
    ALTER TABLE [dbo].[ConventionActions]
    ADD [Description] NVARCHAR(4000) NULL;

    PRINT 'Description column added to ConventionActions table';
END
ELSE
BEGIN
    PRINT 'Description column already exists';
END

GO
