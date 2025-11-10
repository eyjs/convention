<!--
  Generic Menu Item Component

  Renders a menu card/item typically used in the "More Features" grid.
  Displays an icon, title, and optional description.

  Props:
    - feature: Action object containing configuration
      - actionName: Menu item title
      - config: { icon, iconColor, bgColor, description, url, externalUrl }
-->

<template>
  <div
    :class="menuItemClasses"
    @click="handleClick"
    role="button"
    tabindex="0"
    @keydown.enter="handleClick"
    @keydown.space.prevent="handleClick"
  >
    <!-- Icon -->
    <div class="menu-icon" :style="iconStyle">
      <span v-if="config.icon" v-html="config.icon"></span>
      <svg
        v-else
        class="w-8 h-8"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M13 10V3L4 14h7v7l9-11h-7z"
        />
      </svg>
    </div>

    <!-- Content -->
    <div class="menu-content">
      <div class="flex items-center gap-2 mb-1">
        <h3 class="menu-title">{{ feature.title }}</h3>
        <!-- 제출 상태 태그 -->
        <span
          v-if="feature.isComplete !== undefined"
          :class="[
            'px-2 py-0.5 text-xs font-medium rounded',
            feature.isComplete
              ? 'bg-[#17B185]/10 text-[#17B185]'
              : 'bg-gray-100 text-gray-600'
          ]"
        >
          {{ feature.isComplete ? '제출완료' : '미제출' }}
        </span>
      </div>
      <p v-if="config.description" class="menu-description">
        {{ config.description }}
      </p>
    </div>

    <!-- Arrow indicator -->
    <div class="menu-arrow">
      <svg
        class="w-5 h-5 text-gray-400"
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

    <!-- Badge (optional) -->
    <div v-if="config.badge" class="menu-badge">
      {{ config.badge }}
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
    console.error('Failed to parse menu item config:', error)
    return {}
  }
})

// Menu item classes
const menuItemClasses = computed(() => {
  return [
    'menu-item',
    'bg-white',
    'border',
    'border-gray-200',
    'rounded-lg',
    'p-4',
    'cursor-pointer',
    'transition-all',
    'duration-200',
    'hover:shadow-md',
    'hover:border-blue-300',
    'active:scale-95',
    'relative',
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

// Handle click
function handleClick() {
  try {
    // Handle onClick object for navigation
    if (config.value.onClick && config.value.onClick.type === 'NAVIGATE' && config.value.onClick.payload) {
      if (config.value.onClick.payload.startsWith('http')) { // Check if it's an external URL
        window.open(config.value.onClick.payload, '_blank', 'noopener,noreferrer');
      } else {
        router.push(config.value.onClick.payload);
      }
      return;
    }

    // Existing logic for direct url/externalUrl (fallback or alternative)
    if (config.value.externalUrl) {
      window.open(config.value.externalUrl, '_blank', 'noopener,noreferrer');
      return;
    }

    // Internal URL
    if (config.value.url) {
      router.push(config.value.url);
      return;
    }

    // 3. Handle navigation from top-level feature.mapsTo (for ModuleLink, Link behavior types)
    if (props.feature.mapsTo) {
      // Assuming mapsTo can be internal or external, check for http prefix
      if (props.feature.mapsTo.startsWith('http')) {
        window.open(props.feature.mapsTo, '_blank', 'noopener,noreferrer');
      } else {
        router.push(props.feature.mapsTo);
      }
      return;
    }

    // Custom callback
    if (
      config.value.onClick &&
      typeof window[config.value.onClick] === 'function'
    ) {
      window[config.value.onClick](props.feature)
    }
  } catch (error) {
    console.error('Menu item click error:', error)
  }
}
</script>

<style scoped>
.menu-item {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: 1rem;
  align-items: center;
  position: relative;
}

.menu-icon {
  width: 3rem;
  height: 3rem;
  border-radius: 0.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.menu-icon :deep(svg) {
  width: 2rem;
  height: 2rem;
}

.menu-content {
  flex: 1;
  min-width: 0;
}

.menu-title {
  font-size: 1rem;
  font-weight: 600;
  color: #111827;
}

.menu-description {
  font-size: 0.875rem;
  color: #6b7280;
  line-height: 1.4;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.menu-arrow {
  flex-shrink: 0;
}

.menu-badge {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  background-color: #ef4444;
  color: white;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.125rem 0.5rem;
  border-radius: 9999px;
}

/* Mobile optimization */
@media (max-width: 640px) {
  .menu-item {
    gap: 0.75rem;
  }

  .menu-icon {
    width: 2.5rem;
    height: 2.5rem;
  }

  .menu-icon :deep(svg) {
    width: 1.5rem;
    height: 1.5rem;
  }

  .menu-title {
    font-size: 0.9375rem;
  }

  .menu-description {
    font-size: 0.8125rem;
  }
}
</style>
