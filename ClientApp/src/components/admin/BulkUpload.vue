<template>
  <div class="bg-white rounded-lg shadow p-6">
    <h2 class="text-xl font-semibold mb-4">엑셀 일괄 업로드</h2>
    
    <div class="mb-4">
      <label class="block text-sm font-medium text-gray-700 mb-2">
        엑셀 파일 선택
      </label>
      <input
        type="file"
        @change="handleFile"
        accept=".xlsx"
        class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
      />
    </div>

    <div class="mb-4 p-4 bg-blue-50 rounded-md">
      <h3 class="font-medium text-blue-900 mb-2">📋 엑셀 형식</h3>
      <div class="text-sm text-blue-700 space-y-1">
        <p><strong>Sheet1:</strong> 참석자 정보 및 일정표</p>
        <p class="ml-4">A열: 구분 (STAFF 등)</p>
        <p class="ml-4">B열: 부서</p>
        <p class="ml-4">C열: 성명</p>
        <p class="ml-4">D열: HR ID</p>
        <p class="ml-4">E열: 전화번호</p>
        <p class="ml-4">F열~: 일정 헤더 (11/03(월) [1호차] 객실에 집결 ~ 07:30)</p>
        <p class="ml-4 mt-1">데이터: "O" 표시 또는 추가 메모</p>
        <p class="mt-2"><strong>Sheet2:</strong> 일정 상세 설명</p>
        <p class="ml-4">A열: 일정명</p>
        <p class="ml-4">B열: 상세 내용</p>
      </div>
    </div>

    <button
      @click="upload"
      :disabled="!file || uploading"
      class="w-full px-6 py-3 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:bg-gray-300 disabled:cursor-not-allowed mb-4"
    >
      {{ uploading ? '업로드 중...' : '업로드' }}
    </button>

    <div v-if="uploadResult" class="mt-4">
      <div :class="[
        'p-4 rounded-md',
        uploadResult.success ? 'bg-green-50' : 'bg-red-50'
      ]">
        <p class="font-medium" :class="uploadResult.success ? 'text-green-800' : 'text-red-800'">
          {{ uploadResult.message }}
        </p>
        <div v-if="uploadResult.success" class="mt-2 text-sm text-green-700 space-y-1">
          <p>✓ 일정 템플릿: {{ uploadResult.totalSchedules }}개</p>
          <p>✓ 참석자 생성: {{ uploadResult.guestsCreated }}명</p>
          <p>✓ 일정 배정: {{ uploadResult.scheduleAssignments }}건</p>
        </div>
        <ul v-if="uploadResult.errors && uploadResult.errors.length" class="mt-2 text-sm text-red-700">
          <li v-for="(error, idx) in uploadResult.errors" :key="idx">• {{ error }}</li>
        </ul>
      </div>
    </div>

    <!-- 샘플 다운로드 -->
    <div class="mt-6 pt-6 border-t">
      <h3 class="font-semibold mb-3">샘플 파일</h3>
      <p class="text-sm text-gray-600 mb-3">
        샘플 엑셀 파일을 다운로드하여 형식을 확인하세요.
      </p>
      <a
        href="/Sample/업로드_샘플.xlsx"
        download
        class="inline-block px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200"
      >
        📥 샘플 다운로드
      </a>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import apiClient from '@/services/api'

const props = defineProps({
  conventionId: { type: Number, required: true }
})

const file = ref(null)
const uploading = ref(false)
const uploadResult = ref(null)

const handleFile = (e) => {
  file.value = e.target.files[0]
  uploadResult.value = null
}

const upload = async () => {
  if (!file.value) return
  
  uploading.value = true
  uploadResult.value = null
  
  const formData = new FormData()
  formData.append('file', file.value)
  
  try {
    const response = await apiClient.post(`/scheduleupload/conventions/${props.conventionId}/upload`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    
    uploadResult.value = {
      success: true,
      message: response.data.message,
      totalSchedules: response.data.totalSchedules,
      guestsCreated: response.data.guestsCreated,
      scheduleAssignments: response.data.scheduleAssignments,
      errors: response.data.errors
    }
  } catch (error) {
    uploadResult.value = {
      success: false,
      message: '업로드 실패',
      errors: [error.response?.data?.message || error.message]
    }
  } finally {
    uploading.value = false
  }
}
</script>
