<template>
    <div id="app" class="bg-gray-50">
        <component v-if="currentLayout" :is="currentLayout">
            <router-view />
        </component>
        <router-view v-else />
        <GlobalPopup />
    </div>
</template>

<script setup>
    import { computed, defineAsyncComponent, onMounted, watch } from 'vue'
    import { useRoute, onBeforeRouteLeave } from 'vue-router'
    import { useAuthStore } from '@/stores/auth'
    import { useUIStore } from '@/stores/ui'
    import { useConventionStore } from '@/stores/convention'
    import { useKeyboardAdjust } from '@/composables/useKeyboardAdjust'
    import globalChatNotificationService from '@/services/globalChatNotificationService'
    import GlobalPopup from '@/components/common/GlobalPopup.vue' // GlobalPopup 임포트

    const route = useRoute()
    const authStore = useAuthStore()
    const uiStore = useUIStore()
    const conventionStore = useConventionStore()

    // 전역 네비게이션 가드: 뒤로가기 시 모달 먼저 닫기
    onBeforeRouteLeave((to, from, next) => {
        if (uiStore.hasOpenModal()) {
            next(false) // 라우팅 취소
            uiStore.closeTopModal() // 최상단 모달 닫기
        } else {
            next() // 정상 라우팅
        }
    })

    // 전역 키보드 대응 비활성화 (네이티브 스크롤 사용)
    const { isKeyboardVisible } = useKeyboardAdjust({
        offset: 20, // 키보드 위 여백 (px)
        duration: 300, // 스크롤 애니메이션 시간 (ms)
        enabled: false, // 비활성화하여 자연스러운 스크롤 유지
    })

    onMounted(() => {
        authStore.initAuth()
        if (authStore.isAuthenticated) {
            authStore.fetchCurrentUser()
        }
    })

    // 로그인 상태 변경 감지 및 글로벌 SignalR 연결 관리
    watch(
        () => [authStore.isAuthenticated, route.path],
        async ([isAuthenticated, currentPath]) => {
            // 개인 여행 페이지에서는 SignalR 연결을 시도하지 않음
            if (currentPath && currentPath.startsWith('/trips')) {
                globalChatNotificationService.disconnect();
                return;
            }

            if (isAuthenticated && authStore.user && authStore.accessToken) {
                try {
                    // 글로벌 SignalR 연결 시작
                    await globalChatNotificationService.connect(
                        authStore.user.id,
                        authStore.accessToken,
                    )

                    // Unread count 증가 이벤트 수신
                    globalChatNotificationService.onUnreadCountIncrement((conventionId) => {
                        // 채팅창이 닫혀있거나, 다른 컨벤션의 채팅창이 열려있을 때만 카운트 증가
                        if (
                            !uiStore.isChatOpen ||
                            conventionStore.currentConvention?.id !== conventionId
                        ) {
                            console.log(
                                `Received unread count increment for convention ${conventionId}`,
                            )
                            authStore.incrementUnreadCount(conventionId)
                        }
                    })

                    // 재연결 시 처리
                    globalChatNotificationService.onReconnected(() => {
                        console.log('Global SignalR reconnected.')
                    })

                    console.log('Global chat notification service initialized.')
                } catch (error) {
                    console.error('Failed to initialize global chat notification:', error)
                }
            } else {
                // 로그아웃 시 연결 해제
                globalChatNotificationService.disconnect()
            }
        },
        { immediate: true },
    )

    const currentLayout = computed(() => {
        const layoutName = route.meta.layout

        if (!layoutName) {
            return null
        }

        return defineAsyncComponent(() => import(`@/layouts/${layoutName}.vue`))
    })
</script>

<style>
    #app {
        width: 100%;
        min-height: 100vh;
        min-height: 100dvh; /* Dynamic viewport height for better mobile support */
    }
</style>