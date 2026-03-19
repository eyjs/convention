<template>
  <div>
    <div
      class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6"
    >
      <div class="flex items-center gap-4">
        <h2 class="text-xl font-semibold">참석자 관리</h2>
        <!-- 선택된 참석자 수 표시 -->
        <span
          v-if="selectedGuests.length > 0"
          class="px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm font-medium"
        >
          {{ selectedGuests.length }}명 선택됨
        </span>
      </div>
      <div class="flex flex-wrap items-center gap-2">
        <!-- 대량 작업 버튼 -->
        <button
          v-if="selectedGuests.length > 0"
          class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 flex items-center justify-center gap-2 whitespace-nowrap overflow-hidden text-ellipsis"
          @click="showBulkAssignModal = true"
        >
          <svg
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
            />
          </svg>
          일정 일괄 배정
        </button>

        <!-- 속성 일괄 매핑 버튼 -->
        <button
          v-if="selectedGuests.length > 0"
          class="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 flex items-center justify-center gap-2 whitespace-nowrap overflow-hidden text-ellipsis"
          @click="showBulkAttributeModal = true"
        >
          <svg
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"
            />
          </svg>
          속성 일괄 매핑
        </button>
        <!-- 그룹 단위 일정 배정 버튼 -->
        <button
          class="px-4 py-2 bg-orange-600 text-white rounded-lg hover:bg-orange-700 flex items-center justify-center gap-2 whitespace-nowrap overflow-hidden text-ellipsis"
          @click="showGroupAssignModal = true"
        >
          <svg
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
            />
          </svg>
          그룹별 일정 배정
        </button>
        <!-- 그룹 단위 일정 해제 버튼 -->
        <button
          class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 flex items-center justify-center gap-2 whitespace-nowrap overflow-hidden text-ellipsis"
          @click="showGroupRemoveModal = true"
        >
          <svg
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
            />
          </svg>
          그룹별 일정 해제
        </button>
        <!-- 그룹별 속성 초기화 버튼 -->
        <button
          class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 flex items-center justify-center gap-2 whitespace-nowrap overflow-hidden text-ellipsis"
          @click="showGroupAttributeResetModal = true"
        >
          <svg
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
            />
          </svg>
          그룹별 속성 초기화
        </button>
        <button
          class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 flex items-center justify-center whitespace-nowrap overflow-hidden text-ellipsis"
          @click="showCreateModal = true"
        >
          + 참석자 추가
        </button>
      </div>
    </div>

    <!-- 검색 -->
    <div class="mb-4">
      <input
        v-model="searchTerm"
        type="text"
        placeholder="이름, 전화번호, 부서, 소속으로 검색..."
        class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
      />
    </div>

    <div v-if="loading" class="text-center py-8">로딩 중...</div>
    <div
      v-else-if="guests.length === 0"
      class="text-center py-8 bg-white rounded-lg shadow"
    >
      <p class="text-gray-500">등록된 참석자가 없습니다</p>
      <button
        class="mt-4 px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
        @click="showCreateModal = true"
      >
        첫 참석자 추가하기
      </button>
    </div>
    <div
      v-else-if="filteredGuests.length === 0 && searchTerm"
      class="text-center py-8 bg-white rounded-lg shadow"
    >
      <p class="text-gray-500">
        "{{ searchTerm }}" 검색 결과가 없습니다 (전체 {{ guests.length }}명)
      </p>
    </div>
    <div v-else class="bg-white rounded-lg shadow overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left">
                <input
                  type="checkbox"
                  :checked="
                    selectedGuests.length === filteredGuests.length &&
                    filteredGuests.length > 0
                  "
                  class="rounded"
                  @change="toggleSelectAll"
                />
              </th>
              <th
                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
              >
                이름
              </th>
              <th
                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
              >
                전화번호
              </th>
              <th
                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
              >
                부서
              </th>
              <th
                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
              >
                일정
              </th>
              <th
                class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
              >
                속성
              </th>
              <th
                class="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase"
              >
                여권
              </th>
              <th
                class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase"
              >
                작업
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr
              v-for="guest in filteredGuests"
              :key="guest.id"
              class="hover:bg-gray-50"
            >
              <td class="px-6 py-4 whitespace-nowrap" @click.stop>
                <input
                  v-model="selectedGuests"
                  type="checkbox"
                  :value="guest.id"
                  class="rounded"
                />
              </td>
              <td
                class="px-6 py-4 whitespace-nowrap cursor-pointer"
                @click="openDetailModal(guest.id)"
              >
                <div class="font-medium text-gray-900">
                  {{ guest.guestName }}
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ guest.telephone }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ guest.corpPart || '-' }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <span
                  v-if="guest.scheduleTemplates.length === 0"
                  class="text-gray-400"
                  >미배정</span
                >
                <div v-else class="flex flex-wrap gap-1">
                  <span
                    v-for="st in guest.scheduleTemplates"
                    :key="st.scheduleTemplateId"
                    class="px-2 py-0.5 bg-blue-100 text-blue-800 rounded text-xs"
                  >
                    {{ st.courseName }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <span v-if="guest.attributes.length === 0" class="text-gray-400"
                  >-</span
                >
                <span v-else class="text-gray-600"
                  >{{ guest.attributes.length }}개</span
                >
              </td>
              <td
                class="px-6 py-4 whitespace-nowrap text-center text-sm"
                @click.stop
              >
                <div class="flex items-center justify-center gap-1">
                  <!-- 여권번호 -->
                  <span
                    class="w-5 h-5 rounded-full inline-flex items-center justify-center text-xs font-bold"
                    :class="
                      guest.passport?.hasNumber
                        ? 'bg-green-100 text-green-700'
                        : 'bg-red-100 text-red-700'
                    "
                    :title="
                      guest.passport?.hasNumber
                        ? '여권번호 입력됨'
                        : '여권번호 미입력'
                    "
                  >
                    #
                  </span>
                  <!-- 만료일 -->
                  <span
                    class="w-5 h-5 rounded-full inline-flex items-center justify-center text-xs font-bold"
                    :class="getExpiryClass(guest.passport)"
                    :title="getExpiryTitle(guest.passport)"
                  >
                    D
                  </span>
                  <!-- 이미지 -->
                  <span
                    class="w-5 h-5 rounded-full inline-flex items-center justify-center text-xs font-bold"
                    :class="
                      guest.passport?.hasImage
                        ? 'bg-green-100 text-green-700'
                        : 'bg-red-100 text-red-700'
                    "
                    :title="
                      guest.passport?.hasImage
                        ? '여권이미지 업로드됨'
                        : '여권이미지 미업로드'
                    "
                  >
                    P
                  </span>
                  <!-- 검증 토글 -->
                  <!-- prettier-ignore -->
                  <button
                    class="w-5 h-5 rounded-full inline-flex items-center justify-center text-xs font-bold ml-1"
                    :class="guest.passport?.passportVerified ? 'bg-blue-600 text-white' : 'bg-gray-200 text-gray-500 hover:bg-gray-300'"
                    :title="guest.passport?.passportVerified ? `검증완료 (${formatDate(guest.passport.passportVerifiedAt)})` : '미검증 — 클릭하여 검증'"
                    @click="togglePassportVerification(guest)"
                  >V</button>
                </div>
              </td>
              <td
                class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium"
                @click.stop
              >
                <button
                  class="text-primary-600 hover:text-primary-900 mr-3"
                  @click="editGuest(guest)"
                >
                  수정
                </button>
                <button
                  class="text-red-600 hover:text-red-900"
                  @click="deleteGuest(guest.id)"
                >
                  삭제
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- 참석자 생성/수정 모달 -->
    <GuestFormModal
      :is-open="showCreateModal || !!editingGuest"
      :editing-guest="editingGuest"
      :available-templates="availableTemplates"
      :available-option-tours="availableOptionTours"
      :attribute-templates="attributeTemplates"
      :guests="guests"
      :convention-id="conventionId"
      @close="closeGuestModal"
      @saved="onGuestSaved"
    />

    <!-- 참석자 상세 모달 -->
    <GuestDetailModal
      :is-open="showDetailModal"
      :convention-id="conventionId"
      :guest-id="detailGuestId"
      @close="closeDetailModal"
    />

    <!-- 대량 작업 모달 (일정 배정 + 속성 매핑) -->
    <BulkGuestOperations
      :convention-id="conventionId"
      :selected-guests="selectedGuests"
      :guests="guests"
      :available-templates="availableTemplates"
      :attribute-templates="attributeTemplates"
      :show-schedule-modal="showBulkAssignModal"
      :show-attribute-modal="showBulkAttributeModal"
      @close-schedule="showBulkAssignModal = false"
      @close-attribute="showBulkAttributeModal = false"
      @completed="onBulkCompleted"
    />

    <!-- 그룹 단위 작업 모달 -->
    <GroupOperations
      :convention-id="conventionId"
      :guests="guests"
      :available-templates="availableTemplates"
      :show-assign-modal="showGroupAssignModal"
      :show-remove-modal="showGroupRemoveModal"
      :show-attribute-reset-modal="showGroupAttributeResetModal"
      @close-assign="showGroupAssignModal = false"
      @close-remove="showGroupRemoveModal = false"
      @close-attribute-reset="showGroupAttributeResetModal = false"
      @completed="onGroupCompleted"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/api'
import GuestFormModal from '@/components/admin/guest/GuestFormModal.vue'
import GuestDetailModal from '@/components/admin/guest/GuestDetailModal.vue'
import BulkGuestOperations from '@/components/admin/guest/BulkGuestOperations.vue'
import GroupOperations from '@/components/admin/guest/GroupOperations.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

// 공유 상태
const guests = ref([])
const availableTemplates = ref([])
const availableOptionTours = ref([])
const attributeTemplates = ref([])
const loading = ref(true)
const selectedGuests = ref([])
const searchTerm = ref('')

const filteredGuests = computed(() => {
  if (!searchTerm.value) return guests.value
  const term = searchTerm.value.toLowerCase()
  return guests.value.filter(
    (g) =>
      (g.guestName && g.guestName.toLowerCase().includes(term)) ||
      (g.telephone && g.telephone.includes(term)) ||
      (g.corpPart && g.corpPart.toLowerCase().includes(term)) ||
      (g.affiliation && g.affiliation.toLowerCase().includes(term)),
  )
})

// 모달 가시성
const showCreateModal = ref(false)
const editingGuest = ref(null)
const showDetailModal = ref(false)
const detailGuestId = ref(null)
const showBulkAssignModal = ref(false)
const showBulkAttributeModal = ref(false)
const showGroupAssignModal = ref(false)
const showGroupRemoveModal = ref(false)
const showGroupAttributeResetModal = ref(false)

// 데이터 로드
const loadGuests = async () => {
  loading.value = true
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/guests`,
    )
    guests.value = response.data
  } catch (error) {
    console.error('Failed to load guests:', error)
  } finally {
    loading.value = false
  }
}

const loadTemplates = async () => {
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/schedule-templates`,
    )
    availableTemplates.value = response.data
  } catch (error) {
    console.error('Failed to load templates:', error)
  }
}

const loadOptionTours = async () => {
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/option-tours`,
    )
    availableOptionTours.value = response.data
  } catch (error) {
    console.error('Failed to load option tours:', error)
  }
}

const loadAttributeTemplates = async () => {
  try {
    const response = await apiClient.get(
      `/attributetemplate/conventions/${props.conventionId}`,
    )
    attributeTemplates.value = response.data
  } catch (error) {
    console.error('Failed to load attribute templates:', error)
  }
}

// 여권 상태 헬퍼
const getExpiryClass = (passport) => {
  if (!passport?.hasExpiry) return 'bg-red-100 text-red-700'
  const expiry = passport.passportExpiryDate
  if (!expiry) return 'bg-red-100 text-red-700'
  const expiryDate = new Date(expiry)
  const sixMonthsFromNow = new Date()
  sixMonthsFromNow.setMonth(sixMonthsFromNow.getMonth() + 6)
  if (expiryDate < new Date()) return 'bg-red-100 text-red-700'
  if (expiryDate < sixMonthsFromNow) return 'bg-yellow-100 text-yellow-700'
  return 'bg-green-100 text-green-700'
}

const getExpiryTitle = (passport) => {
  if (!passport?.hasExpiry) return '만료일 미입력'
  const expiry = passport.passportExpiryDate
  if (!expiry) return '만료일 미입력'
  const expiryDate = new Date(expiry)
  if (expiryDate < new Date()) return `만료됨 (${expiry})`
  const sixMonthsFromNow = new Date()
  sixMonthsFromNow.setMonth(sixMonthsFromNow.getMonth() + 6)
  if (expiryDate < sixMonthsFromNow) return `6개월 이내 만료 (${expiry})`
  return `유효 (${expiry})`
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleDateString('ko-KR')
}

const togglePassportVerification = async (guest) => {
  const currentStatus = guest.passport?.passportVerified
  const action = currentStatus ? '검증 해제' : '검증 완료'
  if (!confirm(`${guest.guestName}님의 여권을 ${action} 처리하시겠습니까?`))
    return

  try {
    await apiClient.put(`/admin/users/${guest.id}/passport-verification`, {
      verified: !currentStatus,
    })
    await loadGuests()
  } catch (error) {
    console.error('Failed to toggle passport verification:', error)
    alert('처리 실패')
  }
}

// 테이블 조작
const toggleSelectAll = (event) => {
  if (event.target.checked) {
    selectedGuests.value = filteredGuests.value.map((g) => g.id)
  } else {
    selectedGuests.value = []
  }
}

const deleteGuest = async (id) => {
  if (!confirm('참석자를 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/admin/guests/${id}`)
    await loadGuests()
  } catch (error) {
    console.error('Failed to delete guest:', error)
    alert('삭제 실패')
  }
}

// 수정 모달 열기
const editGuest = (guest) => {
  editingGuest.value = guest
}

// 참석자 생성/수정 모달 닫기
const closeGuestModal = () => {
  showCreateModal.value = false
  editingGuest.value = null
}

// 저장 완료 핸들러
const onGuestSaved = async () => {
  closeGuestModal()
  await loadGuests()
}

// 상세 모달
const openDetailModal = (guestId) => {
  detailGuestId.value = guestId
  showDetailModal.value = true
}

const closeDetailModal = () => {
  showDetailModal.value = false
  detailGuestId.value = null
}

// 대량 작업 완료 핸들러
const onBulkCompleted = async () => {
  selectedGuests.value = []
  await loadGuests()
  await loadTemplates()
}

// 그룹 작업 완료 핸들러
const onGroupCompleted = async () => {
  await loadGuests()
  await loadTemplates()
}

onMounted(() => {
  loadGuests()
  loadTemplates()
  loadOptionTours()
  loadAttributeTemplates()
})
</script>
