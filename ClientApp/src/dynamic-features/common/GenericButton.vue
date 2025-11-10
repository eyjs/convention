<!--
  Generic Button Component

  Renders a customizable button based on action configuration.
  Delegates click actions to the useAction composable.
-->
<template>
  <button :class="buttonClasses" @click.prevent="handleClick" :disabled="isLoading">
    <!-- Icon (optional) -->
    <span v-if="config.icon" class="button-icon" v-html="config.icon"></span>

    <!-- Label -->
    <span class="button-label">{{ feature.title }}</span>

    <!-- Loading Spinner -->
    <span
      v-if="isLoading"
      class="ml-2 inline-block animate-spin rounded-full h-4 w-4 border-b-2 border-white"
    ></span>
  </button>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useAction } from '@/composables/useAction'

const props = defineProps({
  feature: {
    type: Object,
    required: true,
  },
})

const isLoading = ref(false)
const { executeAction } = useAction()

// Parse config (stored as JSON string in DB)
const config = computed(() => {
  try {
    if (typeof props.feature.configJson === 'string' && props.feature.configJson.trim() === '') {
      return {};
    }
    return typeof props.feature.configJson === 'string'
      ? JSON.parse(props.feature.configJson)
      : props.feature.configJson || {}
  } catch (error) {
    console.error('Failed to parse button config:', error)
    return {}
  }
})

// Compute button classes based on style and size
const buttonClasses = computed(() => {
  const style = config.value.style || 'primary'
  const size = config.value.size || 'md'

  const baseClasses =
    'inline-flex items-center justify-center font-medium rounded-lg transition-all duration-200 shadow-sm hover:shadow-md disabled:opacity-50 disabled:cursor-not-allowed'

  const styleClasses = {
    primary: 'bg-blue-600 text-white hover:bg-blue-700 active:bg-blue-800',
    secondary: 'bg-gray-600 text-white hover:bg-gray-700 active:bg-gray-800',
    success: 'bg-green-600 text-white hover:bg-green-700 active:bg-green-800',
    danger: 'bg-red-600 text-white hover:bg-red-700 active:bg-red-800',
    warning:
      'bg-yellow-500 text-white hover:bg-yellow-600 active:bg-yellow-700',
    outline: 'bg-white text-blue-600 border-2 border-blue-600 hover:bg-blue-50',
    ghost: 'bg-transparent text-blue-600 hover:bg-blue-50',
  }

  const sizeClasses = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2 text-base',
    lg: 'px-6 py-3 text-lg',
  }

  return [
    baseClasses,
    styleClasses[style] || styleClasses.primary,
    sizeClasses[size] || sizeClasses.md,
  ].join(' ')
})

// Handle button click by delegating to the central action executor
async function handleClick() {
  if (isLoading.value) return

  isLoading.value = true
  try {
    await executeAction(props.feature)
  } catch (error) {
    console.error('Button click error:', error)
    alert('작업을 수행할 수 없습니다.')
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.button-icon {
  display: inline-flex;
  align-items: center;
  margin-right: 0.5rem;
}

.button-icon :deep(svg) {
  width: 1.25rem;
  height: 1.25rem;
}

.button-label {
  white-space: nowrap;
}

/* Mobile responsiveness */
@media (max-width: 640px) {
  button {
    width: 100%;
    justify-content: center;
  }
}
</style>
