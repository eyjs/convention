<template>
  <div>
    <!-- 헤더 -->
    <div class="flex justify-between items-center mb-6">
      <div>
        <h2 class="text-xl sm:text-2xl font-bold text-gray-900">
          참여자 액션 관리
        </h2>
        <p class="text-sm text-gray-600 mt-1">
          참석자가 완료해야 하는 필수 액션을 관리합니다
        </p>
      </div>
      <button
        @click="openCreateModal"
        class="px-4 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg transition-colors"
      >
        액션 추가
      </button>
    </div>

    <!-- BehaviorType 필터 버튼 -->
    <div class="mb-6 flex flex-wrap gap-2">
      <button
        @click="selectedBehaviorType = 'All'"
        :class="[
          'px-4 py-2 rounded-lg text-sm font-medium transition-colors',
          selectedBehaviorType === 'All'
            ? 'bg-primary-600 text-white'
            : 'bg-gray-200 text-gray-700 hover:bg-gray-300',
        ]"
      >
        전체
      </button>
      <button
        @click="selectedBehaviorType = 'StatusOnly'"
        :class="[
          'px-4 py-2 rounded-lg text-sm font-medium transition-colors',
          selectedBehaviorType === 'StatusOnly'
            ? 'bg-primary-600 text-white'
            : 'bg-gray-200 text-gray-700 hover:bg-gray-300',
        ]"
      >
        단순 완료
      </button>
      <button
        @click="selectedBehaviorType = 'FormBuilder'"
        :class="[
          'px-4 py-2 rounded-lg text-sm font-medium transition-colors',
          selectedBehaviorType === 'FormBuilder'
            ? 'bg-primary-600 text-white'
            : 'bg-gray-200 text-gray-700 hover:bg-gray-300',
        ]"
      >
        폼 빌더
      </button>
      <button
        @click="selectedBehaviorType = 'ModuleLink'"
        :class="[
          'px-4 py-2 rounded-lg text-sm font-medium transition-colors',
          selectedBehaviorType === 'ModuleLink'
            ? 'bg-primary-600 text-white'
            : 'bg-gray-200 text-gray-700 hover:bg-gray-300',
        ]"
      >
        모듈 연동
      </button>
      <button
        @click="selectedBehaviorType = 'Link'"
        :class="[
          'px-4 py-2 rounded-lg text-sm font-medium transition-colors',
          selectedBehaviorType === 'Link'
            ? 'bg-primary-600 text-white'
            : 'bg-gray-200 text-gray-700 hover:bg-gray-300',
        ]"
      >
        링크
      </button>
    </div>

    <!-- 로딩 -->
    <div v-if="loading" class="text-center py-12 mt-6">
      <div
        class="inline-block w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"
      ></div>
    </div>

    <!-- 액션 목록 -->
    <div v-else-if="filteredActions.length > 0" class="grid gap-4 mt-6">
      <div
        v-for="action in filteredActions"
        :key="action.id"
        class="bg-white rounded-lg shadow-sm border border-gray-200 p-4 sm:p-5 hover:shadow-md transition-shadow overflow-hidden"
      >
        <div
          class="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-4"
        >
          <div class="flex-1 min-w-0">
            <div class="flex flex-wrap items-center gap-2 mb-2">
              <h3
                class="text-base sm:text-lg font-bold text-gray-900 break-words"
              >
                {{ action.title }}
              </h3>
              <span
                class="px-2 py-1 text-xs font-medium rounded-full flex-shrink-0"
                :class="
                  action.isActive
                    ? 'bg-green-100 text-green-700'
                    : 'bg-gray-100 text-gray-600'
                "
                >{{ action.isActive ? '활성' : '비활성' }}</span
              >
              <span
                class="px-2 py-1 bg-blue-100 text-blue-700 text-xs font-medium rounded-full flex-shrink-0"
                >순서: {{ action.orderNum }}</span
              >
            </div>

            <div class="space-y-2 text-sm">
              <div class="flex items-center text-gray-600 flex-wrap gap-2">
                <svg
                  class="w-4 h-4 flex-shrink-0"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"
                  />
                </svg>
                <span class="font-mono text-xs bg-gray-100 px-2 py-1 rounded">{{
                  getBehaviorTypeName(action.behaviorType)
                }}</span>
                <template
                  v-if="
                    action.behaviorType === 'FormBuilder' && action.targetId
                  "
                >
                  <span class="text-gray-400">|</span>
                  <span class="text-xs"
                    >Form ID:
                    <strong class="font-semibold text-gray-700">{{
                      action.targetId
                    }}</strong></span
                  >
                </template>
              </div>
              <div
                v-if="action.deadline"
                class="flex items-center text-gray-600 flex-wrap gap-2"
              >
                <svg
                  class="w-4 h-4 flex-shrink-0"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
                <span>마감: {{ formatDateTime(action.deadline) }}</span>
              </div>
            </div>
          </div>

          <div class="flex items-center gap-2 flex-shrink-0">
            <button
              @click="toggleAction(action)"
              :title="action.isActive ? '비활성화' : '활성화'"
              class="p-2 hover:bg-gray-100 rounded-lg transition-colors flex-shrink-0"
            >
              <svg
                v-if="action.isActive"
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
                /></svg
              ><svg
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
              @click="openEditModal(action)"
              class="p-2 hover:bg-blue-50 text-blue-600 rounded-lg transition-colors flex-shrink-0"
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
              @click="deleteAction(action)"
              class="p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors flex-shrink-0"
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

    <div
      v-else
      class="text-center py-12 bg-white rounded-lg border-2 border-dashed border-gray-300 mt-6"
    >
      <h3 class="text-lg font-medium text-gray-900 mb-2">
        등록된 액션이 없습니다
      </h3>
      <p class="text-gray-600 mb-4">참석자가 완료해야 할 액션을 추가해보세요</p>
      <button
        @click="openCreateModal"
        class="px-4 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg transition-colors"
      >
        첫 액션 만들기
      </button>
    </div>

    <BaseModal :is-open="showModal" @close="closeModal" max-width="lg">
      <template #header>
        <h3 class="text-lg font-bold text-gray-900">
          {{ editingAction ? '액션 수정' : '새 액션 추가' }}
        </h3>
      </template>

      <template #body>
        <div class="space-y-6">
          <div
            v-if="editingAction && editingAction.behaviorType === 'GenericForm'"
            class="p-3 bg-yellow-50 border border-yellow-200 rounded-lg text-sm text-yellow-800"
          >
            이 액션은 새로운 '폼 빌더' 타입으로 자동 전환되었습니다. 저장 시 '폼
            빌더' 타입으로 업데이트됩니다.
          </div>

          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >제목 (참석자에게 표시) *</label
            >
            <input
              v-model="form.title"
              type="text"
              required
              class="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>

          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >실행 방식 (BehaviorType) *</label
            >
            <select
              v-model="form.behaviorType"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm"
            >
              <option
                v-for="type in behaviorTypes"
                :key="type.value"
                :value="type.value"
              >
                {{ type.label }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >액션 카테고리 *</label
            >
            <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
              <div
                v-for="category in actionCategories"
                :key="category.key"
                @click="selectCategory(category)"
                :class="[
                  'p-4 border-2 rounded-lg cursor-pointer transition-all',
                  form.actionCategory === category.key
                    ? 'border-primary-600 bg-primary-50'
                    : 'border-gray-200 hover:border-blue-300',
                ]"
              >
                <div class="font-semibold text-sm">
                  {{ category.displayName }}
                </div>
                <div class="text-xs text-gray-600 mt-1">
                  {{ category.description }}
                </div>
              </div>
            </div>
          </div>

          <div v-if="form.actionCategory">
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >표시 위치 *</label
            >
            <select
              v-model="form.targetLocation"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg"
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
          </div>

          <div
            v-if="form.behaviorType === 'FormBuilder'"
            class="space-y-4 p-4 border border-gray-200 rounded-lg"
          >
            <h4 class="font-medium text-gray-800">폼 빌더 설정</h4>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2"
                >연결할 폼 *</label
              >
              <select
                v-model="form.targetId"
                required
                class="w-full px-4 py-2 border border-gray-300 rounded-lg text-sm"
              >
                <option :value="null" disabled>
                  폼 빌더에서 생성한 폼을 선택하세요
                </option>
                <option
                  v-for="formDef in formDefinitions"
                  :key="formDef.id"
                  :value="formDef.id"
                >
                  {{ formDef.name }} (ID: {{ formDef.id }})
                </option>
              </select>
            </div>
          </div>

          <div
            v-if="form.behaviorType === 'ModuleLink'"
            class="space-y-4 p-4 border border-gray-200 rounded-lg"
          >
            <h4 class="font-medium text-gray-800">모듈 연동 설정</h4>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2"
                >연결할 URL *</label
              >
              <div class="flex items-center">
                <span
                  class="px-4 py-2 bg-gray-100 border border-r-0 border-gray-300 rounded-l-lg text-gray-600 font-mono text-sm whitespace-nowrap"
                  >/feature/</span
                >
                <input
                  v-model="form.mapsTo"
                  @input="stripFeaturePrefix"
                  type="text"
                  required
                  placeholder="surveys/2"
                  class="flex-1 px-4 py-2 border border-gray-300 rounded-r-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>
              <p class="text-xs text-gray-500 mt-1">
                모듈의 경로만 입력하세요. (예: surveys/2, board/3)
              </p>
            </div>
          </div>

          <div
            v-if="form.behaviorType === 'Link'"
            class="space-y-4 p-4 border border-gray-200 rounded-lg"
          >
            <h4 class="font-medium text-gray-800">링크 설정</h4>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2"
                >연결할 URL (MapsTo) *</label
              >
              <input
                v-model="form.mapsTo"
                type="text"
                required
                placeholder="https://example.com 또는 /internal/path"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg"
              />
            </div>
          </div>

          <div
            v-if="form.behaviorType === 'ShowComponentPopup'"
            class="space-y-4 p-4 border border-gray-200 rounded-lg"
          >
            <h4 class="font-medium text-gray-800">컴포넌트 팝업 설정</h4>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2"
                >팝업 컴포넌트 이름 (MapsTo) *</label
              >
              <input
                v-model="form.mapsTo"
                type="text"
                required
                placeholder="예: MyInfoComponent"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg"
              />
              <p class="text-xs text-gray-500 mt-1">
                `ClientApp/src/popups/popupComponents.js`에 등록된 컴포넌트 이름
              </p>
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2"
                >컴포넌트에 전달할 ID (TargetId)</label
              >
              <input
                v-model.number="form.targetId"
                type="number"
                placeholder="예: 123 (선택 사항)"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg"
              />
              <p class="text-xs text-gray-500 mt-1">
                팝업 컴포넌트의 `props.id`로 전달됩니다.
              </p>
            </div>
          </div>

          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >마감일</label
            >
            <input
              v-model="form.deadline"
              type="datetime-local"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>

          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >정렬 순서</label
            >
            <input
              v-model.number="form.orderNum"
              type="number"
              min="0"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg"
            />
          </div>

          <div class="flex items-center">
            <input
              v-model="form.isActive"
              type="checkbox"
              id="isActive"
              class="w-4 h-4 text-primary-600 rounded"
            />
            <label for="isActive" class="ml-2 text-sm text-gray-700"
              >활성화</label
            >
          </div>

          <div
            v-if="errorMessage"
            class="p-3 bg-red-50 text-red-800 rounded-lg text-sm"
          >
            {{ errorMessage }}
          </div>
        </div>
      </template>

      <template #footer>
        <button
          type="button"
          @click="closeModal"
          class="px-4 py-2 border rounded-lg"
        >
          취소
        </button>
        <button
          @click="saveAction"
          :disabled="submitting"
          class="px-4 py-2 bg-primary-600 text-white rounded-lg disabled:opacity-50"
        >
          {{ submitting ? '저장 중...' : '저장' }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import apiClient from '@/services/api'
import formBuilderService from '@/services/formBuilderService'
import BaseModal from '@/components/common/BaseModal.vue'
import {
  ACTION_CATEGORIES,
  getActionCategory,
} from '@/schemas/actionCategories'
import { getAllowedLocationsForCategory } from '@/schemas/targetLocations'

const props = defineProps({
  conventionId: {
    type: Number,
    required: true,
  },
})

const actions = ref([])
const loading = ref(false)
const showModal = ref(false)
const editingAction = ref(null)
const submitting = ref(false)
const errorMessage = ref('')
const formDefinitions = ref([])

const actionCategories = ACTION_CATEGORIES
const behaviorTypes = [
  { value: 'StatusOnly', label: '단순 완료 처리' },
  { value: 'FormBuilder', label: '폼 빌더' },
  { value: 'ModuleLink', label: '모듈 연동' },
  { value: 'Link', label: '링크' },
  { value: 'ShowComponentPopup', label: '컴포넌트 팝업' }, // ShowComponentPopup 추가
]

const getInitialFormState = () => ({
  title: '',
  actionCategory: '',
  targetLocation: '',
  mapsTo: '',
  deadline: '',
  orderNum: actions.value.length,
  isActive: true,
  behaviorType: 'StatusOnly',
  targetId: null, // FormBuilder용
})

const form = ref(getInitialFormState())

const filteredLocations = computed(() => {
  if (!form.value.actionCategory) return []
  return getAllowedLocationsForCategory(form.value.actionCategory)
})

function selectCategory(category) {
  form.value.actionCategory = category.key
  form.value.targetLocation = ''
}

async function loadFormDefinitions() {
  if (!props.conventionId) {
    errorMessage.value = '컨벤션 ID가 없어 폼 목록을 불러올 수 없습니다.'
    return
  }
  try {
    const response = await formBuilderService.getFormDefinitions(
      props.conventionId,
    )
    console.log('API Response for Form Definitions:', response)
    formDefinitions.value = response.data || []
  } catch (error) {
    console.error('Failed to load form definitions:', error)
    errorMessage.value = '폼 빌더 목록을 불러오는 데 실패했습니다.'
  }
}

async function loadActions() {
  loading.value = true
  try {
    const response = await apiClient.get(
      `/admin/action-management/convention/${props.conventionId}`,
    )
    actions.value = response.data || []
  } catch (error) {
    console.error('Failed to load actions:', error)
  } finally {
    loading.value = false
  }
}

async function openCreateModal() {
  editingAction.value = null
  form.value = getInitialFormState()
  await loadFormDefinitions()
  showModal.value = true
  errorMessage.value = ''
}

async function openEditModal(action) {
  editingAction.value = action
  let behaviorType = action.behaviorType
  if (behaviorType === 'GenericForm') {
    behaviorType = 'FormBuilder'
  }

  let formTargetId = null
  // action.targetId가 null이 아니고 undefined가 아니면 parseInt 적용
  if (action.targetId !== null && action.targetId !== undefined) {
    formTargetId = parseInt(action.targetId, 10)
    // parseInt 결과가 NaN이면 null로 처리 (예: 빈 문자열이 넘어온 경우)
    if (isNaN(formTargetId)) {
      formTargetId = null
    }
  }
  // 하위 호환성: GenericForm에서 마이그레이션 된 경우 targetModuleId를 사용
  // 이 로직은 FormBuilder 타입에만 적용되며, action.targetId가 null/undefined일 때만 실행
  else if (
    behaviorType === 'FormBuilder' &&
    action.targetModuleId !== null &&
    action.targetModuleId !== undefined
  ) {
    formTargetId = parseInt(action.targetModuleId, 10)
    if (isNaN(formTargetId)) {
      formTargetId = null
    }
  }

  // mapsTo에서 /feature/ prefix 제거 (ModuleLink 타입인 경우)
  let cleanedMapsTo = action.mapsTo || ''
  if (behaviorType === 'ModuleLink' && cleanedMapsTo.startsWith('/feature/')) {
    cleanedMapsTo = cleanedMapsTo.substring(9) // '/feature/' 제거
  }

  form.value = {
    title: action.title,
    actionCategory: action.actionCategory || '',
    targetLocation: action.targetLocation || '',
    mapsTo: cleanedMapsTo,
    deadline: action.deadline ? formatDateTimeForInput(action.deadline) : '',
    orderNum: action.orderNum,
    isActive: action.isActive,
    behaviorType: behaviorType,
    targetId: formTargetId,
  }
  await loadFormDefinitions()
  showModal.value = true
  errorMessage.value = ''
}

function stripFeaturePrefix(event) {
  const value = event.target.value
  // /feature/, /feature, feature/ 등 다양한 형태 처리
  if (value.startsWith('/feature/')) {
    form.value.mapsTo = value.substring(9)
  } else if (value.startsWith('feature/')) {
    form.value.mapsTo = value.substring(8)
  } else if (value.startsWith('/feature')) {
    form.value.mapsTo = value.substring(8)
  }
}

function closeModal() {
  showModal.value = false
  editingAction.value = null
  errorMessage.value = ''
}

async function saveAction() {
  errorMessage.value = ''

  // 기본 유효성 검사
  if (!form.value.title) {
    errorMessage.value = '제목을 입력해주세요.'
    return
  }
  if (!form.value.actionCategory) {
    errorMessage.value = '액션 카테고리를 선택해주세요.'
    return
  }
  if (!form.value.targetLocation) {
    errorMessage.value = '표시 위치를 선택해주세요.'
    return
  }

  // BehaviorType별 추가 유효성 검사
  switch (form.value.behaviorType) {
    case 'FormBuilder':
      // targetId가 null이거나 undefined, 또는 NaN이면 유효하지 않음 (0은 유효)
      if (
        form.value.targetId === null ||
        form.value.targetId === undefined ||
        isNaN(form.value.targetId)
      ) {
        errorMessage.value = '연결할 폼을 선택해주세요.'
        return
      }
      break
    case 'ModuleLink':
    case 'Link':
    case 'ShowComponentPopup':
      if (!form.value.mapsTo) {
        errorMessage.value = '연결할 URL 또는 컴포넌트 이름을 입력해주세요.'
        return
      }
      break
  }

  submitting.value = true
  try {
    const payload = {
      conventionId: props.conventionId,
      title: form.value.title,
      actionCategory: form.value.actionCategory,
      targetLocation: form.value.targetLocation,
      deadline: form.value.deadline || null,
      orderNum: form.value.orderNum,
      isActive: form.value.isActive,
      behaviorType: form.value.behaviorType,
      mapsTo: null, // 기본값 null
      targetId: null, // 기본값 null
    }

    // BehaviorType에 따라 mapsTo와 targetId 설정
    switch (form.value.behaviorType) {
      case 'FormBuilder':
        // form.value.targetId가 0도 유효한 값으로 처리
        payload.targetId =
          form.value.targetId !== null &&
          form.value.targetId !== undefined &&
          !isNaN(form.value.targetId)
            ? parseInt(form.value.targetId, 10)
            : null
        break
      case 'ModuleLink':
        // ModuleLink는 /feature/ prefix를 추가
        payload.mapsTo = '/feature/' + form.value.mapsTo
        break
      case 'Link':
      case 'ShowComponentPopup': // ShowComponentPopup도 mapsTo를 사용
        payload.mapsTo = form.value.mapsTo
        // ShowComponentPopup은 targetId도 사용
        if (form.value.behaviorType === 'ShowComponentPopup') {
          payload.targetId =
            form.value.targetId !== null &&
            form.value.targetId !== undefined &&
            !isNaN(form.value.targetId)
              ? parseInt(form.value.targetId, 10)
              : null
        }
        break
      // StatusOnly는 mapsTo, targetId 필요 없음
    }

    if (editingAction.value) {
      await apiClient.put(
        `/admin/action-management/actions/${editingAction.value.id}`,
        payload,
      )
    } else {
      await apiClient.post('/admin/action-management/actions', payload)
    }
    closeModal()
    await loadActions()
  } catch (error) {
    errorMessage.value = error.response?.data?.message || '저장에 실패했습니다.'
  } finally {
    submitting.value = false
  }
}

async function toggleAction(action) {
  try {
    await apiClient.put(`/admin/action-management/actions/${action.id}/toggle`)
    action.isActive = !action.isActive
  } catch (error) {
    alert('상태 변경 실패: ' + error.message)
  }
}

async function deleteAction(action) {
  if (!confirm(`"${action.title}" 액션을 삭제하시겠습니까?`)) return
  try {
    await apiClient.delete(`/admin/action-management/actions/${action.id}`)
    await loadActions()
  } catch (error) {
    alert('삭제 실패: ' + error.message)
  }
}

function formatDateTime(dateString) {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleString('ko-KR')
}

function formatDateTimeForInput(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  return `${year}-${month}-${day}T${hours}:${minutes}`
}

const selectedBehaviorType = ref('All')
const filteredActions = computed(() => {
  if (selectedBehaviorType.value === 'All') {
    return actions.value
  }
  if (selectedBehaviorType.value === 'FormBuilder') {
    return actions.value.filter(
      (a) =>
        a.behaviorType === 'FormBuilder' || a.behaviorType === 'GenericForm',
    )
  }
  return actions.value.filter(
    (a) => a.behaviorType === selectedBehaviorType.value,
  )
})

function getBehaviorTypeName(type) {
  switch (type) {
    case 'StatusOnly':
      return '단순 완료'
    case 'FormBuilder':
      return '폼 빌더'
    case 'GenericForm':
      return '폼 빌더 (구)'
    case 'ModuleLink':
      return '모듈 연동'
    case 'Link':
      return '링크'
    case 'ShowComponentPopup':
      return '컴포넌트 팝업' // ShowComponentPopup 추가
    default:
      return type
  }
}

onMounted(() => {
  loadActions()
})
</script>
