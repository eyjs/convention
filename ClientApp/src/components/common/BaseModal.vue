<template>
  <Transition name="modal-fade">
    <div
      v-if="isOpen"
      class="fixed inset-0 bg-black/60 backdrop-blur-sm flex justify-center items-end md:items-center z-50 md:p-4"
      @mousedown.self="onMouseDown"
      @mouseup.self="onMouseUp"
      @touchstart.self="onTouchStart"
      @touchend.self="onTouchEnd"
    >
      <div
        class="bg-white rounded-t-3xl md:rounded-2xl shadow-2xl w-full max-h-[90vh] flex flex-col overflow-hidden modal-content"
        :class="maxWidthClass"
      >
        <!-- Header -->
        <header
          v-if="$slots.header"
          class="px-4 py-4 md:px-6 md:py-4 border-b flex items-center justify-between flex-shrink-0"
        >
          <slot name="header"></slot>
          <button
            @click="close"
            class="p-2.5 md:p-2 hover:bg-gray-100 active:bg-gray-200 rounded-lg text-gray-500 flex-shrink-0 transition-colors"
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
        <main class="p-4 md:p-6 overflow-y-auto overflow-x-hidden flex-1 min-h-0">
          <slot name="body"></slot>
        </main>

        <!-- Footer -->
        <footer
          v-if="$slots.footer"
          class="px-4 py-4 md:px-6 md:py-4 border-t flex justify-end gap-3 flex-shrink-0"
        >
          <slot name="footer"></slot>
        </footer>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, computed, watch, onUnmounted, onMounted } from 'vue'

const props = defineProps({
  isOpen: Boolean,
  maxWidth: {
    type: String,
    default: '2xl',
  },
})

const emit = defineEmits(['close'])

const startPos = ref({ x: 0, y: 0 })
const historyPushed = ref(false)

// 뒤로가기 이벤트 핸들러
const handlePopState = (event) => {
  if (props.isOpen && historyPushed.value) {
    event.preventDefault()
    close(true) // true = 히스토리에서 닫힘
  }
}

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

const onTouchStart = (e) => {
  const touch = e.touches[0]
  startPos.value = { x: touch.clientX, y: touch.clientY }
}

const onTouchEnd = (e) => {
  const touch = e.changedTouches[0]
  const dx = Math.abs(touch.clientX - startPos.value.x)
  const dy = Math.abs(touch.clientY - startPos.value.y)
  if (dx < 5 && dy < 5) {
    close()
  }
}

const close = (fromHistory = false) => {
  // 히스토리에서 닫힌 경우가 아니면 뒤로가기 실행
  if (!fromHistory && historyPushed.value) {
    historyPushed.value = false
    window.history.back()
  } else {
    historyPushed.value = false
    emit('close')
  }
}

const maxWidthClass = computed(() => {
  return {
    sm: 'max-w-sm',
    md: 'max-w-md',
    lg: 'max-w-lg',
    xl: 'max-w-xl',
    '2xl': 'max-w-2xl',
    '3xl': 'max-w-3xl',
    '4xl': 'max-w-4xl',
    '5xl': 'max-w-5xl',
  }[props.maxWidth]
})

// 모달이 열릴 때 body 스크롤 막기 및 히스토리 관리
watch(
  () => props.isOpen,
  (isOpen, wasOpen) => {
    if (isOpen && !wasOpen) {
      // 모달이 열릴 때
      // 현재 스크롤 위치 저장
      const scrollY = window.scrollY
      document.body.style.position = 'fixed'
      document.body.style.top = `-${scrollY}px`
      document.body.style.width = '100%'
      document.body.style.overflowY = 'scroll'

      // 히스토리에 가상 엔트리 추가
      if (!historyPushed.value) {
        window.history.pushState({ modal: 'open' }, '')
        historyPushed.value = true
      }
    } else if (!isOpen && wasOpen) {
      // 모달이 닫힐 때
      // 스크롤 위치 복원
      const scrollY = document.body.style.top
      document.body.style.position = ''
      document.body.style.top = ''
      document.body.style.width = ''
      document.body.style.overflowY = ''
      window.scrollTo(0, parseInt(scrollY || '0') * -1)
    }
  },
)

onMounted(() => {
  window.addEventListener('popstate', handlePopState)
})

// 컴포넌트 unmount 시 body 스타일 복원 및 이벤트 리스너 제거
onUnmounted(() => {
  document.body.style.position = ''
  document.body.style.top = ''
  document.body.style.width = ''
  document.body.style.overflowY = ''
  window.removeEventListener('popstate', handlePopState)

  // 히스토리 정리
  if (historyPushed.value && props.isOpen) {
    window.history.back()
  }
})
</script>

<style scoped>
/* 데스크탑: 페이드 애니메이션 */
@media (min-width: 768px) {
  .modal-fade-enter-active,
  .modal-fade-leave-active {
    transition: opacity 0.25s ease-in-out;
  }

  .modal-fade-enter-active .modal-content,
  .modal-fade-leave-active .modal-content {
    transition: transform 0.25s ease-in-out, opacity 0.25s ease-in-out;
  }

  .modal-fade-enter-from,
  .modal-fade-leave-to {
    opacity: 0;
  }

  .modal-fade-enter-from .modal-content,
  .modal-fade-leave-to .modal-content {
    transform: scale(0.95);
    opacity: 0;
  }
}

/* 모바일: 슬라이드업 애니메이션 */
@media (max-width: 767px) {
  .modal-fade-enter-active,
  .modal-fade-leave-active {
    transition: opacity 0.3s ease-out;
  }

  .modal-fade-enter-active .modal-content,
  .modal-fade-leave-active .modal-content {
    transition: transform 0.3s ease-out;
  }

  .modal-fade-enter-from,
  .modal-fade-leave-to {
    opacity: 0;
  }

  .modal-fade-enter-from .modal-content,
  .modal-fade-leave-to .modal-content {
    transform: translateY(100%);
  }
}
</style>
