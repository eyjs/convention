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

    <!-- 검색 + 필터 -->
    <div class="mt-6 mb-4 flex flex-wrap items-center gap-2">
      <input
        v-model="searchTerm"
        type="text"
        placeholder="이름, 전화번호, 부서, 소속으로 검색..."
        class="flex-1 min-w-0 px-3 py-1.5 border rounded-lg text-sm focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
      />
      <select
        v-model="passportFilter"
        class="px-3 py-1.5 border rounded-lg text-sm text-gray-700 focus:ring-2 focus:ring-primary-500"
      >
        <option value="">여권 전체</option>
        <option value="noNumber">번호 미입력</option>
        <option value="noExpiry">만료일 미입력</option>
        <option value="expired">만료됨</option>
        <option value="expiring">6개월 내 만료</option>
        <option value="noImage">사본 미등록</option>
        <option value="unverified">미검증</option>
        <option value="complete">여권 완비</option>
      </select>
      <select
        v-model="companionFilter"
        class="px-3 py-1.5 border rounded-lg text-sm text-gray-700 focus:ring-2 focus:ring-primary-500"
      >
        <option value="">동반자 전체</option>
        <option value="hasCompanion">동반자 있음</option>
        <option value="isCompanion">동반자로 등록됨</option>
        <option value="noCompanion">동반자 없음</option>
      </select>
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
      v-else-if="
        filteredGuests.length === 0 &&
        (searchTerm || passportFilter || companionFilter)
      "
      :icon="Search"
      title="검색 결과가 없습니다"
      :description="`전체 ${guests.length}명 중 조건에 맞는 참석자가 없습니다`"
    />
    <div v-else>
      <!-- 모바일 카드 뷰 -->
      <div class="md:hidden space-y-2">
        <div
          v-for="guest in filteredGuests"
          :key="'m-' + guest.id"
          class="bg-white rounded-lg shadow-sm p-3 flex items-center gap-3 active:bg-gray-50"
          @click="openDetailModal(guest.id)"
        >
          <input
            v-model="selectedGuests"
            type="checkbox"
            :value="guest.id"
            class="rounded flex-shrink-0"
            @click.stop
          />
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2">
              <span class="font-semibold text-gray-900">{{ guest.guestName }}</span>
              <span v-if="guest.groupName" class="text-xs px-1.5 py-0.5 bg-blue-50 text-blue-700 rounded">{{ guest.groupName }}</span>
            </div>
            <div class="text-xs text-gray-500 mt-0.5 flex flex-wrap gap-x-2">
              <span v-if="guest.phone">{{ guest.phone }}</span>
              <span v-if="guest.corpPart">{{ guest.corpPart }}</span>
            </div>
          </div>
          <div class="flex items-center gap-1 flex-shrink-0">
            <span v-if="guest.passportVerified" class="w-5 h-5 rounded-full bg-green-100 text-green-600 flex items-center justify-center text-[10px]">✓</span>
            <svg class="w-4 h-4 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" /></svg>
          </div>
        </div>
      </div>

      <!-- PC 테이블 뷰 -->
      <div class="hidden md:block bg-white rounded-lg shadow overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200 text-sm">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-3 py-2 text-left w-8">
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
              <th class="px-3 py-2 text-left text-xs font-medium text-gray-500">
                이름
              </th>
              <th class="px-3 py-2 text-left text-xs font-medium text-gray-500">
                연락처
              </th>
              <th class="px-3 py-2 text-left text-xs font-medium text-gray-500">
                부서
              </th>
              <th class="px-3 py-2 text-left text-xs font-medium text-gray-500">
                그룹
              </th>
              <th class="px-3 py-2 text-left text-xs font-medium text-gray-500">
                동반자
              </th>
              <th class="px-3 py-2 text-left text-xs font-medium text-gray-500">
                일정
              </th>
              <th
                class="px-3 py-2 text-center text-xs font-medium text-gray-500"
              >
                속성
              </th>
              <th
                class="px-3 py-2 text-center text-xs font-medium text-gray-500"
              >
                여권
              </th>
              <th
                class="px-3 py-2 text-right text-xs font-medium text-gray-500"
              >
                작업
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr
              v-for="guest in filteredGuests"
              :key="guest.id"
              class="hover:bg-gray-50 cursor-pointer"
              @click="openDetailModal(guest.id)"
            >
              <td class="px-3 py-1.5 whitespace-nowrap" @click.stop>
                <input
                  v-model="selectedGuests"
                  type="checkbox"
                  :value="guest.id"
                  class="rounded"
                />
              </td>
              <td
                class="px-3 py-1.5 whitespace-nowrap font-medium text-gray-900"
              >
                {{ guest.guestName }}
              </td>
              <td class="px-3 py-1.5 whitespace-nowrap text-gray-500">
                {{ guest.telephone }}
              </td>
              <td class="px-3 py-1.5 whitespace-nowrap text-gray-500">
                {{ guest.corpPart || '-' }}
              </td>
              <td class="px-3 py-1.5 whitespace-nowrap text-gray-500">
                {{ guest.groupName || '-' }}
              </td>
              <td class="px-3 py-1.5 whitespace-nowrap">
                <span
                  v-if="!guest.companions?.length && !getCompanionOf(guest.id)"
                  class="text-gray-400"
                  >-</span
                >
                <div v-else class="flex flex-wrap gap-0.5">
                  <span
                    v-for="c in guest.companions"
                    :key="c.id"
                    class="px-1.5 py-0.5 bg-blue-50 text-blue-700 rounded text-xs leading-tight"
                    :title="c.relationType"
                  >
                    {{ c.name }} ({{ c.relationType }})
                  </span>
                  <span
                    v-if="getCompanionOf(guest.id)"
                    class="px-1.5 py-0.5 bg-orange-50 text-orange-700 rounded text-xs leading-tight"
                    :title="`${getCompanionOf(guest.id).userName}의 동반자`"
                  >
                    ← {{ getCompanionOf(guest.id).userName }}
                  </span>
                </div>
              </td>
              <td class="px-3 py-1.5 whitespace-nowrap">
                <span
                  v-if="guest.scheduleTemplates.length === 0"
                  class="text-gray-400"
                  >미배정</span
                >
                <div v-else class="flex flex-wrap gap-0.5">
                  <span
                    v-for="st in guest.scheduleTemplates"
                    :key="st.scheduleTemplateId"
                    class="px-1.5 py-0.5 bg-primary-50 text-primary-700 rounded text-xs leading-tight"
                  >
                    {{ st.courseName }}
                  </span>
                </div>
              </td>
              <td
                class="px-3 py-1.5 whitespace-nowrap text-center text-gray-500"
              >
                {{ guest.attributes.length || '-' }}
              </td>
              <td class="px-3 py-1.5 whitespace-nowrap text-center" @click.stop>
                <div
                  class="inline-flex items-center gap-1"
                  :title="getPassportTooltip(guest.passport)"
                >
                  <span
                    class="w-2 h-2 rounded-full"
                    :class="
                      guest.passport?.hasNumber ? 'bg-green-500' : 'bg-red-400'
                    "
                    title="여권번호"
                  />
                  <span
                    class="w-2 h-2 rounded-full"
                    :class="getExpiryDotClass(guest.passport)"
                    title="만료일"
                  />
                  <span
                    class="w-2 h-2 rounded-full"
                    :class="
                      guest.passport?.hasImage ? 'bg-green-500' : 'bg-red-400'
                    "
                    title="사본"
                  />
                  <!-- prettier-ignore -->
                  <button
                    class="ml-1 px-1.5 py-0.5 rounded text-xs font-medium transition-colors"
                    :class="guest.passport?.passportVerified ? 'bg-primary-100 text-primary-700 hover:bg-primary-200' : 'bg-gray-100 text-gray-500 hover:bg-gray-200'"
                    @click="togglePassportVerification(guest)"
                  >{{ guest.passport?.passportVerified ? '검증' : '미검증' }}</button>
                </div>
              </td>
              <td class="px-3 py-1.5 whitespace-nowrap text-right" @click.stop>
                <button
                  class="text-gray-400 hover:text-primary-600 p-1 min-h-[36px] min-w-[36px] inline-flex items-center justify-center"
                  title="수정"
                  @click="editGuest(guest)"
                >
                  <Pencil :size="14" />
                </button>
                <button
                  class="text-gray-400 hover:text-red-600 p-1 min-h-[36px] min-w-[36px] inline-flex items-center justify-center"
                  title="삭제"
                  @click="deleteGuest(guest.id)"
                >
                  <Trash2 :size="14" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
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
  Pencil,
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
const passportFilter = ref('')
const companionFilter = ref('')

// 동반자 역방향 조회: 이 사용자가 누구의 동반자인지
const companionOfMap = computed(() => {
  const map = {}
  for (const g of guests.value) {
    if (g.companions) {
      for (const c of g.companions) {
        map[c.companionUserId] = { userName: g.guestName, userId: g.id }
      }
    }
  }
  return map
})

const getCompanionOf = (guestId) => companionOfMap.value[guestId] || null

const filteredGuests = computed(() => {
  let result = guests.value

  // 텍스트 검색
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(
      (g) =>
        (g.guestName && g.guestName.toLowerCase().includes(term)) ||
        (g.telephone && g.telephone.includes(term)) ||
        (g.corpPart && g.corpPart.toLowerCase().includes(term)) ||
        (g.affiliation && g.affiliation.toLowerCase().includes(term)),
    )
  }

  // 여권 필터
  if (passportFilter.value) {
    result = result.filter((g) => {
      const p = g.passport
      switch (passportFilter.value) {
        case 'noNumber':
          return !p?.hasNumber
        case 'noExpiry':
          return !p?.hasExpiry
        case 'expired':
          return getExpiryStatus(p) === 'expired'
        case 'expiring':
          return getExpiryStatus(p) === 'expiring'
        case 'noImage':
          return !p?.hasImage
        case 'unverified':
          return !p?.passportVerified
        case 'complete':
          return (
            p?.hasNumber && p?.hasExpiry && p?.hasImage && p?.passportVerified
          )
        default:
          return true
      }
    })
  }

  // 동반자 필터
  if (companionFilter.value) {
    result = result.filter((g) => {
      switch (companionFilter.value) {
        case 'hasCompanion':
          return g.companions?.length > 0
        case 'isCompanion':
          return !!getCompanionOf(g.id)
        case 'noCompanion':
          return (
            (!g.companions || g.companions.length === 0) &&
            !getCompanionOf(g.id)
          )
        default:
          return true
      }
    })
  }

  return result
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

const getPassportTooltip = (passport) => {
  const number = passport?.hasNumber ? '번호: 입력됨' : '번호: 미입력'
  const expiry = getExpiryLabel(passport)
  const image = passport?.hasImage ? '사본: 등록됨' : '사본: 미등록'
  return `${number}\n만료일: ${expiry}\n${image}`
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
