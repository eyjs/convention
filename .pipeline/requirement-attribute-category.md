# 요구사항: 속성 카테고리 그룹화 + 레거시 제거

## 개요
참석자 업로드 시 동적 속성(H열~)이 더보기 메뉴에 flat하게 나열되는 것을
카테고리별로 그룹화하여 표시한다.
레거시 기능(여행배정, 속성 템플릿)을 제거하고 해당 메뉴를 속성 카테고리 관리로 대체한다.

## Part 1: 레거시 제거

### 여행배정 (TravelAssignment) 삭제
- `Controllers/Admin/TravelAssignmentController.cs` 삭제
- `Services/Convention/TravelAssignmentService.cs` 삭제
- `Interfaces/ITravelAssignmentService.cs` 삭제
- `ClientApp/src/components/admin/TravelAssignmentManager.vue` 삭제
- `Extensions/ServiceCollectionExtensions.cs` DI 등록 제거
- `router/index.js` 라우트 제거
- `useAdminNav.js` 메뉴 항목 제거
- 엔티티 없음 (GuestAttribute 직접 사용)

### 속성 템플릿 (AttributeTemplate) 삭제
- `Entities/AttributeTemplate.cs` 삭제
- `Controllers/Convention/AttributeTemplateController.cs` 삭제
- `ClientApp/src/components/admin/AttributeTemplateManagement.vue` 삭제
- `DTOs/ConventionModels/AttributeTemplateDto.cs` 삭제
- `Data/ConventionDbContext.cs` DbSet 제거
- `Repositories/IUnitOfWork.cs` + `UnitOfWork.cs` Repository 제거
- `router/index.js` 라우트 제거
- `useAdminNav.js` 메뉴 항목 제거
- DB 마이그레이션: AttributeTemplates 테이블 드롭

### 속성 정의 (AttributeDefinition) 삭제
- `Entities/AttributeDefinition.cs` 삭제
- `Controllers/Convention/AttributeController.cs` 확인 후 제거 (사용처 확인)
- `DTOs/ConventionModels/AttributeDefinitionDto.cs` 삭제
- `Data/ConventionDbContext.cs` DbSet 제거
- `Data/Configurations/CoreEntityConfiguration.cs` 설정 제거
- `Repositories/IUnitOfWork.cs` + `UnitOfWork.cs` Repository 제거
- DB 마이그레이션: AttributeDefinitions 테이블 드롭

## Part 2: 속성 카테고리 그룹화

### 데이터 모델

```
AttributeCategory (신규 엔티티)
├── Id: int (PK)
├── ConventionId: int (FK) — 행사별
├── Name: string — 카테고리명 (예: "호텔 정보", "교통 정보")
├── Icon: string? — 이모지 또는 아이콘명
├── OrderNum: int — 정렬 순서
└── CreatedAt: DateTime

AttributeCategoryItem (신규 엔티티)
├── Id: int (PK)
├── AttributeCategoryId: int (FK)
├── AttributeKey: string — 속성 키 (GuestAttribute.AttributeKey와 매칭)
├── OrderNum: int — 카테고리 내 정렬 순서
└── CreatedAt: DateTime
```

- 행사에 업로드된 속성 목록은 GuestAttribute에서 DISTINCT(AttributeKey) WHERE UserId IN (해당 행사 참석자)로 조회
- 카테고리에 매핑되지 않은 속성은 "기타" 카테고리에 자동 표시
- User 엔티티에 붙지 않음 — Convention 기준

### 관리자 화면 (여행배정 메뉴 대체 → "속성 카테고리")

화면 구성:
1. 카테고리 목록 (드래그 정렬 또는 위/아래 버튼)
   - 카테고리명, 아이콘, 속성 수 표시
   - 추가/수정/삭제 버튼
2. 카테고리 편집
   - 이름, 아이콘 입력
   - 업로드된 속성 목록에서 체크박스로 선택
   - 이미 다른 카테고리에 속한 속성은 비활성 표시
3. "기타" 카테고리는 자동 (미분류 속성 자동 포함)

### 사용자 더보기 화면 변경

현재: flat 리스트
```
내 배정 정보
  룸번호: 312
  룸메이트: 심현목
  버스 호차: 2호차
  ...
```

변경 후: 카테고리별 그룹
```
🏨 호텔 정보
  룸번호: 312
  룸메이트: 심현목
🚌 교통 정보
  버스 호차: 2호차
✈️ 항공 정보
  항공편명(인천발): KE 1544
🍽️ 식사 정보
  테이블번호: 1
📋 기타
  T사이즈: 2XL
```

### API 설계

```
# 관리자
GET    /api/admin/conventions/{id}/attribute-categories     — 카테고리 목록 (items 포함)
POST   /api/admin/conventions/{id}/attribute-categories     — 카테고리 생성
PUT    /api/admin/attribute-categories/{categoryId}          — 카테고리 수정
DELETE /api/admin/attribute-categories/{categoryId}          — 카테고리 삭제
GET    /api/admin/conventions/{id}/attribute-keys            — 해당 행사의 속성 키 목록 (DISTINCT)

# 사용자
GET    /api/conventions/{id}/my-attributes/grouped           — 내 속성 카테고리별 그룹화
```

### 일정 상세 visibleAttributes
- 기존 동작 그대로 유지 (카테고리와 무관)

## 작업 순서
1. 레거시 삭제 (여행배정 + 속성 템플릿 + 속성 정의)
2. DB 마이그레이션 (테이블 드롭 + 신규 테이블 생성)
3. 백엔드 (엔티티 + 서비스 + 컨트롤러)
4. 프론트엔드 관리자 화면
5. 프론트엔드 사용자 더보기 화면
6. 빌드 검증 + 배포

## 제약
- 프론트엔드: Vue 3 + JavaScript (TypeScript 아님)
- 기존 GuestAttribute 엔티티/데이터는 변경 없음
- 일정 visibleAttributes 동작 유지
