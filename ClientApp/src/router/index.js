import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import { dynamicFeatureRoutes } from './dynamic'

const routes = [
  // === Auth Routes (No Layout) ===
  {
    path: '/setup',
    name: 'Setup',
    component: () => import('@/views/SetupView.vue'),
    meta: { title: '초기 설정', requiresAuth: false },
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/LoginView.vue'),
    meta: { title: '로그인', requiresAuth: false },
  },
  {
    path: '/find-id',
    name: 'FindId',
    component: () => import('@/views/FindId.vue'),
    meta: { title: '아이디 찾기', requiresAuth: false },
  },
  {
    path: '/find-password',
    name: 'FindPassword',
    component: () => import('@/views/FindPassword.vue'),
    meta: { title: '비밀번호 찾기', requiresAuth: false },
  },

  // === Main Home (행사 목록) ===
  {
    path: '/',
    name: 'MainHome',
    component: () => import('@/views/MainHome.vue'),
    meta: { title: '홈', requiresAuth: true },
  },

  // === User Profile ===
  {
    path: '/my-profile',
    name: 'MyProfile',
    component: () => import('@/views/user/MyProfile.vue'),
    meta: { title: '내 정보', requiresAuth: true },
  },

  // === Convention Routes (행사 상세 + 하위 페이지) ===
  {
    path: '/conventions/:conventionId',
    component: () => import('@/layouts/ConventionLayout.vue'),
    meta: { requiresAuth: true },
    async beforeEnter(to) {
      const conventionStore = useConventionStore()
      const id = parseInt(to.params.conventionId)
      if (isNaN(id)) return { name: 'MainHome' }

      if (conventionStore.currentConvention?.id !== id) {
        await conventionStore.selectConvention(id)
        if (!conventionStore.currentConvention) {
          return { name: 'MainHome' }
        }
      }
    },
    children: [
      {
        path: '',
        name: 'ConventionHome',
        component: () => import('@/views/ConventionHome.vue'),
        meta: { title: '행사 홈', showNav: true },
      },
      {
        path: 'schedule',
        name: 'MySchedule',
        component: () => import('@/views/MySchedule.vue'),
        meta: { title: '나의일정', showNav: true },
      },
      {
        path: 'notices',
        name: 'Board',
        component: () => import('@/views/Board.vue'),
        meta: { title: '공지사항', showNav: true },
      },
      {
        path: 'notices/:id',
        name: 'NoticeDetail',
        component: () => import('@/views/NoticeDetail.vue'),
        props: true,
        meta: { title: '공지사항 상세', showNav: true },
      },
      {
        path: 'features',
        name: 'MoreFeatures',
        component: () => import('@/views/MoreFeaturesView.vue'),
        meta: { title: '추가 기능', showNav: true },
      },
      {
        path: 'participants',
        name: 'Participants',
        component: () => import('@/views/Participants.vue'),
        meta: { title: '주체국', showNav: true },
      },
      {
        path: 'group-schedule',
        name: 'GroupSchedule',
        component: () => import('@/views/GroupSchedule.vue'),
        meta: { title: '합창국', showNav: true },
      },
      {
        path: 'location',
        name: 'LocationInfo',
        component: () => import('@/views/LocationInfo.vue'),
        meta: { title: '장소 정보', showNav: true },
      },
      {
        path: 'hotspot',
        name: 'HotSpot',
        component: () => import('@/views/HotSpot.vue'),
        meta: { title: '핫스팟', showNav: true },
      },
      {
        path: 'tastespot',
        name: 'TasteSpot',
        component: () => import('@/views/TasteSpot.vue'),
        meta: { title: '맛스팟', showNav: true },
      },
      {
        path: 'event-place',
        name: 'EventPlace',
        component: () => import('@/views/EventPlace.vue'),
        meta: { title: '사진첩', showNav: true },
      },
      {
        path: 'travel-info',
        name: 'TravelInfo',
        component: () => import('@/views/features/TravelInfo.vue'),
        meta: { title: '여행 서류 제출', showNav: false },
      },
      {
        path: 'surveys/:id',
        name: 'Survey',
        component: () => import('@/views/feature/Survey.vue'),
        props: true,
        meta: { title: 'Survey', showNav: false },
      },
      {
        path: 'generic-form/:actionId',
        name: 'GenericForm',
        component: () => import('@/views/feature/GenericForm.vue'),
        props: true,
        meta: { title: '폼 작성', showNav: false },
      },
      {
        path: 'form/:formDefinitionId',
        name: 'DynamicFormRenderer',
        component: () => import('@/views/feature/DynamicFormRenderer.vue'),
        props: true,
        meta: { title: '양식 작성', showNav: false },
      },
      {
        path: 'feature',
        children: dynamicFeatureRoutes,
      },
    ],
  },

  // === Convention List (legacy redirect) ===
  {
    path: '/conventions',
    name: 'ConventionList',
    component: () => import('@/views/ConventionList.vue'),
    meta: { title: '스타투어', requiresAuth: true },
  },

  // === Personal Trip Routes ===
  {
    path: '/trips',
    name: 'TripList',
    component: () => import('@/views/trip/TripList.vue'),
    meta: { title: '내 여행', requiresAuth: true },
  },
  {
    path: '/trips/new',
    name: 'NewTrip',
    component: () => import('@/views/trip/TripDetail.vue'),
    meta: { requiresAuth: true, title: '새 여행' },
  },
  {
    path: '/trips/share/:shareToken',
    name: 'SharedTripDetail',
    component: () => import('@/views/trip/TripDetail.vue'),
    props: (route) => ({
      shareToken: route.params.shareToken,
      readonly: true,
    }),
    meta: { requiresAuth: false, title: '공유된 여행' },
  },
  {
    path: '/trips/share/:shareToken/itinerary',
    name: 'SharedTripItinerary',
    component: () => import('@/views/trip/TripItinerary.vue'),
    props: (route) => ({
      shareToken: route.params.shareToken,
      readonly: true,
    }),
    meta: { requiresAuth: false, title: '공유된 여행 일정표' },
  },
  {
    path: '/trips/share/:shareToken/expenses',
    name: 'SharedTripExpenses',
    component: () => import('@/views/trip/TripExpenses.vue'),
    props: (route) => ({
      shareToken: route.params.shareToken,
      readonly: true,
    }),
    meta: { requiresAuth: false, title: '공유된 여행 가계부' },
  },
  {
    path: '/trips/share/:shareToken/notes',
    name: 'SharedTripNotes',
    component: () => import('@/views/trip/TripNotes.vue'),
    props: (route) => ({
      shareToken: route.params.shareToken,
      readonly: true,
    }),
    meta: { requiresAuth: false, title: '공유된 여행 노트' },
  },
  {
    path: '/trips/share/:shareToken/transportation',
    name: 'SharedTripTransportation',
    component: () => import('@/views/trip/TripTransportation.vue'),
    props: (route) => ({
      shareToken: route.params.shareToken,
      readonly: true,
    }),
    meta: { requiresAuth: false, title: '공유된 여행 교통편' },
  },
  {
    path: '/trips/:id',
    name: 'TripDetail',
    component: () => import('@/views/trip/TripDetail.vue'),
    props: true,
    meta: { requiresAuth: true, title: '여행 상세' },
  },
  {
    path: '/trips/:id/itinerary',
    name: 'TripItinerary',
    component: () => import('@/views/trip/TripItinerary.vue'),
    props: true,
    meta: { requiresAuth: true, title: '여행 일정표' },
  },
  {
    path: '/trips/:id/expenses',
    name: 'TripExpenses',
    component: () => import('@/views/trip/TripExpenses.vue'),
    props: true,
    meta: { requiresAuth: true, title: '여행 가계부' },
  },
  {
    path: '/trips/:id/notes',
    name: 'TripNotes',
    component: () => import('@/views/trip/TripNotes.vue'),
    props: true,
    meta: { requiresAuth: true, title: '여행 노트' },
  },
  {
    path: '/trips/:id/transportation',
    name: 'TripTransportation',
    component: () => import('@/views/trip/TripTransportation.vue'),
    props: true,
    meta: { requiresAuth: true, title: '여행 교통편' },
  },

  // === Admin Routes ===
  {
    path: '/admin',
    name: 'Admin',
    component: () => import('@/views/ConventionListView.vue'),
    meta: {
      title: '행사 관리',
      requiresAuth: true,
      requiresAdmin: true,
    },
  },
  {
    path: '/admin/conventions/:id',
    name: 'AdminDashboard',
    component: () => import('@/views/AdminDashboard.vue'),
    meta: {
      title: '행사 대시보드',
      requiresAuth: true,
      requiresAdmin: true,
    },
  },
  {
    path: '/admin/form-builder',
    name: 'FormBuilderList',
    component: () => import('@/views/admin/FormBuilderListView.vue'),
    meta: {
      title: '폼 빌더',
      requiresAuth: true,
      requiresAdmin: true,
    },
  },
  {
    path: '/admin/form-builder/:id',
    name: 'FormBuilderEdit',
    component: () => import('@/views/admin/FormBuilderEditView.vue'),
    meta: {
      title: '폼 편집',
      requiresAuth: true,
      requiresAdmin: true,
    },
  },
  {
    path: '/admin/name-tag-printing',
    name: 'NameTagPrinting',
    component: () => import('@/views/admin/NameTagPrintingView.vue'),
    meta: {
      title: '명찰 인쇄',
      requiresAuth: true,
      requiresAdmin: true,
    },
  },

  // === Legacy redirects ===
  { path: '/home', redirect: '/' },
  { path: '/my-schedule', redirect: '/' },
  { path: '/notices', redirect: '/' },
  { path: '/features', redirect: '/' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) return savedPosition
    return { top: 0 }
  },
})

router.beforeEach(async (to, from, next) => {
  document.title = to.meta.title || 'iFA Convention'

  const authStore = useAuthStore()

  // 인증 불필요 페이지
  if (to.meta.requiresAuth === false) {
    next()
    return
  }

  // 인증 초기화 완료 대기 (토큰 복원 + 사용자 정보 로드)
  await authStore.ensureInitialized()

  // 로그인 페이지 접근 시 이미 인증된 경우
  if (to.path === '/login' && authStore.isAuthenticated) {
    next(authStore.isAdmin ? '/admin' : '/')
    return
  }

  // 인증 필요한데 미인증
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
    return
  }

  // 어드민 권한 체크
  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next('/')
    return
  }

  next()
})

export default router
