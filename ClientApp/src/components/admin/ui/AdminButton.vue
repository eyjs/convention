<template>
  <button
    :class="[baseClass, variantClass, sizeClass]"
    :disabled="disabled || loading"
    v-bind="$attrs"
  >
    <span
      v-if="loading"
      class="inline-block w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin"
    />
    <component :is="icon" v-else-if="icon" :size="iconSize" />
    <slot />
  </button>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  variant: {
    type: String,
    default: 'primary',
    validator: (v) => ['primary', 'secondary', 'danger', 'ghost'].includes(v),
  },
  size: {
    type: String,
    default: 'md',
    validator: (v) => ['sm', 'md', 'lg'].includes(v),
  },
  icon: {
    type: [Object, Function],
    default: null,
  },
  loading: {
    type: Boolean,
    default: false,
  },
  disabled: {
    type: Boolean,
    default: false,
  },
})

const baseClass =
  'inline-flex items-center justify-center gap-2 font-medium rounded-lg transition-colors whitespace-nowrap disabled:opacity-50 disabled:cursor-not-allowed'

const variantClass = computed(() => {
  const map = {
    primary: 'bg-primary-600 text-white hover:bg-primary-700',
    secondary: 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50',
    danger: 'bg-white text-red-600 border border-red-300 hover:bg-red-50',
    ghost: 'text-gray-600 hover:bg-gray-100',
  }
  return map[props.variant]
})

const sizeClass = computed(() => {
  const map = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2 text-sm',
    lg: 'px-5 py-2.5 text-base',
  }
  return map[props.size]
})

const iconSize = computed(() => {
  const map = { sm: 14, md: 16, lg: 18 }
  return map[props.size]
})
</script>
