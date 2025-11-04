<template>
  <div class="sticky top-0 z-40" :class="{ 'bg-white shadow-sm': !transparent }">
    <div class="px-4 py-4">
      <div class="flex items-center justify-between">
        <div class="flex items-center space-x-3">
          <button v-if="showBack" @click="$router.back()" class="p-2 -ml-2 rounded-lg" :class="transparent ? 'text-white/80 hover:bg-white/10' : 'text-gray-500 hover:bg-gray-100'">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" /></svg>
          </button>
          <h1 class="text-xl font-bold" :class="transparent ? 'text-white' : 'text-gray-900'">{{ title }}</h1>
        </div>
        
        <div class="flex items-center space-x-2">
          <!-- Slot for additional action buttons -->
          <slot name="actions"></slot>

          <div class="relative">
            <button @click="showUserMenu = !showUserMenu" class="p-2 -mr-2 rounded-lg" :class="transparent ? 'text-white/80 hover:bg-white/10' : 'text-gray-500 hover:bg-gray-100'">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" /></svg>
            </button>

            <Transition name="fade-down">
              <div v-if="showUserMenu" class="fixed inset-0 z-50" @click="showUserMenu = false">
                <div class="absolute top-16 right-4 w-56 bg-white rounded-md shadow-lg border">
                  <ul>
                    <li>
                      <router-link to="/my-conventions" class="block w-full text-left px-4 py-3 text-sm text-gray-700 hover:bg-gray-100">
                        행사 리스트
                      </router-link>
                    </li>
                    <li v-if="authStore.isAdmin">
                      <router-link to="/admin" class="block w-full text-left px-4 py-3 text-sm text-gray-700 hover:bg-gray-100">
                        관리자 페이지
                      </router-link>
                    </li>
                    <li>
                      <button @click="handleLogout" class="block w-full text-left px-4 py-3 text-sm text-red-600 hover:bg-gray-100">
                        로그아웃
                      </button>
                    </li>
                  </ul>
                </div>
              </div>
            </Transition>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

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
  }
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
  transition: all 0.2s ease-out;
}
.fade-down-enter-from,
.fade-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
