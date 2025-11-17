<template>
  <div class="relative">
    <input
      type="text"
      v-model="searchTerm"
      @input="onInput"
      @focus="onFocus"
      @blur="onBlur"
      @keydown.enter.prevent="onEnter"
      placeholder="도시, 국가명 검색"
      class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
    />
    <ul v-if="showSuggestions && filteredResults.length" class="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
      <li
        v-for="result in filteredResults"
        :key="result.name"
        @touchstart.prevent="selectResult(result)"
        @mousedown.prevent="selectResult(result)"
        class="px-4 py-3 cursor-pointer hover:bg-gray-100 active:bg-gray-200"
      >
        {{ result.name }}
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import countriesData from '@/assets/countries.json'

const props = defineProps({
  modelValue: {
    type: Object,
    default: () => ({ destination: '', countryCode: '' })
  }
})

const emit = defineEmits(['update:modelValue'])

const searchTerm = ref('')
const countries = ref([])
const showSuggestions = ref(false)

onMounted(() => {
  try {
    countries.value = countriesData
    // Initialize search term if a value is provided
    if (props.modelValue?.destination) {
      searchTerm.value = props.modelValue.destination
    }
  } catch (error) {
    console.error('Failed to load countries.json:', error)
  }
})

const filteredResults = computed(() => {
  if (!searchTerm.value) {
    return []
  }
  return countries.value.filter(country =>
    country.name.toLowerCase().includes(searchTerm.value.toLowerCase())
  ).slice(0, 10) // 최대 10개만 표시
})

function onInput() {
  showSuggestions.value = true
  // When user types, we don't know the country code yet
  emit('update:modelValue', { destination: searchTerm.value, countryCode: null })
}

function onFocus() {
  // 입력값이 있으면 드롭다운 표시
  if (searchTerm.value && filteredResults.value.length > 0) {
    showSuggestions.value = true
  }
}

function selectResult(result) {
  searchTerm.value = result.name
  showSuggestions.value = false
  emit('update:modelValue', { destination: result.name, countryCode: result.countryCode })
}

function onEnter() {
  // 엔터키를 눌렀을 때 첫 번째 추천 항목 선택
  if (filteredResults.value.length > 0) {
    selectResult(filteredResults.value[0])
  }
  // form submit 이벤트는 이미 prevent로 막혀있음
}

function onBlur() {
  // 모바일 터치를 위해 더 긴 딜레이 사용
  setTimeout(() => {
    showSuggestions.value = false
  }, 300)
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
