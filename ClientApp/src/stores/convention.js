import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { conventionAPI } from '@/services/api'
import dayjs from 'dayjs'

export const useConventionStore = defineStore('convention', () => {
  // State
  const conventions = ref([])
  const currentConvention = ref(null)
  const schedules = ref([])
  const notices = ref([])
  const tourInfo = ref([])
  const photos = ref([])
  const loading = ref(false)
  const error = ref(null)

  // 현재 선택된 날짜
  const selectedDate = ref(dayjs().format('YYYY-MM-DD'))
  
  // 현재 활성 탭
  const activeTab = ref('나의일정')

  // Getters
  const getCurrentConvention = computed(() => currentConvention.value)
  
  const getSchedulesByDate = computed(() => {
    if (!schedules.value.length) return []
    
    return schedules.value.filter(schedule => {
      const scheduleDate = dayjs(schedule.scheduleDate).format('YYYY-MM-DD')
      return scheduleDate === selectedDate.value
    }).sort((a, b) => {
      const timeA = a.startTime || '00:00:00'
      const timeB = b.startTime || '00:00:00'
      return timeA.localeCompare(timeB)
    })
  })

  const getAvailableDates = computed(() => {
    if (!schedules.value.length) return []
    
    const dates = [...new Set(schedules.value.map(s => 
      dayjs(s.scheduleDate).format('YYYY-MM-DD')
    ))].sort()
    
    return dates.map(date => ({
      date,
      label: dayjs(date).format('M/D'),
      dayName: dayjs(date).format('(ddd)'),
      isToday: dayjs(date).isSame(dayjs(), 'day')
    }))
  })

  // Actions
  async function fetchConventions() {
    loading.value = true
    error.value = null
    
    try {
      const response = await conventionAPI.getConventions()
      conventions.value = response.data?.conventions || []
      
      // 첫 번째 컨벤션을 현재 컨벤션으로 설정
      if (conventions.value.length > 0) {
        await setCurrentConvention(conventions.value[0].id)
      }
    } catch (err) {
      error.value = '컨벤션 데이터를 불러오는데 실패했습니다.'
      console.error('Failed to fetch conventions:', err)
    } finally {
      loading.value = false
    }
  }

  async function setCurrentConvention(conventionId) {
    loading.value = true
    error.value = null
    
    try {
      const response = await conventionAPI.getConvention(conventionId)
      currentConvention.value = response.data?.convention
      schedules.value = response.data?.schedules || []
      
      // 첫 번째 사용 가능한 날짜로 설정
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

  async function fetchNotices(conventionId) {
    loading.value = true
    try {
      const response = await conventionAPI.getConvention(conventionId)
      notices.value = response.data?.notices || []
    } catch (err) {
      console.error('Failed to fetch notices:', err)
      notices.value = []
    } finally {
      loading.value = false
    }
  }

  async function fetchTourInfo(conventionId) {
    loading.value = true
    try {
      const response = await conventionAPI.getConvention(conventionId)
      tourInfo.value = response.data?.tours || []
    } catch (err) {
      console.error('Failed to fetch tour info:', err)
      tourInfo.value = []
    } finally {
      loading.value = false
    }
  }

  async function fetchPhotos(conventionId) {
    loading.value = true
    try {
      const response = await conventionAPI.getConvention(conventionId)
      photos.value = response.data?.photos || []
    } catch (err) {
      console.error('Failed to fetch photos:', err)
      photos.value = []
    } finally {
      loading.value = false
    }
  }

  function setSelectedDate(date) {
    selectedDate.value = date
  }

  function setActiveTab(tab) {
    activeTab.value = tab
  }

  return {
    // State
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
    
    // Getters
    getCurrentConvention,
    getSchedulesByDate,
    getAvailableDates,
    
    // Actions
    fetchConventions,
    setCurrentConvention,
    fetchNotices,
    fetchTourInfo,
    fetchPhotos,
    setSelectedDate,
    setActiveTab
  }
})
