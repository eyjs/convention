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
        class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-3xl sm:w-full"
      >
        <!-- Header -->
        <div class="px-6 py-4 border-b flex items-center justify-between">
          <div class="flex items-center gap-3">
            <h3 class="text-lg font-semibold text-gray-900">
              카카오 알림톡 발송
            </h3>
            <span
              class="px-2 py-0.5 bg-yellow-100 text-yellow-800 text-xs rounded-full font-medium"
              >카카오톡</span
            >
          </div>
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

        <!-- Body -->
        <div
          class="p-6 bg-gray-50 min-h-[480px] max-h-[70vh] overflow-y-auto space-y-4"
        >
          <!-- 팝빌 템플릿 코드 -->
          <div class="bg-white border rounded-lg p-4">
            <label class="block text-sm font-medium text-gray-700 mb-2"
              >알림톡 템플릿 코드</label
            >
            <input
              v-model="templateCode"
              type="text"
              class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-yellow-500 focus:border-yellow-500"
              placeholder="팝빌에 등록된 템플릿 코드 입력"
            />
            <p class="text-xs text-gray-400 mt-1">
              팝빌 관리페이지에서 승인된 템플릿 코드를 입력하세요
            </p>
          </div>

          <!-- 발송 내용 -->
          <div class="bg-white border rounded-lg p-4">
            <div class="flex items-center justify-between mb-2">
              <label class="text-sm font-medium text-gray-700">발송 내용</label>
              <span class="text-xs text-gray-400">{{ content.length }}자</span>
            </div>
            <textarea
              v-model="content"
              rows="6"
              class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-yellow-500 focus:border-yellow-500 resize-none font-mono text-sm"
              placeholder="알림톡 내용을 입력하세요. 팝빌 템플릿과 일치해야 합니다."
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
                  class="px-2 py-1 bg-yellow-50 text-yellow-700 text-xs rounded border border-yellow-200 hover:bg-yellow-100 transition-colors"
                  @click="insertVariable(v.key)"
                >
                  {{ v.label }}
                </button>
              </div>
            </div>
          </div>

          <!-- 대체문자 (선택) -->
          <div class="bg-white border rounded-lg p-4">
            <div class="flex items-center justify-between mb-2">
              <label class="text-sm font-medium text-gray-700"
                >대체문자 (선택)</label
              >
              <span class="text-xs text-gray-400"
                >알림톡 실패 시 SMS로 발송</span
              >
            </div>
            <textarea
              v-model="altContent"
              rows="3"
              class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-yellow-500 focus:border-yellow-500 resize-none font-mono text-sm"
              placeholder="(선택) 알림톡 수신 실패 시 대체 문자 내용"
            ></textarea>
          </div>

          <!-- 수신자 -->
          <div class="bg-white border rounded-lg p-4">
            <div class="flex items-center justify-between mb-3">
              <label class="text-sm font-medium text-gray-700"
                >수신자 ({{ selectedUserIds.length }}명)</label
              >
              <div class="flex items-center gap-2">
                <button
                  class="text-xs text-yellow-600 hover:underline"
                  @click="selectAll"
                >
                  전체 선택
                </button>
                <button
                  class="text-xs text-gray-500 hover:underline"
                  @click="selectedUserIds = []"
                >
                  전체 해제
                </button>
              </div>
            </div>

            <div v-if="guestsLoading" class="text-center py-4 text-gray-400">
              참석자 로딩 중...
            </div>

            <div
              v-else
              class="max-h-48 overflow-y-auto space-y-1 border rounded-lg p-2"
            >
              <label
                v-for="user in guests"
                :key="user.userId"
                class="flex items-center gap-2 px-2 py-1.5 rounded hover:bg-gray-50 cursor-pointer"
              >
                <input
                  v-model="selectedUserIds"
                  type="checkbox"
                  :value="user.userId"
                  class="rounded border-gray-300 text-yellow-500 focus:ring-yellow-500"
                />
                <span class="text-sm text-gray-900">{{ user.guestName }}</span>
                <span class="text-xs text-gray-400">{{ user.telephone }}</span>
                <span v-if="!user.telephone" class="text-xs text-red-400">
                  (번호없음)
                </span>
              </label>
            </div>
          </div>

          <!-- 발송 결과 -->
          <div
            v-if="sendResult"
            class="border rounded-lg p-4"
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
              {{ sendResult.message }}
            </p>
          </div>
        </div>

        <!-- Footer -->
        <div class="px-6 py-4 border-t bg-white flex justify-end gap-2">
          <button
            class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-200 transition-colors"
            @click="emit('close')"
          >
            닫기
          </button>
          <button
            class="px-4 py-2 bg-yellow-500 text-white rounded-lg text-sm font-medium hover:bg-yellow-600 transition-colors disabled:opacity-50"
            :disabled="!canSend || sending"
            @click="confirmAndSend"
          >
            {{
              sending
                ? '발송 중...'
                : `${selectedUserIds.length}명에게 알림톡 발송`
            }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/api'

const props = defineProps({
  conventionId: { type: Number, required: true },
})
const emit = defineEmits(['close'])

const templateVariables = [
  { key: 'guest_name', label: '참석자명' },
  { key: 'title', label: '행사명' },
  { key: 'start_date', label: '시작일' },
  { key: 'end_date', label: '종료일' },
  { key: 'url', label: '접속URL' },
]

const templateCode = ref('')
const content = ref('')
const altContent = ref('')
const guests = ref([])
const guestsLoading = ref(false)
const selectedUserIds = ref([])
const sending = ref(false)
const sendResult = ref(null)

const canSend = computed(
  () =>
    templateCode.value.trim() &&
    content.value.trim() &&
    selectedUserIds.value.length > 0,
)

async function loadGuests() {
  guestsLoading.value = true
  try {
    const res = await apiClient.get(
      `/admin/conventions/${props.conventionId}/guests`,
    )
    guests.value = res.data
    selectAll()
  } catch (e) {
    console.error('참석자 로드 실패:', e)
  } finally {
    guestsLoading.value = false
  }
}

function selectAll() {
  selectedUserIds.value = guests.value
    .filter((g) => g.telephone)
    .map((g) => g.userId)
}

function insertVariable(key) {
  content.value += `#{${key}}`
}

async function confirmAndSend() {
  const count = selectedUserIds.value.length
  if (
    !confirm(
      `${count}명에게 알림톡을 발송하시겠습니까?\n이 작업은 취소할 수 없습니다.`,
    )
  )
    return

  sending.value = true
  sendResult.value = null
  try {
    const res = await apiClient.post(
      `/admin/conventions/${props.conventionId}/alimtalk/send`,
      {
        templateCode: templateCode.value,
        content: content.value,
        altContent: altContent.value || null,
        targetUserIds: selectedUserIds.value,
      },
    )
    sendResult.value = res.data
  } catch (e) {
    console.error('알림톡 발송 실패:', e)
    sendResult.value = {
      message: e.response?.data?.message || '발송 중 오류가 발생했습니다.',
      failCount: 1,
    }
  } finally {
    sending.value = false
  }
}

onMounted(loadGuests)
</script>
