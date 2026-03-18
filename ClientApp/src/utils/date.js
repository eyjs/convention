import dayjs from 'dayjs'

/**
 * YYYY-MM-DD
 * @param {string|Date} date
 * @returns {string}
 */
export function formatDate(date) {
  if (!date) return '-'
  return dayjs(date).format('YYYY-MM-DD')
}

/**
 * YYYY-MM-DD HH:mm
 * @param {string|Date} date
 * @returns {string}
 */
export function formatDateTime(date) {
  if (!date) return '-'
  return dayjs(date).format('YYYY-MM-DD HH:mm')
}

/**
 * HH:mm
 * @param {string|Date} date
 * @returns {string}
 */
export function formatTime(date) {
  if (!date) return '--:--'
  return dayjs(date).format('HH:mm')
}

/**
 * YYYY-MM-DD (요일)
 * @param {string|Date} date
 * @returns {string}
 */
export function formatDateWithDay(date) {
  if (!date) return '-'
  const days = ['일', '월', '화', '수', '목', '금', '토']
  const d = dayjs(date)
  return `${d.format('YYYY-MM-DD')} (${days[d.day()]})`
}

/**
 * datetime-local input용: YYYY-MM-DDTHH:mm
 * @param {string|Date} date
 * @returns {string}
 */
export function formatDateTimeForInput(date) {
  if (!date) return ''
  return dayjs(date).format('YYYY-MM-DDTHH:mm')
}

/**
 * 상대 시간 (방금 전, N시간 전, 어제, M/D)
 * @param {string|Date} date
 * @returns {string}
 */
export function formatRelativeDate(date) {
  if (!date) return '-'
  const d = dayjs(date)
  const now = dayjs()
  const diffHours = now.diff(d, 'hour')

  if (diffHours < 1) return '방금 전'
  if (diffHours < 24) return `${diffHours}시간 전`
  if (diffHours < 48) return '어제'
  return d.format('M/D')
}

/**
 * 8자리 문자열 (YYYYMMDD) → YYYY-MM-DD
 * @param {string} dateStr
 * @returns {string}
 */
export function formatDateString8(dateStr) {
  if (!dateStr || dateStr.length !== 8) return dateStr || '-'
  return `${dateStr.substring(0, 4)}-${dateStr.substring(4, 6)}-${dateStr.substring(6, 8)}`
}
