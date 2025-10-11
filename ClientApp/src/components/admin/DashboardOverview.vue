<template>
  <div>
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mb-8">
      <div class="bg-white rounded-lg shadow p-6">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">총 참석자</p>
            <p class="text-2xl font-bold mt-1">{{ stats.totalGuests }}</p>
          </div>
          <div class="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"/>
            </svg>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow p-6">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">총 일정 코스</p>
            <p class="text-2xl font-bold mt-1">{{ stats.totalSchedules }}</p>
          </div>
          <div class="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
            </svg>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow p-6">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">일정 배정</p>
            <p class="text-2xl font-bold mt-1">{{ stats.scheduleAssignments }}</p>
          </div>
          <div class="w-12 h-12 bg-orange-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/>
            </svg>
          </div>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <div class="bg-white rounded-lg shadow">
        <div class="p-4 sm:p-6 border-b">
          <h3 class="text-lg font-semibold">최근 등록된 참석자</h3>
        </div>
        <div class="p-4 sm:p-6">
          <div v-if="recentGuests.length === 0" class="text-center text-gray-500 py-8">
            등록된 참석자가 없습니다
          </div>
          <div v-else class="space-y-3">
            <div
              v-for="guest in recentGuests"
              :key="guest.id"
              class="flex items-center justify-between p-3 bg-gray-50 rounded-lg hover:bg-gray-100 cursor-pointer"
              @click="$emit('show-guest', guest.id)"
            >
              <div class="flex-1 min-w-0">
                <p class="font-medium truncate">{{ guest.guestName }}</p>
                <p class="text-sm text-gray-500 truncate">{{ guest.corpPart || '부서 미지정' }}</p>
              </div>
              <span class="text-xs text-gray-400 ml-2">방금</span>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow">
        <div class="p-4 sm:p-6 border-b">
          <h3 class="text-lg font-semibold">일정 코스별 현황</h3>
        </div>
        <div class="p-4 sm:p-6">
          <div v-if="scheduleStats.length === 0" class="text-center text-gray-500 py-8">
            등록된 일정이 없습니다
          </div>
          <div v-else class="space-y-3">
            <div
              v-for="schedule in scheduleStats"
              :key="schedule.id"
              class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
            >
              <div class="flex-1 min-w-0">
                <p class="font-medium truncate">{{ schedule.courseName }}</p>
                <p class="text-sm text-gray-500">{{ schedule.itemCount }}개 항목</p>
              </div>
              <span class="text-sm font-medium text-primary-600">{{ schedule.guestCount }}명</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/services/api'

const props = defineProps({
  conventionId: { type: Number, required: true }
})

defineEmits(['show-guest'])

const stats = ref({
  totalGuests: 0,
  totalSchedules: 0,
  scheduleAssignments: 0
})

const recentGuests = ref([])
const scheduleStats = ref([])

const loadStats = async () => {
  try {
    const response = await apiClient.get(`/admin/conventions/${props.conventionId}/stats`)
    stats.value = {
      totalGuests: response.data.totalGuests,
      totalSchedules: response.data.totalSchedules,
      scheduleAssignments: response.data.scheduleAssignments
    }
    recentGuests.value = response.data.recentGuests
    scheduleStats.value = response.data.scheduleStats
  } catch (error) {
    console.error('Failed to load stats:', error)
  }
}

onMounted(() => {
  loadStats()
})
</script>
