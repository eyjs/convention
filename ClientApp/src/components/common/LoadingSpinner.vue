<template>
  <div
    class="flex flex-col items-center justify-center"
    :class="containerClass"
  >
    <div
      class="rounded-full animate-spin"
      :class="[sizeClass, colorClass]"
    ></div>
    <p v-if="text" class="mt-3 text-sm text-gray-500">{{ text }}</p>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  size: {
    type: String,
    default: 'md',
    validator: (v) => ['sm', 'md', 'lg'].includes(v),
  },
  color: {
    type: String,
    default: 'primary',
  },
  text: {
    type: String,
    default: '',
  },
  fullPage: {
    type: Boolean,
    default: false,
  },
})

const sizeClass = computed(() => {
  const sizes = {
    sm: 'w-6 h-6 border-2',
    md: 'w-8 h-8 border-[3px]',
    lg: 'w-12 h-12 border-4',
  }
  return sizes[props.size]
})

const colorClass = computed(() => {
  const colors = {
    primary: 'border-primary-600 border-t-transparent',
    blue: 'border-blue-600 border-t-transparent',
    white: 'border-white border-t-transparent',
  }
  return colors[props.color] || 'border-primary-600 border-t-transparent'
})

const containerClass = computed(() => (props.fullPage ? 'min-h-[60vh]' : ''))
</script>
