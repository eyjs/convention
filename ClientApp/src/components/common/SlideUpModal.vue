<template>
  <Transition name="slide-up">
    <div
      v-if="isOpen"
      class="fixed inset-0 bg-black/60 backdrop-blur-sm flex justify-center items-end"
      :class="zIndexClass"
      @mousedown.self="onMouseDown"
      @mouseup.self="onMouseUp"
    >
      <div
        class="bg-white rounded-t-2xl shadow-2xl w-full max-h-[90vh] flex flex-col"
      >
        <!-- Header -->
        <header
          class="relative px-4 py-4 md:px-6 md:py-4 border-b flex items-center justify-between flex-shrink-0"
        >
          <div class="w-8 md:w-10">
            <slot name="header-left"></slot>
          </div>
          <h2
            class="absolute left-1/2 -translate-x-1/2 text-lg md:text-xl font-bold text-gray-900 text-center truncate px-8"
          >
            <slot name="header-title"></slot>
          </h2>
          <button
            @click="close"
            class="p-2.5 md:p-2 -mr-2 hover:bg-gray-100 active:bg-gray-200 rounded-lg z-10 text-gray-500 transition-colors"
          >
            <svg
              class="w-6 h-6 md:w-5 md:h-5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </header>

        <!-- Body -->
        <main class="flex-1 overflow-y-auto p-4 md:p-6">
          <slot name="body"></slot>
        </main>

        <!-- Footer -->
        <footer v-if="$slots.footer" class="px-4 py-4 md:px-6 md:py-4 border-t flex-shrink-0">
          <slot name="footer"></slot>
        </footer>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, watch, onUnmounted } from 'vue'

const props = defineProps({
  isOpen: Boolean,
  zIndexClass: {
    type: String,
    default: 'z-50',
  },
})

const emit = defineEmits(['close'])

const startPos = ref({ x: 0, y: 0 })

// 모달 열림/닫힘 시 body 스크롤 제어
watch(() => props.isOpen, (newValue) => {
  if (newValue) {
    // 모달이 열릴 때 body 스크롤 막기
    document.body.style.overflow = 'hidden'
    document.body.style.touchAction = 'none'
  } else {
    // 모달이 닫힐 때 body 스크롤 복원
    document.body.style.overflow = ''
    document.body.style.touchAction = ''
  }
})

// 컴포넌트가 언마운트될 때 스크롤 복원
onUnmounted(() => {
  document.body.style.overflow = ''
  document.body.style.touchAction = ''
})

const onMouseDown = (e) => {
  startPos.value = { x: e.clientX, y: e.clientY }
}

const onMouseUp = (e) => {
  const dx = Math.abs(e.clientX - startPos.value.x)
  const dy = Math.abs(e.clientY - startPos.value.y)
  if (dx < 5 && dy < 5) {
    close()
  }
}

const close = () => {
  emit('close')
}
</script>

<style scoped>
.slide-up-enter-active,
.slide-up-leave-active {
  transition: all 0.3s ease-out;
}

.slide-up-enter-from,
.slide-up-leave-to {
  opacity: 0;
  transform: translateY(100%);
}

.slide-up-enter-to,
.slide-up-leave-from {
  opacity: 1;
  transform: translateY(0);
}
</style>
