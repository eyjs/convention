import axios from 'axios'

// API 기본 설정
const api = axios.create({
  baseURL: '/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 요청 인터셉터
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth_token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

// 응답 인터셉터
api.interceptors.response.use(
  (response) => response.data,
  (error) => {
    console.error('API Error:', error)
    
    if (error.response?.status === 401) {
      localStorage.removeItem('auth_token')
      window.location.href = '/login'
    }
    
    return Promise.reject(error)
  }
)

export { api }

// Convention API 서비스
export const conventionService = {
  // 컨벤션 목록 조회
  async getConventions() {
    return await api.get('/DatabaseTest/test-data')
  },

  // 특정 컨벤션 상세 조회
  async getConvention(id) {
    return await api.get(`/DatabaseTest/convention/${id}`)
  },

  // 내 일정 조회
  async getMySchedules(guestId) {
    return await api.get(`/convention/guest/${guestId}/schedules`)
  },

  // 공지사항 조회
  async getNotices(conventionId) {
    return await api.get(`/convention/${conventionId}/notices`)
  },

  // 투어 정보 조회
  async getTourInfo(conventionId) {
    return await api.get(`/convention/${conventionId}/tours`)
  },

  // 사진첩 조회
  async getPhotos(conventionId) {
    return await api.get(`/convention/${conventionId}/photos`)
  },

  // 연결 상태 확인
  async getConnectionStatus() {
    return await api.get('/DatabaseTest/connection-status')
  }
}

// 사용자 인증 서비스
export const authService = {
  async login(credentials) {
    return await api.post('/auth/login', credentials)
  },

  async logout() {
    localStorage.removeItem('auth_token')
    return Promise.resolve()
  },

  async getCurrentUser() {
    return await api.get('/auth/me')
  }
}
