import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import ConventionHome from '../views/ConventionHome.vue'
import LoginView from '../views/LoginView.vue'
import SetupView from '../views/SetupView.vue'
import ConventionListView from '../views/ConventionListView.vue'
import AdminDashboard from '../views/AdminDashboard.vue'
import MySchedule from '../views/MySchedule.vue'
import LocationInfo from '../views/LocationInfo.vue'
import Schedule from '../views/Schedule.vue'
import Participants from '../views/Participants.vue'
import HotSpot from '../views/HotSpot.vue'
import TasteSpot from '../views/TasteSpot.vue'
import GroupSchedule from '../views/GroupSchedule.vue'
import EventPlace from '../views/EventPlace.vue'
import Board from '../views/Board.vue'
import FindId from '../views/FindId.vue'
import FindPassword from '../views/FindPassword.vue'
import NoticeList from '../views/NoticeList.vue'
import NoticeDetail from '../views/NoticeDetail.vue'
import AdminChatbotManagement from '../views/AdminChatbotManagement.vue'

const routes = [
  {
    path: '/setup',
    name: 'Setup',
    component: SetupView,
    meta: { title: '초기 설정', requiresAuth: false }
  },
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
    meta: { title: '로그인', requiresAuth: false }
  },
  {
    path: '/find-id',
    name: 'FindId',
    component: FindId,
    meta: { title: '아이디 찾기', requiresAuth: false }
  },
  {
    path: '/find-password',
    name: 'FindPassword',
    component: FindPassword,
    meta: { title: '비밀번호 찾기', requiresAuth: false }
  },
  {
    path: '/',
    name: 'Home',
    component: ConventionHome,
    meta: { title: '행사 홈', requiresAuth: true }
  },
  {
    path: '/admin',
    name: 'Admin',
    component: ConventionListView,
    meta: { 
      title: '행사 관리', 
      requiresAuth: false,
      requiresAdmin: false
    }
  },
  {
    path: '/admin/conventions/:id',
    name: 'AdminDashboard',
    component: AdminDashboard,
    meta: { 
      title: '행사 대시보드', 
      requiresAuth: false,
      requiresAdmin: false
    }
  },
  {
    path: '/admin/chatbot',
    name: 'AdminChatbotManagement',
    component: AdminChatbotManagement,
    meta: {
      title: '챗봇 관리',
      requiresAuth: false, // Should be true for admin
      requiresAdmin: false // Should be true for admin
    }
  },
  {
    path: '/my-schedule',
    name: 'MySchedule',
    component: MySchedule,
    meta: { title: '나의일정' }
  },
  {
    path: '/location',
    name: 'LocationInfo',
    component: LocationInfo,
    meta: { title: '로마 정보' }
  },
  {
    path: '/schedule',
    name: 'Schedule',
    component: Schedule,
    meta: { title: '일정' }
  },
  {
    path: '/participants',
    name: 'Participants',
    component: Participants,
    meta: { title: '주체국' }
  },
  {
    path: '/hotspot',
    name: 'HotSpot',
    component: HotSpot,
    meta: { title: '핫스팟' }
  },
  {
    path: '/tastespot',
    name: 'TasteSpot',
    component: TasteSpot,
    meta: { title: '맛스팟' }
  },
  {
    path: '/group-schedule',
    name: 'GroupSchedule',
    component: GroupSchedule,
    meta: { title: '합창국' }
  },
  {
    path: '/event-place',
    name: 'EventPlace',
    component: EventPlace,
    meta: { title: '사진첩' }
  },
  {
    path: '/notices',
    name: 'Board',
    component: Board,
    meta: { title: '공지사항', requiresAuth: true }
  },
  {
    path: '/notices/:id',
    name: 'NoticeDetail',
    component: NoticeDetail,
    meta: { title: '공지사항 상세', requiresAuth: true }
  }
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
  }
})

router.beforeEach((to, from, next) => {
  document.title = to.meta.title || 'iFA Convention'
  
  const authStore = useAuthStore()
  const requiresAuth = to.meta.requiresAuth !== false
  const requiresAdmin = to.meta.requiresAdmin || false
  
  if (requiresAuth && !authStore.isAuthenticated) {
    next('/login')
  } else if (requiresAdmin && !authStore.isAdmin) {
    next('/')
  } else if (to.path === '/login' && authStore.isAuthenticated) {
    next('/')
  } else {
    next()
  }
})

export default router
