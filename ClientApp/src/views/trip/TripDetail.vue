<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader :title="trip.title || '여행 상세'" :show-back="true" />

    <div v-if="loading" class="text-center py-20">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
      <p class="mt-4 text-gray-600">여행 정보를 불러오는 중...</p>
    </div>

    <div v-else class="max-w-2xl mx-auto px-4 py-6">
      <!-- 1. 조회 모드: 여행 정보 -->
      <section class="bg-white rounded-lg shadow p-4 mb-6">
        <div class="flex justify-between items-center mb-3">
          <h2 class="text-xl font-bold">여행 정보</h2>
          <button @click="openTripInfoModal" class="text-sm text-blue-600 hover:underline">수정</button>
        </div>
        <div class="space-y-3 text-gray-700">
          <p class="flex items-start"><strong class="w-20 font-semibold text-gray-500">기간</strong> <span>{{ trip.startDate }} ~ {{ trip.endDate }}</span></p>
          <p class="flex items-start"><strong class="w-20 font-semibold text-gray-500">목적지</strong> <span>{{ trip.destination }}</span></p>
          <p v-if="trip.description" class="flex items-start"><strong class="w-20 font-semibold text-gray-500">설명</strong> <span class="whitespace-pre-wrap">{{ trip.description }}</span></p>
        </div>
      </section>

      <!-- 2. 조회 모드: 항공권, 숙소, 일정 -->
      <!-- 항공권 -->
      <section class="bg-white rounded-lg shadow p-4 mb-6">
        <div class="flex justify-between items-center mb-4">
          <h2 class="text-lg font-bold">항공권</h2>
          <button @click="openFlightModal()" class="px-3 py-1 bg-blue-600 text-white rounded text-sm">+ 추가</button>
        </div>
        <div v-if="!trip.flights || trip.flights.length === 0" class="text-center py-8 text-gray-500">등록된 항공권이 없습니다.</div>
        <div v-else class="space-y-3">
          <div v-for="flight in trip.flights" :key="flight.id" class="border rounded-lg p-3 hover:bg-gray-50 cursor-pointer flex justify-between items-center">
            <div @click="openFlightModal(flight)" class="flex-grow">
              <p class="font-medium">{{ flight.airline }} {{ flight.flightNumber }}</p>
              <p class="text-sm text-gray-600 mt-1">{{ flight.departureLocation }} → {{ flight.arrivalLocation }}</p>
            </div>
            <button @click.stop="deleteFlightFromList(flight.id)" class="p-2 text-red-500 hover:text-red-700 rounded-full">
              <Trash2Icon class="w-5 h-5" />
            </button>
          </div>
        </div>
      </section>

      <!-- 숙소 -->
      <section class="bg-white rounded-lg shadow p-4 mb-6">
        <div class="flex justify-between items-center mb-4">
          <h2 class="text-lg font-bold">숙소</h2>
          <button @click="openAccommodationEditModal()" class="px-3 py-1 bg-blue-600 text-white rounded text-sm">+ 추가</button>
        </div>
        <div v-if="!trip.accommodations || trip.accommodations.length === 0" class="text-center py-8 text-gray-500">등록된 숙소가 없습니다.</div>
        <div v-else class="space-y-3">
          <div v-for="acc in trip.accommodations" :key="acc.id" class="border rounded-lg p-3 hover:bg-gray-50 cursor-pointer flex justify-between items-center">
            <div @click="openAccommodationDetailModal(acc)" class="flex-grow">
              <p class="font-medium">{{ acc.name }}</p>
              <p v-if="acc.address" class="text-sm text-gray-600 mt-1">{{ acc.address }}</p>
            </div>
            <button @click.stop="deleteAccommodationFromList(acc.id)" class="p-2 text-red-500 hover:text-red-700 rounded-full">
              <Trash2Icon class="w-5 h-5" />
            </button>
          </div>
        </div>
      </section>

      <!-- 일정 -->
      <section class="bg-white rounded-lg shadow p-4">
        <div class="flex justify-between items-center mb-4">
          <h2 class="text-lg font-bold">일정</h2>
          <button @click="openItineraryModal()" class="px-3 py-1 bg-blue-600 text-white rounded text-sm">+ 추가</button>
        </div>
        <div v-if="groupedItinerary.length === 0" class="text-center py-8 text-gray-500">등록된 일정이 없습니다.</div>
        <div v-else class="space-y-6">
          <div v-for="dayGroup in groupedItinerary" :key="dayGroup.dayNumber">
            <h3 class="text-md font-semibold mb-2 pb-1 border-b">{{ dayGroup.dayNumber }}일차</h3>
            <div class="space-y-3">
              <div 
                v-for="item in dayGroup.items" 
                :key="item.id" 
                class="border rounded-lg p-3 hover:bg-gray-50 cursor-pointer transition-all flex justify-between items-center"
                :class="{ 'border-blue-500 border-2 shadow-md': currentItineraryItemId === item.id }">
                <div @click="openItineraryDetailModal(item)" class="flex-grow">
                  <p class="font-medium">{{ item.locationName }}</p>
                  <p v-if="item.startTime" class="text-sm text-gray-500 mt-1">{{ item.startTime.substring(0, 5) }} - {{ item.endTime.substring(0, 5) }}</p>
                </div>
                <button @click.stop="deleteItineraryItemFromList(item.id)" class="p-2 text-red-500 hover:text-red-700 rounded-full">
                  <Trash2Icon class="w-5 h-5" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>

    <!-- Modals -->
    <SlideUpModal :is-open="isTripInfoModalOpen" @close="closeTripInfoModal">
      <template #header-title>여행 정보 수정</template>
      <template #body>
        <form id="trip-info-form" @submit.prevent="saveTripInfo" class="space-y-4">
          <div><label class="block text-sm font-medium text-gray-700 mb-1">여행 제목</label><input v-model="tripData.title" type="text" class="w-full input" /></div>
          <div><label class="block text-sm font-medium text-gray-700 mb-1">설명</label><textarea v-model="tripData.description" rows="3" class="w-full input"></textarea></div>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div><label class="block text-sm font-medium text-gray-700 mb-1">시작일</label><input v-model="tripData.startDate" type="date" class="w-full input" /></div>
            <div><label class="block text-sm font-medium text-gray-700 mb-1">종료일</label><input v-model="tripData.endDate" type="date" class="w-full input" /></div>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">도시/국가</label>
            <CountryCitySearch v-model="countryCity" />
          </div>
        </form>
      </template>
      <template #footer>
        <div class="flex justify-end gap-3"><button type="button" @click="closeTripInfoModal" class="btn-secondary">취소</button><button type="submit" form="trip-info-form" class="btn-primary">저장</button></div>
      </template>
    </SlideUpModal>

    <SlideUpModal :is-open="isFlightModalOpen" @close="closeFlightModal">
       <template #header-title>{{ editingFlight?.id ? '항공권 수정' : '항공권 추가' }}</template>
       <template #body>
        <form id="flight-form" @submit.prevent="saveFlight" class="space-y-4">
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div><label class="label">항공사</label><input v-model="flightData.airline" type="text" class="input" /></div>
            <div><label class="label">편명</label><input v-model="flightData.flightNumber" type="text" class="input" /></div>
            <div><label class="label">출발지</label><input v-model="flightData.departureLocation" type="text" class="input" /></div>
            <div><label class="label">도착지</label><input v-model="flightData.arrivalLocation" type="text" class="input" /></div>
          </div>
          <div><label class="label">출발 일시</label><DateTimePicker v-model="flightData.departureTime" /></div>
          <div><label class="label">도착 일시</label><DateTimePicker v-model="flightData.arrivalTime" /></div>
        </form>
       </template>
       <template #footer>
        <div class="flex justify-between">
          <button v-if="editingFlight?.id" @click="deleteFlight" type="button" class="btn-danger">삭제</button>
          <div class="flex gap-3 ml-auto"><button type="button" @click="closeFlightModal" class="btn-secondary">취소</button><button type="submit" form="flight-form" class="btn-primary">저장</button></div>
        </div>
       </template>
    </SlideUpModal>

    <SlideUpModal :is-open="isAccommodationEditModalOpen" @close="closeAccommodationEditModal">
      <template #header-title>{{ editingAccommodation?.id ? '숙소 수정' : '숙소 추가' }}</template>
      <template #body>
        <form id="acc-form" @submit.prevent="saveAccommodation" class="space-y-4">
          <div>
            <label class="label">숙소명</label>
            <template v-if="isDomestic">
              <input type="text" :value="accommodationData.name" @focus="openKakaoMapSearchModal('accommodation')" placeholder="숙소명 검색 (카카오맵)" class="input cursor-pointer" readonly />
            </template>
            <GooglePlacesAutocomplete v-else v-model="accommodationData" placeholder="숙소명 검색 (구글맵)" />
          </div>
          <div><label class="label">주소</label><input v-model="accommodationData.address" type="text" class="input" readonly /></div>
          <div><label class="label">체크인</label><DateTimePicker v-model="accommodationData.checkInTime" /></div>
          <div><label class="label">체크아웃</label><DateTimePicker v-model="accommodationData.checkOutTime" /></div>
        </form>
      </template>
      <template #footer>
        <div class="flex justify-between">
          <button v-if="editingAccommodation?.id" @click="deleteAccommodation" type="button" class="btn-danger">삭제</button>
          <div class="flex gap-3 ml-auto"><button type="button" @click="closeAccommodationEditModal" class="btn-secondary">취소</button><button type="submit" form="acc-form" class="btn-primary">저장</button></div>
        </div>
      </template>
    </SlideUpModal>

    <SlideUpModal :is-open="isItineraryModalOpen" @close="closeItineraryModal">
      <template #header-title>{{ editingItineraryItem?.id ? '일정 수정' : '일정 추가' }}</template>
      <template #body>
        <form id="itinerary-form" @submit.prevent="saveItineraryItem" class="space-y-4">
          <div>
            <label class="label">장소</label>
            <template v-if="isDomestic">
              <input type="text" :value="itineraryItemData.locationName" @focus="openKakaoMapSearchModal('itinerary')" placeholder="장소 검색 (카카오맵)" class="input cursor-pointer" readonly />
            </template>
            <GooglePlacesAutocomplete v-else v-model="itineraryItemData" placeholder="장소 검색 (구글맵)" />
          </div>
          <div><label class="label">시작 시간</label><DateTimePicker v-model="itineraryItemData.startTime" /></div>
          <div><label class="label">종료 시간</label><DateTimePicker v-model="itineraryItemData.endTime" /></div>
          <div><label class="label">메모</label><textarea v-model="itineraryItemData.notes" rows="3" class="input"></textarea></div>
        </form>
      </template>
      <template #footer>
        <div class="flex justify-between">
          <button v-if="editingItineraryItem?.id" @click="deleteItineraryItem" type="button" class="btn-danger">삭제</button>
          <div class="flex gap-3 ml-auto"><button type="button" @click="closeItineraryModal" class="btn-secondary">취소</button><button type="submit" form="itinerary-form" class="btn-primary">저장</button></div>
        </div>
      </template>
    </SlideUpModal>
    
    <SlideUpModal :is-open="isItineraryDetailModalOpen" @close="closeItineraryDetailModal">
      <template #header-title>일정 상세</template>
      <template #body>
        <div v-if="selectedItinerary" class="space-y-4">
          <h3 class="text-xl font-bold">{{ selectedItinerary.locationName }}</h3>
          <p class="text-gray-600">{{ selectedItinerary.address }}</p>
          <p class="text-gray-800 font-medium">{{ selectedItinerary.startTime.substring(0, 5) }} - {{ selectedItinerary.endTime.substring(0, 5) }}</p>
          <p v-if="selectedItinerary.notes" class="whitespace-pre-wrap">{{ selectedItinerary.notes }}</p>
          
          <div class="h-48 w-full rounded-lg mt-4">
            <KakaoMap
              v-if="isDomestic && selectedItinerary.latitude && selectedItinerary.longitude"
              :latitude="selectedItinerary.latitude"
              :longitude="selectedItinerary.longitude"
            />
            <GoogleMapPlaceholder v-else-if="!isDomestic && selectedItinerary.latitude" />
          </div>
        </div>
      </template>
      <template #footer>
        <div class="flex justify-end gap-3">
          <button type="button" @click="editSelectedItinerary" class="btn-secondary">수정</button>
          <button type="button" @click="closeItineraryDetailModal" class="btn-primary">닫기</button>
        </div>
      </template>
    </SlideUpModal>

    <AccommodationDetailModal
      :is-open="isAccommodationDetailModalOpen"
      :accommodation="selectedAccommodation"
      @close="closeAccommodationDetailModal"
      @edit="editSelectedAccommodation"
      :is-domestic="isDomestic"
    />

    <!-- Kakao Map Search Modal -->
    <KakaoMapSearchModal 
      :is-open="isKakaoMapSearchModalOpen" 
      @update:is-open="isKakaoMapSearchModalOpen = $event"
      @select-place="handleKakaoPlaceSelection"
      :initial-location="currentKakaoSearchInitialLocation"
    />

  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import MainHeader from '@/components/common/MainHeader.vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import DateTimePicker from '@/components/common/DateTimePicker.vue'
import CountryCitySearch from '@/components/common/CountryCitySearch.vue'
import KakaoMap from '@/components/common/KakaoMap.vue'
import GoogleMapPlaceholder from '@/components/common/GoogleMapPlaceholder.vue'
import GooglePlacesAutocomplete from '@/components/common/GooglePlacesAutocomplete.vue'
import KakaoPlacesAutocomplete from '@/components/common/KakaoPlacesAutocomplete.vue'
import KakaoMapSearchModal from '@/components/common/KakaoMapSearchModal.vue'
import AccommodationDetailModal from '@/components/personalTrip/AccommodationDetailModal.vue' // Import AccommodationDetailModal
import apiClient from '@/services/api'
import { useGoogleMaps } from '@/composables/useGoogleMaps'
import { Trash2 as Trash2Icon } from 'lucide-vue-next' // Import Trash2Icon

const route = useRoute()
const router = useRouter()
const tripId = computed(() => route.params.id)

const loading = ref(true)
const trip = ref({})
const isDomestic = computed(() => trip.value.countryCode === 'KR')
const now = ref(new Date())

// Modal states
const isTripInfoModalOpen = ref(false)
const isFlightModalOpen = ref(false)
const isAccommodationEditModalOpen = ref(false) // Renamed
const isAccommodationDetailModalOpen = ref(false) // New state
const isItineraryModalOpen = ref(false)
const isItineraryDetailModalOpen = ref(false)
const isKakaoMapSearchModalOpen = ref(false) // New state for Kakao Map Search Modal

// Data for modals
const tripData = ref({})
const countryCity = ref({ destination: '', countryCode: '' })
const flightData = ref({})
const accommodationData = ref({ name: '', address: '', postalCode: null, latitude: null, longitude: null, googlePlaceId: null, kakaoPlaceId: null })
const itineraryItemData = ref({ locationName: '', address: '', latitude: null, longitude: null, googlePlaceId: null, kakaoPlaceId: null })

// Editing items
const editingFlight = ref(null)
const editingAccommodation = ref(null)
const editingItineraryItem = ref(null)
const selectedItinerary = ref(null)
const selectedAccommodation = ref(null) // New ref for selected accommodation

// Google Maps and Places
const { isLoaded, loadScript } = useGoogleMaps()

// For Kakao Map Search Modal
const currentKakaoSearchTarget = ref(null) // 'accommodation' or 'itinerary'
const currentKakaoSearchInitialLocation = computed(() => {
  if (currentKakaoSearchTarget.value === 'accommodation') {
    return {
      name: accommodationData.value.name,
      address: accommodationData.value.address,
      postalCode: accommodationData.value.postalCode,
      latitude: accommodationData.value.latitude,
      longitude: accommodationData.value.longitude
    }
  } else if (currentKakaoSearchTarget.value === 'itinerary') {
    return {
      name: itineraryItemData.value.locationName,
      address: itineraryItemData.value.address,
      latitude: itineraryItemData.value.latitude,
      longitude: itineraryItemData.value.longitude
    }
  }
  return { latitude: null, longitude: null, name: '', address: '' }
})

// --- Lifecycle and Data Loading ---
onMounted(async () => {
  await loadTrip()
  loadScript()
  setInterval(() => { now.value = new Date() }, 60000) // Update time every minute for highlight
})

async function loadTrip() {
  loading.value = true
  try {
    const response = await apiClient.get(`/personal-trips/${tripId.value}`)
    trip.value = response.data
  } catch (error) {
    console.error('Failed to load trip:', error)
    alert('여행 정보를 불러오는 데 실패했습니다.')
    router.push('/trips')
  } finally {
    loading.value = false
  }
}

// --- Trip Info ---
function openTripInfoModal() {
  tripData.value = { ...trip.value }
  countryCity.value = { destination: trip.value.destination, countryCode: trip.value.countryCode }
  isTripInfoModalOpen.value = true
}
function closeTripInfoModal() { isTripInfoModalOpen.value = false }
async function saveTripInfo() {
  tripData.value.destination = countryCity.value.destination
  tripData.value.countryCode = countryCity.value.countryCode
  
  try {
    await apiClient.put(`/personal-trips/${tripId.value}`, tripData.value)
    await loadTrip()
    closeTripInfoModal()
  } catch (error) {
    console.error('Failed to save trip info:', error)
    alert('저장에 실패했습니다.')
  }
}

// --- Flights ---
function openFlightModal(flight = null) {
  editingFlight.value = flight
  flightData.value = flight ? { ...flight } : {}
  isFlightModalOpen.value = true
}
function closeFlightModal() { isFlightModalOpen.value = false }
async function saveFlight() {
  try {
    const payload = { ...flightData.value, personalTripId: tripId.value }
    if (editingFlight.value?.id) {
      await apiClient.put(`/personal-trips/flights/${editingFlight.value.id}`, payload)
    } else {
      await apiClient.post(`/personal-trips/${tripId.value}/flights`, payload)
    }
    await loadTrip()
    closeFlightModal()
  } catch (error) {
    console.error('Failed to save flight:', error)
    alert('저장에 실패했습니다.')
  }
}
async function deleteFlight(id) { // Modified to accept id
  if (!confirm('이 항공권을 삭제하시겠습니까?')) return
  try {
    await apiClient.delete(`/personal-trips/flights/${id}`)
    await loadTrip()
    closeFlightModal()
  } catch (error) {
    console.error('Failed to delete flight:', error)
    alert('삭제에 실패했습니다.')
  }
}
async function deleteFlightFromList(id) {
  deleteFlight(id);
}

// --- Accommodations ---
function openAccommodationEditModal(acc = null) { // Renamed
  editingAccommodation.value = acc
  accommodationData.value = acc ? { 
    name: acc.name, 
    address: acc.address, 
    postalCode: acc.postalCode,
    latitude: acc.latitude, 
    longitude: acc.longitude, 
    googlePlaceId: acc.googlePlaceId,
    kakaoPlaceId: acc.kakaoPlaceId,
    checkInTime: acc.checkInTime,
    checkOutTime: acc.checkOutTime
  } : { name: '', address: '', postalCode: null, latitude: null, longitude: null, googlePlaceId: null, kakaoPlaceId: null }
  isAccommodationEditModalOpen.value = true
}
function closeAccommodationEditModal() { isAccommodationEditModalOpen.value = false } // Renamed
function openAccommodationDetailModal(acc) { // New function
  selectedAccommodation.value = acc
  isAccommodationDetailModalOpen.value = true
}
function closeAccommodationDetailModal() { // New function
  isAccommodationDetailModalOpen.value = false
}
function editSelectedAccommodation() { // New function
  closeAccommodationDetailModal()
  openAccommodationEditModal(selectedAccommodation.value)
}
async function saveAccommodation() {
  try {
    const payload = { 
      ...accommodationData.value, 
      personalTripId: tripId.value,
      name: accommodationData.value.name,
      address: accommodationData.value.address,
      postalCode: accommodationData.value.postalCode,
      latitude: accommodationData.value.latitude,
      longitude: accommodationData.value.longitude,
      googlePlaceId: accommodationData.value.googlePlaceId,
      kakaoPlaceId: accommodationData.value.kakaoPlaceId
    }
    if (editingAccommodation.value?.id) {
      await apiClient.put(`/personal-trips/accommodations/${editingAccommodation.value.id}`, payload)
    } else {
      await apiClient.post(`/personal-trips/${tripId.value}/accommodations`, payload)
    }
    await loadTrip()
    closeAccommodationEditModal() // Renamed
  } catch (error) {
    console.error('Failed to save accommodation:', error)
    alert('저장에 실패했습니다.')
  }
}
async function deleteAccommodation(id) { // Modified to accept id
  if (!confirm('이 숙소를 삭제하시겠습니까?')) return
  try {
    await apiClient.delete(`/personal-trips/accommodations/${id}`)
    await loadTrip()
    closeAccommodationEditModal() // Renamed
  } catch (error) {
    console.error('Failed to delete accommodation:', error)
    alert('삭제에 실패했습니다.')
  }
}
function deleteAccommodationFromList(id) {
  deleteAccommodation(id);
}

// --- Itinerary ---
const groupedItinerary = computed(() => {
  if (!trip.value.itineraryItems) return []
  const groups = trip.value.itineraryItems.reduce((acc, item) => {
    const day = item.dayNumber || 1
    if (!acc[day]) acc[day] = { dayNumber: day, items: [] }
    acc[day].items.push(item)
    return acc
  }, {})
  return Object.values(groups).sort((a, b) => a.dayNumber - b.dayNumber)
})

const currentItineraryItemId = computed(() => {
  const currentTime = now.value
  for (const dayGroup of groupedItinerary.value) {
    const tripDate = new Date(trip.value.startDate)
    tripDate.setDate(tripDate.getDate() + dayGroup.dayNumber - 1)
    for (const item of dayGroup.items) {
      if (item.startTime && item.endTime) {
        const startDateTime = new Date(`${tripDate.toISOString().split('T')[0]}T${item.startTime}`)
        const endDateTime = new Date(`${tripDate.toISOString().split('T')[0]}T${item.endTime}`)
        if (currentTime >= startDateTime && currentTime <= endDateTime) return item.id
      }
    }
  }
  return null
})

function openItineraryModal(item = null) {
  editingItineraryItem.value = item
  itineraryItemData.value = item ? { 
    locationName: item.locationName, 
    address: item.address, 
    latitude: item.latitude, 
    longitude: item.longitude, 
    googlePlaceId: item.googlePlaceId,
    kakaoPlaceId: item.kakaoPlaceId,
    startTime: item.startTime,
    endTime: item.endTime,
    notes: item.notes,
    dayNumber: item.dayNumber
  } : { locationName: '', address: '', latitude: null, longitude: null, googlePlaceId: null, kakaoPlaceId: null, dayNumber: 1 }
  isItineraryModalOpen.value = true
}
function closeItineraryModal() { isItineraryModalOpen.value = false }
async function saveItineraryItem() {
  try {
    const payload = { 
      ...itineraryItemData.value, 
      personalTripId: tripId.value,
      locationName: itineraryItemData.value.locationName,
      address: itineraryItemData.value.address,
      latitude: itineraryItemData.value.latitude,
      longitude: itineraryItemData.value.longitude,
      googlePlaceId: itineraryItemData.value.googlePlaceId,
      kakaoPlaceId: itineraryItemData.value.kakaoPlaceId
    }
    if (editingItineraryItem.value?.id) {
      await apiClient.put(`/personal-trips/items/${editingItineraryItem.value.id}`, payload)
    } else {
      await apiClient.post(`/personal-trips/${tripId.value}/items`, payload)
    }
    await loadTrip()
    closeItineraryModal()
  } catch (error) {
    console.error('Failed to save itinerary item:', error)
    alert('저장에 실패했습니다.')
  }
}
async function deleteItineraryItem(id) { // Modified to accept id
  if (!confirm('이 일정을 삭제하시겠습니까?')) return
  try {
    await apiClient.delete(`/personal-trips/items/${id}`)
    await loadTrip()
    closeItineraryModal()
  }  catch (error) {
    console.error('Failed to delete itinerary item:', error)
    alert('삭제에 실패했습니다.')
  }
}
function deleteItineraryItemFromList(id) {
  deleteItineraryItem(id);
}

// --- Itinerary Detail Modal ---
function openItineraryDetailModal(item) {
  selectedItinerary.value = item
  isItineraryDetailModalOpen.value = true
}
function closeItineraryDetailModal() {
  isItineraryDetailModalOpen.value = false
}
function editSelectedItinerary() {
  closeItineraryDetailModal()
  openItineraryModal(selectedItinerary.value)
}

// --- Kakao Map Search Modal ---
function openKakaoMapSearchModal(target) {
  currentKakaoSearchTarget.value = target
  isKakaoMapSearchModalOpen.value = true
}

function handleKakaoPlaceSelection(place) {
  if (currentKakaoSearchTarget.value === 'accommodation') {
    accommodationData.value.name = place.name
    accommodationData.value.address = place.address
    accommodationData.value.postalCode = place.postalCode // Add postalCode
    accommodationData.value.latitude = place.latitude
    accommodationData.value.longitude = place.longitude
    accommodationData.value.kakaoPlaceId = place.kakaoPlaceId
  } else if (currentKakaoSearchTarget.value === 'itinerary') {
    itineraryItemData.value.locationName = place.name
    itineraryItemData.value.address = place.address
    itineraryItemData.value.latitude = place.latitude
    itineraryItemData.value.longitude = place.longitude
    itineraryItemData.value.kakaoPlaceId = place.kakaoPlaceId
  }
  isKakaoMapSearchModalOpen.value = false
}
</script>