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
      'Spans the entire app over any page. Used mainly for auto-opening popups after login.',
    page: 'App.vue (Global)',
    allowedCategories: ['AUTO_POPUP'],
  },
  {
    key: 'HOME_SUB_HEADER',
    displayName: 'Home: Below Main Title',
    description:
      'Adds features directly below the main banner on the Home screen.',
    page: 'ConventionHome.vue',
    allowedCategories: ['BUTTON', 'BANNER', 'CARD'],
  },
  {
    key: 'HOME_CONTENT_TOP',
    displayName: 'Home: Top of Content Area',
    description:
      'Adds features at the top of the main content section, before the checklist.',
    page: 'ConventionHome.vue',
    allowedCategories: ['BUTTON', 'BANNER', 'CARD'],
  },
  {
    key: 'SCHEDULE_CONTENT_TOP',
    displayName: 'My Schedule: Below Date Filter',
    description:
      'Adds features below the horizontal date-selector strip on the "My Schedule" page.',
    page: 'MySchedule.vue',
    allowedCategories: ['BUTTON', 'BANNER', 'CARD'],
  },
  {
    key: 'BOARD_CONTENT_TOP',
    displayName: 'Notice Board: Below Category Filter',
    description:
      'Adds features below the category filter tabs on the "Notice Board" page.',
    page: 'Board.vue',
    allowedCategories: ['BUTTON', 'BANNER'],
  },
  {
    key: 'MORE_FEATURES_GRID',
    displayName: 'More Features: Menu Grid',
    description:
      'Adds new menu items to the main grid on the "More Features" screen.',
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
