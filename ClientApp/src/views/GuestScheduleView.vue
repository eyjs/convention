<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <!-- 헤더 -->
    <div class="sticky top-0 z-10 bg-white shadow">
      <div class="flex items-center p-4">
        <button @click="goBack" class="mr-3">
          <svg
            class="w-6 h-6"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 19l-7-7 7-7"
            />
          </svg>
        </button>
        <h1 class="text-xl font-bold">전체 일정</h1>
      </div>
    </div>

    <!-- 통계 -->
    <div class="p-4 bg-primary-50">
      <div class="grid grid-cols-3 gap-3">
        <div class="bg-white rounded-lg p-3 text-center">
          <p class="text-2xl font-bold text-primary-600">
            {{ totalSchedules }}
          </p>
          <p class="text-xs text-gray-600 mt-1">전체 일정</p>
        </div>
        <div class="bg-white rounded-lg p-3 text-center">
          <p class="text-2xl font-bold text-green-600">{{ completedCount }}</p>
          <p class="text-xs text-gray-600 mt-1">완료</p>
        </div>
        <div class="bg-white rounded-lg p-3 text-center">
          <p class="text-2xl font-bold text-blue-600">{{ remainingCount }}</p>
          <p class="text-xs text-gray-600 mt-1">남은 일정</p>
        </div>
      </div>
    </div>

    <!-- 날짜별 일정 -->
    <div class="p-4">
      <div v-for="(group, index) in schedulesByDate" :key="index" class="mb-8">
        <!-- 날짜 헤더 -->
        <div
          class="sticky top-16 bg-gradient-to-r from-primary-600 to-primary-700 text-white rounded-lg p-3 mb-3 shadow"
        >
          <div class="flex items-center justify-between">
            <div>
              <h2 class="text-lg font-bold">
                {{ formatDateHeader(group.date) }}
              </h2>
              <p class="text-sm opacity-90">
                {{ group.schedules.length }}개 일정
              </p>
            </div>
            <div
              v-if="isToday(group.date)"
              class="bg-white/20 px-3 py-1 rounded-full text-xs font-semibold"
            >
              오늘
            </div>
            <div
              v-else-if="isTomorrow(group.date)"
              class="bg-white/20 px-3 py-1 rounded-full text-xs font-semibold"
            >
              내일
            </div>
          </div>
        </div>

        <!-- 일정 카드 -->
        <div class="space-y-3">
          <div
            v-for="schedule in group.schedules"
            :key="schedule.id"
            class="bg-white rounded-lg shadow hover:shadow-md transition-all p-4"
            :class="{
              'border-l-4 border-green-500': isPast(schedule.scheduleDate),
              'border-l-4 border-blue-500': isCurrent(schedule.scheduleDate),
              'border-l-4 border-gray-300': isFuture(schedule.scheduleDate),
            }"
          >
            <!-- 시간 & 제목 -->
            <div class="flex items-start justify-between mb-3">
              <div class="flex-1">
                <div class="flex items-center gap-3 mb-2">
                  <span class="text-xl font-bold text-primary-600">
                    {{ formatTime(schedule.scheduleDate) }}
                  </span>
                  <div
                    v-if="isCurrent(schedule.scheduleDate)"
                    class="flex items-center gap-1"
                  >
                    <div
                      class="w-2 h-2 bg-blue-500 rounded-full animate-pulse"
                    ></div>
                    <span class="text-xs font-semibold text-blue-600"
                      >진행중</span
                    >
                  </div>
                </div>
                <h3 class="text-lg font-semibold mb-2">{{ schedule.title }}</h3>

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
                  class="text-gray-700 text-sm whitespace-pre-wrap leading-relaxed"
                >
                  {{ schedule.content }}
                </p>
              </div>

              <!-- 상태 아이콘 -->
              <div
                v-if="isPast(schedule.scheduleDate)"
                class="flex-shrink-0 ml-3"
              >
                <svg
                  class="w-6 h-6 text-green-500"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                >
                  <path
                    fill-rule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                    clip-rule="evenodd"
                  />
                </svg>
              </div>
            </div>

            <!-- 시작까지 남은 시간 -->
            <div
              v-if="
                isFuture(schedule.scheduleDate) &&
                !isCurrent(schedule.scheduleDate)
              "
              class="text-xs text-blue-600 bg-blue-50 px-2 py-1 rounded inline-block"
            >
              {{ getTimeUntil(schedule.scheduleDate) }} 후
            </div>
          </div>
        </div>
      </div>

      <div
        v-if="schedulesByDate.length === 0"
        class="text-center py-12 text-gray-500"
      >
        등록된 일정이 없습니다
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/services/api'

const router = useRouter()

const props = defineProps({
  guestId: { type: Number, required: true },
})

const schedulesByDate = ref([])
const allSchedules = ref([])

const totalSchedules = computed(() => allSchedules.value.length)
const completedCount = computed(
  () => allSchedules.value.filter((s) => isPast(s.scheduleDate)).length,
)
const remainingCount = computed(
  () => allSchedules.value.filter((s) => !isPast(s.scheduleDate)).length,
)

const formatDateHeader = (dateString) => {
  const date = new Date(dateString)
  const month = date.getMonth() + 1
  const day = date.getDate()
  const weekdays = ['일', '월', '화', '수', '목', '금', '토']
  const weekday = weekdays[date.getDay()]

  return `${month}월 ${day}일 (${weekday})`
}

const formatTime = (dateString) => {
  const date = new Date(dateString)
  return `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`
}

const isToday = (dateString) => {
  const date = new Date(dateString)
  const today = new Date()
  return date.toDateString() === today.toDateString()
}

const isTomorrow = (dateString) => {
  const date = new Date(dateString)
  const tomorrow = new Date()
  tomorrow.setDate(tomorrow.getDate() + 1)
  return date.toDateString() === tomorrow.toDateString()
}

const isPast = (dateString) => {
  return new Date(dateString) < new Date()
}

const isFuture = (dateString) => {
  return new Date(dateString) > new Date()
}

const isCurrent = (dateString) => {
  const now = new Date()
  const scheduleTime = new Date(dateString)
  const endTime = new Date(scheduleTime.getTime() + 2 * 60 * 60 * 1000) // 2시간 후
  return scheduleTime <= now && now <= endTime
}

const getTimeUntil = (dateString) => {
  const now = new Date()
  const scheduleTime = new Date(dateString)
  const diff = scheduleTime - now

  if (diff < 0) return ''

  const days = Math.floor(diff / (1000 * 60 * 60 * 24))
  const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60))
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60))

  if (days > 0) return `${days}일 ${hours}시간`
  if (hours > 0) return `${hours}시간 ${minutes}분`
  return `${minutes}분`
}

const loadSchedules = async () => {
  try {
    const response = await apiClient.get(
      `/api/guestschedule/guests/${props.guestId}/all`,
    )

    schedulesByDate.value = response.data.byDate
    allSchedules.value = response.data.allSchedules
  } catch (error) {
    console.error('Failed to load schedules:', error)
  }
}

const goBack = () => {
  router.back()
}

onMounted(() => {
  loadSchedules()
})
</script>
