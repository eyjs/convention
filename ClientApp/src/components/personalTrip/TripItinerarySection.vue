<template>
  <!-- Itinerary Edit Modal -->
  <SlideUpModal
    :is-open="isItineraryModalOpen"
    z-index-class="z-[60]"
    @close="closeItineraryModal"
  >
    <template #header-title>{{
      editingItineraryItem?.id ? '일정 수정' : '일정 추가'
    }}</template>
    <template #body>
      <form
        id="itinerary-form"
        class="space-y-4"
        @submit.prevent="saveItineraryItem"
      >
        <div>
          <label class="label">장소</label>
          <template v-if="isDomestic">
            <input
              type="text"
              :value="itineraryItemData.locationName"
              placeholder="장소 검색 (카카오맵)"
              class="input cursor-pointer"
              readonly
              @focus="openKakaoMapSearchModal('itinerary')"
            />
          </template>
          <GooglePlacesAutocomplete
            v-else
            v-model="itineraryItemData"
            placeholder="장소 검색 (구글맵)"
          />
        </div>
        <div>
          <label class="label">시작 시간</label>
          <input
            v-model="itineraryItemData.startTime"
            type="time"
            class="input"
          />
        </div>
        <div>
          <label class="label">종료 시간</label>
          <input
            v-model="itineraryItemData.endTime"
            type="time"
            class="input"
          />
        </div>
        <div>
          <label class="label">카테고리</label>
          <input
            v-model="itineraryItemData.category"
            type="text"
            placeholder="카카오맵에서 자동 설정됨"
            class="input bg-gray-50 cursor-not-allowed"
            readonly
          />
          <div class="flex flex-wrap gap-2 mt-2">
            <button
              type="button"
              class="px-3 py-1 text-sm rounded-full transition-colors"
              :class="
                itineraryItemData.category === '기타'
                  ? 'bg-primary-500 text-white'
                  : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
              "
              @click="itineraryItemData.category = '기타'"
            >
              기타
            </button>
          </div>
        </div>
        <div>
          <label class="label">전화번호</label>
          <input
            v-model="itineraryItemData.phoneNumber"
            type="tel"
            placeholder="카카오맵에서 자동 설정됨"
            class="input bg-gray-50 cursor-not-allowed"
            readonly
          />
        </div>
        <div>
          <label class="label">금액 (원)</label>
          <input
            v-model="itineraryItemData.expenseAmount"
            v-number-format
            type="text"
            placeholder="예: 50000"
            class="input"
            min="0"
            step="100"
          />
        </div>
        <div>
          <label class="label">메모</label
          ><textarea
            v-model="itineraryItemData.notes"
            rows="3"
            class="input"
          ></textarea>
        </div>
      </form>
    </template>
    <template #footer>
      <div class="flex gap-3 w-full">
        <button
          type="button"
          class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors"
          @click="closeItineraryModal"
        >
          취소
        </button>
        <button
          type="submit"
          form="itinerary-form"
          class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all"
        >
          저장
        </button>
      </div>
    </template>
  </SlideUpModal>

  <!-- Itinerary Detail Modal -->
  <SlideUpModal
    :is-open="isItineraryDetailModalOpen"
    z-index-class="z-[60]"
    @close="closeItineraryDetailModal"
  >
    <template #header-title>일정 상세</template>
    <template #body>
      <div v-if="selectedItinerary" class="space-y-4">
        <!-- Category Badge & Phone Link -->
        <div class="flex items-center justify-between">
          <div v-if="selectedItinerary.category">
            <span
              class="inline-flex items-center gap-1.5 px-3 py-1.5 bg-primary-50 text-primary-700 text-sm font-medium rounded-full"
            >
              <component
                :is="getCategoryIcon(selectedItinerary.category)"
                class="w-4 h-4"
              />
              {{ selectedItinerary.category }}
            </span>
          </div>
          <div v-else class="flex-1"></div>

          <!-- Phone Call Link -->
          <a
            v-if="selectedItinerary.phoneNumber"
            :href="`tel:${selectedItinerary.phoneNumber}`"
            class="text-primary-600 hover:text-primary-700 text-sm font-medium"
          >
            전화걸기
          </a>
        </div>

        <h3 class="text-xl font-bold">{{ selectedItinerary.locationName }}</h3>

        <!-- 주소 (길찾기 버튼) -->
        <div v-if="selectedItinerary.address" class="flex items-start gap-2">
          <svg
            class="w-5 h-5 text-gray-400 flex-shrink-0 mt-0.5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
            />
          </svg>
          <p class="text-gray-600 flex-1 text-sm leading-5">
            {{ selectedItinerary.address }}
          </p>
          <a
            v-if="selectedItinerary.latitude && selectedItinerary.longitude"
            :href="`https://map.kakao.com/link/to/${selectedItinerary.locationName},${selectedItinerary.latitude},${selectedItinerary.longitude}`"
            target="_blank"
            class="text-primary-600 hover:text-primary-700 text-sm font-medium whitespace-nowrap leading-5"
            title="카카오맵에서 길찾기"
          >
            길찾기
          </a>
        </div>

        <!-- 시간 -->
        <div
          v-if="selectedItinerary.startTime && selectedItinerary.endTime"
          class="text-gray-800 font-medium flex items-start gap-2"
        >
          <svg
            class="w-5 h-5 text-gray-500 flex-shrink-0 mt-0.5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <span
            >{{ selectedItinerary.startTime.substring(0, 5) }} -
            {{ selectedItinerary.endTime.substring(0, 5) }}</span
          >
        </div>

        <!-- 금액 -->
        <div
          v-if="selectedItinerary.expenseAmount"
          class="text-gray-800 font-medium flex items-start gap-2"
        >
          <svg
            class="w-5 h-5 text-gray-500 flex-shrink-0 mt-0.5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <span>{{ selectedItinerary.expenseAmount.toLocaleString() }}원</span>
        </div>

        <!-- 메모 -->
        <p
          v-if="selectedItinerary.notes"
          class="whitespace-pre-wrap bg-gray-50 p-3 rounded-lg"
        >
          {{ selectedItinerary.notes }}
        </p>

        <button
          v-if="selectedItinerary.latitude && !showItineraryMap"
          class="w-full py-2.5 bg-gray-100 text-gray-700 rounded-lg font-semibold hover:bg-gray-200 transition-colors"
          @click="showItineraryMap = true"
        >
          지도 보기
        </button>

        <div v-if="showItineraryMap" class="space-y-2">
          <div class="h-48 w-full rounded-lg">
            <KakaoMap
              v-if="
                isDomestic &&
                selectedItinerary.latitude &&
                selectedItinerary.longitude
              "
              :latitude="selectedItinerary.latitude"
              :longitude="selectedItinerary.longitude"
            />
            <GoogleMapPlaceholder
              v-else-if="!isDomestic && selectedItinerary.latitude"
            />
          </div>
          <button
            class="w-full py-2.5 bg-gray-100 text-gray-700 rounded-lg font-semibold hover:bg-gray-200 transition-colors"
            @click="showItineraryMap = false"
          >
            지도 접기
          </button>
        </div>
      </div>
    </template>
    <template #footer>
      <div class="flex gap-3 w-full">
        <button
          type="button"
          class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors"
          @click="closeItineraryDetailModal"
        >
          닫기
        </button>
        <button
          v-if="!effectiveReadonly"
          type="button"
          class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all"
          :disabled="selectedItinerary?.isAutoGenerated"
          @click="editSelectedItinerary"
        >
          수정
        </button>
      </div>
    </template>
  </SlideUpModal>

  <!-- Kakao Map Search Modal -->
  <KakaoMapSearchModal
    :is-open="isKakaoMapSearchModalOpen"
    :initial-location="currentKakaoSearchInitialLocation"
    z-index-class="z-[70]"
    @update:is-open="isKakaoMapSearchModalOpen = $event"
    @select-place="handleKakaoPlaceSelection"
  />
</template>

<script setup>
import { ref, computed, watch, nextTick } from 'vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import KakaoMap from '@/components/common/KakaoMap.vue'
import GoogleMapPlaceholder from '@/components/common/GoogleMapPlaceholder.vue'
import GooglePlacesAutocomplete from '@/components/common/GooglePlacesAutocomplete.vue'
import KakaoMapSearchModal from '@/components/common/KakaoMapSearchModal.vue'
import apiClient from '@/services/api'
import { useDistance } from '@/composables/useDistance'
import {
  Utensils,
  Coffee,
  ShoppingBag,
  Landmark,
  CircleDot,
} from 'lucide-vue-next'
import dayjs from 'dayjs'

const props = defineProps({
  trip: { type: Object, required: true },
  tripId: { type: String, default: null },
  effectiveReadonly: { type: Boolean, default: false },
  isDomestic: { type: Boolean, default: true },
})

const emit = defineEmits(['reload', 'open-kakao-map-search'])

// --- Modal states ---
const isItineraryModalOpen = ref(false)
const isItineraryDetailModalOpen = ref(false)
const showItineraryMap = ref(false)
const isKakaoMapSearchModalOpen = ref(false)
const selectedItineraryItems = ref([])
const isBulkChangeDayModalOpen = ref(false)
const bulkChangeDayTargetDay = ref(1)

// --- Data for modals ---
const itineraryItemData = ref({
  locationName: '',
  address: '',
  latitude: null,
  longitude: null,
  googlePlaceId: null,
  kakaoPlaceId: null,
  phoneNumber: null,
  kakaoPlaceUrl: null,
  expenseAmount: null,
})

// --- Editing items ---
const editingItineraryItem = ref(null)
const selectedItinerary = ref(null)

// --- Drag and drop ---
const {
  calculateItemDistances,
  optimizeRouteByDistance,
  calculateTotalDistance,
} = useDistance()
const editModeByDay = ref({})
const draggedItem = ref(null)
const draggedDay = ref(null)
const touchStartY = ref(0)
const touchDraggedElement = ref(null)

// --- Kakao Map Search ---
const currentKakaoSearchTarget = ref(null)
const currentKakaoSearchInitialLocation = computed(() => {
  if (currentKakaoSearchTarget.value === 'itinerary') {
    return {
      name: itineraryItemData.value.locationName,
      address: itineraryItemData.value.address,
      latitude: itineraryItemData.value.latitude,
      longitude: itineraryItemData.value.longitude,
    }
  }
  return { latitude: null, longitude: null, name: '', address: '' }
})

// --- Category icons ---
const categoryIconMap = {
  음식점: Utensils,
  카페: Coffee,
  쇼핑: ShoppingBag,
  관광: Landmark,
  기타: CircleDot,
}

function getCategoryIcon(category) {
  return categoryIconMap[category] || CircleDot
}

// Badge colors for itinerary items
const badgeColors = [
  '#10B981',
  '#8B5CF6',
  '#EF4444',
  '#F59E0B',
  '#3B82F6',
  '#EC4899',
]

function getBadgeColor(index) {
  return badgeColors[index % badgeColors.length]
}

// --- Grouped Itinerary ---
const groupedItinerary = computed(() => {
  if (!props.trip.startDate || !props.trip.endDate) return []

  const startDate = new Date(props.trip.startDate)
  const endDate = new Date(props.trip.endDate)
  const daysDiff = Math.ceil((endDate - startDate) / (1000 * 60 * 60 * 24)) + 1

  const itemsByDay = {}
  if (props.trip.itineraryItems) {
    props.trip.itineraryItems.forEach((item) => {
      const day = item.dayNumber || 1
      if (!itemsByDay[day]) itemsByDay[day] = []
      itemsByDay[day].push(item)
    })
  }

  const allDays = []
  for (let i = 1; i <= daysDiff; i++) {
    let items = itemsByDay[i] || []
    items.sort((a, b) => (a.orderNum || 0) - (b.orderNum || 0))

    const itemsWithDistance = calculateItemDistances(items)
    const totalDistance = calculateTotalDistance(items)

    const currentDate = new Date(startDate)
    currentDate.setDate(startDate.getDate() + i - 1)

    allDays.push({
      dayNumber: i,
      date: currentDate,
      items: itemsWithDistance,
      totalDistance,
    })
  }

  return allDays
})

// --- Itinerary CRUD ---
function openItineraryModal(item = null, dayNumber = null) {
  if (!props.trip.startDate) {
    alert('여행 기간을 먼저 설정해주세요.')
    return
  }

  editingItineraryItem.value = item
  if (item) {
    itineraryItemData.value = {
      locationName: item.locationName || '',
      address: item.address || '',
      latitude: item.latitude || null,
      longitude: item.longitude || null,
      googlePlaceId: item.googlePlaceId || null,
      kakaoPlaceId: item.kakaoPlaceId || null,
      phoneNumber: item.phoneNumber || null,
      category: item.category || null,
      kakaoPlaceUrl: item.kakaoPlaceUrl || null,
      expenseAmount: item.expenseAmount || null,
      dayNumber: item.dayNumber,
      startTime: item.startTime || '',
      endTime: item.endTime || '',
      notes: item.notes || '',
    }
  } else {
    const targetDayNumber = dayNumber || 1
    let defaultStartTime = '09:00'
    let defaultEndTime = '10:00'

    const itemsForDay = props.trip.itineraryItems
      .filter((i) => i.dayNumber === targetDayNumber)
      .sort((a, b) => (a.orderNum || 0) - (b.orderNum || 0))

    if (itemsForDay.length > 0) {
      const lastItem = itemsForDay[itemsForDay.length - 1]
      if (lastItem.endTime) {
        defaultStartTime = lastItem.endTime
        const [lastEndHour, lastEndMinute] = lastItem.endTime
          .split(':')
          .map(Number)
        const nextEndTime = dayjs()
          .hour(lastEndHour)
          .minute(lastEndMinute)
          .add(1, 'hour')
        defaultEndTime = nextEndTime.format('HH:mm')
      }
    }

    itineraryItemData.value = {
      locationName: '',
      address: '',
      latitude: null,
      longitude: null,
      googlePlaceId: null,
      kakaoPlaceId: null,
      notes: '',
      category: '',
      phoneNumber: null,
      kakaoPlaceUrl: null,
      expenseAmount: null,
      dayNumber: targetDayNumber,
      startTime: defaultStartTime,
      endTime: defaultEndTime,
    }
  }
  isItineraryModalOpen.value = true
}

function closeItineraryModal() {
  isItineraryModalOpen.value = false
}

async function saveItineraryItem() {
  if (props.effectiveReadonly) return
  try {
    const isNewItem = !editingItineraryItem.value?.id
    let targetItemId = isNewItem ? null : editingItineraryItem.value.id

    const cleanNumber = (value) => {
      if (!value) return null
      const cleaned = String(value).replace(/,/g, '')
      return cleaned ? Number(cleaned) : null
    }

    const payload = {
      ...itineraryItemData.value,
      personalTripId: props.tripId,
      expenseAmount: cleanNumber(itineraryItemData.value.expenseAmount),
    }

    if (isNewItem) {
      const response = await apiClient.post(
        `/personal-trips/${props.tripId}/items`,
        payload,
      )
      targetItemId = response.data.id
    } else {
      await apiClient.put(
        `/personal-trips/items/${editingItineraryItem.value.id}`,
        payload,
      )
    }

    emit('reload')
    closeItineraryModal()

    if (targetItemId) {
      await nextTick()
      const itemElement = document.querySelector(
        `[data-item-id="${targetItemId}"]`,
      )
      if (itemElement) {
        itemElement.scrollIntoView({ behavior: 'smooth', block: 'center' })
      }
    }
  } catch (error) {
    console.error('Failed to save itinerary item:', error)
    alert('저장에 실패했습니다.')
  }
}

async function deleteItineraryItem(id) {
  if (props.effectiveReadonly) return
  if (!confirm('이 일정을 삭제하시겠습니까?')) return
  try {
    await apiClient.delete(`/personal-trips/items/${id}`)
    emit('reload')
    closeItineraryModal()
  } catch (error) {
    console.error('Failed to delete itinerary item:', error)
    alert('삭제에 실패했습니다.')
  }
}

function deleteItineraryItemFromList(id) {
  deleteItineraryItem(id)
}

// --- Itinerary Detail Modal ---
function openItineraryDetailModal(item) {
  selectedItinerary.value = item
  showItineraryMap.value = false
  isItineraryDetailModalOpen.value = true
}

function closeItineraryDetailModal() {
  isItineraryDetailModalOpen.value = false
  showItineraryMap.value = false
}

function editSelectedItinerary() {
  closeItineraryDetailModal()
  openItineraryModal(selectedItinerary.value)
}

// --- Drag and Drop (Desktop) ---
function onDragStart(item, dayNumber, event) {
  if (!isEditModeForDay(dayNumber)) return

  draggedItem.value = item
  draggedDay.value = dayNumber

  document.body.style.overflow = 'hidden'
  document.body.style.touchAction = 'none'

  if (event.dataTransfer) {
    event.dataTransfer.effectAllowed = 'move'
  }
}

function onDrag(e) {
  e.preventDefault()
}

function onDragEnd() {
  document.body.style.overflow = ''
  document.body.style.touchAction = ''
}

function onDragOver(e) {
  e.preventDefault()
  e.stopPropagation()
}

function onDrop(targetItem, targetDayNumber) {
  if (!isEditModeForDay(targetDayNumber) || !draggedItem.value) return

  document.body.style.overflow = ''
  document.body.style.touchAction = ''

  const items = props.trip.itineraryItems
  const draggedIndex = items.findIndex((i) => i.id === draggedItem.value.id)
  const targetIndex = items.findIndex((i) => i.id === targetItem.id)

  if (draggedIndex === -1 || targetIndex === -1) return

  if (draggedDay.value !== targetDayNumber) {
    alert('같은 날짜 내에서만 순서를 변경할 수 있습니다.')
    draggedItem.value = null
    draggedDay.value = null
    return
  }

  const [removed] = items.splice(draggedIndex, 1)
  items.splice(targetIndex, 0, removed)

  saveItineraryOrder(targetDayNumber)

  draggedItem.value = null
  draggedDay.value = null
}

// --- Touch Drag and Drop (Mobile) ---
function onTouchStart(item, dayNumber, event) {
  if (!isEditModeForDay(dayNumber)) return

  draggedItem.value = item
  draggedDay.value = dayNumber
  touchStartY.value = event.touches[0].clientY
  touchDraggedElement.value = event.currentTarget

  event.currentTarget.style.opacity = '0.5'
  event.currentTarget.style.transform = 'scale(1.05)'
}

function onTouchMove(event) {
  if (!draggedItem.value) return
  event.preventDefault()

  const touch = event.touches[0]
  const elements = document.elementsFromPoint(touch.clientX, touch.clientY)
  const targetCard = elements.find(
    (el) => el.hasAttribute('data-item-id') && el !== touchDraggedElement.value,
  )

  if (targetCard) {
    targetCard.style.borderColor = 'rgba(23, 177, 133, 1)'
  }
}

function onTouchEnd(targetItem, targetDayNumber, event) {
  if (!draggedItem.value) return

  if (touchDraggedElement.value) {
    touchDraggedElement.value.style.opacity = ''
    touchDraggedElement.value.style.transform = ''
  }

  const touch = event.changedTouches[0]
  const elements = document.elementsFromPoint(touch.clientX, touch.clientY)
  const targetCard = elements.find(
    (el) => el.hasAttribute('data-item-id') && el !== touchDraggedElement.value,
  )

  if (targetCard) {
    const targetId = parseInt(targetCard.getAttribute('data-item-id'))
    const target = props.trip.itineraryItems.find((i) => i.id === targetId)

    if (target) {
      if (draggedDay.value !== target.dayNumber) {
        alert('같은 날짜 내에서만 순서를 변경할 수 있습니다.')
        draggedItem.value = null
        draggedDay.value = null
        touchDraggedElement.value = null
        return
      }

      const items = props.trip.itineraryItems
      const draggedIndex = items.findIndex((i) => i.id === draggedItem.value.id)
      const targetIndex = items.findIndex((i) => i.id === targetId)

      if (
        draggedIndex !== -1 &&
        targetIndex !== -1 &&
        draggedIndex !== targetIndex
      ) {
        const [removed] = items.splice(draggedIndex, 1)
        items.splice(targetIndex, 0, removed)
        saveItineraryOrder(target.dayNumber)
      }
    }
  }

  draggedItem.value = null
  draggedDay.value = null
  touchDraggedElement.value = null
}

async function saveItineraryOrder(dayNumber) {
  if (props.effectiveReadonly) return
  const dayItems = props.trip.itineraryItems
    .filter((item) => item.dayNumber === dayNumber)
    .map((item, index) => ({
      id: item.id,
      orderNum: index,
    }))

  try {
    await apiClient.put(`/personal-trips/${props.tripId}/items/reorder`, {
      items: dayItems,
    })
    dayItems.forEach(({ id, orderNum }) => {
      const item = props.trip.itineraryItems.find((i) => i.id === id)
      if (item) {
        item.orderNum = orderNum
      }
    })
  } catch (error) {
    console.error('Failed to save order:', error)
    alert('순서 저장에 실패했습니다.')
    emit('reload')
  }
}

// --- Route Optimization ---
async function optimizeRoute(dayNumber) {
  const dayGroup = groupedItinerary.value.find((g) => g.dayNumber === dayNumber)
  if (!dayGroup) return

  const dayItems = dayGroup.items
  if (dayItems.length < 2) {
    alert('경로를 최적화하려면 2개 이상의 일정이 필요합니다.')
    return
  }

  const optimized = optimizeRouteByDistance(dayItems)

  if (optimized.length === dayItems.length) {
    const updatePayload = optimized.map((item, index) => ({
      id: item.id,
      orderNum: index,
    }))

    try {
      await apiClient.put(`/personal-trips/${props.tripId}/items/reorder`, {
        items: updatePayload,
      })
      emit('reload')
      alert('경로가 최적화되었습니다!')
    } catch (error) {
      console.error('Failed to optimize route:', error)
      alert('경로 최적화에 실패했습니다.')
    }
  }
}

// --- Edit Mode ---
function toggleEditMode(dayNumber) {
  editModeByDay.value[dayNumber] = !editModeByDay.value[dayNumber]

  if (!editModeByDay.value[dayNumber]) {
    const dayItems =
      props.trip.itineraryItems?.filter(
        (item) => item.dayNumber === dayNumber,
      ) || []
    selectedItineraryItems.value = selectedItineraryItems.value.filter(
      (id) => !dayItems.some((item) => item.id === id),
    )
  }
}

function isEditModeForDay(dayNumber) {
  return editModeByDay.value[dayNumber] || false
}

// --- Item Selection ---
function toggleItemSelection(id) {
  const index = selectedItineraryItems.value.indexOf(id)
  if (index > -1) {
    selectedItineraryItems.value.splice(index, 1)
  } else {
    selectedItineraryItems.value.push(id)
  }
}

function getSelectedItemsForDay(dayNumber) {
  const dayItems =
    props.trip.itineraryItems?.filter((item) => item.dayNumber === dayNumber) ||
    []
  return selectedItineraryItems.value.filter((id) =>
    dayItems.some((item) => item.id === id),
  )
}

// --- Bulk Actions ---
function isAllSelectedForDay(dayNumber) {
  const dayItems =
    props.trip.itineraryItems?.filter(
      (item) => item.dayNumber === dayNumber && !item.isAutoGenerated,
    ) || []
  if (dayItems.length === 0) return false
  return dayItems.every((item) =>
    selectedItineraryItems.value.includes(item.id),
  )
}

function toggleSelectAllForDay(dayNumber) {
  const dayItems =
    props.trip.itineraryItems?.filter(
      (item) => item.dayNumber === dayNumber && !item.isAutoGenerated,
    ) || []
  const allSelected = isAllSelectedForDay(dayNumber)

  if (allSelected) {
    selectedItineraryItems.value = selectedItineraryItems.value.filter(
      (id) => !dayItems.some((item) => item.id === id),
    )
  } else {
    const dayItemIds = dayItems.map((item) => item.id)
    selectedItineraryItems.value = [
      ...new Set([...selectedItineraryItems.value, ...dayItemIds]),
    ]
  }
}

async function bulkDeleteSelectedItems(dayNumber) {
  if (props.effectiveReadonly) return
  const itemsToDelete = getSelectedItemsForDay(dayNumber)
  if (itemsToDelete.length === 0) return
  if (!confirm(`${itemsToDelete.length}개의 일정을 삭제하시겠습니까?`)) return

  try {
    await Promise.all(
      itemsToDelete.map((id) =>
        apiClient.delete(`/personal-trips/items/${id}`),
      ),
    )
    selectedItineraryItems.value = selectedItineraryItems.value.filter(
      (id) => !itemsToDelete.includes(id),
    )
    emit('reload')
  } catch (error) {
    console.error('Failed to delete items:', error)
    alert('일정 삭제에 실패했습니다.')
  }
}

function openBulkChangeDayModal() {
  if (selectedItineraryItems.value.length === 0) return
  isBulkChangeDayModalOpen.value = true
}

function closeBulkChangeDayModal() {
  isBulkChangeDayModalOpen.value = false
}

async function saveBulkChangeDay() {
  if (props.effectiveReadonly) return
  if (selectedItineraryItems.value.length === 0) return

  try {
    await Promise.all(
      selectedItineraryItems.value.map((id) =>
        apiClient.put(`/personal-trips/items/${id}`, {
          dayNumber: bulkChangeDayTargetDay.value,
        }),
      ),
    )
    selectedItineraryItems.value = []
    emit('reload')
    closeBulkChangeDayModal()
  } catch (error) {
    console.error('Failed to change day:', error)
    alert('날짜 변경에 실패했습니다.')
  }
}

// --- Date Formatting ---
function formatDateWithDay(date) {
  const weekdays = ['일', '월', '화', '수', '목', '금', '토']
  return `${dayjs(date).format('M/D')}(${weekdays[dayjs(date).day()]})`
}

// --- Kakao Map Search ---
function openKakaoMapSearchModal(target) {
  currentKakaoSearchTarget.value = target
  isKakaoMapSearchModalOpen.value = true
}

function handleKakaoPlaceSelection(place) {
  if (currentKakaoSearchTarget.value === 'itinerary') {
    itineraryItemData.value.locationName = place.name
    itineraryItemData.value.address = place.address
    itineraryItemData.value.latitude = place.latitude
    itineraryItemData.value.longitude = place.longitude
    itineraryItemData.value.kakaoPlaceId = place.kakaoPlaceId
    itineraryItemData.value.phoneNumber = place.phoneNumber || null
    itineraryItemData.value.category = place.category || null
    itineraryItemData.value.kakaoPlaceUrl = place.kakaoPlaceUrl || null
  }
  isKakaoMapSearchModalOpen.value = false
}

// Expose methods so parent can call them
defineExpose({
  openItineraryModal,
  closeItineraryModal,
  openItineraryDetailModal,
  closeItineraryDetailModal,
  deleteItineraryItemFromList,
  toggleEditMode,
  isEditModeForDay,
  toggleItemSelection,
  getSelectedItemsForDay,
  isAllSelectedForDay,
  toggleSelectAllForDay,
  bulkDeleteSelectedItems,
  openBulkChangeDayModal,
  closeBulkChangeDayModal,
  saveBulkChangeDay,
  optimizeRoute,
  onDragStart,
  onDrag,
  onDragEnd,
  onDragOver,
  onDrop,
  onTouchStart,
  onTouchMove,
  onTouchEnd,
  groupedItinerary,
  formatDateWithDay,
  getBadgeColor,
  getCategoryIcon,
  selectedItineraryItems,
  editModeByDay,
  isBulkChangeDayModalOpen,
  bulkChangeDayTargetDay,
  openKakaoMapSearchModal,
  handleKakaoPlaceSelection,
  currentKakaoSearchInitialLocation,
})
</script>
