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
    <!-- CHECKLIST_CARD 그룹 처리 -->
    <div v-if="checklistItems.length > 0" class="bg-white rounded-2xl shadow-lg p-5">
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-lg font-bold text-gray-900">필수 제출 사항</h2>
        <button
          v-if="checklistItems.length > 3"
          @click="isChecklistExpanded = !isChecklistExpanded"
          class="text-sm text-[#17B185] font-medium flex items-center hover:underline"
        >
          {{ isChecklistExpanded ? '접기' : `더보기 (${checklistItems.length - 3})` }}
          <svg
            class="w-4 h-4 ml-0.5 transition-transform"
            :class="{ 'rotate-180': isChecklistExpanded }"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M19 9l-7 7-7-7"
            />
          </svg>
        </button>
      </div>
      <div class="space-y-3">
        <component
          v-for="(feature, index) in displayedChecklistItems"
          :key="feature.id"
          :is="resolveComponent('CHECKLIST_CARD')"
          :feature="feature"
        />
      </div>
    </div>

    <!-- 다른 액션 카테고리 렌더링 -->
    <template
      v-for="feature in nonChecklistFeatures"
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
import { defineAsyncComponent, ref, computed } from 'vue'

const props = defineProps({
  features: {
    type: Array,
    required: true,
    default: () => [],
  },
})

const isChecklistExpanded = ref(false)

// CHECKLIST_CARD와 다른 카테고리 분리
const checklistItems = computed(() =>
  props.features.filter(f => f.actionCategory === 'CHECKLIST_CARD')
)

const nonChecklistFeatures = computed(() =>
  props.features.filter(f => f.actionCategory !== 'CHECKLIST_CARD')
)

// 표시할 체크리스트 아이템 (펼침 상태에 따라 다름)
const displayedChecklistItems = computed(() => {
  if (isChecklistExpanded.value || checklistItems.value.length <= 3) {
    return checklistItems.value
  }
  return checklistItems.value.slice(0, 3)
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
  CHECKLIST_CARD: defineAsyncComponent(() => import('./common/ChecklistCard.vue')),
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
