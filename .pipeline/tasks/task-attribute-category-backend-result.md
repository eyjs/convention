# 태스크 결과: 속성 카테고리 백엔드 구현

## 상태
완료

## 생성/수정 파일

### 신규 생성
- `Entities/AttributeCategory.cs` — 속성 카테고리 엔티티
- `Entities/AttributeCategoryItem.cs` — 속성 카테고리 아이템 엔티티
- `Interfaces/IAttributeCategoryService.cs` — 서비스 인터페이스
- `Services/Convention/AttributeCategoryService.cs` — 서비스 구현
- `Controllers/Admin/AttributeCategoryController.cs` — 관리자 API 컨트롤러
- `Migrations/20260421084448_AddAttributeCategory.cs` — EF Core 마이그레이션

### 수정
- `Data/ConventionDbContext.cs` — AttributeCategories, AttributeCategoryItems DbSet 추가
- `Repositories/IUnitOfWork.cs` — 두 Repository 프로퍼티 추가
- `Repositories/UnitOfWork.cs` — lazy-initialized 프로퍼티 구현
- `Extensions/ServiceCollectionExtensions.cs` — AddConventionServices에 DI 등록
- `Controllers/Convention/ConventionsController.cs` — my-attributes/grouped 엔드포인트 추가, IAttributeCategoryService 생성자 주입

## API 엔드포인트

| Method | Route | 설명 |
|--------|-------|------|
| GET | /api/admin/conventions/{conventionId}/attribute-categories | 카테고리 목록 |
| POST | /api/admin/conventions/{conventionId}/attribute-categories | 카테고리 생성 |
| PUT | /api/admin/attribute-categories/{categoryId} | 카테고리 수정 |
| DELETE | /api/admin/attribute-categories/{categoryId} | 카테고리 삭제 |
| GET | /api/admin/conventions/{conventionId}/attribute-keys | 속성 키 목록 |
| PUT | /api/admin/conventions/{conventionId}/attribute-categories/reorder | 순서 변경 |
| GET | /api/conventions/{conventionId}/my-attributes/grouped | 내 속성 그룹화 (사용자용) |

## 완료 기준 체크

- [x] AttributeCategory 엔티티 생성
- [x] AttributeCategoryItem 엔티티 생성
- [x] DbContext DbSet 등록
- [x] IUnitOfWork Repository 프로퍼티 추가
- [x] UnitOfWork lazy-initialized 구현
- [x] EF Core 마이그레이션 생성 (AddAttributeCategory)
- [x] IAttributeCategoryService 인터페이스 정의
- [x] AttributeCategoryService 구현 (7개 메서드)
- [x] DI 등록 (AddConventionServices)
- [x] AttributeCategoryController (관리자 6개 엔드포인트)
- [x] ConventionsController my-attributes/grouped 사용자 엔드포인트

## 빌드 결과
- dotnet build --no-restore: 오류 0개 (경고 53개는 기존 코드 경고)
- dotnet ef migrations add AddAttributeCategory: 성공

## 마이그레이션 특이사항
마이그레이션이 `AttributeDefinitions`, `AttributeTemplates` 테이블 DROP을 포함합니다.
이 두 테이블은 Initial 마이그레이션에서 생성되었으나 현재 프로젝트 코드에서 엔티티/DbSet이 이미 제거된 상태입니다.
운영 DB 적용 전 해당 테이블에 실데이터 존재 여부를 확인하고 필요 시 백업 후 진행해야 합니다.
