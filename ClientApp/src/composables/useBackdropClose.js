import { ref } from 'vue'

/**
 * 모달 backdrop 닫힘 처리 composable.
 *
 * PC에서 모달 내부 input 드래그 → 바깥 릴리즈 시 의도치 않게 닫히는 문제를 방지한다.
 * - mousedown이 backdrop 본인에서 시작되지 않았으면 무시
 * - 이동거리 5px 이내만 닫힘으로 처리
 *
 * @param {() => void} onClose
 */
export const BACKDROP_CLOSE_THRESHOLD_PX = 5

export function useBackdropClose(onClose) {
  const startPos = ref(null)
  const THRESHOLD = BACKDROP_CLOSE_THRESHOLD_PX

  const onBackdropMouseDown = (e) => {
    if (e.target !== e.currentTarget) {
      startPos.value = null
      return
    }
    startPos.value = { x: e.clientX, y: e.clientY }
  }

  const onBackdropMouseUp = (e) => {
    if (!startPos.value) return
    if (e.target !== e.currentTarget) {
      startPos.value = null
      return
    }
    const dx = Math.abs(e.clientX - startPos.value.x)
    const dy = Math.abs(e.clientY - startPos.value.y)
    startPos.value = null
    if (dx < THRESHOLD && dy < THRESHOLD) {
      onClose()
    }
  }

  const onBackdropTouchStart = (e) => {
    if (e.target !== e.currentTarget) {
      startPos.value = null
      return
    }
    const touch = e.touches[0]
    startPos.value = { x: touch.clientX, y: touch.clientY }
  }

  const onBackdropTouchEnd = (e) => {
    if (!startPos.value) return
    if (e.target !== e.currentTarget) {
      startPos.value = null
      return
    }
    const touch = e.changedTouches[0]
    const dx = Math.abs(touch.clientX - startPos.value.x)
    const dy = Math.abs(touch.clientY - startPos.value.y)
    startPos.value = null
    if (dx < THRESHOLD && dy < THRESHOLD) {
      onClose()
    }
  }

  return {
    onBackdropMouseDown,
    onBackdropMouseUp,
    onBackdropTouchStart,
    onBackdropTouchEnd,
  }
}
