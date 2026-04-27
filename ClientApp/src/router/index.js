import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import { useUIStore } from '@/stores/ui'
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
    path: '/auto-login',
    name: 'AutoLogin',
    component: () => import('@/views/AutoLoginView.vue'),
    meta: { title: '자동 로그인', requiresAuth: false },
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

  // === App Download ===
  {
    path: '/app-download',
    name: 'AppDownload',
    component: () => import('@/views/AppDownloadView.vue'),
    meta: { title: '앱 다운로드', requiresAuth: true },
  },

  // === Main Home (행사 목록) ===
  {
    path: '/',
    name: 'MainHome',
    component: () => import('@/views/MainHome.vue'),
    meta: { title: '홈', requiresAuth: true },
  },

  // === Banner Detail ===
  {
    path: '/banners/:id',
    name: 'BannerDetail',
    component: () => import('@/views/BannerDetailView.vue'),
    meta: { title: '배너 상세', requiresAuth: true },
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
        redirect: (to) => ({
          path: `/conventions/${to.params.conventionId}`,
        }),
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
        path: 'boarding-pass',
        name: 'BoardingPass',
        component: () => import('@/views/BoardingPassView.vue'),
        meta: { title: '내 탑승권', showNav: false },
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
        path: 'travel-guide',
        name: 'TravelGuide',
        component: () => import('@/views/TravelGuide.vue'),
        meta: { title: '여행 가이드', showNav: true },
      },
      {
        path: 'checklist',
        name: 'Checklist',
        component: () => import('@/views/feature/ChecklistView.vue'),
        meta: { title: '체크리스트', showNav: false },
      },
      {
        path: 'surveys',
        name: 'SurveyList',
        component: () => import('@/views/feature/SurveyList.vue'),
        meta: { title: '설문조사', showNav: false },
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
    component: () => import('@/layouts/AdminLayout.vue'),
    meta: { requiresAuth: true, requiresAdmin: true },
    children: [
      {
        path: '',
        name: 'Admin',
        redirect: { name: 'AdminConventionList' },
      },
      {
        path: 'conventions',
        name: 'AdminConventionList',
        component: () => import('@/views/ConventionListView.vue'),
        meta: { title: '행사 관리', adminTitle: '관리자' },
      },
      {
        path: 'users',
        name: 'AdminUsers',
        component: () => import('@/components/admin/UserManagement.vue'),
        meta: { title: '회원 관리', adminTitle: '관리자' },
      },
      {
        path: 'statistics',
        name: 'AdminStatistics',
        component: () => import('@/views/admin/AdminStatisticsView.vue'),
        meta: { title: '통계', adminTitle: '관리자' },
      },
      {
        path: 'guide',
        name: 'AdminGuide',
        component: () => import('@/views/admin/AdminGuideView.vue'),
        meta: { title: '사용 가이드', adminTitle: '관리자' },
      },
      {
        path: 'home-banners',
        name: 'AdminHomeBanners',
        component: () => import('@/views/admin/HomeBannerManagementView.vue'),
        meta: { title: '홈 배너 관리', adminTitle: '관리자' },
      },
      {
        path: 'conventions/:id',
        component: () => import('@/views/AdminDashboard.vue'),
        meta: {
          title: '행사 대시보드',
          adminTitle: '행사 관리',
        },
        children: [
          {
            path: '',
            name: 'AdminDashboard',
            redirect: (to) => ({
              name: 'AdminDashboardOverview',
              params: to.params,
            }),
          },
          {
            path: 'dashboard',
            name: 'AdminDashboardOverview',
            component: () => import('@/components/admin/DashboardOverview.vue'),
            meta: { title: '대시보드' },
          },
          {
            path: 'guests',
            name: 'AdminGuests',
            component: () => import('@/components/admin/GuestManagement.vue'),
            meta: { title: '참석자 관리' },
          },
          {
            path: 'board',
            name: 'AdminBoard',
            component: () => import('@/components/admin/BoardManagement.vue'),
            meta: { title: '게시판 관리' },
          },
          {
            path: 'schedules',
            name: 'AdminSchedules',
            component: () =>
              import('@/components/admin/ScheduleManagement.vue'),
            meta: { title: '일정 관리' },
          },
          {
            path: 'actions',
            name: 'AdminActions',
            component: () => import('@/components/admin/ActionManagement.vue'),
            meta: { title: '액션 관리' },
          },
          {
            path: 'surveys',
            name: 'AdminSurveys',
            component: () => import('@/components/admin/SurveyManagement.vue'),
            meta: { title: '설문 관리' },
          },
          {
            path: 'formbuilder',
            name: 'AdminFormBuilder',
            component: () =>
              import('@/components/admin/FormBuilderManagement.vue'),
            meta: { title: '폼 빌더' },
          },
          {
            path: 'upload',
            name: 'AdminUpload',
            component: () => import('@/components/admin/BulkUpload.vue'),
            meta: { title: '엑셀 업로드' },
          },
          {
            path: 'sns',
            name: 'AdminSns',
            component: () => import('@/components/admin/SnsManagement.vue'),
            meta: { title: 'SNS 발송' },
          },
          {
            path: 'notifications',
            name: 'AdminNotifications',
            component: () =>
              import('@/components/admin/NotificationSender.vue'),
            meta: { title: '알림 발송' },
          },
          {
            path: 'attribute-categories',
            name: 'AdminAttributeCategories',
            component: () =>
              import('@/components/admin/AttributeCategoryManager.vue'),
            meta: { title: '속성 카테고리' },
          },
          {
            path: 'passport',
            name: 'AdminPassport',
            component: () => import('@/components/admin/PassportDashboard.vue'),
            meta: { title: '여권 관리' },
          },
        ],
      },
      {
        path: 'form-builder',
        name: 'FormBuilderList',
        component: () => import('@/views/admin/FormBuilderListView.vue'),
        meta: {
          title: '폼 빌더',
          adminTitle: '폼 빌더',
        },
      },
      {
        path: 'form-builder/:id',
        name: 'FormBuilderEdit',
        component: () => import('@/views/admin/FormBuilderEditView.vue'),
        meta: {
          title: '폼 편집',
          hideAdminHeader: true,
          adminFullScreen: true,
        },
      },
    ],
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
    return { top: 0, behavior: 'instant' }
  },
})

router.beforeEach(async (to, from, next) => {
  // 뒤로가기 시 모달 닫기
  const uiStore = useUIStore()
  if (uiStore.hasOpenModal() && to.path !== from.path) {
    uiStore.closeTopModal()
    next(false)
    return
  }

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
