<template>
  <div>
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

    <template v-else>
      <!-- 날짜 선택 스크롤 (MySchedule 스타일) -->
      <div class="bg-white border-b relative mb-4">
        <div class="overflow-x-auto scrollbar-hide">
          <div class="flex px-1 py-2 space-x-2 min-w-max">
            <!-- 전체 탭 -->
            <button
              :class="[
                'flex-shrink-0 px-3 py-2 rounded-xl text-center transition-all',
                selectedDate === null
                  ? 'bg-primary-600 text-white shadow-lg scale-105'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200',
              ]"
              @click="selectedDate = null"
            >
              <div class="text-sm font-bold leading-[2.125rem]">전체</div>
            </button>
            <button
              v-for="d in dateTabs"
              :key="d.date"
              :class="[
                'flex-shrink-0 px-3 py-2 rounded-xl text-center transition-all',
                selectedDate === d.date
                  ? 'bg-primary-600 text-white shadow-lg scale-105'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200',
              ]"
              @click="selectedDate = d.date"
            >
              <div class="text-xs font-medium mb-0.5">{{ d.day }}</div>
              <div class="text-sm font-bold">{{ d.month }}/{{ d.dayNum }}</div>
            </button>
          </div>
        </div>
      </div>

      <!-- 일자별 카드 -->
      <div class="space-y-3 px-1">
        <div
          v-for="day in filteredDays"
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
              <div class="min-w-0">
                <p class="text-xs text-gray-400">호차</p>
                <p class="font-semibold text-gray-900 truncate">
                  {{ day.bus }}
                </p>
              </div>
            </div>

            <!-- 호텔 -->
            <div v-if="day.hotel" class="flex items-center gap-3">
              <span
                class="w-8 h-8 bg-amber-50 rounded-lg flex items-center justify-center text-base flex-shrink-0"
                >🏨</span
              >
              <div class="min-w-0">
                <p class="text-xs text-gray-400">호텔</p>
                <p class="font-semibold text-gray-900 truncate">
                  {{ day.hotel }}
                </p>
              </div>
            </div>

            <!-- 방번호 -->
            <div class="flex items-center gap-3">
              <span
                class="w-8 h-8 bg-green-50 rounded-lg flex items-center justify-center text-base flex-shrink-0"
                >🔑</span
              >
              <div class="min-w-0">
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
              <div class="min-w-0">
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
              <div class="min-w-0 flex-1">
                <p class="text-xs text-gray-400">메모</p>
                <p class="text-sm text-gray-700 break-words whitespace-pre-line">
                  {{ day.memo }}
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useConventionStore } from '@/stores/convention'
import apiClient from '@/services/api'

const route = useRoute()
const conventionStore = useConventionStore()

const loading = ref(true)
const days = ref([])
const selectedDate = ref(null)

// 날짜 탭 데이터 (MySchedule 스타일)
const dateTabs = computed(() => {
  const weekdays = ['일', '월', '화', '수', '목', '금', '토']
  return days.value.map((d) => {
    const date = new Date(d.date)
    return {
      date: d.date,
      day: weekdays[date.getDay()],
      dayNum: String(date.getDate()),
      month: `${date.getMonth() + 1}`,
    }
  })
})

const filteredDays = computed(() => {
  if (!selectedDate.value) return days.value
  return days.value.filter((d) => d.date === selectedDate.value)
})

function formatDate(dateStr) {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const weekdays = ['일', '월', '화', '수', '목', '금', '토']
  return `${date.getMonth() + 1}월 ${date.getDate()}일 (${weekdays[date.getDay()]})`
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

    // 현재 날짜가 있으면 자동 선택, 없으면 전체
    if (days.value.length > 0) {
      const today = new Date().toISOString().split('T')[0]
      const hasToday = days.value.some((d) => d.date === today)
      selectedDate.value = hasToday ? today : null
    }
  } catch (error) {
    console.error('Failed to load travel info:', error)
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.scrollbar-hide::-webkit-scrollbar {
  display: none;
}
.scrollbar-hide {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
</style>
