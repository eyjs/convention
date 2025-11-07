<template>
  <div
    class="bg-white rounded-lg shadow-md overflow-hidden border border-gray-200 transition-all duration-200"
    :class="{ 'opacity-60 bg-gray-50': isCompleted }"
  >
    <div class="p-5 flex items-center justify-between gap-4">
      <div class="flex-1 min-w-0">
        <h3
          class="text-base font-bold text-gray-900 transition-colors"
          :class="{ 'line-through text-gray-500': isCompleted }"
        >
          {{ feature.title }}
        </h3>
        <p v-if="feature.deadline" class="text-xs text-gray-500 mt-1">
          마감: {{ new Date(feature.deadline).toLocaleDateString() }}
        </p>
      </div>

      <div class="flex-shrink-0">
        <button
          @click="completeAction"
          :disabled="isCompleted"
          class="px-4 py-2 text-sm font-bold rounded-lg transition-transform transform active:scale-95"
          :class="[
            isCompleted
              ? 'bg-green-100 text-green-700 cursor-not-allowed'
              : 'bg-blue-600 text-white hover:bg-blue-700',
          ]"
        >
          <span v-if="isCompleted">✓ 완료됨</span>
          <span v-else>완료하기</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import apiClient from '@/services/api'
import { useConventionStore } from '@/stores/convention'

const props = defineProps({
  feature: {
    type: Object,
    required: true,
  },
})

const conventionStore = useConventionStore()
const isCompleted = ref(props.feature.isComplete || false)

async function completeAction() {
  if (isCompleted.value) return

  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) {
      throw new Error('Convention ID not found')
    }

    await apiClient.post(
      `/conventions/${conventionId}/actions/${props.feature.id}/complete`,
    )

    isCompleted.value = true
    // Optionally, emit an event to notify the parent component
    // emit('actionCompleted', props.feature.id)
  } catch (error) {
    console.error('Failed to complete action:', error)
    alert('액션 완료 처리에 실패했습니다.')
  }
}
</script>
