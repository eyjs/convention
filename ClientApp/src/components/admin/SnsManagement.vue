<template>
  <div class="space-y-6">
    <!-- 탭 -->
    <div class="bg-white rounded-lg shadow">
      <div class="border-b">
        <nav class="flex -mb-px">
          <button
            v-for="tab in tabs"
            :key="tab.key"
            class="px-6 py-3 text-sm font-medium border-b-2 transition-colors"
            :class="
              activeTab === tab.key
                ? 'border-primary-500 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            "
            @click="activeTab = tab.key"
          >
            {{ tab.label }}
          </button>
        </nav>
      </div>
    </div>

    <!-- 문자 발송 탭 (엑셀 기반 단발성) -->
    <div v-if="activeTab === 'sms'" class="space-y-4">
      <!-- 1. 엑셀 업로드 -->
      <div class="bg-white rounded-lg shadow p-4">
        <div class="flex items-start justify-between mb-3">
          <div>
            <h3 class="font-semibold text-gray-900">수신자 엑셀 업로드</h3>
            <p class="text-xs text-gray-500 mt-1">
              A열: 번호 · B열: 이름 · C열: 전화번호 · D열부터: 헤더에
              <code class="text-blue-600">#{변수명}</code> 패턴
            </p>
          </div>
          <button
            class="text-xs text-blue-600 hover:underline"
            @click="downloadSample"
          >
            샘플 다운로드
          </button>
        </div>
        <div class="flex items-center gap-3">
          <input
            ref="fileInput"
            type="file"
            accept=".xlsx,.xls"
            class="flex-1 text-sm file:mr-3 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
            @change="handleFileUpload"
          />
          <button
            v-if="recipients.length > 0"
            class="px-3 py-2 text-sm text-red-500 hover:bg-red-50 rounded-lg"
            @click="resetData"
          >
            초기화
          </button>
        </div>
        <div
          v-if="uploadError"
          class="mt-2 p-2 bg-red-50 text-red-600 text-xs rounded"
        >
          {{ uploadError }}
        </div>
        <div
          v-if="recipients.length > 0"
          class="mt-3 flex items-center gap-4 text-sm text-gray-600"
        >
          <span
            >수신자 <strong>{{ recipients.length }}명</strong></span
          >
          <span
            >변수 <strong>{{ dynamicVariables.length }}개</strong></span
          >
        </div>
      </div>

      <!-- 2. 메시지 편집 + 수신자 선택 -->
      <div
        v-if="recipients.length > 0"
        class="grid grid-cols-1 lg:grid-cols-2 gap-4"
      >
        <!-- 편집기 -->
        <div class="bg-white rounded-lg shadow p-4">
          <h3 class="font-semibold text-gray-900 mb-3">메시지 작성</h3>
          <textarea
            ref="smsTextarea"
            v-model="smsContent"
            rows="10"
            class="w-full px-3 py-2 border rounded-lg text-sm font-mono resize-none"
            placeholder="안녕하세요 #{이름}님, 배정된 방은 #{방번호}호 입니다."
          ></textarea>
          <p class="text-xs text-gray-400 mt-1">
            {{ smsContent.length }}자
            <span v-if="smsContent.length > 90" class="text-amber-500"
              >(LMS)</span
            >
            <span v-else class="text-green-500">(SMS)</span>
          </p>

          <!-- 변수 퀵버튼 -->
          <div class="mt-3">
            <label class="block text-xs font-medium text-gray-500 mb-1.5"
              >변수 삽입</label
            >
            <div class="flex flex-wrap gap-1.5">
              <button
                v-for="v in fixedVariables"
                :key="v.key"
                class="px-2 py-1 bg-green-50 text-green-700 text-xs rounded border border-green-200 hover:bg-green-100"
                @click="insertVariable(v.key)"
              >
                {{ v.label }}
              </button>
              <button
                v-for="v in dynamicVariables"
                :key="v"
                class="px-2 py-1 bg-blue-50 text-blue-700 text-xs rounded border border-blue-200 hover:bg-blue-100"
                @click="insertVariable(v)"
              >
                #{{ '{' }}{{ v }}{{ '}' }}
              </button>
            </div>
          </div>

          <!-- 미리보기 -->
          <div v-if="previewRecipient" class="mt-3">
            <label class="block text-xs font-medium text-gray-500 mb-1.5">
              {{ previewRecipient.name }} 미리보기
            </label>
            <div
              class="bg-gray-100 rounded-lg p-3 text-sm font-mono whitespace-pre-wrap min-h-[60px]"
            >
              {{ previewMessage }}
            </div>
          </div>
        </div>

        <!-- 수신자 -->
        <div class="bg-white rounded-lg shadow p-4">
          <div class="flex items-center justify-between mb-3">
            <h3 class="font-semibold text-gray-900">
              수신자 ({{ selectedIds.length }}/{{ recipients.length }})
            </h3>
            <div class="flex gap-2">
              <button
                class="text-xs text-primary-600 hover:underline"
                @click="selectAll"
              >
                전체
              </button>
              <button
                class="text-xs text-gray-500 hover:underline"
                @click="selectedIds = []"
              >
                해제
              </button>
            </div>
          </div>
          <input
            v-model="recipientSearch"
            type="text"
            placeholder="이름/전화번호 검색"
            class="w-full px-3 py-2 border rounded-lg text-sm mb-2"
          />
          <div
            class="max-h-[400px] overflow-y-auto space-y-1 border rounded-lg p-2"
          >
            <label
              v-for="r in filteredRecipients"
              :key="r.no"
              class="flex items-center gap-2 px-2 py-1.5 rounded hover:bg-gray-50 cursor-pointer text-sm"
              @click="previewRecipientNo = r.no"
            >
              <input
                v-model="selectedIds"
                type="checkbox"
                :value="r.no"
                class="rounded border-gray-300 text-primary-500"
                @click.stop
              />
              <span class="text-xs text-gray-400 w-6">{{ r.no }}</span>
              <span class="text-gray-900">{{ r.name }}</span>
              <span class="text-xs text-gray-400 ml-auto">{{ r.phone }}</span>
            </label>
            <div
              v-if="filteredRecipients.length === 0"
              class="text-center text-xs text-gray-400 py-2"
            >
              검색 결과가 없습니다
            </div>
          </div>

          <!-- 발송 -->
          <button
            class="w-full mt-3 px-4 py-3 bg-red-500 text-white rounded-lg font-medium hover:bg-red-600 disabled:opacity-50"
            :disabled="!smsContent || selectedIds.length === 0 || smsSending"
            @click="sendSms"
          >
            {{
              smsSending
                ? '발송 중...'
                : `${selectedIds.length}명에게 문자 발송`
            }}
          </button>

          <div
            v-if="smsSendResult"
            class="mt-3 p-3 rounded-lg text-sm font-medium"
            :class="
              smsSendResult.failCount === 0
                ? 'bg-green-50 text-green-800'
                : 'bg-amber-50 text-amber-800'
            "
          >
            총 {{ smsSendResult.totalCount }}건 중 성공:
            {{ smsSendResult.successCount }}, 실패:
            {{ smsSendResult.failCount }}
            <ul
              v-if="smsSendResult.failedItems?.length > 0"
              class="mt-2 text-xs"
            >
              <li v-for="(f, i) in smsSendResult.failedItems" :key="i">
                • {{ f.name }} ({{ f.phone }}): {{ f.reason }}
              </li>
            </ul>
          </div>
        </div>
      </div>

      <div
        v-else
        class="bg-white rounded-lg shadow p-8 text-center text-gray-400"
      >
        <p>엑셀 파일을 업로드하면 수신자와 변수가 자동으로 준비됩니다</p>
      </div>
    </div>

    <!-- 카카오 알림톡 탭 -->
    <div v-if="activeTab === 'alimtalk'" class="space-y-6">
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- 좌: 팝빌 템플릿 + 설정 -->
        <div class="space-y-4">
          <!-- 잔여 포인트 -->
          <div
            class="bg-white rounded-lg shadow p-4 flex items-center justify-between"
          >
            <div>
              <span class="text-sm text-gray-500">팝빌 잔여 포인트</span>
              <p class="text-2xl font-bold text-gray-900">
                {{
                  popbillBalance !== null
                    ? `${popbillBalance.toLocaleString()}원`
                    : '-'
                }}
              </p>
            </div>
            <button
              class="px-3 py-1.5 bg-yellow-100 text-yellow-800 text-xs rounded-lg hover:bg-yellow-200"
              @click="loadPopbillBalance"
            >
              새로고침
            </button>
          </div>

          <!-- 팝빌 템플릿 -->
          <div class="bg-white rounded-lg shadow">
            <div class="p-4 border-b flex items-center justify-between">
              <h3 class="font-semibold text-gray-900">팝빌 알림톡 템플릿</h3>
              <button
                class="px-3 py-1.5 bg-yellow-100 text-yellow-800 text-xs rounded-lg hover:bg-yellow-200"
                @click="loadPopbillTemplates"
              >
                새로고침
              </button>
            </div>
            <div class="p-4 space-y-2 max-h-[400px] overflow-y-auto">
              <div
                v-if="popbillTemplatesLoading"
                class="text-center py-8 text-gray-400 text-sm"
              >
                팝빌 연동 중...
              </div>
              <div
                v-for="tpl in popbillTemplates"
                :key="tpl.templateCode"
                class="p-3 border rounded-lg cursor-pointer hover:border-yellow-300 transition-colors"
                :class="{
                  'border-yellow-500 bg-yellow-50':
                    selectedPopbillTemplate?.templateCode === tpl.templateCode,
                }"
                @click="selectPopbillTemplate(tpl)"
              >
                <div class="flex items-center justify-between mb-1">
                  <span class="font-medium text-sm text-gray-900">{{
                    tpl.templateName
                  }}</span>
                  <span
                    class="text-xs px-2 py-0.5 rounded-full"
                    :class="
                      tpl.state === 'R'
                        ? 'bg-green-100 text-green-700'
                        : 'bg-gray-100 text-gray-500'
                    "
                  >
                    {{ tpl.state === 'R' ? '승인' : tpl.state }}
                  </span>
                </div>
                <p class="text-xs text-gray-500">
                  코드: {{ tpl.templateCode }}
                </p>
              </div>
              <div
                v-if="!popbillTemplatesLoading && popbillTemplates.length === 0"
                class="text-center py-8 text-gray-400 text-sm"
              >
                등록된 팝빌 템플릿이 없습니다
              </div>
              <div
                v-if="popbillError"
                class="text-center py-4 text-red-500 text-sm"
              >
                {{ popbillError }}
              </div>
            </div>
          </div>
        </div>

        <!-- 우: 발송 -->
        <div class="bg-white rounded-lg shadow">
          <div class="p-4 border-b">
            <h3 class="font-semibold text-gray-900">알림톡 발송</h3>
          </div>
          <div class="p-4 space-y-4">
            <!-- 선택된 템플릿 내용 (승인 템플릿이므로 수정 불가) -->
            <div>
              <div class="flex items-center justify-between mb-1">
                <label class="text-sm font-medium text-gray-700"
                  >발송 내용</label
                >
                <span
                  v-if="selectedPopbillTemplate"
                  class="text-xs text-amber-600"
                  >승인된 템플릿 — 내용 수정 불가</span
                >
              </div>
              <div
                v-if="alimtalkContent"
                class="w-full px-3 py-2 border rounded-lg text-sm font-mono bg-gray-50 whitespace-pre-wrap min-h-[120px]"
              >
                {{ alimtalkContent }}
              </div>
              <div
                v-else
                class="w-full px-3 py-2 border rounded-lg text-sm text-gray-400 min-h-[120px] flex items-center justify-center"
              >
                좌측에서 팝빌 템플릿을 선택하세요
              </div>
              <p v-if="alimtalkContent" class="text-xs text-gray-400 mt-1">
                변수(#{...})는 발송 시 참석자 정보로 자동 치환됩니다
              </p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1"
                >대체문자 (선택)</label
              >
              <textarea
                v-model="alimtalkAltContent"
                rows="2"
                class="w-full px-3 py-2 border rounded-lg text-sm font-mono resize-none"
                placeholder="알림톡 수신 실패 시 SMS 발송 (선택)"
              ></textarea>
            </div>

            <!-- 수신자 -->
            <div>
              <div class="flex items-center justify-between mb-2">
                <label class="text-sm font-medium text-gray-700">
                  수신자 ({{ alimtalkSelectedIds.length }}명)
                </label>
                <div class="flex gap-2">
                  <button
                    class="text-xs text-yellow-600 hover:underline"
                    @click="alimtalkSelectAll"
                  >
                    전체
                  </button>
                  <button
                    class="text-xs text-gray-500 hover:underline"
                    @click="alimtalkSelectedIds = []"
                  >
                    해제
                  </button>
                </div>
              </div>
              <input
                v-model="alimtalkSearch"
                type="text"
                placeholder="이름/전화번호/부서 검색"
                class="w-full px-3 py-2 border rounded-lg text-sm mb-2"
              />
              <div
                class="max-h-[200px] overflow-y-auto space-y-1 border rounded-lg p-2"
              >
                <label
                  v-for="user in alimtalkFilteredGuests"
                  :key="user.id"
                  class="flex items-center gap-2 px-2 py-1.5 rounded hover:bg-gray-50 cursor-pointer text-sm"
                >
                  <input
                    v-model="alimtalkSelectedIds"
                    type="checkbox"
                    :value="user.id"
                    class="rounded border-gray-300 text-yellow-500"
                  />
                  <span class="text-gray-900">{{ user.guestName }}</span>
                  <span class="text-xs text-gray-400">{{
                    user.telephone
                  }}</span>
                  <span v-if="user.corpPart" class="text-xs text-gray-400"
                    >· {{ user.corpPart }}</span
                  >
                </label>
                <div
                  v-if="alimtalkFilteredGuests.length === 0"
                  class="text-center text-xs text-gray-400 py-2"
                >
                  검색 결과가 없습니다
                </div>
              </div>
            </div>

            <button
              class="w-full py-3 bg-yellow-500 text-white rounded-lg font-medium hover:bg-yellow-600 disabled:opacity-50 transition-colors"
              :disabled="
                !selectedPopbillTemplate ||
                !alimtalkContent ||
                alimtalkSelectedIds.length === 0 ||
                alimtalkSending
              "
              @click="sendAlimtalk"
            >
              {{
                alimtalkSending
                  ? '발송 중...'
                  : `${alimtalkSelectedIds.length}명에게 알림톡 발송`
              }}
            </button>
            <div
              v-if="alimtalkResult"
              class="p-3 rounded-lg text-sm font-medium"
              :class="
                alimtalkResult.failCount === 0
                  ? 'bg-green-50 text-green-800'
                  : 'bg-amber-50 text-amber-800'
              "
            >
              {{ alimtalkResult.message }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 발송 이력 탭 -->
    <div v-if="activeTab === 'logs'" class="bg-white rounded-lg shadow">
      <div class="p-4 border-b">
        <h3 class="font-semibold text-gray-900">발송 이력</h3>
      </div>
      <div class="p-4">
        <div
          v-if="smsHistory.length === 0"
          class="text-center py-12 text-gray-400"
        >
          발송 이력이 없습니다
        </div>
        <div v-else class="space-y-2 max-h-[600px] overflow-y-auto">
          <div
            v-for="sms in smsHistory"
            :key="sms.id"
            class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
          >
            <div class="flex-1 min-w-0 mr-4">
              <div class="flex justify-between mb-1">
                <span class="font-medium text-sm text-gray-900">{{
                  sms.receiverName
                }}</span>
                <span class="text-xs text-gray-500">{{ sms.sentAt }}</span>
              </div>
              <p class="text-sm text-gray-600 whitespace-pre-wrap line-clamp-2">
                {{ sms.message }}
              </p>
            </div>
            <span
              class="px-2 py-0.5 text-xs rounded-full font-medium"
              :class="
                sms.externalId
                  ? 'bg-green-100 text-green-700'
                  : 'bg-gray-100 text-gray-500'
              "
            >
              {{ sms.externalId ? '성공' : '대기' }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import apiClient from '@/services/api'
import { useRoute } from 'vue-router'

const route = useRoute()
const conventionId = route.params.id

const tabs = [
  { key: 'sms', label: '문자 발송' },
  { key: 'alimtalk', label: '카카오 알림톡' },
  { key: 'logs', label: '발송 이력' },
]
const activeTab = ref('sms')

// --- 알림톡 탭용 공통: 참석자 목록 ---
const guests = ref([])
const guestsLoading = ref(false)
const smsHistory = ref([])

async function loadGuests() {
  guestsLoading.value = true
  try {
    const res = await apiClient.get(`/admin/conventions/${conventionId}/guests`)
    guests.value = res.data
  } catch (e) {
    console.error('참석자 로드 실패:', e)
  } finally {
    guestsLoading.value = false
  }
}

async function loadSmsHistory() {
  try {
    const res = await apiClient.get(
      `/admin/conventions/${conventionId}/sms-logs`,
    )
    smsHistory.value = res.data || []
  } catch {
    smsHistory.value = []
  }
}

// ============================================================
// SMS — 엑셀 기반 단발성 발송
// ============================================================

// 고정 변수 (엑셀 B, C열 기반)
const fixedVariables = [
  { key: '이름', label: '#{이름}' },
  { key: '전화번호', label: '#{전화번호}' },
]

// 메모리 상태 (페이지 새로고침 시 초기화)
const recipients = ref([]) // [{ no, name, phone, vars: {} }]
const dynamicVariables = ref([]) // ['방번호', '식사', ...]
const smsContent = ref('')
const smsTextarea = ref(null)
const fileInput = ref(null)
const uploadError = ref('')
const selectedIds = ref([])
const recipientSearch = ref('')
const previewRecipientNo = ref(null)
const smsSending = ref(false)
const smsSendResult = ref(null)

const filteredRecipients = computed(() => {
  const kw = recipientSearch.value.trim().toLowerCase()
  if (!kw) return recipients.value
  return recipients.value.filter(
    (r) =>
      (r.name || '').toLowerCase().includes(kw) || (r.phone || '').includes(kw),
  )
})

const previewRecipient = computed(() => {
  if (previewRecipientNo.value == null) return null
  return recipients.value.find((r) => r.no === previewRecipientNo.value) || null
})

const previewMessage = computed(() => {
  if (!previewRecipient.value) return ''
  return renderMessage(smsContent.value, previewRecipient.value)
})

// 변수 치환
function renderMessage(template, recipient) {
  return template.replace(/#\{([^}]+)\}/g, (match, key) => {
    if (key === '이름') return recipient.name || ''
    if (key === '전화번호') return recipient.phone || ''
    return recipient.vars[key] ?? match
  })
}

// 엑셀 업로드 처리
async function handleFileUpload(event) {
  const file = event.target.files?.[0]
  if (!file) return
  uploadError.value = ''

  try {
    const XLSX = await import('xlsx')
    const data = await file.arrayBuffer()
    // cellText: false + raw: false → 모든 셀 값을 formatted string으로 읽어 앞 0 보존
    const workbook = XLSX.read(data, { cellText: false })
    const sheet = workbook.Sheets[workbook.SheetNames[0]]
    const rows = XLSX.utils.sheet_to_json(sheet, {
      header: 1,
      defval: '',
      raw: false,
    })

    if (rows.length < 2) {
      uploadError.value = '데이터가 없습니다 (헤더 + 데이터 행 필요)'
      return
    }

    const header = rows[0]
    if (header.length < 3) {
      uploadError.value = '최소 A(번호), B(이름), C(전화번호) 열이 필요합니다'
      return
    }

    // D열부터 #{...} 패턴 헤더를 변수로 파싱
    const varNames = []
    const varColIndices = []
    const duplicates = []
    for (let i = 3; i < header.length; i++) {
      const cell = String(header[i] ?? '').trim()
      const m = cell.match(/^#\{([^}]+)\}$/)
      if (m) {
        const key = m[1]
        if (varNames.includes(key)) {
          duplicates.push(key)
          continue
        }
        varNames.push(key)
        varColIndices.push(i)
      }
    }

    if (duplicates.length > 0) {
      uploadError.value = `중복된 변수명: ${duplicates.join(', ')} (첫 번째 컬럼만 사용됩니다)`
      // 계속 진행
    }

    // 데이터 행 파싱
    const parsed = []
    for (let r = 1; r < rows.length; r++) {
      const row = rows[r]
      const name = String(row[1] ?? '').trim()
      // 전화번호 정규화: 숫자만 남기기 (공백/하이픈/괄호/+ 제거)
      const phone = String(row[2] ?? '').replace(/\D/g, '')
      if (!name && !phone) continue // 빈 행 스킵

      const vars = {}
      varNames.forEach((vn, idx) => {
        vars[vn] = String(row[varColIndices[idx]] ?? '').trim()
      })

      parsed.push({
        no: parsed.length + 1,
        name,
        phone,
        vars,
      })
    }

    if (parsed.length === 0) {
      uploadError.value = '유효한 데이터 행이 없습니다'
      return
    }

    recipients.value = parsed
    dynamicVariables.value = varNames
    selectedIds.value = parsed.map((r) => r.no)
    previewRecipientNo.value = parsed[0].no
    smsSendResult.value = null
  } catch (e) {
    console.error('엑셀 파싱 실패:', e)
    uploadError.value = '엑셀 파일을 읽을 수 없습니다: ' + e.message
  }
}

function resetData() {
  recipients.value = []
  dynamicVariables.value = []
  selectedIds.value = []
  previewRecipientNo.value = null
  smsContent.value = ''
  smsSendResult.value = null
  uploadError.value = ''
  if (fileInput.value) fileInput.value.value = ''
}

function insertVariable(key) {
  const tag = `#{${key}}`
  const textarea = smsTextarea.value
  if (textarea) {
    const start = textarea.selectionStart
    const end = textarea.selectionEnd
    smsContent.value =
      smsContent.value.slice(0, start) + tag + smsContent.value.slice(end)
    nextTick(() => {
      textarea.focus()
      textarea.selectionStart = textarea.selectionEnd = start + tag.length
    })
  } else {
    smsContent.value += tag
  }
}

function selectAll() {
  selectedIds.value = recipients.value.map((r) => r.no)
}

async function downloadSample() {
  try {
    const XLSX = await import('xlsx')
    const header = ['번호', '이름', '전화번호', '#{방번호}', '#{식사}', '#{조}']
    const rows = [
      header,
      [1, '홍길동', '01012345678', '301', '한식', 'A조'],
      [2, '김철수', '01087654321', '302', '양식', 'B조'],
    ]
    const ws = XLSX.utils.aoa_to_sheet(rows)
    const wb = XLSX.utils.book_new()
    XLSX.utils.book_append_sheet(wb, ws, '수신자')
    XLSX.writeFile(wb, '문자발송_샘플.xlsx')
  } catch (e) {
    console.error('샘플 다운로드 실패:', e)
  }
}

async function sendSms() {
  const targets = recipients.value.filter((r) =>
    selectedIds.value.includes(r.no),
  )
  if (targets.length === 0) return
  if (!confirm(`${targets.length}명에게 문자를 발송하시겠습니까?`)) return

  smsSending.value = true
  smsSendResult.value = null

  try {
    // 클라이언트에서 변수 치환 후 백엔드 전송
    const payload = {
      recipients: targets.map((r) => ({
        name: r.name,
        phone: r.phone,
        message: renderMessage(smsContent.value, r),
      })),
    }

    const res = await apiClient.post(
      `/admin/conventions/${conventionId}/sms/send-direct`,
      payload,
    )
    smsSendResult.value = res.data
  } catch (e) {
    smsSendResult.value = {
      totalCount: targets.length,
      successCount: 0,
      failCount: targets.length,
      failedItems: [
        {
          name: '',
          phone: '',
          reason: e.response?.data?.message || e.message,
        },
      ],
    }
  } finally {
    smsSending.value = false
  }
}

// --- 알림톡 ---
const popbillTemplates = ref([])
const popbillTemplatesLoading = ref(false)
const popbillError = ref('')
const popbillBalance = ref(null)
const selectedPopbillTemplate = ref(null)
const alimtalkContent = ref('')
const alimtalkAltContent = ref('')
const alimtalkSelectedIds = ref([])
const alimtalkSearch = ref('')
const alimtalkFilteredGuests = computed(() => {
  const kw = alimtalkSearch.value.trim().toLowerCase()
  if (!kw) return guests.value
  return guests.value.filter(
    (g) =>
      (g.guestName || '').toLowerCase().includes(kw) ||
      (g.telephone || '').includes(kw) ||
      (g.corpPart || '').toLowerCase().includes(kw),
  )
})
const alimtalkSending = ref(false)
const alimtalkResult = ref(null)

async function loadPopbillTemplates() {
  popbillTemplatesLoading.value = true
  popbillError.value = ''
  try {
    const res = await apiClient.get('/admin/alimtalk/templates')
    popbillTemplates.value = res.data
  } catch (e) {
    popbillError.value = e.response?.data?.message || '팝빌 연동 실패'
    popbillTemplates.value = []
  } finally {
    popbillTemplatesLoading.value = false
  }
}

async function loadPopbillBalance() {
  try {
    const res = await apiClient.get('/admin/alimtalk/balance')
    popbillBalance.value = res.data.balance
  } catch {
    popbillBalance.value = null
  }
}

function selectPopbillTemplate(tpl) {
  selectedPopbillTemplate.value = tpl
  alimtalkContent.value = tpl.template || ''
}

function alimtalkSelectAll() {
  alimtalkSelectedIds.value = guests.value
    .filter((g) => g.telephone)
    .map((g) => g.id)
}

async function sendAlimtalk() {
  if (
    !confirm(
      `${alimtalkSelectedIds.value.length}명에게 알림톡을 발송하시겠습니까?`,
    )
  )
    return
  alimtalkSending.value = true
  alimtalkResult.value = null
  try {
    const res = await apiClient.post(
      `/admin/conventions/${conventionId}/alimtalk/send`,
      {
        templateCode: selectedPopbillTemplate.value.templateCode,
        content: alimtalkContent.value,
        altContent: alimtalkAltContent.value || null,
        targetUserIds: alimtalkSelectedIds.value,
      },
    )
    alimtalkResult.value = res.data
  } catch (e) {
    alimtalkResult.value = {
      message: e.response?.data?.message || '발송 실패',
      failCount: 1,
    }
  } finally {
    alimtalkSending.value = false
  }
}

onMounted(() => {
  loadGuests()
  loadSmsHistory()
  loadPopbillTemplates()
  loadPopbillBalance()
})
</script>
