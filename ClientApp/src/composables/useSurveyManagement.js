import { ref, computed, onMounted, watch } from 'vue'
import api from '@/services/api'
import { formatDate } from '@/utils/date'
import { useToast } from '@/composables/useToast'

/**
 * 설문 관리 공통 로직 (일반 설문 / 옵션투어 설문 공용)
 * @param {() => number} getConventionId - conventionId getter
 * @param {string} surveyType - 'GENERAL' | 'OPTION_TOUR'
 */
export function useSurveyManagement(getConventionId, surveyType) {
  const surveys = ref([])
  const loading = ref(true)
  const error = ref(null)

  const currentView = ref('list')
  const selectedSurveyId = ref(null)

  const searchQuery = ref('')
  const statusFilter = ref('all')

  const { toastMessage, toastType, showToast } = useToast()

  const isDeleteModalVisible = ref(false)
  const deletingSurveyId = ref(null)
  const deletingTitle = ref('')
  const isDeleting = ref(false)

  function getSurveyStatus(survey) {
    if (!survey.isActive) return 'inactive'
    if (survey.endDate && new Date(survey.endDate) < new Date())
      return 'expired'
    return 'active'
  }

  const filteredSurveys = computed(() => {
    return surveys.value.filter((s) => {
      if (searchQuery.value) {
        const q = searchQuery.value.toLowerCase()
        if (!s.title.toLowerCase().includes(q)) return false
      }
      if (statusFilter.value !== 'all') {
        if (getSurveyStatus(s) !== statusFilter.value) return false
      }
      return true
    })
  })

  const activeSurveyCount = computed(
    () => surveys.value.filter((s) => getSurveyStatus(s) === 'active').length,
  )

  const totalResponses = computed(() =>
    surveys.value.reduce((sum, s) => sum + (s.responseCount ?? 0), 0),
  )

  function getSurveyStatusVariant(survey) {
    const status = getSurveyStatus(survey)
    if (status === 'inactive') return 'danger'
    if (status === 'expired') return 'warning'
    return 'success'
  }

  function getSurveyStatusLabel(survey) {
    const status = getSurveyStatus(survey)
    if (status === 'inactive') return '비활성'
    if (status === 'expired') return '기간만료'
    return '활성'
  }

  function formatDateRange(survey) {
    if (!survey.startDate && !survey.endDate) return '-'
    const fmt = (d) => formatDate(d)
    if (survey.startDate && survey.endDate)
      return `${fmt(survey.startDate)} ~ ${fmt(survey.endDate)}`
    if (survey.startDate) return `${fmt(survey.startDate)} ~`
    return `~ ${fmt(survey.endDate)}`
  }

  async function fetchSurveys() {
    loading.value = true
    error.value = null
    try {
      const response = await api.get(`/surveys/admin/${getConventionId()}`, {
        params: { type: surveyType },
      })
      surveys.value = response.data
    } catch {
      error.value = '설문 목록을 불러오는데 실패했습니다.'
    } finally {
      loading.value = false
    }
  }

  function showCreateView() {
    selectedSurveyId.value = null
    currentView.value = 'create'
  }

  function showEditView(id) {
    selectedSurveyId.value = id
    currentView.value = 'edit'
  }

  function showStatsView(id) {
    selectedSurveyId.value = id
    currentView.value = 'stats'
  }

  function goBackToList() {
    currentView.value = 'list'
    selectedSurveyId.value = null
    fetchSurveys()
  }

  function handleSurveySaved() {
    showToast('설문이 저장되었습니다.')
    goBackToList()
  }

  function confirmDeleteSurvey(survey) {
    deletingSurveyId.value = survey.id
    deletingTitle.value = survey.title
    isDeleteModalVisible.value = true
  }

  async function deleteSurvey() {
    isDeleting.value = true
    try {
      await api.delete(`/surveys/${deletingSurveyId.value}`)
      isDeleteModalVisible.value = false
      showToast('설문이 삭제되었습니다.')
      fetchSurveys()
    } catch {
      showToast('설문 삭제에 실패했습니다.', 'error')
    } finally {
      isDeleting.value = false
    }
  }

  onMounted(() => fetchSurveys())

  watch(getConventionId, () => fetchSurveys())

  return {
    surveys,
    loading,
    error,
    currentView,
    selectedSurveyId,
    searchQuery,
    statusFilter,
    toastMessage,
    toastType,
    showToast,
    isDeleteModalVisible,
    deletingSurveyId,
    deletingTitle,
    isDeleting,
    filteredSurveys,
    activeSurveyCount,
    totalResponses,
    getSurveyStatus,
    getSurveyStatusVariant,
    getSurveyStatusLabel,
    formatDateRange,
    fetchSurveys,
    showCreateView,
    showEditView,
    showStatsView,
    goBackToList,
    handleSurveySaved,
    confirmDeleteSurvey,
    deleteSurvey,
  }
}
