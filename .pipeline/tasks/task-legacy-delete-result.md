# task-legacy-delete-result.md

## 상태
DONE

## 작업 요약
레거시 3개 시스템(여행배정 + 속성 템플릿 + 속성 정의) 완전 삭제

## 삭제된 파일 (11개)

### 백엔드
- Controllers/Admin/TravelAssignmentController.cs
- Controllers/Convention/TravelInfoController.cs (ITravelAssignmentService 의존 — 추가 삭제)
- Controllers/Convention/AttributeTemplateController.cs
- Controllers/Convention/AttributeController.cs
- Services/Convention/TravelAssignmentService.cs
- Interfaces/ITravelAssignmentService.cs
- Entities/AttributeTemplate.cs
- Entities/AttributeDefinition.cs
- DTOs/ConventionModels/AttributeTemplateDto.cs
- DTOs/ConventionModels/AttributeDefinitionDto.cs

### 프론트엔드
- ClientApp/src/components/admin/TravelAssignmentManager.vue
- ClientApp/src/components/admin/AttributeTemplateManagement.vue

## 수정된 파일 (7개)

- Extensions/ServiceCollectionExtensions.cs — ITravelAssignmentService DI 등록 제거
- Data/ConventionDbContext.cs — AttributeDefinitions, AttributeTemplates DbSet 제거
- Data/Configurations/CoreEntityConfiguration.cs — AttributeDefinitionConfiguration, AttributeTemplateConfiguration 클래스 제거
- Repositories/IUnitOfWork.cs — AttributeDefinitions, AttributeTemplates 프로퍼티 제거
- Repositories/UnitOfWork.cs — _attributeDefinitions, _attributeTemplates 필드 + 프로퍼티 제거
- ClientApp/src/router/index.js — AdminAttributes, AdminTravelAssignments 라우트 제거
- ClientApp/src/composables/useAdminNav.js — 속성 템플릿, 여행 배정 메뉴 항목 + Tags, Bus 아이콘 import 제거

## 판단 기록

- TravelInfoController.cs는 원본 태스크에 명시되지 않았으나 ITravelAssignmentService를 직접 주입받는 의존 파일이므로 함께 삭제
- ClientApp/src/views/features/TravelInfo.vue는 `/guest/my-travel-info` API를 사용하는 독립 뷰로 TravelAssignmentService와 무관 — 유지
- 사용자 뷰 라우트 travel-info도 TravelInfo.vue가 살아있으므로 유지
- GuestAttribute, AdminAttributeController.cs는 태스크 지시에 따라 전혀 건드리지 않음

## 검증 결과

- dotnet build --no-restore: 오류 0개 (경고 53개 — 기존 경고)
- npm run build: 성공 (built in 20.39s)
