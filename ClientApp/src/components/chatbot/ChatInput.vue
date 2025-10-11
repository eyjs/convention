<template>
  <div class="border-t border-gray-200 bg-white px-6 py-4">
    <div class="flex items-end space-x-3 max-w-4xl mx-auto">
      <div class="flex-1 relative">
        <textarea
          ref="inputRef"
          v-model="inputText"
          @keydown.enter="handleEnter"
          @input="adjustHeight"
          :disabled="disabled"
          :placeholder="placeholder"
          rows="1"
          class="w-full resize-none rounded-3xl border border-gray-300 px-6 py-4 text-base focus:outline-none focus:border-gray-400 disabled:bg-gray-100 disabled:cursor-not-allowed transition-colors shadow-sm"
          style="max-height: 200px; min-height: 52px;"
        ></textarea>
      </div>

      <button
        @click="handleSend"
        :disabled="!canSend"
        :class="[
          'flex-shrink-0 w-12 h-12 rounded-full flex items-center justify-center transition-all',
          canSend
            ? 'bg-black text-white hover:bg-gray-800 active:scale-95'
            : 'bg-gray-200 text-gray-400 cursor-not-allowed'
        ]"
      >
        <svg 
          v-if="!loading"
          class="w-5 h-5" 
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24"
        >
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 10l7-7m0 0l7 7m-7-7v18" />
        </svg>
        
        <svg 
          v-else
          class="w-5 h-5 animate-spin" 
          fill="none" 
          viewBox="0 0 24 24"
        >
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, nextTick, watch } from 'vue'

const props = defineProps({
  loading: {
    type: Boolean,
    default: false
  },
  disabled: {
    type: Boolean,
    default: false
  },
  placeholder: {
    type: String,
    default: '메시지를 입력하세요...'
  },
  maxLength: {
    type: Number,
    default: 2000
  }
})

const emit = defineEmits(['send'])

const inputText = ref('')
const inputRef = ref(null)

const canSend = computed(() => {
  return (
    inputText.value.trim().length > 0 &&
    !props.loading &&
    !props.disabled &&
    inputText.value.length <= props.maxLength
  )
})

function handleSend() {
  if (!canSend.value) return
  
  const message = inputText.value.trim()
  if (!message) return
  
  emit('send', message)
  
  inputText.value = ''
  
  nextTick(() => {
    adjustHeight()
    inputRef.value?.focus()
  })
}

function handleEnter(event) {
  if (event.shiftKey) {
    return
  }
  
  event.preventDefault()
  handleSend()
}

function adjustHeight() {
  const textarea = inputRef.value
  if (!textarea) return
  
  textarea.style.height = 'auto'
  
  const newHeight = Math.min(textarea.scrollHeight, 200)
  textarea.style.height = `${newHeight}px`
}

function focus() {
  inputRef.value?.focus()
}

function clear() {
  inputText.value = ''
  adjustHeight()
}

watch(() => props.loading, (isLoading) => {
  if (!isLoading) {
    nextTick(() => {
      inputRef.value?.focus()
    })
  }
})

defineExpose({
  focus,
  clear
})
</script>

<style scoped>
textarea::-webkit-scrollbar {
  width: 4px;
}

textarea::-webkit-scrollbar-track {
  background: transparent;
}

textarea::-webkit-scrollbar-thumb {
  background: #d1d5db;
  border-radius: 2px;
}

textarea::-webkit-scrollbar-thumb:hover {
  background: #9ca3af;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

.animate-spin {
  animation: spin 1s linear infinite;
}
</style>
