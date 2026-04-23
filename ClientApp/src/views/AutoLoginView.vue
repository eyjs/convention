<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50">
    <div class="text-center">
      <div
        class="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600 mx-auto"
      ></div>
      <p class="mt-3 text-sm text-gray-500">{{ message }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/api'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const message = ref('자동 로그인 중...')

onMounted(async () => {
  const { token, conventionId, redirect } = route.query

  if (!token) {
    message.value = '잘못된 접근입니다.'
    setTimeout(() => router.push('/login'), 2000)
    return
  }

  try {
    // AccessToken으로 게스트 자동 로그인
    const res = await apiClient.post('/auth/token-login', {
      accessToken: token,
      conventionId: conventionId ? Number(conventionId) : null,
    })

    if (res.data?.accessToken) {
      // JWT 토큰 저장
      authStore.setTokens(res.data.accessToken, res.data.refreshToken)
      authStore.setUser(res.data.user)

      // 리다이렉트
      if (redirect) {
        router.replace(redirect)
      } else if (conventionId) {
        router.replace(`/conventions/${conventionId}`)
      } else {
        router.replace('/')
      }
    } else {
      message.value = '로그인 실패. 로그인 페이지로 이동합니다.'
      setTimeout(() => router.push('/login'), 2000)
    }
  } catch (e) {
    console.error('Auto login failed:', e)
    message.value = '로그인 실패. 로그인 페이지로 이동합니다.'
    setTimeout(() => router.push('/login'), 2000)
  }
})
</script>
