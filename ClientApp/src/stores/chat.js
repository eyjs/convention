import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { chatService } from '@/services/chatService'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention' // convention 스토어 임포트

export const useChatStore = defineStore('chat', () => {
    const messages = ref([])
    const loading = ref(false)
    const error = ref(null)
    const suggestedQuestions = ref([])
    const isOpen = ref(false)

    const authStore = useAuthStore()
    const conventionStore = useConventionStore() // convention 스토어 사용

    // convention.js 스토어에서 직접 conventionId를 가져오는 computed 속성
    const conventionId = computed(() => conventionStore.currentConvention?.id)

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

    function resetChatState() {
        messages.value = []
        loading.value = false
        error.value = null
        suggestedQuestions.value = []
        isOpen.value = false
        console.log("Chat store has been successfully reset.");
    }

    async function sendMessage(question) {
        if (!question.trim()) return

        const userMessage = { id: Date.now(), role: 'user', content: question, timestamp: new Date().toISOString() }
        messages.value.push(userMessage)
        loading.value = true
        error.value = null

        try {
            const historyLimit = 10
            const recentMessages = messages.value.slice(-historyLimit - 1, -1).map(msg => ({ role: msg.role, content: msg.content }))
            const userContext = getUserContext()
            
            // 스토어의 conventionId computed 값을 사용
            const response = await chatService.ask(question, recentMessages, conventionId.value, userContext)
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

    async function loadSuggestedQuestions(id) {
        if (!id) return;
        try {
            const userContext = getUserContext();
            const questions = await chatService.getSuggestedQuestions(id, userContext);
            suggestedQuestions.value = questions;
        } catch (err) {
            console.error('Failed to load suggested questions:', err);
            suggestedQuestions.value = ['이번 행사는 언제 진행되나요?', '참석자는 몇 명인가요?'];
        }
    }

    // conventionId가 변경될 때마다 추천 질문을 다시 로드
    watch(conventionId, (newId) => {
        if (newId) {
            loadSuggestedQuestions(newId);
        }
    }, { immediate: true });

    function toggleChat() { isOpen.value = !isOpen.value; }
    function openChat() { isOpen.value = true; }
    function closeChat() { isOpen.value = false; }

    function addWelcomeMessage(conventionTitle = null) {
        const welcomeText = conventionTitle ? `안녕하세요! "${conventionTitle}" 행사에 대해 궁금하신 점을 물어보세요.` : '안녕하세요! 무엇을 도와드릴까요?';
        const welcomeMessage = { id: Date.now(), role: 'assistant', content: welcomeText, timestamp: new Date().toISOString(), isWelcome: true };
        if (messages.value.length === 0) {
            messages.value.push(welcomeMessage);
        }
    }

    return {
        messages,
        loading,
        error,
        suggestedQuestions,
        isOpen,
        conventionId, // conventionId computed 속성 노출
        lastMessage,
        sendMessage,
        loadSuggestedQuestions,
        toggleChat,
        openChat,
        closeChat,
        addWelcomeMessage,
        resetChatState
    }
})