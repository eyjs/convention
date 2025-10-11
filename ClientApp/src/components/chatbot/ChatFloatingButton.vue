<template>
  <button
    @click="toggleChat"
    :class="[
      'fixed bottom-20 right-4 z-40 w-14 h-14 rounded-full shadow-float transition-all duration-200 flex items-center justify-center',
      'hover:scale-110 active:scale-95',
      chatStore.isOpen 
        ? 'bg-dark-900 hover:bg-dark-800' 
        : 'bg-gradient-to-br from-primary-500 to-primary-600 hover:from-primary-600 hover:to-primary-700'
    ]"
    :title="chatStore.isOpen ? '채팅 닫기' : 'AI 챗봇'"
  >
    <transition name="icon-fade" mode="out-in">
      <svg 
        v-if="chatStore.isOpen"
        key="close"
        class="w-6 h-6 text-white"
        fill="none" 
        stroke="currentColor" 
        viewBox="0 0 24 24"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>

      <svg 
        v-else
        key="chat"
        class="w-6 h-6 text-white"
        fill="none" 
        stroke="currentColor" 
        viewBox="0 0 24 24"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
      </svg>
    </transition>

    <!-- 읽지 않은 메시지 배지 -->
    <span
      v-if="showBadge && unreadCount > 0"
      class="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs font-bold rounded-full flex items-center justify-center shadow-md"
    >
      {{ unreadCount > 9 ? '9+' : unreadCount }}
    </span>

    <!-- 펄스 애니메이션 -->
    <span
      v-if="!chatStore.isOpen"
      class="absolute inset-0 rounded-full bg-primary-400 opacity-75 animate-ping"
    ></span>
  </button>
</template>

<script setup>
import { computed } from 'vue'
import { useChatStore } from '@/stores/chat'

const props = defineProps({
  showBadge: {
    type: Boolean,
    default: false
  }
})

const chatStore = useChatStore()

const unreadCount = computed(() => {
  return 0 // 실제 구현 시 스토어에서 가져오기
})

function toggleChat() {
  chatStore.toggleChat()
}
</script>

<style scoped>
.icon-fade-enter-active,
.icon-fade-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}

.icon-fade-enter-from {
  opacity: 0;
  transform: scale(0.8) rotate(-90deg);
}

.icon-fade-leave-to {
  opacity: 0;
  transform: scale(0.8) rotate(90deg);
}

@keyframes ping {
  75%, 100% {
    transform: scale(1.2);
    opacity: 0;
  }
}

.animate-ping {
  animation: ping 2s cubic-bezier(0, 0, 0.2, 1) infinite;
}
</style>
