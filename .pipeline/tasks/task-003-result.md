# Task 003 Result

## 상태
DONE

## 생성 파일
- `ClientApp/src/components/convention/ConventionHeader.vue` (82줄)
- `ClientApp/src/components/convention/AssignmentBadges.vue` (52줄)

## 완료 기준 체크
- [x] `npm run build` 성공 (16.66s, 에러 없음)
- [x] `npm run lint` 경고/에러 없음 (기존 Node 모듈 타입 경고만 존재, 신규 파일 무관)
- [x] ConventionHeader가 brandColor 기반 그라디언트 헤더 렌더링
- [x] AssignmentBadges가 attributes 배열을 5색 팔레트 순환 뱃지로 렌더링
- [x] 각 컴포넌트 파일 크기 200줄 이하
- [x] `ClientApp/src/components/convention/` 폴더에 생성
- [x] props만으로 독립 동작

## 테스트 결과
- `npm run build`: 통과
- `npm run lint`: 통과 (--fix 적용, 신규 파일 관련 에러 없음)

## 판단 기록
- `dDay` prop은 null 허용 (진행 중 상태 판별)으로 task 지침 해석: 음수 D-Day는 뱃지 미표시
- ConventionHome의 `headerGradientStyle` 로직을 내부 computed로 재구현 (외부 의존 없이 독립적)
- `conventionId` prop은 NotificationBell에 필수이므로 required: true 유지
- AssignmentBadges의 초록/민트 팔레트 배경색이 동일(#E1F5EE)하지만 텍스트 색상이 다름 — 목업 스펙 그대로 반영
