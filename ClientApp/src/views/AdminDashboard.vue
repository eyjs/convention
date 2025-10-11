<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 헤더 -->
    <header class="bg-white shadow-sm sticky top-0 z-40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <div class="flex items-center">
            <h1 class="text-xl sm:text-2xl font-bold text-gray-900">행사 관리</h1>
            <span v-if="convention" class="ml-4 text-sm text-gray-500">{{ convention.title }}</span>
          </div>
          <div class="flex items-center space-x-4">
            <button
              @click="handleLogout"
              class="flex items-center space-x-2 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
              </svg>
              <span class="hidden sm:inline">로그아웃</span>
            </button>
            <button
              @click="showMenu = !showMenu"
              class="sm:hidden p-2 rounded-md text-gray-600 hover:bg-gray-100"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </header>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
      <!-- 탭 네비게이션 -->
      <div class="mb-6 overflow-x-auto">
        <nav class="flex space-x-1 sm:space-x-4 border-b min-w-max">
          <button
            v-for="tab in tabs"
            :key="tab.id"
            @click="activeTab = tab.id"
            :class="[
              'pb-3 px-3 sm:px-4 border-b-2 font-medium text-sm whitespace-nowrap transition-colors',
              activeTab === tab.id
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            ]"
          >
            <span class="hidden sm:inline">{{ tab.name }}</span>
            <span class="sm:hidden">{{ tab.shortName || tab.name }}</span>
          </button>
        </nav>
      </div>

      <!-- 대시보드 -->
      <div v-if="activeTab === 'dashboard'">
        <DashboardOverview :convention-id="conventionId" @show-guest="showGuestFromDashboard" />
      </div>

      <!-- 참석자 관리 -->
      <div v-if="activeTab === 'guests'">
        <GuestManagement :convention-id="conventionId" />
      </div>

      <!-- 일정 관리 -->
      <div v-if="activeTab === 'schedules'">
        <ScheduleManagement :convention-id="conventionId" />
      </div>

      <!-- 업로드 -->
      <div v-if="activeTab === 'upload'">
        <BulkUpload :convention-id="conventionId" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import DashboardOverview from '@/components/admin/DashboardOverview.vue'
import GuestManagement from '@/components/admin/GuestManagement.vue'
import ScheduleManagement from '@/components/admin/ScheduleManagement.vue'
import BulkUpload from '@/components/admin/BulkUpload.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const conventionId = ref(parseInt(route.params.id))
const convention = ref(null)
const activeTab = ref('dashboard')
const showMenu = ref(false)

const showGuestFromDashboard = (guestId) => {
  activeTab.value = 'guests'
  // GuestManagement 컴포넌트에서 상세 표시하도록 이벤트 전달 필요
}

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
  }
}

const tabs = [
  { id: 'dashboard', name: '대시보드', shortName: '대시보드' },
  { id: 'guests', name: '참석자 관리', shortName: '참석자' },
  { id: 'schedules', name: '일정 관리', shortName: '일정' },
  { id: 'upload', name: '엑셀 업로드', shortName: '업로드' }
]

onMounted(async () => {
  // 행사 정보 로드
  try {
    const response = await axios.get(`/api/conventions/${conventionId.value}`)
    convention.value = response.data
  } catch (error) {
    console.error('Failed to load convention:', error)
    alert('행사 정보를 불러오는데 실패했습니다.')
    router.push('/admin')
  }
})
</script>
