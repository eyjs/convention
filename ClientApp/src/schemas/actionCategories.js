/**
 * Action Categories Schema
 *
 * Defines all available action types that can be created by administrators.
 * Each category maps to a corresponding generic component in the frontend.
 *
 * @typedef {Object} ActionCategory
 * @property {string} key - Unique identifier (stored in DB)
 * @property {string} displayName - User-friendly name shown in admin UI
 * @property {string} description - Detailed explanation of the action type
 * @property {string} component - Frontend component to render
 */

export const ACTION_CATEGORIES = [
  {
    key: 'BUTTON',
    displayName: '버튼',
    description:
      '특정 페이지의 지정된 영역에 클릭 가능한 버튼을 생성합니다.',
    component: 'GenericButton',
    guide: {
      title: 'Button: 추가 설정 가이드',
      content:
        '버튼의 세부 동작과 스타일을 JSON으로 설정합니다. `onClick` 객체를 통해 클릭 시 동작을 정의할 수 있습니다.',
      example: JSON.stringify(
        {
          text: '버튼 텍스트 (필수)',
          icon: 'IconName (선택)',
          style: 'primary | secondary (선택)',
          onClick: {
            type: 'NAVIGATE | OPEN_POPUP',
            payload: '/path-or-action-id',
          },
        },
        null,
        2,
      ),
    },
  },
  {
    key: 'MENU',
    displayName: '추가메뉴',
    description:
      '"더보기" 페이지와 같은 메뉴 화면에 새 항목을 추가합니다.',
    component: 'GenericMenuItem',
    guide: {
      title: 'Menu Item: 추가 설정 가이드',
      content: '메뉴 항목의 텍스트, 아이콘, 설명 등을 설정합니다.',
      example: JSON.stringify(
        {
          text: '메뉴 이름 (필수)',
          description: '메뉴 설명 (선택)',
          icon: 'IconName (선택)',
          onClick: {
            type: 'NAVIGATE',
            payload: '/target-path',
          },
        },
        null,
        2,
      ),
    },
  },
  {
    key: 'AUTO_POPUP',
    displayName: '자동팝업',
    description:
      '사용자 로그인과 같은 특정 조건에 따라 자동으로 나타나는 팝업을 표시합니다.',
    component: 'GenericAutoPopup',
    guide: {
      title: 'Auto Popup: 추가 설정 가이드',
      content:
        '자동 팝업의 내용과 동작 방식을 정의합니다. `showOnce`를 true로 설정하면 사용자가 한 번 닫은 후 다시 보지 않습니다.',
      example: JSON.stringify(
        {
          title: '팝업 제목',
          message: '팝업 메시지',
          buttonText: '버튼 텍스트',
          dismissible: true,
          showOnce: true,
          onClick: {
            type: 'NAVIGATE',
            payload: '/target-path',
          },
        },
        null,
        2,
      ),
    },
  },
  {
    key: 'BANNER',
    displayName: '배너',
    description: '페이지 상단에 배너 이미지 또는 메시지를 표시합니다.',
    component: 'GenericBanner',
    guide: {
      title: 'Banner: 추가 설정 가이드',
      content: '배너에 표시될 이미지나 텍스트, 클릭 시 동작을 설정합니다.',
      example: JSON.stringify(
        {
          imageUrl: '/path/to/image.jpg',
          altText: '배너 이미지 설명',
          onClick: {
            type: 'NAVIGATE',
            payload: '/banner-link',
          },
        },
        null,
        2,
      ),
    },
  },
  {
    key: 'CARD',
    displayName: '정보카드',
    description:
      '제목, 설명 및 선택적 아이콘이 있는 정보 카드를 표시합니다.',
    component: 'GenericCard',
    guide: {
      title: 'Info Card: 추가 설정 가이드',
      content: '정보 카드에 표시될 내용을 설정합니다.',
      example: JSON.stringify(
        {
          title: '카드 제목',
          description: '카드 상세 내용',
          icon: 'IconName',
          onClick: {
            type: 'OPEN_POPUP',
            payload: 'SOME_POPUP_ACTION_ID',
          },
        },
        null,
        2,
      ),
    },
  },
  {
    key: 'CHECKLIST_CARD',
    displayName: '체크리스트',
    description:
      '사용자 체크리스트를 위해 완료 체크박스가 있는 카드를 생성합니다.',
    component: 'ChecklistCard',
    guide: {
      title: 'Checklist Card: 추가 설정 가이드',
      content:
        '체크리스트 카드에 표시될 내용을 설정합니다. ConfigJson은 사용되지 않으며, 액션의 기본 제목과 마감일이 자동으로 표시됩니다.',
      example: JSON.stringify(
        {
          "info": "이 카테고리는 ConfigJson을 사용하지 않습니다."
        },
        null,
        2,
      ),
    },
  },
]

/**
 * Helper function to get action category by key
 * @param {string} key - The action category key
 * @returns {ActionCategory|undefined}
 */
export function getActionCategory(key) {
  return ACTION_CATEGORIES.find((category) => category.key === key)
}

/**
 * Helper function to get all action category keys
 * @returns {string[]}
 */
export function getActionCategoryKeys() {
  return ACTION_CATEGORIES.map((category) => category.key)
}

/**
 * Helper function to get action categories for dropdown
 * @returns {Array<{value: string, label: string, description: string}>}
 */
export function getActionCategoryOptions() {
  return ACTION_CATEGORIES.map((category) => ({
    value: category.key,
    label: category.displayName,
    description: category.description,
  }))
}
