<template>
  <span :class="[baseClass, variantClass, sizeClass]">
    <slot />
  </span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  variant: {
    type: String,
    default: 'neutral',
    validator: (v) =>
      ['success', 'warning', 'danger', 'info', 'neutral'].includes(v),
  },
  size: {
    type: String,
    default: 'sm',
    validator: (v) => ['sm', 'md'].includes(v),
  },
})

const baseClass = 'inline-flex items-center font-semibold rounded-full'

const variantClass = computed(() => {
  const map = {
    success: 'bg-green-100 text-green-800',
    warning: 'bg-yellow-100 text-yellow-800',
    danger: 'bg-red-100 text-red-800',
    info: 'bg-blue-100 text-blue-800',
    neutral: 'bg-gray-100 text-gray-600',
  }
  return map[props.variant]
})

const sizeClass = computed(() => {
  const map = {
    sm: 'px-2 py-0.5 text-xs',
    md: 'px-2.5 py-1 text-xs',
  }
  return map[props.size]
})
</script>
