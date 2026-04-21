# Task 003: 컴포넌트 생성 - ConventionHeader + AssignmentBadges

## 목표
행사 홈의 컴팩트 다크 헤더와 배정 정보 인라인 뱃지 컴포넌트를 새로 생성한다.

## 의존성
- 선행 태스크: 없음
- 외부 의존성: MainHeader (기존), NotificationBell (기존)

## 구현 상세

### 생성할 파일
1. **`ClientApp/src/components/convention/ConventionHeader.vue`** (~150줄)
2. **`ClientApp/src/components/convention/AssignmentBadges.vue`** (~80줄)

### ConventionHeader.vue 구현 지침

#### 디자인 참조
목업의 `.hdr` 영역:
- 배경색: 행사의 `brandColor` (기본 어두운 색)
- 좌측: 행사명 (굵게, 흰색) + 날짜 · 장소 (1줄, 반투명 흰색) + D-Day 뱃지
- 우측: 알림벨 (기존 NotificationBell 컴포넌트) + 햄버거 메뉴 (기존 MainHeader 기능)

#### Props
```javascript
const props = defineProps({
  convention: { type: Object, required: true },
  // convention: { title, startDate, endDate, location, brandColor, conventionImg }
  dDay: { type: Number, default: 0 },
})
```

#### 구현 세부
- **배경**: `brandColor` 기반 그라디언트 (기존 ConventionHome의 headerGradientStyle 로직 재사용)
- **높이**: `h-auto` (컴팩트, 기존 h-48보다 작음). 패딩 `px-4 py-3` 정도.
- **행사명**: `text-lg font-bold text-white`
- **날짜 · 장소**: `text-xs text-white/60` 1줄로 표시. 형식: `2026-04-09(수) ~ 04-10(목) · 문경 페트로 호텔`
- **D-Day 뱃지**: 행사명 아래, 작은 뱃지. `bg-white/20 backdrop-blur-sm rounded-full text-xs text-white font-bold px-2.5 py-1`
  - D-Day가 0이면 "D-Day", 양수면 "D-{n}", 진행 중이면 표시 안 함 또는 "진행 중"
- **우측**: 알림벨 아이콘 (기존 NotificationBell 컴포넌트 사용) + 햄버거 메뉴 아이콘
  - 햄버거 메뉴 클릭 시 `emit('menu-click')` 발행
- **이미지 배경 없음**: 기존 `conventionImg` 배경 이미지 제거. brandColor 단색/그라디언트만 사용.

#### 날짜 포맷 함수
기존 ConventionHome의 `formatDateWithDay` 로직을 컴포넌트 내부에 포함:
```javascript
function formatCompactPeriod(startDate, endDate) {
  // "2026-04-09(수) ~ 04-10(목)" 형태
}
```

### AssignmentBadges.vue 구현 지침

#### 디자인 참조
목업의 `.bgs` 영역:
- 속성별 색상 매핑된 작은 뱃지
- 각 뱃지: `[라벨] [값]` 형태
- 색상 팔레트: 보라(#EEEDFE/#3C3489), 초록(#E1F5EE/#085041), 황금(#FAEEDA/#633806) 등 순환

#### Props
```javascript
const props = defineProps({
  // 배정 정보 배열: [{ key: '룸번호', value: '312' }, { key: '룸메이트', value: '심현목' }]
  attributes: { type: Array, default: () => [] },
  // 이 일정에서 표시할 속성 키 목록 (비어있으면 전체 표시)
  showKeys: { type: Array, default: () => [] },
})
```

#### 구현 세부
- **색상 팔레트** (목업 BC 배열 참조): 5가지 색상을 순환 배정
  ```javascript
  const COLOR_PALETTE = [
    { bg: '#EEEDFE', text: '#3C3489' },  // 보라
    { bg: '#E1F5EE', text: '#085041' },  // 초록
    { bg: '#FAEEDA', text: '#633806' },  // 황금
    { bg: '#E1F5EE', text: '#0F6E56' },  // 민트
    { bg: '#FCEBEB', text: '#A32D2D' },  // 빨강
  ]
  ```
- **필터링**: `showKeys`가 비어있지 않으면 해당 키만 표시, 비어있으면 전체 표시
- **렌더링**: `flex flex-wrap gap-1.5`로 인라인 배치
- 각 뱃지: `inline-flex items-center gap-1 rounded-md px-2 py-0.5`
  - 라벨: `text-[10px] opacity-75`
  - 값: `text-xs font-medium`
- 속성이 없으면 렌더링하지 않음 (빈 div 반환)

## 기술 제약
- JavaScript (TypeScript 아님)
- Composition API `<script setup>`
- Tailwind CSS (하드코딩 색상은 style 바인딩으로 - 디자인 토큰 외 목업 특화 색상이므로 허용)
- 기존 MainHeader, NotificationBell 컴포넌트 API 준수

## 완료 기준
1. `npm run build` 성공
2. `npm run lint` 경고/에러 없음
3. ConventionHeader가 brandColor 기반 컴팩트 헤더를 렌더링
4. AssignmentBadges가 attributes 배열을 색상 뱃지로 렌더링
5. 각 컴포넌트 파일 크기 200줄 이하

## 테스트 요구사항
- 빌드 검증: `cd ClientApp && npm run build && npm run lint`
- 수동 검증: 다양한 brandColor 값에서 헤더 가독성 확인
