import axios from 'axios'

const API_URL = import.meta.env.VITE_API_URL || '/api'

const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// 토큰 갱신 관련 상태
let isRefreshing = false
let failedQueue = []

const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token)
    }
  })
  failedQueue = []
}

// 요청 인터셉터 - 토큰 자동 추가
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error),
)

// 응답 인터셉터 - 토큰 만료 처리
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    const requestUrl = originalRequest?.url || ''
    const isAuthRequest =
      requestUrl.includes('/auth/login') ||
      requestUrl.includes('/auth/guest-login') ||
      requestUrl.includes('/auth/logout') ||
      requestUrl.includes('/auth/refresh')

    if (
      error.response?.status === 401 &&
      !originalRequest._retry &&
      !isAuthRequest
    ) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`
            return apiClient(originalRequest)
          })
          .catch((err) => Promise.reject(err))
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const currentRefreshToken = localStorage.getItem('refreshToken')
        if (!currentRefreshToken) {
          throw new Error('No refresh token')
        }

        const response = await axios.post(`${API_URL}/auth/refresh`, {
          refreshToken: currentRefreshToken,
        })

        const { accessToken, refreshToken: newRefreshToken } = response.data

        localStorage.setItem('accessToken', accessToken)
        localStorage.setItem('refreshToken', newRefreshToken)

        processQueue(null, accessToken)
        originalRequest.headers.Authorization = `Bearer ${accessToken}`
        return apiClient(originalRequest)
      } catch (err) {
        processQueue(err, null)
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
        window.location.href = '/login'
        return Promise.reject(err)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)

// Auth API
export const authAPI = {
  register: (data) => apiClient.post('/auth/register', data),
  login: (data) => apiClient.post('/auth/login', data),
  guestLogin: (data) => apiClient.post('/auth/guest-login', data),
  logout: () => apiClient.post('/auth/logout'),
  getCurrentUser: () => apiClient.get('/auth/me'),
  refreshToken: (refreshToken) =>
    apiClient.post('/auth/refresh', { refreshToken }),
  joinConvention: (conventionId, role = 'Guest') =>
    apiClient.post(`/auth/conventions/${conventionId}/join`, { role }),
  leaveConvention: (conventionId) =>
    apiClient.delete(`/auth/conventions/${conventionId}/leave`),
  getAvailableConventions: () => apiClient.get('/auth/conventions/available'),
}

// Convention API
export const conventionAPI = {
  getConventions: () => apiClient.get('/conventions'),
  getConvention: (id) => apiClient.get(`/conventions/${id}`),
}

// Chat API
export const chatAPI = {
  markAsRead: (conventionId) =>
    apiClient.post(`/chat-history/${conventionId}/read`),
}

// Upload API
export const uploadAPI = {
  uploadFile: (formData, onProgress) => {
    return apiClient.post('/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: onProgress,
    })
  },
  uploadNameTags: (conventionId, file) => {
    const formData = new FormData()
    formData.append('file', file)
    return apiClient.post(
      `/upload/conventions/${conventionId}/name-tags`,
      formData,
      { headers: { 'Content-Type': 'multipart/form-data' } },
    )
  },
  deleteFile: (fileId) => apiClient.delete(`/upload/${fileId}`),
}

export const userAPI = {
  updateProfileField: (fieldName, fieldValue) =>
    apiClient.patch('/users/profile/field', { fieldName, fieldValue }),
  uploadPassportImage: (file) => {
    const formData = new FormData()
    formData.append('file', file)
    return apiClient.post('/users/profile/passport-image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },
}

export default apiClient
