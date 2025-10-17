<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <!-- Header -->
    <header class="bg-white shadow-sm sticky top-0 z-40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center h-16">
          <button @click="router.push('/admin')" class="p-2 hover:bg-gray-100 rounded-lg transition-colors" title="Back to Convention List">
            <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/>
            </svg>
          </button>
          <h1 class="text-xl sm:text-2xl font-bold text-gray-900 ml-2">챗봇 관리</h1>
        </div>
      </div>
    </header>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="bg-white rounded-lg shadow-sm p-6">
        <h2 class="text-xl font-semibold mb-4">챗봇 전체 색인 관리</h2>

        <!-- Indexing Status -->
        <div class="mb-6">
          <h3 class="text-lg font-medium text-gray-800 mb-2">현재 상태</h3>
          <div class="bg-gray-50 rounded-lg p-4 flex items-center justify-between">
            <p class="text-gray-600">전체 색인된 문서 수</p>
            <div v-if="loadingStatus" class="animate-pulse bg-gray-300 h-6 w-16 rounded-md"></div>
            <p v-else class="text-2xl font-bold text-blue-600">{{ documentCount }} 개</p>
          </div>
        </div>

        <!-- Indexing Actions -->
        <div>
          <h3 class="text-lg font-medium text-gray-800 mb-2">작업</h3>
          <div class="bg-gray-50 rounded-lg p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="font-medium text-gray-900">전체 데이터 재색인</p>
                <p class="text-sm text-gray-500 mt-1">시스템의 모든 정보를 다시 읽어 챗봇의 지식 베이스를 업데이트합니다. 데이터 양에 따라 시간이 걸릴 수 있습니다.</p>
              </div>
              <button @click="handleReindex" :disabled="reindexing" class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-wait transition-colors">
                <span v-if="reindexing">색인 중...</span>
                <span v-else>재색인 시작</span>
              </button>
            </div>
          </div>
        </div>

        <!-- Result Message -->
        <div v-if="resultMessage" class="mt-4 text-center p-3 rounded-lg" :class="resultType === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'">
          {{ resultMessage }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/services/api'

const router = useRouter()
const loadingStatus = ref(true)
const reindexing = ref(false)
const documentCount = ref(0)
const resultMessage = ref('')
const resultType = ref('success')

async function fetchStatus() {
  loadingStatus.value = true
  try {
    const response = await apiClient.get('/admin/indexing/status')
    documentCount.value = response.data.documentCount
  } catch (error) {
    console.error('Failed to fetch indexing status:', error)
    resultMessage.value = '상태를 불러오는데 실패했습니다.'
    resultType.value = 'error'
  } finally {
    loadingStatus.value = false
  }
}

async function handleReindex() {
  if (!confirm('모든 데이터를 다시 색인하시겠습니까? 기존 색인 정보는 유지되며, 새로운 정보가 추가되거나 업데이트됩니다.')) {
    return
  }

  reindexing.value = true
  resultMessage.value = ''
  try {
    const response = await apiClient.post('/admin/indexing/reindex')
    resultMessage.value = response.data.message
    resultType.value = 'success'
    await fetchStatus() // Refresh status
  } catch (error) {
    console.error('Failed to reindex:', error)
    resultMessage.value = error.response?.data?.message || '재색인 작업에 실패했습니다.'
    resultType.value = 'error'
  } finally {
    reindexing.value = false
  }
}

onMounted(() => {
  fetchStatus()
})
</script>
