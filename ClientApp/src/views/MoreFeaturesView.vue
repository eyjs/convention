<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <MainHeader title="추가 메뉴" :show-back="true" />

    <!-- 로딩 -->
    <div v-if="isLoading" class="flex items-center justify-center py-12">
      <div class="text-center">
        <div
          class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"
        ></div>
        <p class="mt-4 text-gray-600">액션 목록을 불러오는 중...</p>
      </div>
    </div>

    <!-- 동적 액션 렌더러 -->
    <div v-else-if="allActions.length > 0" class="px-4 py-6">
      <DynamicActionRenderer :features="allActions" class="grid grid-cols-3 gap-4" />
    </div>

    <!-- 빈 상태 -->
    <div v-else class="px-4 py-12">
      <div
        class="flex flex-col items-center justify-center text-center bg-white rounded-2xl shadow-sm p-8"
      >
        <svg
          class="w-16 h-16 text-gray-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
          />
        </svg>
        <p class="text-lg text-gray-600 mb-4">
          현재 사용 가능한 액션이 없습니다.
        </p>
        <button
          @click="router.push('/')"
          class="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          홈으로 돌아가기
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/services/api'
import MainHeader from '@/components/common/MainHeader.vue'
import DynamicActionRenderer from '@/dynamic-features/DynamicActionRenderer.vue'

const router = useRouter()
const allActions = ref([])
const isLoading = ref(false)

onMounted(async () => {
  const conventionId = localStorage.getItem('selectedConventionId')
  console.log('ConventionId:', conventionId)

  if (!conventionId) {
    console.log('No conventionId found, redirecting to home')
    router.push('/')
    return
  }

  isLoading.value = true
  try {
    const url = `/conventions/${conventionId}/actions/all`
    const response = await apiClient.get(url, {
      params: {
        targetLocation: 'MORE_FEATURES_GRID',
        isActive: true,
      },
    })
    // Only show MENU category actions in this view
    allActions.value = response.data.filter(action => action.actionCategory === 'MENU') || []
  } catch (error) {
    console.error('Failed to load actions:', error)
    console.error('Error response:', error.response)
    allActions.value = []
  } finally {
    isLoading.value = false
  }
})

const navigateToAction = (action) => {
  if (isExpired(action.deadline)) return
  router.push(action.mapsTo)
}

const isExpired = (deadline) => {
  if (!deadline) return false
  const end = new Date(deadline).getTime()
  const now = Date.now()
  return end <= now
}

const formatDeadlineShort = (dateStr) => {
  const deadline = new Date(dateStr)
  const now = new Date()
  const diff = deadline - now
  const days = Math.floor(diff / (1000 * 60 * 60 * 24))

  if (days > 0) {
    return `D-${days}`
  } else {
    const hours = Math.floor(diff / (1000 * 60 * 60))
    if (hours > 0) {
      return `${hours}시간 남음`
    }
    return '마감임박'
  }
}
</script>
