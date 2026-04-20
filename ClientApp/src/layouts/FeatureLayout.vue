<template>
  <div class="app-frame">
    <div class="safe-top bg-white"></div>

    <div class="safe-content bg-white">
      <header class="sticky top-0 z-10 bg-white border-b border-gray-200">
        <div class="flex items-center px-4 py-3">
          <button
            class="back-button flex items-center gap-2 px-3 py-2 text-gray-600 hover:text-gray-900"
            @click="goBack"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
            <span>뒤로</span>
          </button>
          <div class="flex-1 text-center font-semibold text-lg">
            <slot name="header"></slot>
          </div>
          <div class="w-20"></div>
        </div>
      </header>

      <main class="p-4">
        <slot />
      </main>
    </div>

    <div class="safe-bottom bg-white"></div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'

const router = useRouter()

const goBack = () => {
  router.push('/features')
}
</script>

<style scoped>
/* flex 프레임: 노치/네비바 영역 물리적 차단 */
.app-frame {
  display: flex;
  flex-direction: column;
  height: 100vh;
  height: 100dvh;
  overflow: hidden;
}

.safe-top {
  flex-shrink: 0;
  height: env(safe-area-inset-top, 0px);
}

.safe-content {
  flex: 1;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
}

.safe-bottom {
  height: env(safe-area-inset-bottom, 0px);
  flex-shrink: 0;
}

:global(.capacitor-app) .safe-top {
  height: max(env(safe-area-inset-top, 0px), 2rem);
}

:global(.capacitor-app) .safe-bottom {
  height: max(env(safe-area-inset-bottom, 0px), 3rem);
}
</style>
