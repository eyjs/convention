<!--
  Generic Card Component

  Displays an informational card with title, description, and optional icon/image.
  Delegates click actions to the useAction composable.
-->
<template>
  <div
    :class="cardClasses"
    @click.prevent="handleCardClick"
    :role="isClickable ? 'button' : 'article'"
    :tabindex="isClickable ? 0 : undefined"
    @keydown.enter.prevent="handleCardClick"
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
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useAction } from '@/composables/useAction'

const props = defineProps({
  feature: {
    type: Object,
    required: true,
  },
})

const { executeAction } = useAction()

// Parse config
const config = computed(() => {
  try {
    if (
      typeof props.feature.configJson === 'string' &&
      props.feature.configJson.trim() === ''
    ) {
      return {}
    }
    return typeof props.feature.configJson === 'string'
      ? JSON.parse(props.feature.configJson)
      : props.feature.configJson || {}
  } catch (error) {
    console.error('Failed to parse card config:', error)
    return {}
  }
})

// An action is clickable if it's not a simple status-only type.
const isClickable = computed(() => {
  return props.feature.behaviorType !== 'StatusOnly'
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
  const variantClasses = {
    default: 'border border-gray-200',
    info: 'border-l-4 border-l-blue-500 border border-gray-200',
    success: 'border-l-4 border-l-green-500 border border-gray-200',
    warning: 'border-l-4 border-l-yellow-500 border border-gray-200',
    danger: 'border-l-4 border-l-red-500 border border-gray-200',
  }
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

// Badge classes
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

// Handle card click by delegating to the central action executor
const handleCardClick = () => {
  if (!isClickable.value) return
  try {
    executeAction(props.feature)
  } catch (error) {
    console.error('Card click error:', error)
  }
}
</script>

<style scoped>
.flex-shrink-0 :deep(svg) {
  width: 1.75rem;
  height: 1.75rem;
}
.text-sm :deep(svg) {
  width: 1rem;
  height: 1rem;
}
.group:hover {
  transform: translateY(-2px);
}
@media (max-width: 640px) {
  .group {
    border-radius: 0.5rem;
  }
}
</style>
