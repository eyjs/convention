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
        <div class="text-3xl font-bold text-primary-600">
          {{ checklist.progressPercentage }}%
        </div>
        <p class="text-xs text-gray-500">진행률</p>
      </div>
    </div>

    <!-- 진행바 -->
    <div class="mb-6">
      <div class="h-3 bg-gray-100 rounded-full overflow-hidden">
        <div
          class="h-full bg-gradient-to-r from-primary-500 to-primary-600 transition-all duration-500 ease-out"
          :style="{ width: `${checklist.progressPercentage}%` }"
        ></div>
      </div>
    </div>

    <!-- 체크리스트 아이템 -->
    <div class="space-y-3">
      <router-link
        v-for="item in checklist.items"
        :key="item.actionId"
        :to="item.navigateTo"
        class="flex items-start justify-between p-4 rounded-xl border-2 transition-all group"
                                        :class="[
                                          item.isComplete
                                            ? 'border-[#17B185]'
                                            : isExpired(item.deadline)
                                              ? 'border-gray-200 bg-gray-50 opacity-60 cursor-not-allowed'
                                              : 'border-gray-200 hover:shadow-sm hover:border-primary-500 hover:bg-primary-50',
                                        ]"
                                        :style="item.isComplete ? { backgroundColor: 'rgba(23, 177, 133, 0.1)' } : {}"
        @click.prevent="
          isExpired(item.deadline) ? null : $router.push(item.navigateTo)
        "
      >
        <div class="flex items-start space-x-3 flex-1">
          <!-- 체크박스 아이콘 -->
          <div
            class="w-6 h-6 rounded-full flex items-center justify-center transition-colors flex-shrink-0 mt-0.5"
            :class="
              item.isComplete
                ? 'bg-[#17B185]'
                : 'bg-gray-200 group-hover:bg-primary-200'
            "
          >
            <svg
              v-if="item.isComplete"
              class="w-4 h-4 text-white"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M5 13l4 4L19 7"
              />
            </svg>
            <svg
              v-else
              class="w-4 h-4 text-gray-400 group-hover:text-primary-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 4v16m8-8H4"
              />
            </svg>
          </div>

          <div class="flex-1 min-w-0">
            <!-- 제목 및 긴급도 태그 -->
            <div class="flex items-center gap-2 mb-1 flex-wrap">
              <p
                class="font-medium transition-colors"
                :class="[
                  item.isComplete
                    ? 'text-[#17B185]'
                    : isExpired(item.deadline)
                      ? 'text-gray-400 line-through'
                      : 'text-gray-900 group-hover:text-primary-600',
                ]"
              >
                {{ item.title }}
              </p>
              <!-- 긴급도 태그 -->
              <span
                v-if="!item.isComplete && item.deadline"
                class="px-2 py-0.5 text-xs font-bold rounded-full flex-shrink-0"
                :class="
                  isExpired(item.deadline)
                    ? 'bg-gray-200 text-gray-600'
                    : getUrgencyLevel(item.deadline) !== 'safe'
                      ? getUrgencyClass(item.deadline)
                      : ''
                "
              >
                {{
                  isExpired(item.deadline)
                    ? '마감완료'
                    : getUrgencyText(item.deadline)
                }}
              </span>
            </div>

            <!-- 마감일 및 카운트다운 -->
            <div v-if="item.deadline" class="mt-2">
              <!-- 완료된 경우 -->
              <div v-if="item.isComplete" class="text-xs text-[#17B185]">
                <svg
                  class="w-3 h-3 inline mr-1"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M5 13l4 4L19 7"
                  />
                </svg>
                제출 완료
              </div>

              <!-- 미완료인 경우 -->
              <div v-else class="space-y-1">
                <!-- 마감된 경우 -->
                <div
                  v-if="isExpired(item.deadline)"
                  class="text-xs text-gray-400"
                >
                  <svg
                    class="w-3 h-3 inline mr-1"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                  </svg>
                  마감: {{ formatDeadline(item.deadline) }}
                </div>
                <!-- 마감 안됨 -->
                <div v-else>
                  <div class="text-xs text-gray-500">
                    마감: {{ formatDeadline(item.deadline) }}
                  </div>
                  <!-- 카운트다운 -->
                  <div
                    class="flex items-center gap-2 text-sm font-semibold"
                    :class="getTimeRemainingClass(item.deadline)"
                  >
                    <svg
                      class="w-4 h-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                      />
                    </svg>
                    <span>{{ getTimeRemaining(item.deadline) }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 화살표 -->
        <svg
          class="w-5 h-5 text-gray-400 group-hover:text-primary-600 transition-colors flex-shrink-0 mt-1"
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
      </router-link>
    </div>

    <!-- 완료 메시지 -->
    <div
      v-if="checklist.progressPercentage === 100"
            class="mt-6 p-4 rounded-xl bg-[#17B185]/10 border-2 border-[#17B185]/20"
    >
      <div class="flex items-center space-x-2 text-[#17B185]">
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
            d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        </svg>
        <span class="font-medium">모든 필수 사항을 완료했습니다! 🎉</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  checklist: {
    type: Object,
    required: true,
  },
  brandColor: {
    type: String,
    default: '#10b981', // Default to primary-600 green if not provided
  },
})

const now = ref(Date.now())
let intervalId = null

// 1초마다 현재 시간 업데이트 (각 아이템의 카운트다운을 위해)
onMounted(() => {
  intervalId = setInterval(() => {
    now.value = Date.now()
  }, 1000)
})

onUnmounted(() => {
  if (intervalId) {
    clearInterval(intervalId)
  }
})

function formatDeadline(deadline) {
  const date = new Date(deadline)
  return date.toLocaleDateString('ko-KR', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function isExpired(deadline) {
  if (!deadline) return false
  const end = new Date(deadline).getTime()
  return end <= now.value
}

function getTimeRemaining(deadline) {
  const end = new Date(deadline).getTime()
  const diff = end - now.value

  if (diff <= 0) return '마감됨'

  const days = Math.floor(diff / (1000 * 60 * 60 * 24))
  const hours = Math.floor((diff / (1000 * 60 * 60)) % 24)
  const minutes = Math.floor((diff / (1000 * 60)) % 60)
  const seconds = Math.floor((diff / 1000) % 60)

  if (days > 0) {
    return `${days}일 ${hours}시간 남음`
  } else if (hours > 0) {
    return `${hours}시간 ${minutes}분 남음`
  } else if (minutes > 0) {
    return `${minutes}분 ${seconds}초 남음`
  } else {
    return `${seconds}초 남음`
  }
}

function getTimeRemainingClass(deadline) {
  const level = getUrgencyLevel(deadline)
  if (level === 'critical') return 'text-red-600'
  if (level === 'urgent') return 'text-orange-600'
  if (level === 'warning') return 'text-yellow-600'
  return 'text-gray-600'
}

function getUrgencyLevel(deadline) {
  const end = new Date(deadline).getTime()
  const diff = end - now.value
  const hoursRemaining = diff / (1000 * 60 * 60)

  if (hoursRemaining <= 24) return 'critical' // 24시간 이내
  if (hoursRemaining <= 72) return 'urgent' // 3일 이내
  if (hoursRemaining <= 168) return 'warning' // 7일 이내
  return 'safe'
}

function getUrgencyText(deadline) {
  const level = getUrgencyLevel(deadline)
  if (level === 'critical') return '마감임박!'
  if (level === 'urgent') return '긴급'
  if (level === 'warning') return '주의'
  return ''
}

function getUrgencyClass(deadline) {
  const level = getUrgencyLevel(deadline)
  if (level === 'critical') return 'bg-red-100 text-red-700 animate-pulse'
  if (level === 'urgent') return 'bg-orange-100 text-orange-700'
  if (level === 'warning') return 'bg-yellow-100 text-yellow-700'
  return ''
}

// Helper function to convert hex to rgba
function hexToRgba(hex, alpha) {
  const r = parseInt(hex.slice(1, 3), 16);
  const g = parseInt(hex.slice(3, 5), 16);
  const b = parseInt(hex.slice(5, 7), 16);
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}
</script>
