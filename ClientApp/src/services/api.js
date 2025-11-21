import axios from 'axios'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

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

// 응답 인터셉터 - 토큰 만료 처리
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    // 401 에러이고 재시도하지 않은 경우
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        const refreshToken = localStorage.getItem('refreshToken')
        if (refreshToken) {
          const response = await axios.post(`${API_URL}/auth/refresh`, {
            refreshToken,
          })

          const { accessToken, refreshToken: newRefreshToken } = response.data
          localStorage.setItem('accessToken', accessToken)
          localStorage.setItem('refreshToken', newRefreshToken)

          originalRequest.headers.Authorization = `Bearer ${accessToken}`
          return apiClient(originalRequest)
        }
      } catch (err) {
        // 토큰 갱신 실패 - 로그아웃 처리
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
        localStorage.removeItem('user')
        window.location.href = '/login'
      }
    }

    return Promise.reject(error)
  },
)

// 인증 인터셉터가 없는 Public API Client
export const publicApiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

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
  deleteFile: (fileId) => apiClient.delete(`/upload/${fileId}`),
}

export default apiClient
