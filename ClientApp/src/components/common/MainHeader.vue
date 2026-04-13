<template>
  <div
    class="sticky top-0 z-40"
    :class="{ 'bg-white shadow-sm': !transparent }"
  >
    <div class="px-4 py-4">
      <div class="flex items-center justify-between">
        <div class="flex items-center space-x-3">
          <button
            v-if="showBack"
            class="p-2 -ml-2 rounded-lg"
            :class="
              transparent
                ? 'text-white/80 hover:bg-white/10'
                : 'text-gray-500 hover:bg-gray-100'
            "
            @click="$router.back()"
          >
            <svg
              class="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M15 19l-7-7 7-7"
              />
            </svg>
          </button>
          <h1
            class="text-xl font-bold"
            :class="transparent ? 'text-white' : 'text-gray-900'"
          >
            {{ title }}
          </h1>
        </div>

        <div class="flex items-center space-x-1">
          <!-- Slot for additional action buttons -->
          <slot name="actions"></slot>

          <!-- 알림 벨 (행사 컨텍스트가 있을 때만) -->
          <NotificationBell
            v-if="conventionId"
            :convention-id="conventionId"
            :text-class="transparent ? 'text-white' : 'text-gray-600'"
          />

          <div v-if="showMenu" class="relative">
            <button
              class="p-2 -mr-2 rounded-lg"
              :class="
                transparent
                  ? 'text-white/80 hover:bg-white/10'
                  : 'text-gray-500 hover:bg-gray-100'
              "
              @click="isSidebarOpen = true"
            >
              <svg
                class="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M4 6h16M4 12h16M4 18h16"
                />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
    <SidebarMenu :is-open="isSidebarOpen" @close="isSidebarOpen = false" />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import SidebarMenu from './SidebarMenu.vue'
import NotificationBell from './NotificationBell.vue'

const route = useRoute()
const authStore = useAuthStore()
const conventionId = computed(() => {
  // 관리자 페이지에서는 알림벨 숨김
  if (route.path.startsWith('/admin')) return null
  const id = route.params.conventionId || route.params.id
  return id && authStore.user ? Number(id) : null
})

defineProps({
  title: {
    type: String,
    required: true,
  },
  showBack: {
    type: Boolean,
    default: false,
  },
  transparent: {
    type: Boolean,
    default: false,
  },
  showMenu: {
    type: Boolean,
    default: true,
  },
})

const isSidebarOpen = ref(false)
</script>

<style scoped>
.fade-down-enter-active,
.fade-down-leave-active {
  transition: all 0.2s ease-out;
}
.fade-down-enter-from,
.fade-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* Safe area support for mobile devices with notches */
@supports (padding-top: env(safe-area-inset-top)) {
  .sticky {
    padding-top: env(safe-area-inset-top);
  }
}
</style>
