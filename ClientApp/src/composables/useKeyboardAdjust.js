import { onMounted, onUnmounted, ref } from 'vue'

/**
 * 모바일 키보드 대응 컴포저블
 * 키보드가 올라올 때 입력 필드가 가려지지 않도록 자동으로 스크롤 조정
 */
export function useKeyboardAdjust(options = {}) {
  const {
    offset = 20, // 키보드 위로 올릴 추가 여백 (px)
    duration = 300, // 애니메이션 지속 시간 (ms)
    enabled = true, // 기능 활성화 여부
  } = options

  const isKeyboardVisible = ref(false)
  const activeElement = ref(null)
  const originalViewportHeight = ref(0)
  const scrollPosition = ref(0)

  let resizeObserver = null
  let focusTimeoutId = null

  /**
   * 입력 필드를 화면에 보이도록 스크롤
   */
  const scrollToElement = (element, animated = true) => {
    if (!element) return

    const rect = element.getBoundingClientRect()
    const viewportHeight = window.visualViewport?.height || window.innerHeight

    // 요소가 화면 아래쪽에 가려져 있는지 확인
    const elementBottom = rect.bottom + offset
    const isHidden = elementBottom > viewportHeight

    if (isHidden) {
      const scrollAmount = elementBottom - viewportHeight + window.scrollY

      if (animated) {
        window.scrollTo({
          top: scrollAmount,
          behavior: 'smooth',
        })
      } else {
        window.scrollTo(0, scrollAmount)
      }
    }
  }

  /**
   * Visual Viewport API를 사용한 키보드 감지
   */
  const handleVisualViewportResize = () => {
    if (!window.visualViewport) return

    const currentHeight = window.visualViewport.height

    // 초기 높이 저장
    if (originalViewportHeight.value === 0) {
      originalViewportHeight.value = window.innerHeight
    }

    // 키보드 표시 감지 (viewport 높이가 크게 줄어듦)
    const heightDifference = originalViewportHeight.value - currentHeight
    const isKeyboardShown = heightDifference > 150 // 150px 이상 차이나면 키보드로 간주

    if (isKeyboardShown !== isKeyboardVisible.value) {
      isKeyboardVisible.value = isKeyboardShown

      if (isKeyboardShown && activeElement.value) {
        // 키보드가 올라올 때 약간의 딜레이 후 스크롤
        setTimeout(() => {
          scrollToElement(activeElement.value, true)
        }, 100)
      }
    }
  }

  /**
   * 입력 필드 포커스 핸들러
   */
  const handleFocus = (event) => {
    if (!enabled) return

    const target = event.target

    // input, textarea, contenteditable 요소만 처리
    if (
      target.tagName === 'INPUT' ||
      target.tagName === 'TEXTAREA' ||
      target.contentEditable === 'true'
    ) {
      activeElement.value = target
      scrollPosition.value = window.scrollY

      // iOS에서는 포커스 후 키보드가 올라오는데 시간이 걸림
      clearTimeout(focusTimeoutId)
      focusTimeoutId = setTimeout(() => {
        scrollToElement(target, false)
      }, 300)
    }
  }

  /**
   * 입력 필드 블러 핸들러
   */
  const handleBlur = () => {
    clearTimeout(focusTimeoutId)
    activeElement.value = null
  }

  /**
   * 윈도우 리사이즈 핸들러 (fallback)
   */
  const handleWindowResize = () => {
    if (!window.visualViewport) {
      handleVisualViewportResize()
    }
  }

  /**
   * body 클래스 토글 (CSS로 추가 제어 가능)
   */
  const updateBodyClass = () => {
    if (isKeyboardVisible.value) {
      document.body.classList.add('keyboard-visible')
    } else {
      document.body.classList.remove('keyboard-visible')
    }
  }

  onMounted(() => {
    if (!enabled) return

    // Visual Viewport API 사용 (iOS Safari, Chrome 등)
    if (window.visualViewport) {
      window.visualViewport.addEventListener(
        'resize',
        handleVisualViewportResize,
      )
      window.visualViewport.addEventListener(
        'scroll',
        handleVisualViewportResize,
      )
    } else {
      // Fallback for older browsers
      window.addEventListener('resize', handleWindowResize)
    }

    // 포커스 이벤트 리스너 (캡처 단계에서 처리)
    document.addEventListener('focusin', handleFocus, true)
    document.addEventListener('focusout', handleBlur, true)

    // ResizeObserver로 DOM 변경 감지
    if (typeof ResizeObserver !== 'undefined') {
      resizeObserver = new ResizeObserver(() => {
        if (activeElement.value) {
          scrollToElement(activeElement.value, false)
        }
      })
      resizeObserver.observe(document.body)
    }

    // body 클래스 업데이트
    updateBodyClass()
  })

  onUnmounted(() => {
    if (window.visualViewport) {
      window.visualViewport.removeEventListener(
        'resize',
        handleVisualViewportResize,
      )
      window.visualViewport.removeEventListener(
        'scroll',
        handleVisualViewportResize,
      )
    } else {
      window.removeEventListener('resize', handleWindowResize)
    }

    document.removeEventListener('focusin', handleFocus, true)
    document.removeEventListener('focusout', handleBlur, true)

    if (resizeObserver) {
      resizeObserver.disconnect()
    }

    clearTimeout(focusTimeoutId)

    // Clean up body class
    document.body.classList.remove('keyboard-visible')
  })

  return {
    isKeyboardVisible,
    activeElement,
    scrollToElement,
  }
}
