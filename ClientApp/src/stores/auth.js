import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authAPI } from '@/services/api'

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

        // [수정] storedUser가 null이나 'undefined' 문자열이 아닌지 확인합니다.
        if (storedToken && storedUser && storedUser !== 'undefined') {
            try {
                accessToken.value = storedToken
                refreshToken.value = storedRefreshToken
                user.value = JSON.parse(storedUser)
            } catch (e) {
                console.error('Failed to parse user from localStorage', e)
                // 파싱 실패 시 관련 정보 초기화
                localStorage.removeItem('accessToken')
                localStorage.removeItem('refreshToken')
                localStorage.removeItem('user')
            }
        }
    }

    async function register(data) {
        loading.value = true
        error.value = null

        try {
            const response = await authAPI.register(data)
            return { success: true, data: response.data }
        } catch (err) {
            error.value = err.response?.data?.message || '회원가입에 실패했습니다.'
            return { success: false, error: error.value }
        } finally {
            loading.value = false
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

    // -- 비회원 로그인 함수--
    async function guestLogin(conventionId, name, phone) {
        loading.value = true
        error.value = null

        try {
            const response = await authAPI.guestLogin({ conventionId, name, phone })
            const { accessToken: token, guest, convention } = response.data

            accessToken.value = token

            // 회원과 동일한 user 객체 형태로 상태를 설정합니다.
            const userData = {
                id: guest.id,
                name: guest.name,
                phone: guest.phone,
                role: 'Guest', // 역할을 명확히 'Guest'로 지정
                guestId: guest.id // 챗봇 컨텍스트를 위해 guestId 추가
            }
            user.value = userData
            conventions.value = [convention] // 현재 행사 정보를 배열에 담아 저장

            // 로컬 스토리지에 'user' 키로 통일해서 저장합니다.
            localStorage.setItem('accessToken', token)
            localStorage.setItem('user', JSON.stringify(userData))

            return { success: true }
        } catch (err) {
            if (err.response?.status === 400 && err.response.data.loginId) {
                // 이미 회원으로 전환된 경우
                error.value = err.response.data.message
            } else {
                error.value = err.response?.data?.message || '로그인에 실패했습니다.'
            }
            return { success: false, error: error.value }
        } finally {
            loading.value = false
        }
    }
    
    async function logout() {
        try {
            await authAPI.logout()
        } catch (err) {
            console.error('Logout error:', err)
        } finally {
            accessToken.value = null
            refreshToken.value = null
            user.value = null
            conventions.value = []

            localStorage.removeItem('accessToken')
            localStorage.removeItem('refreshToken')
            localStorage.removeItem('user')
        }
    }

    async function fetchCurrentUser() {
        if (!accessToken.value) return

        loading.value = true
        try {
            const response = await authAPI.getCurrentUser()
            user.value = response.data
            conventions.value = response.data.conventions || []
            localStorage.setItem('user', JSON.stringify(response.data))
        } catch (err) {
            console.error('Fetch user error:', err)
            if (err.response?.status === 401) {
                await logout()
            }
        } finally {
            loading.value = false
        }
    }

    async function joinConvention(conventionId, role = 'Guest') {
        loading.value = true
        error.value = null
        try {
            await authAPI.joinConvention(conventionId, role)
            await fetchCurrentUser()
            return { success: true }
        } catch (err) {
            error.value = err.response?.data?.message || '행사 참여에 실패했습니다.'
            return { success: false, error: error.value }
        } finally {
            loading.value = false
        }
    }

    async function leaveConvention(conventionId) {
        loading.value = true
        error.value = null
        try {
            await authAPI.leaveConvention(conventionId)
            await fetchCurrentUser()
            return { success: true }
        } catch (err) {
            error.value = err.response?.data?.message || '행사 참여 취소에 실패했습니다.'
            return { success: false, error: error.value }
        } finally {
            loading.value = false
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
        guestLogin,
        logout,
        fetchCurrentUser,
        joinConvention,
        leaveConvention
    }
})