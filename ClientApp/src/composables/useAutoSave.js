import { ref, watch } from 'vue'

/**
 * 자동 저장 composable
 * @param {Function} saveFn - async () => void
 * @param {object} options - { delay: ms }
 */
export function useAutoSave(saveFn, options = {}) {
  const { delay = 5000 } = options
  const saving = ref(false)
  const lastSavedAt = ref(null)
  const error = ref(null)
  let timer = null

  function trigger() {
    if (timer) clearTimeout(timer)
    timer = setTimeout(async () => {
      saving.value = true
      error.value = null
      try {
        await saveFn()
        lastSavedAt.value = new Date()
      } catch (e) {
        error.value = e
        console.error('Auto save failed:', e)
      } finally {
        saving.value = false
      }
    }, delay)
  }

  function watchSource(source) {
    watch(source, () => trigger(), { deep: true })
  }

  async function saveNow() {
    if (timer) clearTimeout(timer)
    saving.value = true
    error.value = null
    try {
      await saveFn()
      lastSavedAt.value = new Date()
    } catch (e) {
      error.value = e
    } finally {
      saving.value = false
    }
  }

  return { saving, lastSavedAt, error, trigger, watchSource, saveNow }
}
