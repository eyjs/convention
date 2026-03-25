import { ref, onUnmounted } from 'vue'

export function useToast(duration = 3000) {
  const toastMessage = ref('')
  const toastType = ref('success')
  let timer = null

  function showToast(message, type = 'success') {
    toastMessage.value = message
    toastType.value = type
    if (timer) clearTimeout(timer)
    timer = setTimeout(() => {
      toastMessage.value = ''
    }, duration)
  }

  onUnmounted(() => {
    if (timer) clearTimeout(timer)
  })

  return { toastMessage, toastType, showToast }
}
