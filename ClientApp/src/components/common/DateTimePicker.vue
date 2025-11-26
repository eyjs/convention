<template>
  <DatePicker
    :value="modelValue"
    @update:value="handleUpdate"
    type="datetime"
    format="YYYY-MM-DD HH:mm"
    value-type="YYYY-MM-DDTHH:mm:ss"
    placeholder="날짜와 시간을 선택하세요"
    :show-second="false"
    lang="ko"
    confirm
    append-to-body
    :popup-style="{ zIndex: 9999 }"
    class="w-full"
  />
</template>

<script setup>
import DatePicker from 'vue-datepicker-next'
import 'vue-datepicker-next/index.css'

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['update:modelValue'])

function handleUpdate(value) {
  emit('update:modelValue', value)
}
</script>
