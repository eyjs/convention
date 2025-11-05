import axios from 'axios'

class FeatureService {
  constructor() {
    this.baseURL = import.meta.env.VITE_API_BASE_URL || '/api'
  }

  async getFeaturesByConvention(conventionId) {
    try {
      const response = await axios.get(
        `${this.baseURL}/conventions/${conventionId}/features`,
      )
      return response
    } catch (error) {
      console.error('Error fetching features:', error)
      throw error
    }
  }

  async getFeatureById(featureId) {
    try {
      const response = await axios.get(`${this.baseURL}/features/${featureId}`)
      return response
    } catch (error) {
      console.error('Error fetching feature details:', error)
      throw error
    }
  }

  async updateFeatureStatus(featureId, isActive) {
    try {
      const response = await axios.patch(
        `${this.baseURL}/features/${featureId}/status`,
        { isActive },
      )
      return response
    } catch (error) {
      console.error('Error updating feature status:', error)
      throw error
    }
  }

  async createFeature(featureData) {
    try {
      const response = await axios.post(`${this.baseURL}/features`, featureData)
      return response
    } catch (error) {
      console.error('Error creating feature:', error)
      throw error
    }
  }

  async updateFeature(featureId, featureData) {
    try {
      const response = await axios.put(
        `${this.baseURL}/features/${featureId}`,
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
      const response = await axios.delete(
        `${this.baseURL}/features/${featureId}`,
      )
      return response
    } catch (error) {
      console.error('Error deleting feature:', error)
      throw error
    }
  }
}

export default new FeatureService()
