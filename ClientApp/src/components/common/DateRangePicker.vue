<template>
  <div>
    <label v-if="label" class="block text-sm font-medium text-gray-700 mb-1">{{ label }}</label>
    <VDatePicker v-model="dateRange" is-range :masks="masks" :popover="{ visibility: 'click', positionFixed: true, zIndex: 9999 }" teleport>
      <template #default="{ inputValue, togglePopover }">
        <div class="relative w-full" @click="togglePopover">
          <input
            :value="displayValue(inputValue)"
            :placeholder="placeholder"
            class="w-full input pr-10 cursor-pointer"
            readonly
          />
          <div class="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
            <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
        </div>
      </template>
    </VDatePicker>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { DatePicker as VDatePicker } from 'v-calendar'

const props = defineProps({
  modelValue: {
    type: Object,
    default: () => ({ start: null, end: null }),
  },
  label: {
    type: String,
    default: '',
  },
  placeholder: {
    type: String,
    default: '날짜 범위를 선택하세요',
  }
})

const emit = defineEmits(['update:modelValue'])

const dateRange = ref(props.modelValue)

const masks = ref({
  input: 'YYYY-MM-DD',
})

const displayValue = (inputValue) => {
  const start = inputValue.start !== 'null' ? inputValue.start : null
  const end = inputValue.end !== 'null' ? inputValue.end : null

  if (start && end) {
    return `${start} - ${end}`
  }
  return ''
}

watch(dateRange, (newRange) => {
  emit('update:modelValue', newRange)
})

watch(() => props.modelValue, (newModelValue) => {
  dateRange.value = newModelValue
})
</script>
