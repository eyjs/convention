import apiClient from './api'

export const chatBotService = {
  ask(question, history, conventionId, userContext) {
    const payload = {
      question: question,
      history: history,
      conventionId,
      role: userContext?.role,
      guestId: userContext?.guestId,
      memberId: userContext?.memberId
    };
    return apiClient.post('/conventionchat/ask', payload);
  },

  async getSuggestedQuestions(conventionId, userContextchatBotService) {
    const params = {
      role: userContext?.role,
      guestId: userContext?.guestId,
    };
    const response = await apiClient.get(`/conventionchat/conventions/${conventionId}/suggestions`, { params });
    return response.data;
  }
};
