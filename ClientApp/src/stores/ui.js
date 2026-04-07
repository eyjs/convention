import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useUIStore = defineStore('ui', () => {
  // 글로벌 로딩 프로그레스바
  const activeRequests = ref(0)
  const isLoading = ref(false)
  let loadingStartAt = 0
  const MIN_DISPLAY_MS = 300 // 최소 노출 시간 (깜빡임 방지)

  function startLoading() {
    activeRequests.value++
    if (!isLoading.value) {
      isLoading.value = true
      loadingStartAt = Date.now()
    }
  }

  function stopLoading() {
    activeRequests.value = Math.max(0, activeRequests.value - 1)
    if (activeRequests.value === 0) {
      // 너무 빨리 사라지지 않도록 최소 표시 시간 보장
      const elapsed = Date.now() - loadingStartAt
      const remaining = Math.max(0, MIN_DISPLAY_MS - elapsed)
      setTimeout(() => {
        if (activeRequests.value === 0) isLoading.value = false
      }, remaining)
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
