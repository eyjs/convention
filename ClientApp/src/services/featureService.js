import apiClient from './api'

class FeatureService {
  async getFeaturesByConvention(conventionId) {
    try {
      const response = await apiClient.get(
        `/conventions/${conventionId}/features`,
      )
      return response
    } catch (error) {
      console.error('Error fetching features:', error)
      throw error
    }
  }

  async getFeatureById(featureId) {
    try {
      const response = await apiClient.get(`/features/${featureId}`)
      return response
    } catch (error) {
      console.error('Error fetching feature details:', error)
      throw error
    }
  }

  async updateFeatureStatus(featureId, isActive) {
    try {
      const response = await apiClient.patch(`/features/${featureId}/status`, {
        isActive,
      })
      return response
    } catch (error) {
      console.error('Error updating feature status:', error)
      throw error
    }
  }

  async createFeature(featureData) {
    try {
      const response = await apiClient.post(`/features`, featureData)
      return response
    } catch (error) {
      console.error('Error creating feature:', error)
      throw error
    }
  }

  async updateFeature(featureId, featureData) {
    try {
      const response = await apiClient.put(
        `/features/${featureId}`,
        featureData,
      )
      return response
    } catch (error) {
      console.error('Error updating feature:', error)
      throw error
    }
  }

  async deleteFeature(featureId) {
    try {
      const response = await apiClient.delete(`/features/${featureId}`)
      return response
    } catch (error) {
      console.error('Error deleting feature:', error)
      throw error
    }
  }
}

export default new FeatureService()
