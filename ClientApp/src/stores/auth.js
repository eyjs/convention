import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authAPI } from '@/services/api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const accessToken = ref(null)
  const refreshToken = ref(null)
  const checklistStatus = ref(null)
  const loading = ref(false)
  const error = ref(null)
  const _initialized = ref(false)
  let _initPromise = null

  const isAuthenticated = computed(() => !!accessToken.value)
  const isAdmin = computed(() => user.value?.role === 'Admin')

  const totalUnreadCount = computed(() => {
    if (user.value?.conventions && Array.isArray(user.value.conventions)) {
      return user.value.conventions.reduce(
        (total, conv) => total + (conv.unreadCount || 0),
        0,
      )
    }
    return user.value?.unreadCount || 0
  })

  function isTokenExpired(token) {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]))
      return payload.exp * 1000 < Date.now() - 10000
    } catch {
      return true
    }
  }

  function initAuth() {
    const storedToken = localStorage.getItem('accessToken')
    const storedRefreshToken = localStorage.getItem('refreshToken')

    if (!storedToken) return

    if (isTokenExpired(storedToken)) {
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      return
    }

    accessToken.value = storedToken
    refreshToken.value = storedRefreshToken
  }

  // 라우터 가드에서 호출 — 인증 초기화 + 사용자 정보 로드 완료 보장
  async function ensureInitialized() {
    if (_initialized.value) return
    if (_initPromise) return _initPromise

    _initPromise = (async () => {
      initAuth()
      if (accessToken.value) {
        await fetchCurrentUser()
      }
      _initialized.value = true
    })()

    return _initPromise
  }

  async function login(loginId, password) {
    loading.value = true
    error.value = null
    try {
      const response = await authAPI.login({ loginId, password })
      const {
        accessToken: token,
        refreshToken: refresh,
        user: userData,
      } = response.data

      accessToken.value = token
      refreshToken.value = refresh
      user.value = userData

      localStorage.setItem('accessToken', token)
      localStorage.setItem('refreshToken', refresh)

      return { success: true }
    } catch (err) {
      error.value = err.response?.data?.message || '로그인에 실패했습니다.'
      return { success: false, error: error.value }
    } finally {
      loading.value = false
    }
  }

  async function logout() {
    try {
      await authAPI.logout()
    } catch (err) {
      console.error('Logout API call failed:', err)
    } finally {
      accessToken.value = null
      refreshToken.value = null
      user.value = null
      checklistStatus.value = null
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
    }
  }

  async function register(data) {
    loading.value = true
    error.value = null
    try {
      await authAPI.register(data)
      return { success: true }
    } catch (err) {
      error.value = err.response?.data?.message || '회원가입 실패'
      return { success: false, error: error.value }
    } finally {
      loading.value = false
    }
  }

  async function fetchCurrentUser() {
    if (!accessToken.value) return
    try {
      const response = await authAPI.getCurrentUser()
      user.value = response.data
      checklistStatus.value = response.data.checklistStatus
    } catch (err) {
      console.error('Failed to fetch user', err)
      if (err.response?.status === 401) {
        await logout()
      }
    }
  }

  function incrementUnreadCount(conventionId) {
    if (!user.value) return

    if (user.value.conventions && Array.isArray(user.value.conventions)) {
      user.value = {
        ...user.value,
        conventions: user.value.conventions.map((c) => {
          if (c.id === conventionId) {
            return { ...c, unreadCount: (c.unreadCount || 0) + 1 }
          }
          return c
        }),
      }
    } else if (user.value.conventionId === conventionId) {
      user.value = {
        ...user.value,
        unreadCount: (user.value.unreadCount || 0) + 1,
      }
    }
  }

  function resetUnreadCount(conventionId) {
    if (!user.value) return

    if (user.value.conventions && Array.isArray(user.value.conventions)) {
      user.value = {
        ...user.value,
        conventions: user.value.conventions.map((c) => {
          if (c.id === conventionId) {
            return { ...c, unreadCount: 0 }
          }
          return c
        }),
      }
    } else if (user.value.conventionId === conventionId) {
      user.value = {
        ...user.value,
        unreadCount: 0,
      }
    }
  }

  return {
    user,
    accessToken,
    refreshToken,
    checklistStatus,
    loading,
    error,
    isAuthenticated,
    isAdmin,
    totalUnreadCount,
    initAuth,
    ensureInitialized,
    register,
    login,
    logout,
    fetchCurrentUser,
    incrementUnreadCount,
    resetUnreadCount,
  }
})
