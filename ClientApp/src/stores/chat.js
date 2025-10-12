import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { chatService } from '@/services/chatService'
import { useAuthStore } from '@/stores/auth' // 👈 authService 대신 authStore를 임포트합니다.

export const useChatStore = defineStore('chat', () => {
    const messages = ref([])
    const loading = ref(false)
    const error = ref(null)
    const suggestedQuestions = ref([])
    const isOpen = ref(false)
    const currentConventionId = ref(null)

    const authStore = useAuthStore() // 👈 authStore 인스턴스를 생성합니다.

    const lastMessage = computed(() => {
        return messages.value.length > 0
            ? messages.value[messages.value.length - 1]
            : null
    })

    const userMessages = computed(() => {
        return messages.value.filter(msg => msg.role === 'user')
    })

    const assistantMessages = computed(() => {
        return messages.value.filter(msg => msg.role === 'assistant')
    })

    // 👇 --- 여기가 핵심 수정 부분입니다 --- 👇
    function getUserContext() {
        // authService.getCurrentUser() 대신 authStore의 user 상태를 직접 사용합니다.
        const user = authStore.user
        if (!user) return null

        return {
            role: user.role,
            guestId: user.guestId,
            companionId: user.companionId,
            memberId: user.memberId
        }
    }
    // 👆 --- 여기까지 --- 👆

    async function sendMessage(question) {
        if (!question.trim()) return

        const userMessage = {
            id: Date.now(),
            role: 'user',
            content: question,
            timestamp: new Date().toISOString()
        }
        messages.value.push(userMessage)

        loading.value = true
        error.value = null

        try {
            const userContext = getUserContext()
            const response = await chatService.ask(question, currentConventionId.value, userContext)

            const assistantMessage = {
                id: Date.now() + 1,
                role: 'assistant',
                content: response.data?.answer || '답변을 생성할 수 없습니다.',
                sources: response.data?.sources || [],
                llmProvider: response.data?.llmProvider,
                timestamp: response.data?.timestamp || new Date().toISOString()
            }
            messages.value.push(assistantMessage)

        } catch (err) {
            error.value = '답변을 생성하는 중 오류가 발생했습니다.'
            console.error('Failed to send message:', err)

            const errorMessage = {
                id: Date.now() + 1,
                role: 'assistant',
                content: '죄송합니다. 답변을 생성하는 중 문제가 발생했습니다. 다시 시도해주세요.',
                isError: true,
                timestamp: new Date().toISOString()
            }
            messages.value.push(errorMessage)

        } finally {
            loading.value = false
        }
    }

    async function sendMessageAboutConvention(conventionId, question) {
        if (!question.trim()) return

        const userMessage = {
            id: Date.now(),
            role: 'user',
            content: question,
            timestamp: new Date().toISOString()
        }
        messages.value.push(userMessage)

        loading.value = true
        error.value = null

        try {
            const userContext = getUserContext()
            const response = await chatService.askAboutConvention(conventionId, question, userContext)

            const assistantMessage = {
                id: Date.now() + 1,
                role: 'assistant',
                content: response.answer,
                sources: response.sources || [],
                llmProvider: response.llmProvider,
                timestamp: response.timestamp || new Date().toISOString()
            }
            messages.value.push(assistantMessage)

        } catch (err) {
            error.value = '답변을 생성하는 중 오류가 발생했습니다.'
            console.error('Failed to send message about convention:', err)

            const errorMessage = {
                id: Date.now() + 1,
                role: 'assistant',
                content: '죄송합니다. 해당 행사 정보를 찾을 수 없습니다.',
                isError: true,
                timestamp: new Date().toISOString()
            }
            messages.value.push(errorMessage)

        } finally {
            loading.value = false
        }
    }

    async function sendMessageWithHistory(question) {
        if (!question.trim()) return

        const userMessage = {
            id: Date.now(),
            role: 'user',
            content: question,
            timestamp: new Date().toISOString()
        }
        messages.value.push(userMessage)

        loading.value = true
        error.value = null

        try {
            const historyLimit = 10
            const recentMessages = messages.value
                .slice(-historyLimit)
                .map(msg => ({
                    role: msg.role,
                    content: msg.content
                }))

            const userContext = getUserContext()
            const response = await chatService.askWithHistory(
                question,
                recentMessages,
                currentConventionId.value,
                userContext
            )

            const assistantMessage = {
                id: Date.now() + 1,
                role: 'assistant',
                content: response.data?.answer || '답변을 생성할 수 없습니다.',
                sources: response.data?.sources || [],
                llmProvider: response.data?.llmProvider,
                timestamp: response.data?.timestamp || new Date().toISOString()
            }
            messages.value.push(assistantMessage)

        } catch (err) {
            error.value = '답변을 생성하는 중 오류가 발생했습니다.'
            console.error('Failed to send message with history:', err)

            const errorMessage = {
                id: Date.now() + 1,
                role: 'assistant',
                content: '죄송합니다. 답변을 생성하는 중 문제가 발생했습니다.',
                isError: true,
                timestamp: new Date().toISOString()
            }
            messages.value.push(errorMessage)

        } finally {
            loading.value = false
        }
    }

    async function loadSuggestedQuestions(conventionId) {
        try {
            const userContext = getUserContext()
            const questions = await chatService.getSuggestedQuestions(conventionId, userContext)
            suggestedQuestions.value = questions
        } catch (err) {
            console.error('Failed to load suggested questions:', err)
            suggestedQuestions.value = [
                '이번 행사는 언제 진행되나요?',
                '참석자는 몇 명인가요?',
                '행사 일정을 알려주세요',
                '담당자 연락처를 알려주세요'
            ]
        }
    }

    async function selectSuggestedQuestion(question) {
        await sendMessage(question)
    }

    function toggleChat() {
        isOpen.value = !isOpen.value
    }

    function openChat() {
        isOpen.value = true
    }

    function closeChat() {
        isOpen.value = false
    }

    function clearMessages() {
        messages.value = []
        error.value = null
    }

    function setConventionContext(conventionId) {
        currentConventionId.value = conventionId
        if (conventionId) {
            loadSuggestedQuestions(conventionId)
        }
    }

    function addWelcomeMessage(conventionTitle = null) {
        const welcomeText = conventionTitle
            ? `안녕하세요! "${conventionTitle}" 행사에 대해 궁금하신 점을 물어보세요.`
            : '안녕하세요! 행사에 대해 궁금하신 점을 물어보세요.'

        const welcomeMessage = {
            id: Date.now(),
            role: 'assistant',
            content: welcomeText,
            timestamp: new Date().toISOString(),
            isWelcome: true
        }

        if (messages.value.length === 0) {
            messages.value.push(welcomeMessage)
        }
    }

    return {
        messages,
        loading,
        error,
        suggestedQuestions,
        isOpen,
        currentConventionId,
        lastMessage,
        userMessages,
        assistantMessages,
        sendMessage,
        sendMessageAboutConvention,
        sendMessageWithHistory,
        loadSuggestedQuestions,
        selectSuggestedQuestion,
        toggleChat,
        openChat,
        closeChat,
        clearMessages,
        setConventionContext,
        addWelcomeMessage
    }
})