# Database Migration Scripts

## Guest to User + UserConvention Migration

### Overview
This migration transforms the database schema from a single `Guest` entity to a split architecture:
- **User**: Core user information (shared across conventions)
- **UserConvention**: M:N mapping between Users and Conventions (convention-specific data)

### Migration Process

#### Prerequisites
1. **Backup your database** - This is critical!
2. Stop the application
3. Ensure you have database admin permissions

#### Steps

**1. Run Data Migration Script**
```sql
-- Execute in SQL Server Management Studio or Azure Data Studio
-- File: MigrateGuestToUserConvention.sql
```

This script will:
- Add Guest-specific columns to Users table (CorpName, CorpPart, ResidentNumber, etc.)
- Create UserConventions table
- Migrate Guest data to Users (merge by phone number)
- Create UserConvention records for each Guest-Convention relationship
- Update related tables (GuestAttributes, GuestScheduleTemplates, etc.) with UserId

**2. Create and Run Entity Framework Migration**
```bash
# Create a new migration for schema changes
dotnet ef migrations add RemoveGuestTable

# Review the generated migration code to ensure it's correct

# Apply the migration
dotnet ef database update
```

The EF migration will:
- Drop old GuestId columns and constraints
- Rename GuestActionStatuses to UserActionStatuses
- Rename ConventionChatMessages.GuestName to UserName
- Drop the Guests table
- Add proper foreign key constraints for new UserId columns

**3. Verify Migration**
```sql
-- Check Users count
SELECT COUNT(*) FROM Users;

-- Check UserConventions count
SELECT COUNT(*) FROM UserConventions;

-- Verify UserConvention relationships
SELECT u.Name, u.Phone, c.ConventionName, uc.GroupName
FROM UserConventions uc
INNER JOIN Users u ON u.Id = uc.UserId
INNER JOIN Conventions c ON c.Id = uc.ConventionId;

-- Verify GuestAttributes migration
SELECT u.Name, ga.AttributeKey, ga.AttributeValue
FROM GuestAttributes ga
INNER JOIN Users u ON u.Id = ga.UserId;
```

**4. Test Application**
- Start the application
- Test user login
- Test guest access token login
- Test convention chat
- Test guest management in admin panel
- Test survey functionality

### What Changes in the Database

#### New Tables
- `UserConventions` - M:N mapping with convention-specific data

#### Modified Tables
- `Users` - Added Guest properties (CorpName, CorpPart, ResidentNumber, Affiliation, EnglishName, PassportNumber, PassportExpiryDate)
- `GuestAttributes` - GuestId → UserId
- `GuestScheduleTemplates` - GuestId → UserId
- `GuestActionStatuses` - Renamed to `UserActionStatuses`, GuestId → UserId
- `ConventionChatMessages` - GuestId → UserId, GuestName → UserName
- `SurveyResponses` - GuestId → UserId

#### Removed Tables
- `Guests` - Data merged into Users + UserConventions

### Data Consolidation Logic

**Users are matched by phone number:**
- If a phone number exists in Users table → update user info
- If a phone number doesn't exist → create new user
- Each Guest creates one UserConvention record (UserId + ConventionId)

**Example:**
```
Before:
  Guest1: { Id=1, Name="김철수", Phone="010-1234-5678", ConventionId=1, GroupName="A그룹" }
  Guest2: { Id=2, Name="김철수", Phone="010-1234-5678", ConventionId=2, GroupName="VIP" }

After:
  User: { Id=1, Name="김철수", Phone="010-1234-5678" }
  UserConvention1: { UserId=1, ConventionId=1, GroupName="A그룹" }
  UserConvention2: { UserId=1, ConventionId=2, GroupName="VIP" }
```

### Rollback Plan

If migration fails:

1. **Restore database backup** (safest option)
2. **Revert code changes:**
   ```bash
   git checkout <previous-commit-hash>
   dotnet ef database update <previous-migration-name>
   ```

### Known Issues

- **Duplicate phone numbers**: If Guests table has different people with same phone number, they will be merged into one User. Review data before migration.
- **Null phone numbers**: Guests without phone numbers cannot be migrated. Ensure all Guests have valid phone numbers.

### Support

For issues or questions, refer to:
- `docs/GRADUAL_REFACTORING_GUIDE.md`
- `docs/REFACTORING_STATUS.md`
