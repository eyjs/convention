<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader :title="isEditMode ? '여행 수정' : '새 여행'" :show-back="true" />

    <div class="max-w-2xl mx-auto px-4 py-6">
      <!-- 로딩 상태 -->
      <div v-if="loading" class="text-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-4 text-gray-600">로딩 중...</p>
      </div>

      <template v-else>
        <!-- 여행 기본 정보 -->
        <div class="bg-white rounded-lg shadow p-4 mb-6">
          <h2 class="text-lg font-bold mb-4">여행 정보</h2>

          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">여행 제목 *</label>
              <input v-model="tripData.title" type="text" required class="w-full px-3 py-2 border border-gray-300 rounded-lg" placeholder="예: 2025 싱가포르 여행" />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">설명</label>
              <textarea v-model="tripData.description" rows="3" class="w-full px-3 py-2 border border-gray-300 rounded-lg" placeholder="여행에 대한 메모를 입력하세요"></textarea>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">시작일 *</label>
                <input v-model="tripData.startDate" type="date" required class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">종료일 *</label>
                <input v-model="tripData.endDate" type="date" required class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">국가</label>
                <input v-model="tripData.destination" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" placeholder="예: 싱가포르" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">도시</label>
                <input v-model="tripData.city" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" placeholder="예: 싱가포르" />
              </div>
            </div>

            <button @click="saveTripInfo" class="w-full py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
              {{ isEditMode ? '여행 정보 저장' : '여행 만들기' }}
            </button>

            <button v-if="isEditMode" @click="deleteTrip" class="w-full py-2 bg-red-600 text-white rounded-lg hover:bg-red-700">
              여행 삭제
            </button>
          </div>
        </div>

        <!-- 항공권 및 숙소는 여행이 생성된 후에만 표시 -->
        <template v-if="isEditMode">
          <!-- 항공권 관리 -->
          <div class="bg-white rounded-lg shadow p-4 mb-6">
            <div class="flex justify-between items-center mb-4">
              <h2 class="text-lg font-bold">항공권</h2>
              <button @click="openFlightModal()" class="px-3 py-1 bg-blue-600 text-white rounded hover:bg-blue-700 text-sm">
                + 추가
              </button>
            </div>

            <div v-if="trip.flights.length === 0" class="text-center py-8 text-gray-500">
              등록된 항공권이 없습니다.
            </div>

            <div v-else class="space-y-3">
              <div v-for="flight in trip.flights" :key="flight.id" @click="openFlightModal(flight)" class="border rounded-lg p-3 hover:bg-gray-50 cursor-pointer">
                <div class="flex justify-between items-start">
                  <div class="flex-1">
                    <div class="font-medium">{{ flight.airline }} {{ flight.flightNumber }}</div>
                    <div class="text-sm text-gray-600 mt-1">
                      {{ flight.departureLocation }} → {{ flight.arrivalLocation }}
                    </div>
                    <div class="text-xs text-gray-500 mt-1">
                      {{ formatDateTime(flight.departureTime) }}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 숙소 관리 -->
          <div class="bg-white rounded-lg shadow p-4 mb-6">
            <div class="flex justify-between items-center mb-4">
              <h2 class="text-lg font-bold">숙소</h2>
              <button @click="openAccommodationModal()" class="px-3 py-1 bg-blue-600 text-white rounded hover:bg-blue-700 text-sm">
                + 추가
              </button>
            </div>

            <div v-if="trip.accommodations.length === 0" class="text-center py-8 text-gray-500">
              등록된 숙소가 없습니다.
            </div>

            <div v-else class="space-y-3">
              <div v-for="accommodation in trip.accommodations" :key="accommodation.id" @click="openAccommodationModal(accommodation)" class="border rounded-lg p-3 hover:bg-gray-50 cursor-pointer">
                <div class="font-medium">{{ accommodation.name }}</div>
                <div v-if="accommodation.type" class="text-sm text-gray-600 mt-1">{{ accommodation.type }}</div>
                <div v-if="accommodation.address" class="text-sm text-gray-500 mt-1">{{ accommodation.address }}</div>
                <div class="text-xs text-gray-500 mt-1">
                  체크인: {{ formatDateTime(accommodation.checkInTime) }}
                </div>
              </div>
            </div>
          </div>
        </template>
      </template>
    </div>

    <!-- 항공권 모달 -->
    <SlideUpModal :is-open="isFlightModalOpen" @close="closeFlightModal">
      <template #header-title>{{ editingFlight?.id ? '항공권 수정' : '항공권 추가' }}</template>
      <template #body>
        <form id="flight-form" @submit.prevent="saveFlight">
          <div class="space-y-4">
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">항공사</label>
                <input v-model="flightData.airline" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">편명</label>
                <input v-model="flightData.flightNumber" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">출발지</label>
                <input v-model="flightData.departureLocation" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">도착지</label>
                <input v-model="flightData.arrivalLocation" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">출발 일시</label>
                <input v-model="flightData.departureTime" type="datetime-local" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">도착 일시</label>
                <input v-model="flightData.arrivalTime" type="datetime-local" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">예약 번호</label>
                <input v-model="flightData.bookingReference" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">좌석 번호</label>
                <input v-model="flightData.seatNumber" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">메모</label>
              <textarea v-model="flightData.notes" rows="2" class="w-full px-3 py-2 border border-gray-300 rounded-lg"></textarea>
            </div>
          </div>
        </form>
      </template>
      <template #footer>
        <div class="flex justify-between">
          <button v-if="editingFlight?.id" @click="deleteFlight" type="button" class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700">삭제</button>
          <div class="flex gap-3 ml-auto">
            <button type="button" @click="closeFlightModal" class="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100">취소</button>
            <button type="submit" form="flight-form" class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">저장</button>
          </div>
        </div>
      </template>
    </SlideUpModal>

    <!-- 숙소 모달 -->
    <SlideUpModal :is-open="isAccommodationModalOpen" @close="closeAccommodationModal">
      <template #header-title>{{ editingAccommodation?.id ? '숙소 수정' : '숙소 추가' }}</template>
      <template #body>
        <form id="accommodation-form" @submit.prevent="saveAccommodation">
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">숙소명</label>
              <input v-model="accommodationData.name" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">숙소 타입</label>
              <input v-model="accommodationData.type" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" placeholder="예: 호텔, 에어비앤비" />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">주소</label>
              <input v-model="accommodationData.address" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">체크인</label>
                <input v-model="accommodationData.checkInTime" type="datetime-local" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">체크아웃</label>
                <input v-model="accommodationData.checkOutTime" type="datetime-local" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">예약 번호</label>
                <input v-model="accommodationData.bookingReference" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">연락처</label>
                <input v-model="accommodationData.contactNumber" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">메모</label>
              <textarea v-model="accommodationData.notes" rows="2" class="w-full px-3 py-2 border border-gray-300 rounded-lg"></textarea>
            </div>
          </div>
        </form>
      </template>
      <template #footer>
        <div class="flex justify-between">
          <button v-if="editingAccommodation?.id" @click="deleteAccommodation" type="button" class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700">삭제</button>
          <div class="flex gap-3 ml-auto">
            <button type="button" @click="closeAccommodationModal" class="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100">취소</button>
            <button type="submit" form="accommodation-form" class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">저장</button>
          </div>
        </div>
      </template>
    </SlideUpModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import MainHeader from '@/components/common/MainHeader.vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import apiClient from '@/services/api'

const route = useRoute()
const router = useRouter()

const tripId = computed(() => route.params.id)
const isEditMode = computed(() => !!tripId.value)

const loading = ref(true)
const trip = ref({
  flights: [],
  accommodations: []
})

const tripData = ref({
  title: '',
  description: '',
  startDate: '',
  endDate: '',
  destination: '',
  city: ''
})

// 항공권 모달
const isFlightModalOpen = ref(false)
const editingFlight = ref(null)
const flightData = ref({})

// 숙소 모달
const isAccommodationModalOpen = ref(false)
const editingAccommodation = ref(null)
const accommodationData = ref({})

// 여행 정보 로드
async function loadTrip() {
  if (!isEditMode.value) {
    loading.value = false
    return
  }

  loading.value = true
  try {
    const response = await apiClient.get(`/personal-trips/${tripId.value}`)
    trip.value = response.data
    tripData.value = {
      title: trip.value.title,
      description: trip.value.description,
      startDate: trip.value.startDate,
      endDate: trip.value.endDate,
      destination: trip.value.destination,
      city: trip.value.city
    }
  } catch (error) {
    console.error('여행 로드 실패:', error)
    alert('여행 정보를 불러올 수 없습니다.')
    router.push('/trips')
  } finally {
    loading.value = false
  }
}

// 여행 정보 저장
async function saveTripInfo() {
  if (!tripData.value.title || !tripData.value.startDate || !tripData.value.endDate) {
    alert('필수 항목을 입력해주세요.')
    return
  }

  try {
    if (isEditMode.value) {
      await apiClient.put(`/personal-trips/${tripId.value}`, tripData.value)
      alert('여행 정보가 저장되었습니다.')
      await loadTrip()
    } else {
      const response = await apiClient.post('/personal-trips', tripData.value)
      alert('여행이 생성되었습니다.')
      router.push(`/trips/${response.data.id}`)
    }
  } catch (error) {
    console.error('여행 저장 실패:', error)
    alert(error.response?.data?.message || '여행 저장에 실패했습니다.')
  }
}

// 여행 삭제
async function deleteTrip() {
  if (!confirm('정말로 이 여행을 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/personal-trips/${tripId.value}`)
    alert('여행이 삭제되었습니다.')
    router.push('/trips')
  } catch (error) {
    console.error('여행 삭제 실패:', error)
    alert('여행 삭제에 실패했습니다.')
  }
}

// 항공권 모달
function openFlightModal(flight = null) {
  editingFlight.value = flight
  if (flight) {
    flightData.value = {
      airline: flight.airline,
      flightNumber: flight.flightNumber,
      departureLocation: flight.departureLocation,
      arrivalLocation: flight.arrivalLocation,
      departureTime: flight.departureTime ? flight.departureTime.slice(0, 16) : '',
      arrivalTime: flight.arrivalTime ? flight.arrivalTime.slice(0, 16) : '',
      bookingReference: flight.bookingReference,
      seatNumber: flight.seatNumber,
      notes: flight.notes
    }
  } else {
    flightData.value = {}
  }
  isFlightModalOpen.value = true
}

function closeFlightModal() {
  isFlightModalOpen.value = false
  editingFlight.value = null
  flightData.value = {}
}

async function saveFlight() {
  try {
    if (editingFlight.value?.id) {
      await apiClient.put(`/personal-trips/flights/${editingFlight.value.id}`, flightData.value)
      alert('항공권이 수정되었습니다.')
    } else {
      await apiClient.post(`/personal-trips/${tripId.value}/flights`, flightData.value)
      alert('항공권이 추가되었습니다.')
    }
    closeFlightModal()
    await loadTrip()
  } catch (error) {
    console.error('항공권 저장 실패:', error)
    alert(error.response?.data?.message || '항공권 저장에 실패했습니다.')
  }
}

async function deleteFlight() {
  if (!confirm('이 항공권을 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/personal-trips/flights/${editingFlight.value.id}`)
    alert('항공권이 삭제되었습니다.')
    closeFlightModal()
    await loadTrip()
  } catch (error) {
    console.error('항공권 삭제 실패:', error)
    alert('항공권 삭제에 실패했습니다.')
  }
}

// 숙소 모달
function openAccommodationModal(accommodation = null) {
  editingAccommodation.value = accommodation
  if (accommodation) {
    accommodationData.value = {
      name: accommodation.name,
      type: accommodation.type,
      address: accommodation.address,
      checkInTime: accommodation.checkInTime ? accommodation.checkInTime.slice(0, 16) : '',
      checkOutTime: accommodation.checkOutTime ? accommodation.checkOutTime.slice(0, 16) : '',
      bookingReference: accommodation.bookingReference,
      contactNumber: accommodation.contactNumber,
      notes: accommodation.notes
    }
  } else {
    accommodationData.value = {}
  }
  isAccommodationModalOpen.value = true
}

function closeAccommodationModal() {
  isAccommodationModalOpen.value = false
  editingAccommodation.value = null
  accommodationData.value = {}
}

async function saveAccommodation() {
  try {
    if (editingAccommodation.value?.id) {
      await apiClient.put(`/personal-trips/accommodations/${editingAccommodation.value.id}`, accommodationData.value)
      alert('숙소가 수정되었습니다.')
    } else {
      await apiClient.post(`/personal-trips/${tripId.value}/accommodations`, accommodationData.value)
      alert('숙소가 추가되었습니다.')
    }
    closeAccommodationModal()
    await loadTrip()
  } catch (error) {
    console.error('숙소 저장 실패:', error)
    alert(error.response?.data?.message || '숙소 저장에 실패했습니다.')
  }
}

async function deleteAccommodation() {
  if (!confirm('이 숙소를 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/personal-trips/accommodations/${editingAccommodation.value.id}`)
    alert('숙소가 삭제되었습니다.')
    closeAccommodationModal()
    await loadTrip()
  } catch (error) {
    console.error('숙소 삭제 실패:', error)
    alert('숙소 삭제에 실패했습니다.')
  }
}

// 날짜/시간 포맷
function formatDateTime(dateTimeString) {
  if (!dateTimeString) return ''
  const date = new Date(dateTimeString)
  return date.toLocaleString('ko-KR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(loadTrip)
</script>
