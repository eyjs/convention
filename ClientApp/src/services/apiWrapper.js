import apiClient from './api'
import { mockAPI, isDevelopmentMode } from './mockApi'

/**
 * API 호출 래퍼 - Mock 모드일 때 자동으로 Mock 데이터 반환
 * 
 * 사용법:
 * const response = await apiWrapper.get('/admin/conventions/1/stats', () => mockAPI.getConventionStats(1))
 */
export const apiWrapper = {
  /**
   * GET 요청
   * @param {string} url - API 엔드포인트
   * @param {Function} mockFn - Mock 데이터 반환 함수
   */
  async get(url, mockFn) {
    if (isDevelopmentMode() && mockFn) {
      // 네트워크 지연 시뮬레이션
      await new Promise(resolve => setTimeout(resolve, 300))
      return { data: mockFn() }
    }
    return apiClient.get(url)
  },

  /**
   * POST 요청
   * @param {string} url - API 엔드포인트
   * @param {any} data - 요청 데이터
   * @param {Function} mockFn - Mock 응답 함수
   */
  async post(url, data, mockFn) {
    if (isDevelopmentMode() && mockFn) {
      console.log(`🎭 [MOCK] POST ${url}`, data)
      await new Promise(resolve => setTimeout(resolve, 300))
      return { data: mockFn(data) }
    }
    return apiClient.post(url, data)
  },

  /**
   * PUT 요청
   * @param {string} url - API 엔드포인트
   * @param {any} data - 요청 데이터
   * @param {Function} mockFn - Mock 응답 함수
   */
  async put(url, data, mockFn) {
    if (isDevelopmentMode() && mockFn) {
      console.log(`🎭 [MOCK] PUT ${url}`, data)
      await new Promise(resolve => setTimeout(resolve, 300))
      return { data: mockFn(data) }
    }
    return apiClient.put(url, data)
  },

  /**
   * DELETE 요청
   * @param {string} url - API 엔드포인트
   * @param {Function} mockFn - Mock 응답 함수
   */
  async delete(url, mockFn) {
    if (isDevelopmentMode() && mockFn) {
      console.log(`🎭 [MOCK] DELETE ${url}`)
      await new Promise(resolve => setTimeout(resolve, 300))
      return { data: mockFn ? mockFn() : { success: true } }
    }
    return apiClient.delete(url)
  }
}

export default apiWrapper
