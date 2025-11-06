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

    <!-- 3x3 그리드 -->
    <div v-else-if="allActions.length > 0" class="px-4 py-6">
      <div class="grid grid-cols-3 gap-4">
        <div
          v-for="action in allActions"
          :key="action.id"
          @click="!isExpired(action.deadline) && navigateToAction(action)"
          class="flex flex-col items-center justify-center bg-white rounded-2xl shadow-sm transition-all p-4 border border-gray-100"
          :class="[
            isExpired(action.deadline)
              ? 'opacity-50 cursor-not-allowed'
              : 'hover:shadow-md cursor-pointer',
          ]"
        >
          <!-- 아이콘 -->
          <div
            class="w-16 h-16 bg-gradient-to-br from-primary-100 to-primary-200 rounded-full flex items-center justify-center mb-3"
          >
            <span class="text-3xl">{{ action.iconClass || '📌' }}</span>
          </div>

          <!-- 이름 -->
          <h3
            class="text-center font-semibold text-sm leading-tight"
            :class="
              isExpired(action.deadline)
                ? 'text-gray-400 line-through'
                : 'text-gray-900'
            "
          >
            {{ action.title }}
          </h3>

          <!-- 마감기한 뱃지 (있는 경우만) -->
          <div
            v-if="action.deadline"
            class="mt-2 px-2 py-1 text-xs font-medium rounded-full"
            :class="
              isExpired(action.deadline)
                ? 'bg-gray-200 text-gray-600'
                : 'bg-red-100 text-red-700'
            "
          >
            {{
              isExpired(action.deadline)
                ? '마감완료'
                : formatDeadlineShort(action.deadline)
            }}
          </div>

          <!-- 필수 뱃지 -->
          <div
            v-if="action.isRequired"
            class="mt-2 px-2 py-1 bg-blue-600 text-white text-xs font-bold rounded-full"
          >
            필수
          </div>
        </div>
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
    console.log('Fetching actions from:', url)
    const response = await apiClient.get(url)
    console.log('Actions response:', response)
    console.log('Actions data:', response.data)
    allActions.value = response.data || []
    console.log('All actions loaded:', allActions.value)
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
