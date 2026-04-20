<template>
  <div>
    <AdminPageHeader title="엑셀 일괄 업로드" class="mb-4" />

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

      <div class="mb-4 p-4 bg-primary-50 rounded-md">
        <h3 class="font-medium text-primary-900 mb-2">
          📋 엑셀 형식 (참석자 업로드)
        </h3>
        <div class="text-sm text-primary-700 space-y-2">
          <div class="border-b border-primary-200 pb-2">
            <p class="font-semibold text-primary-800">시트1: 참석자 (필수)</p>
            <p>
              <strong>1~7열 (고정):</strong> 번호 | 소속 | 이름 | 주민번호 |
              전화번호 | 그룹명 | 비고
            </p>
            <p>
              <strong>8열~ (가변):</strong> 배정 속성 자유 추가
              <span class="text-primary-500"
                >예) 룸번호 | 룸메이트 | 버스좌석 | 식사테이블</span
              >
            </p>
            <p class="text-primary-500 text-xs">
              ※ 8번째 컬럼부터 헤더명이 속성 키, 값이 속성 값으로 저장됩니다
            </p>
            <p><strong>필수:</strong> 이름 + (전화번호 OR 주민번호)</p>
            <p class="text-primary-500 text-xs">
              ※ 첫 번째 셀이 ※로 시작하는 행은 안내 행으로 자동 스킵됩니다
            </p>
          </div>
          <div class="border-b border-primary-200 pb-2">
            <p class="font-semibold text-primary-800">
              시트2: 그룹-일정 매핑 (선택)
            </p>
            <p>
              <strong>A열:</strong> 그룹명 | <strong>B열:</strong> 일정코스명 |
              <strong>C열:</strong> 코스설명 (선택)
            </p>
            <p class="text-primary-500 text-xs">
              ※ 일정이 먼저 업로드되어 있어야 합니다
            </p>
          </div>
          <p class="mt-1 text-primary-600">
            ※ 업로드 시 기존 참석자는 엑셀 기준으로 완전히 교체됩니다 (User
            계정/메타데이터는 보존)
          </p>
          <div class="mt-2 p-2 bg-primary-100 rounded text-xs text-primary-800">
            <p class="font-semibold mb-0.5">🔐 신규 참석자 자동 계정 생성</p>
            <p>• 로그인 ID: 자동 생성 (guest_xxx)</p>
            <p>• 기본 비밀번호: <strong>1111</strong></p>
            <p>• 접근 링크(SMS)로 비밀번호 없이 로그인 가능</p>
          </div>
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
        <h3 class="font-semibold mb-3">파일 다운로드</h3>
        <div class="flex flex-wrap gap-2">
          <button
            class="inline-block px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 text-sm"
            @click="downloadGuestSample"
          >
            📄 샘플
          </button>
          <button
            class="inline-block px-4 py-2 bg-primary-50 text-primary-700 rounded-md hover:bg-primary-100 text-sm"
            @click="downloadCurrentData('guests')"
          >
            📥 현재 참석자
          </button>
          <button
            class="inline-block px-4 py-2 bg-green-50 text-green-700 rounded-md hover:bg-green-100 text-sm"
            @click="downloadGuests"
          >
            📥 참석자 속성
          </button>
        </div>
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
          <p><strong>시트명</strong> = 일정 코스명 (예: A조, 관광코스)</p>
          <p><strong>1행:</strong> 헤더 (2행부터 데이터 시작)</p>
          <div class="mt-2 space-y-1 pl-2">
            <p><strong>A열:</strong> 날짜 (필수) - 예: 2025-11-17</p>
            <p><strong>B열:</strong> 시작시간 (필수) - 예: 09:00</p>
            <p><strong>C열:</strong> 종료시간 (선택) - 예: 11:30</p>
            <p><strong>D열:</strong> 장소 (선택) - 예: 호텔로비</p>
            <p><strong>E열:</strong> 지도링크 (선택) - URL</p>
            <p><strong>F열:</strong> 일정명 (필수) - 예: 개인정비</p>
            <p><strong>G열:</strong> 내용 (선택) - 상세 설명, 여러 줄 가능</p>
            <p>
              <strong>H열:</strong> 노출_개인정보 (선택) - 쉼표 구분 속성 키
              <span class="text-purple-500">예: 룸번호,룸메이트</span>
            </p>
          </div>
          <p class="mt-3 text-purple-600">
            ※ 시트별로 일정 코스가 생성됩니다 (멀티시트 지원)
          </p>
          <p class="text-purple-600">
            ※ ※로 시작하는 행은 안내 행으로 스킵됩니다
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
        <h3 class="font-semibold mb-3">파일 다운로드</h3>
        <div class="flex flex-wrap gap-2">
          <button
            class="inline-block px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 text-sm"
            @click="downloadScheduleSample"
          >
            📄 샘플
          </button>
          <button
            class="inline-block px-4 py-2 bg-purple-50 text-purple-700 rounded-md hover:bg-purple-100 text-sm"
            @click="downloadCurrentData('schedules')"
          >
            📥 현재 일정
          </button>
        </div>
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
            <p><strong>E열:</strong> 옵션내용 (선택) - 상세 설명</p>
            <p class="text-orange-500 text-xs mt-1">
              ※ 1행은 헤더, 2행부터 데이터 (행 번호 = 옵션 번호)
            </p>
          </div>
          <div class="pt-2">
            <p class="font-semibold text-orange-800">시트2: 참석자별 매핑</p>
            <p><strong>A열:</strong> 이름 (필수) - 참석자 매칭용</p>
            <p><strong>B열:</strong> 주민번호 (조건부) - 참석자 매칭용</p>
            <p><strong>C열:</strong> 연락처 (조건부) - 참석자 매칭용</p>
            <p>
              <strong>D열:</strong> 옵션 번호 (필수) - 시트1의 엑셀 행 번호,
              콤마로 구분 (예: 2,3,4)
            </p>
            <p class="text-orange-500 text-xs mt-1">
              ※ 이름 + (주민번호 OR 연락처) 중 하나로 참석자 매칭
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
        <div class="flex flex-wrap gap-2">
          <button
            class="inline-block px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 text-sm"
            @click="downloadOptionTourSample"
          >
            📄 샘플
          </button>
          <button
            class="inline-block px-4 py-2 bg-orange-50 text-orange-700 rounded-md hover:bg-orange-100 text-sm"
            @click="downloadCurrentData('option-tours')"
          >
            📥 현재 옵션투어
          </button>
        </div>
      </div>
    </div>

    <!-- 일정 업로드 미리보기 모달 -->
    <ScheduleUploadPreviewModal
      v-if="showScheduleModal"
      :is-open="showScheduleModal"
      :convention-id="conventionId"
      :preview="schedulePreview"
      @close="showScheduleModal = false"
      @saved="handleScheduleSaved"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import apiClient from '@/services/api'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import UploadResult from './UploadResult.vue'
import ScheduleUploadPreviewModal from './ScheduleUploadPreviewModal.vue'
import * as XLSX from 'xlsx'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

// 탭 정의
const tabs = [
  { id: 'guests', name: '참석자 업로드' },
  { id: 'schedules', name: '일정 업로드' },
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
const schedulePreview = ref(null)
const showScheduleModal = ref(false)

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

const handleScheduleSaved = (saveResult) => {
  resultSchedules.value = {
    success: saveResult.success,
    message: `${saveResult.itemsCreated}개 일정 저장 완료`,
    data: {
      templates: saveResult.templatesCreated,
      actions: saveResult.createdActions || [],
    },
    errors: saveResult.errors || [],
    warnings: saveResult.warnings || [],
  }
  fileSchedules.value = null
  schedulePreview.value = null
}

const handleFileOptionTours = (e) => {
  fileOptionTours.value = e.target.files[0]
  resultOptionTours.value = null
}

// 참석자 업로드
const uploadGuests = async () => {
  if (!fileGuests.value) return

  // 현재 참석자 수 조회 후 확인 다이얼로그
  let currentCount = 0
  try {
    const countRes = await apiClient.get(
      `/admin/conventions/${props.conventionId}/guests`,
    )
    currentCount = Array.isArray(countRes.data) ? countRes.data.length : 0
  } catch {
    // 조회 실패 시 0으로 진행
  }

  const confirmMsg =
    `이 작업은 현재 행사의 참석자 목록을 엑셀 기준으로 완전히 교체합니다.\n\n` +
    `• 기존 참석자 ${currentCount}명이 이 행사에서 제외됩니다.\n` +
    `• 엑셀 파일의 참석자로 재등록됩니다.\n` +
    `• User 계정과 개인 메타데이터(속성)는 유지됩니다.\n\n` +
    `계속하시겠습니까?`

  if (!confirm(confirmMsg)) return

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

    const d = response.data
    let message = `기존 ${d.removedUserConventions || 0}명 삭제, ${d.usersCreated + d.usersUpdated}명 재등록 완료 (신규 User: ${d.usersCreated}명, 기존 User 재사용: ${d.usersUpdated}명)`
    if (d.attributesCreated > 0 || d.attributesUpdated > 0) {
      message += `\n속성: 신규 ${d.attributesCreated}건, 수정 ${d.attributesUpdated}건`
    }
    if (d.scheduleAssignmentsCreated > 0 || d.scheduleDuplicatesSkipped > 0) {
      message += `\n일정 배정: ${d.scheduleAssignmentsCreated}건`
      if (d.scheduleDuplicatesSkipped > 0)
        message += ` (중복 스킵: ${d.scheduleDuplicatesSkipped}건)`
    }
    resultGuests.value = {
      success: d.success,
      message,
      data: {
        created: d.usersCreated,
        updated: d.usersUpdated,
        total: d.totalProcessed,
      },
      errors: d.errors || [],
      warnings: [
        ...(d.warnings || []),
        ...(d.scheduleWarnings || []),
        ...(d.attributeWarnings || []),
      ],
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

// 일정 업로드 — 미리보기 호출
const uploadSchedules = async () => {
  if (!fileSchedules.value) return

  uploadingSchedules.value = true
  resultSchedules.value = null

  const formData = new FormData()
  formData.append('file', fileSchedules.value)

  try {
    const previewRes = await apiClient.post(
      `/upload/conventions/${props.conventionId}/schedule-templates/preview`,
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      },
    )

    schedulePreview.value = previewRes.data
    showScheduleModal.value = true
  } catch (error) {
    resultSchedules.value = {
      success: false,
      message: '미리보기 실패',
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

// ===== 샘플 엑셀 생성 (클라이언트) =====
function saveWorkbook(wb, filename) {
  const wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'array' })
  const blob = new Blob([wbout], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  })
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}

// 참석자 업로드 샘플 (시트1: 참석자+가변속성 / 시트2: 그룹-일정매핑)
function downloadGuestSample() {
  const wb = XLSX.utils.book_new()

  // 시트1: 참석자 (고정 7컬럼 + 가변 속성 예시 3컬럼)
  const sheet1 = [
    [
      '번호',
      '소속/부서',
      '이름',
      '주민등록번호',
      '전화번호',
      '그룹명',
      '비고',
      '룸번호',
      '룸메이트',
      '버스좌석',
    ],
    [
      1,
      '영업팀',
      '홍길동',
      '900101-1234567',
      '010-1234-5678',
      'A조',
      '',
      '101',
      '김영희',
      '3A',
    ],
    [
      2,
      '개발팀',
      '김영희',
      '920202-2345678',
      '010-2345-6789',
      'A조',
      '',
      '101',
      '홍길동',
      '3B',
    ],
    [
      3,
      '마케팅팀',
      '이철수',
      '',
      '010-3456-7890',
      'B조',
      'VIP',
      '201',
      '',
      '5C',
    ],
    ['※ 이 행은 안내 행으로 업로드 시 자동 스킵됩니다.'],
  ]
  XLSX.utils.book_append_sheet(wb, XLSX.utils.aoa_to_sheet(sheet1), '참석자')

  // 시트2: 그룹-일정매핑 (그룹명, 일정코스명, 코스설명)
  const sheet2 = [
    ['그룹명', '일정코스명', '코스설명'],
    ['A조', '기본코스', '일반 참석자 코스'],
    ['B조', 'VIP코스', 'VIP 전용 코스'],
  ]
  XLSX.utils.book_append_sheet(
    wb,
    XLSX.utils.aoa_to_sheet(sheet2),
    '그룹-일정매핑',
  )

  saveWorkbook(wb, '참석자업로드_샘플.xlsx')
}

// 일정 업로드 샘플 (멀티시트: 시트명 = 코스명)
function downloadScheduleSample() {
  const wb = XLSX.utils.book_new()
  const header = [
    '날짜',
    '시작시간',
    '종료시간',
    '장소',
    '지도링크',
    '일정명',
    '내용',
    '노출_개인정보',
  ]

  // 시트1: A조 코스
  const sheet1 = [
    header,
    [
      '2026-05-01',
      '09:00',
      '10:00',
      '인천공항 T1',
      '',
      '집결',
      '3층 A카운터 집합',
      '',
    ],
    ['2026-05-01', '11:00', '14:00', '기내', '', 'KE123 탑승', '', ''],
    [
      '2026-05-01',
      '16:00',
      '18:00',
      '호텔 로비',
      '',
      '체크인',
      '',
      '룸번호,룸메이트',
    ],
    [
      '2026-05-02',
      '09:00',
      '12:00',
      '콜로세움',
      'https://maps.google.com/?q=콜로세움',
      '관광',
      '',
      '',
    ],
    ['2026-05-02', '13:00', '15:00', '현지 레스토랑', '', '점심', '', ''],
    ['※ 이 행은 안내 행으로 업로드 시 자동 스킵됩니다.'],
  ]
  XLSX.utils.book_append_sheet(wb, XLSX.utils.aoa_to_sheet(sheet1), 'A조')

  // 시트2: B조 코스 (멀티시트 예시)
  const sheet2 = [
    header,
    [
      '2026-05-01',
      '10:00',
      '11:00',
      '인천공항 T2',
      '',
      '집결',
      '2층 B카운터 집합',
      '',
    ],
    ['2026-05-01', '14:00', '17:00', '기내', '', 'OZ201 탑승', '', ''],
    ['2026-05-01', '19:00', '20:00', '호텔 로비', '', '체크인', '', '룸번호'],
    ['※ 이 행은 안내 행으로 업로드 시 자동 스킵됩니다.'],
  ]
  XLSX.utils.book_append_sheet(wb, XLSX.utils.aoa_to_sheet(sheet2), 'B조')

  saveWorkbook(wb, '일정업로드_샘플.xlsx')
}

// 옵션투어 업로드 샘플
function downloadOptionTourSample() {
  const wb = XLSX.utils.book_new()
  const sheet = [
    ['투어명', '설명', '가격', '날짜', '시작시간', '종료시간', '정원'],
    [
      '바티칸 투어',
      '성 베드로 대성당 + 박물관',
      120,
      '2026-05-03',
      '09:00',
      '13:00',
      20,
    ],
    [
      '나이트 투어',
      '로마 야경 워킹 투어',
      50,
      '2026-05-03',
      '19:00',
      '22:00',
      15,
    ],
  ]
  XLSX.utils.book_append_sheet(wb, XLSX.utils.aoa_to_sheet(sheet), '옵션투어')
  saveWorkbook(wb, '옵션투어_업로드_샘플.xlsx')
}

// 현재 데이터 다운로드
const downloadCurrentData = async (type) => {
  const urlMap = {
    guests: `/upload/conventions/${props.conventionId}/guests/download`,
    schedules: `/upload/conventions/${props.conventionId}/schedules/download`,
    'option-tours': `/upload/conventions/${props.conventionId}/option-tours/download`,
  }

  try {
    const response = await apiClient.get(urlMap[type], { responseType: 'blob' })
    const blob = new Blob([response.data], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url

    const contentDisposition = response.headers['content-disposition']
    let fileName = `${type}_download.xlsx`
    if (contentDisposition) {
      const match = contentDisposition.match(
        /filename\*?=['"]?(?:UTF-8'')?([^'";]+)/i,
      )
      if (match) fileName = decodeURIComponent(match[1])
    }

    link.download = fileName
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Download failed:', error)
    alert('다운로드 실패: ' + (error.response?.data?.message || error.message))
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

      const [dateVal, startTime, endTime, name, content] = row

      if (!dateVal || !startTime || !name) {
        console.warn(`옵션 시트 ${i + 1}행 스킵: 필수 값 누락`)
        continue
      }

      options.push({
        date: excelDateToString(dateVal),
        startTime: normalizeTime(startTime),
        endTime: normalizeTime(endTime),
        name: String(name).trim(),
        optionId: i + 1,
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

      const [name, idNumber, phone, optionIds] = row

      if (!name || (!phone && !idNumber)) {
        console.warn(
          `참석자 매핑 시트 ${i + 1}행 스킵: 이름 또는 식별정보(전화/주민번호) 누락`,
        )
        continue
      }

      // 옵션번호 파싱 (콤마로 구분된 문자열 -> 배열)
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
        idNumber: idNumber ? String(idNumber).trim() : '',
        phone: phone ? String(phone).trim() : '',
        division: '',
        group: '',
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
