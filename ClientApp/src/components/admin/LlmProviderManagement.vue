<template>
  <div class="space-y-6">
    <!-- 시스템 상태 카드 -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
      <div class="bg-white rounded-lg shadow-sm p-4 border-l-4 border-blue-500">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">현재 LLM</p>
            <p class="text-lg font-bold text-gray-900 mt-1">{{ currentProvider?.providerType || '없음' }}</p>
            <p class="text-xs text-gray-500">{{ currentProvider?.modelName }}</p>
          </div>
          <div class="p-3 bg-blue-100 rounded-full">
            <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"/>
            </svg>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm p-4 border-l-4 border-green-500">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">VectorStore 상태</p>
            <p class="text-lg font-bold text-gray-900 mt-1">
              {{ vectorStatus?.isConnected ? '연결됨' : '연결 안됨' }}
            </p>
            <button
              @click="showVectorDetailModal = true"
              class="text-xs text-blue-600 hover:underline mt-1"
            >
              상세보기
            </button>
          </div>
          <div class="p-3 bg-green-100 rounded-full">
            <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4"/>
            </svg>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm p-4 border-l-4 border-purple-500">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">등록된 Provider</p>
            <p class="text-lg font-bold text-gray-900 mt-1">{{ providers.length }}개</p>
            <p class="text-xs text-gray-500">활성: {{ activeProvidersCount }}개</p>
          </div>
          <div class="p-3 bg-purple-100 rounded-full">
            <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"/>
            </svg>
          </div>
        </div>
      </div>
    </div>

    <!-- Provider 목록 -->
    <div class="bg-white rounded-lg shadow-sm">
      <div class="p-4 border-b flex items-center justify-between">
        <h4 class="font-semibold text-gray-900">LLM Provider 목록</h4>
        <button 
          @click="showAddModal = true"
          class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 text-sm flex items-center gap-2"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
          </svg>
          Provider 추가
        </button>
      </div>

      <div v-if="loading" class="text-center py-12 text-gray-500">
        <div class="inline-block w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
        <p class="mt-4">로딩 중...</p>
      </div>

      <div v-else-if="providers.length === 0" class="text-center py-12 text-gray-500">
        등록된 Provider가 없습니다.
      </div>

      <div v-else class="divide-y">
        <div
          v-for="provider in providers"
          :key="provider.id"
          :class="[
            'p-4 hover:bg-gray-50 transition-colors',
            provider.isCurrent && 'bg-blue-50'
          ]"
        >
          <div class="flex items-center justify-between">
            <div class="flex-1">
              <div class="flex items-center gap-3">
                <h5 class="font-medium text-gray-900">{{ provider.providerType }}</h5>
                <span v-if="provider.isCurrent" class="px-2 py-0.5 bg-blue-600 text-white text-xs rounded-full flex items-center gap-1">
                  <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"/>
                  </svg>
                  사용중
                </span>
                <span v-else-if="provider.isActive" class="px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full">
                  활성
                </span>
                <span v-else class="px-2 py-0.5 bg-gray-100 text-gray-600 text-xs rounded-full">
                  비활성
                </span>
              </div>
              <p class="text-sm text-gray-600 mt-1">모델: {{ provider.modelName || '-' }}</p>
              <p class="text-sm text-gray-500">
                {{ provider.baseUrl || 'Default URL' }}
                <span v-if="provider.hasApiKey" class="ml-2 text-xs text-green-600">🔑 API Key 설정됨</span>
              </p>
              <p class="text-xs text-gray-400 mt-1">등록: {{ formatDate(provider.createdAt) }}</p>
            </div>
            <div class="flex gap-2">
              <button
                v-if="!provider.isCurrent"
                @click="swapProvider(provider)"
                class="px-4 py-2 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-lg hover:from-blue-700 hover:to-blue-800 transition-all shadow-sm text-sm font-medium"
                :disabled="swapping"
              >
                <span v-if="swapping">전환중...</span>
                <span v-else>즉시 전환</span>
              </button>
              <button
                @click="editProvider(provider)"
                class="px-3 py-2 text-sm bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200"
              >
                수정
              </button>
              <button
                v-if="!provider.isCurrent"
                @click="deleteProvider(provider.id)"
                class="px-3 py-2 text-sm bg-red-100 text-red-700 rounded-lg hover:bg-red-200"
              >
                삭제
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- VectorStore 상세 모달 -->
    <div v-if="showVectorDetailModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50" @click="showVectorDetailModal = false">
      <div class="bg-white rounded-lg p-6 w-full max-w-2xl" @click.stop>
        <h3 class="text-lg font-bold text-gray-900 mb-4">VectorStore 상세 정보</h3>

        <div class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="border rounded-lg p-4">
              <p class="text-sm text-gray-600">연결 상태</p>
              <p class="text-xl font-bold text-green-600 mt-1">
                {{ vectorStatus?.isConnected ? '✓ 연결됨' : '✗ 연결 안됨' }}
              </p>
            </div>
            <div class="border rounded-lg p-4">
              <p class="text-sm text-gray-600">총 벡터 수</p>
              <p class="text-xl font-bold text-gray-900 mt-1">
                {{ vectorStatus?.totalVectors?.toLocaleString() || 0 }}
              </p>
            </div>
          </div>

          <div class="border rounded-lg p-4">
            <p class="text-sm text-gray-600 mb-2">용량 사용률</p>
            <div class="w-full bg-gray-200 rounded-full h-4">
              <div 
                class="bg-blue-600 h-4 rounded-full transition-all"
                :style="{ width: `${vectorStatus?.capacity?.usagePercent || 0}%` }"
              ></div>
            </div>
            <p class="text-xs text-gray-500 mt-1">
              {{ vectorStatus?.capacity?.used?.toLocaleString() }} / {{ vectorStatus?.capacity?.total?.toLocaleString() }}
              ({{ vectorStatus?.capacity?.usagePercent?.toFixed(2) }}%)
            </p>
          </div>

          <div class="border rounded-lg p-4">
            <p class="text-sm text-gray-600 mb-2">데이터 유형별 분포</p>
            <div class="space-y-2">
              <div v-for="item in vectorStatus?.byType" :key="item.type" class="flex items-center justify-between text-sm">
                <span class="text-gray-700">{{ item.type }}</span>
                <span class="font-medium">{{ item.count?.toLocaleString() }}</span>
              </div>
            </div>
          </div>

          <div class="border rounded-lg p-4">
            <p class="text-sm text-gray-600">마지막 색인 시간</p>
            <p class="text-sm text-gray-900 mt-1">
              {{ vectorStatus?.lastIndexed ? formatDateTime(vectorStatus.lastIndexed) : '-' }}
            </p>
          </div>
        </div>

        <div class="flex justify-end mt-6">
          <button
            @click="showVectorDetailModal = false"
            class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700"
          >
            닫기
          </button>
        </div>
      </div>
    </div>

    <!-- Add/Edit Modal -->
    <div v-if="showAddModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50" @click="showAddModal = false">
      <div class="bg-white rounded-lg p-6 w-full max-w-md" @click.stop>
        <h3 class="text-lg font-bold text-gray-900 mb-4">
          {{ editingProvider ? 'Provider 수정' : 'Provider 추가' }}
        </h3>

        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Provider 타입 *</label>
            <select
              v-model="form.providerType"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value="">선택하세요</option>
              <option value="OpenAI">OpenAI</option>
              <option value="Anthropic">Anthropic (Claude)</option>
              <option value="Google">Google (Gemini)</option>
              <option value="Ollama">Ollama (로컬)</option>
              <option value="Llama3">Llama3</option>
              <option value="Custom">Custom</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">모델명 *</label>
            <input
              v-model="form.modelName"
              type="text"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              placeholder="gpt-4, claude-3-opus, gemini-pro 등"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">API Key</label>
            <input
              v-model="form.apiKey"
              type="password"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              placeholder="sk-... (Ollama는 불필요)"
            />
            <p class="text-xs text-gray-500 mt-1">수정 시 비워두면 기존 키 유지</p>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Base URL</label>
            <input
              v-model="form.baseUrl"
              type="text"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              placeholder="http://localhost:11434 (선택)"
            />
          </div>

          <div>
            <label class="flex items-center gap-2">
              <input
                v-model="form.isActive"
                type="checkbox"
                class="w-4 h-4 text-blue-600 rounded"
              />
              <span class="text-sm text-gray-700">활성화 상태로 등록</span>
            </label>
          </div>
        </div>

        <div class="flex gap-3 mt-6">
          <button
            @click="saveProvider"
            class="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            저장
          </button>
          <button
            @click="closeModal"
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

const loading = ref(false)
const swapping = ref(false)
const providers = ref([])
const currentProvider = ref(null)
const vectorStatus = ref(null)
const showAddModal = ref(false)
const showVectorDetailModal = ref(false)
const editingProvider = ref(null)
const form = ref({
  providerType: '',
  modelName: '',
  apiKey: '',
  baseUrl: '',
  isActive: false
})

const activeProvidersCount = computed(() => 
  providers.value.filter(p => p.isActive).length
)

async function fetchProviders() {
  loading.value = true
  try {
    // 먼저 테스트 엔드포인트로 확인
    console.log('Fetching providers...')
    const testResponse = await apiClient.get('/admin/chatbot/llm-providers/test')
    console.log('Test response:', testResponse.data)
    
      const response = await apiClient.get('/admin/chatbot/llm-providers')
    console.log('Main response:', response.data)
    providers.value = response.data
    
    // 현재 사용중인 Provider 찾기
    currentProvider.value = providers.value.find(p => p.isCurrent) || null
  } catch (error) {
    console.error('Failed to fetch providers:', error)
    console.error('Error response:', error.response?.data)
    alert('Provider 목록을 불러오는데 실패했습니다: ' + (error.response?.data?.error || error.message))
  } finally {
    loading.value = false
  }
}

async function fetchVectorStatus() {
  try {
    console.log('Fetching vector status...')
    const response = await apiClient.get('/admin/chatbot/vector-status')
    console.log('Vector status response:', response.data)
    vectorStatus.value = response.data
  } catch (error) {
    console.error('Failed to fetch vector status:', error)
    console.error('Error details:', {
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      url: error.config?.url
    })
    // 에러가 나도 기본값 설정
    vectorStatus.value = {
      isConnected: false,
      totalVectors: 0,
      capacity: { used: 0, total: 0, usagePercent: 0 },
      byType: []
    }
  }
}

async function swapProvider(provider) {
  if (!confirm(`${provider.providerType}로 LLM을 즉시 전환하시겠습니까?`)) return

  swapping.value = true
  try {
      await apiClient.patch(`/admin/chatbot/llm-providers/${provider.id}/toggle`, {
      isActive: true
    })
    
    await fetchProviders()
    alert(`${provider.providerType}로 전환되었습니다.`)
  } catch (error) {
    console.error('Failed to swap provider:', error)
    alert('전환에 실패했습니다: ' + (error.response?.data?.error || error.message))
  } finally {
    swapping.value = false
  }
}

async function saveProvider() {
  if (!form.value.providerType || !form.value.modelName) {
    alert('Provider 타입과 모델명은 필수입니다.')
    return
  }

  try {
    if (editingProvider.value) {
        await apiClient.put(`/admin/chatbot/llm-providers/${editingProvider.value.id}`, form.value)
    } else {
        await apiClient.post('/admin/chatbot/llm-providers', form.value)
    }
    await fetchProviders()
    closeModal()
    alert('저장되었습니다.')
  } catch (error) {
    console.error('Failed to save provider:', error)
    alert('저장에 실패했습니다: ' + (error.response?.data?.error || error.message))
  }
}

function editProvider(provider) {
  editingProvider.value = provider
  form.value = {
    providerType: provider.providerType,
    modelName: provider.modelName,
    apiKey: '', // 보안상 기존 키는 표시 안함
    baseUrl: provider.baseUrl || '',
    isActive: provider.isActive
  }
  showAddModal.value = true
}

async function deleteProvider(id) {
  if (!confirm('정말 삭제하시겠습니까?')) return

  try {
      await apiClient.delete(`/admin/chatbot/llm-providers/${id}`)
    await fetchProviders()
    alert('삭제되었습니다.')
  } catch (error) {
    console.error('Failed to delete provider:', error)
    alert('삭제에 실패했습니다: ' + (error.response?.data?.error || error.message))
  }
}

function closeModal() {
  showAddModal.value = false
  editingProvider.value = null
  form.value = {
    providerType: '',
    modelName: '',
    apiKey: '',
    baseUrl: '',
    isActive: false
  }
}

function formatDate(dateString) {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleDateString('ko-KR')
}

function formatDateTime(dateString) {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleString('ko-KR')
}

onMounted(() => {
  fetchProviders()
  fetchVectorStatus()
})
</script>
