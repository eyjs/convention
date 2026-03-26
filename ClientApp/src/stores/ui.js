import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useUIStore = defineStore('ui', () => {
  // 모달 스택: { id: symbol, close: function }[]
  const modalStack = ref([])

  function registerModal(id, closeCallback) {
    modalStack.value.push({ id, close: closeCallback })
  }

  function unregisterModal(id) {
    const index = modalStack.value.findIndex((m) => m.id === id)
    if (index !== -1) {
      modalStack.value.splice(index, 1)
    }
  }

  function closeTopModal() {
    if (modalStack.value.length > 0) {
      const topModal = modalStack.value[modalStack.value.length - 1]
      topModal.close()
      return true
    }
    return false
  }

  function hasOpenModal() {
    return modalStack.value.length > 0
  }

  return {
    modalStack,
    registerModal,
    unregisterModal,
    closeTopModal,
    hasOpenModal,
  }
})
