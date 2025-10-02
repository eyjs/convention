# Convention RAG 챗봇 시스템 구축 가이드

## 📋 목차
1. [시스템 개요](#시스템-개요)
2. [아키텍처](#아키텍처)
3. [설정 방법](#설정-방법)
4. [API 사용법](#api-사용법)
5. [프론트엔드 통합](#프론트엔드-통합)
6. [고급 기능](#고급-기능)

---

## 시스템 개요

**Convention RAG 챗봇**은 사용자가 자연어로 질문하면 Convention 데이터베이스에서 관련 정보를 검색하여 답변을 생성하는 시스템입니다.

### 핵심 기능

1. ✅ **자연어 질문-답변**
   - "이번 주 행사가 있나요?"
   - "김철수 참석자 정보 알려줘"
   - "내일 일정이 뭐지?"

2. ✅ **자동 데이터 색인**
   - Convention, Guest, Schedule, Menu 등 모든 데이터 자동 임베딩
   - 실시간 업데이트 지원

3. ✅ **컨텍스트 기반 답변**
   - 관련 정보를 찾아서 정확한 답변 생성
   - 출처 정보 제공

4. ✅ **행사별 필터링**
   - 특정 행사에 대한 질문만 처리 가능

---

## 아키텍처

### 데이터 흐름

```
1. 데이터 색인 (Indexing)
   Convention DB → 텍스트 변환 → 임베딩 생성 → VectorStore 저장

2. 질문-답변 (Query)
   사용자 질문 → 임베딩 생성 → 유사 문서 검색 → LLM 답변 생성
```

### 주요 컴포넌트

1. **ConventionIndexingService**
   - 데이터를 검색 가능한 텍스트로 변환
   - 임베딩 생성 및 저장

2. **ConventionChatService**
   - 사용자 질문 처리
   - RAG 기반 답변 생성

3. **ConventionChatController**
   - REST API 제공
   - 프론트엔드와 통신

---

## 설정 방법

### Step 1: Program.cs에 서비스 등록

```csharp
using LocalRAG.Services;
using LocalRAG.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 기존 설정들...

// Repository 등록
builder.Services.AddRepositories();

// RAG 관련 서비스 (이미 등록되어 있음)
builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
builder.Services.AddSingleton<IEmbeddingService, LocalEmbeddingService>();
builder.Services.AddScoped<IRagService, RagService>();

// ✅ Convention 챗봇 서비스 등록 (추가)
builder.Services.AddScoped<ConventionIndexingService>();
builder.Services.AddScoped<ConventionChatService>();

var app = builder.Build();

// ... 나머지 설정
```

### Step 2: 초기 데이터 색인

애플리케이션 시작 시 자동으로 색인하거나, API로 수동 실행:

```csharp
// Program.cs에 추가 (선택사항)
using (var scope = app.Services.CreateScope())
{
    var indexingService = scope.ServiceProvider
        .GetRequiredService<ConventionIndexingService>();
    var logger = scope.ServiceProvider
        .GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting initial indexing...");
        var result = await indexingService.ReindexAllConventionsAsync();
        logger.LogInformation(
            "Initial indexing completed. Success: {Success}, Failures: {Failures}",
            result.SuccessCount, result.FailureCount);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Initial indexing failed");
    }
}
```

---

## API 사용법

### 1. 일반 질문

**요청:**
```http
POST /api/conventionchat/ask
Content-Type: application/json

{
  "question": "이번 주에 예정된 행사가 있나요?"
}
```

**응답:**
```json
{
  "answer": "네, 이번 주에 '2025 해외 워크샵' 행사가 3월 15일부터 18일까지 예정되어 있습니다.",
  "sources": [
    {
      "content": "행사명: 2025 해외 워크샵\n행사 유형: OVERSEAS\n시작일: 2025년 03월 15일...",
      "similarity": 0.89,
      "type": "convention",
      "conventionId": 1,
      "conventionTitle": "2025 해외 워크샵"
    }
  ],
  "llmProvider": "Gemini",
  "timestamp": "2025-10-02T10:30:00"
}
```

### 2. 특정 행사에 대한 질문

**요청:**
```http
POST /api/conventionchat/conventions/1/ask
Content-Type: application/json

{
  "question": "참석자는 몇 명인가요?"
}
```

**응답:**
```json
{
  "answer": "이번 행사에는 총 50명의 참석자가 등록되어 있습니다.",
  "sources": [...],
  "llmProvider": "Gemini",
  "timestamp": "2025-10-02T10:31:00"
}
```

### 3. 추천 질문 가져오기

**요청:**
```http
GET /api/conventionchat/conventions/1/suggestions
```

**응답:**
```json
[
  "2025 해외 워크샵 행사는 언제 진행되나요?",
  "이번 행사의 담당자는 누구인가요?",
  "행사 일정을 알려주세요",
  "참석자 명단을 알려주세요",
  "참석자는 총 몇 명인가요?"
]
```

### 4. 전체 재색인

**요청:**
```http
POST /api/conventionchat/reindex
```

**응답:**
```json
{
  "success": true,
  "message": "색인 완료. 성공: 10, 실패: 0",
  "result": {
    "successCount": 10,
    "failureCount": 0,
    "errors": [],
    "totalCount": 10
  }
}
```

### 5. 특정 행사 색인

**요청:**
```http
POST /api/conventionchat/conventions/1/index
```

**응답:**
```json
{
  "success": true,
  "message": "Convention 1 색인 완료",
  "documentId": "doc_12345"
}
```

---

## 프론트엔드 통합

### React 예시

```typescript
// ConventionChatbot.tsx
import React, { useState } from 'react';

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

const ConventionChatbot: React.FC<{ conventionId?: number }> = ({ conventionId }) => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);

  const handleAsk = async () => {
    if (!input.trim()) return;

    // 사용자 메시지 추가
    const userMessage: ChatMessage = { role: 'user', content: input };
    setMessages(prev => [...prev, userMessage]);
    setLoading(true);

    try {
      // API 호출
      const url = conventionId 
        ? `/api/conventionchat/conventions/${conventionId}/ask`
        : '/api/conventionchat/ask';

      const response = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ question: input })
      });

      const data = await response.json();

      // 답변 추가
      const assistantMessage: ChatMessage = {
        role: 'assistant',
        content: data.answer
      };
      setMessages(prev => [...prev, assistantMessage]);
    } catch (error) {
      console.error('Error:', error);
    } finally {
      setLoading(false);
      setInput('');
    }
  };

  return (
    <div className="chatbot-container">
      <div className="messages">
        {messages.map((msg, idx) => (
          <div key={idx} className={`message ${msg.role}`}>
            {msg.content}
          </div>
        ))}
        {loading && <div className="loading">답변 생성 중...</div>}
      </div>
      
      <div className="input-area">
        <input
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyPress={(e) => e.key === 'Enter' && handleAsk()}
          placeholder="질문을 입력하세요..."
        />
        <button onClick={handleAsk} disabled={loading}>
          전송
        </button>
      </div>
    </div>
  );
};

export default ConventionChatbot;
```

### Vue 예시

```vue
<!-- ConventionChatbot.vue -->
<template>
  <div class="chatbot-container">
    <div class="messages">
      <div 
        v-for="(msg, idx) in messages" 
        :key="idx" 
        :class="['message', msg.role]"
      >
        {{ msg.content }}
      </div>
      <div v-if="loading" class="loading">답변 생성 중...</div>
    </div>
    
    <div class="input-area">
      <input
        v-model="input"
        @keypress.enter="handleAsk"
        placeholder="질문을 입력하세요..."
      />
      <button @click="handleAsk" :disabled="loading">전송</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

const props = defineProps<{
  conventionId?: number;
}>();

const messages = ref<ChatMessage[]>([]);
const input = ref('');
const loading = ref(false);

const handleAsk = async () => {
  if (!input.value.trim()) return;

  messages.value.push({ role: 'user', content: input.value });
  loading.value = true;

  try {
    const url = props.conventionId
      ? `/api/conventionchat/conventions/${props.conventionId}/ask`
      : '/api/conventionchat/ask';

    const response = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ question: input.value })
    });

    const data = await response.json();
    messages.value.push({ role: 'assistant', content: data.answer });
  } catch (error) {
    console.error('Error:', error);
  } finally {
    loading.value = false;
    input.value = '';
  }
};
</script>
```

---

## 고급 기능

### 1. 실시간 데이터 동기화

Convention 데이터가 변경될 때 자동으로 재색인:

```csharp
// ConventionService.cs에 추가
public class ConventionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ConventionIndexingService _indexingService;

    public async Task<Convention> CreateConventionAsync(Convention convention)
    {
        // 1. Convention 생성
        convention.RegDtm = DateTime.Now;
        convention.DeleteYn = "N";
        await _unitOfWork.Conventions.AddAsync(convention);
        await _unitOfWork.SaveChangesAsync();

        // 2. 자동 색인 (비동기로 실행)
        _ = Task.Run(async () => 
        {
            try
            {
                await _indexingService.IndexConventionAsync(convention.Id);
            }
            catch (Exception ex)
            {
                // 로깅만 하고 메인 작업은 영향 없음
                _logger.LogError(ex, "Failed to index convention {Id}", convention.Id);
            }
        });

        return convention;
    }
}
```

### 2. 커스텀 질문 템플릿

특정 패턴의 질문에 대한 답변 개선:

```csharp
// ConventionChatService.cs에 추가
private string EnhanceQuestion(string question)
{
    // 질문 패턴 분석 및 개선
    if (question.Contains("참석자") && !question.Contains("누구"))
    {
        return $"{question} 참석자 이름과 정보를 알려주세요.";
    }

    if (question.Contains("일정") && !question.Contains("언제"))
    {
        return $"{question} 날짜와 시간을 포함해서 알려주세요.";
    }

    return question;
}
```

### 3. 답변 품질 개선

프롬프트 엔지니어링으로 답변 품질 향상:

```csharp
// RagService.cs 수정
public async Task<RagResponse> QueryAsync(string question, int topK = 5)
{
    var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);
    var searchResults = await _vectorStore.SearchAsync(queryEmbedding, topK);
    
    // 개선된 프롬프트
    var context = string.Join("\n\n", searchResults.Select((r, i) => 
        $"[출처 {i + 1}]\n{r.Content}"));
    
    var enhancedPrompt = $@"
다음 정보를 바탕으로 질문에 답변해주세요.

질문: {question}

참고 정보:
{context}

답변 시 주의사항:
1. 참고 정보에 있는 내용만 사용하세요
2. 확실하지 않으면 '정보가 부족합니다'라고 답하세요
3. 답변은 한국어로, 친절하게 작성하세요
4. 날짜나 숫자는 정확하게 표기하세요

답변:";

    var answer = await _llmProvider.GenerateResponseAsync(enhancedPrompt);
    
    return new RagResponse(answer, searchResults, _llmProvider.ProviderName);
}
```

### 4. 캐싱으로 성능 개선

자주 묻는 질문은 캐싱:

```csharp
// ConventionChatService.cs에 추가
private readonly IMemoryCache _cache;

public async Task<ChatResponse> AskAsync(string question, int? conventionId = null)
{
    var cacheKey = $"chat:{conventionId}:{question.ToLower()}";
    
    if (_cache.TryGetValue(cacheKey, out ChatResponse? cachedResponse))
    {
        return cachedResponse!;
    }

    var response = await _ragService.QueryAsync(question, topK: 5);
    
    // 캐시 저장 (5분)
    _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
    
    return response;
}
```

---

## 질문 예시

### 행사 관련
- "다음 주에 예정된 행사가 있나요?"
- "2025년 3월에 있는 행사 목록 알려줘"
- "해외 워크샵은 언제 시작하나요?"
- "이번 행사 담당자 연락처 알려줘"

### 참석자 관련
- "김철수 참석자 정보 알려줘"
- "영업부 참석자는 몇 명이야?"
- "참석자 중에 동반자가 있는 사람은?"

### 일정 관련
- "내일 일정이 뭐야?"
- "3월 15일에 뭐하지?"
- "오전 9시에 시작하는 일정 알려줘"
- "A조 일정만 보여줘"

### 메뉴/섹션 관련
- "행사 안내 자료 내용 알려줘"
- "숙박 정보는 어디서 확인해?"

---

## 트러블슈팅

### 문제 1: 답변이 부정확함
```
해결책:
1. topK 값 증가 (더 많은 문서 검색)
2. 임베딩 모델 변경
3. 프롬프트 개선
```

### 문제 2: 색인이 느림
```
해결책:
1. 배치 처리로 변경
2. 백그라운드 작업으로 실행
3. 병렬 처리 활용
```

### 문제 3: 메모리 부족
```
해결책:
1. VectorStore를 DB 기반으로 변경
2. 페이징 처리
3. 오래된 데이터 삭제
```

---

## 다음 단계

### 1단계: 기본 구현
- [x] 데이터 색인 서비스
- [x] 질문-답변 서비스
- [x] REST API

### 2단계: 고급 기능
- [ ] 대화 히스토리 관리
- [ ] 멀티턴 대화 지원
- [ ] 음성 인식/TTS 통합

### 3단계: 최적화
- [ ] 답변 품질 평가 시스템
- [ ] A/B 테스트
- [ ] 사용자 피드백 수집

---

## 요약

✅ **구현 완료:**
1. ConventionIndexingService - 데이터 색인
2. ConventionChatService - 질문-답변
3. ConventionChatController - REST API
4. 프론트엔드 예시 코드

✅ **사용 방법:**
1. Program.cs에 서비스 등록
2. 초기 데이터 색인
3. API 호출로 질문-답변
4. 프론트엔드 통합

✅ **주요 API:**
- POST /api/conventionchat/ask - 일반 질문
- POST /api/conventionchat/conventions/{id}/ask - 특정 행사 질문
- GET /api/conventionchat/conventions/{id}/suggestions - 추천 질문
- POST /api/conventionchat/reindex - 전체 재색인

**이제 사용자가 자연어로 Convention 데이터에 대해 질문하고 답변을 받을 수 있습니다! 🎉**
