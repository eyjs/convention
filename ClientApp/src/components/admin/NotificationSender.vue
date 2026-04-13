<template>
  <div class="space-y-6">
    <!-- 발송 폼 -->
    <div class="bg-white rounded-lg shadow p-6">
      <h3 class="text-lg font-bold text-gray-900 mb-4">알림 발송</h3>
      <div class="space-y-4">
        <!-- 유형 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">유형</label>
          <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-2">
            <button
              v-for="t in types" :key="t.value"
              class="px-3 py-2 rounded-lg text-sm font-medium transition-colors"
              :class="form.type === t.value ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'"
              @click="form.type = t.value"
            >{{ t.icon }} {{ t.label }}</button>
          </div>
          <p class="text-xs text-gray-500 mt-1">{{ types.find(t => t.value === form.type)?.desc }}</p>
        </div>

        <!-- 제목 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">제목</label>
          <input v-model="form.title" type="text" class="w-full border rounded-lg px-3 py-2" placeholder="알림 제목" />
        </div>

        <!-- 내용 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">내용</label>
          <textarea v-model="form.body" rows="3" class="w-full border rounded-lg px-3 py-2" placeholder="알림 내용"></textarea>
        </div>

        <!-- 참조 선택 (NOTICE, SURVEY) -->
        <div v-if="form.type === 'NOTICE'">
          <label class="block text-sm font-medium text-gray-700 mb-1">공지 선택</label>
          <select v-model="form.referenceId" class="w-full border rounded-lg px-3 py-2">
            <option :value="null">선택...</option>
            <option v-for="n in notices" :key="n.id" :value="n.id">{{ n.title }}</option>
          </select>
        </div>
        <div v-if="form.type === 'SURVEY'">
          <label class="block text-sm font-medium text-gray-700 mb-1">설문 선택</label>
          <select v-model="form.referenceId" class="w-full border rounded-lg px-3 py-2">
            <option :value="null">선택...</option>
            <option v-for="s in surveys" :key="s.id" :value="s.id">{{ s.title }}</option>
          </select>
        </div>
        <div v-if="form.type === 'SEAT'">
          <label class="block text-sm font-medium text-gray-700 mb-1">좌석 배치도 선택</label>
          <select v-model="form.referenceId" class="w-full border rounded-lg px-3 py-2">
            <option :value="null">선택...</option>
            <option v-for="sl in seatingLayouts" :key="sl.id" :value="sl.id">{{ sl.name }}</option>
          </select>
        </div>
        <div v-if="form.type === 'LINK'">
          <label class="block text-sm font-medium text-gray-700 mb-1">URL</label>
          <input v-model="form.linkUrl" type="url" class="w-full border rounded-lg px-3 py-2" placeholder="https://..." />
        </div>

        <!-- 대상 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">대상</label>
          <div class="flex gap-3">
            <label class="flex items-center gap-1.5"><input v-model="form.targetScope" type="radio" value="ALL" /> 전체</label>
            <label class="flex items-center gap-1.5"><input v-model="form.targetScope" type="radio" value="GROUP" /> 그룹별</label>
            <label class="flex items-center gap-1.5"><input v-model="form.targetScope" type="radio" value="INDIVIDUAL" /> 개인</label>
          </div>
          <select v-if="form.targetScope === 'GROUP'" v-model="form.targetGroupName" class="mt-2 w-full border rounded-lg px-3 py-2">
            <option value="">그룹 선택</option>
            <option v-for="g in groups" :key="g" :value="g">{{ g }}</option>
          </select>
          <!-- 개인 선택 -->
          <div v-if="form.targetScope === 'INDIVIDUAL'" class="mt-2 space-y-2">
            <input v-model="individualSearch" type="text" placeholder="이름 검색..." class="w-full border rounded-lg px-3 py-2 text-sm" />
            <div class="max-h-40 overflow-y-auto border rounded-lg p-2 space-y-1">
              <label
                v-for="g in filteredIndividuals"
                :key="g.id"
                class="flex items-center gap-2 px-2 py-1 rounded hover:bg-gray-50 cursor-pointer text-sm"
              >
                <input v-model="form.targetUserIds" type="checkbox" :value="g.id" />
                <span>{{ g.name }}</span>
                <span class="text-gray-400 text-xs">{{ g.corpPart || g.groupName || '' }}</span>
              </label>
            </div>
            <p class="text-xs text-gray-500">{{ form.targetUserIds?.length || 0 }}명 선택</p>
          </div>
        </div>

        <button
          :disabled="!form.title || sending"
          class="w-full py-3 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 disabled:bg-gray-300 transition-colors"
          @click="send"
        >
          {{ sending ? '발송 중...' : `알림 발송 (${guestCount}명)` }}
        </button>
      </div>
    </div>

    <!-- 발송 이력 -->
    <div class="bg-white rounded-lg shadow p-6">
      <h3 class="text-lg font-bold text-gray-900 mb-4">발송 이력</h3>
      <div v-if="history.length === 0" class="text-center text-gray-400 py-8">아직 발송 이력이 없습니다</div>
      <div v-else class="space-y-3">
        <div
          v-for="h in history" :key="h.id"
          class="border rounded-lg p-4 hover:bg-gray-50 cursor-pointer transition-colors"
          @click="showStats(h.id)"
        >
          <div class="flex items-center justify-between mb-1">
            <div class="flex items-center gap-2">
              <span class="text-lg">{{ typeIcon(h.type) }}</span>
              <span class="font-semibold text-gray-900">{{ h.title }}</span>
            </div>
            <span class="text-xs text-gray-400">{{ formatDate(h.createdAt) }}</span>
          </div>
          <p class="text-sm text-gray-600 line-clamp-1">{{ h.body }}</p>
          <div class="mt-2 flex items-center gap-2">
            <div class="flex-1 h-2 bg-gray-100 rounded-full overflow-hidden">
              <div class="h-full bg-green-500 rounded-full" :style="{ width: h.totalCount ? `${(h.readCount / h.totalCount) * 100}%` : '0%' }"></div>
            </div>
            <span class="text-xs font-medium" :class="h.readCount === h.totalCount ? 'text-green-600' : 'text-gray-500'">
              {{ h.readCount }}/{{ h.totalCount }}명
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- 읽음 통계 모달 -->
    <BaseModal :is-open="!!statsData" max-width="lg" @close="statsData = null">
      <template #header>
        <h3 class="text-lg font-bold">읽음 통계 — {{ statsData?.title }}</h3>
      </template>
      <template #body>
        <div v-if="statsData" class="space-y-4">
          <!-- 프로그레스 -->
          <div class="bg-gray-50 rounded-lg p-4">
            <div class="flex items-center justify-between mb-2">
              <span class="text-sm text-gray-600">읽음률</span>
              <span class="text-xl font-bold" :class="statsData.read === statsData.total ? 'text-green-600' : 'text-blue-600'">
                {{ statsData.total > 0 ? Math.round(statsData.read / statsData.total * 100) : 0 }}%
              </span>
            </div>
            <div class="h-3 bg-gray-200 rounded-full overflow-hidden">
              <div class="h-full bg-green-500 rounded-full transition-all" :style="{ width: statsData.total > 0 ? `${(statsData.read / statsData.total) * 100}%` : '0%' }"></div>
            </div>
            <div class="flex justify-between mt-1 text-xs text-gray-500">
              <span>읽음 {{ statsData.read }}명</span>
              <span>미읽음 {{ statsData.unread }}명</span>
              <span>전체 {{ statsData.total }}명</span>
            </div>
          </div>

          <!-- 읽음/미읽음 목록 -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="border rounded-lg p-3">
              <h4 class="text-sm font-semibold text-green-700 mb-2 flex items-center gap-1">
                <span class="w-2 h-2 rounded-full bg-green-500"></span>
                읽음 ({{ statsData.readUsers?.length }}명)
              </h4>
              <div class="max-h-60 overflow-y-auto space-y-1">
                <div v-for="u in statsData.readUsers" :key="u.name" class="flex items-center justify-between text-sm py-1 px-2 rounded hover:bg-green-50">
                  <span>{{ u.name }}</span>
                  <span class="text-xs text-gray-400">{{ u.readAt ? new Date(u.readAt).toLocaleString('ko-KR', { month: 'numeric', day: 'numeric', hour: '2-digit', minute: '2-digit' }) : '' }}</span>
                </div>
              </div>
            </div>
            <div class="border rounded-lg p-3">
              <h4 class="text-sm font-semibold text-red-700 mb-2 flex items-center gap-1">
                <span class="w-2 h-2 rounded-full bg-red-500"></span>
                미읽음 ({{ statsData.unreadUsers?.length }}명)
              </h4>
              <div class="max-h-60 overflow-y-auto space-y-1">
                <div v-for="u in statsData.unreadUsers" :key="u.name" class="text-sm py-1 px-2 rounded hover:bg-red-50">
                  {{ u.name }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({ conventionId: { type: Number, required: true } })

const types = [
  { value: 'TEXT', label: '텍스트', icon: '💬', desc: '자유 텍스트 전달 (현장 긴급 안내 등)' },
  { value: 'NOTICE', label: '공지', icon: '📢', desc: '게시판 공지를 연결하여 알림' },
  { value: 'SURVEY', label: '설문', icon: '📋', desc: '설문조사 독촉/안내 (딥링크 포함)' },
  { value: 'SCHEDULE', label: '일정', icon: '📅', desc: '일정 변경/추가 안내 (일정 페이지로 이동)' },
  { value: 'SEAT', label: '좌석', icon: '💺', desc: '좌석 배정 완료 알림 (내 자리 보기로 이동)' },
  { value: 'LINK', label: '링크', icon: '🔗', desc: '외부 URL 또는 커스텀 링크 전달' },
]
const individualSearch = ref('')

const form = reactive({ type: 'TEXT', title: '', body: '', linkUrl: null, referenceId: null, targetScope: 'ALL', targetGroupName: '', targetUserIds: [] })
const sending = ref(false)
const history = ref([])
const notices = ref([])
const surveys = ref([])
const seatingLayouts = ref([])
const groups = ref([])
const guests = ref([])
const statsData = ref(null)

const guestCount = computed(() => {
  if (form.targetScope === 'GROUP' && form.targetGroupName)
    return guests.value.filter((g) => g.groupName === form.targetGroupName).length
  if (form.targetScope === 'INDIVIDUAL')
    return form.targetUserIds?.length || 0
  return guests.value.length
})

const filteredIndividuals = computed(() => {
  const q = individualSearch.value.trim().toLowerCase()
  if (!q) return guests.value.slice(0, 100)
  return guests.value.filter((g) => g.name?.toLowerCase().includes(q) || g.corpPart?.toLowerCase().includes(q)).slice(0, 100)
})

function typeIcon(type) { return types.find((t) => t.value === type)?.icon || '💬' }
function formatDate(d) { return new Date(d).toLocaleString('ko-KR', { month: 'numeric', day: 'numeric', hour: '2-digit', minute: '2-digit' }) }

async function loadData() {
  try {
    const [histRes, guestRes, seatRes] = await Promise.all([
      apiClient.get(`/admin/conventions/${props.conventionId}/notifications`),
      apiClient.get(`/admin/conventions/${props.conventionId}/guests`),
      apiClient.get(`/admin/conventions/${props.conventionId}/seating-layouts`).catch(() => ({ data: [] })),
    ])
    seatingLayouts.value = seatRes.data || []
    history.value = histRes.data || []
    guests.value = Array.isArray(guestRes.data) ? guestRes.data : []
    groups.value = [...new Set(guests.value.map((g) => g.groupName).filter(Boolean))].sort()

    // 공지/설문 목록
    const [noticeRes, surveyRes] = await Promise.all([
      apiClient.get(`/admin/notices?conventionId=${props.conventionId}`).catch(() => ({ data: { items: [] } })),
      apiClient.get(`/surveys/convention/${props.conventionId}`).catch(() => ({ data: [] })),
    ])
    notices.value = noticeRes.data?.items || noticeRes.data || []
    surveys.value = surveyRes.data || []
  } catch {}
}

async function send() {
  if (!form.title) return alert('제목을 입력하세요.')
  sending.value = true
  try {
    const res = await apiClient.post(`/admin/conventions/${props.conventionId}/notifications`, form)
    alert(`${res.data.recipientCount}명에게 발송 완료`)
    form.title = ''; form.body = ''; form.referenceId = null; form.linkUrl = null
    loadData()
  } catch (e) {
    alert(e.response?.data?.message || '발송 실패')
  } finally { sending.value = false }
}

async function showStats(id) {
  try {
    const res = await apiClient.get(`/admin/notifications/${id}/stats`)
    statsData.value = res.data
  } catch {}
}

onMounted(loadData)
</script>
