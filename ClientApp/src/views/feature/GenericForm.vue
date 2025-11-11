<template>
  <div class="min-h-screen bg-gray-50 py-8 px-4">
    <div class="max-w-2xl mx-auto">
      <!-- 로딩 상태 -->
      <div v-if="loading" class="text-center py-12">
        <div
          class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"
        ></div>
        <p class="mt-4 text-gray-600">로딩 중...</p>
      </div>

      <!-- 에러 상태 -->
      <div
        v-else-if="error"
        class="bg-red-50 border border-red-200 rounded-lg p-6 text-center"
      >
        <p class="text-red-600">{{ error }}</p>
        <button
          @click="router.back()"
          class="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
        >
          돌아가기
        </button>
      </div>

      <!-- 폼 렌더링 -->
      <div v-else-if="action" class="bg-white rounded-lg shadow-lg p-8">
        <!-- 헤더 -->
        <div class="mb-8">
          <h1 class="text-2xl font-bold text-gray-900 mb-2">
            {{ action.title }}
          </h1>
          <p
            v-if="action.description"
            class="text-gray-600"
            v-html="action.description"
          ></p>
          <div
            v-if="action.deadline"
            class="mt-4 flex items-center text-sm text-orange-600"
          >
            <svg
              class="w-5 h-5 mr-2"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
              ></path>
            </svg>
            마감: {{ formatDate(action.deadline) }}
          </div>
        </div>

        <!-- 동적 폼 필드 -->
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <div v-for="field in formFields" :key="field.key" class="form-group">
            <label
              :for="field.key"
              class="block text-sm font-medium text-gray-700 mb-2"
            >
              {{ field.label }}
              <span v-if="field.required" class="text-red-500">*</span>
            </label>

            <!-- Text Input -->
            <input
              v-if="
                field.type === 'text' ||
                field.type === 'email' ||
                field.type === 'tel'
              "
              :id="field.key"
              v-model="formData[field.key]"
              :type="field.type"
              :placeholder="field.placeholder"
              :required="field.required"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />

            <!-- Textarea -->
            <textarea
              v-else-if="field.type === 'textarea'"
              :id="field.key"
              v-model="formData[field.key]"
              :placeholder="field.placeholder"
              :required="field.required"
              :rows="field.rows || 4"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            ></textarea>

            <!-- Number -->
            <input
              v-else-if="field.type === 'number'"
              :id="field.key"
              v-model.number="formData[field.key]"
              type="number"
              :placeholder="field.placeholder"
              :required="field.required"
              :min="field.min"
              :max="field.max"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />

            <!-- Date -->
            <input
              v-else-if="field.type === 'date'"
              :id="field.key"
              v-model="formData[field.key]"
              type="date"
              :required="field.required"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />

            <!-- Select -->
            <select
              v-else-if="field.type === 'select'"
              :id="field.key"
              v-model="formData[field.key]"
              :required="field.required"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">선택하세요</option>
              <option
                v-for="option in field.options"
                :key="option.value"
                :value="option.value"
              >
                {{ option.label }}
              </option>
            </select>

            <!-- Radio -->
            <div v-else-if="field.type === 'radio'" class="space-y-2">
              <label
                v-for="option in field.options"
                :key="option.value"
                class="flex items-center"
              >
                <input
                  type="radio"
                  :name="field.key"
                  v-model="formData[field.key]"
                  :value="option.value"
                  :required="field.required"
                  class="mr-2"
                />
                <span>{{ option.label }}</span>
              </label>
            </div>

            <!-- Checkbox -->
            <div
              v-else-if="field.type === 'checkbox'"
              class="flex items-center"
            >
              <input
                :id="field.key"
                type="checkbox"
                v-model="formData[field.key]"
                class="mr-2 w-4 h-4 text-blue-600"
              />
              <span>{{ field.checkboxLabel || field.label }}</span>
            </div>

            <!-- 도움말 텍스트 -->
            <p v-if="field.help" class="mt-1 text-sm text-gray-500">
              {{ field.help }}
            </p>
          </div>

          <!-- 제출 버튼 -->
          <div class="flex gap-4 pt-6 border-t">
            <button
              type="button"
              @click="router.back()"
              class="flex-1 px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 font-medium"
            >
              취소
            </button>
            <button
              type="submit"
              :disabled="submitting"
              class="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium disabled:bg-gray-400 disabled:cursor-not-allowed"
            >
              {{
                submitting ? '제출 중...' : isEditing ? '수정 완료' : '제출하기'
              }}
            </button>
          </div>
        </form>

        <!-- 성공 메시지 -->
        <div
          v-if="successMessage"
          class="mt-4 p-4 bg-green-50 border border-green-200 rounded-lg text-green-700"
        >
          {{ successMessage }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConventionStore } from '@/stores/convention'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/api'

const route = useRoute()
const router = useRouter()
const conventionStore = useConventionStore()
const authStore = useAuthStore()

const actionId = computed(() => parseInt(route.params.actionId))
const conventionId = computed(() => conventionStore.currentConvention?.id)

const loading = ref(true)
const error = ref(null)
const action = ref(null)
const formFields = ref([])
const formData = ref({})
const isEditing = ref(false)
const submitting = ref(false)
const successMessage = ref('')

// 날짜 포맷팅
function formatDate(dateString) {
  const date = new Date(dateString)
  return date.toLocaleDateString('ko-KR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

// 액션 정보 로드
async function loadAction() {
  try {
    const response = await apiClient.get(
      `/conventions/${conventionId.value}/actions/${actionId.value}`,
    )
    action.value = response.data

    // ConfigJson 파싱하여 폼 필드 생성
    if (action.value.configJson) {
      try {
        const config = JSON.parse(action.value.configJson)
        formFields.value = config.fields || []

        // 폼 데이터 초기화
        formFields.value.forEach((field) => {
          formData.value[field.key] = field.type === 'checkbox' ? false : ''
        })
      } catch (e) {
        console.error('ConfigJson 파싱 실패:', e)
        error.value = '폼 설정을 불러오는 중 오류가 발생했습니다.'
      }
    } else {
      error.value = '이 액션에는 폼 설정이 없습니다.'
    }
  } catch (err) {
    console.error('액션 로드 실패:', err)
    error.value = '액션 정보를 불러올 수 없습니다.'
  }
}

// 기존 제출 데이터 로드 (있는 경우)
async function loadExistingSubmission() {
  try {
    const response = await apiClient.get(
      `/conventions/${conventionId.value}/actions/${actionId.value}/submission`,
    )

    if (response.data) {
      // 기존 데이터로 폼 채우기
      Object.assign(formData.value, response.data)
      isEditing.value = true
    }
  } catch (err) {
    // 404는 정상 (아직 제출 안 함)
    if (err.response?.status !== 404) {
      console.error('기존 데이터 로드 실패:', err)
    }
  }
}

// 폼 제출
async function handleSubmit() {
  submitting.value = true
  successMessage.value = ''

  try {
    await apiClient.post(
      `/conventions/${conventionId.value}/actions/${actionId.value}/submit`,
      formData.value,
    )

    successMessage.value = '제출이 완료되었습니다!'

    // 진척도 업데이트를 위해 사용자 정보 갱신
    await authStore.fetchCurrentUser()

    // 2초 후 이전 페이지로 이동
    setTimeout(() => {
      router.back()
    }, 2000)
  } catch (err) {
    console.error('제출 실패:', err)
    error.value = err.response?.data?.message || '제출 중 오류가 발생했습니다.'
  } finally {
    submitting.value = false
  }
}

onMounted(async () => {
  if (!conventionId.value) {
    error.value = '행사를 선택해주세요.'
    loading.value = false
    return
  }

  await loadAction()
  if (!error.value) {
    await loadExistingSubmission()
  }
  loading.value = false
})
</script>

<style scoped>
/* 추가 스타일이 필요한 경우 여기에 작성 */
</style>
