<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white shadow-sm sticky top-0 z-40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <div class="flex items-center">
            <button @click="router.back()" class="p-2 hover:bg-gray-100 rounded-lg">
              <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/>
              </svg>
            </button>
            <h1 class="text-xl sm:text-2xl font-bold text-gray-900 ml-2">액션 관리</h1>
          </div>
          <div class="flex gap-2">
            <button
              @click="showTemplateModal = true"
              class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 text-sm"
            >
              템플릿 추가
            </button>
            <button
              @click="showCustomModal = true"
              class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 text-sm"
            >
              커스텀 액션
            </button>
          </div>
        </div>
      </div>
    </header>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      
      <!-- 로딩 -->
      <div v-if="loading" class="flex justify-center items-center py-20">
        <div class="spinner"></div>
      </div>

      <!-- 에러 -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4 text-red-800">
        {{ error }}
      </div>

      <!-- 액션이 없을 때 -->
      <div v-else-if="!actions || actions.length === 0" class="bg-white rounded-lg shadow-sm p-12 text-center">
        <svg class="w-16 h-16 mx-auto text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
        </svg>
        <h3 class="text-lg font-medium text-gray-900 mb-2">등록된 액션이 없습니다</h3>
        <p class="text-gray-600 mb-6">템플릿을 추가하거나 커스텀 액션을 생성하세요</p>
        <div class="flex justify-center gap-3">
          <button
            @click="showTemplateModal = true"
            class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            템플릿에서 추가
          </button>
          <button
            @click="showCustomModal = true"
            class="px-6 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
          >
            커스텀 액션 생성
          </button>
        </div>
      </div>

      <!-- 액션 목록 -->
      <div v-else class="space-y-4">
        <div
          v-for="action in actions"
          :key="action.id"
          class="bg-white rounded-lg shadow-sm p-6 border-l-4"
          :class="action.isActive ? 'border-blue-600' : 'border-gray-300'"
        >
          <div class="flex items-start justify-between">
            <div class="flex-1">
              <div class="flex items-center gap-3 mb-2">
                <h3 class="text-lg font-semibold text-gray-900">{{ action.title }}</h3>
                <span v-if="action.templateName" class="px-2 py-1 bg-blue-100 text-blue-700 text-xs rounded">
                  템플릿
                </span>
                <span v-if="action.isRequired" class="px-2 py-1 bg-red-100 text-red-700 text-xs rounded">
                  필수
                </span>
                <span v-if="!action.isActive" class="px-2 py-1 bg-gray-100 text-gray-700 text-xs rounded">
                  비활성
                </span>
              </div>
              
              <div class="text-sm text-gray-600 space-y-1">
                <p><strong>타입:</strong> {{ action.actionType }}</p>
                <p><strong>경로:</strong> {{ action.mapsTo }}</p>
                <p v-if="action.deadline"><strong>마감:</strong> {{ formatDate(action.deadline) }}</p>
                <p><strong>완료율:</strong> {{ action.completedCount }} / {{ action.totalGuestCount }} ({{ completionRate(action) }}%)</p>
              </div>
            </div>

            <div class="flex gap-2">
              <button
                @click="toggleActive(action)"
                class="px-3 py-1 text-sm rounded"
                :class="action.isActive ? 'bg-red-100 text-red-700' : 'bg-green-100 text-green-700'"
              >
                {{ action.isActive ? '비활성화' : '활성화' }}
              </button>
              <button
                @click="deleteAction(action.id)"
                class="px-3 py-1 text-sm bg-red-100 text-red-700 rounded hover:bg-red-200"
              >
                삭제
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- 템플릿 모달 -->
      <div v-if="showTemplateModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50" @click.self="showTemplateModal = false">
        <div class="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto m-4">
          <div class="sticky top-0 bg-white border-b px-6 py-4">
            <h2 class="text-xl font-bold text-gray-900">템플릿에서 액션 추가</h2>
          </div>

          <div class="p-6">
            <div v-if="availableTemplates.length === 0" class="text-center py-8 text-gray-500">
              사용 가능한 템플릿이 없습니다
            </div>
            <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div
                v-for="template in availableTemplates"
                :key="template.id"
                @click="applyTemplate(template)"
                class="p-4 border-2 rounded-lg cursor-pointer hover:border-blue-600 transition-colors"
              >
                <h3 class="font-semibold text-gray-900">{{ template.templateName }}</h3>
                <p class="text-sm text-gray-600 mt-1">{{ template.description }}</p>
                <span class="inline-block mt-2 px-2 py-1 bg-gray-100 text-gray-700 text-xs rounded">
                  {{ template.category }}
                </span>
              </div>
            </div>
          </div>

          <div class="sticky bottom-0 bg-gray-50 px-6 py-4 border-t">
            <button
              @click="showTemplateModal = false"
              class="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
            >
              닫기
            </button>
          </div>
        </div>
      </div>

      <!-- 커스텀 액션 모달 (간단 버전) -->
      <div v-if="showCustomModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50" @click.self="showCustomModal = false">
        <div class="bg-white rounded-lg shadow-xl max-w-2xl w-full m-4">
          <div class="px-6 py-4 border-b">
            <h2 class="text-xl font-bold text-gray-900">커스텀 액션 생성</h2>
          </div>

          <div class="p-6 space-y-4">
            <p class="text-gray-600">커스텀 액션 생성 기능은 개발 중입니다.</p>
          </div>

          <div class="px-6 py-4 border-t flex justify-end">
            <button
              @click="showCustomModal = false"
              class="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
            >
              닫기
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import apiClient from '@/services/api'

const router = useRouter()
const route = useRoute()

const loading = ref(true)
const error = ref('')
const actions = ref([])
const availableTemplates = ref([])
const showTemplateModal = ref(false)
const showCustomModal = ref(false)

const conventionId = route.params.conventionId

async function fetchData() {
  loading.value = true
  error.value = ''
  
  try {
    const response = await apiClient.get(`/admin/action-management/convention/${conventionId}`)
    actions.value = response.data.actions || []
    availableTemplates.value = response.data.availableTemplates || []
  } catch (err) {
    console.error('Failed to fetch action management data:', err)
    error.value = err.response?.data?.message || '데이터를 불러오는데 실패했습니다.'
  } finally {
    loading.value = false
  }
}

async function applyTemplate(template) {
  try {
    await apiClient.post(`/admin/action-templates/${template.id}/apply-to-convention/${conventionId}`, {
      isActive: true,
      isRequired: false,
      orderNum: actions.value.length
    })
    showTemplateModal.value = false
    await fetchData()
  } catch (err) {
    alert('템플릿 적용 실패: ' + (err.response?.data?.message || err.message))
  }
}

async function toggleActive(action) {
  try {
    // TODO: 개별 액션 토글 API 구현 필요
    action.isActive = !action.isActive
  } catch (err) {
    alert('상태 변경 실패: ' + err.message)
  }
}

async function deleteAction(actionId) {
  if (!confirm('이 액션을 삭제하시겠습니까?')) return
  
  try {
    // TODO: 삭제 API 구현 필요
    actions.value = actions.value.filter(a => a.id !== actionId)
  } catch (err) {
    alert('삭제 실패: ' + err.message)
  }
}

function completionRate(action) {
  if (action.totalGuestCount === 0) return 0
  return Math.round((action.completedCount / action.totalGuestCount) * 100)
}

function formatDate(dateString) {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleDateString('ko-KR')
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.spinner {
  border: 3px solid rgba(59, 130, 246, 0.3);
  border-top: 3px solid rgb(59, 130, 246);
  border-radius: 50%;
  width: 40px;
  height: 40px;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>
