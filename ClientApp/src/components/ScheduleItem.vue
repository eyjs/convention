<template>
  <div
    :class="[
      'bg-white rounded-lg shadow-md hover:shadow-lg transition-all p-4',
      isCurrent
        ? 'border-l-4 border-primary-500 bg-primary-50 ring-2 ring-primary-300'
        : 'border-l-4 border-gray-300',
      !isLast ? 'mb-4' : '',
    ]"
  >
    <div class="flex items-start justify-between">
      <div class="flex-1">
        <!-- 시간과 제목 -->
        <div class="flex items-center gap-3 mb-2">
          <span
            :class="[
              'text-lg font-bold',
              isCurrent ? 'text-primary-600' : 'text-gray-600',
            ]"
          >
            {{ formatTime(schedule.startTime) }}
          </span>
          <h4 class="text-lg font-semibold">{{ schedule.title }}</h4>
          <span
            v-if="isCurrent"
            class="ml-auto px-2 py-1 bg-primary-600 text-white text-xs rounded-full"
          >
            진행 중
          </span>
        </div>

        <!-- 장소 -->
        <div
          v-if="schedule.location"
          class="flex items-center gap-2 text-sm text-gray-600 mb-2"
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
              d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
            />
          </svg>
          <span>{{ schedule.location }}</span>
        </div>

        <!-- 내용 -->
        <p
          v-if="schedule.content"
          class="text-gray-700 whitespace-pre-wrap leading-relaxed text-sm"
        >
          {{ schedule.content }}
        </p>

        <!-- 종료 시간 -->
        <div v-if="schedule.endTime" class="mt-2 text-xs text-gray-500">
          ~ {{ formatTime(schedule.endTime) }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  schedule: {
    type: Object,
    required: true,
  },
  isCurrent: {
    type: Boolean,
    default: false,
  },
  isLast: {
    type: Boolean,
    default: false,
  },
})

const formatTime = (timeString) => {
  if (!timeString) return ''

  // HH:mm:ss 형식에서 HH:mm만 추출
  const parts = timeString.split(':')
  if (parts.length >= 2) {
    return `${parts[0]}:${parts[1]}`
  }
  return timeString
}
</script>
