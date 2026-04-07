<template>
  <Transition name="scroll-top-fade">
    <button
      v-if="visible"
      class="fixed bottom-6 right-6 z-40 w-12 h-12 bg-primary-500 hover:bg-primary-600 text-white rounded-full shadow-lg flex items-center justify-center transition-colors"
      aria-label="맨 위로"
      @click="scrollToTop"
    >
      <svg
        class="w-6 h-6"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M5 15l7-7 7 7"
        />
      </svg>
    </button>
  </Transition>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  threshold: { type: Number, default: 300 },
  target: { type: String, default: 'window' }, // 'window' 또는 CSS 선택자
})

const visible = ref(false)

function getScrollElement() {
  if (props.target === 'window') return window
  return document.querySelector(props.target)
}

function handleScroll() {
  const el = getScrollElement()
  const top = el === window ? window.scrollY : el?.scrollTop || 0
  visible.value = top > props.threshold
}

function scrollToTop() {
  const el = getScrollElement()
  if (el === window) {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  } else if (el) {
    el.scrollTo({ top: 0, behavior: 'smooth' })
  }
}

onMounted(() => {
  const el = getScrollElement()
  if (el) el.addEventListener('scroll', handleScroll, { passive: true })
})

onUnmounted(() => {
  const el = getScrollElement()
  if (el) el.removeEventListener('scroll', handleScroll)
})
</script>

<style scoped>
.scroll-top-fade-enter-active,
.scroll-top-fade-leave-active {
  transition:
    opacity 0.2s ease,
    transform 0.2s ease;
}
.scroll-top-fade-enter-from,
.scroll-top-fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}
</style>
