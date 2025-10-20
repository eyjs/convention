# 빠른 테스트 가이드

## 1. 데이터베이스 마이그레이션 (필수)

### SQL Server Management Studio에서 실행:

```sql
-- 1. 백업
SELECT * INTO Features_Backup FROM Features;

-- 2. 기존 테이블 삭제
DROP TABLE Features;

-- 3. 새 테이블 생성
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

-- 4. 테스트 데이터 삽입 (ConventionId는 실제 존재하는 ID로 변경)
INSERT INTO Features (ConventionId, MenuName, MenuUrl, IsActive, IconUrl)
VALUES (1, '테스트 기능', 'test', 1, '📦');
```

## 2. 백엔드 실행

```bash
cd C:/Users/USER/dev/startour/convention
dotnet run
```

## 3. 프론트엔드 실행

```bash
cd C:/Users/USER/dev/startour/convention/ClientApp
npm run dev
```

## 4. 테스트 절차

1. **브라우저에서 앱 열기**: `http://localhost:5173`
2. **로그인**
3. **행사 선택** (Feature를 추가한 ConventionId와 동일해야 함)
4. **하단 네비게이션의 "더보기" (맨 오른쪽) 클릭**
5. **기능 목록 페이지 확인**: `/features`
6. **"테스트 기능" 카드 클릭**
7. **테스트 페이지 확인**: `/feature/test`
8. **"뒤로" 버튼으로 복귀**

## 5. 확인 사항

### ✅ 정상 동작 시:
- `/features` 페이지에 "테스트 기능" 카드 표시
- 카드에 📦 아이콘 표시
- 클릭 시 `/feature/test`로 이동
- 테스트 페이지 렌더링
- 상단에 "뒤로" 버튼
- 카운터 버튼 동작

### ❌ 문제 발생 시:

#### 기능 목록이 비어있음
- 브라우저 콘솔 확인
- Network 탭에서 `/api/conventions/{id}/features` 호출 확인
- 응답 데이터 확인

#### "기능을 불러올 수 없습니다" 에러
- URL이 정확한지 확인: `/feature/test`
- 파일 경로 확인: `ClientApp/src/dynamic-features/TestFeature/views/TestPage.vue`
- 파일명이 `TestPage.vue`인지 확인

## 6. 추가 기능 테스트 (선택)

### 설문조사 기능 추가:

```sql
INSERT INTO Features (ConventionId, MenuName, MenuUrl, IsActive, IconUrl)
VALUES (1, '설문조사', 'survey', 1, '📋');
```

```bash
# 프론트엔드
mkdir -p ClientApp/src/dynamic-features/SurveyFeature/views
# SurveyPage.vue 생성 (TestPage.vue 참고)
```

## 7. 개발자 도구 활용

### 브라우저 콘솔에서:

```javascript
// Feature Store 확인
import { useFeatureStore } from '@/stores/feature'
const featureStore = useFeatureStore()
console.log(featureStore.activeFeatures)

// 라우터 확인
import { useRoute } from 'vue-router'
const route = useRoute()
console.log(route.meta)
```

## 8. API 직접 테스트

### Postman 또는 curl:

```bash
# 기능 목록 조회
curl http://localhost:5000/api/conventions/1/features

# 기능 생성
curl -X POST http://localhost:5000/api/conventions/1/features \
  -H "Content-Type: application/json" \
  -d '{"menuName":"새 기능","menuUrl":"new-feature","isActive":true,"iconUrl":"🎉"}'

# 상태 토글
curl -X PATCH http://localhost:5000/api/conventions/1/features/1/status \
  -H "Content-Type: application/json" \
  -d '{"isActive":false}'
```

## 9. 문제 해결

### 포트 충돌
```bash
# 백엔드: appsettings.json에서 포트 변경
# 프론트엔드: vite.config.js에서 포트 변경
```

### CORS 에러
```csharp
// Program.cs의 CORS 설정 확인
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", builder => {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

### 라우팅 에러
- `router/index.js`에 `MoreFeaturesView` import 확인
- `/features` 라우트 등록 확인

## 10. 다음 단계

✅ 기본 구조 테스트 완료 후:
1. 실제 설문조사 기능 구현
2. 기존 views 파일들을 features 폴더로 이동
3. 관리자 페이지에서 기능 CRUD 구현
4. 아이콘 이미지 파일 추가
