import apiClient from './api'

/**
 * 공지사항 API 서비스
 */
export const noticeAPI = {
  /**
   * 공지사항 목록 조회
   * @param {Object} params - 쿼리 파라미터 { conventionId, page, pageSize, searchType, searchKeyword }
   * @returns {Promise<Object>}
   */
  getNotices: (params = {}) => {
    return apiClient.get('/notices', { params })
  },

  /**
   * 공지사항 상세 조회
   * @param {number} id - 공지사항 ID
   * @returns {Promise<Object>}
   */
  getNotice: (id) => {
    return apiClient.get(`/notices/${id}`)
  },

  /**
   * 공지사항 생성 (관리자)
   * @param {number} conventionId - 컨벤션 ID
   * @param {Object} data - 공지사항 데이터
   * @returns {Promise<Object>}
   */
  createNotice: (conventionId, data) => {
    return apiClient.post(`/notices?conventionId=${conventionId}`, data)
  },

  /**
   * 공지사항 수정 (관리자)
   * @param {number} id - 공지사항 ID
   * @param {Object} data - 수정할 데이터
   * @returns {Promise<Object>}
   */
  updateNotice: (id, data) => {
    return apiClient.put(`/notices/${id}`, data)
  },

  /**
   * 공지사항 삭제 (관리자)
   * @param {number} id - 공지사항 ID
   * @returns {Promise<void>}
   */
  deleteNotice: (id) => {
    return apiClient.delete(`/notices/${id}`)
  },

  /**
   * 공지사항 조회수 증가
   * @param {number} id - 공지사항 ID
   * @returns {Promise<void>}
   */
  incrementViewCount: (id) => {
    return apiClient.post(`/notices/${id}/view`)
  },

  /**
   * 공지사항 고정 토글 (관리자)
   * @param {number} id - 공지사항 ID
   * @returns {Promise<Object>}
   */
  togglePin: (id) => {
    return apiClient.post(`/notices/${id}/toggle-pin`)
  }
}

export default noticeAPI
