# Plan: SMS 그룹 발송 고도화

## 개요
기존 SmsManagementModal에 탭 방식으로 "그룹 단체 발송"과 "엑셀 변수 발송" 두 가지 흐름을 추가한다.

## 아키텍처 결정

### 백엔드
1. **SmsGroupService** 신규 서비스 생성 - 그룹 조회, 엑셀 템플릿 생성, 엑셀 파싱, 변수 치환 발송 로직
2. **AdminSmsController** 확장 - 4개 신규 엔드포인트 추가
3. **DTO** 추가 - 요청/응답 모델
4. 기존 TemplateVariableService, SmsTemplateContext, ISmsService 재활용

### 프론트엔드
1. **SmsManagementModal.vue** 확장 - 탭 UI 추가 (기존 발송 | 그룹 단체 발송 | 엑셀 변수 발송)
2. **SmsGroupSendTab.vue** 신규 - 그룹 단체 발송 3단계 흐름
3. **SmsExcelSendTab.vue** 신규 - 엑셀 변수 발송 5단계 흐름
4. **smsGroupService.js** 신규 - API 호출 서비스

## 태스크 분할

### Task 1: 백엔드 DTO + Service + Interface (의존성 없음)
- SmsGroupDto.cs 생성
- ISmsGroupService 인터페이스 생성
- SmsGroupService 구현 (그룹 조회, 엑셀 생성, 엑셀 파싱, 변수 치환 발송)
- DI 등록

### Task 2: 백엔드 Controller 엔드포인트 (Task 1 의존)
- AdminSmsController에 4개 엔드포인트 추가

### Task 3: 프론트엔드 API 서비스 (의존성 없음)
- smsGroupService.js 생성

### Task 4: 프론트엔드 그룹 단체 발송 컴포넌트 (Task 3 의존)
- SmsGroupSendTab.vue 생성

### Task 5: 프론트엔드 엑셀 변수 발송 컴포넌트 (Task 3 의존)
- SmsExcelSendTab.vue 생성

### Task 6: SmsManagementModal 탭 통합 (Task 4, 5 의존)
- 기존 모달에 탭 UI 추가, 하위 컴포넌트 연결
