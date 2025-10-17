<template>
  <div class="min-h-screen min-h-dvh bg-gray-50 px-4 py-6">
    <!-- 헤더 -->
    <div class="mb-6">
      <h1 class="text-2xl font-bold text-gray-900">참가자</h1>
      <p class="text-sm text-gray-600 mt-1">행사에 참여하는 전체 인원을 확인하세요.</p>
    </div>

    <!-- 검색 -->
    <div class="mb-6 sticky top-4 z-10">
      <div class="relative">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="이름, 소속 등으로 검색"
          class="w-full pl-10 pr-4 py-3 border rounded-xl shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <svg class="absolute left-3 top-3.5 w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
      </div>
    </div>

    <!-- 로딩 -->
    <div v-if="loading" class="text-center py-12">
      <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      <p class="mt-4 text-gray-600">참가자 목록을 불러오는 중...</p>
    </div>

    <!-- 데이터 없음 -->
    <div v-else-if="filteredParticipants.length === 0" class="text-center py-16">
      <svg class="w-20 h-20 mx-auto mb-4 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M15 21a6 6 0 00-9-5.197M4 7a4 4 0 118 0 4 4 0 01-8 0z" />
      </svg>
      <p class="text-lg font-medium text-gray-900 mb-2">
        {{ searchQuery ? '검색 결과가 없습니다' : '등록된 참가자가 없습니다' }}
      </p>
      <p class="text-sm text-gray-500">
        {{ searchQuery ? '다른 검색어로 시도해 보세요.' : '관리자에게 문의하세요.' }}
      </p>
    </div>

    <!-- 참가자 목록 -->
    <div v-else class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      <div
        v-for="participant in filteredParticipants"
        :key="participant.id"
        class="bg-white rounded-xl shadow-sm hover:shadow-lg transition-all p-4 border border-transparent hover:border-blue-500"
      >
        <div class="flex flex-col items-center text-center">
          <div class="w-20 h-20 rounded-full bg-gray-200 mb-4 flex items-center justify-center">
            <span class="text-3xl text-gray-500">{{ participant.guestName.charAt(0) }}</span>
          </div>
          <p class="font-bold text-gray-900 text-lg">{{ participant.guestName }}</p>
          <p v-if="participant.corpName" class="text-sm text-gray-600 mt-1">{{ participant.corpName }}</p>
          <p v-if="participant.corpPart" class="text-xs text-gray-500 mt-0.5">{{ participant.corpPart }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useConventionStore } from '@/stores/convention'
import { guestAPI } from '@/services/guestService'

const conventionStore = useConventionStore()

const loading = ref(true)
const participants = ref([])
const searchQuery = ref('')

const filteredParticipants = computed(() => {
  if (!searchQuery.value) {
    return participants.value
  }
  const query = searchQuery.value.toLowerCase()
  return participants.value.filter(p =>
    p.guestName.toLowerCase().includes(query) ||
    p.corpName?.toLowerCase().includes(query) ||
    p.corpPart?.toLowerCase().includes(query)
  )
})

const fetchParticipants = async () => {
  loading.value = true
  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) {
      alert('현재 행사 정보를 찾을 수 없습니다. 홈으로 돌아가 다시 시도해주세요.')
      return
    }
    const response = await guestAPI.getParticipants({ conventionId })
    participants.value = response.data
  } catch (error) {
    console.error('Failed to fetch participants:', error)
    alert('참가자 목록을 불러오는데 실패했습니다.')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchParticipants()
})
</script>
