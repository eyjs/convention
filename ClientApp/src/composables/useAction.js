import { useConventionStore } from '@/stores/convention'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/api'
import { computed, defineAsyncComponent } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { usePopupStore } from '@/stores/popup'
import { popupComponents } from '@/popups/popupComponents'

export function useAction() {
  const conventionStore = useConventionStore()
  const authStore = useAuthStore()
  const router = useRouter()
  const route = useRoute()
  const conventionId = computed(() => conventionStore.currentConvention?.id)
  const popupStore = usePopupStore()

  // 행사 컨텍스트 내에서의 경로를 절대 경로로 변환
  function toConventionPath(path) {
    const cid = route.params.conventionId || conventionId.value
    if (!cid) return path
    // 이미 /conventions/ 로 시작하면 그대로 사용
    if (path.startsWith('/conventions/')) return path
    // /feature/xxx → /conventions/:id/feature/xxx
    if (path.startsWith('/')) {
      return `/conventions/${cid}${path}`
    }
    return `/conventions/${cid}/${path}`
  }

  async function fetchChecklist() {
    if (!conventionId.value) {
      console.error('fetchChecklist: Convention ID is not available.')
      return null
    }
    try {
      const response = await apiClient.get(
        `/conventions/${conventionId.value}/actions/checklist`,
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

    const { behaviorType, targetId, mapsTo, title } = action

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
          router.push(toConventionPath(mapsTo))
        } else {
          console.warn('ModuleLink action without mapsTo path:', action)
        }
        break

      case 'Link':
        if (mapsTo) {
          if (mapsTo.startsWith('http://') || mapsTo.startsWith('https://')) {
            window.open(mapsTo, '_blank', 'noopener,noreferrer')
          } else {
            router.push(toConventionPath(mapsTo))
          }
        }
        break

      case 'ShowComponentPopup': {
        // configJson에서 이미지 팝업 URL 확인
        let config = {}
        try {
          config =
            typeof action.configJson === 'string'
              ? JSON.parse(action.configJson)
              : action.configJson || {}
        } catch {
          config = {}
        }

        if (config.popupImageUrl) {
          // 이미지 팝업: BaseModal 대신 popupStore로 이미지 표시
          popupStore.openPopup({
            title: title,
            content: `<div style="text-align:center"><img src="${config.popupImageUrl}" style="max-width:100%;border-radius:8px" alt="${title}" /></div>`,
          })
        } else if (mapsTo && popupComponents[mapsTo]) {
          const component = defineAsyncComponent(popupComponents[mapsTo])
          popupStore.openPopup({
            title: title,
            component: component,
            targetId: targetId,
          })
        } else {
          console.warn(
            `ShowComponentPopup action with invalid config:`,
            action,
          )
        }
        break
      }

      default:
        console.warn('Unknown action behaviorType:', behaviorType)
    }
  }

  return {
    fetchChecklist,
    executeAction,
  }
}
