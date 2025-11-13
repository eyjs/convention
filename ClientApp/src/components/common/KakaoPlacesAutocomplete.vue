<template>
  <div class="relative">
    <input
      type="text"
      ref="autocompleteInput"
      v-model="searchTerm"
      @input="onInput"
      @focus="showSuggestions = true"
      @blur="onBlur"
      :placeholder="placeholder"
      class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
    />
    <ul v-if="showSuggestions && filteredResults.length" class="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-scroll">
      <li
        v-for="result in filteredResults"
        :key="result.id"
        @mousedown.prevent="selectResult(result)"
        class="px-4 py-2 cursor-pointer hover:bg-gray-100"
      >
        {{ result.place_name }} ({{ result.address_name }})
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import axios from 'axios'

const props = defineProps({
  modelValue: {
    type: Object,
    default: () => ({ name: '', address: '', latitude: null, longitude: null, kakaoPlaceId: null })
  },
  placeholder: {
    type: String,
    default: '장소 검색 (카카오맵)'
  }
})

const emit = defineEmits(['update:modelValue'])

const searchTerm = ref(props.modelValue.name || '')
const filteredResults = ref([])
const showSuggestions = ref(false)

let searchTimeout = null

watch(() => props.modelValue.name, (newName) => {
  if (newName !== searchTerm.value) {
    searchTerm.value = newName
  }
})

async function onInput() {
  showSuggestions.value = true
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(async () => {
    if (searchTerm.value.length < 2) {
      filteredResults.value = []
      return
    }
    try {
      const response = await axios.get('https://dapi.kakao.com/v2/local/search/keyword.json', {
        params: { query: searchTerm.value },
        headers: { Authorization: `KakaoAK ${import.meta.env.VITE_KAKAO_MAP_API_KEY}` }
      })
      filteredResults.value = response.data.documents
    } catch (error) {
      console.error('Kakao Places API search failed:', error)
      filteredResults.value = []
    }
  }, 300) // Debounce search
  emit('update:modelValue', { ...props.modelValue, name: searchTerm.value })
}

function selectResult(result) {
  searchTerm.value = result.place_name
  showSuggestions.value = false
  const newPlaceData = {
    name: result.place_name,
    address: result.address_name,
    latitude: parseFloat(result.y),
    longitude: parseFloat(result.x),
    kakaoPlaceId: result.id
  }
  emit('update:modelValue', newPlaceData)
}

function onBlur() {
  setTimeout(() => {
    showSuggestions.value = false
  }, 150)
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
