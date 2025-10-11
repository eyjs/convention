# LLM 챗봇 프론트엔드 설치 및 사용 가이드

## 📋 목차
1. [프로젝트 구조](#프로젝트-구조)
2. [설치 방법](#설치-방법)
3. [사용 방법](#사용-방법)
4. [컴포넌트 설명](#컴포넌트-설명)
5. [커스터마이징](#커스터마이징)
6. [트러블슈팅](#트러블슈팅)

---

## 프로젝트 구조

```
ClientApp/src/
├── components/
│   └── chatbot/                    ✅ 새로 생성됨
│       ├── ChatWindow.vue          # 메인 채팅 창
│       ├── ChatMessage.vue         # 메시지 버블
│       ├── ChatInput.vue           # 입력 영역
│       ├── SuggestedQuestions.vue  # 추천 질문
│       └── ChatFloatingButton.vue  # 플로팅 버튼
├── stores/
│   ├── convention.js               # 기존
│   └── chat.js                     ✅ 새로 생성됨
├── services/
│   ├── api.js                      # 기존
│   └── chatService.js              ✅ 새로 생성됨
├── App.vue                         ✅ 수정됨 (챗봇 통합)
└── main.js
```

---

## 설치 방법

### Step 1: 의존성 확인

필요한 패키지가 이미 설치되어 있는지 확인하세요:

```bash
cd D:\study\startour\convention\ClientApp
```

**package.json 확인:**
```json
{
  "dependencies": {
    "vue": "^3.3.4",           ✅ 있음
    "pinia": "^2.1.6",         ✅ 있음
    "axios": "^1.5.0",         ✅ 있음
    "dayjs": "^1.11.18"        ✅ 있음
  }
}
```

모든 필수 패키지가 이미 설치되어 있으므로 **추가 설치 불필요**!

### Step 2: 파일 확인

다음 파일들이 정상적으로 생성되었는지 확인:

```bash
# 서비스
src/services/chatService.js           ✅

# 스토어
src/stores/chat.js                    ✅

# 컴포넌트
src/components/chatbot/ChatWindow.vue         ✅
src/components/chatbot/ChatMessage.vue        ✅
src/components/chatbot/ChatInput.vue          ✅
src/components/chatbot/SuggestedQuestions.vue ✅
src/components/chatbot/ChatFloatingButton.vue ✅

# 메인 앱
src/App.vue                           ✅ (수정됨)
```

### Step 3: 개발 서버 실행

```bash
# 프론트엔드 개발 서버 실행
cd D:\study\startour\convention\ClientApp
npm run dev

# 출력 예시:
# VITE v4.4.9  ready in 500 ms
# ➜  Local:   http://localhost:3000/
# ➜  Network: http://192.168.0.100:3000/
```

### Step 4: 백엔드 서버 실행

**새 터미널 창에서:**

```bash
cd D:\study\startour\convention
dotnet run

# 출력 예시:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:5001
# info: Microsoft.Hosting.Lifetime[0]
#       Application started.
```

### Step 5: 브라우저에서 확인

1. 브라우저에서 `http://localhost:3000` 접속
2. 우하단에 **파란색 플로팅 버튼** 확인
3. 버튼 클릭 → 채팅 창 열림 확인

---

## 사용 방법

### 1. 기본 사용

#### 채팅 창 열기
```
1. 우하단 플로팅 버튼 클릭
2. 채팅 창이 모달로 열림
3. 환영 메시지와 추천 질문 표시
```

#### 질문하기
```
방법 1: 추천 질문 클릭
- "이번 행사는 언제 진행되나요?" 클릭
- 자동으로 질문 전송

방법 2: 직접 입력
- 하단 입력창에 질문 입력
- Enter 키 또는 전송 버튼 클릭
- (Shift+Enter로 줄바꿈 가능)
```

#### 답변 확인
```
- AI 응답이 왼쪽에 표시됨
- 출처 정보 클릭 → 근거 문서 확인
- LLM 프로바이더 정보 표시 (Gemini, Llama3 등)
```

### 2. 고급 사용

#### 멀티턴 대화 (컨텍스트 유지)

```javascript
// 대화 예시:
사용자: "이번 행사는 언제야?"
AI:     "3월 15일부터 18일까지입니다."

사용자: "참석자는 몇 명이야?"  // 이전 대화 컨텍스트 유지
AI:     "이번 행사에 총 50명이 등록되어 있습니다."
```

**작동 원리:**
- 최근 10개의 메시지를 히스토리로 전송
- LLM이 이전 대화를 참조하여 답변 생성
- 자연스러운 대화 흐름 유지

#### Convention별 컨텍스트

```javascript
// convention.js 스토어에서 currentConvention 설정 시
// 자동으로 해당 Convention 정보만 검색

// 예시:
conventionStore.setCurrentConvention(1)  // Convention ID: 1
// → 이후 모든 질문은 Convention 1에 대한 답변만 제공
```

### 3. 관리자 기능

#### 데이터 재색인

백엔드 API를 직접 호출하여 색인 업데이트:

```bash
# 전체 재색인
curl -X POST http://localhost:5001/api/conventionchat/reindex

# 특정 Convention 색인
curl -X POST http://localhost:5001/api/conventionchat/conventions/1/index
```

또는 개발자 도구 콘솔에서:

```javascript
// 전체 재색인
await chatService.reindexAll()

// 특정 Convention 색인
await chatService.indexConvention(1)
```

---

## 컴포넌트 설명

### 1. ChatWindow.vue (메인 채팅 창)

**역할:**
- 채팅 UI의 메인 컨테이너
- 메시지 목록, 입력창, 추천 질문 통합

**주요 기능:**
```javascript
// 헤더
- AI 아바타 및 제목
- Convention 정보 표시
- 새로고침 버튼 (대화 초기화)
- 닫기 버튼

// 추천 질문 영역
- Convention별 맞춤 질문 표시
- 클릭 시 자동 질문 전송

// 메시지 영역
- 스크롤 가능한 메시지 리스트
- 빈 상태 UI (처음 사용 시)
- 타이핑 인디케이터 (로딩 중)

// 입력 영역
- 텍스트 입력
- 전송 버튼
```

**Props:** 없음 (Store 기반)

**작동 원리:**
```
1. onMounted에서 초기화
   - Convention 컨텍스트 설정
   - 추천 질문 로드
   - 환영 메시지 추가

2. 사용자 입력 처리
   - handleSend() → chatStore.sendMessage()
   - 백엔드 API 호출
   - 응답을 메시지 목록에 추가

3. 자동 스크롤
   - 새 메시지 추가 시 자동으로 최하단 스크롤
   - smooth 애니메이션 적용
```

### 2. ChatMessage.vue (메시지 버블)

**역할:**
- 개별 메시지를 렌더링
- 사용자/AI 구분하여 표시

**Props:**
```javascript
{
  message: {
    id: number,
    role: 'user' | 'assistant',
    content: string,
    timestamp: string,
    sources: Array<Source>,  // AI 메시지만
    llmProvider: string,     // AI 메시지만
    isError: boolean
  }
}
```

**스타일링:**
```javascript
// 사용자 메시지
- 오른쪽 정렬
- 파란색 그라디언트 배경
- 아바타: 사용자 아이콘

// AI 메시지
- 왼쪽 정렬
- 흰색 배경 + 테두리
- 아바타: "AI" 텍스트
- 출처 정보 표시 (토글 가능)
- LLM 프로바이더 표시
```

**출처 정보 표시:**
```javascript
// sources 배열이 있으면 표시
sources: [
  {
    content: "행사명: 2025 해외 워크샵...",
    similarity: 0.89,
    conventionTitle: "2025 해외 워크샵"
  }
]

// UI: "출처 3개" 버튼 → 클릭 시 상세 정보 표시
```

### 3. ChatInput.vue (입력 영역)

**역할:**
- 사용자 입력 받기
- 전송 버튼 제공

**Props:**
```javascript
{
  loading: boolean,          // 로딩 상태
  disabled: boolean,         // 비활성화
  placeholder: string,       // 플레이스홀더
  showCharCount: boolean,    // 글자 수 표시
  showHint: boolean,         // 안내 텍스트
  maxLength: number          // 최대 글자 수 (기본 2000)
}
```

**주요 기능:**
```javascript
// 자동 높이 조절
- 내용에 따라 textarea 높이 자동 변경
- 최소 44px, 최대 120px

// 키보드 단축키
- Enter: 메시지 전송
- Shift+Enter: 줄바꿈

// 유효성 검증
- 공백만 입력 시 전송 불가
- 최대 글자 수 초과 시 전송 불가
```

**Emits:**
```javascript
// 'send' 이벤트
emit('send', message)  // message: string
```

### 4. SuggestedQuestions.vue (추천 질문)

**역할:**
- AI가 추천하는 질문 목록 표시
- 클릭 시 자동 질문 전송

**Props:**
```javascript
{
  questions: Array<string>,  // 추천 질문 목록
  title: string,             // 헤더 제목
  maxVisible: number,        // 최대 표시 개수 (기본 3)
  showRefresh: boolean       // 새로고침 버튼
}
```

**작동 원리:**
```javascript
// 백엔드에서 추천 질문 생성
GET /api/conventionchat/conventions/1/suggestions

// 응답:
[
  "이번 행사는 언제 진행되나요?",
  "참석자는 몇 명인가요?",
  "행사 일정을 알려주세요"
]

// UI 표시
- 처음 3개만 표시
- "더 보기" 버튼 → 전체 표시
```

**Emits:**
```javascript
// 'select' 이벤트
emit('select', question)  // question: string

// 'refresh' 이벤트
emit('refresh')
```

### 5. ChatFloatingButton.vue (플로팅 버튼)

**역할:**
- 채팅 창 열기/닫기 토글
- 화면에 고정된 버튼

**Props:**
```javascript
{
  position: 'bottom-right' | 'bottom-left' | 'top-right' | 'top-left',
  showBadge: boolean,    // 알림 뱃지
  showRipple: boolean    // 물결 효과
}
```

**스타일:**
```javascript
// 열려있을 때
- 빨간색 배경
- X 아이콘

// 닫혀있을 때
- 파란색 그라디언트 배경
- 채팅 아이콘
- 물결 효과 (주목 유도)
```

---

## 커스터마이징

### 1. 색상 변경

**ChatWindow.vue 헤더 색상:**

```vue
<!-- 현재: 파란색 그라디언트 -->
<div class="bg-gradient-to-r from-blue-600 to-blue-700">

<!-- 변경 예시: 초록색 그라디언트 -->
<div class="bg-gradient-to-r from-green-600 to-green-700">
```

**플로팅 버튼 색상:**

```vue
<!-- ChatFloatingButton.vue -->
<!-- 현재 -->
bg-gradient-to-r from-blue-600 to-blue-700

<!-- 변경 예시 -->
bg-gradient-to-r from-purple-600 to-purple-700
```

### 2. 위치 변경

**플로팅 버튼 위치:**

```vue
<!-- App.vue -->
<!-- 현재: 우하단 -->
<ChatFloatingButton position="bottom-right" />

<!-- 좌하단으로 변경 -->
<ChatFloatingButton position="bottom-left" />

<!-- 우상단으로 변경 -->
<ChatFloatingButton position="top-right" />
```

### 3. 메시지 스타일 변경

**말풍선 모양:**

```vue
<!-- ChatMessage.vue -->
<!-- 현재: 둥근 사각형 -->
<div class="rounded-2xl">

<!-- 더 둥글게 -->
<div class="rounded-3xl">

<!-- 각지게 -->
<div class="rounded-lg">
```

### 4. 추천 질문 개수 조절

**SuggestedQuestions.vue에서:**

```vue
<!-- App.vue에서 -->
<SuggestedQuestions
  :questions="questions"
  :max-visible="5"  <!-- 기본 3 → 5로 변경 -->
/>
```

### 5. 입력창 최대 글자 수 변경

```vue
<!-- ChatInput.vue 사용 시 -->
<ChatInput
  :max-length="5000"  <!-- 기본 2000 → 5000으로 변경 -->
/>
```

### 6. 환영 메시지 커스터마이징

**chat.js 스토어:**

```javascript
// 현재
function addWelcomeMessage(conventionTitle = null) {
  const welcomeText = conventionTitle
    ? `안녕하세요! "${conventionTitle}" 행사에 대해 궁금하신 점을 물어보세요.`
    : '안녕하세요! 행사에 대해 궁금하신 점을 물어보세요.'
  
  // ...
}

// 커스터마이징 예시
function addWelcomeMessage(conventionTitle = null) {
  const welcomeText = conventionTitle
    ? `👋 ${conventionTitle} AI 어시스턴트입니다. 무엇을 도와드릴까요?`
    : '👋 무엇을 도와드릴까요?'
  
  // ...
}
```

---

## 트러블슈팅

### 문제 1: 채팅창이 열리지 않음

**증상:**
- 플로팅 버튼 클릭 시 아무 반응 없음

**원인:**
- Store가 제대로 import되지 않음
- 컴포넌트 경로 오류

**해결책:**

```javascript
// App.vue 확인
import ChatWindow from '@/components/chatbot/ChatWindow.vue'
import ChatFloatingButton from '@/components/chatbot/ChatFloatingButton.vue'

// 경로가 정확한지 확인
// @ = src 디렉토리
```

**브라우저 콘솔 확인:**
```
F12 → Console 탭
에러 메시지 확인
```

### 문제 2: API 호출 실패 (404 Not Found)

**증상:**
```
POST http://localhost:3000/api/conventionchat/ask 404 (Not Found)
```

**원인:**
- 백엔드 서버가 실행되지 않음
- API 경로 오류

**해결책:**

```bash
# 1. 백엔드 서버 확인
cd D:\study\startour\convention
dotnet run

# 2. Swagger UI에서 API 확인
# https://localhost:5001/swagger
# ConventionChat 섹션에 엔드포인트 있는지 확인

# 3. Program.cs 확인
# ConventionChatService와 ConventionIndexingService가 등록되어 있는지
builder.Services.AddScoped<ConventionChatService>();
builder.Services.AddScoped<ConventionIndexingService>();
```

### 문제 3: CORS 에러

**증상:**
```
Access to XMLHttpRequest at 'http://localhost:5001/api/...' 
from origin 'http://localhost:3000' has been blocked by CORS policy
```

**원인:**
- 백엔드에서 CORS 설정이 안 됨

**해결책:**

```csharp
// Program.cs에 추가
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ...

app.UseCors("AllowFrontend");
```

### 문제 4: 답변이 생성되지 않음

**증상:**
- 질문 전송은 되지만 답변이 오지 않음
- 로딩 상태가 계속됨

**원인:**
- LLM 프로바이더 설정 오류
- Vector Store에 데이터가 없음

**해결책:**

```bash
# 1. appsettings.json 확인
{
  "LlmSettings": {
    "Provider": "Gemini",  # 또는 "Llama3"
    "Gemini": {
      "ApiKey": "YOUR_API_KEY",  # ← API 키 확인!
      "Model": "gemini-1.5-flash"
    }
  }
}

# 2. 데이터 색인 확인
# Swagger 또는 Postman으로
POST http://localhost:5001/api/conventionchat/reindex

# 3. 백엔드 로그 확인
# 터미널에서 에러 메시지 확인
```

### 문제 5: 추천 질문이 표시되지 않음

**증상:**
- 채팅창은 열리지만 추천 질문이 없음

**원인:**
- Convention 데이터가 로드되지 않음
- API 호출 실패

**해결책:**

```javascript
// 브라우저 콘솔에서 확인
console.log('Convention:', conventionStore.currentConvention)
// null이면 Convention 데이터가 없음

// 수동으로 추천 질문 생성 (임시)
// chat.js 스토어에서
suggestedQuestions.value = [
  '이번 행사는 언제 진행되나요?',
  '참석자는 몇 명인가요?',
  '행사 장소를 알려주세요'
]
```

### 문제 6: 스타일이 깨짐

**증상:**
- 버튼이나 레이아웃이 이상하게 표시됨

**원인:**
- Tailwind CSS가 제대로 로드되지 않음

**해결책:**

```javascript
// main.css 확인
// src/assets/main.css
@tailwind base;
@tailwind components;
@tailwind utilities;

// main.js에서 import 확인
import './assets/main.css'
```

### 문제 7: 메시지가 스크롤되지 않음

**증상:**
- 새 메시지가 추가되지만 스크롤이 안 됨

**해결책:**

```javascript
// ChatWindow.vue에서 watch 확인
watch(
  () => chatStore.messages.length,
  () => {
    scrollToBottom()  // 이 함수가 호출되는지 확인
  }
)

// 브라우저 콘솔에서 테스트
// scrollToBottom() 함수에 console.log 추가
function scrollToBottom() {
  console.log('Scrolling to bottom...')
  nextTick(() => {
    // ...
  })
}
```

---

## 성능 최적화

### 1. 메시지 가상 스크롤

메시지가 많아질 경우 성능 개선:

```bash
npm install vue-virtual-scroller
```

```vue
<!-- ChatWindow.vue -->
<template>
  <RecycleScroller
    :items="chatStore.messages"
    :item-size="100"
    key-field="id"
  >
    <template #default="{ item }">
      <ChatMessage :message="item" />
    </template>
  </RecycleScroller>
</template>
```

### 2. 디바운싱 (입력 최적화)

```javascript
// ChatInput.vue에서
import { debounce } from 'lodash-es'

const debouncedAdjustHeight = debounce(adjustHeight, 100)
```

### 3. 메시지 캐싱

```javascript
// chat.js 스토어에서
const messageCache = new Map()

async function sendMessage(question) {
  const cacheKey = `${currentConventionId.value}:${question}`
  
  if (messageCache.has(cacheKey)) {
    // 캐시된 응답 사용
    const cachedResponse = messageCache.get(cacheKey)
    messages.value.push(cachedResponse)
    return
  }
  
  // ... API 호출
  // 응답을 캐시에 저장
  messageCache.set(cacheKey, assistantMessage)
}
```

---

## 다음 단계

### 단계 1: 기본 테스트 ✅
- [x] 플로팅 버튼 표시 확인
- [x] 채팅 창 열기/닫기
- [x] 질문 전송 및 응답 확인

### 단계 2: 고급 기능 구현 (선택)
- [ ] 음성 입력/출력 (Web Speech API)
- [ ] 파일 업로드 (이미지, 문서)
- [ ] 대화 히스토리 저장 (LocalStorage)
- [ ] 다크 모드 지원

### 단계 3: 프로덕션 배포
- [ ] 환경 변수 설정
- [ ] 빌드 최적화
- [ ] SEO 최적화
- [ ] 모니터링 설정

---

## 요약

✅ **구현 완료:**
1. ✅ ChatService (API 통신)
2. ✅ ChatStore (상태 관리)
3. ✅ ChatWindow (메인 UI)
4. ✅ ChatMessage (메시지 버블)
5. ✅ ChatInput (입력 영역)
6. ✅ SuggestedQuestions (추천 질문)
7. ✅ ChatFloatingButton (플로팅 버튼)
8. ✅ App.vue 통합

✅ **주요 기능:**
- 자연어 질문-답변
- Convention별 컨텍스트
- 멀티턴 대화
- 추천 질문
- 출처 정보 표시
- 반응형 디자인

✅ **사용 방법:**
```bash
# 프론트엔드 실행
cd ClientApp
npm run dev

# 백엔드 실행 (새 터미널)
cd ..
dotnet run

# 브라우저에서
http://localhost:3000
```

**이제 사용자가 자연어로 Convention 정보를 질문하고 AI로부터 답변을 받을 수 있습니다! 🎉**

---

## 문의 및 지원

문제가 발생하면:
1. 브라우저 콘솔 (F12) 확인
2. 백엔드 로그 확인
3. Swagger UI로 API 테스트

Happy Coding! 🚀
