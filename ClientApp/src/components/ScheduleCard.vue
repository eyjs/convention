<template>
  <div class="max-w-4xl mx-auto p-4">
    <h2 class="text-2xl font-bold mb-6">스케줄표</h2>

    <!-- 날짜별 그룹 -->
    <div
      v-for="(dateGroup, dateKey) in groupedSchedules"
      :key="dateKey"
      class="mb-8"
    >
      <div
        class="sticky top-0 bg-white z-10 py-3 border-b-2 border-primary-600"
      >
        <h3 class="text-xl font-bold text-primary-600">
          {{ formatDateHeader(dateKey) }}
        </h3>
      </div>

      <!-- 일정 카드 -->
      <div class="space-y-4 mt-4">
        <div
          v-for="schedule in dateGroup"
          :key="schedule.id"
          class="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow p-4 border-l-4 border-primary-500"
        >
          <div class="flex items-start justify-between">
            <div class="flex-1">
              <div class="flex items-center gap-3 mb-2">
                <span class="text-lg font-bold text-primary-600">
                  {{ formatTime(schedule.scheduleDate) }}
                </span>
                <h4 class="text-lg font-semibold">{{ schedule.title }}</h4>
              </div>

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

              <p
                v-if="schedule.content"
                class="text-gray-700 whitespace-pre-wrap leading-relaxed"
              >
                {{ schedule.content }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div
      v-if="Object.keys(groupedSchedules).length === 0"
      class="text-center py-12 text-gray-500"
    >
      등록된 일정이 없습니다
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/api'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const schedules = ref([])

const groupedSchedules = computed(() => {
  const groups = {}

  schedules.value.forEach((schedule) => {
    const date = new Date(schedule.scheduleDate)
    const dateKey = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`

    if (!groups[dateKey]) {
      groups[dateKey] = []
    }

    groups[dateKey].push(schedule)
  })

  // 각 그룹 내에서 시간순 정렬
  Object.keys(groups).forEach((key) => {
    groups[key].sort(
      (a, b) => new Date(a.scheduleDate) - new Date(b.scheduleDate),
    )
  })

  return groups
})

const formatDateHeader = (dateKey) => {
  const [year, month, day] = dateKey.split('-')
  const date = new Date(year, month - 1, day)
  const weekdays = ['일', '월', '화', '수', '목', '금', '토']
  const weekday = weekdays[date.getDay()]

  return `${month}월 ${day}일 (${weekday})`
}

const formatTime = (dateString) => {
  const date = new Date(dateString)
  const hour = String(date.getHours()).padStart(2, '0')
  const minute = String(date.getMinutes()).padStart(2, '0')
  return `${hour}:${minute}`
}

const loadSchedules = async () => {
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/schedule-templates`,
    )

    // ScheduleTemplate에서 ScheduleItem 추출
    const allSchedules = []
    response.data.forEach((template) => {
      if (template.scheduleItems && template.scheduleItems.length > 0) {
        template.scheduleItems.forEach((item) => {
          allSchedules.push(item)
        })
      }
    })

    schedules.value = allSchedules
  } catch (error) {
    console.error('Failed to load schedules:', error)
  }
}

onMounted(() => {
  loadSchedules()
})
</script>
