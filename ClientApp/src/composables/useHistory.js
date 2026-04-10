import { ref, computed } from 'vue'

/**
 * Undo/Redo 히스토리 composable
 * @param {number} maxSize - 최대 히스토리 크기
 */
export function useHistory(maxSize = 50) {
  const stack = ref([])
  const index = ref(-1)

  const canUndo = computed(() => index.value > 0)
  const canRedo = computed(() => index.value < stack.value.length - 1)

  function push(state) {
    const snapshot = JSON.stringify(state)
    // 현재 위치 이후 redo 스택 클리어
    stack.value = stack.value.slice(0, index.value + 1)
    stack.value.push(snapshot)
    // 최대 크기 초과 시 오래된 것 제거
    if (stack.value.length > maxSize) {
      stack.value.shift()
    } else {
      index.value++
    }
  }

  function undo() {
    if (!canUndo.value) return null
    index.value--
    return JSON.parse(stack.value[index.value])
  }

  function redo() {
    if (!canRedo.value) return null
    index.value++
    return JSON.parse(stack.value[index.value])
  }

  function clear() {
    stack.value = []
    index.value = -1
  }

  return { push, undo, redo, clear, canUndo, canRedo, index, stackSize: computed(() => stack.value.length) }
}
