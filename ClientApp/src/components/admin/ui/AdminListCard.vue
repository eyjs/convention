<template>
  <div class="bg-white rounded-lg shadow-sm overflow-hidden">
    <!-- 카드 본문 (클릭 가능) -->
    <div
      class="p-3 active:bg-gray-50"
      :class="clickable ? 'cursor-pointer' : ''"
      @click="clickable ? $emit('click') : null"
    >
      <slot></slot>
    </div>
    <!-- 하단 액션 버튼 영역 -->
    <div v-if="$slots.actions" class="flex border-t divide-x">
      <slot name="actions"></slot>
    </div>
  </div>
</template>

<script setup>
defineProps({
  clickable: { type: Boolean, default: true },
})

defineEmits(['click'])
</script>
