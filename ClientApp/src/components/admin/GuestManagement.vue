<template>
  <div>
    <AdminPageHeader
      title="참석자 관리"
      :description="`전체 ${guests.length}명`"
    >
      <template v-if="selectedGuests.length > 0">
        <span
          class="px-3 py-1 bg-primary-100 text-primary-800 rounded-full text-sm font-medium"
        >
          {{ selectedGuests.length }}명 선택됨
        </span>
        <AdminButton
          variant="secondary"
          :icon="ClipboardCheck"
          @click="showBulkAssignModal = true"
        >
          일정 일괄 배정
        </AdminButton>
        <AdminButton
          variant="secondary"
          :icon="Tag"
          @click="showBulkAttributeModal = true"
        >
          속성 일괄 매핑
        </AdminButton>
      </template>
      <AdminButton
        variant="secondary"
        :icon="Users"
        @click="showGroupAssignModal = true"
      >
        그룹별 일정 배정
      </AdminButton>
      <AdminButton
        variant="danger"
        :icon="Trash2"
        @click="showGroupRemoveModal = true"
      >
        그룹별 일정 해제
      </AdminButton>
      <AdminButton
        variant="secondary"
        :icon="RefreshCw"
        @click="showGroupAttributeResetModal = true"
      >
        그룹별 속성 초기화
      </AdminButton>
      <AdminButton :icon="Plus" @click="showCreateModal = true">
        참석자 추가
      </AdminButton>
    </AdminPageHeader>

    <!-- 검색 -->
    <div class="mt-6 mb-4">
      <input
        v-model="searchTerm"
        type="text"
        placeholder="이름, 전화번호, 부서, 소속으로 검색..."
        class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
      />
    </div>

    <div v-if="loading" class="text-center py-8">로딩 중...</div>
    <AdminEmptyState
      v-else-if="guests.length === 0"
      :icon="UserX"
      title="등록된 참석자가 없습니다"
    >
      <AdminButton :icon="Plus" @click="showCreateModal = true"
        >첫 참석자 추가하기</AdminButton
      >
    </AdminEmptyState>
    <AdminEmptyState
      v-else-if="filteredGuests.length === 0 && searchTerm"
      :icon="Search"
      :title="`&quot;${searchTerm}&quot; 검색 결과가 없습니다`"
      :description="`전체 ${guests.length}명`"
    />
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
                    class="px-2 py-0.5 bg-primary-100 text-primary-800 rounded text-xs"
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
              <td class="px-6 py-4 whitespace-nowrap text-sm" @click.stop>
                <div class="flex flex-col gap-1">
                  <!-- 여권 항목별 상태 -->
                  <div class="flex items-center gap-2 text-xs">
                    <span
                      class="inline-block w-2 h-2 rounded-full flex-shrink-0"
                      :class="
                        guest.passport?.hasNumber
                          ? 'bg-green-500'
                          : 'bg-red-400'
                      "
                    ></span>
                    <span
                      :class="
                        guest.passport?.hasNumber
                          ? 'text-gray-700'
                          : 'text-red-500'
                      "
                    >
                      {{ guest.passport?.hasNumber ? '번호' : '번호 미입력' }}
                    </span>
                  </div>
                  <div class="flex items-center gap-2 text-xs">
                    <span
                      class="inline-block w-2 h-2 rounded-full flex-shrink-0"
                      :class="getExpiryDotClass(guest.passport)"
                    ></span>
                    <span :class="getExpiryTextClass(guest.passport)">
                      {{ getExpiryLabel(guest.passport) }}
                    </span>
                  </div>
                  <div class="flex items-center gap-2 text-xs">
                    <span
                      class="inline-block w-2 h-2 rounded-full flex-shrink-0"
                      :class="
                        guest.passport?.hasImage ? 'bg-green-500' : 'bg-red-400'
                      "
                    ></span>
                    <span
                      :class="
                        guest.passport?.hasImage
                          ? 'text-gray-700'
                          : 'text-red-500'
                      "
                    >
                      {{ guest.passport?.hasImage ? '사본' : '사본 미등록' }}
                    </span>
                  </div>
                  <!-- 검증 토글 -->
                  <!-- prettier-ignore -->
                  <button
                    class="mt-1 px-2 py-0.5 rounded text-xs font-medium transition-colors"
                    :class="guest.passport?.passportVerified ? 'bg-primary-100 text-primary-700 hover:bg-primary-200' : 'bg-gray-100 text-gray-500 hover:bg-gray-200'"
                    :title="guest.passport?.passportVerified ? `검증완료 (${formatDate(guest.passport.passportVerifiedAt)})` : '클릭하여 검증 완료 처리'"
                    @click="togglePassportVerification(guest)"
                  >{{ guest.passport?.passportVerified ? '검증완료' : '미검증' }}</button>
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
import {
  Plus,
  ClipboardCheck,
  Tag,
  Users,
  Trash2,
  RefreshCw,
  UserX,
  Search,
} from 'lucide-vue-next'
import apiClient from '@/services/api'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminButton from '@/components/admin/ui/AdminButton.vue'
import AdminEmptyState from '@/components/admin/ui/AdminEmptyState.vue'
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
const getExpiryStatus = (passport) => {
  if (!passport?.hasExpiry || !passport.passportExpiryDate) return 'missing'
  const expiryDate = new Date(passport.passportExpiryDate)
  if (expiryDate < new Date()) return 'expired'
  const sixMonths = new Date()
  sixMonths.setMonth(sixMonths.getMonth() + 6)
  if (expiryDate < sixMonths) return 'expiring'
  return 'valid'
}

const getExpiryDotClass = (passport) => {
  const status = getExpiryStatus(passport)
  if (status === 'valid') return 'bg-green-500'
  if (status === 'expiring') return 'bg-yellow-500'
  return 'bg-red-400'
}

const getExpiryTextClass = (passport) => {
  const status = getExpiryStatus(passport)
  if (status === 'valid') return 'text-gray-700'
  if (status === 'expiring') return 'text-yellow-600'
  return 'text-red-500'
}

const getExpiryLabel = (passport) => {
  const status = getExpiryStatus(passport)
  const date = passport?.passportExpiryDate
  if (status === 'missing') return '만료일 미입력'
  if (status === 'expired') return `만료됨 (${date})`
  if (status === 'expiring') return `곧 만료 (${date})`
  return `유효 (${date})`
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
