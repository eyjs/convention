import apiClient from './api'

export const authService = {
  async login({ role, conventionId, identifier, password }) {
    let endpoint = ''
    let payload = {}

    if (role === 'admin') {
      endpoint = '/auth/login/admin'
      payload = {
        memberId: identifier,
        password: password
      }
    } else if (role === 'guest') {
      endpoint = '/auth/login/guest'
      payload = {
        telephone: identifier,
        conventionId: parseInt(conventionId),
        password: password
      }
    } else if (role === 'companion') {
      endpoint = '/auth/login/companion'
      payload = {
        telephone: identifier,
        guestId: parseInt(conventionId),
        password: password
      }
    }

    const response = await apiClient.post(endpoint, payload)
    return response.user
  },

  async getConventions() {
    return await apiClient.get('/auth/conventions')
  },

  getCurrentUser() {
    const userStr = localStorage.getItem('user')
    return userStr ? JSON.parse(userStr) : null
  },

  logout() {
    localStorage.removeItem('user')
  }
}
