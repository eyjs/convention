import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import featureService from '@/services/featureService'

export const useFeatureStore = defineStore('feature', () => {
  const activeFeatures = ref([])
  const loading = ref(false)
  const error = ref(null)

  const getFeatureByUrl = computed(() => {
    return (menuUrl) => {
      return activeFeatures.value.find((f) => f.menuUrl === menuUrl)
    }
  })

  const enabledFeatures = computed(() => {
    return activeFeatures.value.filter((f) => f.isActive)
  })

  const featureCount = computed(() => activeFeatures.value.length)

  async function fetchActiveFeatures(conventionId) {
    if (!conventionId) {
      console.warn('Convention ID is required to fetch features')
      return
    }

    loading.value = true
    error.value = null

    try {
      const response =
        await featureService.getFeaturesByConvention(conventionId)
      activeFeatures.value = response.data?.features || response.data || []
      console.log(
        `Loaded ${activeFeatures.value.length} features for convention ${conventionId}`,
      )
    } catch (err) {
      error.value = '기능 목록을 불러오는데 실패했습니다.'
      console.error('Failed to fetch features:', err)
      activeFeatures.value = []
    } finally {
      loading.value = false
    }
  }

  function resetFeatures() {
    activeFeatures.value = []
    error.value = null
    loading.value = false
  }

  async function toggleFeatureStatus(featureId) {
    const feature = activeFeatures.value.find((f) => f.id === featureId)
    if (!feature) return

    try {
      const response = await featureService.updateFeatureStatus(
        featureId,
        !feature.isActive,
      )
      feature.isActive = response.data.isActive
    } catch (err) {
      console.error('Failed to toggle feature status:', err)
      throw err
    }
  }

  return {
    activeFeatures,
    loading,
    error,
    getFeatureByUrl,
    enabledFeatures,
    featureCount,
    fetchActiveFeatures,
    resetFeatures,
    toggleFeatureStatus,
  }
})
