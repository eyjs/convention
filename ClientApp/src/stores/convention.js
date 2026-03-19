import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { conventionAPI } from '@/services/api'
import dayjs from 'dayjs'

export const useConventionStore = defineStore('convention', () => {
  const conventions = ref([])
  const currentConvention = ref(null)
  const schedules = ref([])
  const notices = ref([])
  const tourInfo = ref([])
  const photos = ref([])
  const loading = ref(false)
  const error = ref(null)
  const selectedDate = ref(dayjs().format('YYYY-MM-DD'))
  const activeTab = ref('나의일정')

  const getCurrentConvention = computed(() => currentConvention.value)

  const getSchedulesByDate = computed(() => {
    if (!schedules.value.length) return []
    return schedules.value
      .filter(
        (s) =>
          dayjs(s.scheduleDate).format('YYYY-MM-DD') === selectedDate.value,
      )
      .sort((a, b) =>
        (a.startTime || '00:00:00').localeCompare(b.startTime || '00:00:00'),
      )
  })

  const getAvailableDates = computed(() => {
    if (!schedules.value.length) return []
    const dates = [
      ...new Set(
        schedules.value.map((s) => dayjs(s.scheduleDate).format('YYYY-MM-DD')),
      ),
    ].sort()
    return dates.map((date) => ({
      date,
      label: dayjs(date).format('M/D'),
      dayName: dayjs(date).format('(ddd)'),
      isToday: dayjs(date).isSame(dayjs(), 'day'),
    }))
  })

  async function fetchConventions() {
    loading.value = true
    error.value = null
    try {
      const response = await conventionAPI.getConventions()
      conventions.value = response.data?.conventions || []
    } catch (err) {
      error.value = '컨벤션 데이터를 불러오는데 실패했습니다.'
      console.error('Failed to fetch conventions:', err)
    } finally {
      loading.value = false
    }
  }

  async function selectConvention(conventionId) {
    const id =
      typeof conventionId === 'number'
        ? conventionId
        : parseInt(conventionId, 10)
    if (isNaN(id)) {
      console.error('Invalid convention ID:', conventionId)
      return
    }

    loading.value = true
    error.value = null
    try {
      const response = await conventionAPI.getConvention(id)
      currentConvention.value = response.data

      const availableDates = getAvailableDates.value
      if (availableDates.length > 0) {
        selectedDate.value = availableDates[0].date
      }
    } catch (err) {
      error.value = '컨벤션 상세 정보를 불러오는데 실패했습니다.'
      console.error('Failed to fetch convention details:', err)
    } finally {
      loading.value = false
    }
  }

  function clearConvention() {
    currentConvention.value = null
    schedules.value = []
    notices.value = []
    tourInfo.value = []
    photos.value = []
  }

  function setSelectedDate(date) {
    selectedDate.value = date
  }

  function setActiveTab(tab) {
    activeTab.value = tab
  }

  return {
    conventions,
    currentConvention,
    schedules,
    notices,
    tourInfo,
    photos,
    loading,
    error,
    selectedDate,
    activeTab,
    getCurrentConvention,
    getSchedulesByDate,
    getAvailableDates,
    fetchConventions,
    selectConvention,
    clearConvention,
    setSelectedDate,
    setActiveTab,
  }
})
