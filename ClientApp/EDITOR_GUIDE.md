## 설치 명령어

```bash
npm install quill@2.0.2 @vueup/vue-quill@1.2.0
```

## 에디터 사용법

BoardManagement.vue에서:

```vue
<script setup>
import RichTextEditor from '@/components/common/RichTextEditor.vue'

const editor = ref(null)

async function savePost() {
  // 저장 전 Base64 이미지를 서버 업로드로 변환
  if (editor.value) {
    postForm.value.content = await editor.value.beforeSave()
  }
  
  // 저장 로직...
}
</script>

<template>
  <!-- textarea 대신 RichTextEditor 사용 -->
  <RichTextEditor ref="editor" v-model="postForm.content" height="500px" />
  
  <!-- 상세보기에서 HTML 렌더링 -->
  <div v-html="viewingPost.content"></div>
</template>
```

## 파일 구조

에디터가 `/upload/image` API를 호출하며, 서버는 다음 경로에 저장:
`D:\Home\20250114\이미지파일명.확장자`
