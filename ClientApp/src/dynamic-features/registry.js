/**
 * 동적 기능 자동 등록 시스템
 *
 * 폴더 구조 기반으로 자동으로 기능을 등록합니다.
 *
 * ## 사용 방법
 *
 * ### 방법 1: 간단한 기능 (index.js 없이)
 * ```
 * /dynamic-features/
 *   └── my-feature/
 *       └── views/
 *           └── MainView.vue   ← 이 파일만 있으면 자동 인식
 * ```
 *
 * ### 방법 2: 복잡한 기능 (index.js 사용)
 * ```
 * /dynamic-features/
 *   └── my-feature/
 *       ├── index.js           ← 진입점
 *       ├── views/
 *       │   ├── Page1.vue
 *       │   └── Page2.vue
 *       └── components/
 * ```
 *
 * index.js 예시:
 * ```javascript
 * export default {
 *   component: () => import('./views/Page1.vue'),
 *   meta: {
 *     title: '내 기능',
 *     icon: 'star'
 *   }
 * }
 * ```
 */

// ============================================
// Vite Glob Import를 사용한 자동 기능 검색
// ============================================

// 1. index.js가 있는 기능들 (복잡한 기능)
const indexModules = import.meta.glob('./*/index.js')

// 2. MainView.vue가 있는 기능들 (간단한 기능)
const mainViewModules = import.meta.glob('./*/views/MainView.vue')

// 3. 모든 뷰 파일들 (폴백용)
const allViewModules = import.meta.glob('./*/views/*.vue')

// ============================================
// Feature Registry 구축
// ============================================

export const featureRegistry = {}

// index.js가 있는 기능들을 먼저 등록
for (const path in indexModules) {
  const match = path.match(/\.\/([^/]+)\/index\.js$/)
  if (match) {
    const featureName = match[1]
    featureRegistry[featureName] = {
      type: 'indexed',
      loader: indexModules[path],
    }
  }
}

// MainView.vue가 있는 기능들을 등록 (index.js가 없는 경우에만)
for (const path in mainViewModules) {
  const match = path.match(/\.\/([^/]+)\/views\/MainView\.vue$/)
  if (match) {
    const featureName = match[1]
    // index.js가 없는 경우에만 등록
    if (!featureRegistry[featureName]) {
      featureRegistry[featureName] = {
        type: 'simple',
        loader: mainViewModules[path],
      }
    }
  }
}

console.log('📦 Registered Features:', Object.keys(featureRegistry))

// ============================================
// 기능 로드 함수
// ============================================

/**
 * 동적으로 기능을 로드합니다.
 *
 * @param {string} featureName - 로드할 기능 이름 (폴더명)
 * @returns {Promise<Object>} - 기능 모듈 (component, meta 등)
 * @throws {Error} - 기능을 찾을 수 없는 경우
 */
export async function loadFeature(featureName) {
  const feature = featureRegistry[featureName]

  if (!feature) {
    // 등록되지 않은 기능 - 더 자세한 에러 메시지
    const available = Object.keys(featureRegistry).join(', ')
    throw new Error(
      `기능 '${featureName}'을(를) 찾을 수 없습니다.\n` +
        `사용 가능한 기능: ${available || '없음'}\n\n` +
        `해결 방법:\n` +
        `1. /dynamic-features/${featureName} 폴더가 존재하는지 확인\n` +
        `2. index.js 또는 views/MainView.vue 파일이 있는지 확인`,
    )
  }

  try {
    if (feature.type === 'indexed') {
      // index.js가 있는 경우 - 모듈을 그대로 반환
      const module = await feature.loader()
      return module.default || module
    } else {
      // MainView.vue만 있는 경우 - 컴포넌트를 래핑
      const component = await feature.loader()
      return {
        component: () => Promise.resolve(component.default || component),
        meta: {
          title: featureName,
          isSimple: true,
        },
      }
    }
  } catch (error) {
    console.error(`Failed to load feature '${featureName}':`, error)
    throw new Error(
      `기능 '${featureName}' 로딩 중 오류가 발생했습니다.\n` +
        `상세: ${error.message}`,
    )
  }
}

/**
 * 기능이 등록되어 있는지 확인합니다.
 *
 * @param {string} featureName - 확인할 기능 이름
 * @returns {boolean}
 */
export function isFeatureRegistered(featureName) {
  return featureName in featureRegistry
}

/**
 * 등록된 모든 기능 목록을 반환합니다.
 *
 * @returns {Array<Object>} - 기능 목록 배열
 */
export function getRegisteredFeatures() {
  return Object.entries(featureRegistry).map(([name, feature]) => ({
    name,
    type: feature.type,
    isIndexed: feature.type === 'indexed',
  }))
}

/**
 * 특정 패턴으로 기능을 검색합니다.
 *
 * @param {string} pattern - 검색 패턴 (정규식 문자열)
 * @returns {Array<string>} - 매칭되는 기능 이름 배열
 */
export function searchFeatures(pattern) {
  const regex = new RegExp(pattern, 'i')
  return Object.keys(featureRegistry).filter((name) => regex.test(name))
}
