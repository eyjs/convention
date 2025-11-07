<!--
  Generic Card Component

  Displays an informational card with title, description, and optional icon/image.
  Supports different variants, clickable cards, and footer actions.

  Props:
    - feature: Action object containing configuration
      - actionName: Card title
      - config: {
          description, icon, iconColor, bgColor, imageUrl,
          variant, url, externalUrl, footer
        }
-->

<template>
  <div
    :class="cardClasses"
    @click="handleCardClick"
    :role="isClickable ? 'button' : 'article'"
    :tabindex="isClickable ? 0 : undefined"
    @keydown.enter="handleCardClick"
    @keydown.space.prevent="handleCardClick"
  >
    <!-- Card Image (optional) -->
    <div
      v-if="config.imageUrl"
      class="w-full h-48 overflow-hidden rounded-t-lg"
    >
      <img
        :src="config.imageUrl"
        :alt="feature.title"
        class="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
      />
    </div>

    <!-- Card Content -->
    <div class="p-5 md:p-6">
      <!-- Icon & Title -->
      <div class="flex items-start gap-4 mb-3">
        <!-- Icon -->
        <div
          v-if="config.icon && !config.imageUrl"
          class="flex-shrink-0 w-12 h-12 rounded-lg flex items-center justify-center"
          :style="iconStyle"
        >
          <span v-html="config.icon"></span>
        </div>

        <!-- Title -->
        <div class="flex-1 min-w-0">
          <h3 class="text-lg md:text-xl font-bold text-gray-900 mb-1">
            {{ feature.title }}
          </h3>

          <!-- Badge (optional) -->
          <span
            v-if="config.badge"
            :class="badgeClasses"
            class="inline-block px-2 py-1 text-xs font-semibold rounded-full"
          >
            {{ config.badge }}
          </span>
        </div>

        <!-- Click indicator -->
        <div
          v-if="isClickable"
          class="flex-shrink-0 text-gray-400 transition-colors group-hover:text-blue-600"
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
        </div>
      </div>

      <!-- Description -->
      <p
        v-if="config.description"
        class="text-sm md:text-base text-gray-600 leading-relaxed"
      >
        {{ config.description }}
      </p>

      <!-- Stats/Metadata (optional) -->
      <div v-if="config.metadata" class="mt-4 flex flex-wrap gap-4">
        <div
          v-for="(item, index) in config.metadata"
          :key="index"
          class="flex items-center gap-2 text-sm text-gray-500"
        >
          <span v-if="item.icon" v-html="item.icon" class="w-4 h-4"></span>
          <span
            >{{ item.label }}:
            <strong class="text-gray-900">{{ item.value }}</strong></span
          >
        </div>
      </div>
    </div>

    <!-- Card Footer (optional) -->
    <div
      v-if="
        config.footer && config.footer.links && config.footer.links.length > 0
      "
      class="px-5 md:px-6 py-4 border-t border-gray-200 bg-gray-50"
    >
      <div class="flex flex-wrap gap-3">
        <a
          v-for="(link, index) in config.footer.links"
          :key="index"
          :href="link.url || link.externalUrl"
          :target="link.externalUrl ? '_blank' : undefined"
          :rel="link.externalUrl ? 'noopener noreferrer' : undefined"
          @click.stop="handleFooterLinkClick(link, $event)"
          class="text-sm font-medium text-blue-600 hover:text-blue-800 hover:underline transition-colors"
        >
          {{ link.label }}
          <svg
            v-if="link.externalUrl"
            class="w-3 h-3 inline-block ml-1"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
            />
          </svg>
        </a>
      </div>
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
    if (typeof props.feature.configJson === 'string' && props.feature.configJson.trim() === '') {
      return {}; // Return empty object for empty string
    }
    return typeof props.feature.configJson === 'string'
      ? JSON.parse(props.feature.configJson)
      : props.feature.configJson || {};
  } catch (error) {
    console.error('Failed to parse card config:', error)
    return {}
  }
})

// Check if card is clickable
const isClickable = computed(() => {
  return !!(config.value.url || config.value.externalUrl)
})

// Card classes
const cardClasses = computed(() => {
  const variant = config.value.variant || 'default'

  const baseClasses = [
    'bg-white',
    'rounded-lg',
    'shadow-md',
    'overflow-hidden',
    'transition-all',
    'duration-200',
    'group',
  ]

  // Variant border colors
  const variantClasses = {
    default: 'border border-gray-200',
    info: 'border-l-4 border-l-blue-500 border border-gray-200',
    success: 'border-l-4 border-l-green-500 border border-gray-200',
    warning: 'border-l-4 border-l-yellow-500 border border-gray-200',
    danger: 'border-l-4 border-l-red-500 border border-gray-200',
  }

  // Hover effects for clickable cards
  if (isClickable.value) {
    baseClasses.push(
      'cursor-pointer',
      'hover:shadow-lg',
      'hover:border-blue-300',
      'active:scale-[0.99]',
    )
  }

  return [
    ...baseClasses,
    variantClasses[variant] || variantClasses.default,
  ].join(' ')
})

// Icon style
const iconStyle = computed(() => {
  const bgColor = config.value.bgColor || '#3B82F6'
  const iconColor = config.value.iconColor || '#FFFFFF'

  return {
    backgroundColor: bgColor,
    color: iconColor,
  }
})

// Badge classes based on variant
const badgeClasses = computed(() => {
  const variant = config.value.variant || 'default'

  const variantBadgeClasses = {
    default: 'bg-gray-100 text-gray-700',
    info: 'bg-blue-100 text-blue-700',
    success: 'bg-green-100 text-green-700',
    warning: 'bg-yellow-100 text-yellow-700',
    danger: 'bg-red-100 text-red-700',
  }

  return variantBadgeClasses[variant] || variantBadgeClasses.default
})

// Handle card click
const handleCardClick = () => {
  if (!isClickable.value) return

  try {
    if (config.value.externalUrl) {
      window.open(config.value.externalUrl, '_blank', 'noopener,noreferrer')
    } else if (config.value.url) {
      router.push(config.value.url)
    }
  } catch (error) {
    console.error('Card click error:', error)
  }
}

// Handle footer link click
const handleFooterLinkClick = (link, event) => {
  // If it's an internal URL, prevent default and use router
  if (link.url && !link.externalUrl) {
    event.preventDefault()
    router.push(link.url)
  }
  // External URLs will be handled by the browser naturally
}
</script>

<style scoped>
/* Icon container deep selector for SVG sizing */
.flex-shrink-0 :deep(svg) {
  width: 1.75rem;
  height: 1.75rem;
}

/* Metadata icon sizing */
.text-sm :deep(svg) {
  width: 1rem;
  height: 1rem;
}

/* Card hover elevation effect */
.group:hover {
  transform: translateY(-2px);
}

/* Mobile optimization */
@media (max-width: 640px) {
  .group {
    border-radius: 0.5rem;
  }
}
</style>
