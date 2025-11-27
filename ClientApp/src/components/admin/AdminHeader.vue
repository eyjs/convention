<template>
  <header class="bg-white shadow-sm sticky top-0 z-40">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between items-center h-16">
        <div class="flex items-center gap-2">
          <button
            v-if="showBackButton"
            @click="router.push(backPath)"
            class="p-2 hover:bg-gray-100 rounded-lg transition-colors"
            :title="backButtonTitle"
          >
            <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          <h1 class="text-lg sm:text-xl md:text-2xl font-bold text-gray-900 truncate">
            {{ title }}
          </h1>
          <span v-if="subtitle" class="ml-2 sm:ml-4 text-xs sm:text-sm text-gray-500 hidden md:block truncate">
            {{ subtitle }}
          </span>
        </div>
        <div class="relative">
          <button @click="showUserMenu = !showUserMenu" class="p-2 hover:bg-gray-100 rounded-lg">
            <svg class="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>

          <Transition name="fade-down">
            <div v-if="showUserMenu" class="fixed inset-0 z-50" @click="showUserMenu = false" />
          </Transition>
          <Transition name="fade-down">
            <div
              v-if="showUserMenu"
              class="absolute top-14 right-0 w-56 bg-white rounded-md shadow-lg border z-50"
            >
              <ul>
                <li>
                  <router-link to="/my-profile" class="block w-full text-left px-4 py-3 text-sm text-gray-700 hover:bg-gray-100">
                    내 정보
                  </router-link>
                </li>
                <li>
                  <router-link to="/home" class="block w-full text-left px-4 py-3 text-sm text-gray-700 hover:bg-gray-100">
                    사용자 페이지 전환
                  </router-link>
                </li>
                <li>
                  <button @click="handleLogout" class="block w-full text-left px-4 py-3 text-sm text-red-600 hover:bg-gray-100">
                    로그아웃
                  </button>
                </li>
              </ul>
            </div>
          </Transition>
        </div>
      </div>
      <!-- 슬롯 영역 (탭 메뉴 등) -->
      <slot></slot>
    </div>
  </header>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

const props = defineProps({
  title: {
    type: String,
    required: true,
  },
  subtitle: {
    type: String,
    default: '',
  },
  showBackButton: {
    type: Boolean,
    default: false,
  },
  backPath: {
    type: String,
    default: '/admin',
  },
  backButtonTitle: {
    type: String,
    default: '뒤로가기',
  },
});

const router = useRouter();
const authStore = useAuthStore();
const showUserMenu = ref(false);

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout();
    router.push('/login');
  }
};
</script>

<style scoped>
.fade-down-enter-active,
.fade-down-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.fade-down-enter-from,
.fade-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
