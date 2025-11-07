/**
 * Target Locations Schema
 *
 * Defines all available placement locations where dynamic features can be rendered.
 * Each location corresponds to a specific "slot" in the frontend UI.
 *
 * @typedef {Object} TargetLocation
 * @property {string} key - Unique identifier (stored in DB)
 * @property {string} displayName - User-friendly name shown in admin UI
 * @property {string} description - Detailed explanation of where the feature appears
 * @property {string} page - The page/view where this location exists
 * @property {string[]} allowedCategories - Which action categories can be placed here
 */

export const TARGET_LOCATIONS = [
  {
    key: 'GLOBAL_ROOT_POPUP',
    displayName: 'App Global (Popup)',
    description:
      '앱 전체의 모든 페이지에 걸쳐 표시됩니다. 주로 로그인 후 팝업을 자동으로 띄우는 데 사용됩니다.',
    page: 'App.vue (Global)',
    allowedCategories: ['AUTO_POPUP'],
  },
  {
    key: 'HOME_SUB_HEADER',
    displayName: 'Home: Below Main Title',
    description:
      '홈 화면의 메인 배너 바로 아래에 기능이 추가됩니다.',
    page: 'ConventionHome.vue',
    allowedCategories: ['BUTTON', 'BANNER', 'CARD', 'CHECKLIST_CARD'],
  },
  {
    key: 'HOME_CONTENT_TOP',
    displayName: 'Home: Top of Content Area',
    description:
      '메인 콘텐츠 섹션 상단, 체크리스트 앞에 기능이 추가됩니다.',
    page: 'ConventionHome.vue',
    allowedCategories: ['BUTTON', 'BANNER', 'CARD', 'CHECKLIST_CARD'],
  },
  {
    key: 'SCHEDULE_CONTENT_TOP',
    displayName: 'My Schedule: Below Date Filter',
    description:
      '"나의 일정" 페이지의 가로 날짜 선택기 아래에 기능이 추가됩니다.',
    page: 'MySchedule.vue',
    allowedCategories: ['BUTTON', 'BANNER', 'CARD'],
  },
  {
    key: 'BOARD_CONTENT_TOP',
    displayName: 'Notice Board: Below Category Filter',
    description:
      '"게시판" 페이지의 카테고리 필터 탭 아래에 기능이 추가됩니다.',
    page: 'Board.vue',
    allowedCategories: ['BUTTON', 'BANNER'],
  },
  {
    key: 'MORE_FEATURES_GRID',
    displayName: 'More Features: Menu Grid',
    description:
      '"더보기" 화면의 메인 그리드에 새 메뉴 항목이 추가됩니다.',
    page: 'MoreFeaturesView.vue',
    allowedCategories: ['MENU'],
  },
  {
    key: 'PARTICIPANTS_TOP',
    displayName: 'Participants: Above List',
    description: 'Adds features at the top of the participants list page.',
    page: 'Participants.vue',
    allowedCategories: ['BUTTON', 'BANNER'],
  },
]

/**
 * Helper function to get target location by key
 * @param {string} key - The target location key
 * @returns {TargetLocation|undefined}
 */
export function getTargetLocation(key) {
  return TARGET_LOCATIONS.find((location) => location.key === key)
}

/**
 * Helper function to get all target location keys
 * @returns {string[]}
 */
export function getTargetLocationKeys() {
  return TARGET_LOCATIONS.map((location) => location.key)
}

/**
 * Helper function to get target locations for dropdown
 * @returns {Array<{value: string, label: string, description: string, page: string}>}
 */
export function getTargetLocationOptions() {
  return TARGET_LOCATIONS.map((location) => ({
    value: location.key,
    label: location.displayName,
    description: location.description,
    page: location.page,
  }))
}

/**
 * Helper function to get allowed target locations for a specific action category
 * @param {string} categoryKey - The action category key
 * @returns {TargetLocation[]}
 */
export function getAllowedLocationsForCategory(categoryKey) {
  return TARGET_LOCATIONS.filter((location) =>
    location.allowedCategories.includes(categoryKey),
  )
}

/**
 * Helper function to validate if a category can be placed at a location
 * @param {string} categoryKey - The action category key
 * @param {string} locationKey - The target location key
 * @returns {boolean}
 */
export function isCategoryAllowedAtLocation(categoryKey, locationKey) {
  const location = getTargetLocation(locationKey)
  return location ? location.allowedCategories.includes(categoryKey) : false
}

/**
 * Group locations by page for easier selection
 * @returns {Object<string, TargetLocation[]>}
 */
export function getLocationsByPage() {
  const grouped = {}
  TARGET_LOCATIONS.forEach((location) => {
    if (!grouped[location.page]) {
      grouped[location.page] = []
    }
    grouped[location.page].push(location)
  })
  return grouped
}
