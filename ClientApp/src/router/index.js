import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useUIStore } from '@/stores/ui'
import { dynamicFeatureRoutes } from './dynamic'

// ============================================
// Lazy Loading으로 모든 컴포넌트 import
// 필요할 때만 로드되어 초기 번들 크기 감소
// ============================================

const routes = [
  // === Auth Routes (No Layout) ===
  {
    path: '/setup',
    name: 'Setup',
    component: () => import('@/views/SetupView.vue'),
    meta: {
      title: '초기 설정',
      requiresAuth: false,
      layout: null,
      showNav: false,
    },
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/LoginView.vue'),
    meta: {
      title: '로그인',
      requiresAuth: false,
      layout: null,
      showNav: false,
    },
  },
  {
    path: '/find-id',
    name: 'FindId',
    component: () => import('@/views/FindId.vue'),
    meta: {
      title: '아이디 찾기',
      requiresAuth: false,
      layout: null,
      showNav: false,
    },
  },
  {
    path: '/find-password',
    name: 'FindPassword',
    component: () => import('@/views/FindPassword.vue'),
    meta: {
      title: '비밀번호 찾기',
      requiresAuth: false,
      layout: null,
      showNav: false,
    },
  },

  // === Main Routes (Default Layout) ===
  {
    path: '/my-profile',
    name: 'MyProfile',
    component: () => import('@/views/user/MyProfile.vue'),
    meta: {
      title: '내 정보',
      requiresAuth: true,
      layout: null,
      showNav: false,
    },
  },

  // === Main Home ===
  {
    path: '/home',
    name: 'MainHome',
    component: () => import('@/views/MainHome.vue'),
    meta: {
      title: '홈',
      requiresAuth: true,
      layout: null,
      showNav: false,
    },
  },

  // === Personal Trip Routes ===
  {
    path: '/trips',
    name: 'TripList',
    component: () => import('@/views/trip/TripList.vue'),
    meta: {
      title: '내 여행',
      requiresAuth: true,
      layout: null,
      showNav: false,
    },
  },
      {
        path: '/trips/new',
        name: 'NewTrip',
        component: () => import('@/views/trip/TripDetail.vue'),
        meta: { requiresAuth: true, title: '새 여행' }
      },
      {
        path: '/trips/share/:shareToken',
        name: 'SharedTripDetail',
        component: () => import('@/views/trip/TripDetail.vue'),
        props: route => ({
          shareToken: route.params.shareToken,
          readonly: true
        }),
        meta: { requiresAuth: false, title: '공유된 여행' }
      },
      {
        path: '/trips/share/:shareToken/itinerary',
        name: 'SharedTripItinerary',
        component: () => import('@/views/trip/TripItinerary.vue'),
        props: route => ({
          shareToken: route.params.shareToken,
          readonly: true
        }),
        meta: { requiresAuth: false, title: '공유된 여행 일정표' }
      },
      {
        path: '/trips/share/:shareToken/expenses',
        name: 'SharedTripExpenses',
        component: () => import('@/views/trip/TripExpenses.vue'),
        props: route => ({
          shareToken: route.params.shareToken,
          readonly: true
        }),
        meta: { requiresAuth: false, title: '공유된 여행 가계부' }
      },
      {
        path: '/trips/share/:shareToken/notes',
        name: 'SharedTripNotes',
        component: () => import('@/views/trip/TripNotes.vue'),
        props: route => ({
          shareToken: route.params.shareToken,
          readonly: true
        }),
        meta: { requiresAuth: false, title: '공유된 여행 노트' }
      },
      {
        path: '/trips/share/:shareToken/transportation',
        name: 'SharedTripTransportation',
        component: () => import('@/views/trip/TripTransportation.vue'),
        props: route => ({
          shareToken: route.params.shareToken,
          readonly: true
        }),
        meta: { requiresAuth: false, title: '공유된 여행 교통편' }
      },
      {
        path: '/trips/:id',
        name: 'TripDetail',
        component: () => import('@/views/trip/TripDetail.vue'),
        props: true,
        meta: { requiresAuth: true, title: '여행 상세' }
      },
      
      {
        path: '/trips/:id/itinerary',
        name: 'TripItinerary',
        component: () => import('@/views/trip/TripItinerary.vue'),
        props: true,
        meta: { requiresAuth: true, title: '여행 일정표' }
      },
      {
        path: '/trips/:id/expenses',
        name: 'TripExpenses',
        component: () => import('@/views/trip/TripExpenses.vue'),
        props: true,
        meta: { requiresAuth: true, title: '여행 가계부' }
      },
      {
        path: '/trips/:id/notes',
        name: 'TripNotes',
        component: () => import('@/views/trip/TripNotes.vue'),
        props: true,
        meta: { requiresAuth: true, title: '여행 노트' }
      },
      {
        path: '/trips/:id/transportation',
        name: 'TripTransportation',
        component: () => import('@/views/trip/TripTransportation.vue'),
        props: true,
        meta: { requiresAuth: true, title: '여행 교통편' }
      },
      // ----------------------------------------------------------------
      // 동적 라우팅
      // ----------------------------------------------------------------

  {
    path: '/',
    name: 'Home',
    component: () => import('@/views/ConventionHome.vue'),
    meta: {
      title: '행사 홈',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
    },
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
      layout: 'DefaultLayout',
      showNav: false,
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
      layout: 'DefaultLayout',
      showNav: false,
    },
  },
  {
    path: '/admin/chatbot',
    name: 'AdminChatbotManagement',
    component: () => import('@/views/AdminChatbotManagement.vue'),
    meta: {
      title: '챗봇 관리',
      requiresAuth: true,
      requiresAdmin: true,
      layout: 'DefaultLayout',
      showNav: false,
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
      layout: 'DefaultLayout',
      showNav: false,
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
      layout: 'DefaultLayout',
      showNav: false,
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
      layout: null, // No layout for this special page
      showNav: false,
    },
  },

  // === Convention Features ===
  {
    path: '/my-schedule',
    name: 'MySchedule',
    component: () => import('@/views/MySchedule.vue'),
    meta: {
      title: '나의일정',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
    },
  },

  {
    path: '/feature/travel-info',
    name: 'TravelInfo',
    component: () => import('@/views/features/TravelInfo.vue'),
    meta: {
      title: '여행 서류 제출',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
      showNav: false,
    },
  },
  {
    path: '/feature/surveys/:id',
    name: 'Survey',
    component: () => import('@/views/feature/Survey.vue'),
    props: true,
    meta: {
      title: 'Survey',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
      showNav: false,
    },
  },
  {
    path: '/feature/generic-form/:actionId',
    name: 'GenericForm',
    component: () => import('@/views/feature/GenericForm.vue'),
    props: true,
    meta: {
      title: '폼 작성',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
      showNav: false,
    },
  },
  {
    path: '/feature/form/:formDefinitionId',
    name: 'DynamicFormRenderer',
    component: () => import('@/views/feature/DynamicFormRenderer.vue'),
    props: true,
    meta: {
      title: '양식 작성',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
      showNav: false,
    },
  },
  {
    path: '/participants',
    name: 'Participants',
    component: () => import('@/views/Participants.vue'),
    meta: {
      title: '주체국',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
    },
  },
  {
    path: '/group-schedule',
    name: 'GroupSchedule',
    component: () => import('@/views/GroupSchedule.vue'),
    meta: {
      title: '합창국',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
    },
  },

  // === Location Features ===
  {
    path: '/location',
    name: 'LocationInfo',
    component: () => import('@/views/LocationInfo.vue'),
    meta: {
      title: '로마 정보',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
    },
  },
  {
    path: '/hotspot',
    name: 'HotSpot',
    component: () => import('@/views/HotSpot.vue'),
    meta: {
      title: '핫스팟',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
    },
  },
  {
    path: '/tastespot',
    name: 'TasteSpot',
    component: () => import('@/views/TasteSpot.vue'),
    meta: {
      title: '맛스팟',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
    },
  },

  // === Media Features ===
  {
    path: '/event-place',
    name: 'EventPlace',
    component: () => import('@/views/EventPlace.vue'),
    meta: {
      title: '사진첩',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
    },
  },

  // === Notice Features ===
  {
    path: '/notices',
    name: 'Board',
    component: () => import('@/views/Board.vue'),
    meta: {
      title: '공지사항',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
    },
  },
  {
    path: '/notices/:id',
    name: 'NoticeDetail',
    component: () => import('@/views/NoticeDetail.vue'),
    props: true,
    meta: {
      title: '공지사항 상세',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
    },
  },

  // === Dynamic Features ===
  {
    path: '/features',
    name: 'MoreFeatures',
    component: () => import('@/views/MoreFeaturesView.vue'),
    meta: {
      title: '추가 기능',
      requiresAuth: true,
      requiresConvention: true,
      layout: 'DefaultLayout',
    },
  },
  {
    path: '/feature',
    children: dynamicFeatureRoutes,
    meta: {
      requiresAuth: true,
      requiresConvention: true,
    },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    } else {
      return { top: 0 }
    }
  },
})

router.beforeEach((to, from, next) => {
  document.title = to.meta.title || 'iFA Convention'

  const authStore = useAuthStore()
  const uiStore = useUIStore()
  const requiresAuth = to.meta.requiresAuth !== false
  const requiresAdmin = to.meta.requiresAdmin || false

  // 0. 모달 체크 - 뒤로가기 시 모달부터 닫기
  if (uiStore.hasOpenModal()) {
    uiStore.closeTopModal()
    next(false) // 라우팅 취소
    return
  }

  // [방어] LocalStorage 무결성 검사 - "undefined" 문자열 오염 정화
  let selectedConventionId = localStorage.getItem('selectedConventionId')
  if (selectedConventionId === 'undefined' || selectedConventionId === 'null') {
    console.warn('Corrupted localStorage detected. Cleaning up...')
    localStorage.removeItem('selectedConventionId')
    selectedConventionId = null
  }

  // 1. 로그인 페이지 접근 시
  if (to.path === '/login' && authStore.isAuthenticated) {
    // 어드민은 어드민 페이지로
    if (authStore.isAdmin) {
      next('/admin')
      return
    }
    // 일반 유저는 홈으로
    next('/home')
    return
  }

  // 2. 인증 필요 체크
  if (requiresAuth && !authStore.isAuthenticated) {
    next('/login')
    return
  }

  // 3. 어드민 권한 체크
  if (requiresAdmin && !authStore.isAdmin) {
    next('/')
    return
  }

  // 4. Convention이 필요한 페이지 체크 (메타 정보 활용)
  // 각 라우트의 meta.requiresConvention 플래그 확인
  const requiresConvention = to.meta.requiresConvention === true

  // Convention이 필요한 페이지인데 선택된 Convention이 없으면 /home으로
  if (
    authStore.isAuthenticated &&
    !authStore.isAdmin &&
    requiresConvention &&
    !selectedConventionId
  ) {
    next('/home')
    return
  }

  next()
})

export default router
