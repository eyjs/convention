# 모바일 키보드 대응 가이드

## 개요
모바일 환경에서 입력 필드에 포커스할 때 키보드가 올라오면서 입력창이 가려지는 문제를 자동으로 해결하는 시스템입니다.

## 주요 기능

### 1. 자동 스크롤 조정
- 입력 필드 포커스 시 자동으로 스크롤하여 키보드 위로 입력창 표시
- iOS Safari, Android Chrome 등 모든 모바일 브라우저 지원
- Visual Viewport API를 활용한 정확한 키보드 높이 감지

### 2. iOS 최적화
- 자동 줌 방지 (font-size: 16px 이상 적용)
- Safe Area 대응
- Fixed 요소 처리

### 3. 터치 영역 최적화
- 최소 44x44px 터치 영역 보장
- 버튼, 링크, 체크박스 등 모든 인터랙티브 요소에 적용

## 설치 및 설정

### 1. Composable 추가 (✅ 완료)
`src/composables/useKeyboardAdjust.js` 파일이 생성되었습니다.

### 2. 전역 CSS 스타일 (✅ 완료)
`src/assets/main.css`에 키보드 대응 스타일이 추가되었습니다.

### 3. App.vue에 적용 (✅ 완료)
```vue
import { useKeyboardAdjust } from '@/composables/useKeyboardAdjust'

// 전역 키보드 대응 활성화
const { isKeyboardVisible } = useKeyboardAdjust({
  offset: 20,        // 키보드 위 여백 (px)
  duration: 300,     // 스크롤 애니메이션 시간 (ms)
  enabled: true      // 항상 활성화
})
```

## 사용 방법

### 기본 사용 (자동 적용)
특별한 설정 없이 모든 input, textarea 요소에 자동으로 적용됩니다.

```vue
<template>
  <div>
    <input 
      v-model="email" 
      type="email" 
      placeholder="이메일 입력"
    />
    
    <textarea 
      v-model="message" 
      rows="5"
      placeholder="메시지 입력"
    />
  </div>
</template>
```

### 플로팅 버튼 사용
하단 고정 버튼이 키보드에 가려지지 않도록 처리:

```vue
<template>
  <div class="floating-action">
    <button>제출하기</button>
  </div>
</template>
```

CSS에서 자동으로 키보드 높이만큼 올라갑니다.

### 컨테이너에 적용
전체 페이지를 키보드 대응 컨테이너로 감싸기:

```vue
<template>
  <div class="keyboard-aware-container">
    <!-- 콘텐츠 -->
  </div>
</template>
```

### 개별 컴포넌트에서 사용
특정 컴포넌트에서만 키보드 상태를 확인하거나 제어하려면:

```vue
<script setup>
import { useKeyboardAdjust } from '@/composables/useKeyboardAdjust'
import { watch } from 'vue'

const { isKeyboardVisible, scrollToElement } = useKeyboardAdjust({
  offset: 30,        // 더 큰 여백
  enabled: true
})

// 키보드 상태에 따라 UI 변경
watch(isKeyboardVisible, (visible) => {
  if (visible) {
    console.log('키보드가 올라왔습니다')
    // 하단 네비게이션 숨기기 등
  }
})

// 수동으로 특정 요소 스크롤
const scrollToInput = () => {
  const element = document.querySelector('#my-input')
  scrollToElement(element, true)
}
</script>

<template>
  <div>
    <!-- 키보드 상태에 따라 조건부 렌더링 -->
    <div v-if="!isKeyboardVisible" class="bottom-nav">
      <!-- 하단 네비게이션 -->
    </div>
  </div>
</template>
```

## 실전 예제

### 예제 1: 로그인 폼

```vue
<template>
  <div class="min-h-screen flex items-center justify-center p-6">
    <div class="w-full max-w-md space-y-6">
      <h1 class="text-2xl font-bold text-center">로그인</h1>
      
      <div class="input-wrapper">
        <label class="block text-sm font-medium mb-2">아이디</label>
        <input
          v-model="loginId"
          type="text"
          class="w-full px-4 py-3 border rounded-lg"
          placeholder="아이디를 입력하세요"
        />
      </div>
      
      <div class="input-wrapper">
        <label class="block text-sm font-medium mb-2">비밀번호</label>
        <input
          v-model="password"
          type="password"
          class="w-full px-4 py-3 border rounded-lg"
          placeholder="비밀번호를 입력하세요"
        />
      </div>
      
      <button class="w-full py-3 bg-primary-600 text-white rounded-lg">
        로그인
      </button>
    </div>
  </div>
</template>
```

### 예제 2: 댓글 입력

```vue
<template>
  <div class="keyboard-aware-container">
    <!-- 댓글 목록 -->
    <div class="space-y-4 p-4">
      <div v-for="comment in comments" :key="comment.id">
        {{ comment.text }}
      </div>
    </div>
    
    <!-- 하단 고정 댓글 입력창 -->
    <div class="floating-action left-0 right-0 bg-white border-t p-4">
      <div class="flex gap-2">
        <input
          v-model="newComment"
          type="text"
          placeholder="댓글을 입력하세요"
          class="flex-1 px-4 py-2 border rounded-lg"
          @keyup.enter="addComment"
        />
        <button
          @click="addComment"
          class="px-6 py-2 bg-primary-600 text-white rounded-lg"
        >
          전송
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const newComment = ref('')
const comments = ref([])

const addComment = () => {
  if (!newComment.value.trim()) return
  
  comments.value.push({
    id: Date.now(),
    text: newComment.value
  })
  
  newComment.value = ''
}
</script>
```

### 예제 3: 설문조사 폼

```vue
<template>
  <div class="keyboard-aware-container p-6">
    <h2 class="text-xl font-bold mb-6">행사 만족도 조사</h2>
    
    <div class="space-y-6">
      <!-- 객관식 질문 -->
      <div class="input-wrapper">
        <label class="block font-medium mb-3">행사 만족도</label>
        <div class="space-y-2">
          <label
            v-for="option in satisfactionOptions"
            :key="option.value"
            class="flex items-center gap-3 p-3 border rounded-lg cursor-pointer hover:bg-gray-50"
          >
            <input
              v-model="survey.satisfaction"
              type="radio"
              :value="option.value"
              class="w-5 h-5"
            />
            <span>{{ option.label }}</span>
          </label>
        </div>
      </div>
      
      <!-- 주관식 질문 -->
      <div class="input-wrapper">
        <label class="block font-medium mb-2">개선 의견</label>
        <textarea
          v-model="survey.feedback"
          rows="5"
          class="w-full px-4 py-3 border rounded-lg"
          placeholder="자유롭게 의견을 작성해주세요"
        ></textarea>
      </div>
    </div>
    
    <!-- 제출 버튼 -->
    <div class="floating-action right-6">
      <button
        @click="submitSurvey"
        class="px-8 py-3 bg-primary-600 text-white rounded-lg shadow-lg"
      >
        제출하기
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const survey = ref({
  satisfaction: '',
  feedback: ''
})

const satisfactionOptions = [
  { value: 5, label: '매우 만족' },
  { value: 4, label: '만족' },
  { value: 3, label: '보통' },
  { value: 2, label: '불만족' },
  { value: 1, label: '매우 불만족' }
]

const submitSurvey = () => {
  console.log('설문 제출:', survey.value)
  alert('설문이 제출되었습니다!')
}
</script>
```

## 문제 해결

### iOS에서 입력창이 여전히 가려지는 경우

1. **font-size 확인**: 16px 미만이면 자동 줌이 발생합니다.
```css
input {
  font-size: 16px; /* 필수 */
}
```

2. **viewport 메타태그 확인**: index.html에 올바른 설정이 있는지 확인
```html
<meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
```

3. **safe-area 적용**: iOS 노치 대응
```css
.container {
  padding-bottom: env(safe-area-inset-bottom);
}
```

### Android에서 키보드가 레이아웃을 밀어올리는 경우

1. **CSS로 최대 높이 제한**:
```css
.main-content {
  max-height: 100vh;
  overflow-y: auto;
}
```

### 플로팅 버튼이 키보드에 가려지는 경우

`floating-action` 클래스가 제대로 적용되었는지 확인:
```vue
<div class="floating-action">
  <button>버튼</button>
</div>
```

### 성능 최적화

1. **이벤트 리스너 최소화**: 전역에서 한 번만 적용 (App.vue)

2. **조건부 활성화**: 데스크톱에서는 비활성화
```javascript
const isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent)

useKeyboardAdjust({
  enabled: isMobile
})
```

## API 레퍼런스

### useKeyboardAdjust(options)

**매개변수:**
- `options.offset` (number): 키보드 위 여백 (기본값: 20)
- `options.duration` (number): 애니메이션 시간 (기본값: 300)
- `options.enabled` (boolean): 기능 활성화 (기본값: true)

**반환값:**
```typescript
{
  isKeyboardVisible: Ref<boolean>,  // 키보드 표시 여부
  activeElement: Ref<Element>,      // 현재 포커스된 요소
  scrollToElement: (element, animated) => void  // 수동 스크롤
}
```

### CSS 클래스

| 클래스 | 설명 |
|--------|------|
| `keyboard-visible` | body에 자동 적용 (키보드 표시 시) |
| `keyboard-aware-container` | 키보드 높이를 고려한 컨테이너 |
| `input-wrapper` | 입력 필드 래퍼 (자동 여백) |
| `floating-action` | 플로팅 버튼 (키보드 대응) |
| `fixed-header` | 고정 헤더 (iOS 처리) |
| `fixed-footer` | 고정 푸터 (iOS 처리) |
| `main-content` | 메인 콘텐츠 영역 |

## 브라우저 지원

- ✅ iOS Safari 13+
- ✅ Android Chrome 88+
- ✅ Samsung Internet 14+
- ✅ Firefox Mobile 88+
- ⚠️ 구형 브라우저는 fallback 동작

## 체크리스트

폼 개발 시 확인사항:

- [ ] 모든 입력 필드 font-size 16px 이상
- [ ] input-wrapper 클래스로 각 필드 감싸기
- [ ] 플로팅 버튼에 floating-action 클래스 적용
- [ ] 키보드 올라올 때 하단 여백 충분한지 확인
- [ ] 실제 모바일 기기에서 테스트
- [ ] iOS Safari와 Android Chrome 모두 테스트
- [ ] 가로/세로 모드 전환 테스트

## 추가 리소스

- [Visual Viewport API 문서](https://developer.mozilla.org/en-US/docs/Web/API/Visual_Viewport_API)
- [iOS Safe Area 가이드](https://webkit.org/blog/7929/designing-websites-for-iphone-x/)
- [모바일 웹 폼 Best Practices](https://developers.google.com/web/fundamentals/design-and-ux/input/forms)

---

**작성일**: 2024-10-21  
**버전**: 1.0.0  
**담당자**: 프론트엔드팀
