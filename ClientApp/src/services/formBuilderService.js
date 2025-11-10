import apiClient from './api'

const formBuilderService = {
  /**
   * 특정 행사에 속한 모든 폼 정의 목록을 가져옵니다.
   * @param {number} conventionId
   * @returns {Promise<AxiosResponse<any>>}
   */
  getFormDefinitions(conventionId) {
    return apiClient.get(`/admin/conventions/${conventionId}/forms`)
  },

  /**
   * 특정 폼 정의와 포함된 필드들을 가져옵니다.
   * @param {number} conventionId
   * @param {number} id - FormDefinition ID
   * @returns {Promise<AxiosResponse<any>>}
   */
  getFormDefinition(id) {
    return apiClient.get(`/forms/${id}/definition`);
  },

  /**
   * 새로운 폼 정의를 생성합니다.
   * @param {number} conventionId
   * @param {object} formData
   * @returns {Promise<AxiosResponse<any>>}
   */
  createFormDefinition(conventionId, formData) {
    return apiClient.post(`/admin/conventions/${conventionId}/forms`, formData)
  },

  /**
   * 기존 폼 정의와 필드들을 업데이트합니다.
   * @param {number} conventionId
   * @param {number} id - FormDefinition ID
   * @param {object} formData
   * @returns {Promise<AxiosResponse<any>>}
   */
  updateFormDefinition(conventionId, id, formData) {
    return apiClient.put(`/admin/conventions/${conventionId}/forms/${id}`, formData)
  },

  /**
   * 폼 정의를 삭제합니다.
   * @param {number} conventionId
   * @param {number} id - FormDefinition ID
   * @returns {Promise<AxiosResponse<any>>}
   */
  deleteFormDefinition(conventionId, id) {
    return apiClient.delete(`/admin/conventions/${conventionId}/forms/${id}`)
  },
}

export default formBuilderService
