<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-xl font-semibold">옵션투어 관리</h2>
      <button
        class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
        @click="openCreateModal"
      >
        + 옵션투어 추가
      </button>
    </div>

    <!-- 옵션투어 목록 -->
    <div class="space-y-4">
      <div
        v-for="tour in optionTours"
        :key="tour.id"
        class="bg-white rounded-lg shadow overflow-hidden"
      >
        <div class="p-4 sm:p-6 flex items-start justify-between">
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-1">
              <h3 class="font-semibold text-lg">{{ tour.name }}</h3>
              <span
                class="px-2 py-0.5 text-xs bg-gray-100 text-gray-600 rounded-full"
              >
                옵션 #{{ tour.customOptionId }}
              </span>
            </div>
            <div class="text-sm text-gray-600">
              <span class="font-medium">{{ formatDate(tour.date) }}</span>
              <span class="ml-2 text-primary-600 font-semibold">
                {{ tour.startTime }}
                <span v-if="tour.endTime" class="text-gray-500">
                  ~ {{ tour.endTime }}
                </span>
              </span>
            </div>
            <p
              v-if="tour.content"
              class="text-sm text-gray-500 mt-1 line-clamp-2"
            >
              {{ tour.content }}
            </p>
            <p class="text-xs text-gray-500 mt-1">
              참석자: {{ tour.participantCount || 0 }}명
            </p>
          </div>
          <div class="flex gap-2 ml-4 flex-shrink-0">
            <button
              class="px-3 py-1.5 text-sm bg-blue-50 text-blue-600 rounded hover:bg-blue-100"
              @click="viewParticipants(tour)"
            >
              참석자 보기
            </button>
            <button
              class="px-3 py-1.5 text-sm bg-white border rounded hover:bg-gray-50"
              @click="openEditModal(tour)"
            >
              수정
            </button>
            <button
              class="px-3 py-1.5 text-sm bg-red-50 text-red-600 rounded hover:bg-red-100"
              @click="deleteTour(tour.id)"
            >
              삭제
            </button>
          </div>
        </div>
      </div>

      <div
        v-if="optionTours.length === 0"
        class="text-center py-12 text-gray-500 bg-white rounded-lg shadow"
      >
        등록된 옵션투어가 없습니다. 옵션투어를 추가해주세요.
      </div>
    </div>

    <!-- 생성/수정 모달 -->
    <BaseModal :is-open="showFormModal" max-width="md" @close="closeFormModal">
      <template #header>
        <h2 class="text-xl font-semibold">
          {{ editingTour ? '옵션투어 수정' : '옵션투어 추가' }}
        </h2>
      </template>
      <template #body>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium mb-1">옵션명 *</label>
            <input
              v-model="form.name"
              type="text"
              class="w-full px-3 py-2 border rounded-lg"
              placeholder="예: 바뚜르산 투어"
            />
          </div>

          <div>
            <label class="block text-sm font-medium mb-1">날짜 *</label>
            <input
              v-model="form.date"
              type="date"
              class="w-full px-3 py-2 border rounded-lg"
            />
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-sm font-medium mb-1">시작 시간 *</label>
              <input
                v-model="form.startTime"
                type="time"
                class="w-full px-3 py-2 border rounded-lg"
              />
            </div>
            <div>
              <label class="block text-sm font-medium mb-1">종료 시간</label>
              <input
                v-model="form.endTime"
                type="time"
                class="w-full px-3 py-2 border rounded-lg"
              />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium mb-1">옵션 ID *</label>
            <input
              v-model.number="form.customOptionId"
              type="number"
              class="w-full px-3 py-2 border rounded-lg"
              placeholder="엑셀 매핑용 옵션 ID"
            />
          </div>

          <div>
            <label class="block text-sm font-medium mb-1">상세 내용</label>
            <textarea
              v-model="form.content"
              class="w-full px-3 py-2 border rounded-lg"
              rows="4"
              placeholder="옵션투어에 대한 상세 설명"
            ></textarea>
          </div>
        </div>
      </template>
      <template #footer>
        <button
          class="px-4 py-2 border rounded-lg hover:bg-gray-50"
          @click="closeFormModal"
        >
          취소
        </button>
        <button
          class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
          @click="saveTour"
        >
          저장
        </button>
      </template>
    </BaseModal>

    <!-- 참석자 목록 모달 -->
    <BaseModal
      :is-open="showParticipantsModal"
      max-width="2xl"
      @close="closeParticipantsModal"
    >
      <template #header>
        <div class="flex justify-between items-center w-full">
          <h2 class="text-xl font-semibold">
            {{ selectedTour?.name }} - 참석자 목록
          </h2>
          <button
            class="px-3 py-1.5 text-sm bg-primary-600 text-white rounded hover:bg-primary-700"
            @click="showAddParticipantModal = true"
          >
            + 참석자 추가
          </button>
        </div>
      </template>
      <template #body>
        <div
          v-if="participants.length === 0"
          class="text-center py-8 text-gray-500"
        >
          이 옵션투어에 등록된 참석자가 없습니다.
        </div>

        <div v-else class="space-y-2">
          <div
            v-for="participant in participants"
            :key="participant.id"
            class="p-4 border rounded-lg hover:bg-gray-50"
          >
            <div class="flex justify-between items-start">
              <div>
                <p class="font-medium">{{ participant.name }}</p>
                <p class="text-sm text-gray-600">{{ participant.phone }}</p>
                <p v-if="participant.corpPart" class="text-sm text-gray-500">
                  {{ participant.corpPart }}
                </p>
                <p v-if="participant.affiliation" class="text-sm text-gray-500">
                  {{ participant.affiliation }}
                </p>
              </div>
              <button
                class="px-3 py-1 text-sm text-red-600 hover:bg-red-50 rounded"
                @click="removeParticipant(participant.id)"
              >
                제거
              </button>
            </div>
          </div>
        </div>
      </template>
    </BaseModal>

    <!-- 참석자 추가 모달 -->
    <BaseModal
      :is-open="showAddParticipantModal"
      max-width="2xl"
      @close="showAddParticipantModal = false"
    >
      <template #header>
        <h2 class="text-xl font-semibold">참석자 추가</h2>
      </template>
      <template #body>
        <div class="mb-4">
          <input
            v-model="searchQuery"
            type="text"
            class="w-full px-3 py-2 border rounded-lg"
            placeholder="이름, 전화번호, 소속으로 검색..."
          />
        </div>

        <div class="max-h-96 overflow-y-auto space-y-2">
          <div
            v-for="guest in filteredGuests"
            :key="guest.id"
            class="p-3 border rounded-lg hover:bg-gray-50 cursor-pointer flex items-center gap-3"
            @click="toggleGuestSelection(guest.id)"
          >
            <input
              type="checkbox"
              :checked="selectedGuestIds.includes(guest.id)"
              class="rounded"
              @click.stop
              @change="toggleGuestSelection(guest.id)"
            />
            <div class="flex-1">
              <p class="font-medium">{{ guest.name }}</p>
              <p class="text-sm text-gray-600">
                {{ guest.phone }}
                <span v-if="guest.affiliation" class="ml-2">
                  {{ guest.affiliation }}
                </span>
              </p>
            </div>
          </div>

          <div
            v-if="filteredGuests.length === 0"
            class="text-center py-8 text-gray-500"
          >
            검색 결과가 없습니다.
          </div>
        </div>
      </template>
      <template #footer>
        <div class="w-full flex justify-between items-center">
          <span class="text-sm text-gray-600">
            {{ selectedGuestIds.length }}명 선택됨
          </span>
          <div class="flex gap-2">
            <button
              class="px-4 py-2 border rounded-lg hover:bg-gray-50"
              @click="showAddParticipantModal = false"
            >
              취소
            </button>
            <button
              class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50"
              :disabled="selectedGuestIds.length === 0"
              @click="addSelectedParticipants"
            >
              추가
            </button>
          </div>
        </div>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const optionTours = ref([])
const showFormModal = ref(false)
const editingTour = ref(null)
const showParticipantsModal = ref(false)
const showAddParticipantModal = ref(false)
const selectedTour = ref(null)
const participants = ref([])
const existingParticipantIds = ref([])
const allGuests = ref([])
const searchQuery = ref('')
const selectedGuestIds = ref([])

const form = ref({
  name: '',
  date: '',
  startTime: '',
  endTime: '',
  customOptionId: 0,
  content: '',
})

const formatDate = (dateStr) => {
  const date = new Date(dateStr)
  return (
    date.toLocaleDateString('ko-KR', {
      month: 'numeric',
      day: 'numeric',
    }) + '일'
  )
}

const filteredGuests = computed(() => {
  const query = searchQuery.value.toLowerCase().trim()
  const guests = allGuests.value.filter(
    (g) => !existingParticipantIds.value.includes(g.id),
  )
  if (!query) return guests
  return guests.filter(
    (g) =>
      g.name?.toLowerCase().includes(query) ||
      g.phone?.toLowerCase().includes(query) ||
      g.affiliation?.toLowerCase().includes(query),
  )
})

// === 옵션투어 CRUD ===

const loadOptionTours = async () => {
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/option-tours`,
    )
    optionTours.value = response.data
  } catch (error) {
    console.error('Failed to load option tours:', error)
  }
}

const openCreateModal = () => {
  editingTour.value = null
  form.value = {
    name: '',
    date: '',
    startTime: '',
    endTime: '',
    customOptionId: 0,
    content: '',
  }
  showFormModal.value = true
}

const openEditModal = (tour) => {
  editingTour.value = tour
  form.value = {
    name: tour.name,
    date: tour.date,
    startTime: tour.startTime,
    endTime: tour.endTime || '',
    customOptionId: tour.customOptionId,
    content: tour.content || '',
  }
  showFormModal.value = true
}

const closeFormModal = () => {
  showFormModal.value = false
  editingTour.value = null
}

const saveTour = async () => {
  if (!form.value.name.trim() || !form.value.date || !form.value.startTime) {
    alert('옵션명, 날짜, 시작 시간은 필수입니다.')
    return
  }

  try {
    if (editingTour.value) {
      await apiClient.put(
        `/admin/option-tours/${editingTour.value.id}`,
        form.value,
      )
    } else {
      await apiClient.post(
        `/admin/conventions/${props.conventionId}/option-tours`,
        form.value,
      )
    }
    await loadOptionTours()
    closeFormModal()
  } catch (error) {
    console.error('Failed to save option tour:', error)
    alert(
      '옵션투어 저장 실패: ' + (error.response?.data?.message || error.message),
    )
  }
}

const deleteTour = async (id) => {
  if (
    !confirm(
      '옵션투어를 삭제하면 참석자 매핑도 함께 삭제됩니다. 계속하시겠습니까?',
    )
  )
    return

  try {
    await apiClient.delete(`/admin/option-tours/${id}`)
    await loadOptionTours()
  } catch (error) {
    console.error('Failed to delete option tour:', error)
    alert(
      '옵션투어 삭제 실패: ' + (error.response?.data?.message || error.message),
    )
  }
}

// === 참석자 관리 ===

const viewParticipants = async (tour) => {
  selectedTour.value = tour
  try {
    const response = await apiClient.get(
      `/admin/option-tours/${tour.id}/participants`,
    )
    participants.value = response.data
    existingParticipantIds.value = response.data.map((p) => p.id)
    showParticipantsModal.value = true
  } catch (error) {
    console.error('Failed to load participants:', error)
    alert(
      '참석자 목록 로드 실패: ' +
        (error.response?.data?.message || error.message),
    )
  }
}

const closeParticipantsModal = () => {
  showParticipantsModal.value = false
  selectedTour.value = null
  participants.value = []
  existingParticipantIds.value = []
}

const removeParticipant = async (userId) => {
  if (!confirm('이 참석자를 옵션투어에서 제거하시겠습니까?')) return

  try {
    await apiClient.delete(
      `/admin/option-tours/${selectedTour.value.id}/participants/${userId}`,
    )
    await viewParticipants(selectedTour.value)
    await loadOptionTours()
  } catch (error) {
    console.error('Failed to remove participant:', error)
    alert(
      '참석자 제거 실패: ' + (error.response?.data?.message || error.message),
    )
  }
}

const loadAllGuests = async () => {
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/guests`,
    )
    allGuests.value = response.data || []
  } catch (error) {
    console.error('Failed to load guests:', error)
  }
}

const toggleGuestSelection = (guestId) => {
  const index = selectedGuestIds.value.indexOf(guestId)
  if (index === -1) {
    selectedGuestIds.value.push(guestId)
  } else {
    selectedGuestIds.value.splice(index, 1)
  }
}

const addSelectedParticipants = async () => {
  if (selectedGuestIds.value.length === 0) return

  try {
    await apiClient.post(
      `/admin/option-tours/${selectedTour.value.id}/participants`,
      { userIds: selectedGuestIds.value },
    )
    showAddParticipantModal.value = false
    selectedGuestIds.value = []
    searchQuery.value = ''
    await viewParticipants(selectedTour.value)
    await loadOptionTours()
  } catch (error) {
    console.error('Failed to add participants:', error)
    alert(
      '참석자 추가 실패: ' + (error.response?.data?.message || error.message),
    )
  }
}

onMounted(() => {
  loadOptionTours()
  loadAllGuests()
})
</script>
