<template>
  <SlideUpModal :is-open="isOpen" @close="closeModal">
    <template #header-title>장소 검색</template>
    <template #body>
      <div class="space-y-4">
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

        <div ref="mapContainer" class="w-full h-64 rounded-lg shadow-md"></div>

        <div v-if="searchResults.length" class="max-h-48 overflow-y-scroll border rounded-lg">
          <ul>
            <li
              v-for="result in searchResults"
              :key="result.id"
              @click="selectPlace(result)"
              class="px-4 py-2 cursor-pointer hover:bg-gray-100 text-sm"
            >
              <p class="font-medium">{{ result.place_name }}</p>
              <p class="text-gray-500">{{ result.address_name }}</p>
            </li>
          </ul>
        </div>
        <p v-else-if="searchTerm && !loadingSearch" class="text-center text-gray-500 text-sm">검색 결과가 없습니다.</p>
        <p v-else-if="loadingSearch" class="text-center text-blue-500 text-sm">검색 중...</p>
      </div>
    </template>
    <template #footer>
      <div class="flex justify-end gap-3">
        <button type="button" @click="closeModal" class="btn-secondary">닫기</button>
      </div>
    </template>
  </SlideUpModal>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  initialLocation: {
    type: Object,
    default: () => ({ latitude: null, longitude: null, name: '', address: '' })
  }
})

const emit = defineEmits(['update:isOpen', 'selectPlace'])

const searchTerm = ref('')
const mapContainer = ref(null)
let map = null
let places = null // Kakao Places service
let markers = []
let infowindow = null
const searchResults = ref([])
const loadingSearch = ref(false)

onMounted(() => {
  // Ensure Kakao Maps API is loaded before attempting to initialize
  if (window.kakao && window.kakao.maps) {
    // Map initialization will happen when modal opens
  } else {
    console.error('Kakao Maps API is not loaded on mount.')
  }
})

// Watch for modal open state
watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    searchTerm.value = props.initialLocation.name || ''
    nextTick(() => {
      // Initialize map only if not already initialized and container is ready
      if (!map && mapContainer.value && window.kakao && window.kakao.maps) {
        initMap()
      }
      // Always relayout map after modal opens to ensure it's correctly sized
      if (map) {
        // Add a small delay to ensure modal transition is complete
        setTimeout(() => {
          map.relayout()
          // If there's an initial location, set center and add marker
          if (props.initialLocation.latitude && props.initialLocation.longitude) {
            const center = new window.kakao.maps.LatLng(props.initialLocation.latitude, props.initialLocation.longitude)
            map.setCenter(center)
            addMarker(center, props.initialLocation.name, props.initialLocation.address)
          } else {
            // Default center for Korea if no initial location
            map.setCenter(new window.kakao.maps.LatLng(33.450701, 126.570667))
          }
        }, 50); // Small delay
      }
    })
  } else {
    clearMarkers()
    searchResults.value = []
    clearTimeout(searchTimeout); // Clear any pending search timeout
    if (map) {
      // map.relayout(); // Not strictly necessary before nullifying, but can help clean up
      map = null; // Explicitly nullify map instance when modal closes
      places = null; // Also nullify places service
    }
  }
})

function initMap() {
  // Ensure window.kakao.maps is available before using it
  if (!window.kakao || !window.kakao.maps) {
    console.error('Kakao Maps API is not available for initMap.')
    return
  }
  const options = {
    center: new window.kakao.maps.LatLng(33.450701, 126.570667), // Default to Jeju
    level: 3
  }
  map = new window.kakao.maps.Map(mapContainer.value, options)
  places = new window.kakao.maps.services.Places() // Initialize Places service
  infowindow = new window.kakao.maps.InfoWindow({ zIndex: 1 })
}

let searchTimeout = null
function searchPlaces() {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    if (searchTerm.value.length < 2) {
      searchResults.value = []
      clearMarkers()
      return
    }
    loadingSearch.value = true
    // Ensure places service is initialized
    if (places) {
      places.keywordSearch(searchTerm.value, placesSearchCB)
    } else {
      console.error('Kakao Places service not initialized.')
      loadingSearch.value = false
    }
  }, 300)
}

function placesSearchCB(data, status, pagination) {
  loadingSearch.value = false
  if (status === window.kakao.maps.services.Status.OK) {
    console.log('Kakao Search Result:', data); // Log the data to inspect its structure
    searchResults.value = data
    displayPlaces(data)
  } else {
    console.error('Kakao Places API search failed:', status)
    searchResults.value = []
    clearMarkers()
  }
}

function displayPlaces(places) {
  clearMarkers()
  if (places.length === 0) {
    return
  }

  const bounds = new window.kakao.maps.LatLngBounds()
  for (let i = 0; i < places.length; i++) {
    const placePosition = new window.kakao.maps.LatLng(places[i].y, places[i].x)
    addMarker(placePosition, places[i].place_name, places[i].address_name)
    bounds.extend(placePosition)
  }
  map.setBounds(bounds)
}

function addMarker(position, title, address) {
  const marker = new window.kakao.maps.Marker({
    map: map,
    position: position
  })
  markers.push(marker)

  window.kakao.maps.event.addListener(marker, 'click', () => {
    infowindow.setContent('<div style="padding:5px;font-size:12px;">' + title + '</div>')
    infowindow.open(map, marker)
    // Optionally, emit the place data when marker is clicked
    emit('selectPlace', {
      name: title,
      address: address,
      latitude: position.getLat(),
      longitude: position.getLng(),
      kakaoPlaceId: null // Kakao API doesn't directly provide a place_id like Google
    })
    closeModal()
  })
}

function clearMarkers() {
  for (let i = 0; i < markers.length; i++) {
    markers[i].setMap(null)
  }
  markers = []
  if (infowindow) infowindow.close()
}

function selectPlace(place) {
  emit('selectPlace', {
    name: place.place_name,
    address: place.address_name,
    latitude: parseFloat(place.y),
    longitude: parseFloat(place.x),
    kakaoPlaceId: place.id
  })
  closeModal()
}

function clearSearch() {
  searchTerm.value = ''
  searchResults.value = []
  clearMarkers()
}

function closeModal() {
  emit('update:isOpen', false)
}
</script>

<style scoped>
.btn-secondary {
  @apply px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100;
}
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
