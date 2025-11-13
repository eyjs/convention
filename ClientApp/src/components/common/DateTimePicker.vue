<template>
  <div class="flex items-center space-x-2">
    <input
      type="date"
      :value="datePart"
      @input="updateDate"
      class="w-full px-3 py-2 border border-gray-300 rounded-lg"
    />
    <select :value="hourPart" @change="updateTime" class="px-3 py-2 border border-gray-300 rounded-lg">
      <option v-for="h in 24" :key="h-1" :value="String(h-1).padStart(2, '0')">{{ String(h-1).padStart(2, '0') }}</option>
    </select>
    <span class="font-bold">:</span>
    <select :value="minutePart" @change="updateTime" class="px-3 py-2 border border-gray-300 rounded-lg">
      <option value="00">00</option>
      <option value="30">30</option>
    </select>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['update:modelValue'])

const datePart = computed(() => props.modelValue ? props.modelValue.split('T')[0] : '')
const timePart = computed(() => props.modelValue ? props.modelValue.split('T')[1] : '00:00')

const hourPart = computed(() => timePart.value ? timePart.value.split(':')[0] : '00')
const minutePart = computed(() => timePart.value ? timePart.value.split(':')[1] : '00')

function updateDate(event) {
  const newDate = event.target.value
  const time = timePart.value || '00:00'
  emit('update:modelValue', `${newDate}T${time}`)
}

function updateTime(event) {
  const date = datePart.value || new Date().toISOString().split('T')[0]
  let newHour = hourPart.value
  let newMinute = minutePart.value

  if (event.target.tagName === 'SELECT') {
    const selectElement = event.target;
    if (selectElement.previousElementSibling && selectElement.previousElementSibling.tagName === 'SPAN') {
      // Minute dropdown
      newMinute = selectElement.value;
    } else {
      // Hour dropdown
      newHour = selectElement.value;
    }
  }
  
  emit('update:modelValue', `${date}T${newHour}:${newMinute}`)
}
</script>