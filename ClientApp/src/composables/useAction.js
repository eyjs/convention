import { useConventionStore } from '@/stores/convention'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/api'
import { computed } from 'vue'

export function useAction() {
  const conventionStore = useConventionStore()
  const authStore = useAuthStore()
  const conventionId = computed(() => conventionStore.currentConvention?.id)

  async function submitAction(actionType, responseData) {
    if (!conventionId.value) {
      throw new Error('Convention not selected.')
    }

    const payload = {
      ResponseDataJson: JSON.stringify(responseData),
    }

    await apiClient.post(
      `/conventions/${conventionId.value}/actions/${actionType}/complete`,
      payload,
    )

    // Refresh user data to update checklist status
    await authStore.fetchCurrentUser()
  }

  return {
    submitAction,
  }
}
