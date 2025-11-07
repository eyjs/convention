import { useConventionStore } from '@/stores/convention'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/api'
import { computed } from 'vue'
import { useRouter } from 'vue-router'

export function useAction() {
  const conventionStore = useConventionStore()
  const authStore = useAuthStore()
  const router = useRouter()
  const conventionId = computed(() => conventionStore.currentConvention?.id)

  async function submitAction(actionId, responseData) {
    if (!conventionId.value) {
      throw new Error('Convention not selected.')
    }

    const payload = {
      ResponseDataJson: JSON.stringify(responseData),
    }

    await apiClient.post(
      `/conventions/${conventionId.value}/actions/${actionId}/complete`,
      payload,
    )

    // Refresh user data to update checklist status
    await authStore.fetchCurrentUser()
  }

  /**
   * BehaviorType에 따라 액션을 실행하는 하이브리드 라우터
   * @param {Object} action - 액션 객체 (behaviorType, id, targetModuleId, mapsTo 등)
   */
  async function executeAction(action) {
    if (!action) {
      console.warn('executeAction: action is null or undefined')
      return
    }

    // BehaviorType 기본값 처리 (없으면 StatusOnly로 간주)
    const behaviorType = action.behaviorType || 'StatusOnly'

    switch (behaviorType) {
      case 'StatusOnly':
        // 기존 방식: 단순 완료 처리 또는 MapsTo 경로로 이동
        if (action.mapsTo) {
          router.push(action.mapsTo)
        } else {
          console.log(`Executing StatusOnly action: ${action.id}`)
          // 필요시 바로 완료 API 호출
          // await submitAction(action.actionType, {})
        }
        break

      case 'GenericForm':
        // 범용 폼: GenericForm 뷰로 라우팅
        router.push({
          name: 'GenericForm',
          params: { actionId: action.id }
        })
        break

      case 'ModuleLink':
        // 모듈 연동 (예: 설문조사)
        if (action.targetModuleId) {
          // 설문조사 모듈로 이동
          router.push({
            name: 'Survey',
            params: { id: action.targetModuleId }
          })
        } else {
          console.warn('ModuleLink action without targetModuleId:', action)
        }
        break

      case 'Link':
        // 링크 처리 (외부/내부)
        if (action.mapsTo) {
          if (action.mapsTo.startsWith('http://') || action.mapsTo.startsWith('https://')) {
            // 외부 링크
            window.open(action.mapsTo, '_blank')
          } else {
            // 내부 링크
            router.push(action.mapsTo)
          }
        }
        break

      default:
        console.warn('Unknown action behaviorType:', behaviorType)
    }
  }

  return {
    submitAction,
    executeAction,
  }
}
