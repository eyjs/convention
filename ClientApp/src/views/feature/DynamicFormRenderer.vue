<template>
  <div class="min-h-screen bg-gray-50 py-8 px-4">
    <div class="max-w-2xl mx-auto">
      <!-- 로딩 상태 -->
      <div v-if="loading" class="text-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-4 text-gray-600">로딩 중...</p>
      </div>

      <!-- 에러 상태 -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
        <p class="text-red-600">{{ error }}</p>
        <button @click="router.back()" class="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700">
          돌아가기
        </button>
      </div>

      <!-- 폼 렌더링 -->
      <div v-else-if="formDefinition" class="bg-white rounded-lg shadow-lg p-8">
        <!-- 헤더 -->
        <div class="mb-8">
          <h1 class="text-2xl font-bold text-gray-900 mb-2">{{ formDefinition.name }}</h1>
          <p v-if="formDefinition.description" class="text-gray-600">{{ formDefinition.description }}</p>
        </div>

        <!-- 동적 폼 필드 -->
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <div v-for="field in sortedFields" :key="field.id" class="form-group">
            <label :for="`field-${field.id}`" class="block text-sm font-medium text-gray-700 mb-2">
              {{ field.label }}
              <span v-if="field.isRequired" class="text-red-500">*</span>
            </label>

            <!-- Text Input -->
            <input
              v-if="field.fieldType === 'text' || field.fieldType === 'email' || field.fieldType === 'tel'"
              :id="`field-${field.id}`"
              v-model="formData[field.key]"
              :type="field.fieldType"
              :placeholder="field.placeholder || ''"
              :required="field.isRequired"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />

            <!-- Textarea -->
            <textarea
              v-else-if="field.fieldType === 'textarea'"
              :id="`field-${field.id}`"
              v-model="formData[field.key]"
              :placeholder="field.placeholder || ''"
              :required="field.isRequired"
              rows="4"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            ></textarea>

            <!-- Number -->
            <input
              v-else-if="field.fieldType === 'number'"
              :id="`field-${field.id}`"
              v-model.number="formData[field.key]"
              type="number"
              :placeholder="field.placeholder || ''"
              :required="field.isRequired"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />

            <!-- Date -->
            <input
              v-else-if="field.fieldType === 'date'"
              :id="`field-${field.id}`"
              v-model="formData[field.key]"
              type="date"
              :required="field.isRequired"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />

            <!-- Select -->
            <select
              v-else-if="field.fieldType === 'select'"
              :id="`field-${field.id}`"
              v-model="formData[field.key]"
              :required="field.isRequired"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">선택하세요</option>
              <option v-for="option in parseOptions(field.optionsJson)" :key="option.value" :value="option.value">
                {{ option.label }}
              </option>
            </select>

            <!-- Radio -->
            <div v-else-if="field.fieldType === 'radio'" class="space-y-2">
              <label v-for="option in parseOptions(field.optionsJson)" :key="option.value" class="flex items-center">
                <input
                  type="radio"
                  :name="`field-${field.id}`"
                  v-model="formData[field.key]"
                  :value="option.value"
                  :required="field.isRequired"
                  class="mr-2"
                />
                <span>{{ option.label }}</span>
              </label>
            </div>

            <!-- Checkbox -->
            <div v-else-if="field.fieldType === 'checkbox'" class="flex items-center">
              <input
                :id="`field-${field.id}`"
                type="checkbox"
                v-model="formData[field.key]"
                class="mr-2 w-4 h-4 text-blue-600"
              />
            </div>

            <!-- File Upload -->
            <input
              v-else-if="field.fieldType === 'file'"
              :id="`field-${field.id}`"
              type="file"
              @change="handleFileChange($event, field.key)"
              :required="field.isRequired"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
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
              {{ submitting ? '제출 중...' : (isEditing ? '수정 완료' : '제출하기') }}
            </button>
          </div>
        </form>

        <!-- 성공 메시지 -->
        <div v-if="successMessage" class="mt-4 p-4 bg-green-50 border border-green-200 rounded-lg text-green-700">
          {{ successMessage }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/api'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const formDefinitionId = computed(() => parseInt(route.params.formDefinitionId))

const loading = ref(true)
const error = ref(null)
const formDefinition = ref(null)
const formData = ref({})
const isEditing = ref(false)
const submitting = ref(false)
const successMessage = ref('')

// 필드를 OrderIndex로 정렬
const sortedFields = computed(() => {
  if (!formDefinition.value?.fields) return []
  return [...formDefinition.value.fields].sort((a, b) => a.orderIndex - b.orderIndex)
})

// Options JSON 파싱
function parseOptions(optionsJson) {
  if (!optionsJson) return []
  try {
    return JSON.parse(optionsJson)
  } catch (e) {
    console.error('Options JSON 파싱 실패:', e)
    return []
  }
}

// 파일 변경 핸들러
function handleFileChange(event, key) {
  const file = event.target.files[0]
  if (file) {
    formData.value[key] = file
  }
}

// 폼 정의 로드
async function loadFormDefinition() {
  try {
    const response = await apiClient.get(`/forms/${formDefinitionId.value}/definition`)
    formDefinition.value = response.data

    // 폼 데이터 초기화
    if (formDefinition.value.fields) {
      formDefinition.value.fields.forEach(field => {
        if (field.fieldType === 'checkbox') {
          formData.value[field.key] = false
        } else {
          formData.value[field.key] = ''
        }
      })
    }
  } catch (err) {
    console.error('폼 정의 로드 실패:', err)
    error.value = '폼 정의를 불러올 수 없습니다.'
  }
}

// 기존 제출 데이터 로드 (있는 경우)
async function loadExistingSubmission() {
  try {
    const response = await apiClient.get(`/forms/submission/${formDefinitionId.value}`)

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
    await apiClient.post(`/forms/${formDefinitionId.value}/submit`, formData.value)

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
  await loadFormDefinition()
  if (!error.value) {
    await loadExistingSubmission()
  }
  loading.value = false
})
</script>

<style scoped>
/* 추가 스타일이 필요한 경우 여기에 작성 */
</style>
