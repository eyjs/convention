<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <!-- 헤더 -->
    <div class="bg-white border-b sticky top-0 z-40">
      <div class="px-4 py-4">
        <h1 class="text-2xl font-bold text-gray-900 mb-2">나의 일정</h1>
        <p class="text-sm text-gray-600">행사 기간 동안의 개인 일정을 확인하세요</p>
      </div>

      <!-- 검색바 -->
      <div class="px-4 pb-4">
        <div class="relative">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="일정 검색..."
            class="w-full pl-10 pr-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <svg class="absolute left-3 top-2.5 w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </div>
      </div>
    </div>

    <!-- 로딩 -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <div class="text-center">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600">일정을 불러오는 중...</p>
      </div>
    </div>

    <!-- 일정 목록 -->
    <div v-else class="px-4 py-6">
      <!-- 날짜 스크롤 -->
      <div class="mb-6 overflow-x-auto scrollbar-hide">
        <div class="flex gap-2 min-w-max pb-2">
          <button
            v-for="date in availableDates"
            :key="date.dateStr"
            @click="selectedDate = date.dateStr"
            :class="[
              'flex-shrink-0 px-4 py-3 rounded-xl text-center transition-all',
              selectedDate === date.dateStr
                ? 'bg-blue-600 text-white shadow-lg'
                : 'bg-white text-gray-700 hover:bg-gray-100 border'
            ]"
          >
            <div class="text-xs font-medium mb-1">{{ date.dayName }}</div>
            <div class="text-2xl font-bold">{{ date.day }}</div>
            <div class="text-xs mt-1 opacity-80">{{ date.month }}월</div>
          </button>
        </div>
      </div>

      <!-- 선택된 날짜의 일정들 -->
      <div v-if="filteredSchedules.length > 0" class="space-y-3">
        <div
          v-for="schedule in filteredSchedules"
          :key="schedule.id"
          @click="openScheduleDetail(schedule)"
          class="bg-white rounded-xl shadow-sm hover:shadow-md transition-all p-4 cursor-pointer"
        >
          <div class="flex items-start gap-4">
            <!-- 시간 -->
            <div class="flex-shrink-0 w-16">
              <div class="px-3 py-2 bg-blue-100 text-blue-700 rounded-lg text-center">
                <div class="text-sm font-bold">{{ schedule.startTime }}</div>
              </div>
            </div>

            <!-- 내용 -->
            <div class="flex-1 min-w-0">
              <h3 class="font-bold text-gray-900 text-lg mb-1">{{ schedule.title }}</h3>
              
              <div v-if="schedule.location" class="flex items-center gap-1 text-sm text-gray-600 mb-2">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <span>{{ schedule.location }}</span>
              </div>

              <p v-if="schedule.content" class="text-sm text-gray-600 line-clamp-2">
                {{ schedule.content }}
              </p>
            </div>

            <!-- 화살표 -->
            <div class="flex-shrink-0">
              <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </div>
          </div>
        </div>
      </div>

      <!-- 일정 없음 -->
      <div v-else class="text-center py-16">
        <svg class="w-20 h-20 mx-auto mb-4 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
        <p class="text-lg font-medium text-gray-900 mb-2">일정이 없습니다</p>
        <p class="text-sm text-gray-500">{{ formatDateHeader(selectedDate) }}에는 예정된 일정이 없습니다</p>
      </div>

      <!-- 전체 일정 요약 -->
      <div v-if="schedules.length > 0" class="mt-8 bg-white rounded-xl shadow-sm p-6">
        <h3 class="font-bold text-gray-900 mb-4 flex items-center gap-2">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
          </svg>
          일정 요약
        </h3>
        <div class="grid grid-cols-2 gap-4">
          <div class="text-center p-4 bg-gray-50 rounded-lg">
            <div class="text-3xl font-bold text-blue-600">{{ totalDays }}</div>
            <div class="text-sm text-gray-600 mt-1">총 일정 일수</div>
          </div>
          <div class="text-center p-4 bg-gray-50 rounded-lg">
            <div class="text-3xl font-bold text-green-600">{{ schedules.length }}</div>
            <div class="text-sm text-gray-600 mt-1">총 일정 개수</div>
          </div>
        </div>
      </div>
    </div>

    <!-- 일정 상세 모달 -->
    <div v-if="selectedSchedule" @click="isTouchDevice && closeScheduleDetail()" class="fixed inset-0 bg-black/50 z-50 flex items-end sm:items-center justify-center p-4">
      <div @click.stop class="bg-white rounded-t-3xl sm:rounded-2xl w-full sm:max-w-lg max-h-[90vh] overflow-y-auto">
        <!-- 모달 헤더 -->
        <div class="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between">
          <h2 class="text-lg font-bold text-gray-900">일정 상세</h2>
          <button @click="closeScheduleDetail" class="p-2 hover:bg-gray-100 rounded-lg">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- 모달 내용 -->
        <div class="p-6 space-y-6">
          <!-- 타이틀 -->
          <div>
            <h3 class="text-2xl font-bold text-gray-900">{{ selectedSchedule.title }}</h3>
          </div>

          <!-- 날짜/시간 -->
          <div class="flex items-start gap-3 p-4 bg-gray-50 rounded-xl">
            <svg class="w-6 h-6 text-gray-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <div>
              <div class="font-semibold text-gray-900">{{ formatDateFull(selectedSchedule.scheduleDate) }}</div>
              <div class="text-gray-600">{{ selectedSchedule.startTime }}</div>
            </div>
          </div>

          <!-- 장소 -->
          <div v-if="selectedSchedule.location" class="flex items-start gap-3 p-4 bg-gray-50 rounded-xl">
            <svg class="w-6 h-6 text-gray-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            <div>
              <div class="font-semibold text-gray-900">장소</div>
              <div class="text-gray-600">{{ selectedSchedule.location }}</div>
            </div>
          </div>

          <!-- 상세 설명 -->
          <div v-if="selectedSchedule.content" class="space-y-2">
            <h4 class="font-semibold text-gray-900">상세 설명</h4>
            <p class="text-gray-600 whitespace-pre-wrap leading-relaxed">{{ selectedSchedule.content }}</p>
          </div>

          <!-- 액션 버튼 -->
          <div class="pt-4">
            <button 
              @click="closeScheduleDetail"
              class="w-full px-4 py-3 bg-blue-600 text-white rounded-xl font-medium hover:bg-blue-700"
            >
              확인
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted, nextTick } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import apiClient from '@/services/api'
import { useDevice } from '@/composables/useDevice'

export default {
  name: 'Schedule',
  setup() {
    const { isTouchDevice } = useDevice()
    const authStore = useAuthStore()
    const conventionStore = useConventionStore()
    const loading = ref(false)
    const schedules = ref([])
    const selectedDate = ref(null)
    const selectedSchedule = ref(null)
    const searchQuery = ref('')

    // 사용 가능한 날짜 목록
    const availableDates = computed(() => {
      if (!schedules.value) return [];
      const dates = new Set(schedules.value.map(s => s.scheduleDate.split('T')[0]))
      return Array.from(dates).sort().map(dateStr => {
        const date = new Date(dateStr)
        const days = ['일', '월', '화', '수', '목', '금', '토']
        return {
          dateStr,
          day: date.getDate(),
          month: date.getMonth() + 1,
          dayName: days[date.getDay()]
        }
      })
    })

    // 필터링된 일정
    const filteredSchedules = computed(() => {
      if (!schedules.value) return [];
      let filtered = schedules.value

      // 날짜 필터
      if (selectedDate.value) {
        filtered = filtered.filter(s => s.scheduleDate.split('T')[0] === selectedDate.value)
      }

      // 검색 필터
      if (searchQuery.value) {
        const query = searchQuery.value.toLowerCase()
        filtered = filtered.filter(s => 
          s.title.toLowerCase().includes(query) ||
          s.content?.toLowerCase().includes(query) ||
          s.location?.toLowerCase().includes(query)
        )
      }

      return filtered.sort((a, b) => a.startTime.localeCompare(b.startTime))
    })

    // 총 일정 일수
    const totalDays = computed(() => {
      return new Set(schedules.value.map(s => s.scheduleDate)).size
    })

    // 날짜 포맷
    const formatDateHeader = (dateStr) => {
      const date = new Date(dateStr)
      const days = ['일', '월', '화', '수', '목', '금', '토']
      return `${date.getMonth() + 1}월 ${date.getDate()}일 (${days[date.getDay()]})`
    }

    const formatDateFull = (dateStr) => {
      const date = new Date(dateStr)
      const days = ['일요일', '월요일', '화요일', '수요일', '목요일', '금요일', '토요일']
      return `${date.getFullYear()}년 ${date.getMonth() + 1}월 ${date.getDate()}일 ${days[date.getDay()]}`
    }

    // 일정 상세 열기
    const openScheduleDetail = (schedule) => {
      selectedSchedule.value = schedule
    }

    const closeScheduleDetail = () => {
      selectedSchedule.value = null
    }



    // API에서 일정 불러오기
    const fetchSchedules = async () => {
      loading.value = true
      try {
        const userId = authStore.user?.id;
        const conventionId = conventionStore.currentConvention?.id;

        if (!userId || !conventionId) {
          console.error("User or Convention not found, cannot fetch schedules.");
          schedules.value = []; // Clear schedules if IDs are missing
          return;
        }

        const response = await apiClient.get(`/user-schedules/${userId}/${conventionId}`);
        schedules.value = response.data;
        
        // 첫 번째 날짜를 기본 선택
        await nextTick();
        if (availableDates.value.length > 0) {
          selectedDate.value = availableDates.value[0].dateStr
        }
      } catch (error) {
        console.error('Failed to load schedules:', error);
        schedules.value = []; // Clear schedules on error
      } finally {
        loading.value = false
      }
    }

    onMounted(async () => {
      // Ensure stores are ready before fetching
      if (!authStore.user) {
        await authStore.fetchCurrentUser();
      }
      if (!conventionStore.currentConvention) {
        const selectedConventionId = localStorage.getItem('selectedConventionId');
        if (selectedConventionId) {
          await conventionStore.setCurrentConvention(parseInt(selectedConventionId));
        }
      }
      fetchSchedules();
    })

    return {
      loading,
      schedules,
      selectedDate,
      selectedSchedule,
      searchQuery,
      availableDates,
      filteredSchedules,
      totalDays,
      formatDateHeader,
      formatDateFull,
      openScheduleDetail,
      closeScheduleDetail,
      isTouchDevice
    }
  }
}
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
