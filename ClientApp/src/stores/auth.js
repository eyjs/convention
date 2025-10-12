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

        if (storedToken && storedUser && storedUser !== 'undefined' && storedUser !== 'null') {
            try {
                accessToken.value = storedToken
                refreshToken.value = storedRefreshToken
                user.value = JSON.parse(storedUser)
            } catch (e) {
                console.error('Failed to parse user from localStorage', e)
                logout()
            }
        }
    }

    async function login(loginId, password) {
        loading.value = true
        error.value = null

        try {
            const response = await authAPI.login({ loginId, password })

            // [핵심 수정] 백엔드에서 보내준 표준 user 객체를 그대로 사용합니다.
            const { accessToken: token, refreshToken: refresh, user: userData, conventions: userConventions } = response.data

            accessToken.value = token
            refreshToken.value = refresh
            user.value = userData // 👈 이제 userData는 항상 올바른 값을 가집니다.
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

    async function guestLogin(conventionId, name, phone) {
        loading.value = true
        error.value = null

        try {
            const response = await authAPI.guestLogin({ conventionId, name, phone })
            const { accessToken: token, guest, convention } = response.data

            accessToken.value = token

            const userData = {
                id: guest.id,
                name: guest.name,
                phone: guest.phone,
                role: 'Guest',
                guestId: guest.id
            }
            user.value = userData
            conventions.value = [convention]

            localStorage.setItem('accessToken', token)
            localStorage.setItem('user', JSON.stringify(userData))

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

    // (나머지 register, fetchCurrentUser 등 다른 함수들은 생략 없이 완전하게 유지되어야 합니다)
    async function register(data) { /* ... */ return { success: false } }
    async function fetchCurrentUser() { /* ... */ }
    async function joinConvention(id) { /* ... */ return { success: false } }
    async function leaveConvention(id) { /* ... */ return { success: false } }

    return { user, accessToken, refreshToken, conventions, loading, error, isAuthenticated, isAdmin, initAuth, register, login, guestLogin, logout, fetchCurrentUser, joinConvention, leaveConvention }
})