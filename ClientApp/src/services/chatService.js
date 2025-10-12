import apiClient from './api'

export const chatService = {
    ask(question, conventionId, userContext) {
        const payload = {
            question,
            conventionId,
            role: userContext?.role,
            guestId: userContext?.guestId,
            memberId: userContext?.memberId
        };
        return apiClient.post('/conventionchat/ask', payload);
    },

    askWithHistory(question, history, conventionId, userContext) {
        const payload = {
            question,
            history,
            conventionId,
            role: userContext?.role,
            guestId: userContext?.guestId,
            memberId: userContext?.memberId
        };
        return apiClient.post('/conventionchat/ask/with-history', payload);
    },

    async getSuggestedQuestions(conventionId, userContext) {
        const params = {
            role: userContext?.role,
            guestId: userContext?.guestId,
        };
        const response = await apiClient.get(`/conventionchat/conventions/${conventionId}/suggestions`, { params });
        return response.data;
    }
};