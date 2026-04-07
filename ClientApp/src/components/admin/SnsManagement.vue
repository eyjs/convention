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

    <!-- 문자 발송 탭 -->
    <div v-if="activeTab === 'sms'" class="space-y-6">
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- 좌: 템플릿 목록 -->
        <div class="bg-white rounded-lg shadow">
          <div class="p-4 border-b flex items-center justify-between">
            <h3 class="font-semibold text-gray-900">문자 템플릿</h3>
            <button
              class="px-3 py-1.5 bg-primary-500 text-white text-xs rounded-lg hover:bg-primary-600"
              @click="startNewSmsTemplate"
            >
              + 새 템플릿
            </button>
          </div>
          <div class="p-4 space-y-2 max-h-[500px] overflow-y-auto">
            <div
              v-if="smsTemplatesLoading"
              class="text-center py-8 text-gray-400"
            >
              로딩 중...
            </div>
            <div
              v-for="tpl in smsTemplates"
              :key="tpl.id"
              class="p-3 border rounded-lg cursor-pointer hover:border-primary-300 transition-colors"
              :class="{
                'border-primary-500 bg-primary-50':
                  selectedSmsTemplate?.id === tpl.id,
              }"
              @click="selectSmsTemplate(tpl)"
            >
              <div class="flex items-center justify-between mb-1">
                <span class="font-medium text-sm text-gray-900">{{
                  tpl.title
                }}</span>
                <button
                  class="text-xs text-gray-400 hover:text-primary-600"
                  @click.stop="editSmsTemplate(tpl)"
                >
                  수정
                </button>
              </div>
              <p class="text-xs text-gray-500 line-clamp-2">
                {{ tpl.content }}
              </p>
            </div>
            <div
              v-if="!smsTemplatesLoading && smsTemplates.length === 0"
              class="text-center py-8 text-gray-400 text-sm"
            >
              등록된 템플릿이 없습니다
            </div>
          </div>
        </div>

        <!-- 중: 편집기 -->
        <div class="bg-white rounded-lg shadow">
          <div class="p-4 border-b">
            <h3 class="font-semibold text-gray-900">
              {{
                smsEditingId
                  ? '템플릿 수정'
                  : selectedSmsTemplate
                    ? '발송 내용'
                    : '새 템플릿'
              }}
            </h3>
          </div>
          <div class="p-4 space-y-4">
            <div v-if="smsEditMode">
              <label class="block text-sm font-medium text-gray-700 mb-1"
                >템플릿명</label
              >
              <input
                v-model="smsEditForm.title"
                type="text"
                class="w-full px-3 py-2 border rounded-lg text-sm"
                placeholder="예: 행사 안내 문자"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1"
                >내용</label
              >
              <textarea
                ref="smsTextarea"
                v-model="smsContent"
                rows="8"
                class="w-full px-3 py-2 border rounded-lg text-sm font-mono resize-none"
                placeholder="안녕하세요 #{guest_name}님, #{title} 안내드립니다."
              ></textarea>
              <p class="text-xs text-gray-400 mt-1">
                {{ smsContent.length }}자
                <span v-if="smsContent.length > 90" class="text-amber-500"
                  >(LMS)</span
                >
                <span v-else class="text-green-500">(SMS)</span>
              </p>
            </div>
            <!-- 변수 -->
            <div>
              <label class="block text-xs font-medium text-gray-500 mb-1.5"
                >변수 삽입</label
              >
              <div class="flex flex-wrap gap-1.5">
                <button
                  v-for="v in templateVariables"
                  :key="v.key"
                  class="px-2 py-1 bg-blue-50 text-blue-700 text-xs rounded border border-blue-200 hover:bg-blue-100"
                  @click="insertSmsVariable(v.key)"
                >
                  {{ v.label }}
                </button>
              </div>
            </div>
            <div v-if="smsEditMode" class="flex gap-2">
              <button
                class="px-3 py-2 bg-gray-100 text-gray-700 rounded-lg text-sm"
                @click="cancelSmsEdit"
              >
                취소
              </button>
              <button
                class="px-3 py-2 bg-primary-500 text-white rounded-lg text-sm"
                :disabled="!smsEditForm.title || !smsContent"
                @click="saveSmsTemplate"
              >
                저장
              </button>
            </div>
          </div>
        </div>

        <!-- 우: 수신자 + 발송 -->
        <div class="bg-white rounded-lg shadow">
          <div class="p-4 border-b flex items-center justify-between">
            <h3 class="font-semibold text-gray-900">
              수신자 ({{ smsSelectedIds.length }}명)
            </h3>
            <div class="flex gap-2">
              <button
                class="text-xs text-primary-600 hover:underline"
                @click="smsSelectAll"
              >
                전체
              </button>
              <button
                class="text-xs text-gray-500 hover:underline"
                @click="smsSelectedIds = []"
              >
                해제
              </button>
            </div>
          </div>
          <div class="p-4">
            <div
              v-if="guestsLoading"
              class="text-center py-4 text-gray-400 text-sm"
            >
              로딩 중...
            </div>
            <template v-else>
              <input
                v-model="smsSearch"
                type="text"
                placeholder="이름/전화번호/부서 검색"
                class="w-full px-3 py-2 border rounded-lg text-sm mb-2"
              />
              <div
                class="max-h-[300px] overflow-y-auto space-y-1 border rounded-lg p-2 mb-4"
              >
                <label
                  v-for="user in smsFilteredGuests"
                  :key="user.id"
                  class="flex items-center gap-2 px-2 py-1.5 rounded hover:bg-gray-50 cursor-pointer text-sm"
                >
                  <input
                    v-model="smsSelectedIds"
                    type="checkbox"
                    :value="user.id"
                    class="rounded border-gray-300 text-primary-500"
                  />
                  <span class="text-gray-900">{{ user.guestName }}</span>
                  <span class="text-xs text-gray-400">{{
                    user.telephone
                  }}</span>
                  <span v-if="user.corpPart" class="text-xs text-gray-400"
                    >· {{ user.corpPart }}</span
                  >
                  <span v-if="!user.telephone" class="text-xs text-red-400"
                    >(번호없음)</span
                  >
                </label>
                <div
                  v-if="smsFilteredGuests.length === 0"
                  class="text-center text-xs text-gray-400 py-2"
                >
                  검색 결과가 없습니다
                </div>
              </div>
            </template>
            <!-- 미리보기 -->
            <div
              v-if="smsPreview"
              class="mb-4 bg-gray-100 rounded-lg p-3 text-sm font-mono whitespace-pre-wrap"
            >
              {{ smsPreview }}
            </div>
            <div class="flex gap-2">
              <button
                class="flex-1 px-3 py-2 bg-gray-100 text-gray-700 rounded-lg text-sm"
                :disabled="!smsContent || smsSelectedIds.length === 0"
                @click="previewSms"
              >
                미리보기
              </button>
              <button
                class="flex-1 px-3 py-2 bg-red-500 text-white rounded-lg text-sm disabled:opacity-50"
                :disabled="
                  !smsContent || smsSelectedIds.length === 0 || smsSending
                "
                @click="sendSms"
              >
                {{
                  smsSending ? '발송 중...' : `${smsSelectedIds.length}명 발송`
                }}
              </button>
            </div>
            <div
              v-if="smsSendResult"
              class="mt-3 p-3 rounded-lg text-sm font-medium"
              :class="
                smsSendResult.failCount === 0
                  ? 'bg-green-50 text-green-800'
                  : 'bg-amber-50 text-amber-800'
              "
            >
              {{ smsSendResult.message }}
            </div>
          </div>
        </div>
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

const templateVariables = [
  { key: 'guest_name', label: '참석자명' },
  { key: 'guest_phone', label: '전화번호' },
  { key: 'corp_part', label: '부서' },
  { key: 'title', label: '행사명' },
  { key: 'start_date', label: '시작일' },
  { key: 'end_date', label: '종료일' },
  { key: 'url', label: '접속URL' },
]

// --- 공통 데이터 ---
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

// --- SMS ---
const smsTemplates = ref([])
const smsTemplatesLoading = ref(false)
const selectedSmsTemplate = ref(null)
const smsEditMode = ref(false)
const smsEditingId = ref(null)
const smsEditForm = ref({ title: '', content: '' })
const smsContent = ref('')
const smsTextarea = ref(null)
const smsSelectedIds = ref([])
const smsSearch = ref('')
const smsFilteredGuests = computed(() => {
  const kw = smsSearch.value.trim().toLowerCase()
  if (!kw) return guests.value
  return guests.value.filter(
    (g) =>
      (g.guestName || '').toLowerCase().includes(kw) ||
      (g.telephone || '').includes(kw) ||
      (g.corpPart || '').toLowerCase().includes(kw),
  )
})
const smsPreview = ref('')
const smsSending = ref(false)
const smsSendResult = ref(null)

async function loadSmsTemplates() {
  smsTemplatesLoading.value = true
  try {
    const res = await apiClient.get('/admin/sms-templates')
    smsTemplates.value = res.data
  } catch (e) {
    console.error('SMS 템플릿 로드 실패:', e)
  } finally {
    smsTemplatesLoading.value = false
  }
}

function selectSmsTemplate(tpl) {
  selectedSmsTemplate.value = tpl
  smsContent.value = tpl.content
  smsEditMode.value = false
}

function startNewSmsTemplate() {
  smsEditingId.value = null
  smsEditForm.value = { title: '', content: '' }
  smsContent.value = ''
  smsEditMode.value = true
}

function editSmsTemplate(tpl) {
  smsEditingId.value = tpl.id
  smsEditForm.value = { title: tpl.title, content: tpl.content }
  smsContent.value = tpl.content
  smsEditMode.value = true
}

function cancelSmsEdit() {
  smsEditMode.value = false
  if (selectedSmsTemplate.value) {
    smsContent.value = selectedSmsTemplate.value.content
  }
}

async function saveSmsTemplate() {
  try {
    const payload = {
      title: smsEditForm.value.title,
      content: smsContent.value,
    }
    if (smsEditingId.value) {
      await apiClient.put(`/admin/sms-templates/${smsEditingId.value}`, payload)
      const idx = smsTemplates.value.findIndex(
        (t) => t.id === smsEditingId.value,
      )
      if (idx >= 0)
        smsTemplates.value[idx] = { ...smsTemplates.value[idx], ...payload }
    } else {
      const res = await apiClient.post('/admin/sms-templates', payload)
      smsTemplates.value.unshift(res.data)
      selectedSmsTemplate.value = res.data
    }
    smsEditMode.value = false
  } catch (e) {
    alert('템플릿 저장 실패')
  }
}

function insertSmsVariable(key) {
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

function smsSelectAll() {
  smsSelectedIds.value = guests.value
    .filter((g) => g.telephone)
    .map((g) => g.id)
}

async function previewSms() {
  if (smsSelectedIds.value.length === 0) return
  try {
    const res = await apiClient.post(
      `/admin/conventions/${conventionId}/sms/preview`,
      {
        content: smsContent.value,
        targetUserId: smsSelectedIds.value[0],
      },
    )
    smsPreview.value = res.data.previewMessage
  } catch {
    smsPreview.value = '미리보기 실패'
  }
}

async function sendSms() {
  if (!confirm(`${smsSelectedIds.value.length}명에게 문자를 발송하시겠습니까?`))
    return
  smsSending.value = true
  smsSendResult.value = null
  try {
    const res = await apiClient.post(
      `/admin/conventions/${conventionId}/sms/send`,
      {
        content: smsContent.value,
        targetUserIds: smsSelectedIds.value,
      },
    )
    smsSendResult.value = res.data
    loadSmsHistory()
  } catch (e) {
    smsSendResult.value = {
      message: e.response?.data?.message || '발송 실패',
      failCount: 1,
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
  loadSmsTemplates()
  loadSmsHistory()
  loadPopbillTemplates()
  loadPopbillBalance()
})
</script>
