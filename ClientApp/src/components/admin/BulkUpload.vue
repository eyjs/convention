<template>
  <div class="bg-white rounded-lg shadow p-6">
    <h2 class="text-xl font-semibold mb-4">엑셀 일괄 업로드</h2>

    <!-- 탭 메뉴 -->
    <div class="border-b border-gray-200 mb-6">
      <nav class="-mb-px flex space-x-8">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          :class="[
            'whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm',
            activeTab === tab.id
              ? 'border-primary-500 text-primary-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
          ]"
          @click="activeTab = tab.id"
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
          accept=".xlsx"
          class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
          @change="handleFileGuests"
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
        :disabled="!fileGuests || uploadingGuests"
        class="w-full px-6 py-3 bg-primary-600 text-white rounded-md hover:bg-primary-700 disabled:bg-gray-300 disabled:cursor-not-allowed mb-4"
        @click="uploadGuests"
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
          accept=".xlsx"
          class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
          @change="handleFileSchedules"
        />
      </div>

      <div class="mb-4 p-4 bg-purple-50 rounded-md">
        <h3 class="font-medium text-purple-900 mb-2">
          📋 엑셀 형식 (일정 업로드)
        </h3>
        <div class="text-sm text-purple-700 space-y-1">
          <p><strong>1행:</strong> 헤더 (생략 가능, 2행부터 데이터 시작)</p>
          <div class="mt-2 space-y-1 pl-2">
            <p><strong>A열:</strong> 날짜 (필수) - 예: 2025-11-17</p>
            <p><strong>B열:</strong> 시작시간 (필수) - 예: 09:00</p>
            <p><strong>C열:</strong> 종료시간 (선택) - 예: 11:30</p>
            <p><strong>D열:</strong> 장소 (선택) - 예: 호텔로비</p>
            <p><strong>E열:</strong> 일정명 (필수) - 예: 개인정비</p>
            <p><strong>F열:</strong> 메모 (선택) - 상세 설명, 여러 줄 가능</p>
          </div>
          <p class="mt-3 text-purple-600">
            ※ 업로드할 때마다 새로운 일정 템플릿 생성
          </p>
          <p class="text-purple-600">
            ※ 과거 템플릿은 웹에서 확인 후 삭제 가능
          </p>
        </div>
      </div>

      <button
        :disabled="!fileSchedules || uploadingSchedules"
        class="w-full px-6 py-3 bg-purple-600 text-white rounded-md hover:bg-purple-700 disabled:bg-gray-300 disabled:cursor-not-allowed mb-4"
        @click="uploadSchedules"
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

    <!-- 옵션투어 업로드 탭 -->
    <div v-if="activeTab === 'option-tours'" class="space-y-4">
      <div class="mb-4">
        <label class="block text-sm font-medium text-gray-700 mb-2">
          엑셀 파일 선택
        </label>
        <input
          type="file"
          accept=".xlsx"
          class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
          @change="handleFileOptionTours"
        />
      </div>

      <div class="mb-4 p-4 bg-orange-50 rounded-md">
        <h3 class="font-medium text-orange-900 mb-2">
          📋 엑셀 형식 (옵션투어 업로드)
        </h3>
        <div class="text-sm text-orange-700 space-y-2">
          <div class="border-b border-orange-200 pb-2">
            <p class="font-semibold text-orange-800">시트1: 옵션</p>
            <p><strong>A열:</strong> 날짜 (필수) - 예: 2025-05-18</p>
            <p><strong>B열:</strong> 시작시간 (필수) - 예: 02:00</p>
            <p><strong>C열:</strong> 종료시간 (필수) - 예: 09:00</p>
            <p><strong>D열:</strong> 옵션명 (필수) - 예: 바뚜르산</p>
            <p><strong>E열:</strong> 옵션ID (필수) - 사용자 지정 ID (숫자)</p>
            <p><strong>F열:</strong> 옵션내용 (선택) - 상세 설명</p>
          </div>
          <div class="pt-2">
            <p class="font-semibold text-orange-800">시트2: 참석자별 매핑</p>
            <p><strong>A열:</strong> 번호 (선택)</p>
            <p><strong>B열:</strong> 사업단/소속 (선택)</p>
            <p><strong>C열:</strong> 이름 (필수) - 참석자 매칭용</p>
            <p><strong>D열:</strong> 주민번호 (조건부 필수) - 참석자 매칭용</p>
            <p><strong>E열:</strong> 연락처 (조건부 필수) - 참석자 매칭용</p>
            <p><strong>F열:</strong> 그룹 (선택)</p>
            <p>
              <strong>G열:</strong> 옵션 ID (필수) - 콤마로 구분 (예: 1,2,3)
            </p>
          </div>
          <p class="mt-3 text-orange-600">
            ※ 참석자는 이름 + (전화번호 OR 주민번호)로 매칭됩니다
          </p>
          <p class="text-orange-600">※ 참석자가 미리 등록되어 있어야 합니다</p>
          <p class="text-orange-600">
            ※ 프론트엔드에서 일정 + 옵션 = 일정표로 조합됩니다
          </p>
        </div>
      </div>

      <button
        :disabled="!fileOptionTours || uploadingOptionTours"
        class="w-full px-6 py-3 bg-orange-600 text-white rounded-md hover:bg-orange-700 disabled:bg-gray-300 disabled:cursor-not-allowed mb-4"
        @click="uploadOptionTours"
      >
        {{ uploadingOptionTours ? '업로드 중...' : '옵션투어 업로드' }}
      </button>

      <UploadResult
        v-if="resultOptionTours"
        :result="resultOptionTours"
        type="option-tours"
      />

      <div class="mt-6 pt-6 border-t">
        <h3 class="font-semibold mb-3">샘플 파일</h3>
        <a
          href="/Sample/옵션투어_업로드_샘플.xlsx"
          download
          class="inline-block px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200"
        >
          📥 옵션투어 업로드 샘플
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
          accept=".xlsx"
          class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
          @change="handleFileAttributes"
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
        :disabled="!fileAttributes || uploadingAttributes"
        class="w-full px-6 py-3 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:bg-gray-300 disabled:cursor-not-allowed mb-4"
        @click="uploadAttributes"
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
            class="inline-block px-4 py-2 bg-green-100 text-green-800 rounded-md hover:bg-green-200"
            @click="downloadGuests"
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
import * as XLSX from 'xlsx'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

// 탭 정의
const tabs = [
  { id: 'guests', name: '참석자 업로드' },
  { id: 'schedules', name: '일정 업로드' },
  { id: 'attributes', name: '속성 업로드' },
  { id: 'option-tours', name: '옵션투어 업로드' },
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

// 옵션투어 업로드 상태
const fileOptionTours = ref(null)
const uploadingOptionTours = ref(false)
const resultOptionTours = ref(null)

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

const handleFileOptionTours = (e) => {
  fileOptionTours.value = e.target.files[0]
  resultOptionTours.value = null
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

// 엑셀 날짜를 YYYY-MM-DD 형식으로 변환
const excelDateToString = (excelDate) => {
  if (typeof excelDate === 'string') return excelDate
  const date = XLSX.SSF.parse_date_code(excelDate)
  return `${date.y}-${String(date.m).padStart(2, '0')}-${String(date.d).padStart(2, '0')}`
}

// 시간 문자열 정리 (공백 제거, HH:MM 형식으로)
const normalizeTime = (timeStr) => {
  if (!timeStr) return ''
  return String(timeStr).trim()
}

// 옵션투어 업로드
const uploadOptionTours = async () => {
  if (!fileOptionTours.value) return

  uploadingOptionTours.value = true
  resultOptionTours.value = null

  try {
    // 엑셀 파일 읽기
    const data = await fileOptionTours.value.arrayBuffer()
    const workbook = XLSX.read(data, { type: 'array' })

    // 시트 확인
    if (workbook.SheetNames.length < 2) {
      throw new Error(
        '엑셀 파일에 2개의 시트가 필요합니다 (옵션, 참석자별 매핑)',
      )
    }

    const optionsSheetName = workbook.SheetNames[0]
    const mappingSheetName = workbook.SheetNames[1]

    // 시트1: 옵션 파싱
    const optionsSheet = workbook.Sheets[optionsSheetName]
    const optionsRaw = XLSX.utils.sheet_to_json(optionsSheet, { header: 1 })

    const options = []
    for (let i = 1; i < optionsRaw.length; i++) {
      const row = optionsRaw[i]
      if (!row || row.length === 0) continue

      const [dateVal, startTime, endTime, name, optionId, content] = row

      if (!dateVal || !startTime || !name || optionId == null) {
        console.warn(`옵션 시트 ${i + 1}행 스킵: 필수 값 누락`)
        continue
      }

      options.push({
        date: excelDateToString(dateVal),
        startTime: normalizeTime(startTime),
        endTime: normalizeTime(endTime),
        name: String(name).trim(),
        optionId: Number(optionId),
        content: content ? String(content).trim() : '',
      })
    }

    // 시트2: 참석자별 매핑 파싱
    const mappingSheet = workbook.Sheets[mappingSheetName]
    const mappingRaw = XLSX.utils.sheet_to_json(mappingSheet, { header: 1 })

    const participantMappings = []
    for (let i = 1; i < mappingRaw.length; i++) {
      const row = mappingRaw[i]
      if (!row || row.length === 0) continue

      const [num, division, name, idNumber, phone, group, optionIds] = row

      if (!name || !phone) {
        console.warn(
          `참석자 매핑 시트 ${i + 1}행 스킵: 이름 또는 전화번호 누락`,
        )
        continue
      }

      // 옵션ID 파싱 (콤마로 구분된 문자열 -> 배열)
      let optionIdArray = []
      if (optionIds) {
        const idsStr = String(optionIds)
        optionIdArray = idsStr
          .split(',')
          .map((id) => id.trim())
          .filter((id) => id !== '')
          .map((id) => Number(id))
      }

      participantMappings.push({
        name: String(name).trim(),
        phone: String(phone).trim(),
        division: division ? String(division).trim() : '',
        idNumber: idNumber ? String(idNumber).trim() : '',
        group: group ? String(group).trim() : '',
        optionIds: optionIdArray,
      })
    }

    // API로 전송
    const response = await apiClient.post(
      `/upload/conventions/${props.conventionId}/option-tours`,
      {
        options,
        participantMappings,
      },
    )

    resultOptionTours.value = {
      success: response.data.success,
      message: `옵션 ${response.data.optionsCreated}개, 참석자 매핑 ${response.data.mappingsCreated}개 생성됨`,
      data: {
        optionsCreated: response.data.optionsCreated,
        mappingsCreated: response.data.mappingsCreated,
      },
      errors: response.data.errors || [],
      warnings: response.data.warnings || [],
    }
  } catch (error) {
    resultOptionTours.value = {
      success: false,
      message: '업로드 실패',
      errors: [
        error.response?.data?.error ||
          error.response?.data?.message ||
          error.message,
      ],
    }
  } finally {
    uploadingOptionTours.value = false
  }
}
</script>
