<template>
  <div id="app" class="bg-gray-50">
    <router-view />
    <GlobalPopup />

    <!-- 기본 비밀번호 경고 모달 -->
    <Teleport to="body">
      <Transition name="fade">
        <div
          v-if="showDefaultPasswordModal"
          class="fixed inset-0 z-[9999] flex items-center justify-center p-4 bg-black/50"
          @mousedown="onBackdropMouseDown"
          @mouseup="onBackdropMouseUp"
          @touchstart="onBackdropTouchStart"
          @touchend="onBackdropTouchEnd"
        >
          <div class="bg-white rounded-2xl shadow-xl max-w-sm w-full p-6">
            <div class="text-center mb-5">
              <div
                class="w-14 h-14 mx-auto mb-3 bg-amber-100 rounded-full flex items-center justify-center"
              >
                <svg
                  class="w-7 h-7 text-amber-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                  />
                </svg>
              </div>
              <h3 class="text-lg font-bold text-gray-900 mb-2">
                비밀번호 변경 권장
              </h3>
              <p class="text-sm text-gray-600 leading-relaxed">
                현재 기본 비밀번호(1111)를 사용 중입니다.<br />
                보안을 위해 비밀번호를 변경해 주세요.
              </p>
            </div>

            <div class="space-y-2">
              <button
                class="w-full py-3 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 transition-colors"
                @click="goToChangePassword"
              >
                비밀번호 변경하러 가기
              </button>
              <button
                class="w-full py-3 text-gray-500 text-sm hover:bg-gray-50 rounded-xl"
                @click="dismissDefaultPasswordModal"
              >
                나중에
              </button>
              <label
                class="flex items-center justify-center gap-2 text-xs text-gray-400 mt-2 cursor-pointer"
              >
                <input
                  v-model="dontShowAgain"
                  type="checkbox"
                  class="rounded border-gray-300"
                />
                다시 보지 않기 (1111 계속 사용)
              </label>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useUIStore } from '@/stores/ui'
import { useKeyboardAdjust } from '@/composables/useKeyboardAdjust'
import GlobalPopup from '@/components/common/GlobalPopup.vue'
import { useBackdropClose } from '@/composables/useBackdropClose'

const authStore = useAuthStore()
const uiStore = useUIStore()
const router = useRouter()

useKeyboardAdjust({
  offset: 20,
  duration: 300,
  enabled: false,
})

const onKeyDown = (e) => {
  if (e.key === 'Escape') {
    uiStore.closeTopModal()
  }
}

// 기본 비밀번호 경고 모달
// - 기본: 매 로그인마다 노출
// - "다시 보지 않기" 체크 시: 해당 사용자는 두번 다시 노출 안 함 (계정별 플래그)
const showDefaultPasswordModal = ref(false)
const dontShowAgain = ref(false)

function getDismissKey() {
  const userId = authStore.user?.id || authStore.user?.loginId || 'anonymous'
  return `defaultPasswordDismissed:${userId}`
}

function checkDefaultPassword() {
  const flagged = sessionStorage.getItem('defaultPasswordLogin') === '1'
  if (!flagged || !authStore.user) return
  const dismissed = localStorage.getItem(getDismissKey()) === '1'
  if (dismissed) {
    sessionStorage.removeItem('defaultPasswordLogin')
    return
  }
  showDefaultPasswordModal.value = true
}

function dismissDefaultPasswordModal() {
  // "다시 보지 않기" 체크 시 영구 숨김 (계정별)
  if (dontShowAgain.value) {
    localStorage.setItem(getDismissKey(), '1')
  }
  showDefaultPasswordModal.value = false
  dontShowAgain.value = false
  sessionStorage.removeItem('defaultPasswordLogin')
}

const {
  onBackdropMouseDown,
  onBackdropMouseUp,
  onBackdropTouchStart,
  onBackdropTouchEnd,
} = useBackdropClose(dismissDefaultPasswordModal)

function goToChangePassword() {
  showDefaultPasswordModal.value = false
  sessionStorage.removeItem('defaultPasswordLogin')
  router.push('/my-profile')
}

// 사용자 로그인 상태 변경 감지
watch(
  () => authStore.user,
  (user) => {
    if (user) checkDefaultPassword()
  },
  { immediate: true },
)

// 라우트 변경 시에도 재확인 (로그인 직후 홈 진입 보강)
watch(
  () => router.currentRoute.value.fullPath,
  () => {
    if (authStore.user) checkDefaultPassword()
  },
)

onMounted(() => {
  authStore.ensureInitialized()
  window.addEventListener('keydown', onKeyDown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', onKeyDown)
})
</script>

<style>
#app {
  width: 100%;
  min-height: 100vh;
  min-height: 100dvh;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
