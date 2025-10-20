<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 헤더 -->
    <header class="bg-white shadow-sm sticky top-0 z-40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <h1 class="text-2xl font-bold text-gray-900">관리자 대시보드</h1>
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
        
        <!-- 탭 메뉴 -->
        <div class="border-t">
          <nav class="-mb-px flex space-x-8">
            <button
              v-for="tab in tabs"
              :key="tab.id"
              @click="activeTab = tab.id"
              :class="[
                'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
                activeTab === tab.id
                  ? 'border-primary-600 text-primary-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              ]"
            >
              <div class="flex items-center space-x-2">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="tab.icon" />
                </svg>
                <span>{{ tab.label }}</span>
              </div>
            </button>
          </nav>
        </div>
      </div>
    </header>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 행사 관리 탭 -->
      <div v-if="activeTab === 'conventions'">
        <div class="flex justify-between items-center mb-6">
          <div>
            <h2 class="text-xl font-bold text-gray-900">행사 관리</h2>
            <p class="text-sm text-gray-600 mt-1">전체 {{ conventions.length }}개 행사</p>
          </div>
          <button
            @click="showCreateModal = true"
            class="flex items-center space-x-2 px-4 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg transition-colors shadow-sm"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            <span>새 행사 만들기</span>
          </button>
        </div>

        <!-- 로딩 -->
        <div v-if="loading" class="text-center py-12">
          <div class="inline-block w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"></div>
          <p class="text-gray-600 mt-4">로딩 중...</p>
        </div>

        <!-- 행사 목록 -->
        <div v-else class="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          <div
            v-for="convention in conventions"
            :key="convention.id"
            class="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden cursor-pointer"
            @click="goToConvention(convention.id)"
          >
            <div 
              class="h-32 relative"
              :style="{ background: `linear-gradient(135deg, ${convention.brandColor || '#6366f1'} 0%, ${adjustColor(convention.brandColor || '#6366f1', -20)} 100%)` }"
            >
              <div class="absolute top-3 right-3">
                <span v-if="convention.completeYn === 'Y'" class="px-2 py-1 bg-gray-800/50 text-white text-xs rounded-full">종료</span>
                <span v-else class="px-2 py-1 bg-green-500/80 text-white text-xs rounded-full">진행중</span>
              </div>
              <div class="absolute bottom-4 left-4 right-4">
                <h3 class="text-white font-bold text-lg truncate">{{ convention.title }}</h3>
              </div>
            </div>
            <div class="p-4">
              <div class="space-y-2 text-sm text-gray-600">
                <div class="flex items-center">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  {{ formatDate(convention.startDate) }} ~ {{ formatDate(convention.endDate) }}
                </div>
                <div class="flex items-center">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                  </svg>
                  참석자 {{ convention.guestCount }}명
                </div>
              </div>
              <div class="mt-4 pt-4 border-t flex justify-end space-x-2">
                <button @click.stop="editConvention(convention)" class="px-3 py-1 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-md">수정</button>
                <button @click.stop="completeConvention(convention.id)" 
                        :class="convention.completeYn === 'Y' ? 'bg-green-500 hover:bg-green-600' : 'bg-red-500 hover:bg-red-600'" 
                        class="px-3 py-1 text-sm font-medium text-white rounded-md">
                  {{ convention.completeYn === 'Y' ? '재개' : '종료' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 챗봇 관리 탭 -->
      <div v-if="activeTab === 'chatbot'">
        <ChatbotManagement />
      </div>

      <!-- 회원 관리 탭 -->
      <div v-if="activeTab === 'users'">
        <div class="mb-6">
          <h2 class="text-xl font-bold text-gray-900">회원 관리</h2>
          <p class="text-sm text-gray-600 mt-1">전체 회원 조회 및 관리</p>
        </div>
        <div class="bg-white rounded-lg shadow p-6">
          <p class="text-gray-600">회원 관리 기능 구현 예정</p>
        </div>
      </div>

      <!-- 통계 탭 -->
      <div v-if="activeTab === 'statistics'">
        <div class="mb-6">
          <h2 class="text-xl font-bold text-gray-900">통계</h2>
          <p class="text-sm text-gray-600 mt-1">시스템 전체 통계 및 분석</p>
        </div>
        <div class="grid gap-6 md:grid-cols-2 lg:grid-cols-4 mb-6">
          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">전체 행사</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ conventions.length }}</p>
              </div>
              <div class="p-3 bg-blue-100 rounded-full">
                <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
              </div>
            </div>
          </div>
          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">진행중 행사</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ activeConventions }}</p>
              </div>
              <div class="p-3 bg-green-100 rounded-full">
                <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
          </div>
          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">전체 참석자</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ totalGuests }}</p>
              </div>
              <div class="p-3 bg-purple-100 rounded-full">
                <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
              </div>
            </div>
          </div>
          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">전체 일정</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ totalSchedules }}</p>
              </div>
              <div class="p-3 bg-yellow-100 rounded-full">
                <svg class="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
            </div>
          </div>
        </div>
        <div class="bg-white rounded-lg shadow p-6">
          <p class="text-gray-600">상세 통계 기능 구현 예정</p>
        </div>
      </div>
    </div>
    <ConventionFormModal
      v-if="showCreateModal"
      :convention="editingConvention"
      @close="showCreateModal = false"
      @save="handleSaveConvention"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/api'
import ChatbotManagement from '@/components/admin/ChatbotManagement.vue'
import ConventionFormModal from '@/components/admin/ConventionFormModal.vue'

const router = useRouter()
const authStore = useAuthStore()

const activeTab = ref('conventions')
const conventions = ref([])
const loading = ref(false)
const showCreateModal = ref(false)
const editingConvention = ref(null)

async function handleSaveConvention(conventionData) {
  try {
    if (editingConvention.value) {
      // 수정
      await apiClient.put(`/conventions/${editingConvention.value.id}`, conventionData)
    } else {
      // 생성
      await apiClient.post('/conventions', conventionData)
    }
    showCreateModal.value = false
    editingConvention.value = null
    await loadConventions()
  } catch (error) {
    console.error('Failed to save convention:', error)
    alert('행사 저장에 실패했습니다.')
  }
}

function editConvention(convention) {
  editingConvention.value = { ...convention };
  showCreateModal.value = true;
}

async function completeConvention(conventionId) {
  if (!confirm('행사를 종료 처리하시겠습니까?')) return;

  try {
    await apiClient.post(`/conventions/${conventionId}/complete`);
    await loadConventions();
  } catch (error) {
    console.error('Failed to complete convention:', error);
    alert('행사 종료 처리에 실패했습니다.');
  }
}

const tabs = [
  {
    id: 'conventions',
    label: '행사 관리',
    icon: 'M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10'
  },
  {
    id: 'chatbot',
    label: '챗봇 관리',
    icon: 'M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z'
  },
  {
    id: 'users',
    label: '회원 관리',
    icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z'
  },
  {
    id: 'statistics',
    label: '통계',
    icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z'
  }
]

const activeConventions = computed(() => {
  return conventions.value.filter(c => c.completeYn === 'N').length
})

const totalGuests = computed(() => {
  return conventions.value.reduce((sum, c) => sum + (c.guestCount || 0), 0)
})

const totalSchedules = computed(() => {
  return conventions.value.reduce((sum, c) => sum + (c.scheduleCount || 0), 0)
})

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
  }
}

async function loadConventions() {
  loading.value = true
  try {
    const response = await apiClient.get('/conventions')
    conventions.value = response.data
  } catch (error) {
    console.error('Failed to load conventions:', error)
  } finally {
    loading.value = false
  }
}

function goToConvention(conventionId) {
  router.push(`/admin/conventions/${conventionId}`)
}

function formatDate(dateString) {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('ko-KR', { year: 'numeric', month: '2-digit', day: '2-digit' })
}

function adjustColor(color, amount) {
  if (!color) return '#555'
  const num = parseInt(color.replace('#', ''), 16)
  const r = Math.max(0, Math.min(255, (num >> 16) + amount))
  const g = Math.max(0, Math.min(255, ((num >> 8) & 0x00FF) + amount))
  const b = Math.max(0, Math.min(255, (num & 0x0000FF) + amount))
  return '#' + ((r << 16) | (g << 8) | b).toString(16).padStart(6, '0')
}

onMounted(() => {
  loadConventions()
})
</script>
