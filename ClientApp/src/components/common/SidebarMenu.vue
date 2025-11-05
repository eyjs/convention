<template>
  <div v-show="isOpen" class="fixed inset-0 z-50">
    <!-- Overlay -->
    <Transition name="fade">
      <div
        v-if="isOpen"
        class="absolute inset-0 bg-black/50"
        @click="close"
      ></div>
    </Transition>

    <!-- Sidebar -->
    <Transition name="slide-right">
      <div
        v-show="isOpen"
        class="absolute right-0 w-72 h-full bg-white shadow-xl"
      >
        <div class="p-4">
          <h2 class="text-xl font-bold">메뉴</h2>
          <button
            @click="close"
            class="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
          >
            ✕
          </button>
        </div>
        <ul>
          <li>
            <router-link
              to="/my-conventions"
              class="block w-full text-left px-4 py-3 text-sm text-gray-700 bg-gray-50 hover:bg-gray-200 rounded-md mb-2 shadow-sm"
            >
              행사 리스트
            </router-link>
          </li>
          <li v-if="authStore.isAdmin">
            <router-link
              to="/admin"
              class="block w-full text-left px-4 py-3 text-sm text-gray-700 hover:bg-gray-100 rounded-md mb-2"
            >
              관리자 페이지
            </router-link>
          </li>
          <li>
            <button
              @click="handleLogout"
              class="block w-full text-left px-4 py-3 text-sm text-red-600 hover:bg-gray-100 rounded-md mb-2"
            >
              로그아웃
            </button>
          </li>
        </ul>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

defineProps({
  isOpen: {
    type: Boolean,
    required: true,
  },
})

const emit = defineEmits(['close'])

const authStore = useAuthStore()
const router = useRouter()

const close = () => {
  emit('close')
}

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
    close()
  }
}
</script>

<style scoped>
.slide-right-enter-active,
.slide-right-leave-active {
  transition: transform 0.3s ease-in-out;
}

.slide-right-enter-from,
.slide-right-leave-to {
  transform: translateX(100%);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease-in-out;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
