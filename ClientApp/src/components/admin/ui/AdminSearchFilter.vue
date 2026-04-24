<template>
  <div class="mt-6 bg-white rounded-lg shadow-sm p-4">
    <div class="flex flex-col sm:flex-row gap-3">
      <!-- 셀렉트 필터들 (왼쪽) -->
      <slot name="filters"></slot>

      <!-- 검색 input (오른쪽, 남은 공간 채움) -->
      <input
        :value="modelValue"
        type="text"
        :placeholder="placeholder"
        class="flex-1 px-3 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
        @input="$emit('update:modelValue', $event.target.value)"
      />
    </div>
    <div
      v-if="resultCount !== null || showReset"
      class="flex items-center justify-between mt-2 text-sm text-gray-500"
    >
      <span v-if="resultCount !== null">검색 결과: {{ resultCount }}건</span>
      <span v-else></span>
      <button
        v-if="showReset"
        class="text-primary-600 hover:underline text-xs"
        @click="$emit('reset')"
      >
        필터 초기화
      </button>
    </div>
  </div>
</template>

<script setup>
defineProps({
  modelValue: { type: String, default: '' },
  placeholder: { type: String, default: '이름, 전화번호 검색...' },
  resultCount: { type: Number, default: null },
  showReset: { type: Boolean, default: false },
})

defineEmits(['update:modelValue', 'reset'])
</script>
