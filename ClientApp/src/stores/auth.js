import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authAPI } from '@/services/api'
import { useChatStore } from '@/stores/chat' // 👈 chat.js 스토어를 임포트합니다.

export const useAuthStore = defineStore('auth', () => {
    const user = ref(null)
    const accessToken = ref(null)
    const refreshToken = ref(null)
    const conventions = ref([])
    const loading = ref(false)
    const error = ref(null)

    const isAuthenticated = computed(() => !!accessToken.value)
    const isAdmin = computed(() => user.value?.role === 'Admin')

    function initAuth() {
        const storedToken = localStorage.getItem('accessToken')
        const storedRefreshToken = localStorage.getItem('refreshToken')
        const storedUser = localStorage.getItem('user')

        if (storedToken && storedUser && storedUser !== 'undefined' && storedUser !== 'null') {
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
            const { accessToken: token, refreshToken: refresh, user: userData, conventions: userConventions } = response.data

            accessToken.value = token
            refreshToken.value = refresh
            user.value = userData
            conventions.value = userConventions || []

            localStorage.setItem('accessToken', token)
            localStorage.setItem('refreshToken', refresh)
            localStorage.setItem('user', JSON.stringify(userData))

            return { success: true }
        } catch (err) {
            error.value = err.response?.data?.message || '로그인에 실패했습니다.'
            return { success: false, error: error.value }
        } finally {
            loading.value = false
        }
    }

    // 👇 --- [핵심 수정] 로그아웃 시 챗봇 상태를 함께 초기화합니다. --- 👇
    async function logout() {
        const chatStore = useChatStore() // chat.js 스토어 인스턴스를 가져옵니다.

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

            localStorage.removeItem('accessToken')
            localStorage.removeItem('refreshToken')
            localStorage.removeItem('user')

            // 2. 챗봇 스토어의 상태를 완전히 초기화합니다.
            chatStore.resetChatState()

            console.log("Logout successful and all states have been reset.");
        }
    }
    // 👆 --- 여기까지 --- 👆

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
        } catch (err) {
            console.error('Failed to fetch user', err)
            if (err.response?.status === 401) {
                await logout()
            }
        }
    }

    return {
        user,
        accessToken,
        refreshToken,
        conventions,
        loading,
        error,
        isAuthenticated,
        isAdmin,
        initAuth,
        register,
        login,
        logout,
        fetchCurrentUser
    }
})