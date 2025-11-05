<template>
  <div class="bg-white rounded-lg shadow p-6">
    <h2 class="text-xl font-semibold mb-4">엑셀 일괄 업로드</h2>

    <!-- 탭 메뉴 -->
    <div class="border-b border-gray-200 mb-6">
      <nav class="-mb-px flex space-x-8">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          @click="activeTab = tab.id"
          :class="[
            'whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm',
            activeTab === tab.id
              ? 'border-primary-500 text-primary-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
          ]"
        >
          {{ tab.name }}
        </button>
      </nav>
    </div>

    <!-- 참석자 업로드 탭 -->
    <div v-if="activeTab === 'guests'" class="space-y-4">
      <div class="mb-4">
        <label class="block text-sm font-medium text-gray-700 mb-2">
          엑셀 파일 선택
        </label>
        <input
          type="file"
          @change="handleFileGuests"
          accept=".xlsx"
          class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
        />
      </div>

      <div class="mb-4 p-4 bg-blue-50 rounded-md">
        <h3 class="font-medium text-blue-900 mb-2">
          📋 엑셀 형식 (참석자 업로드)
        </h3>
        <div class="text-sm text-blue-700 space-y-1">
          <p>
            <strong>컬럼 순서:</strong> 소속 | 부서 | 이름 | 사번(주민번호) |
            전화번호 | 그룹
          </p>
          <p><strong>필수:</strong> 이름, 전화번호, 그룹</p>
          <p><strong>선택:</strong> 소속, 부서, 사번(주민번호)</p>
          <p class="mt-2 text-blue-600">
            ※ 이름 + (전화번호 OR 주민번호) 매칭으로 중복 시 업데이트
          </p>
        </div>
      </div>

      <button
        @click="uploadGuests"
        :disabled="!fileGuests || uploadingGuests"
        class="w-full px-6 py-3 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:bg-gray-300 disabled:cursor-not-allowed mb-4"
      >
        {{ uploadingGuests ? '업로드 중...' : '참석자 업로드' }}
      </button>

      <UploadResult v-if="resultGuests" :result="resultGuests" />

      <div class="mt-6 pt-6 border-t">
        <h3 class="font-semibold mb-3">샘플 파일</h3>
        <a
          href="/Sample/참석자업로드_샘플.xlsx"
          download
          class="inline-block px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200"
        >
          📥 참석자 업로드 샘플
        </a>
      </div>
    </div>

    <!-- 일정 업로드 탭 -->
    <div v-if="activeTab === 'schedules'" class="space-y-4">
      <div class="mb-4">
        <label class="block text-sm font-medium text-gray-700 mb-2">
          엑셀 파일 선택
        </label>
        <input
          type="file"
          @change="handleFileSchedules"
          accept=".xlsx"
          class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
        />
      </div>

      <div class="mb-4 p-4 bg-purple-50 rounded-md">
        <h3 class="font-medium text-purple-900 mb-2">
          📋 엑셀 형식 (일정 업로드)
        </h3>
        <div class="text-sm text-purple-700 space-y-1">
          <p><strong>A열:</strong> 일정헤더 (예: 11/03(일)_조식_07:30)</p>
          <p class="ml-4 text-xs">형식: 월/일(요일)_일정명_시:분</p>
          <p><strong>B열:</strong> 상세 내용 (엑셀 내부 줄바꿈 지원)</p>
          <p class="mt-2 text-purple-600">
            ※ 업로드할 때마다 새로운 템플릿 생성 (SCHEDULE_0001, 0002, ...)
          </p>
          <p class="text-purple-600">
            ※ 과거 템플릿은 웹에서 확인 후 삭제 가능
          </p>
        </div>
      </div>

      <button
        @click="uploadSchedules"
        :disabled="!fileSchedules || uploadingSchedules"
        class="w-full px-6 py-3 bg-purple-600 text-white rounded-md hover:bg-purple-700 disabled:bg-gray-300 disabled:cursor-not-allowed mb-4"
      >
        {{ uploadingSchedules ? '업로드 중...' : '일정 업로드' }}
      </button>

      <UploadResult
        v-if="resultSchedules"
        :result="resultSchedules"
        type="schedules"
      />

      <div class="mt-6 pt-6 border-t">
        <h3 class="font-semibold mb-3">샘플 파일</h3>
        <a
          href="/Sample/일정업로드_샘플.xlsx"
          download
          class="inline-block px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200"
        >
          📥 일정 업로드 샘플
        </a>
      </div>
    </div>

    <!-- 속성 업로드 탭 -->
    <div v-if="activeTab === 'attributes'" class="space-y-4">
      <div class="mb-4">
        <label class="block text-sm font-medium text-gray-700 mb-2">
          엑셀 파일 선택
        </label>
        <input
          type="file"
          @change="handleFileAttributes"
          accept=".xlsx"
          class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
        />
      </div>

      <div class="mb-4 p-4 bg-green-50 rounded-md">
        <h3 class="font-medium text-green-900 mb-2">
          📋 엑셀 형식 (속성 업로드)
        </h3>
        <div class="text-sm text-green-700 space-y-1">
          <p><strong>A열:</strong> 이름 (필수)</p>
          <p><strong>B열:</strong> 전화번호 (필수)</p>
          <p><strong>C열 이후:</strong> 동적 속성 (헤더: 속성명, 값: 속성값)</p>
          <p class="mt-2">예시: 나이 | 성별 | 직급 | 선호음식 | ...</p>
          <p class="mt-2 text-green-600">
            ※ 참석자에게 메타정보를 추가로 붙입니다
          </p>
          <p class="text-green-600">※ 통계 정보가 생성됩니다 (속성별 분포)</p>
        </div>
      </div>

      <button
        @click="uploadAttributes"
        :disabled="!fileAttributes || uploadingAttributes"
        class="w-full px-6 py-3 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:bg-gray-300 disabled:cursor-not-allowed mb-4"
      >
        {{ uploadingAttributes ? '업로드 중...' : '속성 업로드' }}
      </button>

      <UploadResult
        v-if="resultAttributes"
        :result="resultAttributes"
        type="attributes"
      />

      <div class="mt-6 pt-6 border-t">
        <h3 class="font-semibold mb-3">파일 다운로드</h3>
        <div class="flex gap-2">
          <a
            href="/Sample/속성업로드_샘플.xlsx"
            download
            class="inline-block px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200"
          >
            📥 속성 업로드 샘플
          </a>
          <button
            @click="downloadGuests"
            class="inline-block px-4 py-2 bg-green-100 text-green-800 rounded-md hover:bg-green-200"
          >
            📥 전체 참석자 속성 다운로드
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import apiClient from '@/services/api'
import UploadResult from './UploadResult.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

// 탭 정의
const tabs = [
  { id: 'guests', name: '참석자 업로드' },
  { id: 'schedules', name: '일정 업로드' },
  { id: 'attributes', name: '속성 업로드' },
]

const activeTab = ref('guests')

// 참석자 업로드 상태
const fileGuests = ref(null)
const uploadingGuests = ref(false)
const resultGuests = ref(null)

// 일정 업로드 상태
const fileSchedules = ref(null)
const uploadingSchedules = ref(false)
const resultSchedules = ref(null)

// 속성 업로드 상태
const fileAttributes = ref(null)
const uploadingAttributes = ref(false)
const resultAttributes = ref(null)

// 파일 핸들러
const handleFileGuests = (e) => {
  fileGuests.value = e.target.files[0]
  resultGuests.value = null
}

const handleFileSchedules = (e) => {
  fileSchedules.value = e.target.files[0]
  resultSchedules.value = null
}

const handleFileAttributes = (e) => {
  fileAttributes.value = e.target.files[0]
  resultAttributes.value = null
}

// 참석자 업로드
const uploadGuests = async () => {
  if (!fileGuests.value) return

  uploadingGuests.value = true
  resultGuests.value = null

  const formData = new FormData()
  formData.append('file', fileGuests.value)

  try {
    const response = await apiClient.post(
      `/upload/conventions/${props.conventionId}/guests`,
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      },
    )

    resultGuests.value = {
      success: response.data.success,
      message: `${response.data.totalProcessed}명 처리 완료 (신규: ${response.data.usersCreated}명, 업데이트: ${response.data.usersUpdated}명)`,
      data: {
        created: response.data.usersCreated,
        updated: response.data.usersUpdated,
        total: response.data.totalProcessed,
      },
      errors: response.data.errors || [],
      warnings: response.data.warnings || [],
    }
  } catch (error) {
    resultGuests.value = {
      success: false,
      message: '업로드 실패',
      errors: [
        error.response?.data?.error ||
          error.response?.data?.message ||
          error.message,
      ],
    }
  } finally {
    uploadingGuests.value = false
  }
}

// 일정 업로드
const uploadSchedules = async () => {
  if (!fileSchedules.value) return

  uploadingSchedules.value = true
  resultSchedules.value = null

  const formData = new FormData()
  formData.append('file', fileSchedules.value)

  try {
    const response = await apiClient.post(
      `/upload/conventions/${props.conventionId}/schedule-templates`,
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      },
    )

    resultSchedules.value = {
      success: response.data.success,
      message: `${response.data.templatesCreated}개 일정 템플릿 생성됨`,
      data: {
        templates: response.data.templatesCreated,
        actions: response.data.createdActions || [],
      },
      errors: response.data.errors || [],
      warnings: response.data.warnings || [],
    }
  } catch (error) {
    resultSchedules.value = {
      success: false,
      message: '업로드 실패',
      errors: [
        error.response?.data?.error ||
          error.response?.data?.message ||
          error.message,
      ],
    }
  } finally {
    uploadingSchedules.value = false
  }
}

// 참석자 속성 다운로드
const downloadGuests = async () => {
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/guests/download`,
      {
        responseType: 'blob',
      },
    )

    // 파일 다운로드
    const blob = new Blob([response.data], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url

    // Content-Disposition 헤더에서 파일명 추출 (없으면 기본값)
    const contentDisposition = response.headers['content-disposition']
    let fileName = '참석자속성.xlsx'
    if (contentDisposition) {
      const fileNameMatch = contentDisposition.match(
        /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/,
      )
      if (fileNameMatch && fileNameMatch[1]) {
        fileName = fileNameMatch[1].replace(/['"]/g, '')
      }
    }

    link.download = fileName
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Failed to download guests:', error)
    alert('다운로드 실패: ' + (error.response?.data?.message || error.message))
  }
}

// 속성 업로드
const uploadAttributes = async () => {
  if (!fileAttributes.value) return

  uploadingAttributes.value = true
  resultAttributes.value = null

  const formData = new FormData()
  formData.append('file', fileAttributes.value)

  try {
    const response = await apiClient.post(
      `/upload/conventions/${props.conventionId}/attributes`,
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      },
    )

    resultAttributes.value = {
      success: response.data.success,
      message: `${response.data.usersProcessed}명의 속성 처리 완료 (신규: ${response.data.attributesCreated}, 업데이트: ${response.data.attributesUpdated})`,
      data: {
        usersProcessed: response.data.usersProcessed,
        attributesCreated: response.data.attributesCreated,
        attributesUpdated: response.data.attributesUpdated,
        statistics: response.data.statistics || {},
      },
      errors: response.data.errors || [],
      warnings: response.data.warnings || [],
    }
  } catch (error) {
    resultAttributes.value = {
      success: false,
      message: '업로드 실패',
      errors: [
        error.response?.data?.error ||
          error.response?.data?.message ||
          error.message,
      ],
    }
  } finally {
    uploadingAttributes.value = false
  }
}
</script>
