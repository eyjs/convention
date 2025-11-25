/**
 * 숫자 포맷팅 directive
 * focus 전: 콤마 포맷 (1,000,000)
 * focus 후: 순수 숫자 (1000000)
 */

function formatNumber(value) {
  if (!value && value !== 0) return ''
  const num = String(value).replace(/,/g, '')
  if (isNaN(num)) return value
  return Number(num).toLocaleString('ko-KR')
}

function unformatNumber(value) {
  if (!value) return ''
  return String(value).replace(/,/g, '')
}

export default {
  mounted(el, binding) {
    const input = el.tagName === 'INPUT' ? el : el.querySelector('input')
    if (!input) return

    // 초기값 포맷팅
    if (input.value) {
      input.value = formatNumber(input.value)
    }

    // focus 이벤트: 콤마 제거
    input.addEventListener('focus', () => {
      input.value = unformatNumber(input.value)
    })

    // blur 이벤트: 콤마 추가
    input.addEventListener('blur', () => {
      const unformatted = unformatNumber(input.value)
      if (unformatted) {
        input.value = formatNumber(unformatted)
        // v-model 업데이트를 위해 input 이벤트 발생
        input.dispatchEvent(new Event('input', { bubbles: true }))
      }
    })

    // input 이벤트: 숫자만 입력 가능하도록
    input.addEventListener('input', (e) => {
      // focus 상태에서만 숫자만 허용
      if (document.activeElement === input) {
        const cursorPos = input.selectionStart
        const cleaned = input.value.replace(/[^\d]/g, '')
        if (input.value !== cleaned) {
          input.value = cleaned
          input.setSelectionRange(cursorPos - 1, cursorPos - 1)
        }
      }
    })
  },

  updated(el, binding) {
    const input = el.tagName === 'INPUT' ? el : el.querySelector('input')
    if (!input || document.activeElement === input) return

    // focus되지 않은 상태에서만 포맷팅
    if (input.value) {
      const unformatted = unformatNumber(input.value)
      const formatted = formatNumber(unformatted)
      if (input.value !== formatted) {
        input.value = formatted
      }
    }
  }
}
