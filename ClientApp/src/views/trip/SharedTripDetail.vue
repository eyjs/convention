<template>
  <div class="min-h-screen relative bg-gray-50">
    <div class="relative z-10 pt-4">
      <div v-if="loading" class="text-center py-20">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-4 text-gray-600 font-medium">여행 정보를 불러오는 중...</p>
      </div>

      <div v-else class="max-w-2xl mx-auto px-4 py-4">
        <!-- Hero Section with Trip Info -->
        <section class="relative overflow-hidden bg-primary-500 rounded-2xl shadow-xl p-6 mb-6 text-white">
          <!-- Background Image -->
          <div
            v-if="trip.coverImageUrl"
            class="absolute inset-0 bg-cover bg-center"
            :style="{ backgroundImage: `url(${trip.coverImageUrl})` }"
          ></div>
          <!-- Overlay for text readability -->
          <div class="absolute inset-0 bg-black/30"></div>

          <div class="relative z-10">
            <div class="flex justify-between items-start mb-4">
              <div class="flex-1">
                <h2 class="text-2xl font-bold mb-2">{{ trip.title }}</h2>
                <div class="flex items-center gap-2 text-white/90 mb-1">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                  <span class="font-medium">{{ trip.startDate }} ~ {{ trip.endDate }}</span>
                </div>
                <div class="flex items-center gap-2 text-white/90">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
                  <span class="font-medium">{{ trip.destination }}</span>
                </div>
              </div>
            </div>
            <p v-if="trip.description" class="text-white/90 text-sm leading-relaxed mt-2">{{ trip.description }}</p>
          </div>
        </section>

        <!-- Flights & Accommodations -->
        <div class="space-y-4 mb-6">
          <section v-if="trip.flights && trip.flights.length > 0" class="bg-white rounded-2xl shadow-md p-5">
            <h2 class="text-lg font-bold text-gray-900 flex items-center gap-2 mb-3">
              <svg class="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" /></svg>
              항공편
            </h2>
            <div class="space-y-2">
              <div v-for="flight in trip.flights" :key="flight.id" class="border-t pt-2">
                <p class="font-semibold">{{ flight.airline }} {{ flight.flightNumber }}</p>
                <p class="text-sm text-gray-600">{{ flight.departureLocation }} → {{ flight.arrivalLocation }}</p>
              </div>
            </div>
          </section>
          <section v-if="trip.accommodations && trip.accommodations.length > 0">
            <h2 class="text-lg font-bold text-gray-900 flex items-center gap-2 mb-3 px-2">
              <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" /></svg>
              숙소
            </h2>
            <div class="space-y-4">
              <AccommodationCard
                v-for="acc in trip.accommodations"
                :key="acc.id"
                :accommodation="acc"
                :show-actions="false"
                :show-time-remaining="true"
                @click="openAccommodationDetailModal(acc)"
                class="cursor-pointer"
              />
            </div>
          </section>
        </div>

        <!-- 일정 -->
        <section class="bg-white rounded-2xl shadow-md p-5">
          <div class="flex justify-between items-center mb-5">
            <h2 class="text-xl font-bold text-gray-900">일정</h2>
          </div>
      
      
              <!-- Day Filter Tabs with Scroll Arrows -->
              <div v-if="groupedItinerary.length > 0" class="relative">
                <div
                  ref="dayFilterContainer"
                  class="flex gap-2 overflow-x-auto pb-2 mb-4 no-scrollbar"
                  @scroll="handleDayFilterScroll"
                >
                  <button 
                    @click="selectedDay = null"
                    :class="['flex items-center justify-center px-5 py-2.5 rounded-xl font-bold text-sm whitespace-nowrap transition-all', selectedDay === null ? 'text-white shadow-lg scale-105' : 'bg-gray-100 text-gray-700 hover:bg-gray-200']"
                    :style="selectedDay === null ? 'background-color: rgba(23, 177, 133, 1);' : ''"
                  >
                    전체
                  </button>
                  <button 
                    v-for="day in groupedItinerary"
                    :key="day.dayNumber"
                    @click="selectedDay = day.dayNumber"
                    :class="['flex items-center justify-center px-5 py-2.5 rounded-xl font-bold text-sm whitespace-nowrap transition-all', selectedDay === day.dayNumber ? 'text-white shadow-lg scale-105' : 'bg-gray-100 text-gray-700 hover:bg-gray-200']"
                    :style="selectedDay === day.dayNumber ? 'background-color: rgba(23, 177, 133, 1);' : ''"
                  >
                    Day {{ day.dayNumber }}
                  </button>
                </div>
                
                <!-- Left Scroll Arrow -->
                <div
                  v-if="showLeftDayScroll"
                  class="absolute left-0 top-0 bottom-2 flex items-center bg-gradient-to-r from-white to-transparent pr-4 pointer-events-none"
                >
                  <button
                    @click="scrollDayFilterLeft"
                    class="p-1.5 bg-white rounded-full shadow-md pointer-events-auto hover:bg-gray-50 transition-colors"
                  >
                    <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                    </svg>
                  </button>
                </div>
                
                <!-- Right Scroll Arrow -->
                <div
                  v-if="showRightDayScroll"
                  class="absolute right-0 top-0 bottom-2 flex items-center bg-gradient-to-l from-white to-transparent pl-4 pointer-events-none"
                >
                  <button
                    @click="scrollDayFilterRight"
                    class="p-1.5 bg-white rounded-full shadow-md pointer-events-auto hover:bg-gray-50 transition-colors"
                  >
                    <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                    </svg>
                  </button>
                </div>
              </div>

          <div v-if="groupedItinerary.length === 0" class="text-center py-12">
            <p class="text-gray-500 font-medium">등록된 일정이 없습니다</p>
          </div>

          <div v-else class="space-y-6">
            <div v-for="dayGroup in filteredItinerary" :key="dayGroup.dayNumber">
              <div class="mb-4">
                <h3 class="text-lg font-bold text-gray-900">Day {{ dayGroup.dayNumber }}</h3>
                <span class="text-sm text-gray-500">{{ dayGroup.items.length }}개 일정</span>
              </div>

              <!-- Itinerary Items with Improved Layout -->
              <div>
                <div v-for="(item, index) in dayGroup.items" :key="item.id" class="flex gap-4">
                  <!-- 1. Timeline Column (Original Style) -->
                  <div class="relative flex-shrink-0 w-5 flex flex-col items-center">
                    <!-- Top line (hidden for first item) -->
                    <div
                      v-if="index > 0"
                      class="absolute top-0 left-1/2 -translate-x-1/2 h-9"
                      style="width: 0px; border-right: 1px dashed rgba(23, 177, 133, 0.5);"
                    ></div>
                    
                    <!-- Bullet -->
                    <div
                      class="relative z-10 w-3 h-3 mt-9 rounded-full flex-shrink-0"
                      style="background-color: rgba(23, 177, 133, 1);"
                    ></div>
  
                    <!-- Bottom line & Distance Badge Container -->
                    <div
                      v-if="index < dayGroup.items.length - 1"
                      class="absolute top-9 bottom-0 left-1/2 -translate-x-1/2"
                      style="width: 0px;"
                    >
                      <!-- The actual line -->
                      <div class="absolute inset-0" style="border-right: 1px dashed rgba(23, 177, 133, 0.5);"></div>
                      
                      <!-- Distance Badge -->
                      <div
                        v-if="item.distanceToNext"
                        class="absolute z-20 bottom-0 left-1/2 -translate-x-1/2 px-2 py-0.5 rounded-full text-xs font-medium whitespace-nowrap"
                        style="background-color: rgba(23, 177, 133, 0.1); color: rgba(23, 177, 133, 1);"
                      >
                        {{ item.distanceToNext.formatted }}
                      </div>
                    </div>
                  </div>
  
                  <!-- 2. Content Column (Improved) -->
                  <div class="flex-1 pb-6">
                    <div
                      @click="openItineraryDetailModal(item)"
                      :ref="(el) => { if (item.id === currentItineraryItemId) currentItineraryItemRef = el }"
                      :data-item-id="item.id"
                      class="group relative bg-white rounded-xl p-4 shadow-md hover:shadow-lg transition-all cursor-pointer"
                      :style="currentItineraryItemId === item.id ? 'border: 2px solid rgba(23, 177, 133, 1); box-shadow: 0 4px 6px -1px rgba(23, 177, 133, 0.1);' : ''"
                    >
                      <div class="flex items-start justify-between gap-3">
                        <!-- Main content -->
                        <div class="flex-1 min-w-0">
                          <!-- Place name + category icon -->
                          <div class="flex items-center gap-2 mb-1">
                            <h3 class="font-bold text-gray-900 text-base">{{ item.locationName }}</h3>
                            <component :is="getCategoryIcon(item.category)" v-if="item.category" class="w-4 h-4 text-gray-500 flex-shrink-0" />
                          </div>
                          
                          <!-- Category + Time range -->
                          <div class="flex items-center gap-2 text-sm text-gray-500 mb-1">
                            <span v-if="item.category">{{ item.category }}</span>
                            <span v-if="item.category && item.startTime && item.endTime">·</span>
                            <span v-if="item.startTime && item.endTime" class="text-primary-600 font-medium">
                              {{ item.startTime.substring(0, 5) }}-{{ item.endTime.substring(0, 5) }}
                            </span>
                          </div>
                          
                          <!-- Notes/Details -->
                          <p v-if="item.notes && item.notes.trim() !== ''" class="text-sm text-gray-600 leading-relaxed mt-2">
                            {{ item.notes }}
                          </p>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>

    <!-- Modals -->
    <AccommodationDetailModal
      :is-open="isAccommodationDetailModalOpen"
      :accommodation="selectedAccommodation"
      :is-domestic="isDomestic"
      :show-edit="false"
      @update:is-open="isAccommodationDetailModalOpen = $event"
    />

    <SlideUpModal :is-open="isItineraryDetailModalOpen" @close="closeItineraryDetailModal">
      <template #header-title>일정 상세</template>
      <template #body>
        <div v-if="selectedItinerary" class="space-y-4">
          <h3 class="text-xl font-bold">{{ selectedItinerary.locationName }}</h3>
          <p v-if="selectedItinerary.address" class="text-gray-600">{{ selectedItinerary.address }}</p>
          <p v-if="selectedItinerary.startTime && selectedItinerary.endTime" class="text-gray-800 font-medium">{{ selectedItinerary.startTime.substring(0, 5) }} - {{ selectedItinerary.endTime.substring(0, 5) }}</p>
          <p v-if="selectedItinerary.notes" class="whitespace-pre-wrap bg-gray-50 p-3 rounded-lg">{{ selectedItinerary.notes }}</p>
          
          <button 
            v-if="selectedItinerary.latitude && !showItineraryMap" 
            @click="showItineraryMap = true"
            class="w-full py-2.5 bg-gray-100 text-gray-700 rounded-lg font-semibold hover:bg-gray-200 transition-colors"
          >
            지도 보기
          </button>

          <div v-if="showItineraryMap" class="space-y-2">
            <div class="h-48 w-full rounded-lg">
              <KakaoMap
                v-if="isDomestic && selectedItinerary.latitude && selectedItinerary.longitude"
                :latitude="selectedItinerary.latitude"
                :longitude="selectedItinerary.longitude"
              />
              <GoogleMapPlaceholder v-else-if="!isDomestic && selectedItinerary.latitude" />
            </div>
            <button 
              @click="showItineraryMap = false"
              class="w-full py-2.5 bg-gray-100 text-gray-700 rounded-lg font-semibold hover:bg-gray-200 transition-colors"
            >
              지도 접기
            </button>
          </div>
        </div>
      </template>
      <template #footer>
        <div class="flex gap-3 w-full">
          <button type="button" @click="closeItineraryDetailModal" class="flex-1 py-3 px-4 bg-gradient-to-r from-cyan-500 to-blue-600 text-white rounded-xl font-semibold hover:shadow-lg active:scale-95 transition-all">닫기</button>
        </div>
      </template>
    </SlideUpModal>
  </div>
</template>
<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import AccommodationDetailModal from '@/components/personalTrip/AccommodationDetailModal.vue'
import AccommodationCard from '@/components/personalTrip/AccommodationCard.vue'
import KakaoMap from '@/components/common/KakaoMap.vue'
import GoogleMapPlaceholder from '@/components/common/GoogleMapPlaceholder.vue'
import apiClient from '@/services/api'
import { useDistance } from '@/composables/useDistance'
import { Utensils, Coffee, ShoppingBag, Landmark, CircleDot } from 'lucide-vue-next'
import dayjs from 'dayjs'

const route = useRoute()
const router = useRouter()
const shareToken = computed(() => route.params.shareToken)

const loading = ref(true)
const trip = ref({})
const isDomestic = computed(() => trip.value.countryCode === 'KR')

const categoryIconMap = {
  '음식점': Utensils,
  '카페': Coffee,
  '쇼핑': ShoppingBag,
  '관광': Landmark,
  '기타': CircleDot,
}

function getCategoryIcon(category) {
  return categoryIconMap[category] || CircleDot
}
const now = ref(new Date())
const selectedDay = ref(null)
const dayFilterContainer = ref(null)
const showLeftDayScroll = ref(false)
const showRightDayScroll = ref(false)

const handleDayFilterScroll = () => {
  if (dayFilterContainer.value) {
    showLeftDayScroll.value = dayFilterContainer.value.scrollLeft > 0
    showRightDayScroll.value =
      dayFilterContainer.value.scrollLeft <
      dayFilterContainer.value.scrollWidth - dayFilterContainer.value.clientWidth
  }
}

const scrollDayFilterLeft = () => {
  dayFilterContainer.value?.scrollBy({ left: -200, behavior: 'smooth' })
}

const scrollDayFilterRight = () => {
  dayFilterContainer.value?.scrollBy({ left: 200, behavior: 'smooth' })
}


const currentItineraryItemRef = ref(null)

// Modal states
const isAccommodationDetailModalOpen = ref(false)
const isItineraryDetailModalOpen = ref(false)
const showItineraryMap = ref(false)
const selectedAccommodation = ref(null)
const selectedItinerary = ref(null)

// --- Lifecycle and Data Loading ---
onMounted(async () => {
  await loadTrip()
  setInterval(() => { now.value = new Date() }, 60000) // Update time every minute for highlight
  await nextTick()
  handleDayFilterScroll()
})


async function loadTrip() {
  loading.value = true
  try {
    const response = await apiClient.get(`/personal-trips/public/${shareToken.value}`)
    trip.value = response.data
  } catch (error) {
    console.error('Failed to load shared trip:', error)
    alert('여행 정보를 불러오는 데 실패했습니다. 링크가 유효하지 않을 수 있습니다.')
    // Optionally, redirect to a 404 page or home
    router.push('/')
  } finally {
    loading.value = false
  }
}

// --- Modal Controls ---
function openAccommodationDetailModal(acc) {
  selectedAccommodation.value = acc
  isAccommodationDetailModalOpen.value = true
}

function openItineraryDetailModal(item) {
  selectedItinerary.value = item
  showItineraryMap.value = false
  isItineraryDetailModalOpen.value = true
}

function closeItineraryDetailModal() {
  isItineraryDetailModalOpen.value = false
  showItineraryMap.value = false
}

// --- Itinerary ---
const { calculateItemDistances } = useDistance()

const groupedItinerary = computed(() => {
  if (!trip.value.startDate || !trip.value.endDate) return []

  const startDate = new Date(trip.value.startDate)
  const endDate = new Date(trip.value.endDate)
  const daysDiff = Math.ceil((endDate - startDate) / (1000 * 60 * 60 * 24)) + 1

  const itemsByDay = {}
  if (trip.value.itineraryItems) {
    trip.value.itineraryItems.forEach(item => {
      const day = item.dayNumber || 1
      if (!itemsByDay[day]) itemsByDay[day] = []
      itemsByDay[day].push(item)
    })
  }

  const allDays = []
  for (let i = 1; i <= daysDiff; i++) {
    let items = itemsByDay[i] || []
    items.sort((a, b) => (a.orderNum || 0) - (b.orderNum || 0));
    const itemsWithDistance = calculateItemDistances(items)
    
    const currentDate = new Date(startDate)
    currentDate.setDate(startDate.getDate() + i - 1)

    allDays.push({
      dayNumber: i,
      date: currentDate,
      items: itemsWithDistance,
    })
  }

  return allDays
})

const filteredItinerary = computed(() => {
  if (selectedDay.value === null) {
    return groupedItinerary.value
  }
  return groupedItinerary.value.filter(day => day.dayNumber === selectedDay.value)
})

watch(groupedItinerary, async () => {
  await nextTick()
  handleDayFilterScroll()
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

// Auto-scroll to current itinerary item
watch(currentItineraryItemId, async (newId) => {
  if (newId) {
    await nextTick();
    if (currentItineraryItemRef.value) {
      currentItineraryItemRef.value.scrollIntoView({
        behavior: 'smooth',
        block: 'center',
      });
    }
  }
});
</script>
