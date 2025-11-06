<template>
  <Transition name="modal-fade">
    <div
      v-if="isOpen"
      class="fixed inset-0 bg-black/60 backdrop-blur-sm flex justify-center items-center z-50 p-4"
      @mousedown.self="onMouseDown"
      @mouseup.self="onMouseUp"
    >
      <div
        class="bg-white rounded-2xl shadow-2xl w-full max-h-[90vh] flex flex-col"
        :class="maxWidthClass"
      >
        <!-- Header -->
        <header v-if="$slots.header" class="px-6 py-4 border-b flex items-center justify-between">
          <slot name="header"></slot>
          <button @click="close" class="p-2 hover:bg-gray-100 rounded-lg">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </header>

        <!-- Body -->
        <main class="p-6 overflow-y-auto">
          <slot name="body"></slot>
        </main>

        <!-- Footer -->
        <footer v-if="$slots.footer" class="px-6 py-4 border-t flex justify-end gap-3">
          <slot name="footer"></slot>
        </footer>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, computed } from 'vue';

const props = defineProps({
  isOpen: Boolean,
  maxWidth: {
    type: String,
    default: '2xl',
  },
});

const emit = defineEmits(['close']);

const startPos = ref({ x: 0, y: 0 });

const onMouseDown = (e) => {
  startPos.value = { x: e.clientX, y: e.clientY };
};

const onMouseUp = (e) => {
  const dx = Math.abs(e.clientX - startPos.value.x);
  const dy = Math.abs(e.clientY - startPos.value.y);
  if (dx < 5 && dy < 5) {
    close();
  }
};

const close = () => {
  emit('close');
};

const maxWidthClass = computed(() => {
  return {
    'sm': 'max-w-sm',
    'md': 'max-w-md',
    'lg': 'max-w-lg',
    'xl': 'max-w-xl',
    '2xl': 'max-w-2xl',
    '3xl': 'max-w-3xl',
    '4xl': 'max-w-4xl',
    '5xl': 'max-w-5xl',
  }[props.maxWidth];
});
</script>

<style scoped>
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.25s ease-in-out;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}
</style>
