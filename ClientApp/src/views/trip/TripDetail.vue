<template>
  <div class="min-h-screen relative bg-gray-50">
    <!-- Decorative Background Elements -->
    <div class="fixed inset-0 z-0 overflow-hidden pointer-events-none">
      <!-- Large gradient blobs -->
      <div
        class="absolute top-20 -right-32 w-96 h-96 bg-gradient-to-br from-sky-200/15 to-blue-200/15 rounded-full blur-3xl"
      ></div>
      <div
        class="absolute bottom-40 -left-40 w-80 h-80 bg-gradient-to-br from-blue-200/12 to-cyan-200/12 rounded-full blur-3xl"
      ></div>
      <div
        class="absolute top-1/3 left-1/3 w-72 h-72 bg-gradient-to-br from-cyan-200/12 to-sky-200/12 rounded-full blur-3xl"
      ></div>

      <!-- Subtle dot pattern -->
      <div
        class="absolute inset-0 opacity-[0.02]"
        style="
          background-image: url('data:image/svg+xml,%3Csvg width=&quot;20&quot; height=&quot;20&quot; xmlns=&quot;http://www.w3.org/2000/svg&quot;%3E%3Cg fill=&quot;%239C92AC&quot; fill-opacity=&quot;1&quot;%3E%3Ccircle cx=&quot;2&quot; cy=&quot;2&quot; r=&quot;1&quot;/%3E%3C/g%3E%3C/svg%3E');
        "
      ></div>
    </div>

    <div class="relative z-10">
      <MainHeader
        :title="trip.title || '여행 상세'"
        :show-back="true"
        :show-menu="!effectiveReadonly"
      >
        <template #actions>
          <div class="relative">
            <button
              class="p-2 text-gray-500 hover:bg-gray-100 rounded-lg"
              @click="openReminderModal"
            >
              <BellIcon class="w-6 h-6" />
              <span
                v-if="hasNewReminders"
                class="absolute top-1 right-1 block h-2 w-2 rounded-full ring-2 ring-white bg-red-500"
              ></span>
            </button>
          </div>
        </template>
      </MainHeader>

      <div v-if="loading" class="text-center py-20">
        <div
          class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"
        ></div>
        <p class="mt-4 text-gray-600 font-medium">여행 정보를 불러오는 중...</p>
      </div>

      <div v-else class="max-w-2xl mx-auto px-4 py-4 pb-24 space-y-6">
        <TripHeroCard
          :trip="trip"
          :effective-readonly="effectiveReadonly"
          :trip-id="tripId"
          @open-share="openShareModal"
          @open-edit="openTripInfoModal"
        />

        <TripDashboardComponent
          :trip="trip"
          :readonly="effectiveReadonly"
          @open-accommodation-modal="openAccommodationManagementModal"
          @open-flight-modal="openFlightManagementModal"
          @go-to-itinerary="handleGoToItinerary"
          @go-to-expenses="handleGoToExpenses"
          @go-to-notes="handleGoToNotes"
        />
      </div>

      <!-- Modals -->
      <ShareTripModal
        :is-open="isShareModalOpen"
        :is-shared="trip.isShared"
        :share-url="shareableUrl"
        @close="closeShareModal"
        @update:is-shared="handleSharingToggle"
      />

      <TripInfoModal
        :is-open="isTripInfoModalOpen"
        :trip="trip"
        :trip-id="tripId"
        :effective-readonly="effectiveReadonly"
        @close="handleTripInfoClose"
        @saved="handleTripInfoSaved"
        @deleted="handleTripDeleted"
      />

      <AccommodationManagementModal
        :is-open="isAccommodationManagementModalOpen"
        :accommodations="trip.accommodations"
        @close="closeAccommodationManagementModal"
        @add="handleAddAccommodation"
        @edit="handleEditAccommodation"
        @delete="handleDeleteAccommodation"
        @select="handleSelectAccommodation"
      />

      <SlideUpModal
        :is-open="isAccommodationEditModalOpen"
        z-index-class="z-[60]"
        @close="closeAccommodationEditModal"
      >
        <template #header-title>{{
          editingAccommodation?.id ? '숙소 수정' : '숙소 추가'
        }}</template>
        <template #body>
          <form
            id="acc-form"
            class="space-y-4"
            @submit.prevent="saveAccommodation"
          >
            <div>
              <label class="label">숙소명</label>
              <template v-if="isDomestic">
                <input
                  type="text"
                  :value="accommodationData.name"
                  placeholder="숙소명 검색 (카카오맵)"
                  class="input cursor-pointer"
                  readonly
                  @focus="openKakaoMapSearchModal('accommodation')"
                />
              </template>
              <GooglePlacesAutocomplete
                v-else
                v-model="accommodationData"
                placeholder="숙소명 검색 (구글맵)"
              />
            </div>
            <div>
              <label class="label">주소</label
              ><input
                v-model="accommodationData.address"
                type="text"
                class="input"
                readonly
              />
            </div>
            <div>
              <label class="label">체크인</label>
              <CommonDatePicker
                v-model:value="accommodationData.checkInTime"
                type="datetime"
                format="YYYY-MM-DD HH:mm"
                value-type="YYYY-MM-DDTHH:mm:ss"
              />
            </div>
            <div>
              <label class="label">체크아웃</label>
              <CommonDatePicker
                v-model:value="accommodationData.checkOutTime"
                type="datetime"
                format="YYYY-MM-DD HH:mm"
                value-type="YYYY-MM-DDTHH:mm:ss"
              />
            </div>
            <div>
              <label class="label">비용 (원)</label>
              <input
                v-model="accommodationData.expenseAmount"
                v-number-format
                type="text"
                class="input"
                placeholder="예: 100000"
                min="0"
                step="100"
              />
            </div>
          </form>
        </template>
        <template #footer>
          <div class="flex gap-3 w-full">
            <button
              type="button"
              class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors"
              @click="closeAccommodationEditModal"
            >
              취소
            </button>
            <button
              type="submit"
              form="acc-form"
              class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all"
            >
              저장
            </button>
          </div>
        </template>
      </SlideUpModal>

      <TripItinerarySection
        ref="itinerarySectionRef"
        :trip="trip"
        :trip-id="tripId"
        :effective-readonly="effectiveReadonly"
        :is-domestic="isDomestic"
        @reload="loadTrip"
      />

      <AccommodationDetailModal
        :is-open="isAccommodationDetailModalOpen"
        :accommodation="selectedAccommodation"
        :is-domestic="isDomestic"
        :show-edit="!effectiveReadonly"
        @close="closeAccommodationDetailModal"
        @edit="editSelectedAccommodation"
      />

      <TripReminderModal
        :is-open="isReminderModalOpen"
        :upcoming-reminders="upcomingReminders"
        @close="closeReminderModal"
      />

      <!-- Kakao Map Search Modal (for accommodation) -->
      <KakaoMapSearchModal
        :is-open="isKakaoMapSearchModalOpen"
        :initial-location="currentKakaoSearchInitialLocation"
        z-index-class="z-[70]"
        @update:is-open="isKakaoMapSearchModalOpen = $event"
        @select-place="handleKakaoPlaceSelection"
      />
    </div>
  </div>

  <!-- Bottom Navigation Bar -->
  <BottomNavigationBar
    v-if="
      (tripId && tripId !== 'undefined') ||
      (shareToken && shareToken !== 'undefined')
    "
    :trip-id="tripId || trip.id"
    :share-token="shareToken"
  />
</template>
<script setup>
import {
  ref,
  computed,
  onMounted,
  watch,
  nextTick,
  defineAsyncComponent,
} from 'vue'
import { useRoute, useRouter } from 'vue-router'
import MainHeader from '@/components/common/MainHeader.vue'
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import TripDashboardComponent from '@/components/personalTrip/TripDashboardComponent.vue'
import TripHeroCard from '@/components/personalTrip/TripHeroCard.vue'
import apiClient from '@/services/api'
import { useGoogleMaps } from '@/composables/useGoogleMaps'
import { BellIcon } from 'lucide-vue-next'
import dayjs from 'dayjs'

// 모달/대형 컴포넌트는 lazy loading
const CommonDatePicker = defineAsyncComponent(
  () => import('@/components/common/CommonDatePicker.vue'),
)
const GooglePlacesAutocomplete = defineAsyncComponent(
  () => import('@/components/common/GooglePlacesAutocomplete.vue'),
)
const KakaoMapSearchModal = defineAsyncComponent(
  () => import('@/components/common/KakaoMapSearchModal.vue'),
)
const AccommodationDetailModal = defineAsyncComponent(
  () => import('@/components/personalTrip/AccommodationDetailModal.vue'),
)
const AccommodationManagementModal = defineAsyncComponent(
  () => import('@/components/personalTrip/AccommodationManagementModal.vue'),
)
const ShareTripModal = defineAsyncComponent(
  () => import('@/components/personalTrip/ShareTripModal.vue'),
)
const TripInfoModal = defineAsyncComponent(
  () => import('@/components/personalTrip/TripInfoModal.vue'),
)
const TripItinerarySection = defineAsyncComponent(
  () => import('@/components/personalTrip/TripItinerarySection.vue'),
)
const TripReminderModal = defineAsyncComponent(
  () => import('@/components/personalTrip/TripReminderModal.vue'),
)

// Props for readonly mode and shared access
const props = defineProps({
  id: String,
  shareToken: String,
  readonly: {
    type: Boolean,
    default: false,
  },
})

const router = useRouter()

// Determine tripId and readonly mode
const tripId = computed(() => props.id || null)
const shareToken = computed(() => props.shareToken || null)
const isSharedView = computed(() => !!shareToken.value)
const effectiveReadonly = computed(() => props.readonly || isSharedView.value)

const loading = ref(true)
const trip = ref({})

// Child component refs
const itinerarySectionRef = ref(null)

// Modal states
const isReminderModalOpen = ref(false)
const isTripInfoModalOpen = ref(false)
const isShareModalOpen = ref(false)
const isAccommodationManagementModalOpen = ref(false)
const isAccommodationEditModalOpen = ref(false)
const isAccommodationDetailModalOpen = ref(false)
const isKakaoMapSearchModalOpen = ref(false)
const lastCheckedReminders = ref(null)

const isDomestic = computed(() => trip.value.countryCode === 'KR')

const today = dayjs()

// --- Reminders ---
const upcomingReminders = computed(() => {
  const reminders = []
  if (!trip.value.startDate) return reminders
  const start = dayjs(trip.value.startDate)
  const daysUntilTrip = start.diff(today, 'day')

  if (daysUntilTrip > 0 && daysUntilTrip <= 7) {
    reminders.push({
      id: 'trip-start',
      title: '여행 시작 임박',
      description: '여행 준비를 확인하세요!',
      dateText: trip.value.startDate,
      daysUntil: daysUntilTrip,
    })
  }

  if (trip.value.accommodations) {
    trip.value.accommodations.forEach((acc) => {
      if (acc.checkInTime) {
        const checkIn = dayjs(acc.checkInTime)
        const diff = checkIn.diff(today, 'day')
        if (diff >= 0 && diff <= 1) {
          reminders.push({
            id: `checkin-${acc.id}`,
            title: diff === 0 ? '오늘 체크인' : '내일 체크인',
            description: acc.name,
            dateText: checkIn.format('YYYY-MM-DD HH:mm'),
            daysUntil: diff,
          })
        }
      }
    })
  }

  if (trip.value.flights) {
    trip.value.flights.forEach((flight) => {
      if (flight.departureTime) {
        const departure = dayjs(flight.departureTime)
        const diff = departure.diff(today, 'day')
        if (diff >= 0 && diff <= 1) {
          reminders.push({
            id: `flight-${flight.id}`,
            title: diff === 0 ? '오늘 출발' : '내일 출발',
            description: `${flight.airline || ''} ${flight.flightNumber || ''} - ${flight.departureLocation} → ${flight.arrivalLocation}`,
            dateText: departure.format('YYYY-MM-DD HH:mm'),
            daysUntil: diff,
          })
        }
      }
    })
  }

  return reminders.sort((a, b) => a.daysUntil - b.daysUntil)
})

const hasNewReminders = computed(() => {
  if (upcomingReminders.value.length === 0) return false
  if (!lastCheckedReminders.value) return true

  const currentIds = upcomingReminders.value
    .map((r) => r.id)
    .sort()
    .join(',')
  const checkedIds = lastCheckedReminders.value.reminderIds || ''

  return currentIds !== checkedIds
})

function openReminderModal() {
  isReminderModalOpen.value = true

  const tripIdValue = tripId.value || shareToken.value
  if (tripIdValue) {
    const reminderData = {
      reminderIds: upcomingReminders.value
        .map((r) => r.id)
        .sort()
        .join(','),
      checkedAt: new Date().toISOString(),
    }
    localStorage.setItem(
      `reminders_checked_${tripIdValue}`,
      JSON.stringify(reminderData),
    )
    lastCheckedReminders.value = reminderData
  }
}

function closeReminderModal() {
  isReminderModalOpen.value = false
}

// --- Shareable URL ---
const shareableUrl = computed(() => {
  if (trip.value.isShared && trip.value.shareToken) {
    return `${window.location.origin}/trips/share/${trip.value.shareToken}`
  }
  return ''
})

// Google Maps
const { loadScript } = useGoogleMaps()

// --- Lifecycle ---
onMounted(async () => {
  try {
    await loadTrip()
    loadScript()
    setInterval(() => {}, 60000)

    const tripIdValue = tripId.value || shareToken.value
    if (tripIdValue) {
      const stored = localStorage.getItem(`reminders_checked_${tripIdValue}`)
      if (stored) {
        lastCheckedReminders.value = JSON.parse(stored)
      }
    }
  } catch (error) {
    console.error('Failed to initialize TripDetail:', error)
  }
})

watch(
  () => props.id,
  async (newId, oldId) => {
    if (newId && newId !== oldId) {
      await loadTrip()
    }
  },
)

async function loadTrip() {
  loading.value = true
  try {
    if (shareToken.value) {
      const response = await apiClient.get(
        `/personal-trips/public/${shareToken.value}`,
      )
      trip.value = response.data
    } else if (!tripId.value) {
      trip.value = {
        title: '새 여행',
        description: '',
        startDate: '',
        endDate: '',
        destination: '',
        countryCode: '',
        flights: [],
        accommodations: [],
        itineraryItems: [],
      }
      await nextTick()
      openTripInfoModal()
    } else {
      const response = await apiClient.get(
        `/personal-trips/${tripId.value}?_=${new Date().getTime()}`,
      )
      trip.value = response.data
    }
  } catch (error) {
    console.error('Failed to load trip:', error)
    if (shareToken.value) {
      alert(
        '여행 정보를 불러오는 데 실패했습니다. 링크가 유효하지 않을 수 있습니다.',
      )
      router.push('/')
    } else {
      alert('여행 정보를 불러오는 데 실패했습니다.')
      router.push('/trips')
    }
  } finally {
    loading.value = false
  }
}

// --- Sharing ---
function openShareModal() {
  isShareModalOpen.value = true
}
function closeShareModal() {
  isShareModalOpen.value = false
}
async function handleSharingToggle(isShared) {
  if (effectiveReadonly.value) return
  try {
    if (isShared) {
      const response = await apiClient.post(
        `/personal-trips/${tripId.value}/share`,
      )
      trip.value.shareToken = response.data.shareToken
      trip.value.isShared = true
    } else {
      await apiClient.delete(`/personal-trips/${tripId.value}/share`)
      trip.value.isShared = false
    }
  } catch (error) {
    console.error('Failed to update sharing status:', error)
    alert('공유 상태 변경에 실패했습니다.')
    trip.value.isShared = !isShared
  }
}

// --- Trip Info Modal ---
function openTripInfoModal() {
  isTripInfoModalOpen.value = true
}

function handleTripInfoClose() {
  isTripInfoModalOpen.value = false
  // 새 여행 생성 모드에서 모달을 닫으면 목록으로 돌아가기
  if (!tripId.value && !trip.value.id) {
    router.push('/trips')
  }
}

function handleTripInfoSaved({ isNew, tripId: newTripId, data }) {
  isTripInfoModalOpen.value = false
  if (isNew) {
    router.push(`/trips/${newTripId}`)
  } else {
    trip.value = data
  }
}

function handleTripDeleted() {
  isTripInfoModalOpen.value = false
  router.push('/trips')
}

// --- Accommodations ---
const accommodationData = ref({
  name: '',
  address: '',
  postalCode: null,
  latitude: null,
  longitude: null,
  googlePlaceId: null,
  kakaoPlaceId: null,
  expenseAmount: null,
  type: null,
  bookingReference: null,
  contactNumber: null,
  notes: null,
})
const editingAccommodation = ref(null)
const selectedAccommodation = ref(null)

function openAccommodationManagementModal() {
  isAccommodationManagementModalOpen.value = true
}
function closeAccommodationManagementModal() {
  isAccommodationManagementModalOpen.value = false
}

function handleAddAccommodation() {
  openAccommodationEditModal()
}

function handleEditAccommodation(acc) {
  openAccommodationEditModal(acc)
}

function handleDeleteAccommodation(accId) {
  deleteAccommodationFromList(accId)
}

function handleSelectAccommodation(acc) {
  openAccommodationDetailModal(acc)
}

function openAccommodationEditModal(acc = null) {
  editingAccommodation.value = acc
  accommodationData.value = acc
    ? {
        name: acc.name,
        type: acc.type,
        address: acc.address,
        postalCode: acc.postalCode,
        latitude: acc.latitude,
        longitude: acc.longitude,
        googlePlaceId: acc.googlePlaceId,
        kakaoPlaceId: acc.kakaoPlaceId,
        checkInTime: acc.checkInTime,
        checkOutTime: acc.checkOutTime,
        bookingReference: acc.bookingReference,
        contactNumber: acc.contactNumber,
        notes: acc.notes,
        expenseAmount: acc.expenseAmount,
      }
    : {
        name: '',
        type: null,
        address: '',
        postalCode: null,
        latitude: null,
        longitude: null,
        googlePlaceId: null,
        kakaoPlaceId: null,
        checkInTime: null,
        checkOutTime: null,
        bookingReference: null,
        contactNumber: null,
        notes: null,
        expenseAmount: null,
      }
  isAccommodationEditModalOpen.value = true
}
function closeAccommodationEditModal() {
  isAccommodationEditModalOpen.value = false
}
function openAccommodationDetailModal(acc) {
  selectedAccommodation.value = acc
  isAccommodationDetailModalOpen.value = true
}
function closeAccommodationDetailModal() {
  isAccommodationDetailModalOpen.value = false
}
function editSelectedAccommodation() {
  closeAccommodationDetailModal()
  openAccommodationEditModal(selectedAccommodation.value)
}
async function saveAccommodation() {
  if (effectiveReadonly.value) return
  try {
    const cleanNumber = (value) => {
      if (!value) return null
      const cleaned = String(value).replace(/,/g, '')
      return cleaned ? Number(cleaned) : null
    }

    const payload = {
      personalTripId: tripId.value,
      name: accommodationData.value.name,
      type: accommodationData.value.type || null,
      address: accommodationData.value.address,
      postalCode: accommodationData.value.postalCode,
      latitude: accommodationData.value.latitude,
      longitude: accommodationData.value.longitude,
      checkInTime: accommodationData.value.checkInTime,
      checkOutTime: accommodationData.value.checkOutTime,
      bookingReference: accommodationData.value.bookingReference || null,
      contactNumber: accommodationData.value.contactNumber || null,
      notes: accommodationData.value.notes || null,
      googlePlaceId: accommodationData.value.googlePlaceId,
      kakaoPlaceId: accommodationData.value.kakaoPlaceId,
      expenseAmount: cleanNumber(accommodationData.value.expenseAmount),
    }

    if (editingAccommodation.value?.id) {
      await apiClient.put(
        `/personal-trips/accommodations/${editingAccommodation.value.id}`,
        payload,
      )
    } else {
      await apiClient.post(
        `/personal-trips/${tripId.value}/accommodations`,
        payload,
      )
    }
    await loadTrip()
    closeAccommodationEditModal()
  } catch (error) {
    console.error('Failed to save accommodation:', error)
    alert('저장에 실패했습니다.')
  }
}
async function deleteAccommodation(id) {
  if (effectiveReadonly.value) return
  if (!confirm('이 숙소를 삭제하시겠습니까?')) return
  try {
    await apiClient.delete(`/personal-trips/accommodations/${id}`)
    await loadTrip()
    closeAccommodationEditModal()
  } catch (error) {
    console.error('Failed to delete accommodation:', error)
    alert('삭제에 실패했습니다.')
  }
}
function deleteAccommodationFromList(id) {
  deleteAccommodation(id)
}

// --- Transportation (redirect to separate page) ---
function openFlightManagementModal() {
  if (shareToken.value) {
    router.push(`/trips/share/${shareToken.value}/transportation`)
  } else if (tripId.value) {
    router.push(`/trips/${tripId.value}/transportation`)
  } else {
    alert('여행을 먼저 저장해주세요.')
  }
}

// --- Kakao Map Search Modal (for accommodation) ---
const currentKakaoSearchTarget = ref(null)
const currentKakaoSearchInitialLocation = computed(() => {
  if (currentKakaoSearchTarget.value === 'accommodation') {
    return {
      name: accommodationData.value.name,
      address: accommodationData.value.address,
      postalCode: accommodationData.value.postalCode,
      latitude: accommodationData.value.latitude,
      longitude: accommodationData.value.longitude,
    }
  }
  return { latitude: null, longitude: null, name: '', address: '' }
})

function openKakaoMapSearchModal(target) {
  currentKakaoSearchTarget.value = target
  isKakaoMapSearchModalOpen.value = true
}

function handleKakaoPlaceSelection(place) {
  if (currentKakaoSearchTarget.value === 'accommodation') {
    accommodationData.value.name = place.name
    accommodationData.value.address = place.address
    accommodationData.value.postalCode = place.postalCode
    accommodationData.value.latitude = place.latitude
    accommodationData.value.longitude = place.longitude
    accommodationData.value.kakaoPlaceId = place.kakaoPlaceId
  }
  isKakaoMapSearchModalOpen.value = false
}

// --- Navigation handlers ---
function handleGoToItinerary() {
  if (shareToken.value) {
    router.push(`/trips/share/${shareToken.value}/itinerary`)
  } else if (tripId.value) {
    router.push(`/trips/${tripId.value}/itinerary`)
  } else {
    alert('여행을 먼저 저장해주세요.')
  }
}

function handleGoToExpenses() {
  if (shareToken.value) {
    router.push(`/trips/share/${shareToken.value}/expenses`)
  } else if (tripId.value) {
    router.push(`/trips/${tripId.value}/expenses`)
  } else {
    alert('여행을 먼저 저장해주세요.')
  }
}

function handleGoToNotes() {
  if (shareToken.value) {
    router.push(`/trips/share/${shareToken.value}/notes`)
  } else if (tripId.value) {
    router.push(`/trips/${tripId.value}/notes`)
  } else {
    alert('여행을 먼저 저장해주세요.')
  }
}
</script>
