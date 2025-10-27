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
              @click="openCustomModal"
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
            @click="openCustomModal"
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

      <!-- 커스텀 액션 모달 -->
      <div v-if="showCustomModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="showCustomModal = false">
        <div class="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
          <div class="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between">
            <h2 class="text-xl font-bold text-gray-900">커스텀 액션 생성</h2>
            <button @click="showCustomModal = false" class="text-gray-400 hover:text-gray-600">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </button>
          </div>

          <form @submit.prevent="createCustomAction" class="p-6 space-y-6">
            <!-- 기본 정보 -->
            <div class="space-y-4">
              <h3 class="text-lg font-semibold text-gray-900">기본 정보</h3>

              <!-- 액션 타입 -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">
                  액션 타입 (고유 ID)
                  <span class="text-red-500">*</span>
                </label>
                <input
                  v-model="customActionForm.actionType"
                  type="text"
                  placeholder="예: CUSTOM_BUTTON_001"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                />
                <p class="mt-1 text-xs text-gray-500">영문 대문자, 숫자, 언더스코어만 사용 (예: CUSTOM_BUTTON_001)</p>
              </div>

              <!-- 제목 -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">
                  제목
                  <span class="text-red-500">*</span>
                </label>
                <input
                  v-model="customActionForm.title"
                  type="text"
                  placeholder="사용자에게 표시될 제목"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                />
              </div>
            </div>

            <!-- 액션 카테고리 -->
            <div class="space-y-4">
              <h3 class="text-lg font-semibold text-gray-900">액션 타입</h3>

              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">
                  액션 카테고리
                  <span class="text-red-500">*</span>
                </label>
                <div class="grid grid-cols-2 md:grid-cols-3 gap-3">
                  <div
                    v-for="category in actionCategories"
                    :key="category.key"
                    @click="selectCategory(category)"
                    :class="[
                      'p-4 border-2 rounded-lg cursor-pointer transition-all',
                      customActionForm.actionCategory === category.key
                        ? 'border-blue-600 bg-blue-50'
                        : 'border-gray-200 hover:border-blue-300'
                    ]"
                  >
                    <div class="font-semibold text-sm">{{ category.displayName }}</div>
                    <div class="text-xs text-gray-600 mt-1">{{ category.description }}</div>
                  </div>
                </div>
              </div>

              <!-- 선택한 카테고리의 가이드 -->
              <div v-if="selectedCategoryGuide" class="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <div class="flex items-start justify-between mb-2">
                  <h4 class="font-semibold text-blue-900">📘 {{ selectedCategoryGuide.title }}</h4>
                  <button
                    type="button"
                    @click="copyGuideExample"
                    class="px-3 py-1 text-xs bg-blue-600 text-white rounded hover:bg-blue-700"
                  >
                    예시 복사
                  </button>
                </div>
                <p class="text-sm text-blue-800 mb-3">{{ selectedCategoryGuide.content }}</p>
                <pre class="bg-white p-3 rounded border border-blue-200 text-xs overflow-x-auto">{{ selectedCategoryGuide.example }}</pre>
              </div>
            </div>

            <!-- 타겟 위치 -->
            <div v-if="customActionForm.actionCategory">
              <label class="block text-sm font-medium text-gray-700 mb-2">
                표시 위치
                <span class="text-red-500">*</span>
                <button
                  type="button"
                  @click="showLocationGuide = !showLocationGuide"
                  class="ml-2 text-blue-600 hover:text-blue-700"
                >
                  <svg class="w-4 h-4 inline" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd"/>
                  </svg>
                </button>
              </label>

              <!-- 위치 가이드 토글 -->
              <div v-if="showLocationGuide" class="mb-3 p-3 bg-yellow-50 border border-yellow-200 rounded-lg text-sm text-yellow-800">
                선택한 액션 카테고리에 맞는 위치만 표시됩니다. 각 위치는 사용자 화면의 특정 영역에 액션을 배치합니다.
              </div>

              <select
                v-model="customActionForm.targetLocation"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                required
              >
                <option value="">위치를 선택하세요</option>
                <option
                  v-for="location in filteredLocations"
                  :key="location.key"
                  :value="location.key"
                >
                  {{ location.displayName }} - {{ location.page }}
                </option>
              </select>

              <p v-if="customActionForm.targetLocation" class="mt-2 text-sm text-gray-600">
                {{ getLocationDescription(customActionForm.targetLocation) }}
              </p>
            </div>

            <!-- 설정 JSON -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                설정 (JSON)
                <button
                  type="button"
                  @click="validateJson"
                  class="ml-2 text-blue-600 hover:text-blue-700 text-xs"
                >
                  ✓ JSON 검증
                </button>
              </label>
              <textarea
                v-model="customActionForm.configJson"
                rows="8"
                placeholder='{"text": "버튼 텍스트", "style": "primary"}'
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono text-sm"
              ></textarea>
              <p v-if="jsonValidationError" class="mt-1 text-sm text-red-600">
                ❌ {{ jsonValidationError }}
              </p>
              <p v-else-if="jsonValidationSuccess" class="mt-1 text-sm text-green-600">
                ✓ 유효한 JSON 형식입니다
              </p>
            </div>

            <!-- 추가 옵션 -->
            <div class="space-y-4">
              <h3 class="text-lg font-semibold text-gray-900">추가 옵션</h3>

              <!-- 경로 -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">
                  연결 경로
                </label>
                <input
                  v-model="customActionForm.mapsTo"
                  type="text"
                  placeholder="/feature/custom-page"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <p class="mt-1 text-xs text-gray-500">클릭 시 이동할 경로 (선택사항)</p>
              </div>

              <!-- 마감일 -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">
                  마감일
                </label>
                <input
                  v-model="customActionForm.deadline"
                  type="datetime-local"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              <!-- 정렬 순서 -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">
                  정렬 순서
                </label>
                <input
                  v-model.number="customActionForm.orderNum"
                  type="number"
                  min="0"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <p class="mt-1 text-xs text-gray-500">숫자가 작을수록 먼저 표시됩니다</p>
              </div>

              <!-- 활성화 -->
              <div class="flex items-center">
                <input
                  v-model="customActionForm.isActive"
                  type="checkbox"
                  id="isActive"
                  class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                />
                <label for="isActive" class="ml-2 text-sm font-medium text-gray-700">
                  즉시 활성화
                </label>
              </div>
            </div>

            <!-- 버튼 -->
            <div class="sticky bottom-0 bg-white border-t pt-4 flex justify-end gap-3">
              <button
                type="button"
                @click="showCustomModal = false"
                class="px-6 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
              >
                취소
              </button>
              <button
                type="submit"
                :disabled="creatingAction"
                class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {{ creatingAction ? '생성 중...' : '액션 생성' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import apiClient from '@/services/api'
import { ACTION_CATEGORIES, getActionCategory } from '@/schemas/actionCategories'
import { TARGET_LOCATIONS, getAllowedLocationsForCategory, getTargetLocation } from '@/schemas/targetLocations'

const router = useRouter()
const route = useRoute()

const loading = ref(true)
const error = ref('')
const actions = ref([])
const availableTemplates = ref([])
const showTemplateModal = ref(false)
const showCustomModal = ref(false)
const showLocationGuide = ref(false)
const creatingAction = ref(false)
const jsonValidationError = ref('')
const jsonValidationSuccess = ref(false)

const conventionId = route.params.conventionId

// Schemas
const actionCategories = ACTION_CATEGORIES

// 커스텀 액션 폼
const customActionForm = ref({
  actionType: '',
  title: '',
  actionCategory: '',
  targetLocation: '',
  configJson: '',
  mapsTo: '',
  deadline: '',
  orderNum: actions.value.length,
  isActive: true
})

// 선택한 카테고리의 가이드
const selectedCategoryGuide = computed(() => {
  if (!customActionForm.value.actionCategory) return null
  const category = getActionCategory(customActionForm.value.actionCategory)
  return category?.guide || null
})

// 선택한 카테고리에 맞는 타겟 위치 필터링
const filteredLocations = computed(() => {
  if (!customActionForm.value.actionCategory) return []
  return getAllowedLocationsForCategory(customActionForm.value.actionCategory)
})

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
    const response = await apiClient.put(`/admin/action-management/actions/${action.id}/toggle`)
    action.isActive = response.data.isActive
  } catch (err) {
    console.error('Failed to toggle action:', err)
    alert('상태 변경 실패: ' + (err.response?.data?.message || err.message))
  }
}

async function deleteAction(actionId) {
  if (!confirm('이 액션을 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/admin/action-management/actions/${actionId}`)
    actions.value = actions.value.filter(a => a.id !== actionId)
  } catch (err) {
    console.error('Failed to delete action:', err)
    alert('삭제 실패: ' + (err.response?.data?.message || err.message))
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

// ==========================================
// 커스텀 액션 생성 관련 함수들
// ==========================================

function selectCategory(category) {
  customActionForm.value.actionCategory = category.key
  // 카테고리 변경 시 타겟 위치 초기화
  customActionForm.value.targetLocation = ''
  // JSON 가이드 예시를 자동으로 채워넣기 (선택사항)
  if (!customActionForm.value.configJson && category.guide?.example) {
    customActionForm.value.configJson = category.guide.example
  }
}

function getLocationDescription(locationKey) {
  const location = getTargetLocation(locationKey)
  return location?.description || ''
}

function validateJson() {
  jsonValidationError.value = ''
  jsonValidationSuccess.value = false

  if (!customActionForm.value.configJson) {
    jsonValidationError.value = 'JSON을 입력해주세요'
    return false
  }

  try {
    JSON.parse(customActionForm.value.configJson)
    jsonValidationSuccess.value = true
    return true
  } catch (error) {
    jsonValidationError.value = `JSON 형식이 올바르지 않습니다: ${error.message}`
    return false
  }
}

async function copyGuideExample() {
  if (!selectedCategoryGuide.value?.example) return

  try {
    await navigator.clipboard.writeText(selectedCategoryGuide.value.example)
    alert('예시가 클립보드에 복사되었습니다!')
  } catch (err) {
    console.error('복사 실패:', err)
    // 폴백: 수동으로 텍스트 영역에 복사
    customActionForm.value.configJson = selectedCategoryGuide.value.example
    alert('설정 JSON 필드에 예시를 붙여넣었습니다')
  }
}

async function createCustomAction() {
  // JSON 검증
  if (!validateJson()) {
    return
  }

  creatingAction.value = true

  try {
    const payload = {
      conventionId: parseInt(conventionId),
      actionType: customActionForm.value.actionType,
      title: customActionForm.value.title,
      actionCategory: customActionForm.value.actionCategory,
      targetLocation: customActionForm.value.targetLocation,
      configJson: customActionForm.value.configJson,
      mapsTo: customActionForm.value.mapsTo || '/',
      deadline: customActionForm.value.deadline || null,
      orderNum: customActionForm.value.orderNum,
      isActive: customActionForm.value.isActive
    }

    await apiClient.post('/admin/action-management/actions', payload)

    alert('커스텀 액션이 생성되었습니다!')
    showCustomModal.value = false
    resetForm()
    await fetchData()
  } catch (err) {
    console.error('액션 생성 실패:', err)
    alert('액션 생성 실패: ' + (err.response?.data?.message || err.message))
  } finally {
    creatingAction.value = false
  }
}

function resetForm() {
  customActionForm.value = {
    actionType: '',
    title: '',
    actionCategory: '',
    targetLocation: '',
    configJson: '',
    mapsTo: '',
    deadline: '',
    orderNum: actions.value.length,
    isActive: true
  }
  jsonValidationError.value = ''
  jsonValidationSuccess.value = false
  showLocationGuide.value = false
}

function openCustomModal() {
  resetForm()
  showCustomModal.value = true
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
