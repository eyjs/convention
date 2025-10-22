<template>
  <div class="space-y-6">
    <!-- 상태 모니터링 -->
    <div class="bg-white rounded-lg shadow p-6">
      <h3 class="text-lg font-bold text-gray-900 mb-4">챗봇 시스템 상태</h3>
      
      <div class="grid gap-4 md:grid-cols-2">
        <!-- 전체 상태 -->
        <div class="border rounded-lg p-4">
          <div class="flex items-center justify-between mb-2">
            <span class="text-sm font-medium text-gray-700">전체 시스템</span>
            <div class="flex items-center">
              <div :class="[
                'w-3 h-3 rounded-full mr-2',
                systemStatus.overall ? 'bg-green-500 animate-pulse' : 'bg-red-500'
              ]"></div>
              <span :class="[
                'text-sm font-semibold',
                systemStatus.overall ? 'text-green-600' : 'text-red-600'
              ]">
                {{ systemStatus.overall ? '정상' : '비정상' }}
              </span>
            </div>
          </div>
          <p class="text-xs text-gray-500">{{ systemStatus.message }}</p>
        </div>

        <!-- Ollama 연결 -->
        <div class="border rounded-lg p-4">
          <div class="flex items-center justify-between mb-2">
            <span class="text-sm font-medium text-gray-700">Ollama 서버</span>
            <div class="flex items-center">
              <div :class="[
                'w-3 h-3 rounded-full mr-2',
                systemStatus.ollama ? 'bg-green-500 animate-pulse' : 'bg-red-500'
              ]"></div>
              <span :class="[
                'text-sm font-semibold',
                systemStatus.ollama ? 'text-green-600' : 'text-red-600'
              ]">
                {{ systemStatus.ollama ? '연결됨' : '연결 안됨' }}
              </span>
            </div>
          </div>
          <p class="text-xs text-gray-500">{{ ollamaUrl }}</p>
        </div>

        <!-- 현재 모델 -->
        <div class="border rounded-lg p-4">
          <div class="flex items-center justify-between mb-2">
            <span class="text-sm font-medium text-gray-700">활성 모델</span>
            <div class="flex items-center">
              <div :class="[
                'w-3 h-3 rounded-full mr-2',
                systemStatus.model ? 'bg-green-500 animate-pulse' : 'bg-red-500'
              ]"></div>
              <span :class="[
                'text-sm font-semibold',
                systemStatus.model ? 'text-green-600' : 'text-red-600'
              ]">
                {{ systemStatus.model ? '로드됨' : '로드 안됨' }}
              </span>
            </div>
          </div>
          <p class="text-xs text-gray-500">{{ currentModel || '선택된 모델 없음' }}</p>
        </div>

        <!-- API 키 -->
        <div class="border rounded-lg p-4">
          <div class="flex items-center justify-between mb-2">
            <span class="text-sm font-medium text-gray-700">API 키</span>
            <div class="flex items-center">
              <div :class="[
                'w-3 h-3 rounded-full mr-2',
                systemStatus.apiKey ? 'bg-green-500 animate-pulse' : 'bg-red-500'
              ]"></div>
              <span :class="[
                'text-sm font-semibold',
                systemStatus.apiKey ? 'text-green-600' : 'text-red-600'
              ]">
                {{ systemStatus.apiKey ? '설정됨' : '미설정' }}
              </span>
            </div>
          </div>
          <p class="text-xs text-gray-500">{{ apiKeyCount }}개 등록</p>
        </div>
      </div>

      <button 
        @click="checkSystemStatus"
        :disabled="checking"
        class="mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 flex items-center space-x-2"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
        </svg>
        <span>{{ checking ? '확인 중...' : '상태 새로고침' }}</span>
      </button>
    </div>

    <!-- 모델 관리 -->
    <div class="bg-white rounded-lg shadow p-6">
      <h3 class="text-lg font-bold text-gray-900 mb-4">AI 모델 관리</h3>
      
      <!-- 현재 모델 -->
      <div class="mb-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm font-medium text-blue-900">현재 활성 모델</p>
            <p class="text-lg font-bold text-blue-700 mt-1">{{ currentModel || '선택된 모델 없음' }}</p>
          </div>
          <button 
            @click="loadAvailableModels"
            class="px-3 py-1.5 text-sm bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            모델 목록 새로고침
          </button>
        </div>
      </div>

      <!-- 사용 가능한 모델 목록 -->
      <div class="space-y-3">
        <p class="text-sm font-medium text-gray-700">사용 가능한 모델</p>
        <div v-if="loadingModels" class="text-center py-8 text-gray-500">
          모델 목록을 불러오는 중...
        </div>
        <div v-else-if="availableModels.length === 0" class="text-center py-8 text-gray-500">
          사용 가능한 모델이 없습니다. Ollama에서 모델을 설치해주세요.
        </div>
        <div v-else class="space-y-2">
          <div
            v-for="model in availableModels"
            :key="model.name"
            class="border rounded-lg p-4 hover:border-blue-500 transition-colors cursor-pointer"
            :class="{ 'border-blue-500 bg-blue-50': model.name === currentModel }"
            @click="selectModel(model.name)"
          >
            <div class="flex items-center justify-between">
              <div class="flex-1">
                <p class="font-medium text-gray-900">{{ model.name }}</p>
                <p class="text-xs text-gray-500 mt-1">크기: {{ formatSize(model.size) }}</p>
              </div>
              <div class="flex items-center space-x-2">
                <span v-if="model.name === currentModel" class="px-2 py-1 bg-blue-600 text-white text-xs rounded-full">
                  사용 중
                </span>
                <button
                  v-else
                  @click.stop="selectModel(model.name)"
                  class="px-3 py-1.5 text-sm bg-gray-600 text-white rounded hover:bg-gray-700"
                >
                  선택
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- API 키 관리 -->
    <div class="bg-white rounded-lg shadow p-6">
      <div class="flex items-center justify-between mb-4">
        <h3 class="text-lg font-bold text-gray-900">API 키 관리</h3>
        <button 
          @click="showApiKeyModal = true"
          class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 flex items-center space-x-2"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          <span>API 키 추가</span>
        </button>
      </div>

      <!-- API 키 목록 -->
      <div v-if="apiKeys.length === 0" class="text-center py-8 text-gray-500">
        등록된 API 키가 없습니다.
      </div>
      <div v-else class="space-y-3">
        <div
          v-for="apiKey in apiKeys"
          :key="apiKey.id"
          class="border rounded-lg p-4 hover:border-gray-400 transition-colors"
        >
          <div class="flex items-center justify-between">
            <div class="flex-1">
              <div class="flex items-center space-x-2">
                <p class="font-medium text-gray-900">{{ apiKey.provider }}</p>
                <span v-if="apiKey.isActive" class="px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full">
                  활성
                </span>
                <span v-else class="px-2 py-0.5 bg-gray-100 text-gray-600 text-xs rounded-full">
                  비활성
                </span>
              </div>
              <p class="text-sm text-gray-500 mt-1">{{ maskApiKey(apiKey.keyValue) }}</p>
              <p class="text-xs text-gray-400 mt-1">등록일: {{ formatDate(apiKey.createdAt) }}</p>
            </div>
            <div class="flex items-center space-x-2">
              <button
                @click="toggleApiKeyStatus(apiKey)"
                class="px-3 py-1.5 text-sm border rounded hover:bg-gray-50"
                :class="apiKey.isActive ? 'border-red-300 text-red-600' : 'border-green-300 text-green-600'"
              >
                {{ apiKey.isActive ? '비활성화' : '활성화' }}
              </button>
              <button
                @click="editApiKey(apiKey)"
                class="px-3 py-1.5 text-sm border border-blue-300 text-blue-600 rounded hover:bg-blue-50"
              >
                수정
              </button>
              <button
                @click="deleteApiKey(apiKey.id)"
                class="px-3 py-1.5 text-sm border border-red-300 text-red-600 rounded hover:bg-red-50"
              >
                삭제
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- API 키 추가/수정 모달 -->
    <div v-if="showApiKeyModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 w-full max-w-md">
        <h3 class="text-lg font-bold text-gray-900 mb-4">
          {{ editingApiKey ? 'API 키 수정' : 'API 키 추가' }}
        </h3>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">제공자</label>
            <select
              v-model="apiKeyForm.provider"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value="">선택하세요</option>
              <option value="OpenAI">OpenAI</option>
              <option value="Anthropic">Anthropic</option>
              <option value="Google">Google (Gemini)</option>
              <option value="Cohere">Cohere</option>
              <option value="Custom">Custom</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">API 키</label>
            <input
              v-model="apiKeyForm.keyValue"
              type="password"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              placeholder="sk-..."
            />
          </div>

          <div>
            <label class="flex items-center space-x-2">
              <input
                v-model="apiKeyForm.isActive"
                type="checkbox"
                class="w-4 h-4 text-blue-600 rounded"
              />
              <span class="text-sm text-gray-700">활성화</span>
            </label>
          </div>
        </div>

        <div class="flex space-x-3 mt-6">
          <button
            @click="saveApiKey"
            class="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            저장
          </button>
          <button
            @click="closeApiKeyModal"
            class="flex-1 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
          >
            취소
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/api'

// 상태 관리
const checking = ref(false)
const loadingModels = ref(false)
const systemStatus = ref({
  overall: false,
  ollama: false,
  model: false,
  apiKey: false,
  message: '시스템을 확인하는 중...'
})

const ollamaUrl = ref('http://localhost:11434')
const currentModel = ref('')
const availableModels = ref([])

// API 키 관리
const apiKeys = ref([])
const showApiKeyModal = ref(false)
const editingApiKey = ref(null)
const apiKeyForm = ref({
  provider: '',
  keyValue: '',
  isActive: true
})

const apiKeyCount = computed(() => apiKeys.value.filter(k => k.isActive).length)

// 시스템 상태 확인
async function checkSystemStatus() {
  checking.value = true
  try {
    const response = await apiClient.get('/admin/chatbot/status')
    systemStatus.value = response.data
  } catch (error) {
    console.error('Failed to check system status:', error)
    systemStatus.value = {
      overall: false,
      ollama: false,
      model: false,
      apiKey: false,
      message: '상태 확인 실패'
    }
  } finally {
    checking.value = false
  }
}

// 사용 가능한 모델 로드
async function loadAvailableModels() {
  loadingModels.value = true
  try {
    const response = await apiClient.get('/admin/chatbot/models')
    availableModels.value = response.data.models || []
    currentModel.value = response.data.currentModel || ''
  } catch (error) {
    console.error('Failed to load models:', error)
    availableModels.value = []
  } finally {
    loadingModels.value = false
  }
}

// 모델 선택
async function selectModel(modelName) {
  if (modelName === currentModel.value) return
  
  try {
    await apiClient.post('/admin/chatbot/select-model', { modelName })
    currentModel.value = modelName
    await checkSystemStatus()
    alert(`모델이 "${modelName}"(으)로 변경되었습니다.`)
  } catch (error) {
    console.error('Failed to select model:', error)
    alert('모델 변경에 실패했습니다.')
  }
}

// API 키 로드
async function loadApiKeys() {
  try {
    const response = await apiClient.get('/admin/chatbot/api-keys')
    apiKeys.value = response.data
  } catch (error) {
    console.error('Failed to load API keys:', error)
  }
}

// API 키 저장
async function saveApiKey() {
  if (!apiKeyForm.value.provider || !apiKeyForm.value.keyValue) {
    alert('모든 필드를 입력해주세요.')
    return
  }

  try {
    if (editingApiKey.value) {
      await apiClient.put(`/admin/chatbot/api-keys/${editingApiKey.value.id}`, apiKeyForm.value)
    } else {
      await apiClient.post('/admin/chatbot/api-keys', apiKeyForm.value)
    }
    
    await loadApiKeys()
    await checkSystemStatus()
    closeApiKeyModal()
    alert('API 키가 저장되었습니다.')
  } catch (error) {
    console.error('Failed to save API key:', error)
    alert('API 키 저장에 실패했습니다.')
  }
}

// API 키 수정
function editApiKey(apiKey) {
  editingApiKey.value = apiKey
  apiKeyForm.value = {
    provider: apiKey.provider,
    keyValue: '', // 보안상 기존 키는 표시하지 않음
    isActive: apiKey.isActive
  }
  showApiKeyModal.value = true
}

// API 키 삭제
async function deleteApiKey(id) {
  if (!confirm('정말 이 API 키를 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/admin/chatbot/api-keys/${id}`)
    await loadApiKeys()
    await checkSystemStatus()
    alert('API 키가 삭제되었습니다.')
  } catch (error) {
    console.error('Failed to delete API key:', error)
    alert('API 키 삭제에 실패했습니다.')
  }
}

// API 키 활성/비활성 토글
async function toggleApiKeyStatus(apiKey) {
  try {
    await apiClient.patch(`/admin/chatbot/api-keys/${apiKey.id}/toggle`, {
      isActive: !apiKey.isActive
    })
    await loadApiKeys()
    await checkSystemStatus()
  } catch (error) {
    console.error('Failed to toggle API key:', error)
    alert('상태 변경에 실패했습니다.')
  }
}

// 모달 닫기
function closeApiKeyModal() {
  showApiKeyModal.value = false
  editingApiKey.value = null
  apiKeyForm.value = {
    provider: '',
    keyValue: '',
    isActive: true
  }
}

// 유틸리티 함수
function maskApiKey(key) {
  if (!key || key.length < 10) return '****'
  return key.substring(0, 8) + '...' + key.substring(key.length - 4)
}

function formatSize(bytes) {
  if (!bytes) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
}

function formatDate(dateString) {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('ko-KR')
}

onMounted(() => {
  checkSystemStatus()
  loadAvailableModels()
  loadApiKeys()
})
</script>
