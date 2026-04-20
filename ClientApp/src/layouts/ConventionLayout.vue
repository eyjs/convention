<template>
  <div class="app-frame">
    <!-- 상단 안전영역 (노치/카메라 차단) -->
    <div class="safe-top bg-gray-50"></div>

    <!-- 스크롤 가능한 콘텐츠 영역 -->
    <div class="safe-content bg-gray-50">
      <!-- 공통 헤더 (brandColor 기반) -->
      <ConventionHeader
        v-if="convention"
        :convention="convention"
        :convention-id="conventionId"
        :d-day="dDay"
        @menu-click="isSidebarOpen = true"
      />
      <SidebarMenu :is-open="isSidebarOpen" @close="isSidebarOpen = false" />

      <main :class="{ 'pb-nav': showNav }">
        <router-view />
      </main>
    </div>

    <!-- 하단 네비게이션 (안전영역 포함) -->
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
            isActive(item.path)
              ? 'text-primary-600'
              : 'text-gray-500 active:scale-95',
          ]"
          @click="navigateTo(item)"
        >
          <svg
            class="w-6 h-6 mb-1"
            :class="isActive(item.path) ? 'stroke-[2.5]' : 'stroke-2'"
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
            v-if="isActive(item.path)"
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
      <!-- 하단 안전영역 (시스템 네비바 차단) -->
      <div class="safe-bottom-spacer"></div>
    </nav>

    <!-- nav 없을 때도 하단 안전영역 확보 -->
    <div v-if="!showNav" class="safe-bottom bg-gray-50"></div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import ConventionHeader from '@/components/convention/ConventionHeader.vue'
import SidebarMenu from '@/components/common/SidebarMenu.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const conventionStore = useConventionStore()
const showNav = computed(() => route.meta.showNav !== false)
const isSidebarOpen = ref(false)
const convention = computed(() => conventionStore.currentConvention)

const dDay = computed(() => {
  if (!convention.value?.startDate) return null
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const start = new Date(convention.value.startDate)
  start.setHours(0, 0, 0, 0)
  const end = convention.value.endDate ? new Date(convention.value.endDate) : start
  end.setHours(23, 59, 59, 999)
  if (today > end) return null
  if (today >= start && today <= end) return 0
  return Math.ceil((start - today) / (1000 * 60 * 60 * 24))
})

const conventionId = computed(() => {
  const id = route.params.conventionId
  return id ? parseInt(id) : null
})
const basePath = computed(() => `/conventions/${conventionId.value}`)

const navItems = computed(() => [
  {
    path: basePath.value,
    label: '홈',
    iconPath:
      'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6',
    badge: 0,
  },
  {
    path: `${basePath.value}/notices`,
    label: '게시판',
    iconPath:
      'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
    badge: 0,
  },
  {
    path: `${basePath.value}/features`,
    label: '더보기',
    iconPath: 'M4 6h16M4 12h16M4 18h16',
    badge: 0,
  },
])

function isActive(path) {
  return route.path === path
}

function navigateTo(item) {
  router.push(item.path).catch((err) => {
    console.error('Navigation error:', err)
  })
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

/* 전체 화면을 flex 컬럼으로 잡아서 상단/하단 안전영역 물리적 차단 */
.app-frame {
  display: flex;
  flex-direction: column;
  height: 100vh;
  height: 100dvh;
  overflow: hidden;
}

/* 상단 안전영역: 노치/카메라 차단 — 콘텐츠 절대 침범 불가 */
.safe-top {
  flex-shrink: 0;
  height: env(safe-area-inset-top, 0px);
}

/* 콘텐츠 영역: 남은 공간 전부, 내부 스크롤 */
.safe-content {
  flex: 1;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
}

/* 하단 네비: fixed 대신 flex 하단 고정 */
.safe-nav {
  flex-shrink: 0;
}

/* 하단 시스템 네비바 차단 스페이서 */
.safe-bottom-spacer {
  height: env(safe-area-inset-bottom, 0px);
  flex-shrink: 0;
}

.safe-bottom {
  height: env(safe-area-inset-bottom, 0px);
  flex-shrink: 0;
}

/* nav가 flex 하단이므로 main에 하단 여백 불필요 */
.pb-nav {
  padding-bottom: 0;
}

/* 앱(웹뷰): env()가 0 반환 시 고정값 사용 */
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
