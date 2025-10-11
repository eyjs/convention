<template>
  <div class="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100">
    <!-- 헤더 배너 -->
    <div class="relative h-48 bg-gradient-to-br from-primary-600 to-primary-800 overflow-hidden">
      <!-- 배경 패턴 -->
      <div class="absolute inset-0 opacity-10">
        <div class="absolute top-0 left-0 w-64 h-64 bg-white rounded-full -translate-x-32 -translate-y-32"></div>
        <div class="absolute bottom-0 right-0 w-96 h-96 bg-white rounded-full translate-x-48 translate-y-48"></div>
      </div>
      
      <!-- 로그아웃 버튼 -->
      <div class="absolute top-4 right-4 z-10">
        <button
          @click="handleLogout"
          class="flex items-center space-x-2 px-4 py-2 bg-white/20 backdrop-blur-sm hover:bg-white/30 rounded-full text-white text-sm font-medium transition-all"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
          </svg>
          <span>로그아웃</span>
        </button>
      </div>
      
      <div class="relative h-full flex flex-col justify-center px-6 text-white">
        <h1 class="text-2xl font-bold mb-2">{{ convention.title }}</h1>
        <div class="flex items-center space-x-2 text-sm text-white/90">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <span>{{ convention.startDate }} ~ {{ convention.endDate }}</span>
        </div>
        <div class="flex items-center space-x-2 text-sm text-white/90 mt-1">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          <span>{{ convention.location }}</span>
        </div>
        
        <!-- D-Day 뱃지 -->
        <div class="mt-4 inline-flex items-center px-3 py-1.5 bg-white/20 backdrop-blur-sm rounded-full text-sm font-bold">
          <svg class="w-4 h-4 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          D-{{ dDay }}
        </div>
      </div>
    </div>

    <!-- 메인 컨텐츠 -->
    <div class="px-4 py-6 space-y-6 -mt-8">
      <!-- 빠른 액션 카드 -->
      <div class="bg-white rounded-2xl shadow-lg p-5">
        <h2 class="text-lg font-bold text-gray-900 mb-4">빠른 메뉴</h2>
        <div class="grid grid-cols-4 gap-4">
          <button
            v-for="menu in quickMenus"
            :key="menu.id"
            @click="navigateTo(menu.route)"
            class="flex flex-col items-center justify-center space-y-2 p-3 rounded-xl hover:bg-gray-50 active:scale-95 transition-all"
          >
            <div class="w-12 h-12 bg-gradient-to-br from-primary-400 to-primary-600 rounded-full flex items-center justify-center shadow-md">
              <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="menu.iconPath" />
              </svg>
            </div>
            <span class="text-xs font-medium text-gray-700 text-center leading-tight">{{ menu.label }}</span>
          </button>
        </div>
      </div>

      <!-- 오늘의 일정 -->
      <div class="bg-white rounded-2xl shadow-lg p-5" v-if="todaySchedules.length > 0">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-bold text-gray-900">오늘의 일정</h2>
          <button 
            @click="navigateTo('/my-schedule')"
            class="text-sm text-primary-600 font-medium flex items-center"
          >
            전체보기
            <svg class="w-4 h-4 ml-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </div>
        
        <div class="space-y-3">
          <div
            v-for="schedule in todaySchedules"
            :key="schedule.id"
            class="flex items-start space-x-3 p-3 bg-gray-50 rounded-xl"
          >
            <div class="w-12 h-12 bg-primary-100 rounded-full flex items-center justify-center flex-shrink-0">
              <svg class="w-6 h-6 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center space-x-2 mb-1">
                <span class="px-2 py-0.5 bg-primary-600 text-white text-xs font-bold rounded">{{ schedule.time }}</span>
                <span class="text-xs text-gray-500">{{ schedule.location }}</span>
              </div>
              <h3 class="font-semibold text-gray-900 text-sm">{{ schedule.title }}</h3>
            </div>
          </div>
        </div>
      </div>

      <!-- 최근 공지 -->
      <div class="bg-white rounded-2xl shadow-lg p-5" v-if="recentNotices.length > 0">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-bold text-gray-900">공지사항</h2>
          <button 
            @click="navigateTo('/board')"
            class="text-sm text-primary-600 font-medium flex items-center"
          >
            더보기
            <svg class="w-4 h-4 ml-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </div>
        
        <div class="space-y-2">
          <button
            v-for="notice in recentNotices"
            :key="notice.id"
            @click="openNotice(notice)"
            class="w-full text-left p-3 hover:bg-gray-50 rounded-xl transition-all"
          >
            <div class="flex items-start space-x-2">
              <span class="px-2 py-0.5 bg-red-100 text-red-700 text-xs font-bold rounded flex-shrink-0 mt-0.5">공지</span>
              <div class="flex-1 min-w-0">
                <h3 class="font-medium text-gray-900 text-sm truncate">{{ notice.title }}</h3>
                <p class="text-xs text-gray-500 mt-1">{{ notice.date }}</p>
              </div>
            </div>
          </button>
        </div>
      </div>

      <!-- 참가자 통계 -->
      <div class="grid grid-cols-3 gap-3">
        <div class="bg-white rounded-xl shadow-md p-4 text-center">
          <div class="w-10 h-10 bg-primary-100 rounded-full flex items-center justify-center mx-auto mb-2">
            <svg class="w-5 h-5 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
          </div>
          <div class="text-2xl font-bold text-gray-900">{{ stats.total }}</div>
          <div class="text-xs text-gray-600 mt-1">전체 참가자</div>
        </div>
        
        <div class="bg-white rounded-xl shadow-md p-4 text-center">
          <div class="w-10 h-10 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-2">
            <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div class="text-2xl font-bold text-gray-900">{{ stats.confirmed }}</div>
          <div class="text-xs text-gray-600 mt-1">참석 확정</div>
        </div>
        
        <div class="bg-white rounded-xl shadow-md p-4 text-center">
          <div class="w-10 h-10 bg-orange-100 rounded-full flex items-center justify-center mx-auto mb-2">
            <svg class="w-5 h-5 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div class="text-2xl font-bold text-gray-900">{{ stats.pending }}</div>
          <div class="text-xs text-gray-600 mt-1">대기중</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
  }
}

const convention = ref({
  title: 'iFA STAR TOUR @ ROMA',
  startDate: '2025-03-12',
  endDate: '2025-03-05',
  location: 'Roma, Italy'
})

const quickMenus = ref([
  { 
    id: 1, 
    label: '나의일정', 
    route: '/my-schedule',
    iconPath: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z'
  },
  { 
    id: 2, 
    label: '장소정보', 
    route: '/location',
    iconPath: 'M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z'
  },
  { 
    id: 3, 
    label: '맛집', 
    route: '/tastespot',
    iconPath: 'M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253'
  },
  { 
    id: 4, 
    label: '사진첩', 
    route: '/event-place',
    iconPath: 'M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z'
  },
])

const todaySchedules = ref([
  {
    id: 1,
    time: '09:00',
    title: 'STAFF 선발대 미팅',
    location: '인천공항'
  },
  {
    id: 2,
    time: '14:00',
    title: '로마 도착',
    location: '로마공항'
  }
])

const recentNotices = ref([
  {
    id: 1,
    title: '일정 변경 안내',
    date: '2025-05-10'
  },
  {
    id: 2,
    title: '호텔 체크인 정보',
    date: '2025-05-09'
  }
])

const stats = ref({
  total: 156,
  confirmed: 142,
  pending: 14
})

const dDay = computed(() => {
  const today = new Date()
  const start = new Date(convention.value.startDate)
  const diff = Math.ceil((start - today) / (1000 * 60 * 60 * 24))
  return diff > 0 ? diff : 0
})

function navigateTo(route) {
  router.push(route)
}

function openNotice(notice) {
  router.push(`/board/${notice.id}`)
}
</script>
