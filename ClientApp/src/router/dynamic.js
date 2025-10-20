import DynamicFeatureLoader from '@/dynamic-features/DynamicFeatureLoader.vue'

export const dynamicFeatureRoutes = [
  {
    path: ':featureName',
    name: 'DynamicFeature',
    component: DynamicFeatureLoader,
    props: true,
    meta: {
      title: '기능',
      layout: 'FeatureLayout',
      requiresAuth: true,
      showNav: false
    }
  }
]

export default dynamicFeatureRoutes
