<template>
  <div class="relative">
    <div class="relative">
      <input
        type="text"
        v-model="searchTerm"
        @input="onInput"
        @focus="onFocus"
        @blur="onBlur"
        @keydown.down.prevent="onArrowDown"
        @keydown.up.prevent="onArrowUp"
        @keydown.enter.prevent="onEnter"
        @keydown.escape="closeSuggestions"
        placeholder="도시 또는 국가명 검색 (예: 도쿄, 일본)"
        class="w-full px-4 py-3 pr-10 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-all"
        :class="{ 'ring-2 ring-primary-500': showSuggestions && searchResults.length }"
      />
      <!-- 검색 아이콘 -->
      <div class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400">
        <svg v-if="!isLoading" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
        <svg v-else class="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
      </div>
    </div>

    <!-- 검색 결과 드롭다운 -->
    <Transition name="dropdown">
      <ul
        v-if="showSuggestions && searchResults.length"
        class="absolute z-50 w-full mt-2 bg-white border border-gray-200 rounded-xl shadow-xl max-h-72 overflow-y-auto"
      >
        <li
          v-for="(result, index) in searchResults"
          :key="`${result.name_en}-${result.countryCode}`"
          @touchstart.prevent="selectResult(result)"
          @mousedown.prevent="selectResult(result)"
          class="px-4 py-3 cursor-pointer transition-colors border-b border-gray-50 last:border-b-0"
          :class="{
            'bg-primary-50 text-primary-700': index === highlightedIndex,
            'hover:bg-gray-50 active:bg-gray-100': index !== highlightedIndex
          }"
        >
          <div class="flex items-center gap-3">
            <!-- 국기 이모지 -->
            <span class="text-xl">{{ getCountryFlag(result.countryCode) }}</span>
            <div class="flex-1 min-w-0">
              <div class="font-medium text-gray-900 truncate">{{ result.name }}</div>
              <div class="text-sm text-gray-500 truncate">{{ result.country }} · {{ result.name_en }}</div>
            </div>
            <!-- 국내/해외 뱃지 -->
            <span
              v-if="result.isDomestic"
              class="px-2 py-0.5 text-xs font-medium bg-green-100 text-green-700 rounded-full"
            >
              국내
            </span>
          </div>
        </li>
      </ul>
    </Transition>

    <!-- 검색 결과 없음 -->
    <Transition name="dropdown">
      <div
        v-if="showSuggestions && searchTerm.length >= 1 && searchResults.length === 0 && !isLoading"
        class="absolute z-50 w-full mt-2 bg-white border border-gray-200 rounded-xl shadow-xl p-4 text-center text-gray-500"
      >
        <svg class="w-8 h-8 mx-auto mb-2 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <p class="text-sm">검색 결과가 없습니다</p>
        <p class="text-xs text-gray-400 mt-1">다른 키워드로 검색해보세요</p>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import { searchCities } from '@/services/citySearchService'

const props = defineProps({
  modelValue: {
    type: Object,
    default: () => ({ destination: '', countryCode: '' })
  }
})

const emit = defineEmits(['update:modelValue'])

const searchTerm = ref('')
const searchResults = ref([])
const showSuggestions = ref(false)
const isLoading = ref(false)
const highlightedIndex = ref(-1)

// 디바운스 타이머
let debounceTimer = null

onMounted(() => {
  // 기존 값이 있으면 표시
  if (props.modelValue?.destination) {
    searchTerm.value = props.modelValue.destination
  }
})

// modelValue 변경 감지
watch(() => props.modelValue, (newVal) => {
  if (newVal?.destination && newVal.destination !== searchTerm.value) {
    searchTerm.value = newVal.destination
  }
}, { deep: true })

// 검색 실행 (디바운스)
function performSearch() {
  if (!searchTerm.value || searchTerm.value.length < 1) {
    searchResults.value = []
    return
  }

  isLoading.value = true

  // 디바운스: 150ms 후 검색
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    try {
      searchResults.value = searchCities(searchTerm.value, 8)
    } catch (error) {
      console.error('City search error:', error)
      searchResults.value = []
    } finally {
      isLoading.value = false
    }
  }, 150)
}

function onInput() {
  showSuggestions.value = true
  highlightedIndex.value = -1
  performSearch()

  // 입력 중에는 countryCode를 null로
  emit('update:modelValue', { destination: searchTerm.value, countryCode: null })
}

function onFocus() {
  if (searchTerm.value) {
    showSuggestions.value = true
    performSearch()
  }
}

function onBlur() {
  // 모바일 터치를 위해 딜레이
  setTimeout(() => {
    showSuggestions.value = false
    highlightedIndex.value = -1
  }, 200)
}

function selectResult(result) {
  searchTerm.value = result.displayName
  showSuggestions.value = false
  highlightedIndex.value = -1

  emit('update:modelValue', {
    destination: result.displayName,
    countryCode: result.countryCode,
    // 추가 정보도 함께 전달
    cityName: result.name,
    cityNameEn: result.name_en,
    country: result.country,
    countryEn: result.country_en,
    latitude: result.latitude,
    longitude: result.longitude,
    timezone: result.timezone,
    currency: result.currency,
    isDomestic: result.isDomestic,
  })
}

function onArrowDown() {
  if (!showSuggestions.value || searchResults.value.length === 0) return
  highlightedIndex.value = Math.min(highlightedIndex.value + 1, searchResults.value.length - 1)
}

function onArrowUp() {
  if (!showSuggestions.value || searchResults.value.length === 0) return
  highlightedIndex.value = Math.max(highlightedIndex.value - 1, 0)
}

function onEnter() {
  if (highlightedIndex.value >= 0 && searchResults.value[highlightedIndex.value]) {
    selectResult(searchResults.value[highlightedIndex.value])
  } else if (searchResults.value.length > 0) {
    selectResult(searchResults.value[0])
  }
}

function closeSuggestions() {
  showSuggestions.value = false
  highlightedIndex.value = -1
}

// 국가 코드로 국기 이모지 생성
function getCountryFlag(countryCode) {
  if (!countryCode) return '🌍'
  const codePoints = countryCode
    .toUpperCase()
    .split('')
    .map(char => 127397 + char.charCodeAt(0))
  return String.fromCodePoint(...codePoints)
}
</script>

<style scoped>
/* 드롭다운 애니메이션 */
.dropdown-enter-active,
.dropdown-leave-active {
  transition: all 0.2s ease;
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

/* 스크롤바 스타일 */
ul::-webkit-scrollbar {
  width: 6px;
}

ul::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 10px;
}

ul::-webkit-scrollbar-thumb {
  background: #ccc;
  border-radius: 10px;
}

ul::-webkit-scrollbar-thumb:hover {
  background: #aaa;
}

ul {
  scrollbar-width: thin;
  scrollbar-color: #ccc #f1f1f1;
}
</style>
