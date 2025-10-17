import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useUIStore = defineStore('ui', () => {
  const isChatOpen = ref(false);

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
    isChatOpen,
    toggleChat,
    openChat,
    closeChat,
  };
});
