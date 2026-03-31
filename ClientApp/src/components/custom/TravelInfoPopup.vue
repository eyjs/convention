<template>
  <div class="space-y-3">
    <!-- 로딩 -->
    <div v-if="loading" class="flex justify-center py-8">
      <div
        class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"
      ></div>
    </div>

    <!-- 데이터 없음 -->
    <div
      v-else-if="!days || days.length === 0"
      class="text-center py-8 text-gray-400"
    >
      <p class="text-lg mb-1">배정 정보가 없습니다</p>
      <p class="text-sm">관리자에게 문의해주세요</p>
    </div>

    <!-- 일자별 카드 -->
    <div
      v-for="day in days"
      :key="day.date"
      class="bg-white border border-gray-200 rounded-xl overflow-hidden"
    >
      <!-- 날짜 헤더 -->
      <div class="bg-primary-50 px-4 py-2.5 border-b border-primary-100">
        <span class="font-bold text-primary-800 text-sm">{{
          formatDate(day.date)
        }}</span>
      </div>

      <div class="p-4 space-y-2.5">
        <!-- 호차 -->
        <div v-if="day.bus" class="flex items-center gap-3">
          <span
            class="w-8 h-8 bg-blue-50 rounded-lg flex items-center justify-center text-base flex-shrink-0"
            >🚌</span
          >
          <div>
            <p class="text-xs text-gray-400">호차</p>
            <p class="font-semibold text-gray-900">{{ day.bus }}</p>
          </div>
        </div>

        <!-- 호텔 -->
        <div v-if="day.hotel" class="flex items-center gap-3">
          <span
            class="w-8 h-8 bg-amber-50 rounded-lg flex items-center justify-center text-base flex-shrink-0"
            >🏨</span
          >
          <div>
            <p class="text-xs text-gray-400">호텔</p>
            <p class="font-semibold text-gray-900">{{ day.hotel }}</p>
          </div>
        </div>

        <!-- 방번호 -->
        <div class="flex items-center gap-3">
          <span
            class="w-8 h-8 bg-green-50 rounded-lg flex items-center justify-center text-base flex-shrink-0"
            >🔑</span
          >
          <div>
            <p class="text-xs text-gray-400">방번호</p>
            <p v-if="day.room" class="font-semibold text-gray-900">
              {{ day.room }}호
            </p>
            <span
              v-else
              class="inline-block px-2 py-0.5 bg-gray-100 text-gray-400 text-xs rounded-full"
              >배정 전</span
            >
          </div>
        </div>

        <!-- 룸메이트 -->
        <div
          v-if="day.roommates && day.roommates.length > 0"
          class="flex items-start gap-3"
        >
          <span
            class="w-8 h-8 bg-purple-50 rounded-lg flex items-center justify-center text-base flex-shrink-0"
            >👥</span
          >
          <div>
            <p class="text-xs text-gray-400">룸메이트</p>
            <div class="flex flex-wrap gap-1.5 mt-0.5">
              <span
                v-for="name in day.roommates"
                :key="name"
                class="inline-block px-2 py-0.5 bg-purple-50 text-purple-700 text-xs rounded-full"
                >{{ name }}</span
              >
            </div>
          </div>
        </div>

        <!-- 메모 -->
        <div v-if="day.memo" class="flex items-start gap-3">
          <span
            class="w-8 h-8 bg-orange-50 rounded-lg flex items-center justify-center text-base flex-shrink-0"
            >📝</span
          >
          <div>
            <p class="text-xs text-gray-400">메모</p>
            <p class="text-sm text-gray-700">{{ day.memo }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useConventionStore } from '@/stores/convention'
import apiClient from '@/services/api'

const route = useRoute()
const conventionStore = useConventionStore()

const loading = ref(true)
const days = ref([])

function formatDate(dateStr) {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const weekdays = ['일', '월', '화', '수', '목', '금', '토']
  const month = date.getMonth() + 1
  const day = date.getDate()
  const weekday = weekdays[date.getDay()]
  return `${month}월 ${day}일 (${weekday})`
}

onMounted(async () => {
  try {
    const conventionId =
      route.params.conventionId || conventionStore.currentConvention?.id
    if (!conventionId) return

    const response = await apiClient.get(
      `/conventions/${conventionId}/my-travel-info`,
    )
    days.value = response.data.days || []
  } catch (error) {
    console.error('Failed to load travel info:', error)
  } finally {
    loading.value = false
  }
})
</script>
