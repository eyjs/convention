<template>
  <Transition name="slide-up">
    <div
      v-if="isOpen"
      class="fixed inset-0 bg-black/60 backdrop-blur-sm flex justify-center items-end"
      :class="zIndexClass"
      @mousedown="onBackdropMouseDown"
      @mouseup="onBackdropMouseUp"
      @touchstart="onBackdropTouchStart"
      @touchend="onBackdropTouchEnd"
    >
      <div
        class="bg-white rounded-t-2xl shadow-2xl w-full max-h-[85dvh] max-h-[85vh] flex flex-col pb-safe md:pb-0"
        @touchmove.stop
      >
        <!-- Header -->
        <header
          class="relative bg-white px-4 py-4 md:px-6 md:py-4 border-b flex items-center flex-shrink-0"
          :class="alignLeft ? 'justify-between' : 'justify-between'"
        >
          <template v-if="alignLeft">
            <div class="flex-1 min-w-0 pr-2">
              <slot name="header-title"></slot>
            </div>
          </template>
          <template v-else>
            <div class="w-8 md:w-10">
              <slot name="header-left"></slot>
            </div>
            <h2
              class="absolute left-1/2 -translate-x-1/2 text-lg md:text-xl font-bold text-gray-900 text-center truncate px-8"
            >
              <slot name="header-title"></slot>
            </h2>
          </template>
          <button
            class="p-2.5 md:p-2 -mr-2 hover:bg-gray-100 active:bg-gray-200 rounded-lg z-10 text-gray-500 transition-colors flex-shrink-0"
            @click="close"
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
        <footer
          v-if="$slots.footer"
          class="px-4 py-4 md:px-6 md:py-4 border-t flex-shrink-0"
        >
          <slot name="footer"></slot>
        </footer>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { watch, onUnmounted } from 'vue'
import { useUIStore } from '@/stores/ui'
import { useBackdropClose } from '@/composables/useBackdropClose'

const uiStore = useUIStore()

const props = defineProps({
  isOpen: Boolean,
  zIndexClass: {
    type: String,
    default: 'z-50',
  },
  alignLeft: {
    type: Boolean,
    default: false,
  },
})

const emit = defineEmits(['close'])

const modalId = Symbol('modal') // 고유 ID

const close = () => {
  emit('close')
}

const {
  onBackdropMouseDown,
  onBackdropMouseUp,
  onBackdropTouchStart,
  onBackdropTouchEnd,
} = useBackdropClose(close)

// 모달 열림/닫힘 시 body 스크롤 제어 및 스택 관리
watch(
  () => props.isOpen,
  (newValue, oldValue) => {
    if (newValue && !oldValue) {
      // body + .safe-content 모두 스크롤 차단
      document.body.style.overflow = 'hidden'
      document.body.style.touchAction = 'none'
      const scrollContainer = document.querySelector('.safe-content')
      if (scrollContainer) {
        scrollContainer.dataset.prevOverflow = scrollContainer.style.overflow
        scrollContainer.style.overflow = 'hidden'
      }

      uiStore.registerModal(modalId, () => {
        emit('close')
      })
    } else if (!newValue && oldValue) {
      document.body.style.overflow = ''
      document.body.style.touchAction = ''
      const scrollContainer = document.querySelector('.safe-content')
      if (scrollContainer) {
        scrollContainer.style.overflow =
          scrollContainer.dataset.prevOverflow || ''
      }

      uiStore.unregisterModal(modalId)
    }
  },
)

// 컴포넌트가 언마운트될 때 스크롤 복원 및 스택에서 제거
onUnmounted(() => {
  if (props.isOpen) {
    uiStore.unregisterModal(modalId)
  }
  document.body.style.overflow = ''
  document.body.style.touchAction = ''
  const scrollContainer = document.querySelector('.safe-content')
  if (scrollContainer) {
    scrollContainer.style.overflow = scrollContainer.dataset.prevOverflow || ''
  }
})
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

/* 하단 safe-area */
@media (max-width: 767px) {
  .pb-safe {
    padding-bottom: var(--safe-area-bottom, env(safe-area-inset-bottom, 8px));
  }
}
</style>
