<template>
  <div class="space-y-6">
    <!-- 헤더 -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-2xl font-bold text-gray-900">참여자 액션 관리</h2>
        <p class="text-sm text-gray-600 mt-1">
          참석자가 완료해야 하는 필수 액션을 관리합니다
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
        <span>액션 추가</span>
      </button>
    </div>

    <!-- 로딩 -->
    <div v-if="loading" class="text-center py-12">
      <div
        class="inline-block w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"
      ></div>
    </div>

    <!-- 액션 목록 -->
    <div v-else-if="actions.length > 0" class="grid gap-4">
      <div
        v-for="action in actions"
        :key="action.id"
        class="bg-white rounded-lg shadow-sm border border-gray-200 p-5 hover:shadow-md transition-shadow"
      >
        <div class="flex items-start justify-between">
          <div class="flex-1">
            <div class="flex items-center space-x-3 mb-2">
              <h3 class="text-lg font-bold text-gray-900">
                {{ action.title }}
              </h3>

              <!-- 템플릿/전용 배지 -->
              <span
                v-if="action.templateName"
                class="px-2 py-1 bg-blue-100 text-blue-700 text-xs font-medium rounded"
                title="공통 템플릿 액션"
              >
                공통 템플릿
              </span>
              <span
                v-else
                class="px-2 py-1 bg-green-100 text-green-700 text-xs font-medium rounded"
                title="이 행사 전용 액션"
              >
                이 행사 전용
              </span>

              <span
                class="px-2 py-1 text-xs font-medium rounded-full"
                :class="
                  action.isActive
                    ? 'bg-green-100 text-green-700'
                    : 'bg-gray-100 text-gray-600'
                "
              >
                {{ action.isActive ? '활성' : '비활성' }}
              </span>
              <span
                class="px-2 py-1 bg-blue-100 text-blue-700 text-xs font-medium rounded-full"
              >
                순서: {{ action.orderNum }}
              </span>
            </div>

            <div class="space-y-2 text-sm">
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
                    d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"
                  />
                </svg>
                <span class="font-mono text-xs bg-gray-100 px-2 py-1 rounded">{{
                  getBehaviorTypeName(action.behaviorType)
                }}</span>
              </div>

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
                    d="M13 9l3 3m0 0l-3 3m3-3H8m13 0a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
                <span>{{ action.mapsTo }}</span>
              </div>

              <div
                v-if="action.deadline"
                class="flex items-center text-gray-600"
              >
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
                    d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
                <span>마감: {{ formatDateTime(action.deadline) }}</span>
              </div>

              <div
                v-if="action.configJson"
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
                <pre
                  class="text-xs bg-gray-100 p-2 rounded overflow-auto max-w-md"
                  >{{ action.configJson }}</pre
                >
              </div>
            </div>
          </div>

          <!-- 액션 버튼 -->
          <div class="flex items-center space-x-2 ml-4">
            <button
              @click="toggleAction(action)"
              :title="action.isActive ? '비활성화' : '활성화'"
              class="p-2 hover:bg-gray-100 rounded-lg transition-colors"
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
              @click="openEditModal(action)"
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
              @click="deleteAction(action)"
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
          d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
        />
      </svg>
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

    <!-- 액션 생성/수정 모달 -->
    <div
      v-if="showModal"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
    >
      <div
        class="bg-white rounded-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto"
      >
        <div
          class="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between"
        >
          <h3 class="text-lg font-bold text-gray-900">
            {{ editingAction ? '액션 수정' : '새 액션 추가' }}
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

        <form @submit.prevent="saveAction" class="p-6 space-y-4">
          <!-- 이 행사 전용 체크박스 -->
          <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <div class="flex items-start gap-3">
              <input
                v-model="form.isCustom"
                type="checkbox"
                id="isCustom"
                class="mt-1 w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
              />
              <div class="flex-1">
                <label
                  for="isCustom"
                  class="block text-sm font-semibold text-blue-900 cursor-pointer"
                >
                  이 행사 전용 액션
                </label>
                <p class="text-xs text-blue-700 mt-1">
                  체크 시: 이 행사에만 적용되는 고유 액션 (예: 1일차 투어
                  선택)<br />
                  체크 해제: 여러 행사에서 재사용 가능한 공통 액션 (예:
                  설문조사, 서류 제출)
                </p>
              </div>
            </div>
          </div>

          <!-- BehaviorType 선택 -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              실행 방식 (BehaviorType) *
            </label>
            <select
              v-model="form.behaviorType"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            >
              <option value="StatusOnly">StatusOnly - 단순 완료 처리 (기존 방식)</option>
              <option value="GenericForm">GenericForm - 범용 폼 데이터 수집</option>
              <option value="ModuleLink">ModuleLink - 공통 모듈 연동</option>
              <option value="Link">Link - 외부/내부 링크</option>
            </select>
            <p class="text-xs text-gray-500 mt-1">
              액션 클릭 시 어떻게 동작할지 선택하세요
            </p>
          </div>

          <!-- 액션 카테고리 -->
          <div class="space-y-4">
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              액션 카테고리 *
            </label>
            <div class="grid grid-cols-2 md:grid-cols-3 gap-3">
              <div
                v-for="category in actionCategories"
                :key="category.key"
                @click="selectCategory(category)"
                :class="[
                  'p-4 border-2 rounded-lg cursor-pointer transition-all',
                  form.actionCategory === category.key
                    ? 'border-blue-600 bg-blue-50'
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

          <!-- 선택한 카테고리의 가이드 -->
          <div
            v-if="selectedCategoryGuide"
            class="bg-blue-50 border border-blue-200 rounded-lg p-4"
          >
            <div class="flex items-start justify-between mb-2">
              <h4 class="font-semibold text-blue-900">
                📘 {{ selectedCategoryGuide.title }}
              </h4>
              <button
                type="button"
                @click="copyGuideExample"
                class="px-3 py-1 text-xs bg-blue-600 text-white rounded hover:bg-blue-700"
              >
                예시 복사
              </button>
            </div>
            <p class="text-sm text-blue-800 mb-3">
              {{ selectedCategoryGuide.content }}
            </p>
            <pre
              class="bg-white p-3 rounded border border-blue-200 text-xs overflow-x-auto"
              >{{ selectedCategoryGuide.example }}</pre
            >
          </div>

          <!-- 타겟 위치 -->
          <div v-if="form.actionCategory">
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              표시 위치 *
              <button
                type="button"
                @click="showLocationGuide = !showLocationGuide"
                class="ml-2 text-blue-600 hover:text-blue-700"
              >
                <svg
                  class="w-4 h-4 inline"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                >
                  <path
                    fill-rule="evenodd"
                    d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z"
                    clip-rule="evenodd"
                  />
                </svg>
              </button>
            </label>

            <!-- 위치 가이드 토글 -->
            <div
              v-if="showLocationGuide"
              class="mb-3 p-3 bg-yellow-50 border border-yellow-200 rounded-lg text-sm text-yellow-800"
            >
              선택한 액션 카테고리에 맞는 위치만 표시됩니다. 각 위치는 사용자
              화면의 특정 영역에 액션을 배치합니다.
            </div>

            <select
              v-model="form.targetLocation"
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

            <p
              v-if="form.targetLocation"
              class="mt-2 text-sm text-gray-600"
            >
              {{ getLocationDescription(form.targetLocation) }}
            </p>
          </div>

          <!-- TargetModuleId (ModuleLink 타입일 때만 표시) -->
          <div v-if="form.behaviorType === 'ModuleLink'">
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              연결할 모듈 ID (TargetModuleId) *
            </label>
            <input
              v-model.number="form.targetModuleId"
              type="number"
              required
              placeholder="예: 설문조사 ID = 1"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
            <p class="text-xs text-gray-500 mt-1">
              연결할 설문조사 또는 다른 모듈의 ID를 입력하세요
            </p>
          </div>

          <!-- ConfigJson (GenericForm 타입일 때 안내 강화) -->
          <div v-if="form.behaviorType === 'GenericForm'" class="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <h4 class="text-sm font-semibold text-blue-900 mb-2">
              GenericForm 설정 가이드
            </h4>
            <p class="text-xs text-blue-700 mb-2">
              ConfigJson에 폼 필드를 정의하세요. 예시:
            </p>
            <pre class="bg-white p-2 rounded text-xs overflow-x-auto">
{
  "fields": [
    {
      "key": "passportNo",
      "label": "여권번호",
      "type": "text",
      "required": true,
      "placeholder": "M12345678"
    },
    {
      "key": "emergencyContact",
      "label": "비상 연락처",
      "type": "tel",
      "required": true
    }
  ]
}
            </pre>
          </div>

          <!-- 제목 -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              제목 (참석자에게 표시) *
            </label>
            <input
              v-model="form.title"
              type="text"
              required
              placeholder="예: 여행 서류 제출"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
          </div>

          <!-- Vue 라우터 경로 -->
          <div v-if="form.behaviorType !== 'StatusOnly'">
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              페이지 경로 (MapsTo) *
            </label>
            <div class="flex items-center">
              <span
                class="px-3 py-2 bg-gray-100 text-gray-700 border border-r-0 border-gray-300 rounded-l-lg font-mono text-sm"
              >
                /Feature
              </span>
              <input
                v-model="form.mapsTo"
                type="text"
                required
                placeholder="travel-info"
                class="flex-1 px-4 py-2 border border-gray-300 rounded-r-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>
            <p class="text-xs text-gray-500 mt-1">
              참석자가 클릭 시 이동할 경로 (앞에 /Feature가 자동으로 붙습니다)
            </p>
          </div>

          <!-- 마감일 -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              마감일
            </label>
            <input
              v-model="form.deadline"
              type="datetime-local"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
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
          </div>

          <!-- 설정 JSON -->
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2">
              추가 설정 (JSON)
            </label>
            <textarea
              v-model="form.configJson"
              rows="4"
              placeholder='{"key": "value"}'
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent font-mono text-sm"
            ></textarea>
            <p class="text-xs text-gray-500 mt-1">
              선택 사항: 복잡한 액션에 필요한 설정을 JSON 형식으로 입력
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
              활성화 (참석자에게 표시)
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
import { ref, onMounted, computed } from 'vue'
import apiClient from '@/services/api'
import {
  ACTION_CATEGORIES,
  getActionCategory,
} from '@/schemas/actionCategories'
import {
  TARGET_LOCATIONS,
  getAllowedLocationsForCategory,
  getTargetLocation,
} from '@/schemas/targetLocations'

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
const showLocationGuide = ref(false)
const jsonValidationError = ref('')
const jsonValidationSuccess = ref(false)

// Schemas
const actionCategories = ACTION_CATEGORIES

const form = ref({
  title: '',
  actionCategory: '',
  targetLocation: '',
  mapsTo: '',
  deadline: '',
  orderNum: 0,
  configJson: '',
  isActive: true,
  isCustom: false,
  behaviorType: 'StatusOnly', // 기본값: StatusOnly
  targetModuleId: null, // ModuleLink 타입일 때 사용
})

// 선택한 카테고리의 가이드
const selectedCategoryGuide = computed(() => {
  if (!form.value.actionCategory) return null
  const category = getActionCategory(form.value.actionCategory)
  return category?.guide || null
})

// 선택한 카테고리에 맞는 타겟 위치 필터링
const filteredLocations = computed(() => {
  if (!form.value.actionCategory) return []
  return getAllowedLocationsForCategory(form.value.actionCategory)
})

function selectCategory(category) {
  form.value.actionCategory = category.key
  // 카테고리 변경 시 타겟 위치 초기화
  form.value.targetLocation = ''
  // JSON 가이드 예시를 자동으로 채워넣기 (선택사항)
  if (!form.value.configJson && category.guide?.example) {
    form.value.configJson = category.guide.example
  }
}

function getLocationDescription(locationKey) {
  const location = getTargetLocation(locationKey)
  return location?.description || ''
}

function validateJson() {
  jsonValidationError.value = ''
  jsonValidationSuccess.value = false

  if (!form.value.configJson) {
    jsonValidationError.value = 'JSON을 입력해주세요'
    return false
  }

  try {
    JSON.parse(form.value.configJson)
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
    form.value.configJson = selectedCategoryGuide.value.example
    alert('설정 JSON 필드에 예시를 붙여넣었습니다')
  }
}

async function loadActions() {
  loading.value = true
  try {
    const response = await apiClient.get(
      `/admin/action-management/convention/${props.conventionId}`,
    )
    actions.value = response.data.actions || []
  } catch (error) {
    console.error('Failed to load actions:', error)
    // 403 에러는 권한 문제, 빈 배열로 처리
    if (error.response?.status === 403) {
      actions.value = []
    } else {
      alert('액션 목록을 불러오는데 실패했습니다.')
    }
  } finally {
    loading.value = false
  }
}

function openCreateModal() {
  editingAction.value = null
  form.value = {
    title: '',
    actionCategory: '',
    targetLocation: '',
    mapsTo: '',
    deadline: '',
    orderNum: actions.value.length,
    configJson: '',
    isActive: true,
    isCustom: false,
    behaviorType: 'StatusOnly',
    targetModuleId: null,
  }
  showModal.value = true
  errorMessage.value = ''
}

function openEditModal(action) {
  editingAction.value = action
  const mapsToWithoutPrefix = action.mapsTo.startsWith('/Feature')
    ? action.mapsTo.substring(8)
    : action.mapsTo

  form.value = {
    title: action.title,
    actionCategory: action.actionCategory || '',
    targetLocation: action.targetLocation || '',
    mapsTo: mapsToWithoutPrefix,
    deadline: action.deadline ? formatDateTimeForInput(action.deadline) : '',
    orderNum: action.orderNum,
    configJson: action.configJson || '',
    isActive: action.isActive,
    isCustom: !action.templateName, // 템플릿이 없으면 전용
    behaviorType: action.behaviorType || 'StatusOnly',
    targetModuleId: action.targetModuleId || null,
  }
  showModal.value = true
  errorMessage.value = ''
}

function closeModal() {
  showModal.value = false
  editingAction.value = null
  errorMessage.value = ''
}

async function saveAction() {
  console.log('hi')
  errorMessage.value = ''
  submitting.value = true

  try {
    const payload = {
      ...form.value,
      mapsTo:
        '/Feature' +
        (form.value.mapsTo.startsWith('/') ? '' : '/') +
        form.value.mapsTo,
      conventionId: props.conventionId,
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
  const date = new Date(dateString)
  return date.toLocaleString('ko-KR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
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

function getBehaviorTypeName(type) {
  switch (type) {
    case 'StatusOnly': return '단순 완료';
    case 'GenericForm': return '범용 폼';
    case 'ModuleLink': return '모듈 연동';
    case 'Link': return '링크';
    default: return type;
  }
}

onMounted(() => {
  loadActions()
})
</script>
