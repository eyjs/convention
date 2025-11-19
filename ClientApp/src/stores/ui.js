import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useUIStore = defineStore('ui', () => {
  const isModalOpen = ref(false);
  const openModalCount = ref(0);

  function openModal() {
    openModalCount.value++;
    isModalOpen.value = true;
  }

  function closeModal() {
    openModalCount.value--;
    if (openModalCount.value <= 0) {
      isModalOpen.value = false;
      openModalCount.value = 0; // Ensure it doesn't go negative
    }
  }

  return { isModalOpen, openModal, closeModal };
});