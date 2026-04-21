import { computed } from 'vue'
import { useRoute } from 'vue-router'
import {
  LayoutDashboard,
  Users,
  FileText,
  Calendar,
  Zap,
  ClipboardList,
  FormInput,
  Upload,
  FolderOpen,
  BarChart3,
  BookOpen,
  MessageSquare,
  LayoutGrid,
  Image,
} from 'lucide-vue-next'

const rootNavItems = [
  {
    key: 'conventions',
    label: '행사 관리',
    icon: FolderOpen,
    routeName: 'AdminConventionList',
    path: '/admin/conventions',
  },
  {
    key: 'users',
    label: '회원 관리',
    icon: Users,
    routeName: 'AdminUsers',
    path: '/admin/users',
  },
  {
    key: 'statistics',
    label: '통계',
    icon: BarChart3,
    routeName: 'AdminStatistics',
    path: '/admin/statistics',
  },
  {
    key: 'home-banners',
    label: '홈 배너 관리',
    icon: Image,
    routeName: 'AdminHomeBanners',
    path: '/admin/home-banners',
  },
  {
    key: 'guide',
    label: '사용 가이드',
    icon: BookOpen,
    routeName: 'AdminGuide',
    path: '/admin/guide',
  },
]

const conventionNavItems = [
  {
    key: 'dashboard',
    label: '대시보드',
    icon: LayoutDashboard,
    path: 'dashboard',
    routeName: 'AdminDashboardOverview',
  },
  {
    key: 'guests',
    label: '참석자 관리',
    icon: Users,
    path: 'guests',
    routeName: 'AdminGuests',
  },
  {
    key: 'board',
    label: '게시판 관리',
    icon: FileText,
    path: 'board',
    routeName: 'AdminBoard',
  },
  {
    key: 'schedules',
    label: '일정 관리',
    icon: Calendar,
    path: 'schedules',
    routeName: 'AdminSchedules',
  },
  {
    key: 'actions',
    label: '액션 관리',
    icon: Zap,
    path: 'actions',
    routeName: 'AdminActions',
  },
  {
    key: 'surveys',
    label: '설문 관리',
    icon: ClipboardList,
    path: 'surveys',
    routeName: 'AdminSurveys',
  },
  {
    key: 'option-tour-surveys',
    label: '옵션투어 설문',
    icon: ClipboardList,
    path: 'option-tour-surveys',
    routeName: 'AdminOptionTourSurveys',
  },
  {
    key: 'formbuilder',
    label: '폼 빌더',
    icon: FormInput,
    path: 'formbuilder',
    routeName: 'AdminFormBuilder',
  },
  {
    key: 'upload',
    label: '엑셀 업로드',
    icon: Upload,
    path: 'upload',
    routeName: 'AdminUpload',
  },
  {
    key: 'sns',
    label: 'SNS 발송',
    icon: MessageSquare,
    path: 'sns',
    routeName: 'AdminSns',
  },
  {
    key: 'notifications',
    label: '알림 발송',
    icon: MessageSquare,
    path: 'notifications',
    routeName: 'AdminNotifications',
  },
  {
    key: 'seating',
    label: '좌석 배치도',
    icon: LayoutGrid,
    path: 'seating',
    routeName: 'AdminSeatingList',
  },
  {
    key: 'attribute-categories',
    label: '속성 카테고리',
    icon: FolderOpen,
    path: 'attribute-categories',
    routeName: 'AdminAttributeCategories',
  },
]

export function useAdminNav() {
  const route = useRoute()

  const isConventionLevel = computed(() => !!route.params.id)

  const navItems = computed(() =>
    isConventionLevel.value ? conventionNavItems : rootNavItems,
  )

  const activeKey = computed(() => {
    const name = route.name
    const items = navItems.value
    for (const item of items) {
      if (item.routeName === name) return item.key
    }
    return items[0]?.key || ''
  })

  function getPath(cId, item) {
    if (item.path.startsWith('/')) return item.path
    return `/admin/conventions/${cId}/${item.path}`
  }

  return { navItems, activeKey, getPath, isConventionLevel }
}
