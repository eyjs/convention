<!--
  Dynamic Action Renderer

  Renders multiple inline actions (buttons, cards, etc.) based on actionCategory.
  Used for placing features at specific locations within pages.

  Usage:
    <DynamicActionRenderer :features="featuresArray" />

  Props:
    - features: Array of action objects with actionCategory field
-->

<template>
  <div v-if="features && features.length > 0" class="dynamic-actions-container">
    <template
      v-for="feature in features"
      :key="feature.id || feature.actionType"
    >
      <component
        v-if="resolveComponent(feature.actionCategory)"
        :is="resolveComponent(feature.actionCategory)"
        :feature="feature"
      />
      <div
        v-else
        class="p-4 bg-yellow-50 border border-yellow-200 rounded-lg text-sm text-yellow-800"
      >
        ⚠️ Unknown action category: {{ feature.actionCategory }}
      </div>
    </template>
  </div>
</template>

<script setup>
import { defineAsyncComponent } from 'vue'

const props = defineProps({
  features: {
    type: Array,
    required: true,
    default: () => [],
  },
})

/**
 * Component mapping based on actionCategory
 * Maps action category keys to their corresponding generic components
 */
const componentMap = {
  BUTTON: defineAsyncComponent(() => import('./common/GenericButton.vue')),
  MENU: defineAsyncComponent(() => import('./common/GenericMenuItem.vue')),
  AUTO_POPUP: defineAsyncComponent(
    () => import('./common/GenericAutoPopup.vue'),
  ),
  BANNER: defineAsyncComponent(() => import('./common/GenericBanner.vue')),
  CARD: defineAsyncComponent(() => import('./common/GenericCard.vue')),
}

/**
 * Resolve component based on action category
 * @param {string} category - The action category key
 * @returns {Component|null} - The resolved component or null
 */
const resolveComponent = (category) => {
  if (!category) {
    console.warn('Action category is missing')
    return null
  }

  const component = componentMap[category]

  if (!component) {
    console.warn(`No component found for category: ${category}`)
  }

  return component
}
</script>

<style scoped>
.dynamic-actions-container {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

/* Allow horizontal layout for certain action types */
.dynamic-actions-container.horizontal {
  flex-direction: row;
  flex-wrap: wrap;
}
</style>
