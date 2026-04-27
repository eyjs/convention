<template>
  <div>
    <!-- 단계 표시 -->
    <div class="flex items-center gap-1 mb-4">
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
          class="w-6 h-px"
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
            <span class="font-semibold"
              >{{
                selectedGroupInfo.count - selectedGroupInfo.noPhoneCount
              }}명</span
            >에게 발송됩니다.
            <span
              v-if="selectedGroupInfo.noPhoneCount > 0"
              class="text-amber-600"
            >
              (전화번호 없는 {{ selectedGroupInfo.noPhoneCount }}명 제외)
            </span>
          </p>
        </div>
      </div>
    </div>

    <!-- Step 2: 메시지 작성 -->
    <div v-if="step === 2">
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
          placeholder="메시지를 입력하세요. #{guest_name} 등 변수를 사용할 수 있습니다."
        ></textarea>

        <!-- 변수 삽입 -->
        <div class="mt-2">
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
      </div>
    </div>

    <!-- Step 3: 확인 + 발송 -->
    <div v-if="step === 3">
      <h4 class="font-semibold text-gray-800 mb-3">발송 확인</h4>

      <!-- 요약 -->
      <div class="bg-white border rounded-lg p-4 mb-4">
        <dl class="grid grid-cols-2 gap-3 text-sm">
          <div>
            <dt class="text-gray-500">대상 그룹</dt>
            <dd class="font-semibold text-gray-900">{{ selectedGroup }}</dd>
          </div>
          <div>
            <dt class="text-gray-500">수신자 수</dt>
            <dd class="font-semibold text-gray-900">{{ recipientCount }}명</dd>
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
          @click="goToStep2"
        >
          다음
        </button>

        <button
          v-if="step === 2"
          class="px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors disabled:opacity-50"
          :disabled="!messageContent.trim()"
          @click="goToStep3"
        >
          다음
        </button>

        <button
          v-if="step === 3 && !sendResult"
          class="px-4 py-2 bg-red-500 text-white rounded-lg text-sm font-medium hover:bg-red-600 transition-colors disabled:opacity-50"
          :disabled="sending"
          @click="confirmAndSend"
        >
          {{ sending ? '발송 중...' : `${recipientCount}명에게 발송` }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import apiClient from '@/services/api'
import { fetchSmsGroups } from '@/services/smsGroupService'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const stepLabels = ['그룹 선택', '메시지 작성', '발송 확인']
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

const recipientCount = computed(() => {
  if (!selectedGroupInfo.value) return 0
  return selectedGroupInfo.value.count - selectedGroupInfo.value.noPhoneCount
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

// Step 2: 메시지
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

function goToStep2() {
  step.value = 2
}

// Step 3: 발송
const sending = ref(false)
const sendProgress = ref({ current: 0, total: 0 })
const sendResult = ref(null)

const sendProgressPercent = computed(() => {
  if (sendProgress.value.total === 0) return 0
  return Math.round(
    (sendProgress.value.current / sendProgress.value.total) * 100,
  )
})

function goToStep3() {
  step.value = 3
  sendResult.value = null
  sendProgress.value = { current: 0, total: 0 }
}

async function confirmAndSend() {
  const count = recipientCount.value
  if (
    !confirm(
      `${count}명에게 문자를 발송하시겠습니까?\n이 작업은 취소할 수 없습니다.`,
    )
  )
    return

  sending.value = true
  sendResult.value = null

  try {
    // 그룹의 수신자 목록 조회
    const guestsRes = await apiClient.get(
      `/admin/conventions/${props.conventionId}/guests`,
    )
    let guests = guestsRes.data

    // 그룹 필터링
    if (selectedGroup.value && selectedGroup.value !== '전체') {
      guests = guests.filter((g) => g.groupName === selectedGroup.value)
    }

    // 전화번호 있는 수신자만
    const recipients = guests.filter((g) => g.telephone)

    sendProgress.value = { current: 0, total: recipients.length }

    const result = {
      totalCount: recipients.length,
      successCount: 0,
      failCount: 0,
      failedItems: [],
    }

    // 순차 발송 (send-one 방식)
    for (const recipient of recipients) {
      try {
        const res = await apiClient.post(
          `/admin/conventions/${props.conventionId}/sms/send-one`,
          {
            name: recipient.guestName,
            phone: recipient.telephone,
            message: messageContent.value,
          },
        )
        if (res.data.success) {
          result.successCount++
        } else {
          result.failCount++
          result.failedItems.push({
            name: recipient.guestName,
            phone: recipient.telephone,
            reason: res.data.reason,
          })
        }
      } catch (e) {
        result.failCount++
        result.failedItems.push({
          name: recipient.guestName,
          phone: recipient.telephone,
          reason: e.message,
        })
      }
      sendProgress.value = {
        ...sendProgress.value,
        current: sendProgress.value.current + 1,
      }
    }

    sendResult.value = result
  } catch (e) {
    console.error('발송 실패:', e)
    sendResult.value = {
      successCount: 0,
      failCount: 1,
      failedItems: [{ name: '-', phone: '-', reason: e.message }],
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
