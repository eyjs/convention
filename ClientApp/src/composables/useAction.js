import { useConventionStore } from '@/stores/convention'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/api'
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { usePopupStore } from '@/stores/popup' // usePopupStore 임포트

export function useAction() {
  const conventionStore = useConventionStore()
  const authStore = useAuthStore()
  const router = useRouter()
  const conventionId = computed(() => conventionStore.currentConvention?.id)
  const popupStore = usePopupStore() // popupStore 인스턴스 생성

  async function fetchChecklist() {
    if (!conventionId.value) {
      console.error('fetchChecklist: Convention ID is not available.')
      return null
    }
    try {
      const response = await apiClient.get(
        `/api/conventions/${conventionId.value}/actions/checklist`,
      )
      return response.data
    } catch (error) {
      console.error('Failed to fetch action checklist:', error)
      return null
    }
  }

  /**
   * BehaviorType에 따라 액션을 실행하는 하이브리드 라우터
   * @param {Object} action - 체크리스트에서 가져온 액션 객체
   */
  async function executeAction(action) {
    if (!action) {
      console.warn('executeAction: action is null or undefined')
      return
    }

    const { behaviorType, targetId, targetModuleId, mapsTo, route, title } =
      action // configJson 제거

    switch (behaviorType) {
      case 'StatusOnly':
        console.log(`Executing StatusOnly action: ${action.id}`)
        break

      case 'GenericForm': // Deprecated, fallback to FormBuilder
        console.warn(
          'Deprecated BehaviorType "GenericForm" used, falling back to "FormBuilder".',
        )
      // Fall-through
      case 'FormBuilder':
        console.log('Executing FormBuilder action:', action)
        console.log('Target ID:', targetId)
        if (targetId) {
          router.push({
            name: 'DynamicFormRenderer',
            params: { formDefinitionId: targetId },
          })
        } else {
          console.warn(
            'FormBuilder action without targetId (FormDefinitionId):',
            action,
          )
        }
        break

      case 'ModuleLink':
        if (mapsTo) {
          router.push(mapsTo)
        } else {
          console.warn('ModuleLink action without mapsTo path:', action)
        }
        break

      case 'Link':
        if (mapsTo) {
          if (mapsTo.startsWith('http://') || mapsTo.startsWith('https://')) {
            window.open(mapsTo, '_blank', 'noopener,noreferrer')
          } else {
            router.push(mapsTo)
          }
        }
        break

      case 'ShowComponentPopup':
        if (mapsTo) {
          // mapsTo를 componentName으로 사용
          popupStore.openPopup({
            title: title, // action의 title을 팝업 제목으로 사용
            componentName: mapsTo, // mapsTo 필드를 컴포넌트 이름으로 사용
            targetId: targetId, // targetId 필드를 컴포넌트 props의 id로 사용
            // modalType 및 options는 필요에 따라 기본값 사용 또는 action에 추가 필드 정의
          })
        } else {
          console.warn(
            'ShowComponentPopup action without mapsTo (componentName):',
            action,
          )
        }
        break

      default:
        console.warn('Unknown action behaviorType:', behaviorType)
    }
  }

  return {
    fetchChecklist,
    executeAction,
  }
}
