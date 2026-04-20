<template>
  <div class="app-frame">
    <div class="safe-top bg-gray-50"></div>

    <div class="safe-content bg-gray-50">
      <main>
        <slot />
      </main>
    </div>

    <nav
      v-if="showNav"
      class="safe-nav bg-white border-t border-gray-200 shadow-lg z-40"
    >
      <div
        class="flex items-center justify-around h-16 max-w-screen-xl mx-auto"
      >
        <button
          v-for="item in navItems"
          :key="item.path"
          :class="[
            'flex flex-col items-center justify-center flex-1 h-full transition-all relative',
            currentRoute === item.path
              ? 'text-primary-600'
              : 'text-gray-500 active:scale-95',
          ]"
          @click="navigateTo(item.path)"
        >
          <svg
            class="w-6 h-6 mb-1"
            :class="currentRoute === item.path ? 'stroke-[2.5]' : 'stroke-2'"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              :d="item.iconPath"
            />
          </svg>
          <span class="text-xs font-medium">{{ item.label }}</span>
          <span
            v-if="currentRoute === item.path"
            class="absolute top-0 left-1/2 transform -translate-x-1/2 w-12 h-1 bg-primary-600 rounded-b-full"
          ></span>
          <span
            v-if="item.badge > 0"
            class="absolute top-2 right-1/4 w-4 h-4 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center"
          >
            {{ item.badge > 9 ? '9+' : item.badge }}
          </span>
        </button>
      </div>
      <div class="safe-bottom-spacer"></div>
    </nav>

    <div v-if="!showNav" class="safe-bottom bg-gray-50"></div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const conventionStore = useConventionStore()

const currentRoute = computed(() => route.path)
const showNav = computed(() => route.meta.showNav !== false)

const navItems = computed(() => [
  {
    path: '/',
    label: '홈',
    iconPath:
      'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6',
    badge: 0,
  },
  {
    path: '/my-schedule',
    label: '일정',
    iconPath:
      'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
    badge: 0,
  },
  {
    path: '/notices',
    label: '게시판',
    iconPath:
      'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
    badge: 0,
  },
  {
    path: '/features',
    label: '더보기',
    iconPath: 'M4 6h16M4 12h16M4 18h16',
    badge: 0,
  },
])

function navigateTo(path) {
  if (path) {
    router.push(path).catch((err) => {
      console.error('Navigation error:', err)
    })
  }
}
</script>

<style scoped>
.modal-slide-enter-active,
.modal-slide-leave-active {
  transition: all 0.3s ease-in-out;
}
.modal-slide-enter-active .w-full,
.modal-slide-leave-active .w-full {
  transition: all 0.3s ease-in-out;
}

.modal-slide-enter-from,
.modal-slide-leave-to {
  background-color: rgba(0, 0, 0, 0);
}

@media (max-width: 639px) {
  .modal-slide-enter-from .w-full,
  .modal-slide-leave-to .w-full {
    transform: translateY(100%);
  }
}

@media (min-width: 640px) {
  .modal-slide-enter-from .w-full,
  .modal-slide-leave-to .w-full {
    transform: scale(0.95);
    opacity: 0;
  }
}

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

.safe-nav {
  flex-shrink: 0;
}

.safe-bottom-spacer,
.safe-bottom {
  height: env(safe-area-inset-bottom, 0px);
  flex-shrink: 0;
}

/* Capacitor 앱: env()가 0 반환 시 고정값 */
:global(.capacitor-app) .safe-top {
  height: max(env(safe-area-inset-top, 0px), 2rem);
}

:global(.capacitor-app) .safe-bottom-spacer,
:global(.capacitor-app) .safe-bottom {
  height: max(env(safe-area-inset-bottom, 0px), 3rem);
}

button:active {
  transition: transform 0.1s ease;
}
</style>
