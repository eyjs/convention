<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 메인 컨텐츠 -->
    <main class="pb-20">
      <router-view />
    </main>
    
    <!-- 하단 네비게이션 바 (로그인 페이지에서는 숨김) -->
    <nav v-if="showNav" class="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 shadow-lg z-40">
      <div class="flex items-center justify-around h-16 max-w-screen-xl mx-auto">
        <button
          v-for="item in navItems"
          :key="item.path"
          @click="navigateTo(item.path)"
          :class="[
            'flex flex-col items-center justify-center flex-1 h-full transition-all relative',
            currentRoute === item.path 
              ? 'text-primary-600' 
              : 'text-gray-500 active:scale-95'
          ]"
        >
          <svg 
            class="w-6 h-6 mb-1" 
            :class="currentRoute === item.path ? 'stroke-[2.5]' : 'stroke-2'"
            fill="none" 
            stroke="currentColor" 
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" :d="item.iconPath" />
          </svg>
          <span class="text-xs font-medium">{{ item.label }}</span>
          
          <!-- 활성 인디케이터 -->
          <span 
            v-if="currentRoute === item.path"
            class="absolute top-0 left-1/2 transform -translate-x-1/2 w-12 h-1 bg-primary-600 rounded-b-full"
          ></span>
          
          <!-- 뱃지 (알림) -->
          <span 
            v-if="item.badge > 0"
            class="absolute top-2 right-1/4 w-4 h-4 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center"
          >
            {{ item.badge > 9 ? '9+' : item.badge }}
          </span>
        </button>
      </div>
    </nav>
    
    <!-- 챗봇 -->
    <ChatWindow />
    <ChatFloatingButton />
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import ChatWindow from '@/components/chatbot/ChatWindow.vue'
import ChatFloatingButton from '@/components/chatbot/ChatFloatingButton.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const currentRoute = computed(() => route.path)
const showNav = computed(() => route.path !== '/login')

const navItems = [
  {
    path: '/',
    label: '홈',
    iconPath: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6',
    badge: 0
  },
  {
    path: '/my-schedule',
    label: '일정',
    iconPath: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
    badge: 0
  },
  {
    path: '/board',
    label: '게시판',
    iconPath: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
    badge: 3
  },
  {
    path: '/chat',
    label: '채팅',
    iconPath: 'M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z',
    badge: 5
  },
  {
    path: '/location',
    label: '더보기',
    iconPath: 'M4 6h16M4 12h16M4 18h16',
    badge: 0
  }
]

function navigateTo(path) {
  router.push(path)
}
</script>

<style scoped>
/* iOS Safe Area */
@supports (padding: env(safe-area-inset-bottom)) {
  nav {
    padding-bottom: env(safe-area-inset-bottom);
  }
}

/* 터치 피드백 애니메이션 */
button:active {
  transition: transform 0.1s ease;
}
</style>
