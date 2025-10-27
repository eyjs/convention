<!--
  Generic Auto Popup Component

  Displays a modal popup that appears automatically based on trigger conditions.
  Supports "don't show again" functionality with localStorage.

  Props:
    - feature: Action object containing configuration
      - actionName: Popup title
      - config: {
          trigger, delay, message, imageUrl, size,
          showOnce, buttons, position
        }
-->

<template>
  <Teleport to="body">
    <Transition name="popup-fade">
      <div
        v-if="isVisible"
        class="fixed inset-0 z-50 flex items-center justify-center"
        @click.self="handleBackdropClick"
      >
        <!-- Backdrop -->
        <div class="absolute inset-0 bg-black bg-opacity-50 backdrop-blur-sm"></div>

        <!-- Popup Container -->
        <div
          :class="popupClasses"
          class="relative bg-white rounded-2xl shadow-2xl transform transition-all duration-300"
          role="dialog"
          aria-modal="true"
          :aria-labelledby="`popup-title-${feature.id}`"
        >
          <!-- Close Button -->
          <button
            @click="handleClose"
            class="absolute top-4 right-4 text-gray-400 hover:text-gray-600 transition-colors z-10"
            aria-label="닫기"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>

          <!-- Image (optional) -->
          <div v-if="config.imageUrl" class="w-full overflow-hidden rounded-t-2xl">
            <img
              :src="config.imageUrl"
              :alt="feature.actionName"
              class="w-full h-48 object-cover"
            />
          </div>

          <!-- Content -->
          <div class="p-6 md:p-8">
            <!-- Title -->
            <h2
              :id="`popup-title-${feature.id}`"
              class="text-xl md:text-2xl font-bold text-gray-900 mb-4"
            >
              {{ feature.actionName }}
            </h2>

            <!-- Message -->
            <div
              v-if="config.message"
              class="prose prose-sm md:prose-base max-w-none mb-6 text-gray-700"
              v-html="config.message"
            ></div>

            <!-- Action Buttons -->
            <div
              v-if="config.buttons && config.buttons.length > 0"
              class="flex flex-col sm:flex-row gap-3 mb-4"
            >
              <button
                v-for="(button, index) in config.buttons"
                :key="index"
                :class="getButtonClasses(button.style)"
                @click="handleButtonClick(button)"
              >
                {{ button.label }}
              </button>
            </div>

            <!-- Don't show again checkbox -->
            <div v-if="config.showOnce" class="flex items-center mt-4 pt-4 border-t border-gray-200">
              <input
                id="dont-show-again"
                v-model="dontShowAgain"
                type="checkbox"
                class="h-4 w-4 text-blue-600 rounded border-gray-300 focus:ring-blue-500"
              />
              <label for="dont-show-again" class="ml-2 text-sm text-gray-600">
                다시 보지 않기
              </label>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps({
  feature: {
    type: Object,
    required: true,
  },
})

const router = useRouter()
const isVisible = ref(false)
const dontShowAgain = ref(false)

// Parse config
const config = computed(() => {
  try {
    return typeof props.feature.config === 'string'
      ? JSON.parse(props.feature.config)
      : props.feature.config || {}
  } catch (error) {
    console.error('Failed to parse popup config:', error)
    return {}
  }
})

// Popup size classes
const popupClasses = computed(() => {
  const size = config.value.size || 'md'

  const sizeClasses = {
    sm: 'max-w-sm w-full mx-4',
    md: 'max-w-md w-full mx-4',
    lg: 'max-w-2xl w-full mx-4',
    xl: 'max-w-4xl w-full mx-4',
  }

  return sizeClasses[size] || sizeClasses.md
})

// Get localStorage key for this popup
const getStorageKey = () => {
  return `popup-dismissed-${props.feature.id || props.feature.actionType}`
}

// Check if popup should be shown
const shouldShow = () => {
  if (!config.value.showOnce) return true

  const storageKey = getStorageKey()
  const dismissed = localStorage.getItem(storageKey)
  return !dismissed
}

// Show popup based on trigger condition
const triggerPopup = () => {
  if (!shouldShow()) {
    return
  }

  const trigger = config.value.trigger || 'onPageLoad'
  const delay = config.value.delay || 500

  if (trigger === 'immediate') {
    isVisible.value = true
  } else if (trigger === 'onPageLoad' || trigger === 'onLogin') {
    setTimeout(() => {
      isVisible.value = true
    }, delay)
  } else if (trigger === 'timed') {
    setTimeout(() => {
      isVisible.value = true
    }, delay)
  }
}

// Handle close
const handleClose = () => {
  if (dontShowAgain.value && config.value.showOnce) {
    const storageKey = getStorageKey()
    localStorage.setItem(storageKey, 'true')
  }

  isVisible.value = false
}

// Handle backdrop click
const handleBackdropClick = () => {
  if (config.value.dismissOnBackdrop !== false) {
    handleClose()
  }
}

// Handle button click
const handleButtonClick = (button) => {
  try {
    // Execute button action
    if (button.url) {
      router.push(button.url)
    } else if (button.externalUrl) {
      window.open(button.externalUrl, '_blank', 'noopener,noreferrer')
    } else if (button.onClick && typeof window[button.onClick] === 'function') {
      window[button.onClick](props.feature)
    }

    // Auto-close unless specified otherwise
    if (button.closeOnClick !== false) {
      handleClose()
    }
  } catch (error) {
    console.error('Popup button click error:', error)
  }
}

// Get button classes based on style
const getButtonClasses = (style = 'primary') => {
  const baseClasses = 'flex-1 px-6 py-3 rounded-lg font-medium transition-all duration-200'

  const styleClasses = {
    primary: 'bg-blue-600 text-white hover:bg-blue-700 active:bg-blue-800',
    secondary: 'bg-gray-200 text-gray-800 hover:bg-gray-300 active:bg-gray-400',
    success: 'bg-green-600 text-white hover:bg-green-700 active:bg-green-800',
    danger: 'bg-red-600 text-white hover:bg-red-700 active:bg-red-800',
    outline: 'bg-white text-blue-600 border-2 border-blue-600 hover:bg-blue-50',
  }

  return `${baseClasses} ${styleClasses[style] || styleClasses.primary}`
}

// Handle escape key
const handleEscape = (event) => {
  if (event.key === 'Escape' && isVisible.value) {
    handleClose()
  }
}

// Lifecycle
onMounted(() => {
  triggerPopup()
  document.addEventListener('keydown', handleEscape)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleEscape)
})
</script>

<style scoped>
/* Popup fade transition */
.popup-fade-enter-active,
.popup-fade-leave-active {
  transition: opacity 0.3s ease;
}

.popup-fade-enter-from,
.popup-fade-leave-to {
  opacity: 0;
}

.popup-fade-enter-active .relative,
.popup-fade-leave-active .relative {
  transition: transform 0.3s ease, opacity 0.3s ease;
}

.popup-fade-enter-from .relative {
  transform: scale(0.95) translateY(-20px);
  opacity: 0;
}

.popup-fade-leave-to .relative {
  transform: scale(0.95) translateY(20px);
  opacity: 0;
}

/* Prevent body scroll when popup is open */
:deep(body:has(.popup-fade-enter-active)),
:deep(body:has(.popup-fade-leave-active)) {
  overflow: hidden;
}
</style>
