<template>
  <div class="relative">
    <input
      type="text"
      v-model="searchTerm"
      @input="onInput"
      @focus="showSuggestions = true"
      @blur="onBlur"
      placeholder="도시, 국가명 검색"
      class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
    />
    <ul v-if="showSuggestions && filteredResults.length" class="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-scroll">
      <li
        v-for="result in filteredResults"
        :key="result.name"
        @mousedown.prevent="selectResult(result)"
        class="px-4 py-2 cursor-pointer hover:bg-gray-100"
      >
        {{ result.name }}
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'

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

onMounted(async () => {
  try {
    const response = await fetch('/src/assets/countries.json')
    countries.value = await response.json()
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
  )
})

function onInput() {
  showSuggestions.value = true
  // When user types, we don't know the country code yet
  emit('update:modelValue', { destination: searchTerm.value, countryCode: null })
}

function selectResult(result) {
  searchTerm.value = result.name
  showSuggestions.value = false
  emit('update:modelValue', { destination: result.name, countryCode: result.countryCode })
}

function onBlur() {
  // Use a timeout to allow click event on suggestions to fire
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
