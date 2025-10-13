<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 헤더 -->
    <div class="sticky top-0 z-40 bg-white shadow-sm">
      <div class="px-4 py-4">
        <div class="flex items-center justify-between">
          <div class="flex items-center space-x-3">
            <button @click="$router.back()" class="p-2 hover:bg-gray-100 rounded-lg">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
              </svg>
            </button>
            <h1 class="text-xl font-bold text-gray-900">나의 일정</h1>
          </div>
          <button @click="showCalendarView = !showCalendarView" class="p-2 hover:bg-gray-100 rounded-lg">
            <svg v-if="!showCalendarView" class="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <svg v-else class="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- 날짜 선택 스크롤 -->
    <div class="bg-white border-b sticky top-[72px] z-30">
      <div class="overflow-x-auto scrollbar-hide">
        <div class="flex px-4 py-3 space-x-2 min-w-max">
          <button
            v-for="date in dates"
            :key="date.date"
            @click="selectedDate = date.date"
            :class="[
              'flex-shrink-0 px-4 py-3 rounded-xl text-center transition-all',
              selectedDate === date.date
                ? 'bg-primary-600 text-white shadow-lg scale-105'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            ]"
          >
            <div class="text-xs font-medium mb-1">{{ date.day }}</div>
            <div class="text-xl font-bold">{{ date.dayNum }}</div>
            <div class="text-xs mt-1 opacity-80">{{ date.month }}</div>
          </button>
        </div>
      </div>
    </div>

    <!-- 일정 리스트 -->
    <div v-if="!showCalendarView" class="px-4 py-6 space-y-4">
      <!-- 날짜별 그룹 -->
      <div v-for="dateGroup in groupedSchedules" :key="dateGroup.date" class="space-y-3">
        <div class="flex items-center justify-between px-2">
          <h2 class="text-sm font-bold text-gray-900">
            {{ formatDateHeader(dateGroup.date) }}
          </h2>
          <span class="text-xs text-gray-500">{{ dateGroup.schedules.length }}개 일정</span>
        </div>

        <!-- 일정 카드 -->
        <div
          v-for="schedule in dateGroup.schedules"
          :key="schedule.id"
          @click="openScheduleDetail(schedule)"
          class="bg-white rounded-xl shadow-sm hover:shadow-md transition-all p-4 cursor-pointer"
        >
          <!-- 시간 & 타이틀 -->
          <div class="flex items-start space-x-3">
            <div class="w-16 flex-shrink-0">
              <div class="px-2 py-1 bg-primary-100 text-primary-700 rounded-lg text-center">
                <div class="text-xs font-bold">{{ schedule.startTime }}</div>
                <div v-if="schedule.endTime" class="text-xs opacity-70">{{ schedule.endTime }}</div>
              </div>
            </div>
            
            <div class="flex-1 min-w-0">
              <div class="flex items-start justify-between mb-2">
                <h3 class="font-bold text-gray-900 text-base">{{ schedule.title }}</h3>
                <span v-if="schedule.category" class="px-2 py-1 bg-blue-100 text-blue-700 rounded text-xs font-medium ml-2 flex-shrink-0">
                  {{ schedule.category }}
                </span>
              </div>
              
              <!-- 장소 -->
              <div v-if="schedule.location" class="flex items-center space-x-1 text-sm text-gray-600 mb-2">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <span>{{ schedule.location }}</span>
              </div>

              <!-- 설명 -->
              <p v-if="schedule.description" class="text-sm text-gray-600 line-clamp-2">
                {{ schedule.description }}
              </p>

              <!-- 참가자/그룹 -->
              <div v-if="schedule.group" class="flex items-center space-x-2 mt-3">
                <div class="flex items-center space-x-1 px-2 py-1 bg-gray-100 rounded-full text-xs">
                  <svg class="w-3 h-3 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                  </svg>
                  <span class="text-gray-700 font-medium">{{ schedule.group }}</span>
                </div>
                <span v-if="schedule.participants" class="text-xs text-gray-500">
                  {{ schedule.participants }}명 참여
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- 일정 없음 -->
        <div v-if="dateGroup.schedules.length === 0" class="text-center py-8 text-gray-500">
          <svg class="w-12 h-12 mx-auto mb-2 opacity-50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <p class="text-sm">이 날은 예정된 일정이 없습니다</p>
        </div>
      </div>
    </div>

    <!-- 캘린더 뷰 -->
    <div v-else class="px-4 py-6">
      <div class="bg-white rounded-xl shadow-sm p-4">
        <div class="grid grid-cols-7 gap-2 mb-4">
          <div v-for="day in ['일', '월', '화', '수', '목', '금', '토']" :key="day" class="text-center text-xs font-bold text-gray-500 py-2">
            {{ day }}
          </div>
        </div>
        <div class="grid grid-cols-7 gap-2">
          <div
            v-for="day in calendarDays"
            :key="day.date"
            @click="selectCalendarDay(day)"
            :class="[
              'aspect-square flex flex-col items-center justify-center rounded-lg cursor-pointer transition-all',
              day.isToday ? 'bg-primary-100 border-2 border-primary-600' : '',
              day.hasSchedule ? 'bg-blue-50' : 'hover:bg-gray-50',
              !day.isCurrentMonth ? 'opacity-30' : ''
            ]"
          >
            <span :class="['text-sm font-medium', day.isToday ? 'text-primary-600 font-bold' : 'text-gray-700']">
              {{ day.day }}
            </span>
            <div v-if="day.scheduleCount > 0" class="flex items-center justify-center mt-1">
              <div class="w-1 h-1 rounded-full bg-primary-600"></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 일정 상세 모달 -->
    <div v-if="selectedSchedule" @click="closeScheduleDetail" class="fixed inset-0 bg-black/50 z-50 flex items-end sm:items-center justify-center p-4">
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
          <!-- 카테고리 & 타이틀 -->
          <div>
            <span v-if="selectedSchedule.category" class="px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-sm font-medium">
              {{ selectedSchedule.category }}
            </span>
            <h3 class="text-2xl font-bold text-gray-900 mt-3">{{ selectedSchedule.title }}</h3>
          </div>

          <!-- 시간 정보 -->
          <div class="flex items-start space-x-3 p-4 bg-gray-50 rounded-xl">
            <svg class="w-6 h-6 text-gray-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <div class="font-semibold text-gray-900">{{ formatDate(selectedSchedule.date) }}</div>
              <div class="text-gray-600">{{ selectedSchedule.startTime }} ~ {{ selectedSchedule.endTime || '종료시간 미정' }}</div>
            </div>
          </div>

          <!-- 장소 정보 -->
          <div v-if="selectedSchedule.location" class="flex items-start space-x-3 p-4 bg-gray-50 rounded-xl">
            <svg class="w-6 h-6 text-gray-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            <div>
              <div class="font-semibold text-gray-900">장소</div>
              <div class="text-gray-600">{{ selectedSchedule.location }}</div>
            </div>
          </div>

          <!-- 그룹 정보 -->
          <div v-if="selectedSchedule.group" class="flex items-start space-x-3 p-4 bg-gray-50 rounded-xl">
            <svg class="w-6 h-6 text-gray-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
            <div>
              <div class="font-semibold text-gray-900">참여 그룹</div>
              <div class="text-gray-600">{{ selectedSchedule.group }} ({{ selectedSchedule.participants }}명)</div>
            </div>
          </div>

          <!-- 설명 -->
          <div v-if="selectedSchedule.description" class="space-y-2">
            <h4 class="font-semibold text-gray-900">상세 설명</h4>
            <p class="text-gray-600 whitespace-pre-wrap leading-relaxed">{{ selectedSchedule.description }}</p>
          </div>

          <!-- 액션 버튼 -->
          <div class="flex space-x-3 pt-4">
            <button class="flex-1 px-4 py-3 bg-gray-100 text-gray-700 rounded-xl font-medium hover:bg-gray-200">
              캘린더에 추가
            </button>
            <button class="flex-1 px-4 py-3 bg-primary-600 text-white rounded-xl font-medium hover:bg-primary-700">
              길찾기
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/api'

const showCalendarView = ref(false)
const selectedDate = ref('2025-03-12')
const selectedSchedule = ref(null)

// 날짜 목록 생성
const dates = ref([
  { date: '2025-03-11', day: '화', dayNum: '11', month: '3월' },
  { date: '2025-03-12', day: '수', dayNum: '12', month: '3월' },
  { date: '2025-03-13', day: '목', dayNum: '13', month: '3월' },
  { date: '2025-03-14', day: '금', dayNum: '14', month: '3월' },
  { date: '2025-03-15', day: '토', dayNum: '15', month: '3월' },
])

// 임시 일정 데이터
const schedules = ref([
  {
    id: 1,
    date: '2025-03-12',
    startTime: '09:00',
    endTime: '12:00',
    title: 'STAFF 선발대 미팅',
    location: '인천국제공항 제1터미널',
    category: '미팅',
    group: '1호차',
    participants: 45,
    description: '전체 참가자 오리엔테이션 및 일정 안내'
  },
  {
    id: 2,
    date: '2025-03-12',
    startTime: '14:00',
    endTime: '18:00',
    title: '로마 도착',
    location: '로마 피우미치노 공항',
    category: '이동',
    group: '전체',
    participants: 150,
    description: '인천발 → 로마행 직항편\n\n체크인 시간: 오전 11:00\n탑승 게이트: 미정'
  },
  {
    id: 3,
    date: '2025-03-13',
    startTime: '09:30',
    endTime: '12:00',
    title: '바티칸 박물관 투어',
    location: '바티칸 박물관',
    category: '관광',
    group: 'A조',
    participants: 50,
    description: '세계적인 예술 작품 감상\n가이드 투어 진행'
  },
  {
    id: 4,
    date: '2025-03-13',
    startTime: '14:00',
    endTime: '17:00',
    title: '콜로세움 관람',
    location: '콜로세움',
    category: '관광',
    group: 'B조',
    participants: 50,
    description: '고대 로마 원형 경기장 투어'
  },
])

// 날짜별 일정 그룹화
const groupedSchedules = computed(() => {
  const grouped = {}
  schedules.value.forEach(schedule => {
    if (!grouped[schedule.date]) {
      grouped[schedule.date] = []
    }
    grouped[schedule.date].push(schedule)
  })
  
  return Object.keys(grouped).map(date => ({
    date,
    schedules: grouped[date].sort((a, b) => a.startTime.localeCompare(b.startTime))
  }))
})

// 캘린더 날짜 생성
const calendarDays = computed(() => {
  const days = []
  const today = new Date()
  const currentMonth = today.getMonth()
  const currentYear = today.getFullYear()
  
  const firstDay = new Date(currentYear, currentMonth, 1)
  const lastDay = new Date(currentYear, currentMonth + 1, 0)
  const startDay = firstDay.getDay()
  
  // 이전 달 날짜
  for (let i = startDay - 1; i >= 0; i--) {
    const date = new Date(currentYear, currentMonth, -i)
    days.push({
      date: date.toISOString().split('T')[0],
      day: date.getDate(),
      isCurrentMonth: false,
      isToday: false,
      hasSchedule: false,
      scheduleCount: 0
    })
  }
  
  // 현재 달 날짜
  for (let i = 1; i <= lastDay.getDate(); i++) {
    const date = new Date(currentYear, currentMonth, i)
    const dateStr = date.toISOString().split('T')[0]
    const daySchedules = schedules.value.filter(s => s.date === dateStr)
    
    days.push({
      date: dateStr,
      day: i,
      isCurrentMonth: true,
      isToday: dateStr === today.toISOString().split('T')[0],
      hasSchedule: daySchedules.length > 0,
      scheduleCount: daySchedules.length
    })
  }
  
  return days
})

function formatDateHeader(dateStr) {
  const date = new Date(dateStr)
  const days = ['일', '월', '화', '수', '목', '금', '토']
  return `${date.getMonth() + 1}월 ${date.getDate()}일 (${days[date.getDay()]})`
}

function formatDate(dateStr) {
  const date = new Date(dateStr)
  const days = ['일요일', '월요일', '화요일', '수요일', '목요일', '금요일', '토요일']
  return `${date.getFullYear()}년 ${date.getMonth() + 1}월 ${date.getDate()}일 ${days[date.getDay()]}`
}

function openScheduleDetail(schedule) {
  selectedSchedule.value = schedule
}

function closeScheduleDetail() {
  selectedSchedule.value = null
}

function selectCalendarDay(day) {
  selectedDate.value = day.date
  showCalendarView.value = false
}

// API에서 일정 불러오기
onMounted(async () => {
  try {
    // const response = await apiClient.get('/guest/schedules')
    // schedules.value = response.data
  } catch (error) {
    console.error('Failed to load schedules:', error)
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
