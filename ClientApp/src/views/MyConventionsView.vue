<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <header class="bg-white shadow-sm sticky top-0 z-40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <h1 class="text-2xl font-bold text-gray-900">행사 선택</h1>
          <button
            @click="handleLogout"
            class="flex items-center space-x-2 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
            </svg>
            <span>로그아웃</span>
          </button>
        </div>
      </div>
    </header>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"></div>
        <p class="text-gray-600 mt-4">행사 목록을 불러오는 중...</p>
      </div>

      <div v-else-if="conventions.length === 0" class="text-center py-12 bg-white rounded-lg border-2 border-dashed">
        <svg class="w-16 h-16 mx-auto text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
        </svg>
        <h3 class="mt-4 text-lg font-medium text-gray-900">참여 중인 행사가 없습니다</h3>
        <p class="mt-2 text-gray-600">관리자에게 문의하세요.</p>
      </div>

      <div v-else class="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        <div
          v-for="convention in conventions"
          :key="convention.id"
          class="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden cursor-pointer debug-border"
          @click="selectConvention(convention)"
        >
          <div 
            class="h-32 relative"
            :style="{ background: `linear-gradient(135deg, ${convention.brandColor || '#6366f1'} 0%, ${adjustColor(convention.brandColor || '#6366f1', -20)} 100%)` }"
          >
            <div class="absolute top-3 right-3">
              <span v-if="convention.completeYn === 'N'" class="px-2 py-1 bg-green-500/80 text-white text-xs rounded-full backdrop-blur-sm">
                진행중
              </span>
              <span v-else class="px-2 py-1 bg-gray-800/50 text-white text-xs rounded-full backdrop-blur-sm">
                종료
              </span>
            </div>
            <div class="absolute bottom-4 left-4 right-4">
              <h3 class="text-white font-bold text-lg truncate">{{ convention.title }}</h3>
            </div>
          </div>
          <div class="p-4">
            <div class="space-y-2 text-sm">
              <div class="flex items-center text-gray-600">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                {{ formatDate(convention.startDate) }} ~ {{ formatDate(convention.endDate) }}
              </div>
              <div class="flex items-center text-gray-600">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
                참석자 {{ convention.guestCount }}명
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import apiClient from '@/services/api'

const router = useRouter()
const authStore = useAuthStore()
const conventionStore = useConventionStore()

const conventions = ref([])
const loading = ref(false)

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
  }
}

const loadUserConventions = async () => {
  loading.value = true
  try {
    const response = await apiClient.get('/conventions/my-conventions')
    conventions.value = response.data
  } catch (error) {
    console.error('Failed to load user conventions:', error)
    alert('행사 목록을 불러오는데 실패했습니다.')
  } finally {
    loading.value = false
  }
}

const selectConvention = async (convention) => {
  await conventionStore.selectConvention(convention.id)
  router.push('/')
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('ko-KR', { year: 'numeric', month: '2-digit', day: '2-digit' })
}

const adjustColor = (color, amount) => {
  if (!color) return '#555';
  const num = parseInt(color.replace('#', ''), 16)
  const r = Math.max(0, Math.min(255, (num >> 16) + amount))
  const g = Math.max(0, Math.min(255, ((num >> 8) & 0x00FF) + amount))
  const b = Math.max(0, Math.min(255, (num & 0x0000FF) + amount))
  return '#' + ((r << 16) | (g << 8) | b).toString(16).padStart(6, '0')
}

onMounted(() => {
  loadUserConventions()
})
</script>

<style>
.debug-border {
  border: 2px solid red !important;
}
</style>
