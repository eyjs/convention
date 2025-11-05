<template>
  <div class="chat-container">
    <div class="chat-header">
      <div>
        <h2 class="font-bold text-lg">{{ conventionTitle }}</h2>
        <button
          @click="showParticipantList = true"
          class="text-sm text-gray-600 hover:underline"
        >
          {{ participantCount }}명 참여중
        </button>
      </div>
      <button
        @click="uiStore.closeChat()"
        class="p-2 -mr-2 text-gray-500 hover:text-gray-800"
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
            d="M6 18L18 6M6 6l12 12"
          ></path>
        </svg>
      </button>
    </div>
    <div class="messages-area" ref="messagesArea">
      <div
        v-for="(msg, index) in messages"
        :key="index"
        class="message-wrapper"
        :class="{
          'my-message': isMyMessage(msg),
          'other-message': !isMyMessage(msg),
        }"
      >
        <div class="message" :class="{ 'is-admin': msg.isAdmin }">
          <div class="message-header">
            <span
              v-if="msg.isAdmin"
              class="mr-2 inline-flex items-center justify-center h-4 w-4 rounded-full bg-blue-600 text-white"
            >
              <svg
                class="w-3 h-3"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="3"
                  d="M5 13l4 4L19 7"
                ></path>
              </svg>
            </span>
            <span class="font-bold">{{ msg.userName }}</span>
            <span class="text-xs text-gray-500 ml-2">{{
              formatTime(msg.createdAt)
            }}</span>
          </div>
          <div class="message-content">
            <p>{{ msg.message }}</p>
          </div>
        </div>
      </div>
    </div>
    <div class="input-area">
      <form @submit.prevent="handleSendMessage" class="flex">
        <input
          type="text"
          v-model="newMessage"
          placeholder="메시지를 입력하세요..."
          @keydown.enter.prevent="handleSendMessage"
          class="flex-grow border rounded-l-md p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
          :disabled="!isConnected"
        />
        <button
          type="submit"
          class="bg-blue-500 text-white px-4 py-2 rounded-r-md hover:bg-blue-600 disabled:bg-gray-400"
          :disabled="!isConnected || newMessage.length === 0"
        >
          전송
        </button>
      </form>
    </div>

    <Transition name="modal-fade">
      <ParticipantList
        v-if="showParticipantList"
        :participants="participantList"
        @close="showParticipantList = false"
      />
    </Transition>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, nextTick, computed } from 'vue'
import ConventionChatService from '@/services/conventionChatService.js'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import { useUIStore } from '@/stores/ui'
import ParticipantList from './ParticipantList.vue'
import { chatAPI } from '@/services/api'

const props = defineProps({
  conventionId: {
    type: [String, Number],
    required: true,
  },
  token: {
    type: String,
    required: true,
  },
})

const authStore = useAuthStore()
const conventionStore = useConventionStore()
const uiStore = useUIStore()

const messages = ref([])
const newMessage = ref('')
const isConnected = ref(false)
const participantCount = ref(0)
const participantList = ref([])
const showParticipantList = ref(false)
const messagesArea = ref(null)
let chatService = null

const conventionTitle = computed(
  () => conventionStore.currentConvention?.title || '채팅',
)

const isMyMessage = (msg) => {
  if (!authStore.user || msg.userId === undefined) {
    return false
  }
  return msg.userId === authStore.user.id
}

onMounted(async () => {
  // Mark chat as read
  chatAPI
    .markAsRead(props.conventionId)
    .then(() => {
      // 읽음 처리 성공 시 로컬 unread count도 초기화
      authStore.resetUnreadCount(props.conventionId)
    })
    .catch((err) => console.error('Failed to mark chat as read:', err))

  if (props.conventionId && props.token) {
    chatService = new ConventionChatService(props.conventionId, props.token)

    try {
      const history = await chatService.getHistory()
      messages.value = history
      scrollToBottom()

      await chatService.connect()
      isConnected.value = true
      console.log('Chat service connected successfully.')

      chatService.onReceiveMessage((messageData) => {
        messages.value.push(messageData)
        scrollToBottom()
      })

      chatService.onUpdateParticipantCount((count) => {
        participantCount.value = count
      })

      chatService.onUpdateParticipantList((list) => {
        participantList.value = list
      })
    } catch (error) {
      console.error('Failed to initialize chat service:', error)
    }
  } else {
    console.error('Convention ID or token is missing.')
  }
})

onUnmounted(() => {
  if (chatService) {
    chatService.disconnect()
    isConnected.value = false
    console.log('Chat service disconnected.')
  }
})

const handleSendMessage = () => {
  if (newMessage.value.trim() && chatService) {
    chatService
      .sendMessage(newMessage.value)
      .then(() => {
        newMessage.value = ''
      })
      .catch((err) => console.error('Send message error:', err))
  }
}

const scrollToBottom = () => {
  nextTick(() => {
    if (messagesArea.value) {
      messagesArea.value.scrollTop = messagesArea.value.scrollHeight
    }
  })
}

const formatTime = (isoString) => {
  const date = new Date(isoString)
  return date.toLocaleTimeString('ko-KR', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })
}
</script>

<style scoped>
.chat-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-bottom: 1px solid #e2e8f0;
  background-color: #ffffff;
}

.messages-area {
  flex-grow: 1;
  padding: 1rem;
  overflow-y: auto;
  background-color: #f7fafc;
}

.message-wrapper {
  display: flex;
  flex-direction: column;
  margin-bottom: 1rem;
}

.message {
  max-width: 80%;
}

.my-message {
  align-items: flex-end;
}

.other-message {
  align-items: flex-start;
}

.message.is-admin .message-header .font-bold {
  color: #2563eb; /* blue-600 */
  font-weight: bold;
}

.message-header {
  margin-bottom: 0.25rem;
}

.my-message .message-content {
  background-color: #fef08a; /* yellow-200 */
  color: black;
}

.other-message .message-content {
  background-color: white;
  color: black;
}

.message-content {
  padding: 0.75rem;
  border-radius: 0.375rem;
  display: inline-block;
}

.input-area {
  padding: 1rem;
  border-top: 1px solid #e2e8f0;
  background-color: #ffffff;
}

.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: all 0.3s ease-in-out;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}
.modal-fade-enter-active .bg-white,
.modal-fade-leave-active .bg-white {
  transition: all 0.3s ease-in-out;
}
.modal-fade-enter-from .bg-white,
.modal-fade-leave-to .bg-white {
  transform: scale(0.95);
}
</style>
