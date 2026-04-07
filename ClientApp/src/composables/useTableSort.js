import { ref, computed } from 'vue'

/**
 * 테이블 클라이언트 사이드 정렬 composable
 *
 * 사용 예:
 * const { sortKey, sortDir, sortedData, toggleSort } = useTableSort(items, 'name')
 * <th @click="toggleSort('name')">이름 <SortIcon :dir="sortKey === 'name' ? sortDir : null" /></th>
 * <tr v-for="row in sortedData">...</tr>
 */
export function useTableSort(items, defaultKey = null, defaultDir = 'asc') {
  const sortKey = ref(defaultKey)
  const sortDir = ref(defaultDir) // 'asc' | 'desc' | null

  function toggleSort(key) {
    if (sortKey.value !== key) {
      sortKey.value = key
      sortDir.value = 'asc'
      return
    }
    // 같은 컬럼 클릭 시 asc → desc → 해제 3단계
    if (sortDir.value === 'asc') {
      sortDir.value = 'desc'
    } else if (sortDir.value === 'desc') {
      sortKey.value = null
      sortDir.value = 'asc'
    } else {
      sortDir.value = 'asc'
    }
  }

  function getValue(obj, path) {
    if (!path) return obj
    return path.split('.').reduce((o, k) => (o ? o[k] : undefined), obj)
  }

  function compare(a, b) {
    if (a == null && b == null) return 0
    if (a == null) return 1
    if (b == null) return -1

    // 숫자
    if (typeof a === 'number' && typeof b === 'number') return a - b

    // 날짜 문자열 (ISO 형식 또는 Date 객체)
    const aDate = new Date(a)
    const bDate = new Date(b)
    if (!isNaN(aDate) && !isNaN(bDate) && String(a).match(/\d{4}/)) {
      return aDate - bDate
    }

    // 문자열
    return String(a).localeCompare(String(b), 'ko')
  }

  const sortedData = computed(() => {
    const source = typeof items === 'function' ? items() : items.value || items
    if (!Array.isArray(source)) return []
    if (!sortKey.value) return source

    const sorted = [...source].sort((a, b) => {
      const valA = getValue(a, sortKey.value)
      const valB = getValue(b, sortKey.value)
      const result = compare(valA, valB)
      return sortDir.value === 'desc' ? -result : result
    })
    return sorted
  })

  return {
    sortKey,
    sortDir,
    sortedData,
    toggleSort,
  }
}
