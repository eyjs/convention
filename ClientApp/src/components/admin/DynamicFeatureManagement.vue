<template>
  <div class="space-y-6">
    <!-- 헤더 -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-2xl font-bold text-gray-900">동적 기능 관리</h2>
        <p class="text-sm text-gray-600 mt-1">
          페이지별로 버튼, 배너, 팝업 등 동적 기능을 추가하고 관리합니다
        </p>
      </div>
      <button
        @click="openCreateModal"
        class="flex items-center space-x-2 px-4 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg transition-colors"
      >
        <svg
          class="w-5 h-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M12 4v16m8-8H4"
          />
        </svg>
        <span>기능 추가</span>
      </button>
    </div>

    <!-- 로딩 -->
    <div v-if="loading" class="text-center py-12">
      <div
        class="inline-block w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"
      ></div>
    </div>

    <!-- 기능 목록 (페이지별 그룹) -->
    <div v-else-if="features.length > 0" class="space-y-6">
      <div
        v-for="(group, page) in groupedFeatures"
        :key="page"
        class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden"
      >
        <!-- 페이지 헤더 -->
        <div class="bg-gray-50 px-5 py-3 border-b border-gray-200">
          <h3 class="text-lg font-bold text-gray-900">{{ page }}</h3>
          <p class="text-xs text-gray-600 mt-0.5">{{ group.length }}개 기능</p>
        </div>

        <!-- 기능 목록 -->
        <div class="divide-y divide-gray-200">
          <div
            v-for="feature in group"
            :key="feature.id"
            class="p-5 hover:bg-gray-50 transition-colors"
          >
            <div class="flex items-start justify-between">
              <div class="flex-1">
                <div class="flex items-center space-x-3 mb-2">
                  <h4 class="text-base font-bold text-gray-900">
                    {{ feature.actionName }}
                  </h4>

                  <!-- Category 배지 -->
                  <span
                    :class="getCategoryBadgeClass(feature.actionCategory)"
                    class="px-2 py-1 text-xs font-medium rounded"
                  >
                    {{ getCategoryDisplayName(feature.actionCategory) }}
                  </span>

                  <!-- 활성화 상태 -->
                  <span
                    class="px-2 py-1 text-xs font-medium rounded-full"
                    :class="
                      feature.isActive
                        ? 'bg-green-100 text-green-700'
                        : 'bg-gray-100 text-gray-600'
                    "
                  >
                    {{ feature.isActive ? '활성' : '비활성' }}
                  </span>

                  <!-- 순서 -->
                  <span
                    class="px-2 py-1 bg-blue-100 text-blue-700 text-xs font-medium rounded-full"
                  >
                    순서: {{ feature.orderNum }}
                  </span>
                </div>

                <div class="space-y-2 text-sm">
                  <!-- 위치 -->
                  <div class="flex items-center text-gray-600">
                    <svg
                      class="w-4 h-4 mr-2"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z"
                      />
                    </svg>
                    <span class="font-medium">{{
                      getLocationDisplayName(feature.targetLocation)
                    }}</span>
                  </div>

                  <!-- 설정 미리보기 -->
                  <div
                    v-if="feature.config"
                    class="flex items-start text-gray-600"
                  >
                    <svg
                      class="w-4 h-4 mr-2 mt-0.5"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"
                      />
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                      />
                    </svg>
                    <div class="flex-1">
                      <button
                        @click="toggleConfigPreview(feature.id)"
                        class="text-primary-600 hover:text-primary-700 text-xs font-medium"
                      >
                        {{
                          expandedConfigs.includes(feature.id)
                            ? '설정 숨기기'
                            : '설정 보기'
                        }}
                      </button>
                      <pre
                        v-if="expandedConfigs.includes(feature.id)"
                        class="text-xs bg-gray-100 p-2 rounded overflow-auto max-w-md mt-1"
                        >{{ formatConfig(feature.config) }}</pre
                      >
                    </div>
                  </div>
                </div>
              </div>

              <!-- 액션 버튼 -->
              <div class="flex items-center space-x-2 ml-4">
                <button
                  @click="toggleFeature(feature)"
                  :title="feature.isActive ? '비활성화' : '활성화'"
                  class="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                >
                  <svg
                    v-if="feature.isActive"
                    class="w-5 h-5 text-green-600"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                  </svg>
                  <svg
                    v-else
                    class="w-5 h-5 text-gray-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                  </svg>
                </button>

                <button
                  @click="openEditModal(feature)"
                  class="p-2 hover:bg-blue-50 text-blue-600 rounded-lg transition-colors"
                  title="수정"
                >
                  <svg
                    class="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                    />
                  </svg>
                </button>

                <button
                  @click="deleteFeature(feature)"
                  class="p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors"
                  title="삭제"
                >
                  <svg
                    class="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                    />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 빈 상태 -->
    <div
      v-else
      class="text-center py-12 bg-white rounded-lg border-2 border-dashed border-gray-300"
    >
      <svg
        class="w-16 h-16 mx-auto text-gray-400 mb-4"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zm0 0h12a2 2 0 002-2v-4a2 2 0 00-2-2h-2.343M11 7.343l1.657-1.657a2 2 0 012.828 0l2.829 2.829a2 2 0 010 2.828l-8.486 8.485M7 17h.01"
        />
      </svg>
      <h3 class="text-lg font-medium text-gray-900 mb-2">
        등록된 기능이 없습니다
      </h3>
      <p class="text-gray-600 mb-4">
        버튼, 배너, 팝업 등 동적 기능을 추가해보세요
      </p>
      <button
        @click="openCreateModal"
        class="px-4 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg transition-colors"
      >
        첫 기능 만들기
      </button>
    </div>

    <!-- 생성/수정 모달 -->
    <div
      v-if="showModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
    >
      <div
        class="bg-white rounded-xl w-full max-w-3xl max-h-[90vh] overflow-y-auto"
      >
        <div
          class="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between"
        >
          <h3 class="text-lg font-bold text-gray-900">
            {{ editingFeature ? '기능 수정' : '새 기능 추가' }}
          </h3>
          <button
            @click="closeModal"
            class="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <svg
              class="w-5 h-5 text-gray-500"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </div>

        <form @submit.prevent="saveFeature" class="p-6 space-y-5">
          <!-- Action Category (드롭다운) -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              기능 유형 *
              <span class="text-xs font-normal text-gray-500">(The What)</span>
            </label>
            <select
              v-model="form.actionCategory"
              @change="handleCategoryChange"
              required
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            >
              <option value="">-- 선택하세요 --</option>
              <option
                v-for="category in categoryOptions"
                :key="category.value"
                :value="category.value"
              >
                {{ category.label }}
              </option>
            </select>
            <p v-if="selectedCategory" class="text-xs text-gray-600 mt-1">
              {{ selectedCategory.description }}
            </p>
          </div>

          <!-- Target Location (드롭다운, category에 따라 필터링) -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              표시 위치 *
              <span class="text-xs font-normal text-gray-500">(The Where)</span>
            </label>
            <select
              v-model="form.targetLocation"
              required
              :disabled="!form.actionCategory"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
            >
              <option value="">-- 선택하세요 --</option>
              <option
                v-for="location in filteredLocationOptions"
                :key="location.value"
                :value="location.value"
              >
                {{ location.label }}
              </option>
            </select>
            <p v-if="selectedLocation" class="text-xs text-gray-600 mt-1">
              <strong>{{ selectedLocation.page }}</strong
              >: {{ selectedLocation.description }}
            </p>
            <p
              v-else-if="
                form.actionCategory && filteredLocationOptions.length === 0
              "
              class="text-xs text-red-600 mt-1"
            >
              이 유형은 사용 가능한 위치가 없습니다.
            </p>
          </div>

          <!-- Action Name (제목) -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              제목 *
            </label>
            <input
              v-model="form.actionName"
              type="text"
              required
              placeholder="예: 행사 안내 배너, 설문조사 버튼"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
          </div>

          <!-- Config (JSON) -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              설정 (JSON) *
            </label>
            <textarea
              v-model="form.config"
              rows="8"
              required
              :placeholder="getConfigPlaceholder()"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent font-mono text-sm"
            ></textarea>
            <div
              class="mt-2 p-3 bg-blue-50 border border-blue-200 rounded text-xs text-blue-800"
            >
              <strong>설정 예시:</strong>
              <pre class="mt-1 overflow-auto">{{ getConfigExample() }}</pre>
            </div>
          </div>

          <!-- 정렬 순서 -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              정렬 순서
            </label>
            <input
              v-model.number="form.orderNum"
              type="number"
              min="0"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
            <p class="text-xs text-gray-500 mt-1">
              작은 숫자가 먼저 표시됩니다
            </p>
          </div>

          <!-- 활성화 -->
          <div class="flex items-center">
            <input
              v-model="form.isActive"
              type="checkbox"
              id="isActive"
              class="w-4 h-4 text-primary-600 rounded focus:ring-2 focus:ring-primary-500"
            />
            <label for="isActive" class="ml-2 text-sm text-gray-700">
              활성화 (사용자에게 표시)
            </label>
          </div>

          <!-- 에러 메시지 -->
          <div
            v-if="errorMessage"
            class="p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-800"
          >
            {{ errorMessage }}
          </div>

          <!-- 버튼 -->
          <div class="flex space-x-3 pt-4">
            <button
              type="button"
              @click="closeModal"
              class="flex-1 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
            >
              취소
            </button>
            <button
              type="submit"
              :disabled="submitting"
              class="flex-1 px-4 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg transition-colors disabled:opacity-50"
            >
              {{ submitting ? '저장 중...' : '저장' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/api'
import {
  ACTION_CATEGORIES,
  getActionCategoryOptions,
  getActionCategory,
} from '@/schemas/actionCategories'
import {
  TARGET_LOCATIONS,
  getTargetLocationOptions,
  getAllowedLocationsForCategory,
  getTargetLocation,
} from '@/schemas/targetLocations'

const props = defineProps({
  conventionId: {
    type: Number,
    required: true,
  },
})

const features = ref([])
const loading = ref(false)
const showModal = ref(false)
const editingFeature = ref(null)
const submitting = ref(false)
const errorMessage = ref('')
const expandedConfigs = ref([])

const form = ref({
  actionCategory: '',
  targetLocation: '',
  actionName: '',
  config: '',
  orderNum: 0,
  isActive: true,
})

// Schema options
const categoryOptions = getActionCategoryOptions()
const allLocationOptions = getTargetLocationOptions()

// Selected category/location 정보
const selectedCategory = computed(() => {
  return form.value.actionCategory
    ? getActionCategory(form.value.actionCategory)
    : null
})

const selectedLocation = computed(() => {
  return form.value.targetLocation
    ? getTargetLocation(form.value.targetLocation)
    : null
})

// Location options filtered by selected category
const filteredLocationOptions = computed(() => {
  if (!form.value.actionCategory) return []

  const allowedLocations = getAllowedLocationsForCategory(
    form.value.actionCategory,
  )
  return allowedLocations.map((loc) => ({
    value: loc.key,
    label: loc.displayName,
    description: loc.description,
    page: loc.page,
  }))
})

// 페이지별로 기능 그룹화
const groupedFeatures = computed(() => {
  const grouped = {}

  features.value.forEach((feature) => {
    const location = getTargetLocation(feature.targetLocation)
    const page = location?.page || 'Unknown Page'

    if (!grouped[page]) {
      grouped[page] = []
    }
    grouped[page].push(feature)
  })

  // 각 그룹을 orderNum으로 정렬
  Object.keys(grouped).forEach((page) => {
    grouped[page].sort((a, b) => a.orderNum - b.orderNum)
  })

  return grouped
})

async function loadFeatures() {
  loading.value = true
  try {
    const response = await apiClient.get(
      `/conventions/${props.conventionId}/actions`,
    )
    // actionCategory와 targetLocation이 있는 것만 (동적 기능)
    features.value = (response.data || []).filter(
      (f) => f.actionCategory && f.targetLocation,
    )
  } catch (error) {
    console.error('Failed to load features:', error)
    features.value = []
  } finally {
    loading.value = false
  }
}

function openCreateModal() {
  editingFeature.value = null
  form.value = {
    actionCategory: '',
    targetLocation: '',
    actionName: '',
    config: '',
    orderNum: features.value.length,
    isActive: true,
  }
  showModal.value = true
  errorMessage.value = ''
}

function openEditModal(feature) {
  editingFeature.value = feature
  form.value = {
    actionCategory: feature.actionCategory,
    targetLocation: feature.targetLocation,
    actionName: feature.actionName,
    config:
      typeof feature.config === 'string'
        ? feature.config
        : JSON.stringify(feature.config, null, 2),
    orderNum: feature.orderNum,
    isActive: feature.isActive,
  }
  showModal.value = true
  errorMessage.value = ''
}

function closeModal() {
  showModal.value = false
  editingFeature.value = null
  errorMessage.value = ''
}

function handleCategoryChange() {
  // Category 변경 시 location 초기화
  form.value.targetLocation = ''
}

async function saveFeature() {
  errorMessage.value = ''
  submitting.value = true

  try {
    // JSON validation
    let configObj
    try {
      configObj = JSON.parse(form.value.config)
    } catch (e) {
      throw new Error('설정 JSON 형식이 올바르지 않습니다: ' + e.message)
    }

    const payload = {
      conventionId: props.conventionId,
      actionCategory: form.value.actionCategory,
      targetLocation: form.value.targetLocation,
      actionName: form.value.actionName,
      config: form.value.config,
      orderNum: form.value.orderNum,
      isActive: form.value.isActive,
      // actionType은 자동 생성 (category_timestamp 같은 형식)
      actionType:
        editingFeature.value?.actionType ||
        `${form.value.actionCategory}_${Date.now()}`,
      mapsTo: 'DYNAMIC_FEATURE', // 동적 기능 표시
    }

    if (editingFeature.value) {
      await apiClient.put(
        `/conventions/${props.conventionId}/actions/${editingFeature.value.id}`,
        payload,
      )
    } else {
      await apiClient.post(
        `/conventions/${props.conventionId}/actions`,
        payload,
      )
    }

    closeModal()
    await loadFeatures()
  } catch (error) {
    errorMessage.value =
      error.message || error.response?.data?.message || '저장에 실패했습니다.'
  } finally {
    submitting.value = false
  }
}

async function toggleFeature(feature) {
  try {
    await apiClient.put(
      `/conventions/${props.conventionId}/actions/${feature.id}`,
      {
        ...feature,
        isActive: !feature.isActive,
      },
    )
    feature.isActive = !feature.isActive
  } catch (error) {
    alert('상태 변경 실패: ' + error.message)
  }
}

async function deleteFeature(feature) {
  if (!confirm(`"${feature.actionName}" 기능을 삭제하시겠습니까?`)) return

  try {
    await apiClient.delete(
      `/conventions/${props.conventionId}/actions/${feature.id}`,
    )
    await loadFeatures()
  } catch (error) {
    alert('삭제 실패: ' + error.message)
  }
}

function toggleConfigPreview(featureId) {
  const index = expandedConfigs.value.indexOf(featureId)
  if (index > -1) {
    expandedConfigs.value.splice(index, 1)
  } else {
    expandedConfigs.value.push(featureId)
  }
}

function getCategoryDisplayName(key) {
  const category = ACTION_CATEGORIES.find((c) => c.key === key)
  return category?.displayName || key
}

function getLocationDisplayName(key) {
  const location = TARGET_LOCATIONS.find((l) => l.key === key)
  return location?.displayName || key
}

function getCategoryBadgeClass(key) {
  const classes = {
    BUTTON: 'bg-blue-100 text-blue-700',
    MENU: 'bg-purple-100 text-purple-700',
    AUTO_POPUP: 'bg-pink-100 text-pink-700',
    BANNER: 'bg-yellow-100 text-yellow-700',
    CARD: 'bg-green-100 text-green-700',
  }
  return classes[key] || 'bg-gray-100 text-gray-700'
}

function formatConfig(config) {
  try {
    const obj = typeof config === 'string' ? JSON.parse(config) : config
    return JSON.stringify(obj, null, 2)
  } catch (e) {
    return config
  }
}

function getConfigPlaceholder() {
  if (!form.value.actionCategory) return '{"key": "value"}'

  const examples = {
    BUTTON: '{\n  "style": "primary",\n  "size": "md",\n  "url": "/path"\n}',
    MENU: '{\n  "icon": "<svg>...</svg>",\n  "description": "설명",\n  "url": "/path"\n}',
    AUTO_POPUP: '{\n  "trigger": "onPageLoad",\n  "message": "내용"\n}',
    BANNER: '{\n  "imageUrl": "https://...",\n  "height": "md"\n}',
    CARD: '{\n  "description": "설명",\n  "variant": "info"\n}',
  }

  return examples[form.value.actionCategory] || '{"key": "value"}'
}

function getConfigExample() {
  if (!form.value.actionCategory) return '{}'

  const examples = {
    BUTTON: {
      style: 'primary',
      size: 'md',
      url: '/my-schedule',
      icon: '<svg>...</svg>',
    },
    MENU: {
      icon: '<svg>...</svg>',
      description: '상세 설명',
      url: '/feature/custom',
      iconColor: '#FFFFFF',
      bgColor: '#3B82F6',
    },
    AUTO_POPUP: {
      trigger: 'onPageLoad',
      delay: 1000,
      message: '환영합니다!',
      size: 'md',
      showOnce: true,
    },
    BANNER: {
      imageUrl: 'https://example.com/banner.jpg',
      height: 'md',
      overlayText: '행사 안내',
      url: '/notices',
    },
    CARD: {
      description: '카드 설명 텍스트',
      variant: 'info',
      icon: '<svg>...</svg>',
      badge: 'NEW',
    },
  }

  return JSON.stringify(examples[form.value.actionCategory] || {}, null, 2)
}

onMounted(() => {
  loadFeatures()
})
</script>
