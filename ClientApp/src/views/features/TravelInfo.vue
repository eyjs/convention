<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 헤더 -->
    <header class="bg-white shadow-sm sticky top-0 z-10">
      <div class="flex items-center px-4 py-4">
        <button 
          @click="router.back()"
          class="p-2 -ml-2 hover:bg-gray-100 rounded-lg transition-colors"
        >
          <svg class="w-6 h-6 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <h1 class="ml-2 text-xl font-bold text-gray-900">여행 서류 제출</h1>
      </div>
    </header>

    <!-- 메인 컨텐츠 -->
    <div class="max-w-2xl mx-auto px-4 py-6">
      <!-- 안내 카드 -->
      <div class="bg-blue-50 border border-blue-200 rounded-xl p-4 mb-6">
        <div class="flex items-start space-x-3">
          <svg class="w-6 h-6 text-blue-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div class="flex-1">
            <h3 class="font-semibold text-blue-900 mb-1">필수 제출 서류</h3>
            <p class="text-sm text-blue-800">해외 행사 참석을 위해 여권 정보와 비자 서류를 제출해주세요.</p>
          </div>
        </div>
      </div>

      <!-- 폼 -->
      <form @submit.prevent="handleSubmit" class="bg-white rounded-xl shadow-lg p-6 space-y-6">
        <!-- 영문 이름 -->
        <div>
          <label class="block text-sm font-semibold text-gray-700 mb-2">
            영문 이름 (여권상 표기) *
          </label>
          <input
            v-model="form.englishName"
            type="text"
            required
            placeholder="예: HONG GILDONG"
            class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          />
          <p class="text-xs text-gray-500 mt-1">여권에 표기된 영문 이름을 대문자로 입력해주세요</p>
        </div>

        <!-- 여권 번호 -->
        <div>
          <label class="block text-sm font-semibold text-gray-700 mb-2">
            여권 번호 *
          </label>
          <input
            v-model="form.passportNumber"
            type="text"
            required
            placeholder="예: M12345678"
            class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          />
        </div>

        <!-- 여권 만료일 -->
        <div>
          <label class="block text-sm font-semibold text-gray-700 mb-2">
            여권 만료일 *
          </label>
          <input
            v-model="form.passportExpiryDate"
            type="date"
            required
            class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          />
          <p class="text-xs text-gray-500 mt-1">여행 종료일로부터 최소 6개월 이상 유효해야 합니다</p>
        </div>

        <!-- 비자 서류 업로드 -->
        <div>
          <label class="block text-sm font-semibold text-gray-700 mb-2">
            비자 서류 (이미지 또는 PDF)
          </label>
          
          <!-- 파일 업로드 영역 -->
          <div 
            @click="triggerFileInput"
            @drop.prevent="handleDrop"
            @dragover.prevent
            class="border-2 border-dashed border-gray-300 rounded-xl p-6 text-center cursor-pointer hover:border-primary-500 hover:bg-primary-50 transition-colors"
            :class="{ 'border-primary-500 bg-primary-50': isDragging }"
          >
            <input
              ref="fileInput"
              type="file"
              accept="image/*,.pdf"
              @change="handleFileSelect"
              class="hidden"
            />
            
            <div v-if="!uploadedFile">
              <svg class="w-12 h-12 mx-auto text-gray-400 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
              </svg>
              <p class="text-gray-700 font-medium mb-1">클릭하거나 파일을 드래그하세요</p>
              <p class="text-sm text-gray-500">이미지 또는 PDF 파일 (최대 10MB)</p>
            </div>

            <div v-else class="flex items-center justify-between">
              <div class="flex items-center space-x-3">
                <div class="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                  <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div class="text-left">
                  <p class="font-medium text-gray-900">{{ uploadedFile.name }}</p>
                  <p class="text-sm text-gray-500">{{ formatFileSize(uploadedFile.size) }}</p>
                </div>
              </div>
              <button
                type="button"
                @click.stop="removeFile"
                class="p-2 hover:bg-red-50 rounded-lg transition-colors"
              >
                <svg class="w-5 h-5 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>

          <!-- 업로드 진행 -->
          <div v-if="uploading" class="mt-3">
            <div class="flex items-center justify-between text-sm text-gray-600 mb-1">
              <span>업로드 중...</span>
              <span>{{ uploadProgress }}%</span>
            </div>
            <div class="h-2 bg-gray-200 rounded-full overflow-hidden">
              <div 
                class="h-full bg-primary-600 transition-all duration-300"
                :style="{ width: `${uploadProgress}%` }"
              ></div>
            </div>
          </div>
        </div>

        <!-- 에러 메시지 -->
        <div v-if="errorMessage" class="p-4 bg-red-50 border border-red-200 rounded-xl">
          <div class="flex items-start space-x-3">
            <svg class="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <p class="text-sm text-red-800">{{ errorMessage }}</p>
          </div>
        </div>

        <!-- 버튼 -->
        <div class="flex space-x-3 pt-4">
          <button
            type="button"
            @click="router.back()"
            class="flex-1 px-6 py-3 border-2 border-gray-300 text-gray-700 font-semibold rounded-xl hover:bg-gray-50 transition-colors"
          >
            취소
          </button>
          <button
            type="submit"
            :disabled="submitting || !isFormValid"
            class="flex-1 px-6 py-3 bg-primary-600 hover:bg-primary-700 text-white font-semibold rounded-xl transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ submitting ? '제출 중...' : '제출하기' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/api'

const router = useRouter()
const authStore = useAuthStore()

const form = ref({
  englishName: '',
  passportNumber: '',
  passportExpiryDate: '',
  visaDocumentAttachmentId: null
})

const fileInput = ref(null)
const uploadedFile = ref(null)
const uploading = ref(false)
const uploadProgress = ref(0)
const submitting = ref(false)
const errorMessage = ref('')
const isDragging = ref(false)

const isFormValid = computed(() => {
  return form.value.englishName && 
         form.value.passportNumber && 
         form.value.passportExpiryDate
})

function triggerFileInput() {
  fileInput.value?.click()
}

async function handleFileSelect(event) {
  const file = event.target.files?.[0]
  if (file) {
    await uploadFile(file)
  }
}

function handleDrop(event) {
  isDragging.value = false
  const file = event.dataTransfer.files?.[0]
  if (file) {
    uploadFile(file)
  }
}

async function uploadFile(file) {
  // 파일 크기 체크 (10MB)
  if (file.size > 10 * 1024 * 1024) {
    errorMessage.value = '파일 크기는 10MB 이하여야 합니다.'
    return
  }

  // 파일 타입 체크
  const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf']
  if (!allowedTypes.includes(file.type)) {
    errorMessage.value = '이미지(JPG, PNG) 또는 PDF 파일만 업로드 가능합니다.'
    return
  }

  uploading.value = true
  uploadProgress.value = 0
  errorMessage.value = ''

  try {
    const formData = new FormData()
    formData.append('files', file)
    formData.append('category', 'visa')

    // 진행률 시뮬레이션 (실제로는 axios의 onUploadProgress 사용)
    const progressInterval = setInterval(() => {
      if (uploadProgress.value < 90) {
        uploadProgress.value += 10
      }
    }, 200)

    const response = await apiClient.post('/file/upload/files', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })

    clearInterval(progressInterval)
    uploadProgress.value = 100

    if (response.data && response.data.length > 0) {
      form.value.visaDocumentAttachmentId = response.data[0].id
      uploadedFile.value = {
        name: file.name,
        size: file.size,
        id: response.data[0].id
      }
    }
  } catch (error) {
    console.error('File upload failed:', error)
    errorMessage.value = '파일 업로드에 실패했습니다. 다시 시도해주세요.'
  } finally {
    uploading.value = false
  }
}

function removeFile() {
  uploadedFile.value = null
  form.value.visaDocumentAttachmentId = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

function formatFileSize(bytes) {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
}

async function handleSubmit() {
  if (!isFormValid.value) return

  submitting.value = true
  errorMessage.value = ''

  try {
    // DateOnly 형식으로 변환 (YYYY-MM-DD)
    const formattedDate = form.value.passportExpiryDate

    await apiClient.put('/guest/my-travel-info', {
      englishName: form.value.englishName,
      passportNumber: form.value.passportNumber,
      passportExpiryDate: formattedDate,
      visaDocumentAttachmentId: form.value.visaDocumentAttachmentId
    })

    // 제출 성공 - authStore 새로고침
    await authStore.fetchCurrentUser()

    alert('여행 정보가 성공적으로 제출되었습니다!')
    router.push('/')
  } catch (error) {
    console.error('Travel info submission failed:', error)
    errorMessage.value = error.response?.data?.message || '제출에 실패했습니다. 다시 시도해주세요.'
  } finally {
    submitting.value = false
  }
}
</script>
