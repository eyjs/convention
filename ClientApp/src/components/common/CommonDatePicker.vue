<template>
  <!-- 모바일: 네이티브 HTML5 Input (터치 디바이스 감지) -->
  <input
    v-if="isMobile"
    :type="nativeInputType"
    :value="nativeValue"
    @input="onNativeInput"
    :placeholder="placeholder"
    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white"
    :class="{ 'text-gray-400': !value }"
  />

  <!-- 데스크톱: vue-datepicker-next -->
  <date-picker
    v-else
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
import { ref, computed, onMounted } from 'vue'
import DatePicker from 'vue-datepicker-next'
import 'vue-datepicker-next/index.css'
import dayjs from 'dayjs'

const props = defineProps({
  value: {
    type: [String, Date, Array],
    default: '',
  },
  type: {
    type: String,
    default: 'date', // 'date', 'datetime'
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
})

const emit = defineEmits(['update:value'])

// 모바일 감지
const isMobile = ref(false)

onMounted(() => {
  // 터치 디바이스 또는 화면 너비로 모바일 판별
  isMobile.value =
    ('ontouchstart' in window) ||
    (navigator.maxTouchPoints > 0) ||
    window.innerWidth < 768
})

// 네이티브 input type 결정
const nativeInputType = computed(() => {
  if (props.type === 'datetime') return 'datetime-local'
  if (props.type === 'date') return 'date'
  return 'datetime-local'
})

// 네이티브 input용 값 변환
const nativeValue = computed(() => {
  if (!props.value) return ''

  try {
    // YYYY-MM-DD HH:mm:ss -> YYYY-MM-DDTHH:mm 형식으로 변환
    if (props.type === 'datetime') {
      return dayjs(props.value).format('YYYY-MM-DDTHH:mm')
    }
    // YYYY-MM-DD 형식 그대로
    return dayjs(props.value).format('YYYY-MM-DD')
  } catch (e) {
    console.warn('Date conversion error:', e)
    return ''
  }
})

// 네이티브 input 변경 처리
function onNativeInput(event) {
  const inputValue = event.target.value
  if (!inputValue) {
    emit('update:value', null)
    return
  }

  try {
    // datetime-local: 2024-12-25T14:30 -> YYYY-MM-DD HH:mm:ss
    if (props.type === 'datetime') {
      const formatted = dayjs(inputValue).format(props.valueType || 'YYYY-MM-DD HH:mm:ss')
      emit('update:value', formatted)
    } else {
      // date: 2024-12-25 -> YYYY-MM-DD
      const formatted = dayjs(inputValue).format(props.valueType || 'YYYY-MM-DD')
      emit('update:value', formatted)
    }
  } catch (e) {
    console.error('Date parsing error:', e)
  }
}

// 데스크톱 picker 변경 처리
function onDatePickerUpdate(newValue) {
  emit('update:value', newValue)
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

/* 네이티브 input placeholder 스타일 */
input[type="date"]::-webkit-calendar-picker-indicator,
input[type="datetime-local"]::-webkit-calendar-picker-indicator {
  cursor: pointer;
  filter: opacity(0.6);
}

input[type="date"]:hover::-webkit-calendar-picker-indicator,
input[type="datetime-local"]:hover::-webkit-calendar-picker-indicator {
  filter: opacity(1);
}
</style>

<style>
/* append-to-body로 body에 직접 추가된 DatePicker 팝업 */
body > .mx-datepicker-main.mx-datepicker-popup {
  z-index: 99999 !important;
}
</style>
