import apiClient from './api'

export const chatService = {
    /**
     * ê������ ������ �����ϴ�.
     * @param {string} question - ������� ����
     * @param {number|null} conventionId - ���� ������ ID
     * @param {object|null} userContext - ����� ���ؽ�Ʈ (role, guestId ��)
     * @returns {Promise}
     */
    ask(question, conventionId, userContext) {
        // [�ٽ� ����] userContext�� ��ġ�� ���, ��������� �Ӽ��� �Ҵ��մϴ�.
        // �̷��� �ϸ� �� ���ε� ���� ���ɼ��� ��õ������ �����մϴ�.
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
     * ��ȭ ����� �����Ͽ� ������ �����ϴ�.
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
     * ��õ ���� ����� �����ɴϴ�.
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