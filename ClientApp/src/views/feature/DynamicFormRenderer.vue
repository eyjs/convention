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
          class="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
          @click="router.back()"
        >
          돌아가기
        </button>
      </div>

      <!-- 폼 렌더링 -->
      <div v-else-if="formDefinition" class="bg-white rounded-lg shadow-lg p-8">
        <!-- 헤더 -->
        <div class="mb-8">
          <h1 class="text-2xl font-bold text-gray-900 mb-2">
            {{ formDefinition.name }}
          </h1>
          <p v-if="formDefinition.description" class="text-gray-600">
            {{ formDefinition.description }}
          </p>
        </div>

        <!-- 동적 폼 필드 -->
        <form class="space-y-6" @submit.prevent="handleSubmit">
          <div v-for="field in sortedFields" :key="field.id" class="form-group">
            <label
              :for="`field-${field.id}`"
              class="block text-sm font-medium text-gray-700 mb-2"
            >
              {{ field.label }}
              <span v-if="field.isRequired" class="text-red-500">*</span>
            </label>

            <!-- Text Input -->
            <input
              v-if="
                field.fieldType === 'text' ||
                field.fieldType === 'email' ||
                field.fieldType === 'tel'
              "
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
              <option
                v-for="option in parseOptions(field.optionsJson)"
                :key="option.value"
                :value="option.value"
              >
                {{ option.label }}
              </option>
            </select>

            <!-- Radio -->
            <div v-else-if="field.fieldType === 'radio'" class="space-y-2">
              <label
                v-for="option in parseOptions(field.optionsJson)"
                :key="option.value"
                class="flex items-center"
              >
                <input
                  v-model="formData[field.key]"
                  type="radio"
                  :name="`field-${field.id}`"
                  :value="option.value"
                  :required="field.isRequired"
                  class="mr-2"
                />
                <span>{{ option.label }}</span>
              </label>
            </div>

            <!-- Checkbox -->
            <div
              v-else-if="field.fieldType === 'checkbox'"
              class="flex items-center"
            >
              <input
                :id="`field-${field.id}`"
                v-model="formData[field.key]"
                type="checkbox"
                class="mr-2 w-4 h-4 text-blue-600"
              />
            </div>

            <!-- File Upload -->
            <div v-else-if="field.fieldType === 'file'" class="space-y-3">
              <!-- 파일 선택 input -->
              <input
                :id="`field-${field.id}`"
                type="file"
                :required="field.isRequired && !existingFileUrls[field.key]"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                @change="handleFileChange($event, field.key)"
              />

              <!-- 새로 선택한 파일 정보 -->
              <p v-if="uploadedFiles[field.key]" class="text-sm text-green-600">
                ✓ 선택된 파일: {{ uploadedFiles[field.key].name }} ({{
                  Math.round(uploadedFiles[field.key].size / 1024)
                }}
                KB)
              </p>

              <!-- 기존 업로드된 파일 미리보기 -->
              <div
                v-if="existingFileUrls[field.key] && !uploadedFiles[field.key]"
                class="border border-gray-200 rounded-lg p-4 bg-gray-50"
              >
                <p class="text-sm font-medium text-gray-700 mb-2">
                  업로드된 파일:
                </p>

                <!-- 이미지 파일인 경우 미리보기 -->
                <div
                  v-if="isImageFile(existingFileUrls[field.key])"
                  class="space-y-2"
                >
                  <img
                    :src="`http://localhost:5000${existingFileUrls[field.key]}`"
                    :alt="field.label"
                    class="max-w-full h-auto max-h-64 rounded-lg border border-gray-300 cursor-pointer hover:opacity-90 transition-opacity"
                    @click="openImageViewer(existingFileUrls[field.key])"
                  />
                  <div class="flex gap-2">
                    <button
                      type="button"
                      class="px-3 py-1 text-sm bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors"
                      @click="openImageViewer(existingFileUrls[field.key])"
                    >
                      🔍 크게 보기
                    </button>
                    <button
                      type="button"
                      class="px-3 py-1 text-sm bg-green-600 text-white rounded hover:bg-green-700 transition-colors"
                      @click="downloadFile(existingFileUrls[field.key])"
                    >
                      ⬇ 다운로드
                    </button>
                  </div>
                </div>

                <!-- PDF 파일인 경우 -->
                <div
                  v-else-if="isPdfFile(existingFileUrls[field.key])"
                  class="space-y-2"
                >
                  <!-- PDF 썸네일 (첫 페이지 미리보기) -->
                  <div
                    class="border border-gray-300 rounded-lg overflow-hidden bg-white"
                  >
                    <iframe
                      :src="`http://localhost:5000${existingFileUrls[field.key]}#toolbar=0&navpanes=0&scrollbar=0`"
                      class="w-full h-64 pointer-events-none"
                      title="PDF Preview"
                    ></iframe>
                  </div>
                  <div class="flex gap-2">
                    <button
                      type="button"
                      class="px-3 py-1 text-sm bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors"
                      @click="openPdfViewer(existingFileUrls[field.key])"
                    >
                      📄 PDF 보기
                    </button>
                    <button
                      type="button"
                      class="px-3 py-1 text-sm bg-green-600 text-white rounded hover:bg-green-700 transition-colors"
                      @click="downloadFile(existingFileUrls[field.key])"
                    >
                      ⬇ 다운로드
                    </button>
                  </div>
                </div>

                <!-- 일반 파일인 경우 다운로드 버튼만 -->
                <div v-else>
                  <p class="text-sm text-gray-600 mb-2">
                    📄 {{ getFileName(existingFileUrls[field.key]) }}
                  </p>
                  <button
                    type="button"
                    class="px-3 py-1 text-sm bg-green-600 text-white rounded hover:bg-green-700 transition-colors"
                    @click="downloadFile(existingFileUrls[field.key])"
                  >
                    ⬇ 다운로드
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- 제출 버튼 -->
          <div class="flex gap-4 pt-6 border-t">
            <button
              type="button"
              class="flex-1 px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 font-medium"
              @click="router.back()"
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

      <!-- 이미지 뷰어 모달 -->
      <div
        v-if="imageViewerUrl"
        class="fixed inset-0 z-50 bg-black bg-opacity-90"
      >
        <!-- 닫기 버튼 (고정 위치 - 우상단) -->
        <button
          class="fixed top-6 right-6 w-12 h-12 flex items-center justify-center bg-white rounded-full text-gray-800 hover:bg-gray-200 transition-colors shadow-2xl z-[60] font-bold text-xl"
          @click="closeImageViewer"
        >
          ✕
        </button>

        <!-- 다운로드 버튼 (고정 위치 - 우하단) -->
        <button
          type="button"
          class="fixed bottom-6 right-6 px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors shadow-2xl z-[60] font-medium flex items-center gap-2"
          @click="downloadFile(imageViewerUrl)"
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
              d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
            />
          </svg>
          다운로드
        </button>

        <!-- 이미지 (중앙 정렬) -->
        <div
          class="w-full h-full flex items-center justify-center p-20"
          @click="closeImageViewer"
        >
          <img
            :src="`http://localhost:5000${imageViewerUrl}`"
            alt="Image Viewer"
            class="max-w-full max-h-full object-contain rounded-lg shadow-2xl"
            @click.stop
          />
        </div>
      </div>

      <!-- PDF 뷰어 모달 -->
      <div
        v-if="pdfViewerUrl"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-90 p-4"
        @click="closePdfViewer"
      >
        <div
          class="relative w-full max-w-6xl h-[90vh] bg-white rounded-lg shadow-2xl overflow-hidden"
          @click.stop
        >
          <!-- 헤더 -->
          <div
            class="flex items-center justify-between p-4 border-b border-gray-200 bg-gray-50"
          >
            <div class="flex items-center gap-3">
              <span class="text-lg font-semibold text-gray-900"
                >📄 PDF 문서</span
              >
              <span class="text-sm text-gray-600">{{
                getFileName(pdfViewerUrl)
              }}</span>
            </div>
            <div class="flex items-center gap-2">
              <button
                type="button"
                class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors text-sm font-medium flex items-center gap-2"
                @click="downloadFile(pdfViewerUrl)"
              >
                <svg
                  class="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
                  />
                </svg>
                다운로드
              </button>
              <button
                type="button"
                class="w-10 h-10 flex items-center justify-center bg-gray-200 hover:bg-gray-300 rounded-lg text-gray-800 transition-colors font-bold"
                @click="closePdfViewer"
              >
                ✕
              </button>
            </div>
          </div>

          <!-- PDF Viewer -->
          <iframe
            :src="`http://localhost:5000${pdfViewerUrl}`"
            class="w-full h-[calc(90vh-4rem)] border-0"
            title="PDF Viewer"
          ></iframe>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import apiClient from '@/services/api'
import formBuilderService from '@/services/formBuilderService'

const props = defineProps({
  formDefinitionId: String, // 라우터에서 자동 주입 (params.formDefinitionId)
})

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const conventionStore = useConventionStore()

const formDefinitionId = computed(() => parseInt(props.formDefinitionId))

const loading = ref(true)
const error = ref(null)
const formDefinition = ref(null)
const formData = ref({})
const uploadedFiles = ref({}) // 새로 선택한 파일을 별도로 관리
const existingFileUrls = ref({}) // 기존 업로드된 파일 URL
const isEditing = ref(false)
const submitting = ref(false)
const successMessage = ref('')
const imageViewerUrl = ref(null) // 이미지 뷰어용
const pdfViewerUrl = ref(null) // PDF 뷰어용

// 필드를 OrderIndex로 정렬
const sortedFields = computed(() => {
  if (!formDefinition.value?.fields) return []
  return [...formDefinition.value.fields].sort(
    (a, b) => a.orderIndex - b.orderIndex,
  )
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
    // 파일을 별도로 저장 (Vue reactivity 문제 회피)
    uploadedFiles.value[key] = file
    // formData에는 파일명만 표시용으로 저장
    formData.value[key] = file.name
    console.log('파일 선택됨:', key, file.name, file.size, 'bytes')
  }
}

// 이미지 파일 여부 확인
function isImageFile(url) {
  if (!url) return false
  const imageExtensions = [
    '.jpg',
    '.jpeg',
    '.png',
    '.gif',
    '.bmp',
    '.webp',
    '.svg',
  ]
  const lowerUrl = url.toLowerCase()
  return imageExtensions.some((ext) => lowerUrl.endsWith(ext))
}

// PDF 파일 여부 확인
function isPdfFile(url) {
  if (!url) return false
  return url.toLowerCase().endsWith('.pdf')
}

// URL에서 파일명 추출
function getFileName(url) {
  if (!url) return ''
  const parts = url.split('/')
  return parts[parts.length - 1]
}

// 이미지 뷰어 열기
function openImageViewer(url) {
  imageViewerUrl.value = url
}

// 이미지 뷰어 닫기
function closeImageViewer() {
  imageViewerUrl.value = null
}

// PDF 뷰어 열기
function openPdfViewer(url) {
  pdfViewerUrl.value = url
}

// PDF 뷰어 닫기
function closePdfViewer() {
  pdfViewerUrl.value = null
}

// 파일 다운로드 함수
function downloadFile(url) {
  const link = document.createElement('a')
  link.href = `http://localhost:5000/api/files/download?path=${encodeURIComponent(url)}`
  link.download = getFileName(url)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

// 폼 정의 로드
async function loadFormDefinition() {
  try {
    const response = await formBuilderService.getFormDefinition(
      formDefinitionId.value,
    )
    formDefinition.value = response.data

    // 폼 데이터 초기화
    if (formDefinition.value.fields) {
      formDefinition.value.fields.forEach((field) => {
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
    const response = await apiClient.get(
      `/forms/submission/${formDefinitionId.value}`,
    )

    if (response.data) {
      // 기존 데이터로 폼 채우기 (폼 정의에 있는 필드만 할당)
      formDefinition.value.fields.forEach((field) => {
        const value = response.data[field.key]
        if (value !== undefined) {
          // 파일 필드인 경우 URL을 별도로 저장
          if (
            field.fieldType === 'file' &&
            value &&
            typeof value === 'string' &&
            value.startsWith('/')
          ) {
            existingFileUrls.value[field.key] = value
            formData.value[field.key] = '' // input은 비워둠 (보안상 설정 불가)
          } else {
            formData.value[field.key] = value
          }
        }
      })
      isEditing.value = true
    }
  } catch (err) {
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
    const submitFormData = new FormData()

    // 일반 텍스트 필드와 파일 필드를 FormData에 추가
    const plainFormData = {}
    let fileKey = null // 파일 필드의 키를 저장할 변수

    // formDefinition의 필드 목록을 기반으로 plainFormData를 구성
    for (const field of formDefinition.value.fields) {
      const key = field.key

      // 파일 필드인 경우
      if (field.fieldType === 'file') {
        // 새 파일을 선택한 경우
        if (uploadedFiles.value[key]) {
          const file = uploadedFiles.value[key]
          console.log('파일 추가 중:', key, file.name, file.size, 'bytes')
          submitFormData.append('File', file, file.name)
          fileKey = key
          plainFormData[key] = null // 백엔드에서 새 URL로 채울 것임
        }
        // 기존 파일이 있고 새 파일을 선택하지 않은 경우
        else if (existingFileUrls.value[key]) {
          plainFormData[key] = existingFileUrls.value[key] // 기존 URL 유지
        }
        // 파일이 없는 경우
        else {
          plainFormData[key] = null
        }
      } else {
        // 일반 필드는 formData에서 가져오기
        plainFormData[key] = formData.value[key]
      }
    }

    // 파일 필드의 키가 있다면, 백엔드에서 해당 키를 찾아 URL로 대체할 수 있도록 힌트를 제공
    if (fileKey) {
      submitFormData.append('FileFieldKey', fileKey) // 대문자로 시작하도록 수정
    }

    // 일반 폼 데이터를 JSON 문자열로 변환하여 'FormDataJson' 필드로 추가 (백엔드 DTO와 일치)
    submitFormData.append('FormDataJson', JSON.stringify(plainFormData))

    // FormData 내용 디버깅
    console.log('=== FormData 내용 ===')
    for (const pair of submitFormData.entries()) {
      if (pair[1] instanceof File) {
        console.log(
          `${pair[0]}: [File] ${pair[1].name} (${pair[1].size} bytes, ${pair[1].type})`,
        )
      } else {
        console.log(`${pair[0]}: ${pair[1]}`)
      }
    }
    console.log('===================')

    // FormData 전송 시 Content-Type을 multipart/form-data로 명시
    await apiClient.post(
      `/forms/${formDefinitionId.value}/submit`,
      submitFormData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      },
    )

    successMessage.value = '제출이 완료되었습니다!'

    await authStore.fetchCurrentUser()

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

async function initForm() {
  loading.value = true
  error.value = null

  if (!conventionStore.currentConvention) {
    await conventionStore.selectConvention(parseInt(route.params.conventionId))
  }

  await loadFormDefinition()
  if (!error.value) {
    await loadExistingSubmission()
  }
  loading.value = false
}

onMounted(() => {
  initForm()
})

// Watch for route changes (when navigating between different forms)
watch(
  () => props.formDefinitionId,
  (newId, oldId) => {
    if (newId && newId !== oldId) {
      initForm()
    }
  },
)
</script>

<style scoped>
/* 추가 스타일이 필요한 경우 여기에 작성 */
</style>
