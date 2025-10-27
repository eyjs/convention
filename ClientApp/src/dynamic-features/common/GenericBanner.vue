<!--
  Generic Banner Component

  Displays a banner image or message with optional overlay text and CTA button.
  Supports clickable banners and various height configurations.

  Props:
    - feature: Action object containing configuration
      - actionName: Banner title (used for alt text if image)
      - config: {
          imageUrl, height, url, externalUrl, overlayText,
          overlayPosition, ctaButton, backgroundColor, textColor
        }
-->

<template>
  <div
    :class="bannerClasses"
    :style="bannerStyle"
    @click="handleBannerClick"
    :role="isClickable ? 'button' : 'img'"
    :tabindex="isClickable ? 0 : undefined"
    @keydown.enter="handleBannerClick"
    @keydown.space.prevent="handleBannerClick"
  >
    <!-- Background Image -->
    <div
      v-if="config.imageUrl"
      class="absolute inset-0 bg-cover bg-center bg-no-repeat"
      :style="{ backgroundImage: `url(${config.imageUrl})` }"
      :aria-label="feature.actionName"
    >
      <!-- Image overlay gradient (for better text readability) -->
      <div
        v-if="config.overlayText || config.ctaButton"
        class="absolute inset-0 bg-gradient-to-b from-black/30 via-transparent to-black/50"
      ></div>
    </div>

    <!-- Content Overlay -->
    <div
      v-if="config.overlayText || config.ctaButton"
      :class="overlayClasses"
      class="relative z-10 w-full h-full flex flex-col px-6 py-8 md:px-12 md:py-12"
    >
      <!-- Overlay Text -->
      <div v-if="config.overlayText" class="max-w-3xl">
        <h2
          class="text-2xl md:text-4xl font-bold mb-2 drop-shadow-lg"
          :style="{ color: config.textColor || '#FFFFFF' }"
        >
          {{ feature.actionName }}
        </h2>
        <p
          class="text-base md:text-lg drop-shadow-md"
          :style="{ color: config.textColor || '#FFFFFF' }"
        >
          {{ config.overlayText }}
        </p>
      </div>

      <!-- CTA Button -->
      <button
        v-if="config.ctaButton"
        :class="ctaButtonClasses"
        @click.stop="handleCtaClick"
        class="mt-4 self-start"
      >
        {{ config.ctaButton.label }}
        <svg
          class="w-4 h-4 ml-2 inline-block"
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
    </div>

    <!-- Click indicator (if banner is clickable) -->
    <div
      v-if="isClickable && !config.ctaButton"
      class="absolute top-4 right-4 bg-white/90 rounded-full p-2 shadow-lg z-10 opacity-0 group-hover:opacity-100 transition-opacity"
    >
      <svg class="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M9 5l7 7-7 7"
        />
      </svg>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps({
  feature: {
    type: Object,
    required: true,
  },
})

const router = useRouter()

// Parse config
const config = computed(() => {
  try {
    return typeof props.feature.config === 'string'
      ? JSON.parse(props.feature.config)
      : props.feature.config || {}
  } catch (error) {
    console.error('Failed to parse banner config:', error)
    return {}
  }
})

// Check if banner is clickable
const isClickable = computed(() => {
  return !!(config.value.url || config.value.externalUrl)
})

// Banner classes
const bannerClasses = computed(() => {
  const height = config.value.height || 'md'

  const heightClasses = {
    sm: 'h-32 md:h-40',
    md: 'h-48 md:h-64',
    lg: 'h-64 md:h-96',
    xl: 'h-96 md:h-[32rem]',
    full: 'h-screen',
  }

  const baseClasses = [
    'relative',
    'w-full',
    'rounded-lg',
    'overflow-hidden',
    'shadow-md',
    'group',
  ]

  if (isClickable.value) {
    baseClasses.push('cursor-pointer', 'transition-transform', 'duration-200', 'hover:scale-[1.02]')
  }

  return [...baseClasses, heightClasses[height] || heightClasses.md].join(' ')
})

// Banner style (for solid color banners without image)
const bannerStyle = computed(() => {
  if (!config.value.imageUrl && config.value.backgroundColor) {
    return {
      backgroundColor: config.value.backgroundColor,
    }
  }
  return {}
})

// Overlay position classes
const overlayClasses = computed(() => {
  const position = config.value.overlayPosition || 'center'

  const positionClasses = {
    top: 'justify-start items-start',
    center: 'justify-center items-center text-center',
    bottom: 'justify-end items-start',
    'bottom-center': 'justify-end items-center text-center',
  }

  return positionClasses[position] || positionClasses.center
})

// CTA button classes
const ctaButtonClasses = computed(() => {
  const style = config.value.ctaButton?.style || 'primary'

  const baseClasses = 'px-6 py-3 rounded-lg font-medium transition-all duration-200 shadow-lg hover:shadow-xl active:scale-95'

  const styleClasses = {
    primary: 'bg-blue-600 text-white hover:bg-blue-700',
    secondary: 'bg-white text-gray-900 hover:bg-gray-100',
    success: 'bg-green-600 text-white hover:bg-green-700',
    danger: 'bg-red-600 text-white hover:bg-red-700',
    outline: 'bg-transparent text-white border-2 border-white hover:bg-white/10',
  }

  return `${baseClasses} ${styleClasses[style] || styleClasses.primary}`
})

// Handle banner click
const handleBannerClick = () => {
  if (!isClickable.value) return

  try {
    if (config.value.externalUrl) {
      window.open(config.value.externalUrl, '_blank', 'noopener,noreferrer')
    } else if (config.value.url) {
      router.push(config.value.url)
    }
  } catch (error) {
    console.error('Banner click error:', error)
  }
}

// Handle CTA button click
const handleCtaClick = () => {
  if (config.value.ctaButton?.url) {
    router.push(config.value.ctaButton.url)
  } else if (config.value.ctaButton?.externalUrl) {
    window.open(config.value.ctaButton.externalUrl, '_blank', 'noopener,noreferrer')
  } else {
    // Fallback to banner's default click action
    handleBannerClick()
  }
}
</script>

<style scoped>
/* Ensure text is readable on any background */
.drop-shadow-lg {
  filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.5));
}

.drop-shadow-md {
  filter: drop-shadow(0 1px 3px rgba(0, 0, 0, 0.4));
}

/* Smooth transition for hover effects */
.group:hover .opacity-0 {
  opacity: 1;
}

/* Mobile optimization */
@media (max-width: 640px) {
  .group {
    border-radius: 0.5rem;
  }
}
</style>
