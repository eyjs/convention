# Migration SQL 검토 체크리스트

## 🔴 위험 신호 (반드시 확인!)

### 1. DROP COLUMN
```sql
ALTER TABLE [Users] DROP COLUMN [PhoneNumber];
```
→ **위험!** 해당 컬럼 데이터 전부 삭제됨
→ **조치:** 데이터 백업 또는 별도 마이그레이션 스크립트 작성

### 2. ALTER COLUMN (타입 변경)
```sql
ALTER TABLE [Products] ALTER COLUMN [Price] nvarchar(max) NOT NULL;
```
→ **위험!** 타입 변환 실패 시 오류
→ **조치:** 기존 데이터 호환성 확인

### 3. NOT NULL 제약
```sql
ALTER TABLE [Users] ALTER COLUMN [Email] nvarchar(100) NOT NULL;
```
→ **위험!** 기존에 NULL 값이 있으면 실패
→ **조치:** NULL 값 먼저 업데이트
```sql
UPDATE [Users] SET [Email] = 'unknown@example.com' WHERE [Email] IS NULL;
```

### 4. DROP TABLE
```sql
DROP TABLE [OldTable];
```
→ **매우 위험!** 테이블과 모든 데이터 삭제
→ **조치:** 정말 삭제할 테이블인지 확인

## ✅ 안전 신호

### 1. CREATE TABLE
```sql
CREATE TABLE [NewEntities] (...);
```
→ **안전** 기존 데이터 영향 없음

### 2. ADD COLUMN (Nullable)
```sql
ALTER TABLE [Users] ADD [GroupName] nvarchar(100) NULL;
```
→ **안전** 기존 데이터에 NULL 추가

### 3. CREATE INDEX
```sql
CREATE INDEX [IX_User_GroupName] ON [Users] ([GroupName]);
```
→ **안전** 데이터 영향 없음

### 4. ADD FOREIGN KEY
```sql
ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Users] 
  FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
```
→ **주의** 기존 데이터가 제약 조건을 위반하면 실패
→ **조치:** 고아 레코드(orphaned records) 확인
