<template>
  <date-picker
    :value="value"
    @update:value="onDatePickerUpdate"
    :type="type"
    :format="format"
    :value-type="valueType"
    :placeholder="placeholder"
    :show-second="false"
    lang="ko"
    confirm
    append-to-body
    :editable="editable"
    class="common-datepicker w-full"
    input-class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
    :popup-style="{ zIndex: 99999 }"
  ></date-picker>
</template>

<script setup>
import DatePicker from 'vue-datepicker-next';
import 'vue-datepicker-next/index.css';

const props = defineProps({
  value: {
    type: [String, Date, Array],
    default: '',
  },
  type: {
    type: String,
    default: 'date',
  },
  format: {
    type: String,
    default: 'YYYY-MM-DD',
  },
  valueType: {
    type: String,
    default: 'YYYY-MM-DD',
  },
  placeholder: {
    type: String,
    default: '날짜 선택',
  },
  editable: {
    type: Boolean,
    default: true,
  },
});

const emit = defineEmits(['update:value']);

function onDatePickerUpdate(newValue) {
  emit('update:value', newValue);
}
</script>

<style>
/* 전역 스타일로 DatePicker의 모양을 커스텀합니다. */
.common-datepicker .mx-input {
  height: auto;
  font-size: 1rem;
  line-height: 1.5rem;
  box-shadow: none !important;
}

/* DatePicker 팝업이 모달 위에 표시되도록 z-index 보장 */
:deep(.mx-datepicker-popup) {
  z-index: 99999 !important;
}
</style>

<style>
/* append-to-body로 body에 직접 추가된 DatePicker 팝업 */
body > .mx-datepicker-main.mx-datepicker-popup {
  z-index: 99999 !important;
}
</style>