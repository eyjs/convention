import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authAPI } from '@/services/api'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const accessToken = ref(null)
  const refreshToken = ref(null)
  const conventions = ref([])
  const checklistStatus = ref(null) // 체크리스트 상태
  const loading = ref(false)
  const error = ref(null)

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

  function initAuth() {
    const storedToken = localStorage.getItem('accessToken')
    const storedRefreshToken = localStorage.getItem('refreshToken')
    const storedUser = localStorage.getItem('user')

    if (
      storedToken &&
      storedUser &&
      storedUser !== 'undefined' &&
      storedUser !== 'null'
    ) {
      try {
        accessToken.value = storedToken
        refreshToken.value = storedRefreshToken
        user.value = JSON.parse(storedUser)
      } catch (e) {
        console.error('Failed to parse user from localStorage', e)
        // 파싱 실패 시 모든 관련 정보를 깨끗이 지웁니다.
        logout()
      }
    }
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
      localStorage.setItem('user', JSON.stringify(userData))

      // Clear any previously selected convention on a new login
      localStorage.removeItem('selectedConventionId')

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
      // API 호출은 실패할 수도 있으므로, finally 블록에서 상태를 확실히 초기화합니다.
      await authAPI.logout()
    } catch (err) {
      console.error('Logout API call failed:', err)
    } finally {
      // 1. 인증 관련 상태와 localStorage를 먼저 비웁니다.
      accessToken.value = null
      refreshToken.value = null
      user.value = null
      conventions.value = []
      checklistStatus.value = null
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('user')

      // 2. 챗봇 스토어의 상태를 완전히 초기화하는 로직은 logout을 호출하는 곳에서 처리해야 합니다.
      //    예: 로그인 페이지로 리다이렉션하기 전에 useChatStore().resetChatState() 호출
      console.log('Logout successful and all states have been reset.')
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
      checklistStatus.value = response.data.checklistStatus // 체크리스트 상태 저장
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
      // user.value 전체를 새 객체로 교체하여 reactivity 트리거
      user.value = {
        ...user.value,
        conventions: user.value.conventions.map((c) => {
          if (c.id === conventionId) {
            return { ...c, unreadCount: (c.unreadCount || 0) + 1 }
          }
          return c
        }),
      }
      // localStorage 업데이트
      localStorage.setItem('user', JSON.stringify(user.value))
    } else if (user.value.conventionId === conventionId) {
      user.value = {
        ...user.value,
        unreadCount: (user.value.unreadCount || 0) + 1,
      }
      // localStorage 업데이트
      localStorage.setItem('user', JSON.stringify(user.value))
    }
  }

  function resetUnreadCount(conventionId) {
    if (!user.value) return

    if (user.value.conventions && Array.isArray(user.value.conventions)) {
      // user.value 전체를 새 객체로 교체하여 reactivity 트리거
      user.value = {
        ...user.value,
        conventions: user.value.conventions.map((c) => {
          if (c.id === conventionId) {
            return { ...c, unreadCount: 0 }
          }
          return c
        }),
      }
      // localStorage 업데이트
      localStorage.setItem('user', JSON.stringify(user.value))
    } else if (user.value.conventionId === conventionId) {
      user.value = {
        ...user.value,
        unreadCount: 0,
      }
      // localStorage 업데이트
      localStorage.setItem('user', JSON.stringify(user.value))
    }
  }
  return {
    user,
    accessToken,
    refreshToken,
    conventions,
    checklistStatus, // export
    loading,
    error,
    isAuthenticated,
    isAdmin,
    totalUnreadCount,
    initAuth,
    register,
    login,
    logout,
    fetchCurrentUser,
    incrementUnreadCount,
    resetUnreadCount,
  }
})
