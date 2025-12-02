<template>
  <SlideUpModal
    :is-open="isOpen"
    @close="closeModal"
    :z-index-class="zIndexClass"
    
  >
    <template #header-title>장소 검색</template>
    <template #body>
      <div class="space-y-4">
        <!-- 1. Search Input -->
        <div class="relative">
          <input
            type="text"
            v-model="searchTerm"
            @input="searchPlaces"
            placeholder="장소, 주소 검색"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
          />
          <button
            v-if="searchTerm"
            @click="clearSearch"
            class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700"
          >
            ✕
          </button>
        </div>

        <!-- 2. Search Results / Status -->
        <div v-if="searchResults.length" class="max-h-64 overflow-y-scroll border rounded-lg">
          <ul>
            <li
              v-for="result in searchResults"
              :key="result.id"
              @click="showPlaceOnMap(result)"
              class="px-4 py-3 cursor-pointer hover:bg-gray-100 text-sm border-b last:border-b-0"
              :class="{ 'bg-blue-50': selectedPlaceForMap && selectedPlaceForMap.id === result.id }"
            >
              <p class="font-medium">{{ result.place_name }}</p>
              <p class="text-gray-500">{{ result.address_name }}</p>
              <p v-if="result.phone" class="text-gray-600 text-xs mt-1 flex items-center gap-1">
                <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                </svg>
                {{ result.phone }}
              </p>
            </li>
          </ul>
        </div>
        <p v-else-if="searchTerm && !loadingSearch" class="text-center text-gray-500 text-sm py-4">검색 결과가 없습니다.</p>
        <p v-else-if="loadingSearch" class="text-center text-blue-500 text-sm py-4">검색 중...</p>

        <!-- 3. Map Container (conditionally shown) -->
        <div v-if="selectedPlaceForMap">
          <div ref="mapContainer" class="w-full h-64 rounded-lg shadow-md mt-4"></div>
        </div>
      </div>
    </template>
    <template #footer>
      <div class="flex justify-end gap-3 w-full">
        <button type="button" @click="closeModal" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">닫기</button>
        <button 
          type="button" 
          @click="confirmSelection" 
          :disabled="!selectedPlaceForMap"
          class="flex-1 py-3 px-4 bg-gradient-to-r from-cyan-500 to-blue-600 text-white rounded-xl font-semibold hover:shadow-lg active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
        >
          이 장소로 선택
        </button>
      </div>
    </template>
  </SlideUpModal>
</template>

<script setup>
import { ref, watch, nextTick } from 'vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  initialLocation: {
    type: Object,
    default: () => ({ latitude: null, longitude: null, name: '', address: '' })
  },
  zIndexClass: {
    type: String,
    default: 'z-50'
  }
})

const emit = defineEmits(['update:isOpen', 'selectPlace'])

const searchTerm = ref('')
const mapContainer = ref(null)
let map = null
let places = null // Kakao Places service
let marker = null // Single marker instance
const searchResults = ref([])
const loadingSearch = ref(false)
const selectedPlaceForMap = ref(null)

// Watch for modal open state
watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    // Reset state when modal opens
    searchTerm.value = props.initialLocation.name || ''
    searchResults.value = []
    selectedPlaceForMap.value = null
    map = null
    places = null
    marker = null
  } else {
    // Clear any pending search timeout when closing
    clearTimeout(searchTimeout)
  }
})

function initMapAndMarker(place) {
  if (!window.kakao || !window.kakao.maps) {
    console.error('Kakao Maps API is not available.')
    return
  }

  const position = new window.kakao.maps.LatLng(place.y, place.x)
  
  const options = {
    center: position,
    level: 3
  }

  if (!map) {
    map = new window.kakao.maps.Map(mapContainer.value, options)
  } else {
    map.setCenter(position)
  }

  // Clear existing marker
  if (marker) {
    marker.setMap(null)
  }

  // Create and display new marker
  marker = new window.kakao.maps.Marker({
    map: map,
    position: position
  })

  map.relayout()
}

let searchTimeout = null
function searchPlaces() {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    if (searchTerm.value.length < 2) {
      searchResults.value = []
      return
    }
    loadingSearch.value = true
    
    if (!places) {
      places = new window.kakao.maps.services.Places()
    }
    
    places.keywordSearch(searchTerm.value, (data, status) => {
      loadingSearch.value = false
      if (status === window.kakao.maps.services.Status.OK) {
        searchResults.value = data
      } else {
        searchResults.value = []
      }
    })
  }, 300)
}

function showPlaceOnMap(place) {
  selectedPlaceForMap.value = place
  nextTick(() => {
    initMapAndMarker(place)
  })
}

function confirmSelection() {
  if (!selectedPlaceForMap.value) return

  const place = selectedPlaceForMap.value

  // 카테고리 파싱: "음식점 > 한식 > 육류,고기요리" -> "음식점"
  let categoryName = null
  if (place.category_name) {
    const categories = place.category_name.split(' > ')
    categoryName = categories[0] || null
  }

  emit('selectPlace', {
    name: place.place_name,
    address: place.address_name,
    latitude: parseFloat(place.y),
    longitude: parseFloat(place.x),
    kakaoPlaceId: place.id,
    phoneNumber: place.phone || null,
    category: categoryName,
    kakaoPlaceUrl: place.place_url || null
  })
  closeModal()
}

function clearSearch() {
  searchTerm.value = ''
  searchResults.value = []
  selectedPlaceForMap.value = null
  map = null
  marker = null
}

function closeModal() {
  emit('update:isOpen', false)
}
</script>

<style scoped>
/* Custom scrollbar for WebKit browsers */
ul::-webkit-scrollbar {
  width: 8px;
}

ul::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 10px;
}

ul::-webkit-scrollbar-thumb {
  background: #888;
  border-radius: 10px;
}

ul::-webkit-scrollbar-thumb:hover {
  background: #555;
}

/* Custom scrollbar for Firefox */
ul {
  scrollbar-width: thin; /* "auto" or "thin" */
  scrollbar-color: #888 #f1f1f1; /* thumb and track color */
}
</style>
