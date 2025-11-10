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
      <div class="space-y-3">
        <GenericMenuItem
          v-for="action in allActions"
          :key="action.id"
          :feature="action"
        />
      </div>
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
import GenericMenuItem from '@/dynamic-features/common/GenericMenuItem.vue'

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
    // 메뉴 액션과 상태 정보를 병렬로 가져오기
    const [actionsResponse, statusesResponse] = await Promise.all([
      apiClient.get(`/conventions/${conventionId}/actions/menu`),
      apiClient.get(`/conventions/${conventionId}/actions/statuses`),
    ])

    const actions = actionsResponse.data || []
    const statuses = statusesResponse.data || []

    // 상태 정보를 맵으로 변환
    const statusMap = new Map(statuses.map((s) => [s.conventionActionId, s]))

    // 액션에 isComplete 정보 추가
    allActions.value = actions.map((action) => ({
      ...action,
      isComplete: statusMap.get(action.id)?.isComplete || false,
    }))
  } catch (error) {
    console.error('Failed to load menu actions:', error)
    console.error('Error response:', error.response)
    allActions.value = []
  } finally {
    isLoading.value = false
  }
})
</script>
