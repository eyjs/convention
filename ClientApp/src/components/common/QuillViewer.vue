<template>
  <div ref="editorRef" class="ql-viewer"></div>
</template>

<script setup>
import 'quill/dist/quill.bubble.css'
import { ref, watch, onMounted } from 'vue'
import { useQuillEditor } from '@/composables/useQuillEditor'

const props = defineProps({
  content: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['image-clicked'])

const { editorRef, setContent } = useQuillEditor({
  readOnly: true,
  theme: 'bubble',
  placeholder: '',
  modules: {
    blotFormatter: false
  }
})

// Use watch to set content when the ref is available and content is passed.
watch(() => [editorRef.value, props.content], ([newEl, newContent]) => {
  if (newEl) {
    // Set content
    setContent(newContent)

    // Add click listener for images
    newEl.addEventListener('click', (event) => {
      if (event.target.tagName === 'IMG') {
        emit('image-clicked', event.target.src)
      }
    })
  }
}, { immediate: true, deep: true })

</script>

<style>
.ql-viewer :deep(.ql-editor) {
  all: revert;
}
.ql-bubble .ql-editor {
    border: none;
    padding: 0;
}

.ql-viewer :deep(.ql-editor ol),
.ql-viewer :deep(.ql-editor ul) {
  padding-left: 1.5em;
}
.ql-viewer :deep(.ql-editor ol > li) {
  list-style-type: decimal;
}
.ql-viewer :deep(.ql-editor ul > li) {
  list-style-type: disc;
}
</style>
