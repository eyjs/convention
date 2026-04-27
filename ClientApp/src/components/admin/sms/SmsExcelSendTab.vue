<template>
  <div>
    <!-- 단계 표시 -->
    <div class="flex items-center gap-1 mb-4">
      <span v-for="s in 5" :key="s" class="flex items-center gap-1">
        <span
          class="w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold"
          :class="
            step === s
              ? 'bg-primary-500 text-white'
              : step > s
                ? 'bg-green-500 text-white'
                : 'bg-gray-200 text-gray-500'
          "
        >
          {{ step > s ? '&#10003;' : s }}
        </span>
        <span
          v-if="s < 5"
          class="w-4 h-px"
          :class="step > s ? 'bg-green-400' : 'bg-gray-300'"
        ></span>
      </span>
      <span class="ml-2 text-sm text-gray-500">
        {{ stepLabels[step - 1] }}
      </span>
    </div>

    <!-- Step 1: 그룹 선택 -->
    <div v-if="step === 1">
      <h4 class="font-semibold text-gray-800 mb-3">그룹 선택</h4>

      <div v-if="groupsLoading" class="text-center py-8 text-gray-400">
        그룹 로딩 중...
      </div>

      <div v-else class="bg-white border rounded-lg p-4">
        <label class="block text-sm font-medium text-gray-700 mb-2"
          >발송 대상 그룹</label
        >
        <select
          v-model="selectedGroup"
          class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
        >
          <option value="" disabled>그룹을 선택하세요</option>
          <option v-for="g in groups" :key="g.groupName" :value="g.groupName">
            {{ g.groupName }} ({{ g.count }}명{{
              g.noPhoneCount > 0 ? `, 번호없음 ${g.noPhoneCount}명` : ''
            }})
          </option>
        </select>

        <div v-if="selectedGroupInfo" class="mt-3 p-3 bg-blue-50 rounded-lg">
          <p class="text-sm text-blue-800">
            <span class="font-semibold">{{ selectedGroupInfo.count }}명</span>의
            수신자 정보가 엑셀에 포함됩니다.
          </p>
        </div>
      </div>
    </div>

    <!-- Step 2: 엑셀 다운로드 -->
    <div v-if="step === 2">
      <h4 class="font-semibold text-gray-800 mb-3">엑셀 템플릿 다운로드</h4>

      <div class="bg-white border rounded-lg p-4">
        <p class="text-sm text-gray-600 mb-4">
          아래 버튼을 클릭하면 선택한 그룹(<span class="font-semibold">{{
            selectedGroup
          }}</span
          >)의 수신자 이름과 전화번호가 채워진 엑셀 파일이 다운로드됩니다.
        </p>

        <div class="bg-gray-50 rounded-lg p-3 mb-4 text-xs text-gray-500">
          <p class="font-medium text-gray-700 mb-1">엑셀 구조:</p>
          <ul class="list-disc list-inside space-y-0.5">
            <li>
              <span class="text-green-600 font-medium">이름, 전화번호</span> -
              자동 채워짐 (수정 불가)
            </li>
            <li>
              <span class="text-blue-600 font-medium">변수1, 변수2, ...</span> -
              헤더명을 변수명으로 변경하고 값을 입력하세요
            </li>
          </ul>
        </div>

        <button
          class="w-full px-4 py-3 bg-green-500 text-white rounded-lg text-sm font-medium hover:bg-green-600 transition-colors flex items-center justify-center gap-2"
          :disabled="downloading"
          @click="downloadTemplate"
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
              d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
            />
          </svg>
          {{ downloading ? '다운로드 중...' : '엑셀 템플릿 다운로드' }}
        </button>

        <p v-if="downloaded" class="mt-2 text-xs text-green-600 text-center">
          다운로드 완료! 변수를 입력한 후 다음 단계에서 업로드하세요.
        </p>
      </div>
    </div>

    <!-- Step 3: 엑셀 업로드 -->
    <div v-if="step === 3">
      <h4 class="font-semibold text-gray-800 mb-3">엑셀 업로드</h4>

      <div class="bg-white border rounded-lg p-4">
        <!-- 파일 업로드 영역 -->
        <div
          class="border-2 border-dashed rounded-lg p-6 text-center transition-colors"
          :class="
            isDragOver ? 'border-primary-500 bg-primary-50' : 'border-gray-300'
          "
          @dragover.prevent="isDragOver = true"
          @dragleave.prevent="isDragOver = false"
          @drop.prevent="handleDrop"
        >
          <svg
            class="w-10 h-10 mx-auto text-gray-400 mb-3"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
            />
          </svg>
          <p class="text-sm text-gray-600 mb-2">
            변수를 입력한 엑셀 파일을 드래그하거나
          </p>
          <label class="cursor-pointer">
            <span
              class="px-4 py-2 bg-primary-500 text-white text-sm rounded-lg hover:bg-primary-600 transition-colors"
            >
              파일 선택
            </span>
            <input
              type="file"
              accept=".xlsx,.xls"
              class="hidden"
              @change="handleFileSelect"
            />
          </label>
        </div>

        <!-- 파싱 로딩 -->
        <div v-if="parsing" class="mt-4 text-center text-gray-400">
          엑셀 파싱 중...
        </div>

        <!-- 파싱 결과 -->
        <div v-if="parseResult" class="mt-4">
          <div class="flex items-center gap-2 mb-3">
            <span class="text-sm font-medium text-gray-700">파싱 결과</span>
            <span
              class="px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full"
            >
              {{ parseResult.recipients.length }}명
            </span>
            <span
              v-if="parseResult.errorRows.length > 0"
              class="px-2 py-0.5 bg-red-100 text-red-700 text-xs rounded-full"
            >
              오류 {{ parseResult.errorRows.length }}건
            </span>
          </div>

          <!-- 변수 컬럼 -->
          <div v-if="parseResult.columns.length > 0" class="mb-3">
            <p class="text-xs text-gray-500 mb-1">변수 컬럼:</p>
            <div class="flex flex-wrap gap-1">
              <span
                v-for="col in parseResult.columns"
                :key="col"
                class="px-2 py-0.5 bg-blue-50 text-blue-700 text-xs rounded border border-blue-200"
              >
                {{ '#\{' + col + '\}' }}
              </span>
            </div>
          </div>

          <!-- 샘플 미리보기 -->
          <div v-if="parseResult.recipients.length > 0" class="mb-3">
            <p class="text-xs text-gray-500 mb-1">샘플 (최대 3행):</p>
            <div class="overflow-x-auto">
              <table class="w-full text-xs border">
                <thead>
                  <tr class="bg-gray-50">
                    <th class="px-2 py-1 text-left border-r">이름</th>
                    <th class="px-2 py-1 text-left border-r">전화번호</th>
                    <th
                      v-for="col in parseResult.columns"
                      :key="col"
                      class="px-2 py-1 text-left border-r"
                    >
                      {{ col }}
                    </th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="(r, idx) in parseResult.recipients.slice(0, 3)"
                    :key="idx"
                    class="border-t"
                  >
                    <td class="px-2 py-1 border-r">{{ r.name }}</td>
                    <td class="px-2 py-1 border-r">{{ r.phone }}</td>
                    <td
                      v-for="col in parseResult.columns"
                      :key="col"
                      class="px-2 py-1 border-r"
                    >
                      {{ r.variables[col] || '' }}
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- 오류 행 -->
          <div v-if="parseResult.errorRows.length > 0">
            <p class="text-xs text-red-600 font-medium mb-1">오류 행:</p>
            <ul
              class="text-xs text-red-500 space-y-0.5 max-h-24 overflow-y-auto"
            >
              <li v-for="err in parseResult.errorRows" :key="err.row">
                {{ err.row }}행: {{ err.reason }}
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <!-- Step 4: 메시지 작성 -->
    <div v-if="step === 4">
      <h4 class="font-semibold text-gray-800 mb-3">메시지 작성</h4>

      <!-- 템플릿 선택 -->
      <div class="bg-white border rounded-lg p-4 mb-4">
        <label class="block text-sm font-medium text-gray-700 mb-2"
          >템플릿 선택 (선택사항)</label
        >
        <select
          v-model="selectedTemplateId"
          class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          @change="applyTemplate"
        >
          <option :value="null">직접 입력</option>
          <option v-for="tpl in templates" :key="tpl.id" :value="tpl.id">
            {{ tpl.title }}
          </option>
        </select>
      </div>

      <div class="bg-white border rounded-lg p-4">
        <div class="flex items-center justify-between mb-2">
          <label class="text-sm font-medium text-gray-700">발송 내용</label>
          <span class="text-xs text-gray-400">
            {{ messageContent.length }}자
            <span v-if="messageContent.length > 90" class="text-amber-500"
              >(장문 LMS)</span
            >
          </span>
        </div>
        <textarea
          ref="messageTextarea"
          v-model="messageContent"
          rows="6"
          class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 resize-none font-mono text-sm"
          placeholder="메시지를 입력하세요. #{guest_name}, #{호차} 등 변수를 사용할 수 있습니다."
        ></textarea>

        <!-- 변수 삽입 -->
        <div class="mt-2">
          <label class="block text-xs font-medium text-gray-500 mb-1.5"
            >고정 변수</label
          >
          <div class="flex flex-wrap gap-1.5 mb-2">
            <button
              v-for="v in templateVariables"
              :key="v.key"
              class="px-2 py-1 bg-blue-50 text-blue-700 text-xs rounded border border-blue-200 hover:bg-blue-100 transition-colors"
              @click="insertVariable(v.key)"
            >
              {{ v.label }}
            </button>
          </div>

          <div v-if="excelVariables.length > 0">
            <label class="block text-xs font-medium text-gray-500 mb-1.5"
              >엑셀 변수</label
            >
            <div class="flex flex-wrap gap-1.5">
              <button
                v-for="col in excelVariables"
                :key="col"
                class="px-2 py-1 bg-green-50 text-green-700 text-xs rounded border border-green-200 hover:bg-green-100 transition-colors"
                @click="insertVariable(col)"
              >
                {{ col }}
              </button>
            </div>
          </div>
        </div>

        <!-- 실시간 미리보기 -->
        <div
          v-if="parseResult && parseResult.recipients.length > 0"
          class="mt-4 pt-3 border-t"
        >
          <label class="block text-xs font-medium text-gray-500 mb-1.5"
            >미리보기 (첫 번째 수신자 기준)</label
          >
          <div
            class="bg-gray-100 rounded-lg p-3 font-mono text-sm whitespace-pre-wrap"
          >
            {{ previewMessage }}
          </div>
        </div>
      </div>
    </div>

    <!-- Step 5: 확인 + 발송 -->
    <div v-if="step === 5">
      <h4 class="font-semibold text-gray-800 mb-3">발송 확인</h4>

      <!-- 요약 -->
      <div class="bg-white border rounded-lg p-4 mb-4">
        <dl class="grid grid-cols-2 gap-3 text-sm">
          <div>
            <dt class="text-gray-500">수신자 수</dt>
            <dd class="font-semibold text-gray-900">
              {{ parseResult.recipients.length }}명
            </dd>
          </div>
          <div>
            <dt class="text-gray-500">메시지 유형</dt>
            <dd class="font-semibold text-gray-900">
              {{ messageContent.length <= 90 ? 'SMS' : 'LMS' }}
              <span class="text-xs text-gray-400"
                >({{ messageContent.length }}자)</span
              >
            </dd>
          </div>
          <div>
            <dt class="text-gray-500">변수 컬럼</dt>
            <dd class="font-semibold text-gray-900">
              {{ excelVariables.join(', ') || '없음' }}
            </dd>
          </div>
        </dl>
      </div>

      <!-- 발송 진행률 -->
      <div v-if="sending" class="bg-white border rounded-lg p-4 mb-4">
        <div class="flex items-center justify-between mb-2">
          <span class="text-sm font-medium text-gray-700">발송 중...</span>
          <span class="text-sm text-gray-500"
            >{{ sendProgress.current }} / {{ sendProgress.total }}</span
          >
        </div>
        <div class="w-full bg-gray-200 rounded-full h-2">
          <div
            class="bg-primary-500 h-2 rounded-full transition-all duration-300"
            :style="{ width: `${sendProgressPercent}%` }"
          ></div>
        </div>
      </div>

      <!-- 발송 결과 -->
      <div
        v-if="sendResult"
        class="border rounded-lg p-4 mb-4"
        :class="
          sendResult.failCount === 0
            ? 'bg-green-50 border-green-200'
            : 'bg-amber-50 border-amber-200'
        "
      >
        <p
          class="text-sm font-medium"
          :class="
            sendResult.failCount === 0 ? 'text-green-800' : 'text-amber-800'
          "
        >
          발송 완료: 성공 {{ sendResult.successCount }}명 / 실패
          {{ sendResult.failCount }}명
        </p>
        <div
          v-if="sendResult.failedItems && sendResult.failedItems.length > 0"
          class="mt-2"
        >
          <p class="text-xs text-amber-700 font-medium mb-1">실패 목록:</p>
          <ul
            class="text-xs text-amber-600 space-y-0.5 max-h-32 overflow-y-auto"
          >
            <li v-for="(item, idx) in sendResult.failedItems" :key="idx">
              {{ item.name }} ({{ item.phone }}) - {{ item.reason }}
            </li>
          </ul>
        </div>
      </div>
    </div>

    <!-- Footer -->
    <div class="flex justify-between mt-4 pt-4 border-t">
      <button
        v-if="step > 1"
        class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-200 transition-colors"
        :disabled="sending"
        @click="step--"
      >
        이전
      </button>
      <div v-else></div>

      <div class="flex gap-2">
        <button
          v-if="step === 1"
          class="px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors disabled:opacity-50"
          :disabled="!selectedGroup"
          @click="step = 2"
        >
          다음
        </button>

        <button
          v-if="step === 2"
          class="px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors"
          @click="step = 3"
        >
          다음
        </button>

        <button
          v-if="step === 3"
          class="px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors disabled:opacity-50"
          :disabled="!parseResult || parseResult.recipients.length === 0"
          @click="goToStep4"
        >
          다음
        </button>

        <button
          v-if="step === 4"
          class="px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors disabled:opacity-50"
          :disabled="!messageContent.trim()"
          @click="goToStep5"
        >
          다음
        </button>

        <button
          v-if="step === 5 && !sendResult"
          class="px-4 py-2 bg-red-500 text-white rounded-lg text-sm font-medium hover:bg-red-600 transition-colors disabled:opacity-50"
          :disabled="sending"
          @click="confirmAndSend"
        >
          {{
            sending
              ? '발송 중...'
              : `${parseResult.recipients.length}명에게 발송`
          }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import apiClient from '@/services/api'
import {
  fetchSmsGroups,
  downloadExcelTemplate,
  parseExcel,
  sendWithExcel,
} from '@/services/smsGroupService'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const stepLabels = [
  '그룹 선택',
  '엑셀 다운로드',
  '엑셀 업로드',
  '메시지 작성',
  '발송 확인',
]
const step = ref(1)

const templateVariables = [
  { key: 'guest_name', label: '참석자명' },
  { key: 'guest_phone', label: '전화번호' },
  { key: 'corp_part', label: '부서' },
  { key: 'title', label: '행사명' },
  { key: 'start_date', label: '시작일' },
  { key: 'end_date', label: '종료일' },
  { key: 'url', label: '접속URL' },
]

// Step 1: 그룹
const groups = ref([])
const groupsLoading = ref(false)
const selectedGroup = ref('')

const selectedGroupInfo = computed(() => {
  return groups.value.find((g) => g.groupName === selectedGroup.value) || null
})

async function loadGroups() {
  groupsLoading.value = true
  try {
    const res = await fetchSmsGroups(props.conventionId)
    groups.value = res.data
  } catch (e) {
    console.error('그룹 로드 실패:', e)
  } finally {
    groupsLoading.value = false
  }
}

// Step 2: 엑셀 다운로드
const downloading = ref(false)
const downloaded = ref(false)

async function downloadTemplate() {
  downloading.value = true
  try {
    const res = await downloadExcelTemplate(
      props.conventionId,
      selectedGroup.value,
    )
    const blob = new Blob([res.data], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `SMS_발송_${selectedGroup.value}_${new Date().toISOString().slice(0, 10)}.xlsx`
    link.click()
    URL.revokeObjectURL(url)
    downloaded.value = true
  } catch (e) {
    console.error('엑셀 다운로드 실패:', e)
    alert('엑셀 다운로드에 실패했습니다.')
  } finally {
    downloading.value = false
  }
}

// Step 3: 엑셀 업로드
const isDragOver = ref(false)
const parsing = ref(false)
const parseResult = ref(null)

const excelVariables = computed(() => {
  return parseResult.value?.columns || []
})

function handleDrop(event) {
  isDragOver.value = false
  const file = event.dataTransfer.files[0]
  if (file) uploadAndParse(file)
}

function handleFileSelect(event) {
  const file = event.target.files[0]
  if (file) uploadAndParse(file)
}

async function uploadAndParse(file) {
  parsing.value = true
  parseResult.value = null
  try {
    const res = await parseExcel(props.conventionId, file)
    parseResult.value = res.data
  } catch (e) {
    console.error('엑셀 파싱 실패:', e)
    alert('엑셀 파싱에 실패했습니다.')
  } finally {
    parsing.value = false
  }
}

// Step 4: 메시지 작성
const messageContent = ref('')
const messageTextarea = ref(null)
const templates = ref([])
const selectedTemplateId = ref(null)

async function loadTemplates() {
  try {
    const res = await apiClient.get('/admin/sms-templates')
    templates.value = res.data
  } catch (e) {
    console.error('템플릿 로드 실패:', e)
  }
}

function applyTemplate() {
  const tpl = templates.value.find((t) => t.id === selectedTemplateId.value)
  if (tpl) {
    messageContent.value = tpl.content
  }
}

function insertVariable(key) {
  const tag = `#{${key}}`
  const textarea = messageTextarea.value
  if (textarea) {
    const start = textarea.selectionStart
    const end = textarea.selectionEnd
    const text = messageContent.value
    messageContent.value = text.slice(0, start) + tag + text.slice(end)
    nextTick(() => {
      textarea.focus()
      textarea.selectionStart = textarea.selectionEnd = start + tag.length
    })
  } else {
    messageContent.value += tag
  }
}

const previewMessage = computed(() => {
  if (!parseResult.value || parseResult.value.recipients.length === 0) return ''
  const first = parseResult.value.recipients[0]
  let msg = messageContent.value
  // 고정 변수 치환 (클라이언트 미리보기용)
  msg = msg.replace(/#{guest_name}/g, first.name)
  msg = msg.replace(/#{guest_phone}/g, first.phone)
  // 엑셀 변수 치환
  for (const [key, value] of Object.entries(first.variables)) {
    msg = msg.replace(new RegExp(`#\\{${key}\\}`, 'g'), value)
  }
  return msg
})

function goToStep4() {
  step.value = 4
}

// Step 5: 발송
const sending = ref(false)
const sendProgress = ref({ current: 0, total: 0 })
const sendResult = ref(null)

const sendProgressPercent = computed(() => {
  if (sendProgress.value.total === 0) return 0
  return Math.round(
    (sendProgress.value.current / sendProgress.value.total) * 100,
  )
})

function goToStep5() {
  step.value = 5
  sendResult.value = null
  sendProgress.value = { current: 0, total: 0 }
}

async function confirmAndSend() {
  const count = parseResult.value.recipients.length
  if (
    !confirm(
      `${count}명에게 개인화된 문자를 발송하시겠습니까?\n이 작업은 취소할 수 없습니다.`,
    )
  )
    return

  sending.value = true
  sendResult.value = null
  sendProgress.value = { current: 0, total: count }

  try {
    const res = await sendWithExcel(
      props.conventionId,
      messageContent.value,
      parseResult.value.recipients,
    )
    sendResult.value = res.data

    // 진행률 완료 처리
    sendProgress.value = { current: count, total: count }
  } catch (e) {
    console.error('발송 실패:', e)
    sendResult.value = {
      successCount: 0,
      failCount: 1,
      failedItems: [
        {
          name: '-',
          phone: '-',
          reason: e.response?.data?.message || e.message,
        },
      ],
    }
  } finally {
    sending.value = false
  }
}

onMounted(() => {
  loadGroups()
  loadTemplates()
})
</script>
