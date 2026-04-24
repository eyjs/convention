<template>
  <button
    class="flex-1 py-2.5 text-xs font-medium transition-colors"
    :class="variantClass"
    @click="$emit('click')"
  >
    <slot></slot>
  </button>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  variant: { type: String, default: 'default' },
})

defineEmits(['click'])

const variantClass = computed(() => {
  const classes = {
    default: 'text-gray-600 active:bg-gray-50',
    primary: 'text-primary-600 active:bg-primary-50',
    danger: 'text-red-600 active:bg-red-50',
    success: 'text-green-600 active:bg-green-50',
    warning: 'text-amber-600 active:bg-amber-50',
  }
  return classes[props.variant] || classes.default
})
</script>
