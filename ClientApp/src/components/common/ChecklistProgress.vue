<template>
  <div
    v-if="checklist.progressPercentage !== 100"
    class="bg-white rounded-2xl shadow-lg p-6"
  >
    <div class="flex items-center justify-between mb-4">
      <div>
        <h3 class="text-lg font-bold text-gray-900">필수 제출 사항</h3>
        <p class="text-sm text-gray-600 mt-1">
          {{ checklist.completedItems }} / {{ checklist.totalItems }} 완료
        </p>
      </div>
      <div class="text-right">
        <div class="text-3xl font-bold" :style="{ color: brandColor }">
          {{ checklist.progressPercentage }}%
        </div>
        <p class="text-xs text-gray-500">진행률</p>
      </div>
    </div>

    <!-- 진행바 -->
    <div class="mb-6">
      <div class="h-3 bg-gray-100 rounded-full overflow-hidden">
        <div
          class="h-full transition-all duration-500 ease-out"
          :style="{
            width: `${checklist.progressPercentage}%`,
            background: `linear-gradient(to right, ${brandColor}, ${hexToRgba(brandColor, 0.8)})`
          }"
        ></div>
      </div>
    </div>

    <!-- 체크리스트 아이템 -->
    <div class="space-y-3">
      <div
        v-for="item in checklist.items"
        :key="item.actionId"
        class="flex items-start justify-between p-4 rounded-xl border-2 transition-all group cursor-pointer"
        :class="[
          item.isComplete
            ? 'border-green-500'
            : isExpired(item.deadline)
              ? 'border-gray-200 bg-gray-50 opacity-60 cursor-not-allowed'
              : 'border-gray-200 hover:shadow-sm hover:border-blue-500 hover:bg-blue-50',
        ]"
        :style="item.isComplete ? { backgroundColor: hexToRgba(brandColor, 0.1), borderColor: brandColor } : {}"
        @click.prevent="isExpired(item.deadline) ? null : handleItemClick(item)"
      >
        <div class="flex items-start space-x-3 flex-1">
          <div
            class="w-6 h-6 rounded-full flex items-center justify-center transition-colors flex-shrink-0 mt-0.5"
            :class="
              item.isComplete
                ? 'bg-green-500'
                : 'bg-gray-200 group-hover:bg-blue-200'
            "
            :style="{ backgroundColor: item.isComplete ? brandColor : '' }"
          >
            <svg v-if="item.isComplete" class="w-4 h-4 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
            </svg>
          </div>

          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-1 flex-wrap">
              <p
                class="font-medium transition-colors"
                :class="[
                  item.isComplete
                    ? 'text-green-600'
                    : isExpired(item.deadline)
                      ? 'text-gray-400 line-through'
                      : 'text-gray-900 group-hover:text-blue-600',
                ]"
                :style="{ color: item.isComplete ? brandColor : '' }"
              >
                {{ item.title }}
              </p>
            </div>
          </div>
        </div>

        <svg class="w-5 h-5 text-gray-400 group-hover:text-blue-600 transition-colors flex-shrink-0 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
        </svg>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useAction } from '@/composables/useAction'

const props = defineProps({
  checklist: {
    type: Object,
    required: true,
  },
  brandColor: {
    type: String,
    default: '#10b981',
  },
})

const { executeAction } = useAction()

function handleItemClick(item) {
  // API 응답 객체의 속성 이름은 camelCase일 수 있으므로,
  // action 객체를 재구성하여 executeAction에 전달합니다.
  const action = {
    id: item.actionId,
    title: item.title,
    behaviorType: item.behaviorType,
    targetId: item.targetId,
    mapsTo: item.navigateTo,
  };
  executeAction(action);
}

function isExpired(deadline) {
  if (!deadline) return false
  return new Date(deadline).getTime() <= Date.now()
}

function hexToRgba(hex, alpha) {
  const r = parseInt(hex.slice(1, 3), 16);
  const g = parseInt(hex.slice(3, 5), 16);
  const b = parseInt(hex.slice(5, 7), 16);
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}
</script>