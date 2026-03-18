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
  (error) => {
    return Promise.reject(error)
  },
)

// 응답 인터셉터 - 토큰 만료 처리 (경쟁 조건 방지)
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    // 401 에러이고 재시도하지 않은 경우 (로그인 요청은 토큰 갱신 불필요)
    const requestUrl = originalRequest?.url || ''
    const isAuthRequest =
      requestUrl.includes('/auth/login') ||
      requestUrl.includes('/auth/guest-login')
    if (
      error.response?.status === 401 &&
      !originalRequest._retry &&
      !isAuthRequest
    ) {
      // 이미 토큰 갱신 중이면 큐에 추가
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
        const refreshToken = localStorage.getItem('refreshToken')
        if (!refreshToken) {
          throw new Error('No refresh token')
        }

        const response = await axios.post(`${API_URL}/auth/refresh`, {
          refreshToken,
        })

        const { accessToken, refreshToken: newRefreshToken } = response.data
        localStorage.setItem('accessToken', accessToken)
        localStorage.setItem('refreshToken', newRefreshToken)

        // 대기 중인 모든 요청 처리
        processQueue(null, accessToken)

        originalRequest.headers.Authorization = `Bearer ${accessToken}`
        return apiClient(originalRequest)
      } catch (err) {
        // 토큰 갱신 실패 - 대기 중인 요청들도 실패 처리
        processQueue(err, null)

        // 로그아웃 처리
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
        localStorage.removeItem('user')
        localStorage.removeItem('selectedConventionId')
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
  guestLogin: (data) => apiClient.post('/auth/guest-login', data), // 👈 비회원 로그인 추가
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
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      onUploadProgress: onProgress,
    })
  },
  uploadNameTags: (conventionId, file) => {
    const formData = new FormData()
    formData.append('file', file)
    return apiClient.post(
      `/upload/conventions/${conventionId}/name-tags`,
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      },
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
