import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useUIStore = defineStore('ui', () => {
  const isModalOpen = ref(false);
  const openModalCount = ref(0);
  const isChatOpen = ref(false);

  // 모달 스택: { id: string, close: function }[]
  const modalStack = ref([]);

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

  // 모달을 스택에 등록 (id와 close 콜백)
  function registerModal(id, closeCallback) {
    modalStack.value.push({ id, close: closeCallback });
  }

  // 모달을 스택에서 제거
  function unregisterModal(id) {
    const index = modalStack.value.findIndex(m => m.id === id);
    if (index !== -1) {
      modalStack.value.splice(index, 1);
    }
  }

  // 뒤로가기 시 가장 최근 모달 닫기
  function closeTopModal() {
    if (modalStack.value.length > 0) {
      const topModal = modalStack.value[modalStack.value.length - 1];
      topModal.close();
      return true; // 모달을 닫았음
    }
    return false; // 닫을 모달이 없음
  }

  // 현재 열린 모달이 있는지 확인
  function hasOpenModal() {
    return modalStack.value.length > 0;
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
    modalStack,
    registerModal,
    unregisterModal,
    closeTopModal,
    hasOpenModal,
    isChatOpen,
    toggleChat,
    openChat,
    closeChat
  };
});