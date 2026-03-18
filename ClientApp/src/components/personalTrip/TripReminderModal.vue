<template>
  <SlideUpModal
    :is-open="isOpen"
    z-index-class="z-[60]"
    @close="$emit('close')"
  >
    <template #header-title>알림 및 리마인더</template>
    <template #body>
      <div v-if="upcomingReminders.length > 0" class="space-y-4">
        <div
          v-for="reminder in upcomingReminders"
          :key="reminder.id"
          class="flex items-start gap-3 p-3 bg-blue-50 border border-blue-200 rounded-lg"
        >
          <div
            class="flex-shrink-0 w-10 h-10 bg-blue-500 text-white rounded-full flex items-center justify-center text-sm font-bold"
          >
            D-{{ reminder.daysUntil }}
          </div>
          <div class="flex-1">
            <p class="font-medium text-gray-900">{{ reminder.title }}</p>
            <p class="text-sm text-gray-600">{{ reminder.description }}</p>
            <p class="text-xs text-blue-600 mt-1">{{ reminder.dateText }}</p>
          </div>
        </div>
      </div>
      <div v-else class="text-center py-8 text-gray-500">
        <p>현재 활성화된 알림이나 리마인더가 없습니다.</p>
      </div>
    </template>
    <template #footer>
      <button
        type="button"
        class="w-full py-3 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors"
        @click="$emit('close')"
      >
        닫기
      </button>
    </template>
  </SlideUpModal>
</template>

<script setup>
import SlideUpModal from '@/components/common/SlideUpModal.vue'

defineProps({
  isOpen: { type: Boolean, required: true },
  upcomingReminders: { type: Array, default: () => [] },
})

defineEmits(['close'])
</script>
