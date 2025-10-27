# 📝 타이포그래피 시스템 가이드

> **업데이트**: 2025-10-27
> **버전**: 1.0.0
> **담당**: Claude Code

---

## 📋 목차

1. [개요](#개요)
2. [Font Size Scale](#font-size-scale)
3. [사용 가이드](#사용-가이드)
4. [반응형 패턴](#반응형-패턴)
5. [컴포넌트별 권장 사항](#컴포넌트별-권장-사항)
6. [Rich Text Content (Prose)](#rich-text-content-prose)
7. [주의사항](#주의사항)

---

## 🎯 개요

이 프로젝트는 **Mobile-First** 접근법을 사용하는 중앙집중식 타이포그래피 시스템을 적용합니다.

### 핵심 원칙

- **일관성**: 모든 페이지에서 동일한 폰트 크기 적용
- **가독성**: Mobile 15px, Desktop 16px 기본 크기
- **접근성**: WCAG 2.1 AA 준수
- **확장성**: Tailwind Config 기반 관리

### 기술 스택

- Tailwind CSS 3.3.3
- @tailwindcss/typography 플러그인
- Pretendard Variable 폰트

---

## 📏 Font Size Scale

### 기본 크기 정의

| Class | Size (rem) | Size (px) | Line Height | 용도 |
|-------|-----------|-----------|-------------|------|
| `text-xs` | 0.75rem | 12px | 1.5 | Badge, Tag, Caption |
| `text-sm` | 0.875rem | 14px | 1.6 | Small Text, Form Helper |
| `text-base` | 0.9375rem | **15px** | 1.7 | **Body Text (Mobile)** |
| `text-lg` | 1rem | 16px | 1.75 | Body Text (Desktop) |
| `text-xl` | 1.125rem | 18px | 1.75 | Subtitle, Card Title |
| `text-2xl` | 1.25rem | 20px | 1.6 | Section Title |
| `text-3xl` | 1.5rem | 24px | 1.5 | Page Title (H1) |
| `text-4xl` | 1.875rem | 30px | 1.4 | Large Title |
| `text-5xl` | 2.25rem | 36px | 1.3 | Hero Title |
| `text-6xl` | 3rem | 48px | 1.2 | Display Title |

### 자동 적용

- **Body 기본**: 모든 페이지의 body는 자동으로 `text-base` (15px) 적용
- **Headings**: h1~h6 태그는 자동으로 적절한 크기 적용
- **입력 필드**: input/textarea/select는 최소 16px (iOS 줌 방지)

---

## 💡 사용 가이드

### 기본 사용

```vue
<!-- Body Text -->
<p class="text-base">본문 텍스트입니다.</p>

<!-- Small Text -->
<span class="text-sm">작은 텍스트</span>

<!-- Large Text -->
<div class="text-lg">큰 텍스트</div>
```

### 제목 (Headings)

```vue
<!-- HTML 태그 사용 (권장) -->
<h1>페이지 제목</h1>  <!-- 자동으로 text-3xl 적용 -->
<h2>섹션 제목</h2>      <!-- 자동으로 text-2xl 적용 -->

<!-- 또는 클래스 사용 -->
<div class="text-3xl font-bold">페이지 제목</div>
```

---

## 📱 반응형 패턴

### Mobile-First 원칙

모든 타이포그래피는 Mobile 크기부터 시작합니다.

```vue
<!-- 기본: Mobile에서 text-base (15px) -->
<p class="text-base">
  이 텍스트는 모바일에서 15px입니다.
</p>

<!-- 반응형: Mobile 15px → Desktop 16px -->
<p class="text-base lg:text-lg">
  모바일 15px, 데스크톱 16px
</p>
```

### 권장 반응형 패턴

#### 제목 (Title)
```vue
<!-- Mobile 20px → Desktop 24px -->
<h1 class="text-2xl md:text-3xl font-bold">
  페이지 제목
</h1>
```

#### 부제목 (Subtitle)
```vue
<!-- Mobile 15px → Desktop 18px -->
<h2 class="text-base md:text-xl font-semibold">
  섹션 제목
</h2>
```

#### 본문 (Body)
```vue
<!-- Mobile 14px → Desktop 15px -->
<p class="text-sm md:text-base">
  본문 내용
</p>

<!-- 또는 기본 크기 유지 -->
<p class="text-base">
  모든 디바이스에서 15px
</p>
```

#### 버튼 (Button)
```vue
<!-- Mobile 14px → Desktop 16px -->
<button class="text-sm md:text-base">
  클릭하세요
</button>
```

---

## 🧩 컴포넌트별 권장 사항

### Modal (모달)

```vue
<template>
  <div class="fixed inset-0 ...">
    <div class="bg-white ...">
      <!-- Header -->
      <div class="px-4 md:px-6 py-4">
        <h2 class="text-lg md:text-xl font-bold">모달 제목</h2>
      </div>

      <!-- Body -->
      <div class="px-4 md:px-6 py-6">
        <p class="text-sm md:text-base">모달 본문</p>
      </div>

      <!-- Footer -->
      <div class="px-4 md:px-6 py-4">
        <button class="text-sm md:text-base">확인</button>
      </div>
    </div>
  </div>
</template>
```

### Card (카드)

```vue
<template>
  <div class="bg-white rounded-lg p-4 md:p-6">
    <!-- Title -->
    <h3 class="text-base md:text-lg font-semibold mb-2">
      카드 제목
    </h3>

    <!-- Content -->
    <p class="text-sm md:text-base text-gray-600">
      카드 내용
    </p>

    <!-- Metadata -->
    <div class="text-xs text-gray-500 mt-3">
      메타 정보
    </div>
  </div>
</template>
```

### Form (폼)

```vue
<template>
  <form>
    <!-- Label -->
    <label class="text-sm font-medium text-gray-700">
      이메일
    </label>

    <!-- Input (자동으로 16px 적용됨) -->
    <input
      type="email"
      class="text-base"
      placeholder="email@example.com"
    />

    <!-- Helper Text -->
    <p class="text-xs text-gray-500 mt-1">
      이메일 주소를 입력하세요
    </p>
  </form>
</template>
```

### Table (테이블)

```vue
<template>
  <table>
    <thead>
      <tr>
        <!-- Header: 소문자 + 굵게 -->
        <th class="text-xs uppercase font-semibold">
          이름
        </th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <!-- Cell: 보통 크기 -->
        <td class="text-sm">
          홍길동
        </td>
      </tr>
    </tbody>
  </table>
</template>
```

### Badge / Tag

```vue
<!-- Badge -->
<span class="text-xs px-2 py-1 bg-blue-100 rounded">
  New
</span>

<!-- Tag -->
<span class="text-xs font-medium px-2 py-0.5 bg-gray-200 rounded-full">
  태그
</span>
```

---

## 📚 Rich Text Content (Prose)

### Quill 에디터 또는 마크다운 콘텐츠

Rich text 콘텐츠는 **Prose 클래스**를 사용합니다.

```vue
<template>
  <div>
    <!-- 방법 1: Prose 클래스 (권장) -->
    <div class="prose prose-sm lg:prose-base max-w-none" v-html="content"></div>

    <!-- 방법 2: Quill 전용 클래스 (전역 스타일 적용됨) -->
    <div class="ql-editor-readonly" v-html="quillContent"></div>
  </div>
</template>
```

### Prose 크기 변형

| Class | Mobile | Desktop | 용도 |
|-------|--------|---------|------|
| `prose-sm` | 14px | 15px | 작은 콘텐츠 |
| `prose` (또는 `prose-base`) | 15px | 16px | **기본** |
| `prose-lg` | 16px | 18px | 큰 콘텐츠 |
| `prose-xl` | 18px | 20px | 강조 콘텐츠 |

### 반응형 Prose

```vue
<!-- Mobile: 작게 (14px) → Desktop: 보통 (16px) -->
<div class="prose prose-sm md:prose-base max-w-none">
  {{ content }}
</div>

<!-- Mobile: 보통 (15px) → Desktop: 크게 (18px) -->
<div class="prose prose-base md:prose-lg max-w-none">
  {{ content }}
</div>
```

---

## ⚠️ 주의사항

### 1. 절대 고정 크기 사용 금지

```vue
<!-- ❌ 잘못된 예 -->
<p style="font-size: 16px;">텍스트</p>
<p class="text-[16px]">텍스트</p>

<!-- ✅ 올바른 예 -->
<p class="text-base">텍스트</p>
<p class="text-sm md:text-base">텍스트</p>
```

### 2. 입력 필드는 최소 16px

iOS Safari에서 16px 미만의 입력 필드는 자동 줌이 발생합니다.

```vue
<!-- ✅ 자동으로 16px 이상 유지됨 -->
<input class="text-base" />  <!-- Mobile/Desktop 모두 16px 이상 -->
```

### 3. Line Height 자동 적용

fontSize 설정 시 line-height가 자동으로 적용되므로 별도 지정 불필요.

```vue
<!-- ✅ line-height 자동 적용 -->
<p class="text-base">자동으로 1.7 line-height</p>

<!-- ❌ 불필요한 중복 -->
<p class="text-base leading-7">불필요</p>
```

### 4. Heading 태그 우선 사용

의미론적 HTML을 위해 가능한 h1~h6 태그를 사용하세요.

```vue
<!-- ✅ 권장: 의미론적 -->
<h1>페이지 제목</h1>

<!-- ❌ 비권장: 비의미론적 -->
<div class="text-3xl font-bold">페이지 제목</div>
```

### 5. max-width 제거

Prose 클래스는 기본적으로 max-width를 가지므로 제거해야 합니다.

```vue
<!-- ✅ max-w-none 필수 -->
<div class="prose max-w-none">
  {{ content }}
</div>
```

---

## 🔄 마이그레이션 가이드

### 기존 코드 → 새 타이포그래피 시스템

#### Before (기존)

```vue
<template>
  <div>
    <h1 class="text-2xl">제목</h1>
    <p class="text-base">본문</p>
  </div>
</template>

<style scoped>
.ql-editor-readonly {
  font-size: 16px;
  line-height: 1.8;
}
</style>
```

#### After (개선)

```vue
<template>
  <div>
    <!-- 반응형 제목 -->
    <h1 class="text-2xl md:text-3xl">제목</h1>

    <!-- 반응형 본문 -->
    <p class="text-sm md:text-base">본문</p>

    <!-- Prose 클래스 사용 -->
    <div class="prose prose-sm lg:prose-base max-w-none" v-html="content"></div>
  </div>
</template>

<!-- style scoped 제거 - 전역 스타일 사용 -->
```

---

## 📞 문의

타이포그래피 시스템 관련 문의:
- 슬랙: #frontend-team
- 담당자: Claude Code
- 문서 업데이트: 변경 사항 발생 시 이 문서도 함께 업데이트

---

**마지막 업데이트**: 2025-10-27
**다음 리뷰 예정**: Phase 2 완료 시
