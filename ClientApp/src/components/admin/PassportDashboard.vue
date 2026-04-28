<template>
  <div>
    <AdminPageHeader title="여권 검증 관리" />

    <!-- 통계 카드 -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4">
      <button
        v-for="card in statCards"
        :key="card.key"
        class="bg-white rounded-lg shadow-sm p-4 text-left hover:shadow-md transition-shadow"
        :class="selectedStatus === card.key ? 'ring-2 ring-primary-500' : ''"
        @click="selectStatus(card.key)"
      >
        <div class="text-2xl font-bold" :class="card.color">
          {{ stats[card.key] || 0 }}
        </div>
        <div class="text-sm text-gray-500 mt-1">{{ card.label }}</div>
      </button>
    </div>

    <!-- 검색/필터 영역 -->
    <AdminSearchFilter
      v-model="search"
      placeholder="이름, 전화번호, 소속 검색..."
      :result-count="filteredList.length"
      :show-reset="!!(search || selectedStatus !== 'all' || selectedGroup)"
      @reset="resetFilters"
    >
      <template #filters>
        <AdminSelect v-model="selectedStatus">
          <option value="all">전체 상태</option>
          <option value="approved">승인완료</option>
          <option value="pending">승인대기</option>
          <option value="rejected">거절</option>
          <option value="unregistered">미등록</option>
        </AdminSelect>
        <AdminSelect v-if="groupNames.length > 0" v-model="selectedGroup">
          <option value="">전체 그룹</option>
          <option v-for="g in groupNames" :key="g" :value="g">{{ g }}</option>
        </AdminSelect>
      </template>
    </AdminSearchFilter>

    <!-- 참석자 리스트 -->
    <div class="mt-3 bg-white rounded-lg shadow overflow-hidden">
      <div class="p-3 border-b">
        <h3 class="font-semibold text-sm">
          {{ currentStatusLabel }}
          <span class="text-gray-400 font-normal ml-1"
            >({{ filteredList.length }}명)</span
          >
        </h3>
      </div>

      <div v-if="loading" class="p-8 text-center text-gray-400">로딩 중...</div>

      <div
        v-else-if="filteredList.length === 0"
        class="p-8 text-center text-gray-400"
      >
        해당 상태의 참석자가 없습니다
      </div>

      <div v-else class="divide-y max-h-[60vh] overflow-y-auto">
        <div
          v-for="guest in filteredList"
          :key="guest.id"
          class="p-4 hover:bg-gray-50 cursor-pointer"
          @click="openDetail(guest)"
        >
          <div class="flex items-center justify-between">
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2">
                <span class="font-medium text-gray-900">{{ guest.name }}</span>
                <span
                  v-if="guest.groupName"
                  class="text-xs px-1.5 py-0.5 bg-blue-50 text-blue-700 rounded"
                >
                  {{ guest.groupName }}
                </span>
                <span
                  class="text-xs px-1.5 py-0.5 rounded"
                  :class="getStatusBadgeClass(guest)"
                  >{{ getStatusLabel(guest) }}</span
                >
              </div>
              <div class="text-xs text-gray-500 mt-0.5 flex flex-wrap gap-x-3">
                <span>{{ guest.phone || '-' }}</span>
                <span
                  v-if="guest.lastName || guest.firstName"
                  class="font-medium text-gray-700"
                >
                  {{ guest.lastName }} {{ guest.firstName }}
                </span>
                <span v-if="guest.passportNumber"
                  >No. {{ guest.passportNumber }}</span
                >
                <span v-if="guest.passportExpiryDate"
                  >만료 {{ guest.passportExpiryDate }}</span
                >
              </div>
              <div class="flex items-center gap-2 mt-1">
                <span
                  v-if="guest.passportImageUrl"
                  class="text-xs text-blue-600 cursor-pointer hover:underline"
                  @click.stop="viewPassportImage(guest.passportImageUrl)"
                  >여권사본 보기</span
                >
                <span
                  v-if="guest.passportVerifiedAt"
                  class="text-xs text-gray-400"
                >
                  승인: {{ formatDateTime(guest.passportVerifiedAt) }}
                </span>
              </div>
              <div
                v-if="guest.passportRejectionReason"
                class="text-xs text-red-500 mt-0.5"
              >
                거절 사유: {{ guest.passportRejectionReason }}
              </div>
            </div>
            <div class="flex items-center gap-2 flex-shrink-0 ml-4">
              <!-- 승인대기 상태에서만 승인/거절 버튼 표시 -->
              <template
                v-if="!guest.passportVerified && guest.passportImageUrl"
              >
                <button
                  class="px-3 py-1.5 text-xs bg-green-50 text-green-700 rounded-lg hover:bg-green-100"
                  @click.stop="approvePassport(guest)"
                >
                  승인
                </button>
                <button
                  class="px-3 py-1.5 text-xs bg-red-50 text-red-700 rounded-lg hover:bg-red-100"
                  @click.stop="openRejectModal(guest)"
                >
                  거절
                </button>
              </template>
              <svg
                class="w-4 h-4 text-gray-300"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M9 5l7 7-7 7"
                />
              </svg>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 참석자 상세 모달 -->
    <GuestDetailModal
      :is-open="showDetailModal"
      :convention-id="conventionId"
      :guest-id="detailGuestId"
      @close="showDetailModal = false"
      @edit="onDetailEdit"
    />

    <!-- 참석자 수정 모달 -->
    <GuestFormModal
      :is-open="!!editingGuest"
      :convention-id="conventionId"
      :editing-guest="editingGuest"
      :available-templates="[]"
      :available-option-tours="[]"
      :attribute-templates="[]"
      @close="editingGuest = null"
      @saved="onGuestSaved"
    />

    <!-- 거절 사유 모달 -->
    <BaseModal
      :is-open="showRejectModal"
      max-width="sm"
      @close="showRejectModal = false"
    >
      <template #header>
        <h2 class="text-lg font-semibold">여권 거절</h2>
      </template>
      <template #body>
        <p class="text-sm text-gray-600 mb-3">
          <strong>{{ rejectTarget?.name }}</strong
          >의 여권을 거절합니다.
        </p>
        <label class="block text-sm font-medium mb-1">거절 사유 *</label>
        <textarea
          v-model="rejectReason"
          rows="3"
          class="w-full px-3 py-2 border rounded-lg resize-none text-sm"
          placeholder="거절 사유를 입력하세요 (예: 여권 만료, 사진 불량)"
        ></textarea>
      </template>
      <template #footer>
        <button
          class="px-4 py-2 border rounded-lg hover:bg-gray-50"
          @click="showRejectModal = false"
        >
          취소
        </button>
        <button
          class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
          :disabled="!rejectReason.trim()"
          @click="confirmReject"
        >
          거절
        </button>
      </template>
    </BaseModal>
  </div>

  <ImageViewer
    v-model="passportViewerOpen"
    :images="passportViewerImages"
  />
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/api'
import ImageViewer from '@/components/common/ImageViewer.vue'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminSearchFilter from '@/components/admin/ui/AdminSearchFilter.vue'
import AdminSelect from '@/components/admin/ui/AdminSelect.vue'
import GuestDetailModal from '@/components/admin/guest/GuestDetailModal.vue'
import GuestFormModal from '@/components/admin/guest/GuestFormModal.vue'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const passportViewerOpen = ref(false)
const passportViewerImages = ref([])

const stats = ref({})
const allGuests = ref([])
const selectedStatus = ref('all')
const selectedGroup = ref('')
const search = ref('')
const loading = ref(false)
const showDetailModal = ref(false)
const detailGuestId = ref(null)
const editingGuest = ref(null)
const showRejectModal = ref(false)
const rejectTarget = ref(null)
const rejectReason = ref('')

const statCards = [
  { key: 'all', label: '전체', color: 'text-gray-900' },
  { key: 'approved', label: '승인완료', color: 'text-green-600' },
  { key: 'pending', label: '승인대기', color: 'text-amber-600' },
  { key: 'unregistered', label: '미등록', color: 'text-gray-400' },
]

const groupNames = computed(() => {
  const groups = new Set(
    allGuests.value.map((g) => g.groupName).filter(Boolean),
  )
  return [...groups].sort()
})

const currentStatusLabel = computed(() => {
  if (selectedStatus.value === 'all') return '전체'
  return statCards.find((c) => c.key === selectedStatus.value)?.label || '전체'
})

const filteredList = computed(() => {
  let list = allGuests.value
  if (selectedStatus.value !== 'all') {
    list = list.filter((g) => getGuestStatus(g) === selectedStatus.value)
  }
  if (selectedGroup.value) {
    list = list.filter((g) => g.groupName === selectedGroup.value)
  }
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(
      (g) =>
        g.name?.toLowerCase().includes(q) ||
        g.phone?.includes(q) ||
        g.groupName?.toLowerCase().includes(q),
    )
  }
  return list
})

function resetFilters() {
  search.value = ''
  selectedStatus.value = 'all'
  selectedGroup.value = ''
}

function getGuestStatus(guest) {
  if (guest.passportVerified) return 'approved'
  if (guest.passportImageUrl) return 'pending'
  if (guest.passportRejectedAt) return 'rejected'
  return 'unregistered'
}

function getStatusLabel(guest) {
  const s = getGuestStatus(guest)
  return {
    approved: '승인',
    pending: '대기',
    rejected: '거절',
    unregistered: '미등록',
  }[s]
}

function getStatusBadgeClass(guest) {
  const s = getGuestStatus(guest)
  return {
    approved: 'bg-green-50 text-green-700',
    pending: 'bg-amber-50 text-amber-700',
    rejected: 'bg-red-50 text-red-700',
    unregistered: 'bg-gray-100 text-gray-500',
  }[s]
}

function selectStatus(status) {
  selectedStatus.value = selectedStatus.value === status ? 'all' : status
  selectedGroup.value = ''
}

function viewPassportImage(url) {
  if (!url) return
  if (url.toLowerCase().endsWith('.pdf')) {
    window.open(url, '_blank')
  } else {
    passportViewerImages.value = [url]
    passportViewerOpen.value = true
  }
}

function formatDateTime(dateStr) {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return `${d.getFullYear()}.${String(d.getMonth() + 1).padStart(2, '0')}.${String(d.getDate()).padStart(2, '0')}`
}

function onDetailEdit(guestDetail) {
  showDetailModal.value = false
  if (guestDetail) {
    // 리스트에서 해당 참석자 찾아서 editingGuest로 전달
    const guest = allGuests.value.find((g) => g.id === guestDetail.id)
    if (guest) editingGuest.value = guest
  }
}

async function onGuestSaved() {
  editingGuest.value = null
  await loadData()
}

function openDetail(guest) {
  detailGuestId.value = guest.id
  showDetailModal.value = true
}

async function approvePassport(guest) {
  if (!confirm(`${guest.name}의 여권을 승인하시겠습니까?`)) return
  try {
    await apiClient.post(`/admin/guests/${guest.id}/passport/approve`)
    await loadData()
  } catch (error) {
    alert('승인 처리 실패')
  }
}

function openRejectModal(guest) {
  rejectTarget.value = guest
  rejectReason.value = ''
  showRejectModal.value = true
}

async function confirmReject() {
  if (!rejectReason.value.trim() || !rejectTarget.value) return
  try {
    await apiClient.post(
      `/admin/guests/${rejectTarget.value.id}/passport/reject`,
      {
        reason: rejectReason.value.trim(),
      },
    )
    showRejectModal.value = false
    await loadData()
  } catch (error) {
    alert('거절 처리 실패')
  }
}

async function loadData() {
  loading.value = true
  try {
    const [statsRes, listRes] = await Promise.all([
      apiClient.get(`/admin/conventions/${props.conventionId}/passport-stats`),
      apiClient.get(`/admin/conventions/${props.conventionId}/passport-list`),
    ])
    stats.value = statsRes.data
    allGuests.value = listRes.data
  } catch (error) {
    console.error('Failed to load passport data:', error)
  } finally {
    loading.value = false
  }
}

onMounted(loadData)
</script>
