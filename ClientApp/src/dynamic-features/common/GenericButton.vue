<!--
  Generic Button Component

  Renders a customizable button based on action configuration.
  Supports different styles, sizes, and click actions.

  Props:
    - feature: Action object containing configuration
      - actionName: Button label
      - config: { style, size, icon, url, externalUrl }
-->

<template>
  <button :class="buttonClasses" @click="handleClick" :disabled="isLoading">
    <!-- Icon (optional) -->
    <span v-if="config.icon" class="button-icon" v-html="config.icon"></span>

    <!-- Label -->
    <span class="button-label">{{ feature.title }}</span>

    <!-- Loading Spinner -->
    <span
      v-if="isLoading"
      class="ml-2 inline-block animate-spin rounded-full h-4 w-4 border-b-2 border-white"
    ></span>

    <!-- Arrow Icon (for links) -->
    <svg
      v-if="hasUrl && !isLoading"
      class="w-4 h-4 ml-2"
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
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps({
  feature: {
    type: Object,
    required: true,
  },
})

const router = useRouter()
const isLoading = ref(false)

// Parse config (stored as JSON string in DB)
const config = computed(() => {
  try {
    return typeof props.feature.configJson === 'string'
      ? JSON.parse(props.feature.configJson)
      : props.feature.configJson || {}
  } catch (error) {
    console.error('Failed to parse button config:', error)
    return {}
  }
})

// Check if button has URL
const hasUrl = computed(() => {
  return !!(config.value.url || config.value.externalUrl)
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

// Handle button click
async function handleClick() {
  if (isLoading.value) return

  try {
    // External URL (opens in new tab)
    if (config.value.externalUrl) {
      window.open(config.value.externalUrl, '_blank', 'noopener,noreferrer')
      return
    }

    // Internal URL (Vue Router navigation)
    if (config.value.url) {
      isLoading.value = true
      await router.push(config.value.url)
      return
    }

    // Custom callback (if provided in config)
    if (
      config.value.onClick &&
      typeof window[config.value.onClick] === 'function'
    ) {
      window[config.value.onClick](props.feature)
    }
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
