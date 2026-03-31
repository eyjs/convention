import {
  CheckSquare,
  Menu,
  Image,
  Info,
  Bell,
  Settings,
  MousePointerClick,
} from 'lucide-vue-next'
import { popupComponents } from '@/popups/popupComponents'

// 등록된 팝업 컴포넌트 목록을 드롭다운 옵션으로 변환
const popupComponentOptions = Object.keys(popupComponents).map((key) => ({
  value: key,
  label: key,
}))

/**
 * Action Presets — 관리자가 쉽게 액션을 생성할 수 있는 프리셋 정의
 *
 * 각 프리셋은 actionCategory, behaviorType, targetLocation 등을 자동 설정하고
 * 최소한의 입력만 받아 payload를 생성합니다.
 */
export const ACTION_PRESETS = [
  {
    id: 'checklist',
    label: '체크리스트',
    description: '참석자 할일 체크 항목 (다중 추가/수정)',
    icon: CheckSquare,
    isBulk: true,
    formFields: [],
    buildPayload() {
      return null // bulk 모드에서는 사용하지 않음
    },
    buildRowPayload(row, conventionId, orderNum) {
      return {
        conventionId,
        title: row.title,
        actionCategory: 'CHECKLIST_CARD',
        targetLocation: row.targetLocation || 'HOME_CONTENT_TOP',
        behaviorType: 'StatusOnly',
        deadline: row.deadline || null,
        isActive: row.isActive ?? true,
        orderNum,
        mapsTo: null,
        targetId: null,
        configJson: null,
      }
    },
  },
  {
    id: 'link-menu',
    label: '링크 메뉴',
    description: '더보기에 메뉴 항목 추가',
    icon: Menu,
    formFields: [
      { key: 'title', label: '메뉴 이름', type: 'text', required: true },
      {
        key: 'linkType',
        label: '링크 유형',
        type: 'select',
        options: [
          { value: 'internal', label: '내부 페이지 (모듈)' },
          { value: 'external', label: '외부 링크' },
        ],
        default: 'internal',
      },
      {
        key: 'mapsTo',
        label: 'URL',
        type: 'text',
        required: true,
        placeholder: '내부: surveys/2  |  외부: https://...',
      },
    ],
    buildPayload(formData, conventionId) {
      const isExternal = formData.linkType === 'external'
      return {
        conventionId,
        title: formData.title,
        actionCategory: 'MENU',
        targetLocation: 'MORE_FEATURES_GRID',
        behaviorType: isExternal ? 'Link' : 'ModuleLink',
        deadline: null,
        isActive: true,
        orderNum: formData.orderNum ?? 0,
        mapsTo: isExternal
          ? formData.mapsTo
          : '/feature/' + formData.mapsTo.replace(/^\/?(feature\/)?/, ''),
        targetId: null,
        configJson: null,
      }
    },
  },
  {
    id: 'banner',
    label: '배너',
    description: '페이지에 이미지 배너 표시',
    icon: Image,
    formFields: [
      {
        key: 'title',
        label: '배너 제목 (관리용)',
        type: 'text',
        required: true,
      },
      {
        key: 'imageUrl',
        label: '배너 이미지',
        type: 'image',
        required: true,
      },
      {
        key: 'height',
        label: '배너 높이',
        type: 'select',
        options: [
          { value: 'sm', label: '작게' },
          { value: 'md', label: '보통' },
          { value: 'lg', label: '크게' },
        ],
        default: 'md',
      },
      {
        key: 'linkType',
        label: '클릭 시 동작',
        type: 'select',
        options: [
          { value: 'none', label: '없음' },
          { value: 'internal', label: '내부 페이지 이동' },
          { value: 'external', label: '외부 링크 열기' },
        ],
        default: 'none',
      },
      {
        key: 'linkUrl',
        label: '이동 URL',
        type: 'text',
        showIf: (fd) => fd.linkType !== 'none',
        placeholder: '내부: /feature/... | 외부: https://...',
      },
      {
        key: 'targetLocation',
        label: '표시 위치',
        type: 'select',
        options: [
          { value: 'HOME_SUB_HEADER', label: '홈 - 배너 아래' },
          { value: 'HOME_CONTENT_TOP', label: '홈 - 콘텐츠 상단' },
          { value: 'SCHEDULE_CONTENT_TOP', label: '일정 - 상단' },
          { value: 'BOARD_CONTENT_TOP', label: '게시판 - 상단' },
          { value: 'PARTICIPANTS_TOP', label: '참석자 - 상단' },
        ],
        default: 'HOME_SUB_HEADER',
      },
    ],
    buildPayload(formData, conventionId) {
      const config = {
        imageUrl: formData.imageUrl,
        height: formData.height || 'md',
      }
      // 배너의 클릭 동작은 configJson.url / configJson.externalUrl로 전달
      if (formData.linkType === 'internal' && formData.linkUrl) {
        config.url = formData.linkUrl
      } else if (formData.linkType === 'external' && formData.linkUrl) {
        config.externalUrl = formData.linkUrl
      }

      return {
        conventionId,
        title: formData.title,
        actionCategory: 'BANNER',
        targetLocation: formData.targetLocation || 'HOME_SUB_HEADER',
        behaviorType: 'StatusOnly',
        deadline: null,
        isActive: true,
        orderNum: formData.orderNum ?? 0,
        // GenericBanner는 configJson.url로 네비게이션하므로 mapsTo에도 동일 값
        mapsTo: config.url || config.externalUrl || null,
        targetId: null,
        configJson: JSON.stringify(config),
      }
    },
  },
  {
    id: 'info-card',
    label: '안내 카드',
    description: '정보 카드 표시',
    icon: Info,
    formFields: [
      { key: 'title', label: '카드 제목', type: 'text', required: true },
      {
        key: 'targetLocation',
        label: '표시 위치',
        type: 'select',
        options: [
          { value: 'HOME_SUB_HEADER', label: '홈 - 배너 아래' },
          { value: 'HOME_CONTENT_TOP', label: '홈 - 콘텐츠 상단' },
          { value: 'SCHEDULE_CONTENT_TOP', label: '일정 - 상단' },
        ],
        default: 'HOME_CONTENT_TOP',
      },
      {
        key: 'linkType',
        label: '클릭 시 동작',
        type: 'select',
        options: [
          { value: 'none', label: '없음' },
          { value: 'module', label: '내부 모듈 이동' },
          { value: 'link', label: '외부 링크' },
        ],
        default: 'none',
      },
      {
        key: 'mapsTo',
        label: 'URL',
        type: 'text',
        showIf: (fd) => fd.linkType !== 'none',
        placeholder: '모듈: surveys/2  |  링크: https://...',
      },
    ],
    buildPayload(formData, conventionId) {
      let behaviorType = 'StatusOnly'
      let mapsTo = null
      if (formData.linkType === 'module' && formData.mapsTo) {
        behaviorType = 'ModuleLink'
        mapsTo = '/feature/' + formData.mapsTo.replace(/^\/?(feature\/)?/, '')
      } else if (formData.linkType === 'link' && formData.mapsTo) {
        behaviorType = 'Link'
        mapsTo = formData.mapsTo
      }
      return {
        conventionId,
        title: formData.title,
        actionCategory: 'CARD',
        targetLocation: formData.targetLocation || 'HOME_CONTENT_TOP',
        behaviorType,
        deadline: null,
        isActive: true,
        orderNum: formData.orderNum ?? 0,
        mapsTo,
        targetId: null,
        configJson: null,
      }
    },
  },
  {
    id: 'popup',
    label: '팝업 공지',
    description: '자동 팝업으로 안내 표시',
    icon: Bell,
    formFields: [
      { key: 'title', label: '팝업 제목', type: 'text', required: true },
      {
        key: 'message',
        label: '팝업 내용 (HTML)',
        type: 'html',
        required: true,
      },
      {
        key: 'size',
        label: '팝업 크기',
        type: 'select',
        options: [
          { value: 'sm', label: '작게' },
          { value: 'md', label: '보통' },
          { value: 'lg', label: '크게' },
        ],
        default: 'md',
      },
      {
        key: 'showOnce',
        label: '"다시 보지 않기" 옵션 표시',
        type: 'checkbox',
        default: true,
      },
      {
        key: 'buttonLabel',
        label: '버튼 텍스트',
        type: 'text',
        placeholder: '확인 (비워두면 버튼 없음)',
      },
      {
        key: 'buttonLinkType',
        label: '버튼 클릭 동작',
        type: 'select',
        showIf: (fd) => !!fd.buttonLabel,
        options: [
          { value: 'close', label: '팝업 닫기' },
          { value: 'internal', label: '내부 페이지 이동' },
          { value: 'external', label: '외부 링크 열기' },
        ],
        default: 'close',
      },
      {
        key: 'buttonUrl',
        label: '이동할 페이지',
        type: 'select',
        showIf: (fd) => !!fd.buttonLabel && fd.buttonLinkType === 'internal',
        options: [
          { value: 'schedule', label: '나의 일정' },
          { value: 'board', label: '게시판' },
          { value: 'more', label: '더보기' },
        ],
        default: 'schedule',
      },
      {
        key: 'buttonExternalUrl',
        label: '외부 링크 URL',
        type: 'text',
        showIf: (fd) => !!fd.buttonLabel && fd.buttonLinkType === 'external',
        placeholder: 'https://...',
      },
    ],
    buildPayload(formData, conventionId) {
      const config = {
        message: formData.message,
        size: formData.size || 'md',
        showOnce: formData.showOnce ?? true,
        trigger: 'onPageLoad',
        delay: 500,
        dismissOnBackdrop: true,
      }
      // 버튼 설정
      if (formData.buttonLabel) {
        const btn = {
          label: formData.buttonLabel,
          style: 'primary',
          closeOnClick: true,
        }
        if (formData.buttonLinkType === 'internal' && formData.buttonUrl) {
          btn.url = `/conventions/${conventionId}/${formData.buttonUrl}`
        } else if (
          formData.buttonLinkType === 'external' &&
          formData.buttonExternalUrl
        ) {
          btn.externalUrl = formData.buttonExternalUrl
        }
        config.buttons = [btn]
      }

      return {
        conventionId,
        title: formData.title,
        actionCategory: 'AUTO_POPUP',
        targetLocation: 'GLOBAL_ROOT_POPUP',
        behaviorType: 'StatusOnly',
        deadline: null,
        isActive: true,
        orderNum: formData.orderNum ?? 0,
        mapsTo: null,
        targetId: null,
        configJson: JSON.stringify(config),
      }
    },
  },
  {
    id: 'button',
    label: '동적 버튼',
    description: '페이지에 버튼 추가 (링크, 이미지 팝업 등)',
    icon: MousePointerClick,
    formFields: [
      { key: 'title', label: '버튼 텍스트', type: 'text', required: true },
      {
        key: 'targetLocation',
        label: '표시 위치',
        type: 'select',
        options: [
          { value: 'HOME_SUB_HEADER', label: '홈 - 배너 아래' },
          { value: 'HOME_CONTENT_TOP', label: '홈 - 콘텐츠 상단' },
          { value: 'SCHEDULE_CONTENT_TOP', label: '일정 - 상단' },
        ],
        default: 'HOME_SUB_HEADER',
      },
      {
        key: 'actionType',
        label: '클릭 시 동작',
        type: 'select',
        options: [
          { value: 'internal', label: '내부 페이지 이동' },
          { value: 'external', label: '외부 링크 열기' },
          { value: 'imagePopup', label: '이미지 팝업' },
          ...(popupComponentOptions.length > 0
            ? [{ value: 'component', label: '컴포넌트 팝업 (개인화)' }]
            : []),
        ],
        default: 'internal',
      },
      {
        key: 'internalPage',
        label: '이동할 페이지',
        type: 'select',
        showIf: (fd) => fd.actionType === 'internal',
        options: [
          { value: 'schedule', label: '나의 일정' },
          { value: 'board', label: '게시판' },
          { value: 'more', label: '더보기' },
        ],
        default: 'schedule',
      },
      {
        key: 'externalUrl',
        label: '외부 링크 URL',
        type: 'text',
        showIf: (fd) => fd.actionType === 'external',
        required: true,
        placeholder: 'https://...',
      },
      {
        key: 'popupImage',
        label: '팝업 이미지',
        type: 'image',
        showIf: (fd) => fd.actionType === 'imagePopup',
        required: true,
      },
      {
        key: 'componentName',
        label: '연결할 컴포넌트',
        type: 'select',
        showIf: (fd) => fd.actionType === 'component',
        options: popupComponentOptions,
        required: true,
      },
      {
        key: 'buttonStyle',
        label: '버튼 스타일',
        type: 'select',
        options: [
          { value: 'primary', label: '파랑 (기본)' },
          { value: 'success', label: '초록' },
          { value: 'warning', label: '노랑' },
          { value: 'danger', label: '빨강' },
          { value: 'outline', label: '테두리' },
          { value: 'ghost', label: '투명' },
        ],
        default: 'primary',
      },
      {
        key: 'buttonSize',
        label: '버튼 크기',
        type: 'select',
        options: [
          { value: 'sm', label: '작게' },
          { value: 'md', label: '보통' },
          { value: 'lg', label: '크게' },
        ],
        default: 'md',
      },
    ],
    buildPayload(formData, conventionId) {
      const config = {
        style: formData.buttonStyle || 'primary',
        size: formData.buttonSize || 'md',
      }

      let behaviorType = 'StatusOnly'
      let mapsTo = null

      if (formData.actionType === 'internal' && formData.internalPage) {
        behaviorType = 'ModuleLink'
        mapsTo = `/conventions/${conventionId}/${formData.internalPage}`
      } else if (formData.actionType === 'external' && formData.externalUrl) {
        behaviorType = 'Link'
        mapsTo = formData.externalUrl
      } else if (formData.actionType === 'imagePopup' && formData.popupImage) {
        behaviorType = 'ShowComponentPopup'
        config.popupImageUrl = formData.popupImage
      } else if (formData.actionType === 'component' && formData.componentName) {
        behaviorType = 'ShowComponentPopup'
        mapsTo = formData.componentName
      }

      return {
        conventionId,
        title: formData.title,
        actionCategory: 'BUTTON',
        targetLocation: formData.targetLocation || 'HOME_SUB_HEADER',
        behaviorType,
        deadline: null,
        isActive: true,
        orderNum: formData.orderNum ?? 0,
        mapsTo,
        targetId: null,
        configJson: JSON.stringify(config),
      }
    },
  },
]

export const EXPERT_PRESET = {
  id: 'expert',
  label: '고급 설정',
  description: '모든 옵션 직접 설정',
  icon: Settings,
}

export function getPresetById(id) {
  return ACTION_PRESETS.find((p) => p.id === id)
}
