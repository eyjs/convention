<template>
  <div>
    <!-- 일정표 헤더 -->
    <div class="flex items-center space-x-2 mb-4 mt-2">
      <div class="w-6 h-6 text-ifa-green">
        <CalendarIcon />
      </div>
      <h3 class="text-lg font-bold">일정표</h3>
    </div>

    <!-- 날짜 필터 -->
    <DateFilter />

    <!-- 현재 선택된 날짜 -->
    <div class="mb-4">
      <h4 class="text-base font-semibold text-gray-800">
        {{ formatSelectedDate }}
      </h4>
    </div>

    <!-- 일정 목록 -->
    <div v-if="loading" class="flex justify-center py-8">
      <LoadingSpinner />
    </div>

    <div v-else-if="schedulesByDate.length === 0" class="text-center py-8 text-gray-500">
      선택한 날짜에 일정이 없습니다.
    </div>

    <div v-else class="space-y-4">
      <ScheduleItem
        v-for="(schedule, index) in schedulesByDate"
        :key="schedule.id"
        :ref="el => { if (index === currentScheduleIndex) currentScheduleRef = el }"
        :schedule="schedule"
        :isLast="index === schedulesByDate.length - 1"
        :isCurrent="index === currentScheduleIndex"
      />
    </div>
  </div>
</template>

<script setup>
import { computed, watch, ref, nextTick } from 'vue'
import { useConventionStore } from '@/stores/convention'
import { storeToRefs } from 'pinia'
import dayjs from 'dayjs'
import DateFilter from '@/components/DateFilter.vue'
import ScheduleItem from '@/components/ScheduleItem.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import CalendarIcon from '@/components/icons/CalendarIcon.vue'

const conventionStore = useConventionStore()
const { selectedDate, loading } = storeToRefs(conventionStore)
const schedulesByDate = computed(() => conventionStore.getSchedulesByDate)
const currentScheduleRef = ref(null)

const formatSelectedDate = computed(() => {
  const date = dayjs(selectedDate.value)
  const today = dayjs()

  if (date.isSame(today, 'day')) {
    return `${date.format('M/D(ddd)')} - 오늘`
  }

  return date.format('M/D(ddd)')
})

// 현재 시간에 가장 가까운 일정 인덱스 계산
const currentScheduleIndex = computed(() => {
  if (!schedulesByDate.value.length) return -1

  // 오늘이 아니면 첫 번째 일정 반환
  const today = dayjs()
  const selectedDay = dayjs(selectedDate.value)
  if (!selectedDay.isSame(today, 'day')) {
    return 0
  }

  const now = dayjs()
  console.log('=== Schedule Auto-Scroll Debug ===')
  console.log('Current time:', now.format('YYYY-MM-DD HH:mm:ss'))
  console.log('Selected date:', selectedDate.value)
  console.log('Total schedules:', schedulesByDate.value.length)

  // 현재 시간 이전의 가장 마지막 일정을 찾기
  let currentIndex = -1
  for (let i = 0; i < schedulesByDate.value.length; i++) {
    const schedule = schedulesByDate.value[i]
    const scheduleTime = schedule.startTime || '00:00:00'

    // 오늘 날짜에 일정 시간을 합쳐서 dayjs 객체로 비교
    const scheduleDateTime = dayjs(`${selectedDate.value} ${scheduleTime}`)

    console.log(`Schedule ${i}: ${schedule.title}`)
    console.log(`  startTime: ${scheduleTime}`)
    console.log(`  dateTime: ${scheduleDateTime.format('YYYY-MM-DD HH:mm:ss')}`)
    console.log(`  isBefore: ${scheduleDateTime.isBefore(now)}`)
    console.log(`  isSame: ${scheduleDateTime.isSame(now)}`)

    if (scheduleDateTime.isBefore(now) || scheduleDateTime.isSame(now)) {
      currentIndex = i
      console.log(`  ✓ This is current (index ${i})`)
    } else {
      console.log(`  ✗ Future schedule, stopping`)
      break
    }
  }

  console.log('Final currentIndex:', currentIndex)
  console.log('================================')

  // 현재 시간 이전 일정이 없으면 첫 번째 일정
  return currentIndex === -1 ? 0 : currentIndex
})

// 현재 일정으로 자동 스크롤
watch([schedulesByDate, currentScheduleIndex], async () => {
  if (currentScheduleIndex.value >= 0) {
    await nextTick()
    if (currentScheduleRef.value && currentScheduleRef.value.$el) {
      currentScheduleRef.value.$el.scrollIntoView({
        behavior: 'smooth',
        block: 'center'
      })
    }
  }
}, { immediate: true })
</script>
