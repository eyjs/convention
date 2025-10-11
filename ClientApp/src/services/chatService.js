import apiClient from './api'

export const chatService = {
  async ask(question, conventionId = null, userContext = null) {
    try {
      const response = await apiClient.post('/conventionchat/ask', {
        question,
        conventionId,
        ...userContext
      })
      return response
    } catch (error) {
      console.error('Failed to ask question:', error)
      throw error
    }
  },

  async askAboutConvention(conventionId, question, userContext = null) {
    try {
      const response = await apiClient.post(
        `/conventionchat/conventions/${conventionId}/ask`,
        { 
          question,
          ...userContext
        }
      )
      return response
    } catch (error) {
      console.error(`Failed to ask about convention ${conventionId}:`, error)
      throw error
    }
  },

  async askWithHistory(question, history = [], conventionId = null, userContext = null) {
    try {
      const response = await apiClient.post('/conventionchat/ask/with-history', {
        question,
        history,
        conventionId,
        ...userContext
      })
      return response
    } catch (error) {
      console.error('Failed to ask with history:', error)
      throw error
    }
  },

  async getSuggestedQuestions(conventionId, userContext = null) {
    try {
      const params = new URLSearchParams()
      if (userContext?.role) params.append('role', userContext.role)
      if (userContext?.guestId) params.append('guestId', userContext.guestId)
      if (userContext?.companionId) params.append('companionId', userContext.companionId)
      
      const queryString = params.toString()
      const url = `/conventionchat/conventions/${conventionId}/suggestions${queryString ? '?' + queryString : ''}`
      
      const response = await apiClient.get(url)
      return response
    } catch (error) {
      console.error(`Failed to get suggestions for convention ${conventionId}:`, error)
      return []
    }
  },

  async reindexAll() {
    try {
      const response = await apiClient.post('/conventionchat/reindex')
      return response
    } catch (error) {
      console.error('Failed to reindex all:', error)
      throw error
    }
  },

  async indexConvention(conventionId) {
    try {
      const response = await apiClient.post(
        `/conventionchat/conventions/${conventionId}/index`
      )
      return response
    } catch (error) {
      console.error(`Failed to index convention ${conventionId}:`, error)
      throw error
    }
  }
}
