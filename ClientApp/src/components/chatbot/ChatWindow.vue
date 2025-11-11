<template>
  <div
    v-if="chatStore.isOpen"
    class="fixed inset-0 z-50 flex items-center justify-center p-4"
  >
    <div
      class="absolute inset-0 bg-black/50 backdrop-blur-sm"
      @click="handleClose"
    ></div>

    <div
      class="relative w-full max-w-3xl h-[90vh] bg-white rounded-2xl shadow-2xl flex flex-col overflow-hidden animate-scale-in"
    >
      <div class="flex-shrink-0 bg-white border-b border-gray-200 px-6 py-4">
        <div class="flex items-center justify-between">
          <div class="flex items-center space-x-3">
            <div
              class="w-10 h-10 rounded-full bg-gradient-to-br from-emerald-400 to-cyan-500 flex items-center justify-center shadow-sm"
            >
              <svg
                class="w-6 h-6 text-white"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"
                />
              </svg>
            </div>
            <div>
              <h2 class="text-lg font-semibold text-gray-900">iFA STAR AI</h2>
              <p class="text-xs text-gray-500">
                {{
                  conventionStore.currentConvention?.title ||
                  '무엇을 도와드릴까요?'
                }}
              </p>
            </div>
          </div>

          <div class="flex items-center space-x-1">
            <button
              @click="handleRefresh"
              class="p-2 text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
              title="새 채팅"
            >
              <svg
                class="w-5 h-5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 4v16m8-8H4"
                />
              </svg>
            </button>

            <button
              @click="handleClose"
              class="p-2 text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
              title="닫기"
            >
              <svg
                class="w-5 h-5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- <SuggestedQuestions
        v-if="showSuggestions"
        :questions="chatStore.suggestedQuestions"
        @select="handleSuggestedQuestion"
      /> -->

      <div
        ref="messagesContainer"
        class="flex-1 overflow-y-auto px-6 py-6 bg-white custom-scrollbar"
      >
        <div v-if="chatStore.messages.length > 0" class="space-y-6">
          <ChatMessage
            v-for="message in chatStore.messages"
            :key="message.id"
            :message="message"
            :user-profile-image-url="userProfileImageUrl"
          />
        </div>

        <div
          v-else
          class="h-full flex flex-col items-center justify-center text-center"
        >
          <div
            class="w-20 h-20 mb-6 bg-gradient-to-br from-emerald-400 to-cyan-500 rounded-full flex items-center justify-center shadow-lg"
          >
            <svg
              class="w-10 h-10 text-white"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M13 10V3L4 14h7v7l9-11h-7z"
              />
            </svg>
          </div>
          <h3 class="text-2xl font-semibold text-gray-900 mb-2">
            무엇을 도와드릴까요?
          </h3>
          <p class="text-gray-500 max-w-md">
            행사 일정, 참석자 정보, 장소 등 궁금하신 내용을 자유롭게 물어보세요
          </p>
        </div>

        <div v-if="chatStore.loading" class="flex items-start space-x-3 mb-6">
          <div
            class="w-8 h-8 rounded-full bg-gradient-to-br from-emerald-400 to-cyan-500 flex items-center justify-center shadow-sm flex-shrink-0"
          >
            <svg
              class="w-5 h-5 text-white"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"
              />
            </svg>
          </div>
          <div class="flex-1 bg-gray-100 rounded-2xl px-4 py-3">
            <div class="flex space-x-1.5">
              <div
                class="w-2 h-2 bg-gray-400 rounded-full animate-bounce"
                style="animation-delay: 0ms"
              ></div>
              <div
                class="w-2 h-2 bg-gray-400 rounded-full animate-bounce"
                style="animation-delay: 150ms"
              ></div>
              <div
                class="w-2 h-2 bg-gray-400 rounded-full animate-bounce"
                style="animation-delay: 300ms"
              ></div>
            </div>
          </div>
        </div>
      </div>

      <ChatInput
        ref="chatInputRef"
        :loading="chatStore.loading"
        :placeholder="inputPlaceholder"
        @send="handleSend"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick, onMounted } from 'vue'
import { useChatStore } from '@/stores/chat'
import { useConventionStore } from '@/stores/convention'
import { useAuthStore } from '@/stores/auth'
import ChatMessage from './ChatMessage.vue'
import ChatInput from './ChatInput.vue'
import SuggestedQuestions from './SuggestedQuestions.vue'

const chatStore = useChatStore()
const conventionStore = useConventionStore()
const authStore = useAuthStore()

const messagesContainer = ref(null)
const chatInputRef = ref(null)

const userProfileImageUrl = computed(() => authStore.user?.profileImageUrl)

const showSuggestions = computed(() => {
  return (
    chatStore.messages.length <= 1 && chatStore.suggestedQuestions.length > 0
  )
})

const inputPlaceholder = computed(() => {
  const convention = conventionStore.currentConvention
  return convention
    ? `${convention.title}에 대해 질문하세요...`
    : '메시지를 입력하세요...'
})

async function handleSend(message) {
  await chatStore.sendMessage(message)
  scrollToBottom()
}

async function handleSuggestedQuestion(question) {
  await handleSend(question)
}

function handleRefresh() {
  if (confirm('대화 내용을 모두 삭제하시겠습니까?')) {
    chatStore.clearMessages()

    const conventionTitle = conventionStore.currentConvention?.title
    chatStore.addWelcomeMessage(conventionTitle)
  }
}

function handleClose() {
  chatStore.closeChat()
}

function scrollToBottom() {
  nextTick(() => {
    const container = messagesContainer.value
    if (container) {
      container.scrollTo({
        top: container.scrollHeight,
        behavior: 'smooth',
      })
    }
  })
}

onMounted(() => {
  const conventionId = conventionStore.currentConvention?.id

  if (conventionId) {
    // chatStore.loadSuggestedQuestions(conventionId)

    const conventionTitle = conventionStore.currentConvention?.title
    chatStore.addWelcomeMessage(conventionTitle)
  }
})

watch(
  () => chatStore.messages.length,
  () => {
    scrollToBottom()
  },
)

watch(
  () => conventionStore.currentConvention?.id,
  (newId) => {
    if (newId) {
      // chatStore.loadSuggestedQuestions(newId)
    }
  },
)
</script>

<style scoped>
@keyframes scaleIn {
  from {
    opacity: 0;
    transform: scale(0.95);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.animate-scale-in {
  animation: scaleIn 0.2s ease-out;
}

@keyframes bounce {
  0%,
  100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-4px);
  }
}

.animate-bounce {
  animation: bounce 1s infinite;
}

.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #e5e7eb;
  border-radius: 3px;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #d1d5db;
}
</style>
