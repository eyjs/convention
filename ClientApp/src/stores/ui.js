import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useUIStore = defineStore('ui', () => {
  const isModalOpen = ref(false);
  const openModalCount = ref(0);
  const isChatOpen = ref(false);

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

  function toggleChat() {
    isChatOpen.value = !isChatOpen.value;
  }

  function openChat() {
    isChatOpen.value = true;
  }

  function closeChat() {
    isChatOpen.value = false;
  }

  return {
    isModalOpen,
    openModal,
    closeModal,
    isChatOpen,
    toggleChat,
    openChat,
    closeChat
  };
});