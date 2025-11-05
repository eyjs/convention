import apiClient from './api'

/**
 * 참가자 관련 API 서비스
 */
export const guestAPI = {
  /**
   * 특정 컨벤션의 참가자 목록 조회
   * @param {Object} params - { conventionId, search }
   * @returns {Promise<Object>}
   */
  getParticipants: (params = {}) => {
    return apiClient.get('/guest/participants', { params })
  },
}

export default guestAPI
