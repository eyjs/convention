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
        :schedule="schedule"
        :isLast="index === schedulesByDate.length - 1"
      />
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
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

const formatSelectedDate = computed(() => {
  const date = dayjs(selectedDate.value)
  const today = dayjs()
  
  if (date.isSame(today, 'day')) {
    return `${date.format('M/D(ddd)')} - 오늘`
  }
  
  return date.format('M/D(ddd)')
})
</script>
