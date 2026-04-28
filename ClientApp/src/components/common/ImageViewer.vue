<template>
  <Teleport to="body">
    <div
      v-if="modelValue"
      class="fixed inset-0 z-[100] bg-black/90 flex items-center justify-center"
      @click="close"
      @touchstart="onTouchStart"
      @touchend="onTouchEnd"
    >
      <img
        loading="lazy"
        :src="currentSrc"
        class="max-w-full max-h-full object-contain select-none"
        @click.stop
      />

      <button
        class="absolute top-4 right-4 w-10 h-10 bg-white/20 text-white rounded-full flex items-center justify-center text-xl hover:bg-white/30"
        @click="close"
      >
        &times;
      </button>

      <button
        v-if="hasPrev"
        class="absolute left-3 top-1/2 -translate-y-1/2 w-10 h-10 bg-white/20 text-white rounded-full flex items-center justify-center hover:bg-white/30"
        @click.stop="prev"
      >
        <svg
          class="w-5 h-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M15 19l-7-7 7-7"
          />
        </svg>
      </button>

      <button
        v-if="hasNext"
        class="absolute right-3 top-1/2 -translate-y-1/2 w-10 h-10 bg-white/20 text-white rounded-full flex items-center justify-center hover:bg-white/30"
        @click.stop="next"
      >
        <svg
          class="w-5 h-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M9 5l7 7-7 7"
          />
        </svg>
      </button>

      <div
        v-if="images.length > 1"
        class="absolute bottom-6 left-1/2 -translate-x-1/2 px-3 py-1 bg-black/50 text-white text-sm rounded-full"
      >
        {{ currentIndex + 1 }} / {{ images.length }}
      </div>

      <slot name="actions" />
    </div>
  </Teleport>
</template>

<script setup>
import { ref, computed, watch, onBeforeUnmount } from 'vue'
import { useUIStore } from '@/stores/ui'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  images: { type: Array, default: () => [] },
  startIndex: { type: Number, default: 0 },
})

const emit = defineEmits(['update:modelValue'])

const uiStore = useUIStore()
const modalId = Symbol('imageViewer')
const currentIndex = ref(0)
let touchStartX = 0

const currentSrc = computed(() => props.images[currentIndex.value] || '')
const hasPrev = computed(() => currentIndex.value > 0)
const hasNext = computed(() => currentIndex.value < props.images.length - 1)

function close() {
  emit('update:modelValue', false)
}

function prev() {
  if (hasPrev.value) currentIndex.value--
}

function next() {
  if (hasNext.value) currentIndex.value++
}

function onTouchStart(e) {
  touchStartX = e.touches[0].clientX
}

function onTouchEnd(e) {
  const diff = touchStartX - e.changedTouches[0].clientX
  if (Math.abs(diff) > 50) {
    if (diff > 0) next()
    else prev()
  }
}

watch(
  () => props.modelValue,
  (open) => {
    if (open) {
      currentIndex.value = props.startIndex
      uiStore.registerModal(modalId, close)
    } else {
      uiStore.unregisterModal(modalId)
    }
  },
)

watch(
  () => props.startIndex,
  (val) => {
    if (props.modelValue) currentIndex.value = val
  },
)

onBeforeUnmount(() => {
  uiStore.unregisterModal(modalId)
})
</script>
