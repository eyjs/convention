<template>
  <SlideUpModal :is-open="isOpen" @close="close">
    <template #header-title>숙소 관리</template>
    <template #body>
      <div class="space-y-4">
        <div v-if="!accommodations || accommodations.length === 0" class="text-center py-10">
          <p class="text-gray-500">등록된 숙소가 없습니다.</p>
        </div>
        <div v-else class="space-y-4">
          <AccommodationCard
            v-for="acc in accommodations"
            :key="acc.id"
            :accommodation="acc"
            :show-actions="true"
            :show-time-remaining="true"
            @select="onSelect"
            @delete="onDelete"
          />
        </div>
      </div>
    </template>
    <template #footer>
      <div class="flex flex-col gap-3 w-full">
        <button @click="onAdd" class="w-full py-3 bg-primary-500 text-white rounded-xl font-semibold hover:shadow-lg active:scale-95 transition-all">
          + 새 숙소 추가
        </button>
        <button type="button" @click="close" class="w-full py-3 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">
          닫기
        </button>
      </div>
    </template>
  </SlideUpModal>
</template>

<script setup>
import SlideUpModal from '@/components/common/SlideUpModal.vue';
import AccommodationCard from './AccommodationCard.vue';

defineProps({
  isOpen: Boolean,
  accommodations: {
    type: Array,
    default: () => [],
  },
});

const emit = defineEmits(['close', 'add', 'edit', 'delete', 'select']);

const close = () => emit('close');
const onAdd = () => emit('add');
const onDelete = (accommodationId) => emit('delete', accommodationId);
const onSelect = (accommodation) => emit('select', accommodation);

</script>
