import { computed } from 'vue'
import { useRoute } from 'vue-router'
import {
  LayoutDashboard,
  Users,
  FileText,
  Calendar,
  Tags,
  Zap,
  ClipboardList,
  FormInput,
  Upload,
  Bell,
  FolderOpen,
  BarChart3,
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
    key: 'attributes',
    label: '속성 템플릿',
    icon: Tags,
    path: 'attributes',
    routeName: 'AdminAttributes',
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
    key: 'notices',
    label: '공지사항',
    icon: Bell,
    path: 'notices',
    routeName: 'AdminNotices',
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
