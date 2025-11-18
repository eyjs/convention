<template>
  <SlideUpModal :is-open="isOpen" @close="close">
    <template #header-title>숙소 관리</template>
    <template #body>
      <div class="space-y-4">
        <div v-if="!accommodations || accommodations.length === 0" class="text-center py-10">
          <p class="text-gray-500">등록된 숙소가 없습니다.</p>
        </div>
        <div v-else class="space-y-3">
          <div v-for="acc in accommodations" :key="acc.id" class="border rounded-lg p-3 flex justify-between items-center">
            <div @click="onSelect(acc)" class="flex-1 min-w-0 cursor-pointer">
              <p class="font-semibold text-gray-800 truncate">{{ acc.name }}</p>
              <p class="text-sm text-gray-500 truncate">{{ acc.address }}</p>
              <p class="text-xs text-gray-500 mt-1">{{ formatDateTime(acc.checkInTime) }} ~ {{ formatDateTime(acc.checkOutTime) }}</p>
            </div>
            <div class="flex-shrink-0 flex gap-2 ml-4">
              <button @click="onEdit(acc)" class="p-2 text-blue-600 hover:bg-blue-50 rounded-full">
                <PencilIcon class="w-5 h-5" />
              </button>
              <button @click="onDelete(acc.id)" class="p-2 text-red-600 hover:bg-red-50 rounded-full">
                <Trash2Icon class="w-5 h-5" />
              </button>
            </div>
          </div>
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
import { PencilIcon, Trash2Icon } from 'lucide-vue-next';
import dayjs from 'dayjs';

const props = defineProps({
  isOpen: Boolean,
  accommodations: {
    type: Array,
    default: () => [],
  },
});

const emit = defineEmits(['close', 'add', 'edit', 'delete', 'select']);

const close = () => emit('close');
const onAdd = () => emit('add');
const onEdit = (accommodation) => emit('edit', accommodation);
const onDelete = (accommodationId) => emit('delete', accommodationId);
const onSelect = (accommodation) => emit('select', accommodation);

const formatDateTime = (dateTime) => {
  if (!dateTime) return '';
  return dayjs(dateTime).format('YYYY-MM-DD');
};
</script>
