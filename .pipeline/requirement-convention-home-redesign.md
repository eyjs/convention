# 행사 홈 화면 구조 개편

## 배경

기획자가 사용자 화면 구조 변경을 요청. 목업 HTML(`/docs/행사_홈페이지_목업.html`) 기반.
핵심: 행사 홈 진입 시 바로 타임라인 일정이 보이도록, 일정 탭을 홈에 통합하고 더보기를 내 정보 중심으로 재구성.

## 스코프 정의

### 이번 피처 (Phase 1: 화면 구조 개편)
- 행사 홈 = 타임라인 일정 통합
- 하단 탭 4개 → 3개 (홈/게시판/더보기)
- 더보기 = 내 정보 + 배정 정보 + 메뉴 섹션

### 별도 피처 (Phase 2: 속성 마이그레이션) — 이번에 안 함
- GuestAttribute를 계정 레벨 → 행사(UserConvention) 레벨로 분리
- 계정 자체 속성 vs 행사별 속성 2-depth 구조
- DB 마이그레이션 + 관리자 UI 변경 포함

### 별도 피처 (Phase 3: 동반자 시스템) — 이번에 안 함
- 관리자가 동반자 매칭 (가족/미취학아동/관리자 등)
- 동반자 간 정보 확인 가능
- 동반자별 다른 일정 배정 가능
- 관리자 매칭 UI + 사용자 조회 UI

---

## Phase 1 상세 요구사항

### 1. 행사 홈 (ConventionHome) 재구성

**현재**: 배너 헤더 + D-Day + 내 정보 카드 + 체크리스트 + 설문 + 공지 미리보기 + 일정 미리보기 3개
**변경**: 컴팩트 헤더 + 타임라인 일정 (전체)

#### 1-1. 헤더 변경
- 현재: 큰 배너 이미지(h-48) + 그라디언트 오버레이
- 변경: **컴팩트 헤더** (목업 참조)
  - **배경색은 행사의 brandColor(브랜드컬러)** 를 따름 (행사 생성 시 설정)
  - 행사명 (굵게, 흰색)
  - 날짜 · 장소 (1줄, 반투명 흰색)
  - **D-Day 뱃지** (위 2줄 아래에 추가)
  - 우측 알림벨 (현재와 동일)
  - 우측 햄버거 메뉴 (현재와 동일)

#### 1-2. 타임라인 일정 (홈 메인 콘텐츠)
- 목업의 세로 타임라인 UI 구현
- 날짜별 구분선 (`4월 9일`, `4월 10일`)
- 각 일정 항목: 시간 | 도트+라인 | 제목 + 장소 + 배정뱃지 | 화살표
- **현재 진행 중** 일정: 초록 도트 + 연초록 배경 하이라이트
- **지난 일정**: opacity 낮춤 (흐림 처리)
- **배정 정보 뱃지**: 해당 일정에 연결된 속성값을 인라인 뱃지로 표시 (룸번호, 룸메이트, 골프조 등)
- 일정 클릭 → **바텀시트 모달** (시간, 장소(지도링크), 상세 안내, 내 배정 정보)

#### 1-3. 기존 홈 요소 처리
| 요소 | 처리 |
|------|------|
| 배너 이미지 헤더 | → 컴팩트 다크 헤더로 교체 |
| D-Day 뱃지 | → 헤더 내부로 이동 (유지) |
| 내 정보 카드 (이름/코스/그룹) | → 더보기로 이동 |
| GLOBAL_ROOT_POPUP 동적 액션 | → 유지 (위치 무관) |
| HOME_SUB_HEADER 동적 액션 | → 유지 (헤더 아래) |
| HOME_CONTENT_TOP 동적 액션 | → 유지 (타임라인 위) |
| 체크리스트 진행상황 | → 더보기 메뉴 섹션으로 이동 |
| 설문조사 미완료 | → 더보기 메뉴 섹션으로 이동 |
| 공지사항 미리보기 | → 제거 (게시판 탭에서 접근) |
| 나의 일정 미리보기 3개 | → 제거 (타임라인으로 대체) |

#### 1-4. SCHEDULE_CONTENT_TOP 동적 액션
- 기존 일정 탭에 있던 동적 액션
- 홈의 타임라인 위 영역에서 렌더링

### 2. 하단 탭 변경

**현재**: 홈 / 일정 / 게시판 / 더보기 (4탭)
**변경**: 홈 / 게시판 / 더보기 (3탭)

- ConventionLayout.vue의 navItems에서 `schedule` 항목 제거
- `/conventions/:id/schedule` 라우트 → `/conventions/:id`로 redirect

### 3. 더보기 재구성

**현재**: Dynamic Action 메뉴 2x2 그리드 (MENU 타입 액션 + 설문 고정카드)
**변경**: 내 정보 + 배정 정보 + 메뉴 섹션

#### 3-1. 내 정보 섹션 (목업 참조)
- 이름 / 역할
- 코스 (배정된 일정 코스)
- 그룹

#### 3-2. 내 배정 정보 섹션
- 2열 그리드 카드 (목업 참조)
- GuestAttribute 키-값 전체 표시
- 값 없으면 "미배정" 표시
- **현재는 계정 레벨 속성** (Phase 2에서 행사 레벨로 마이그레이션 예정)

#### 3-3. 메뉴 섹션
- 배정 정보 아래에 별도 영역
- **설문조사**: 1개 메뉴로 노출, 진입 시 발행된 설문 목록 표시 (1건이든 10건이든 메뉴는 1개)
- **Dynamic Action MENU 타입**: 기존 동적 메뉴들 유지
- **체크리스트**: 기존 체크리스트 그룹 카드 유지
- 목업 스타일의 리스트 행 형태 (아이콘 + 라벨 + 값/뱃지)

### 4. 배정 정보 API 변경 (Phase 1 범위)

현재 `UserProfileService.GetMyConventionInfoAsync`에서 attributes를 의도적으로 빈 배열로 반환 중.

- `attributes`에 실제 GuestAttribute 데이터 포함하여 반환
- 프론트에서 키-값 전체 열거
- Phase 2에서 행사별 속성으로 마이그레이션할 때 API 응답 구조는 동일하게 유지 가능

### 5. 일정 업로드 구조 개편

참조 파일: `docs/일정업로드_김도현_260415.xlsx`, `docs/참석자업로드_김도현_260415.xlsx`

#### 5-1. ScheduleItem에 VisibleAttributes 필드 추가
- `VisibleAttributes` (string, nullable) — 쉼표 구분 속성 키 목록
- 예: `"룸번호,룸메이트"`, `"식사 테이블 번호"`
- 타임라인에서 이 키 목록으로 사용자의 GuestAttribute를 필터해서 뱃지 표시
- DB 마이그레이션 필요

#### 5-2. 일정 엑셀 업로드 — 멀티시트 지원
- 시트 1개 = ScheduleTemplate(일정 코스) 1개
- 여러 시트 → 한번에 여러 코스 생성
- 컬럼: 날짜, 시작시간, 종료시간, 장소, 지도링크, 일정명, 내용, **노출_개인정보**
- `노출_개인정보` 컬럼 → ScheduleItem.VisibleAttributes에 저장
- 마지막 행이 `※`로 시작하면 안내 행이므로 스킵

#### 5-3. 참석자 엑셀 업로드 — 가변 속성 컬럼 지원
- 고정 컬럼: 번호, 소속, 이름, 주민등록번호, 전화번호, 그룹명, 비고
- **가변 컬럼** (8번째 이후): 연두색 컬럼 = GuestAttribute 키-값
- 예: `룸번호=312`, `룸메이트=김영희`, `버스좌석=A-12`, `식사테이블=4`
- 헤더 행의 컬럼명 = AttributeKey, 데이터 행의 값 = AttributeValue
- 기존 GuestAttribute가 있으면 덮어쓰기

#### 5-4. 그룹-일정매핑 시트
- 참석자 엑셀의 2번째 시트: `그룹-일정매핑`
- 컬럼: 그룹명, 일정코스명, 코스설명
- 그룹명 → 참석자의 GroupName과 매칭
- 일정코스명 → ScheduleTemplate 이름과 매칭
- 업로드 시 UserConvention.GroupName 설정 + GuestScheduleTemplate 매핑

---

## 기술 설계 방향

### 컴포넌트 분리 (ConventionHome.vue 크기 관리)

현재 1,162줄 → 타임라인 통합 시 1,800줄+ 예상. 반드시 분리:

```
components/convention/
├── ConventionHeader.vue        # 컴팩트 다크 헤더 (NEW)
├── ScheduleTimeline.vue        # 타임라인 일정 UI (MySchedule.vue에서 추출)
├── ScheduleDetailModal.vue     # 일정 상세 바텀시트 (MySchedule.vue에서 추출)
└── AssignmentBadges.vue        # 배정 정보 인라인 뱃지 (NEW)

views/
├── ConventionHome.vue          # 헤더 + 타임라인 조합 (대폭 축소)
└── MoreFeaturesView.vue        # 내 정보 + 배정 + 메뉴 (전면 재작성)
```

### MySchedule.vue 처리
- 핵심 UI → `ScheduleTimeline.vue` + `ScheduleDetailModal.vue`로 추출
- 라우트 제거 후 redirect 설정
- 옵션투어 통합 로직, 관리자 참석자 보기 등은 ScheduleTimeline 내부에 유지

### targetLocations.js 수정
- `SCHEDULE_CONTENT_TOP` → page 설명 수정 (일정 탭 → 홈)
- `MORE_FEATURES_GRID` → 더보기 메뉴 섹션에서 계속 렌더링

---

## 영향받는 파일 목록

| 파일 | 변경 | 범위 |
|------|------|------|
| `ConventionHome.vue` | 헤더 교체 + 타임라인 통합 + 기존 요소 제거/이동 | 대 |
| `MySchedule.vue` | 컴포넌트 추출 후 라우트 제거 | 대 |
| `MoreFeaturesView.vue` | 내 정보 + 배정 + 메뉴로 전면 재작성 | 대 |
| `ConventionLayout.vue` | navItems 4→3탭 | 소 |
| `router/index.js` | schedule 라우트 제거/redirect | 소 |
| `targetLocations.js` | 설명 텍스트 수정 | 소 |
| `UserProfileService.cs` | attributes 실제 데이터 반환 | 소 |
| `components/convention/*.vue` | 신규 컴포넌트 4개 생성 | 중 |

---

## 후속 피처 메모

### Phase 2: 속성 2-depth 마이그레이션
- 현재: `GuestAttribute` → User(계정) 레벨에 종속
- 변경: 행사별 속성은 `UserConvention` 테이블 레벨로 이동
  - 행사마다 속성값이 달라지므로 행사 게스트에 붙는 속성
  - 계정 자체에 붙는 속성(여권번호, 영문명 등)은 계정 레벨 유지
- DB 스키마: `ConventionGuestAttribute` 테이블 신규 또는 UserConvention에 JSON 컬럼
- 관리자 속성 템플릿 UI도 행사 레벨로 분리

### Phase 3: 동반자 시스템
- 관리자가 참석자 간 동반자 관계 매칭
- 유형: 가족(와이프), 미취학아동, 관리자(비서) 등
- 동반자 간 서로의 정보 확인 가능
- 동반자별 다른 일정 배정 가능 (별도 코스)
- 더보기에서 "동반자 보기" 토글 UI

## 참고
- 목업: `/docs/행사_홈페이지_목업.html`
- 행사 목록(MainHome)은 이번 스코프 외 — 별도 유지
- HomeBanner 캐러셀은 MainHome(행사 리스트 페이지)에서 사용 — 행사 상세와 무관
