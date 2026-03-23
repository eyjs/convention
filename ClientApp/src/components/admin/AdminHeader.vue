<template>
  <header class="bg-white shadow-sm sticky top-0 z-50">
    <div class="h-16 px-4 sm:px-6 lg:px-8 flex items-center justify-between">
      <div class="flex items-center gap-3 min-w-0">
        <!-- Mobile sidebar toggle -->
        <button
          class="p-2 hover:bg-gray-100 rounded-lg transition-colors md:hidden"
          @click="$emit('toggle-sidebar')"
        >
          <Menu :size="20" class="text-gray-600" />
        </button>

        <!-- Breadcrumb -->
        <nav class="flex items-center gap-1.5 text-sm min-w-0">
          <router-link
            to="/admin"
            class="text-gray-400 hover:text-gray-600 transition-colors hidden sm:block flex-shrink-0"
          >
            관리자
          </router-link>
          <ChevronRight
            v-if="subtitle"
            :size="14"
            class="text-gray-300 hidden sm:block flex-shrink-0"
          />
          <span class="font-semibold text-gray-900 truncate">
            {{ subtitle || title }}
          </span>
        </nav>
      </div>

      <!-- User menu -->
      <div class="relative flex-shrink-0">
        <button
          class="flex items-center gap-2 p-2 hover:bg-gray-100 rounded-lg transition-colors"
          @click="showUserMenu = !showUserMenu"
        >
          <div
            class="w-8 h-8 bg-primary-100 text-primary-700 rounded-full flex items-center justify-center text-sm font-semibold"
          >
            {{ userInitial }}
          </div>
          <ChevronDown :size="16" class="text-gray-400 hidden sm:block" />
        </button>

        <Transition name="fade-down">
          <div
            v-if="showUserMenu"
            class="fixed inset-0 z-40"
            @click="showUserMenu = false"
          />
        </Transition>
        <Transition name="fade-down">
          <div
            v-if="showUserMenu"
            class="absolute top-12 right-0 w-56 bg-white rounded-lg shadow-lg border z-50 py-1"
          >
            <router-link
              to="/my-profile"
              class="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50"
              @click="showUserMenu = false"
            >
              <UserIcon :size="16" class="text-gray-400" />
              내 정보
            </router-link>
            <router-link
              to="/home"
              class="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50"
              @click="showUserMenu = false"
            >
              <ArrowLeftRight :size="16" class="text-gray-400" />
              사용자 페이지 전환
            </router-link>
            <div class="border-t my-1" />
            <button
              class="flex items-center gap-3 w-full px-4 py-2.5 text-sm text-red-600 hover:bg-red-50"
              @click="handleLogout"
            >
              <LogOut :size="16" />
              로그아웃
            </button>
          </div>
        </Transition>
      </div>
    </div>
  </header>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  Menu,
  ChevronRight,
  ChevronDown,
  User as UserIcon,
  ArrowLeftRight,
  LogOut,
} from 'lucide-vue-next'

defineProps({
  title: {
    type: String,
    required: true,
  },
  subtitle: {
    type: String,
    default: '',
  },
})

defineEmits(['toggle-sidebar'])

const router = useRouter()
const authStore = useAuthStore()
const showUserMenu = ref(false)

const userInitial = computed(() => {
  const name = authStore.user?.name || authStore.user?.loginId || '?'
  return name.charAt(0).toUpperCase()
})

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
  }
}
</script>

<style scoped>
.fade-down-enter-active,
.fade-down-leave-active {
  transition:
    opacity 0.2s ease,
    transform 0.2s ease;
}
.fade-down-enter-from,
.fade-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
