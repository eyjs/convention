<template>
  <div
    @click="toggleAction"
    class="bg-white rounded-lg p-4 flex items-center gap-3 cursor-pointer transition-all duration-200 hover:shadow-md border border-gray-200"
    :class="{ 'opacity-60': isCompleted }"
  >
    <!-- 체크박스 -->
    <div
      class="flex-shrink-0 w-6 h-6 rounded border-2 flex items-center justify-center transition-all duration-200"
      :class="[
        isCompleted
          ? 'bg-[#17B185] border-[#17B185]'
          : 'border-gray-300 hover:border-[#17B185]',
      ]"
    >
      <svg
        v-if="isCompleted"
        class="w-4 h-4 text-white"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="3"
          d="M5 13l4 4L19 7"
        />
      </svg>
    </div>

    <!-- 텍스트 영역 -->
    <div class="flex-1 min-w-0">
      <h3
        class="text-base font-medium transition-colors"
        :class="[
          isCompleted
            ? 'line-through text-gray-400'
            : 'text-gray-900',
        ]"
      >
        {{ feature.title }}
      </h3>
      <p v-if="feature.deadline && !isCompleted" class="text-xs text-gray-500 mt-0.5">
        마감: {{ new Date(feature.deadline).toLocaleDateString() }}
      </p>
    </div>

    <!-- 완료 뱃지 (옵션) -->
    <div v-if="isCompleted" class="flex-shrink-0">
      <span class="px-2 py-1 bg-[#17B185]/10 text-[#17B185] text-xs font-medium rounded">
        완료
      </span>
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

async function toggleAction() {
  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) {
      throw new Error('Convention ID not found')
    }

    // 토글: 완료 상태를 반대로 전환
    const newStatus = !isCompleted.value

    await apiClient.post(
      `/conventions/${conventionId}/actions/${props.feature.id}/toggle`,
      { isComplete: newStatus }
    )

    isCompleted.value = newStatus

    // 성공 시 간단한 피드백
    if (newStatus) {
      console.log('항목이 완료되었습니다.')
    } else {
      console.log('완료가 취소되었습니다.')
    }

    // Optionally, emit an event to notify the parent component
    // emit('actionToggled', props.feature.id, newStatus)
  } catch (error) {
    console.error('Failed to toggle action:', error)
    alert('상태 변경에 실패했습니다.')
  }
}
</script>
