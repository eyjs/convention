# Task 006 결과

## 상태
DONE

## 생성/수정 파일
- `ClientApp/src/layouts/ConventionLayout.vue` — navItems에서 '일정' 탭 제거 (4탭 → 3탭)
- `ClientApp/src/router/index.js` — MySchedule 라우트를 redirect로 변경 (→ /conventions/:id)
- `ClientApp/src/schemas/targetLocations.js` — SCHEDULE_CONTENT_TOP 설명 업데이트

## 완료 기준 체크
- [x] npm run build 성공
- [x] npm run lint 에러 없음
- [ ] dotnet build --no-restore 성공 — 기존 CS0535 오류 존재 (task-006 이전부터 있던 문제: ScheduleUploadService가 IScheduleTemplateUploadService.ConfirmMultiSheetAsync 미구현, 이 태스크 범위 외)
- [x] 하단 탭 3개 (홈/게시판/더보기)
- [x] /conventions/:id/schedule 접근 시 /conventions/:id로 redirect
- [x] SCHEDULE_CONTENT_TOP 설명 홈 화면 기준으로 업데이트
- [x] key 값 변경 없음 (DB 저장값 보존)

## 테스트 결과
- 프론트엔드 빌드: 성공 (21.39s)
- ESLint: 경고/에러 없음 (기존 node 모듈 타입 경고만 표시)
- 백엔드 빌드: CS0535 오류 — 이 태스크 수정과 무관한 기존 문제

## 판단 기록
- 백엔드 빌드 오류(CS0535)는 ScheduleUploadService가 태스크 이전부터 인터페이스를 완전히 구현하지 않은 상태였음. 이 태스크에서는 백엔드 파일을 수정하지 않았으므로 해당 오류는 이 태스크의 범위가 아님.
- redirect 구현 시 conventionId 파라미터를 to.params에서 참조하여 올바른 행사 홈으로 리다이렉트 됨.
