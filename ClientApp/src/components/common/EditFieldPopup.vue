<template>
  <SlideUpModal :is-open="isOpen" @close="$emit('close')">
    <template #header-title>{{ title }}</template>
    <template #body>
      <form id="edit-field-form" @submit.prevent="handleSave" class="space-y-2">
        <label class="block text-sm font-medium text-gray-700">{{ label }}</label>
        <input 
          v-if="type !== 'date'"
          ref="inputRef"
          v-model="internalValue" 
          :type="type" 
          class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" 
        />
        <CommonDatePicker
          v-else
          v-model:value="internalValue"
          class="w-full"
        />
      </form>
    </template>
    <template #footer>
      <div class="flex gap-3 w-full">
        <button type="button" @click="$emit('close')" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 transition-colors">취소</button>
        <button type="submit" form="edit-field-form" class="flex-1 py-3 px-4 bg-blue-600 text-white rounded-xl font-semibold hover:bg-blue-700">저장</button>
      </div>
    </template>
  </SlideUpModal>
</template>

<script setup>
import { ref, watch, nextTick } from 'vue';
import SlideUpModal from './SlideUpModal.vue';
import CommonDatePicker from './CommonDatePicker.vue';

const props = defineProps({
  isOpen: Boolean,
  title: String,
  label: String,
  value: [String, Number],
  type: {
    type: String,
    default: 'text',
  },
  fieldKey: {
    type: String,
    required: true,
  }
});

const emit = defineEmits(['close', 'save']);

const internalValue = ref('');
const inputRef = ref(null);

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    internalValue.value = props.value;
    nextTick(() => {
      inputRef.value?.focus();
    });
  }
});

function handleSave() {
  emit('save', { key: props.fieldKey, value: internalValue.value });
}
</script>
