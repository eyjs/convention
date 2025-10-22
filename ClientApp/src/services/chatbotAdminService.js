import apiClient from './api'

// 챗봇 관리 API
export const chatbotAdminAPI = {
  // 시스템 상태
  getStatus: () => apiClient.get('/admin/chatbot/stats'),
  
  // 모델 관리 (Ollama 연동 시 별도 구현 필요)
  getModels: () => {
    // TODO: Ollama API 연동
    return Promise.resolve({
      data: {
        models: [],
        currentModel: ''
      }
    })
  },
  
  selectModel: (modelName) => {
    // TODO: Ollama API 연동
    return Promise.resolve({ data: { success: true } })
  },

  // 벡터 통계
  getVectorStats: () => apiClient.get('/admin/chatbot/vector-stats'),
  
  // 행사 목록
  getConventions: () => apiClient.get('/admin/chatbot/conventions'),
  
  // 재색인
  reindexAll: () => apiClient.post('/admin/chatbot/reindex-all'),
  reindexConvention: (conventionId) => apiClient.post(`/admin/chatbot/reindex-convention/${conventionId}`),
  
  // 챗봇 토글
  toggleChatbot: (conventionId, enabled) => 
    apiClient.put(`/admin/chatbot/convention/${conventionId}/toggle`, { enabled }),
  
  // 벡터 DB 초기화
  clearVectors: () => apiClient.delete('/admin/chatbot/clear-vectors'),
  
  // 최근 활동
  getRecentActivities: () => apiClient.get('/admin/chatbot/recent-activities'),
  
  // 로그
  getLogs: () => apiClient.get('/admin/chatbot/logs')
}

// API 키 관리 API (향후 구현 필요)
export const apiKeyAPI = {
  getAll: () => {
    // TODO: 백엔드에 API 키 관리 엔드포인트 추가 필요
    return Promise.resolve({ data: [] })
  },
  
  create: (data) => {
    // TODO: 백엔드 구현
    return Promise.resolve({ data: { id: Date.now(), ...data } })
  },
  
  update: (id, data) => {
    // TODO: 백엔드 구현
    return Promise.resolve({ data: { success: true } })
  },
  
  delete: (id) => {
    // TODO: 백엔드 구현
    return Promise.resolve({ data: { success: true } })
  },
  
  toggle: (id, isActive) => {
    // TODO: 백엔드 구현
    return Promise.resolve({ data: { success: true } })
  }
}

export default {
  ...chatbotAdminAPI,
  apiKeys: apiKeyAPI
}
