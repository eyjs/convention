<template>
  <div id="app" class="bg-gray-50">
    <router-view />
    <GlobalPopup />
  </div>
</template>

<script setup>
import { onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useUIStore } from '@/stores/ui'
import { useConventionStore } from '@/stores/convention'
import { useKeyboardAdjust } from '@/composables/useKeyboardAdjust'
import globalChatNotificationService from '@/services/globalChatNotificationService'
import GlobalPopup from '@/components/common/GlobalPopup.vue'

const route = useRoute()
const authStore = useAuthStore()
const uiStore = useUIStore()
const conventionStore = useConventionStore()

useKeyboardAdjust({
  offset: 20,
  duration: 300,
  enabled: false,
})

onMounted(() => {
  // 인증 초기화는 라우터 가드(ensureInitialized)에서 처리
  // SignalR 연결을 위해 여기서도 초기화 시도
  authStore.ensureInitialized()
})

// SignalR 연결 관리
watch(
  () => [authStore.isAuthenticated, route.path],
  async ([isAuthenticated, currentPath]) => {
    if (currentPath && currentPath.startsWith('/trips')) {
      globalChatNotificationService.disconnect()
      return
    }

    if (isAuthenticated && authStore.user && authStore.accessToken) {
      try {
        await globalChatNotificationService.connect(
          authStore.user.id,
          authStore.accessToken,
        )

        globalChatNotificationService.onUnreadCountIncrement((conventionId) => {
          if (
            !uiStore.isChatOpen ||
            conventionStore.currentConvention?.id !== conventionId
          ) {
            authStore.incrementUnreadCount(conventionId)
          }
        })

        globalChatNotificationService.onReconnected(() => {
          console.log('Global SignalR reconnected.')
        })
      } catch (error) {
        console.error('Failed to initialize global chat notification:', error)
      }
    } else {
      globalChatNotificationService.disconnect()
    }
  },
  { immediate: true },
)
</script>

<style>
#app {
  width: 100%;
  min-height: 100vh;
  min-height: 100dvh;
}
</style>
