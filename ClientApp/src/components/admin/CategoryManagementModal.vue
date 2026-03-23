<template>
  <BaseModal :show="true" @close="$emit('close')">
    <template #header>
      <h2 class="text-lg font-semibold">카테고리 관리</h2>
    </template>
    <p class="text-gray-500 text-sm py-8 text-center">
      카테고리 관리 기능은 준비 중입니다.
    </p>
    <template #footer>
      <button
        class="px-4 py-2 bg-white text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50"
        @click="$emit('close')"
      >
        닫기
      </button>
    </template>
  </BaseModal>
</template>

<script setup>
import BaseModal from '@/components/common/BaseModal.vue'

defineEmits(['close', 'categories-updated'])
</script>
