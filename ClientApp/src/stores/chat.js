import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { chatService } from '@/services/chatService'
import { useAuthStore } from '@/stores/auth'

export const useChatStore = defineStore('chat', () => {
    const messages = ref([])
    const loading = ref(false)
    const error = ref(null)
    const suggestedQuestions = ref([])
    const isOpen = ref(false)
    const currentConventionId = ref(null)

    const authStore = useAuthStore()

    const lastMessage = computed(() => messages.value.length > 0 ? messages.value[messages.value.length - 1] : null)

    function getUserContext() {
        const user = authStore.user
        if (!user) {
            console.warn("getUserContext: User is not logged in, context will be null.")
            return null
        }

        return {
            role: user.role,
            guestId: user.guestId || user.id,
            memberId: user.memberId
        }
    }

    async function sendMessage(question) {
        if (!question.trim()) return

        const userMessage = { id: Date.now(), role: 'user', content: question, timestamp: new Date().toISOString() }
        messages.value.push(userMessage)
        loading.value = true
        error.value = null

        try {
            const userContext = getUserContext()
            const response = await chatService.ask(question, currentConventionId.value, userContext)
            const data = response.data;

            const assistantMessage = {
                id: Date.now() + 1,
                role: 'assistant',
                content: data?.answer || '답변을 생성할 수 없습니다.',
                sources: data?.sources || [],
                llmProvider: data?.llmProvider,
                timestamp: data?.timestamp || new Date().toISOString()
            }
            messages.value.push(assistantMessage)

        } catch (err) {
            console.error('Failed to send message:', err)
            const errorMessage = { id: Date.now() + 1, role: 'assistant', content: '죄송합니다. 답변을 생성하는 중 문제가 발생했습니다.', isError: true, timestamp: new Date().toISOString() }
            messages.value.push(errorMessage)
        } finally {
            loading.value = false
        }
    }

    async function sendMessageWithHistory(question) {
        if (!question.trim()) return

        const userMessage = { id: Date.now(), role: 'user', content: question, timestamp: new Date().toISOString() }
        messages.value.push(userMessage)
        loading.value = true
        error.value = null

        try {
            const historyLimit = 10
            const recentMessages = messages.value.slice(-historyLimit).map(msg => ({ role: msg.role, content: msg.content }))
            const userContext = getUserContext()
            const response = await chatService.askWithHistory(question, recentMessages, currentConventionId.value, userContext)
            const data = response.data;

            const assistantMessage = {
                id: Date.now() + 1,
                role: 'assistant',
                content: data?.answer || '답변을 생성할 수 없습니다.',
                sources: data?.sources || [],
                llmProvider: data?.llmProvider,
                timestamp: data?.timestamp || new Date().toISOString()
            }
            messages.value.push(assistantMessage)

        } catch (err) {
            console.error('Failed to send message with history:', err)
            const errorMessage = { id: Date.now() + 1, role: 'assistant', content: '죄송합니다. 답변을 생성하는 중 문제가 발생했습니다.', isError: true, timestamp: new Date().toISOString() }
            messages.value.push(errorMessage)
        } finally {
            loading.value = false
        }
    }

    async function loadSuggestedQuestions(conventionId) {
        try {
            const userContext = getUserContext();
            const questions = await chatService.getSuggestedQuestions(conventionId, userContext);
            suggestedQuestions.value = questions;
        } catch (err) {
            console.error('Failed to load suggested questions:', err);
            suggestedQuestions.value = ['이번 행사는 언제 진행되나요?', '참석자는 몇 명인가요?'];
        }
    }

    function toggleChat() { isOpen.value = !isOpen.value; }
    function openChat() { isOpen.value = true; }
    function closeChat() { isOpen.value = false; }
    function clearMessages() { messages.value = []; error.value = null; }

    function setConventionContext(conventionId) {
        currentConventionId.value = conventionId;
        if (conventionId) {
            loadSuggestedQuestions(conventionId);
        }
    }

    function addWelcomeMessage(conventionTitle = null) {
        const welcomeText = conventionTitle ? `안녕하세요! "${conventionTitle}" 행사에 대해 궁금하신 점을 물어보세요.` : '안녕하세요! 무엇을 도와드릴까요?';
        const welcomeMessage = { id: Date.now(), role: 'assistant', content: welcomeText, timestamp: new Date().toISOString(), isWelcome: true };
        if (messages.value.length === 0) {
            messages.value.push(welcomeMessage);
        }
    }

    // 👇 --- [핵심 수정] 누락되었던 함수들을 return 문에 모두 추가합니다. --- 👇
    return {
        messages,
        loading,
        error,
        suggestedQuestions,
        isOpen,
        currentConventionId,
        lastMessage,
        sendMessage,
        sendMessageWithHistory,
        loadSuggestedQuestions,
        toggleChat,
        openChat,
        closeChat,
        clearMessages,
        setConventionContext,
        addWelcomeMessage
    }
})