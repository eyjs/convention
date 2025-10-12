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
        // localStorage에 값이 없거나, 'undefined' 문자열이거나, 비어있을 경우 null을 반환합니다.
        if (!userStr || userStr === 'undefined') {
            return null
        }
        try {
            // 안전하게 JSON 파싱을 시도합니다.
            return JSON.parse(userStr)
        } catch (e) {
            console.error("Failed to parse user from localStorage:", e)
            return null // 파싱 실패 시 null 반환
        }
    },
    

    logout() {
        localStorage.removeItem('user')
    }
}