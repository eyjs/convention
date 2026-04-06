import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useUIStore = defineStore('ui', () => {
  // 글로벌 로딩 프로그레스바
  const activeRequests = ref(0)
  const isLoading = ref(false)
  let loadingTimer = null

  function startLoading() {
    activeRequests.value++
    if (!isLoading.value) {
      // 200ms 지연 후 표시 (짧은 요청은 프로그레스바 안 보임)
      loadingTimer = setTimeout(() => {
        if (activeRequests.value > 0) isLoading.value = true
      }, 200)
    }
  }

  function stopLoading() {
    activeRequests.value = Math.max(0, activeRequests.value - 1)
    if (activeRequests.value === 0) {
      clearTimeout(loadingTimer)
      isLoading.value = false
    }
  }

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
    isLoading,
    startLoading,
    stopLoading,
    modalStack,
    registerModal,
    unregisterModal,
    closeTopModal,
    hasOpenModal,
  }
})
