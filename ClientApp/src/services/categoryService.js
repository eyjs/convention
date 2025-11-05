import api from './api'

export const categoryAPI = {
  getNoticeCategories(conventionId) {
    return api.get(`/conventions/${conventionId}/notice-categories`)
  },
  createNoticeCategory(conventionId, data) {
    return api.post(`/conventions/${conventionId}/notice-categories`, data)
  },
  updateNoticeCategory(conventionId, categoryId, data) {
    return api.put(
      `/conventions/${conventionId}/notice-categories/${categoryId}`,
      data,
    )
  },
  deleteNoticeCategory(conventionId, categoryId) {
    return api.delete(
      `/conventions/${conventionId}/notice-categories/${categoryId}`,
    )
  },
}
