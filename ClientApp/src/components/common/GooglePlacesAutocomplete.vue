<template>
  <input
    type="text"
    ref="autocompleteInput"
    :value="modelValue.name"
    @input="onInput"
    :placeholder="placeholder"
    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
  />
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useGoogleMaps } from '@/composables/useGoogleMaps'

const props = defineProps({
  modelValue: {
    type: Object,
    default: () => ({ name: '', address: '', latitude: null, longitude: null, googlePlaceId: null })
  },
  placeholder: {
    type: String,
    default: '장소 검색'
  },
  isItinerary: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue'])

const autocompleteInput = ref(null)
const { isLoaded, loadScript } = useGoogleMaps()
let autocomplete = null

onMounted(() => {
  if (!isLoaded.value) {
    loadScript() // Ensure script is loaded
  }
})

watch(isLoaded, (loaded) => {
  if (loaded) {
    initAutocomplete()
  }
})

watch(() => props.modelValue.name, (newName) => {
  // Update input value if modelValue.name changes externally
  if (autocompleteInput.value && autocompleteInput.value.value !== newName) {
    autocompleteInput.value.value = newName
  }
})

function initAutocomplete() {
  if (!window.google || !autocompleteInput.value) return

  autocomplete = new google.maps.places.Autocomplete(autocompleteInput.value)
  autocomplete.addListener('place_changed', () => {
    const place = autocomplete.getPlace()
    const newPlaceData = {
      name: place.name || '',
      address: place.formatted_address || '',
      latitude: place.geometry?.location?.lat() || null,
      longitude: place.geometry?.location?.lng() || null,
      googlePlaceId: place.place_id || null
    }
    emit('update:modelValue', newPlaceData)
  })
}

function onInput(event) {
  // Emit current input value to keep v-model in sync for non-autocomplete changes
  emit('update:modelValue', { ...props.modelValue, name: event.target.value })
}
</script>
