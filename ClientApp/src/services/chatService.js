import apiClient from './api'

export const chatService = {
    /**
     * 챗봇에게 질문을 보냅니다.
     * @param {string} question - 사용자의 질문
     * @param {number|null} conventionId - 현재 컨벤션 ID
     * @param {object|null} userContext - 사용자 컨텍스트 (role, guestId 등)
     * @returns {Promise}
     */
    ask(question, conventionId, userContext) {
        // [핵심 수정] userContext를 펼치는 대신, 명시적으로 속성을 할당합니다.
        // 이렇게 하면 모델 바인딩 실패 가능성을 원천적으로 차단합니다.
        const payload = {
            question,
            conventionId,
            role: userContext?.role,
            guestId: userContext?.guestId,
            memberId: userContext?.memberId
        };
        return apiClient.post('/conventionchat/ask', payload);
    },

    /**
     * 대화 기록을 포함하여 질문을 보냅니다.
     */
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

    /**
     * 추천 질문 목록을 가져옵니다.
     */
    async getSuggestedQuestions(conventionId, userContext) {
        const params = {
            role: userContext?.role,
            guestId: userContext?.guestId,
        };
        const response = await apiClient.get(`/conventionchat/conventions/${conventionId}/suggestions`, { params });
        return response.data;
    }
};