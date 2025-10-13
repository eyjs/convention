<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 헤더 -->
    <header class="bg-white shadow-sm sticky top-0 z-40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <h1 class="text-2xl font-bold text-gray-900">행사 관리</h1>
          <div class="flex items-center space-x-4">
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
      </div>
    </header>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 액션 바 -->
      <div class="flex justify-between items-center mb-6">
        <div>
          <p class="text-sm text-gray-600">전체 {{ conventions.length }}개 행사</p>
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
        <button
          @click="goToChatbotManagement"
          class="flex items-center space-x-2 px-4 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-lg transition-colors shadow-sm ml-4"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
          <span>챗봇 관리</span>
        </button>
      </div>

      <!-- 로딩 -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"></div>
        <p class="text-gray-600 mt-4">로딩 중...</p>
      </div>

      <!-- 빈 상태 -->
      <div v-else-if="conventions.length === 0" class="text-center py-12 bg-white rounded-lg border-2 border-dashed">
        <svg class="w-16 h-16 mx-auto text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
        </svg>
        <h3 class="mt-4 text-lg font-medium text-gray-900">행사가 없습니다</h3>
        <p class="mt-2 text-gray-600">첫 번째 행사를 만들어보세요</p>
      </div>

      <!-- 행사 목록 -->
      <div v-else class="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        <div
          v-for="convention in conventions"
          :key="convention.id"
          class="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden cursor-pointer"
          @click="goToConvention(convention.id)"
        >
          <!-- 상단 배너 -->
          <div 
            class="h-32 relative"
            :style="{ background: `linear-gradient(135deg, ${convention.brandColor || '#6366f1'} 0%, ${adjustColor(convention.brandColor || '#6366f1', -20)} 100%)` }"
          >
            <div class="absolute top-3 right-3 flex space-x-2">
              <span v-if="convention.completeYn === 'Y'" class="px-2 py-1 bg-gray-800/50 text-white text-xs rounded-full backdrop-blur-sm">
                종료
              </span>
              <span v-else class="px-2 py-1 bg-green-500/80 text-white text-xs rounded-full backdrop-blur-sm">
                진행중
              </span>
            </div>
            <div class="absolute bottom-4 left-4 right-4">
              <h3 class="text-white font-bold text-lg truncate">{{ convention.title }}</h3>
              <p class="text-white/90 text-sm mt-1">{{ convention.conventionType === 'OVERSEAS' ? '해외' : '국내' }} 행사</p>
            </div>
          </div>

          <!-- 정보 -->
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
              <div class="flex items-center text-gray-600">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
                일정 {{ convention.scheduleCount }}개
              </div>
            </div>

            <!-- 액션 버튼 -->
            <div class="mt-4 pt-4 border-t flex space-x-2">
              <button
                @click.stop="editConvention(convention)"
                class="flex-1 px-3 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
              >
                수정
              </button>
              <button
                @click.stop="toggleComplete(convention)"
                :class="[
                  'flex-1 px-3 py-2 text-sm font-medium rounded-lg transition-colors',
                  convention.completeYn === 'Y' 
                    ? 'text-green-700 bg-green-100 hover:bg-green-200'
                    : 'text-gray-700 bg-gray-100 hover:bg-gray-200'
                ]"
              >
                {{ convention.completeYn === 'Y' ? '재개' : '종료' }}
              </button>
              <button
                @click.stop="deleteConvention(convention)"
                class="px-3 py-2 text-sm font-medium text-red-700 bg-red-100 hover:bg-red-200 rounded-lg transition-colors"
              >
                삭제
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 생성/수정 모달 -->
    <ConventionFormModal
      v-if="showCreateModal || editingConvention"
      :convention="editingConvention"
      @close="closeModal"
      @save="saveConvention"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import ConventionFormModal from '@/components/admin/ConventionFormModal.vue'

const router = useRouter()
const authStore = useAuthStore()

const conventions = ref([])
const loading = ref(false)
const showCreateModal = ref(false)
const editingConvention = ref(null)

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
  }
}

const loadConventions = async () => {
  loading.value = true
  try {
    const response = await axios.get('/api/conventions')
    conventions.value = response.data
  } catch (error) {
    console.error('Failed to load conventions:', error)
    alert('행사 목록을 불러오는데 실패했습니다.')
  } finally {
    loading.value = false
  }
}

const goToConvention = (id) => {
  router.push(`/admin/conventions/${id}`)
}

const goToChatbotManagement = () => {
  router.push('/admin/chatbot')
}

const editConvention = (convention) => {
  editingConvention.value = convention
}

const deleteConvention = async (convention) => {
  if (!confirm(`"${convention.title}" 행사를 삭제하시겠습니까?`)) return

  try {
    await axios.delete(`/api/conventions/${convention.id}`)
    alert('행사가 삭제되었습니다.')
    loadConventions()
  } catch (error) {
    console.error('Failed to delete convention:', error)
    alert('행사 삭제에 실패했습니다.')
  }
}

const toggleComplete = async (convention) => {
  try {
    await axios.post(`/api/conventions/${convention.id}/complete`)
    loadConventions()
  } catch (error) {
    console.error('Failed to toggle complete:', error)
    alert('상태 변경에 실패했습니다.')
  }
}

const closeModal = () => {
  showCreateModal.value = false
  editingConvention.value = null
}

const saveConvention = async (data) => {
  try {
    if (editingConvention.value) {
      await axios.put(`/api/conventions/${editingConvention.value.id}`, data)
      alert('행사가 수정되었습니다.')
    } else {
      await axios.post('/api/conventions', data)
      alert('행사가 생성되었습니다.')
    }
    closeModal()
    loadConventions()
  } catch (error) {
    console.error('Failed to save convention:', error)
    alert('저장에 실패했습니다.')
  }
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('ko-KR', { year: 'numeric', month: '2-digit', day: '2-digit' })
}

const adjustColor = (color, amount) => {
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
