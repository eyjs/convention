<template>
  <div class="min-h-screen min-h-dvh bg-gray-50 pb-20">
    <!-- 헤더 -->
    <div
      class="bg-gradient-to-r from-primary-600 to-primary-700 text-white p-6 shadow-lg"
    >
      <h1 class="text-2xl font-bold">{{ conventionTitle }}</h1>
      <p class="text-sm opacity-90 mt-1">{{ guestName }}님 환영합니다</p>
    </div>

    <!-- 현재 진행중인 일정 -->
    <div v-if="currentSchedule" class="p-4">
      <div
        class="bg-white rounded-lg shadow-lg p-5 border-l-4 border-green-500"
      >
        <div class="flex items-center gap-2 mb-2">
          <div class="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
          <span class="text-sm font-semibold text-green-700">진행중</span>
        </div>
        <div class="flex items-center gap-3 mb-2">
          <span class="text-xl font-bold text-primary-600">{{
            formatTime(currentSchedule.scheduleDate)
          }}</span>
          <h3 class="text-lg font-bold">{{ currentSchedule.title }}</h3>
        </div>
        <p
          v-if="currentSchedule.location"
          class="text-sm text-gray-600 flex items-center gap-1 mb-2"
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
          </svg>
          {{ currentSchedule.location }}
        </p>
        <p v-if="currentSchedule.content" class="text-gray-700 text-sm">
          {{ currentSchedule.content }}
        </p>
      </div>
    </div>

    <!-- 다음 일정 -->
    <div v-if="nextSchedule" class="px-4 pb-4">
      <h2
        class="text-sm font-semibold text-gray-600 mb-2 flex items-center gap-2"
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
            d="M13 7l5 5m0 0l-5 5m5-5H6"
          />
        </svg>
        다음 일정
      </h2>
      <div class="bg-white rounded-lg shadow p-4 border-l-4 border-blue-500">
        <div class="flex items-center gap-3 mb-2">
          <span class="text-lg font-bold text-primary-600">{{
            formatTime(nextSchedule.scheduleDate)
          }}</span>
          <h3 class="text-base font-semibold">{{ nextSchedule.title }}</h3>
        </div>
        <p
          v-if="nextSchedule.location"
          class="text-sm text-gray-600 flex items-center gap-1"
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
          </svg>
          {{ nextSchedule.location }}
        </p>
        <p v-if="timeUntilNext" class="text-xs text-blue-600 mt-2">
          {{ timeUntilNext }} 후 시작
        </p>
      </div>
    </div>

    <!-- 오늘 일정 요약 -->
    <div class="px-4 pb-4">
      <div class="flex justify-between items-center mb-3">
        <h2 class="text-lg font-bold">오늘 일정</h2>
        <span class="text-sm text-gray-500">{{ todaySchedules.length }}개</span>
      </div>

      <div
        v-if="todaySchedules.length === 0"
        class="bg-white rounded-lg shadow p-6 text-center text-gray-500"
      >
        오늘 일정이 없습니다
      </div>

      <div v-else class="space-y-2">
        <div
          v-for="schedule in todaySchedules"
          :key="schedule.id"
          class="bg-white rounded-lg shadow p-3 hover:shadow-md transition-shadow"
          :class="{ 'opacity-50': isPast(schedule.scheduleDate) }"
        >
          <div class="flex items-center gap-3">
            <span class="text-base font-bold text-primary-600 w-16">{{
              formatTime(schedule.scheduleDate)
            }}</span>
            <div class="flex-1">
              <p class="font-semibold">{{ schedule.title }}</p>
              <p v-if="schedule.location" class="text-xs text-gray-500">
                {{ schedule.location }}
              </p>
            </div>
            <svg
              v-if="isPast(schedule.scheduleDate)"
              class="w-5 h-5 text-green-500"
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
      </div>
    </div>

    <!-- 내일 일정 미리보기 -->
    <div v-if="tomorrowSchedules.length > 0" class="px-4 pb-4">
      <h2 class="text-lg font-bold mb-3">내일 일정</h2>
      <div class="space-y-2">
        <div
          v-for="schedule in tomorrowSchedules.slice(0, 3)"
          :key="schedule.id"
          class="bg-white rounded-lg shadow p-3"
        >
          <div class="flex items-center gap-3">
            <span class="text-base font-bold text-gray-600 w-16">{{
              formatTime(schedule.scheduleDate)
            }}</span>
            <div class="flex-1">
              <p class="font-semibold">{{ schedule.title }}</p>
              <p v-if="schedule.location" class="text-xs text-gray-500">
                {{ schedule.location }}
              </p>
            </div>
          </div>
        </div>
        <button
          v-if="tomorrowSchedules.length > 3"
          @click="goToFullSchedule"
          class="w-full py-2 text-sm text-primary-600 hover:bg-primary-50 rounded-lg"
        >
          + {{ tomorrowSchedules.length - 3 }}개 더보기
        </button>
      </div>
    </div>

    <!-- 전체 일정 보기 버튼 -->
    <div class="fixed bottom-0 left-0 right-0 p-4 bg-white border-t shadow-lg">
      <button
        @click="goToFullSchedule"
        class="w-full py-3 bg-primary-600 text-white rounded-lg font-semibold hover:bg-primary-700 transition-colors"
      >
        전체 일정 보기
      </button>
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
  conventionTitle: { type: String, default: '행사' },
  guestName: { type: String, default: '참석자' },
})

const currentSchedule = ref(null)
const nextSchedule = ref(null)
const todaySchedules = ref([])
const tomorrowSchedules = ref([])

const timeUntilNext = computed(() => {
  if (!nextSchedule.value) return null

  const now = new Date()
  const nextTime = new Date(nextSchedule.value.scheduleDate)
  const diff = nextTime - now

  if (diff < 0) return null

  const hours = Math.floor(diff / (1000 * 60 * 60))
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60))

  if (hours > 0) {
    return `${hours}시간 ${minutes}분`
  }
  return `${minutes}분`
})

const formatTime = (dateString) => {
  const date = new Date(dateString)
  return `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`
}

const isPast = (dateString) => {
  return new Date(dateString) < new Date()
}

const loadSchedules = async () => {
  try {
    const response = await apiClient.get(
      `/api/guestschedule/guests/${props.guestId}/today-next`,
    )

    currentSchedule.value = response.data.current
    nextSchedule.value = response.data.next
    todaySchedules.value = response.data.today
    tomorrowSchedules.value = response.data.tomorrow
  } catch (error) {
    console.error('Failed to load schedules:', error)
  }
}

const goToFullSchedule = () => {
  router.push(`/guest/schedule/${props.guestId}`)
}

onMounted(() => {
  loadSchedules()

  // 1분마다 갱신
  setInterval(loadSchedules, 60000)
})
</script>
