<template>
  <div
    class="fixed inset-0 z-50 overflow-y-auto"
    role="dialog"
    aria-modal="true"
  >
    <div
      class="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0"
    >
      <div
        class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
        @click="emit('close')"
      ></div>
      <span class="hidden sm:inline-block sm:align-middle sm:h-screen"
        >&#8203;</span
      >

      <div
        class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all w-full sm:my-8 sm:align-middle sm:max-w-3xl sm:w-full"
      >
        <!-- Header -->
        <div class="px-4 sm:px-6 py-3 sm:py-4 border-b">
          <div class="flex items-center justify-between mb-3">
            <h3 class="text-base sm:text-lg font-semibold text-gray-900">
              문자 발송
            </h3>
            <button
              class="text-gray-400 hover:text-gray-600"
              @click="emit('close')"
            >
              <svg
                class="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>
          </div>

          <!-- 탭 -->
          <div class="flex border-b -mb-px">
            <button
              v-for="tab in tabs"
              :key="tab.key"
              class="px-4 py-2 text-sm font-medium border-b-2 transition-colors"
              :class="
                activeTab === tab.key
                  ? 'border-primary-500 text-primary-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              "
              @click="activeTab = tab.key"
            >
              {{ tab.label }}
            </button>
          </div>

          <!-- 기존 발송 단계 표시 (기존 탭일 때만) -->
          <div
            v-if="activeTab === 'default'"
            class="flex items-center gap-1 text-sm mt-3"
          >
            <span v-for="s in 3" :key="s" class="flex items-center gap-1">
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
                v-if="s < 3"
                class="w-4 h-px"
                :class="step > s ? 'bg-green-400' : 'bg-gray-300'"
              ></span>
            </span>
          </div>
        </div>

        <!-- Body -->
        <div
          class="p-4 sm:p-6 bg-gray-50 min-h-[300px] sm:min-h-[480px] max-h-[60vh] sm:max-h-[70vh] overflow-y-auto"
        >
          <!-- 그룹 단체 발송 탭 -->
          <SmsGroupSendTab
            v-if="activeTab === 'group'"
            :convention-id="props.conventionId"
          />

          <!-- 엑셀 변수 발송 탭 -->
          <SmsExcelSendTab
            v-if="activeTab === 'excel'"
            :convention-id="props.conventionId"
          />

          <!-- 기존 발송 (default 탭) -->
          <!-- Step 1: 템플릿 선택/편집 -->
          <div v-if="activeTab === 'default' && step === 1">
            <div class="flex items-center justify-between mb-4">
              <h4 class="font-semibold text-gray-800">문자 템플릿</h4>
              <button
                class="px-3 py-1.5 bg-primary-500 text-white text-sm rounded-lg hover:bg-primary-600 transition-colors"
                @click="startNewTemplate"
              >
                + 새 템플릿
              </button>
            </div>

            <!-- 템플릿 목록 -->
            <div
              v-if="!isEditing && templates.length > 0"
              class="space-y-2 mb-4"
            >
              <div
                v-for="tpl in templates"
                :key="tpl.id"
                class="bg-white border rounded-lg p-4 hover:border-primary-300 transition-colors cursor-pointer"
                :class="{
                  'border-primary-500 ring-1 ring-primary-500':
                    selectedTemplate?.id === tpl.id,
                }"
                @click="selectTemplate(tpl)"
              >
                <div class="flex items-center justify-between mb-1">
                  <span class="font-medium text-gray-900">{{ tpl.title }}</span>
                  <div class="flex items-center gap-2">
                    <button
                      class="text-xs text-gray-500 hover:text-primary-600"
                      @click.stop="editTemplate(tpl)"
                    >
                      수정
                    </button>
                  </div>
                </div>
                <p class="text-sm text-gray-500 line-clamp-2">
                  {{ tpl.content }}
                </p>
              </div>
            </div>

            <div
              v-if="!isEditing && templates.length === 0 && !templatesLoading"
              class="text-center py-8 text-gray-400"
            >
              등록된 템플릿이 없습니다
            </div>

            <div v-if="templatesLoading" class="text-center py-8 text-gray-400">
              로딩 중...
            </div>

            <!-- 템플릿 편집 폼 -->
            <div v-if="isEditing" class="bg-white border rounded-lg p-4">
              <h5 class="font-medium text-gray-800 mb-3">
                {{ editingId ? '템플릿 수정' : '새 템플릿' }}
              </h5>
              <div class="space-y-3">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1"
                    >템플릿명</label
                  >
                  <input
                    v-model="editForm.title"
                    type="text"
                    class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    placeholder="예: 행사 안내 문자"
                  />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1"
                    >내용</label
                  >
                  <textarea
                    ref="contentTextarea"
                    v-model="editForm.content"
                    rows="6"
                    class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 resize-none font-mono text-sm"
                    placeholder="안녕하세요 #{guest_name}님, #{title} 안내드립니다."
                  ></textarea>
                  <p class="text-xs text-gray-400 mt-1">
                    {{ editForm.content.length }}자
                    <span
                      v-if="editForm.content.length > 90"
                      class="text-amber-500"
                    >
                      (장문 LMS)
                    </span>
                  </p>
                </div>

                <!-- 변수 삽입 버튼 -->
                <div>
                  <label class="block text-xs font-medium text-gray-500 mb-1.5"
                    >변수 삽입</label
                  >
                  <div class="flex flex-wrap gap-1.5">
                    <button
                      v-for="v in templateVariables"
                      :key="v.key"
                      class="px-2 py-1 bg-blue-50 text-blue-700 text-xs rounded border border-blue-200 hover:bg-blue-100 transition-colors"
                      @click="insertVariable(v.key)"
                    >
                      {{ v.label }}
                    </button>
                  </div>
                </div>

                <div class="flex gap-2 pt-2">
                  <button
                    class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg text-sm hover:bg-gray-200 transition-colors"
                    @click="cancelEdit"
                  >
                    취소
                  </button>
                  <button
                    class="px-4 py-2 bg-primary-500 text-white rounded-lg text-sm hover:bg-primary-600 transition-colors"
                    :disabled="!editForm.title || !editForm.content"
                    @click="saveTemplate"
                  >
                    저장
                  </button>
                </div>
              </div>
            </div>

            <!-- 직접 입력 옵션 -->
            <div v-if="!isEditing" class="mt-4 border-t pt-4">
              <button
                class="text-sm text-gray-500 hover:text-primary-600"
                @click="startDirectInput"
              >
                템플릿 없이 직접 입력 →
              </button>
            </div>
          </div>

          <!-- Step 2: 내용 확인 + 수신자 선택 -->
          <div v-if="activeTab === 'default' && step === 2">
            <h4 class="font-semibold text-gray-800 mb-4">
              발송 내용 및 수신자
            </h4>

            <!-- 발송 내용 -->
            <div class="bg-white border rounded-lg p-4 mb-4">
              <div class="flex items-center justify-between mb-2">
                <label class="text-sm font-medium text-gray-700"
                  >발송 내용</label
                >
                <span class="text-xs text-gray-400"
                  >{{ messageContent.length }}자</span
                >
              </div>
              <textarea
                ref="messageTextarea"
                v-model="messageContent"
                rows="5"
                class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 resize-none font-mono text-sm"
              ></textarea>
              <!-- 변수 삽입 -->
              <div class="mt-2 flex flex-wrap gap-1.5">
                <button
                  v-for="v in templateVariables"
                  :key="v.key"
                  class="px-2 py-1 bg-blue-50 text-blue-700 text-xs rounded border border-blue-200 hover:bg-blue-100 transition-colors"
                  @click="insertVariableToMessage(v.key)"
                >
                  {{ v.label }}
                </button>
              </div>
            </div>

            <!-- 수��자 -->
            <div class="bg-white border rounded-lg p-4">
              <div class="flex items-center justify-between mb-3">
                <label class="text-sm font-medium text-gray-700"
                  >수신자 ({{ selectedUserIds.length }}명 선택)</label
                >
                <div class="flex items-center gap-2">
                  <button
                    class="text-xs text-primary-600 hover:underline"
                    @click="selectAllUsers"
                  >
                    전체 선택
                  </button>
                  <button
                    class="text-xs text-gray-500 hover:underline"
                    @click="deselectAllUsers"
                  >
                    전체 해���
                  </button>
                </div>
              </div>

              <!-- 그룹 필터 -->
              <div v-if="groupNames.length > 0" class="mb-3">
                <label class="block text-xs font-medium text-gray-500 mb-1.5"
                  >그룹 필터</label
                >
                <div class="flex flex-wrap gap-1.5">
                  <button
                    class="px-2.5 py-1 text-xs rounded-lg border transition-colors"
                    :class="
                      selectedGroupFilter === ''
                        ? 'bg-primary-500 text-white border-primary-500'
                        : 'bg-white text-gray-600 border-gray-200 hover:bg-gray-50'
                    "
                    @click="filterByGroup('')"
                  >
                    전체
                  </button>
                  <button
                    v-for="group in groupNames"
                    :key="group"
                    class="px-2.5 py-1 text-xs rounded-lg border transition-colors"
                    :class="
                      selectedGroupFilter === group
                        ? 'bg-primary-500 text-white border-primary-500'
                        : 'bg-white text-gray-600 border-gray-200 hover:bg-gray-50'
                    "
                    @click="filterByGroup(group)"
                  >
                    {{ group }}
                  </button>
                </div>
              </div>

              <div v-if="usersLoading" class="text-center py-4 text-gray-400">
                참석자 로딩 중...
              </div>

              <div
                v-else
                class="max-h-[30vh] sm:max-h-[50vh] overflow-y-auto space-y-1 border rounded-lg p-2"
              >
                <label
                  v-for="user in filteredGuests"
                  :key="user.userId"
                  class="flex items-center gap-2 px-2 py-1.5 rounded hover:bg-gray-50 cursor-pointer"
                >
                  <input
                    v-model="selectedUserIds"
                    type="checkbox"
                    :value="user.userId"
                    class="rounded border-gray-300 text-primary-500 focus:ring-primary-500"
                  />
                  <span class="text-sm text-gray-900">{{
                    user.guestName
                  }}</span>
                  <span class="text-xs text-gray-400">{{
                    user.telephone
                  }}</span>
                  <span v-if="!user.telephone" class="text-xs text-red-400">
                    (번호없음)
                  </span>
                </label>
              </div>
            </div>
          </div>

          <!-- Step 3: 미리보기 + 발송 -->
          <div v-if="activeTab === 'default' && step === 3">
            <h4 class="font-semibold text-gray-800 mb-4">발송 확인</h4>

            <!-- 요약 -->
            <div class="bg-white border rounded-lg p-4 mb-4">
              <dl class="grid grid-cols-2 gap-3 text-sm">
                <div>
                  <dt class="text-gray-500">수신자 수</dt>
                  <dd class="font-semibold text-gray-900">
                    {{ selectedUserIds.length }}명
                  </dd>
                </div>
                <div>
                  <dt class="text-gray-500">메시지 길이</dt>
                  <dd class="font-semibold text-gray-900">
                    {{ messageContent.length }}자
                    <span
                      class="text-xs font-normal"
                      :class="
                        messageContent.length <= 90
                          ? 'text-green-600'
                          : 'text-amber-600'
                      "
                    >
                      ({{ messageContent.length <= 90 ? 'SMS' : 'LMS' }})
                    </span>
                  </dd>
                </div>
              </dl>
            </div>

            <!-- 미리보기 -->
            <div class="bg-white border rounded-lg p-4 mb-4">
              <div class="flex items-center justify-between mb-2">
                <label class="text-sm font-medium text-gray-700"
                  >미리보기</label
                >
                <select
                  v-model="previewUserId"
                  class="text-xs border rounded px-2 py-1"
                  @change="loadPreview"
                >
                  <option :value="null" disabled>수신자 선택</option>
                  <option
                    v-for="user in selectedUsers"
                    :key="user.userId"
                    :value="user.userId"
                  >
                    {{ user.guestName }}
                  </option>
                </select>
              </div>
              <div
                class="bg-gray-100 rounded-lg p-4 font-mono text-sm whitespace-pre-wrap min-h-[80px]"
              >
                <template v-if="previewLoading">미리보기 로딩 중...</template>
                <template v-else-if="previewMessage">{{
                  previewMessage
                }}</template>
                <span v-else class="text-gray-400"
                  >수신자를 선택하면 실제 발송될 내용을 확인할 수 있습니다</span
                >
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
                  sendResult.failCount === 0
                    ? 'text-green-800'
                    : 'text-amber-800'
                "
              >
                {{ sendResult.message }}
              </p>
            </div>
          </div>
        </div>

        <!-- Footer (기존 발송 탭만) -->
        <div
          v-if="activeTab === 'default'"
          class="px-4 sm:px-6 py-3 sm:py-4 border-t bg-white flex flex-col sm:flex-row sm:justify-between gap-2"
        >
          <button
            v-if="step > 1"
            class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-200 transition-colors order-2 sm:order-1"
            @click="step--"
          >
            이전
          </button>
          <div v-else class="hidden sm:block"></div>

          <div class="flex gap-2 order-1 sm:order-2">
            <button
              class="flex-1 sm:flex-none px-4 py-2 bg-gray-100 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-200 transition-colors"
              @click="emit('close')"
            >
              닫기
            </button>

            <!-- Step 1: 다음 -->
            <button
              v-if="step === 1"
              class="flex-1 sm:flex-none px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors disabled:opacity-50"
              :disabled="!canProceedStep1"
              @click="goToStep2"
            >
              다음
            </button>

            <!-- Step 2: 다음 -->
            <button
              v-if="step === 2"
              class="flex-1 sm:flex-none px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors disabled:opacity-50"
              :disabled="!canProceedStep2"
              @click="goToStep3"
            >
              다음
            </button>

            <!-- Step 3: 발송 -->
            <button
              v-if="step === 3"
              class="flex-1 sm:flex-none px-4 py-2 bg-red-500 text-white rounded-lg text-sm font-medium hover:bg-red-600 transition-colors disabled:opacity-50"
              :disabled="sending"
              @click="confirmAndSend"
            >
              {{
                sending ? '발송 중...' : `${selectedUserIds.length}명에게 발송`
              }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'

import apiClient from '@/services/api'
import SmsGroupSendTab from '@/components/admin/sms/SmsGroupSendTab.vue'
import SmsExcelSendTab from '@/components/admin/sms/SmsExcelSendTab.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
})
const emit = defineEmits(['close'])

// 탭 관리
const tabs = [
  { key: 'default', label: '기존 발송' },
  { key: 'group', label: '그룹 단체 발송' },
  { key: 'excel', label: '엑셀 변수 발송' },
]
const activeTab = ref('default')

const guests = ref([])

const step = ref(1)

// 템플릿 변수 정의
const templateVariables = [
  { key: 'guest_name', label: '참석자명' },
  { key: 'guest_phone', label: '전화번호' },
  { key: 'corp_part', label: '부서' },
  { key: 'title', label: '행사명' },
  { key: 'start_date', label: '시작일' },
  { key: 'end_date', label: '종료일' },
  { key: 'url', label: '접속URL' },
]

// --- Step 1: 템플릿 ---
const templates = ref([])
const templatesLoading = ref(false)
const selectedTemplate = ref(null)
const isEditing = ref(false)
const editingId = ref(null)
const editForm = ref({ title: '', content: '' })
const contentTextarea = ref(null)

async function loadTemplates() {
  templatesLoading.value = true
  try {
    const res = await apiClient.get('/admin/sms-templates')
    templates.value = res.data
  } catch (e) {
    console.error('템플릿 로드 실패:', e)
  } finally {
    templatesLoading.value = false
  }
}

function selectTemplate(tpl) {
  selectedTemplate.value = tpl
}

function startNewTemplate() {
  editingId.value = null
  editForm.value = { title: '', content: '' }
  isEditing.value = true
}

function editTemplate(tpl) {
  editingId.value = tpl.id
  editForm.value = { title: tpl.title, content: tpl.content }
  isEditing.value = true
}

function cancelEdit() {
  isEditing.value = false
  editingId.value = null
  editForm.value = { title: '', content: '' }
}

async function saveTemplate() {
  try {
    if (editingId.value) {
      const res = await apiClient.put(
        `/admin/sms-templates/${editingId.value}`,
        editForm.value,
      )
      const idx = templates.value.findIndex((t) => t.id === editingId.value)
      if (idx >= 0)
        templates.value[idx] = { ...templates.value[idx], ...editForm.value }
      if (selectedTemplate.value?.id === editingId.value) {
        selectedTemplate.value = templates.value[idx]
      }
    } else {
      const res = await apiClient.post('/admin/sms-templates', editForm.value)
      templates.value.unshift(res.data)
      selectedTemplate.value = res.data
    }
    cancelEdit()
  } catch (e) {
    console.error('템플릿 저장 실패:', e)
    alert('템플릿 저장에 실패했습니다.')
  }
}

function insertVariable(key) {
  const tag = `#{${key}}`
  const textarea = contentTextarea.value
  if (textarea) {
    const start = textarea.selectionStart
    const end = textarea.selectionEnd
    const text = editForm.value.content
    editForm.value.content = text.slice(0, start) + tag + text.slice(end)
    nextTick(() => {
      textarea.focus()
      textarea.selectionStart = textarea.selectionEnd = start + tag.length
    })
  } else {
    editForm.value.content += tag
  }
}

function startDirectInput() {
  selectedTemplate.value = null
  messageContent.value = ''
  step.value = 2
  if (guests.value.length === 0) loadGuests()
}

const canProceedStep1 = computed(
  () => selectedTemplate.value || isEditing.value === false,
)

// --- Step 2: 내용 + 수신자 ---
const messageContent = ref('')
const selectedUserIds = ref([])
const usersLoading = ref(false)
const messageTextarea = ref(null)
const selectedGroupFilter = ref('')

const groupNames = computed(() => {
  const groups = new Set(guests.value.map((g) => g.groupName).filter(Boolean))
  return [...groups].sort()
})

const filteredGuests = computed(() => {
  if (!selectedGroupFilter.value) return guests.value
  return guests.value.filter((g) => g.groupName === selectedGroupFilter.value)
})

function filterByGroup(group) {
  selectedGroupFilter.value = group
  if (group) {
    selectedUserIds.value = filteredGuests.value
      .filter((g) => g.telephone)
      .map((g) => g.userId)
  }
}

async function loadGuests() {
  usersLoading.value = true
  try {
    const res = await apiClient.get(
      `/admin/conventions/${props.conventionId}/guests`,
    )
    guests.value = res.data
  } catch (e) {
    console.error('참석�� 로드 실패:', e)
  } finally {
    usersLoading.value = false
  }
}

async function goToStep2() {
  if (selectedTemplate.value) {
    messageContent.value = selectedTemplate.value.content
  }
  if (guests.value.length === 0) await loadGuests()
  // 전체 참석자 선택 (전화번호 있는 사람만)
  selectedUserIds.value = guests.value
    .filter((g) => g.telephone)
    .map((g) => g.userId)
  step.value = 2
}

function selectAllUsers() {
  selectedUserIds.value = guests.value
    .filter((g) => g.telephone)
    .map((g) => g.userId)
}

function deselectAllUsers() {
  selectedUserIds.value = []
}

function insertVariableToMessage(key) {
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

const canProceedStep2 = computed(
  () =>
    messageContent.value.trim().length > 0 && selectedUserIds.value.length > 0,
)

const selectedUsers = computed(() =>
  guests.value.filter((g) => selectedUserIds.value.includes(g.userId)),
)

// --- Step 3: 미리보기 + 발송 ---
const previewUserId = ref(null)
const previewMessage = ref('')
const previewLoading = ref(false)
const sending = ref(false)
const sendResult = ref(null)

function goToStep3() {
  step.value = 3
  sendResult.value = null
  // 자동으로 첫 수신자 미리보기
  if (selectedUsers.value.length > 0) {
    previewUserId.value = selectedUsers.value[0].userId
    loadPreview()
  }
}

async function loadPreview() {
  if (!previewUserId.value) return
  previewLoading.value = true
  try {
    const res = await apiClient.post(
      `/admin/conventions/${props.conventionId}/sms/preview`,
      {
        content: messageContent.value,
        targetUserId: previewUserId.value,
      },
    )
    previewMessage.value = res.data.previewMessage
  } catch (e) {
    console.error('미리보기 실패:', e)
    previewMessage.value = '미리보기 로드 실패'
  } finally {
    previewLoading.value = false
  }
}

async function confirmAndSend() {
  const count = selectedUserIds.value.length
  if (
    !confirm(
      `${count}명에게 문자를 발송하시겠습니까?\n이 작업은 취소할 수 없습니다.`,
    )
  )
    return

  sending.value = true
  sendResult.value = null
  try {
    const res = await apiClient.post(
      `/admin/conventions/${props.conventionId}/sms/send`,
      {
        content: messageContent.value,
        targetUserIds: selectedUserIds.value,
      },
    )
    sendResult.value = res.data
  } catch (e) {
    console.error('발송 실패:', e)
    sendResult.value = {
      message: e.response?.data?.message || '발송 중 오류가 발생했습니다.',
      failCount: 1,
    }
  } finally {
    sending.value = false
  }
}

onMounted(loadTemplates)
</script>
