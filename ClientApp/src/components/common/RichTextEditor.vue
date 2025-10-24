<template>
  <div class="quill-editor-wrapper">
    <QuillEditor
      ref="quillEditor"
      v-model:content="content"
      content-type="html"
      :toolbar="toolbarOptions"
      :modules="editorModules"
      @ready="onEditorReady"
      theme="snow"
      :style="{ height: editorHeight }"
    />
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { QuillEditor, Quill } from '@vueup/vue-quill'
import BlotFormatter from 'quill-blot-formatter'
import '@vueup/vue-quill/dist/vue-quill.snow.css'
import apiClient from '@/services/api'

if (!Quill.imports['modules/blotFormatter']) {
  Quill.register('modules/blotFormatter', BlotFormatter)
}

const editorModules = [
  {
    name: 'blotFormatter',
    module: BlotFormatter,
    options: {},
  },
];

const props = defineProps({
  modelValue: { type: String, default: '' },
  height: { type: String, default: '400px' },
  placeholder: { type: String, default: '내용을 입력하세요...' }
})

const emit = defineEmits(['update:modelValue'])

const quillEditor = ref(null)
const content = ref(props.modelValue)
const editorHeight = ref(props.height)

const toolbarOptions = [
  ['bold', 'italic', 'underline', 'strike'],
  ['blockquote', 'code-block'],
  [{ 'header': 1 }, { 'header': 2 }],
  [{ 'list': 'ordered'}, { 'list': 'bullet' }],
  [{ 'indent': '-1'}, { 'indent': '+1' }],
  [{ 'size': ['small', false, 'large', 'huge'] }],
  [{ 'color': [] }, { 'background': [] }],
  [{ 'align': [] }],
  ['link', 'image'],
  ['clean']
]

function onEditorReady(quill) {
  const toolbar = quill.getModule('toolbar')
  toolbar.addHandler('image', imageHandler)
  handlePaste(quill)
  handleDrop(quill)
}

async function imageHandler() {
  const input = document.createElement('input')
  input.setAttribute('type', 'file')
  input.setAttribute('accept', 'image/*')
  input.setAttribute('multiple', 'multiple')
  
  input.onchange = async () => {
    const files = Array.from(input.files)
    if (!files.length) return

    const quill = quillEditor.value.getQuill()
    const range = quill.getSelection()

    for (const file of files) {
      if (file.size > 10 * 1024 * 1024) {
        alert(`${file.name}은(는) 10MB를 초과합니다.`)
        continue
      }

      try {
        const imageUrl = await uploadImage(file)
        quill.insertEmbed(range.index, 'image', imageUrl)
        quill.setSelection(range.index + 1)
      } catch (error) {
        console.error('Image upload failed:', error)
        alert(`${file.name} 업로드 실패`)
      }
    }
  }

  input.click()
}

async function uploadImage(file) {
  const formData = new FormData()
  formData.append('file', file)

  const response = await apiClient.post('/file/upload/image', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })

  return response.data.url
}

function handlePaste(quill) {
  quill.root.addEventListener('paste', async (e) => {
    const clipboardData = e.clipboardData || window.clipboardData
    const items = clipboardData.items

    for (const item of items) {
      if (item.type.indexOf('image') !== -1) {
        e.preventDefault()
        
        const file = item.getAsFile()
        const range = quill.getSelection()

        try {
          const imageUrl = await uploadImage(file)
          quill.insertEmbed(range.index, 'image', imageUrl)
          quill.setSelection(range.index + 1)
        } catch (error) {
          console.error('Paste image upload failed:', error)
        }
      }
    }
  })
}

function handleDrop(quill) {
  quill.root.addEventListener('drop', async (e) => {
    e.preventDefault()
    
    const files = Array.from(e.dataTransfer.files).filter(file => 
      file.type.startsWith('image/')
    )

    if (!files.length) return

    const range = quill.getSelection() || { index: quill.getLength() }

    for (const file of files) {
      try {
        const imageUrl = await uploadImage(file)
        quill.insertEmbed(range.index, 'image', imageUrl)
        quill.setSelection(range.index + 1)
      } catch (error) {
        console.error('Drop image upload failed:', error)
      }
    }
  })

  quill.root.addEventListener('dragover', (e) => e.preventDefault())
}

async function convertBase64ToFile(quill) {
  const imgTags = quill.root.querySelectorAll('img[src^="data:image"]')
  
  for (const img of imgTags) {
    try {
      const base64Data = img.src
      const blob = await fetch(base64Data).then(r => r.blob())
      const file = new File([blob], `paste-${Date.now()}.png`, { type: blob.type })

      const imageUrl = await uploadImage(file)
      img.src = imageUrl
    } catch (error) {
      console.error('Failed to convert base64 image:', error)
    }
  }
}

watch(content, (newValue) => {
  emit('update:modelValue', newValue)
})

watch(() => props.modelValue, (newValue) => {
  if (newValue !== content.value) {
    content.value = newValue
  }
})

defineExpose({
  async beforeSave() {
    const quill = quillEditor.value?.getQuill()
    if (quill) {
      await convertBase64ToFile(quill)
    }
    return content.value
  }
})
</script>

<style>
.quill-editor-wrapper {
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  overflow: hidden;
}

.ql-container {
  font-family: inherit;
  font-size: 14px;
}

:deep(.ql-editor) {
  all: revert;
  min-height: 400px;
  padding: 20px;
}

.ql-editor img {
  max-width: 100%;
  height: auto;
}

.ql-toolbar {
  border-bottom: 1px solid #e5e7eb;
  background-color: #f9fafb;
}

.ql-editor.ql-blank::before {
  color: #9ca3af;
  font-style: normal;
}

.quill-editor-wrapper :deep(.ql-editor ol),
.quill-editor-wrapper :deep(.ql-editor ul) {
  padding-left: 1.5em;
}
.quill-editor-wrapper :deep(.ql-editor ol > li) {
  list-style-type: decimal;
}
.quill-editor-wrapper :deep(.ql-editor ul > li) {
  list-style-type: disc;
}
</style>
