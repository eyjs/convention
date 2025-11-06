<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <!-- 헤더 -->
    <header class="bg-white shadow-sm sticky top-0 z-40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <div class="flex items-center gap-2">
            <button
              @click="router.push('/admin')"
              class="p-2 hover:bg-gray-100 rounded-lg transition-colors"
              title="행사 목록으로"
            >
              <svg
                class="w-5 h-5 text-gray-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M10 19l-7-7m0 0l7-7m-7 7h18"
                />
              </svg>
            </button>
            <h1 class="text-lg sm:text-xl md:text-2xl font-bold text-gray-900">
              행사 관리
            </h1>
            <span
              v-if="convention"
              class="ml-2 sm:ml-4 text-xs sm:text-sm text-gray-500"
              >{{ convention.title }}</span
            >
          </div>
          <div class="relative">
            <button
              @click="showUserMenu = !showUserMenu"
              class="p-2 hover:bg-gray-100 rounded-lg"
            >
              <svg
                class="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 5v.01M12 12v.01M12 19v.01"
                />
              </svg>
            </button>

            <Transition name="fade-down">
              <div
                v-if="showUserMenu"
                class="fixed inset-0 z-50"
                @click="showUserMenu = false"
              >
                <div
                  class="absolute top-16 right-4 w-56 bg-white rounded-md shadow-lg border"
                >
                  <ul>
                    <li>
                      <router-link
                        to="/"
                        class="block w-full text-left px-4 py-3 text-sm text-gray-700 hover:bg-gray-100"
                      >
                        사용자 홈으로
                      </router-link>
                    </li>
                    <li>
                      <button
                        @click="handleLogout"
                        class="block w-full text-left px-4 py-3 text-sm text-red-600 hover:bg-gray-100"
                      >
                        로그아웃
                      </button>
                    </li>
                  </ul>
                </div>
              </div>
            </Transition>
          </div>
        </div>
      </div>
    </header>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
      <!-- 탭 네비게이션 -->
      <div class="mb-6 overflow-x-auto">
        <nav class="flex space-x-1 sm:space-x-4 border-b min-w-max">
          <button
            v-for="tab in tabs"
            :key="tab.id"
            @click="activeTab = tab.id"
            :class="[
              'pb-3 px-3 sm:px-4 border-b-2 font-medium text-sm whitespace-nowrap transition-colors',
              activeTab === tab.id
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
            ]"
          >
            <span class="hidden sm:inline">{{ tab.name }}</span>
            <span class="sm:hidden">{{ tab.shortName || tab.name }}</span>
          </button>
        </nav>
      </div>

      <!-- 대시보드 -->
      <div v-if="activeTab === 'dashboard'">
        <DashboardOverview
          :convention-id="conventionId"
          @show-guest="showGuestFromDashboard"
        />
      </div>

      <!-- 참석자 관리 -->
      <div v-if="activeTab === 'guests'">
        <GuestManagement :convention-id="conventionId" />
      </div>

      <!-- 일정 관리 -->
      <div v-if="activeTab === 'schedules'">
        <ScheduleManagement :convention-id="conventionId" />
      </div>

      <!-- 액션 관리 -->
      <div v-if="activeTab === 'actions'">
        <ActionManagement :convention-id="conventionId" />
      </div>

      <!-- 속성 템플릿 -->
      <div v-if="activeTab === 'attributes'">
        <AttributeTemplateManagement :convention-id="conventionId" />
      </div>

      <!-- 게시판 -->
      <div v-if="activeTab === 'board'">
        <BoardManagement :convention-id="conventionId" />
      </div>

      <!-- 설문 관리 -->
      <div v-if="activeTab === 'surveys'">
        <SurveyManagement :convention-id="conventionId" />
      </div>

      <!-- 업로드 -->
      <div v-if="activeTab === 'upload'">
        <BulkUpload :convention-id="conventionId" />
      </div>

      <!-- 데이터베이스 -->
      <div v-if="activeTab === 'database'">
        <DatabaseMigration />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { conventionAPI } from '@/services/api'
import DashboardOverview from '@/components/admin/DashboardOverview.vue'
import GuestManagement from '@/components/admin/GuestManagement.vue'
import DatabaseMigration from '@/components/admin/DatabaseMigration.vue'
import ScheduleManagement from '@/components/admin/ScheduleManagement.vue'
import ActionManagement from '@/components/admin/ActionManagement.vue'
import BulkUpload from '@/components/admin/BulkUpload.vue'
import AttributeTemplateManagement from '@/components/admin/AttributeTemplateManagement.vue'
import BoardManagement from '@/components/admin/BoardManagement.vue'
import SurveyManagement from '@/components/admin/SurveyManagement.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const conventionId = ref(parseInt(route.params.id))
const convention = ref(null)
const activeTab = ref('dashboard')
const showMenu = ref(false)
const showUserMenu = ref(false)

const showGuestFromDashboard = (guestId) => {
  activeTab.value = 'guests'
  // GuestManagement 컴포넌트에서 상세 표시하도록 이벤트 전달 필요
}

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
  }
}

const tabs = [
  { id: 'dashboard', name: '대시보드', shortName: '대시보드' },
  { id: 'guests', name: '참석자 관리', shortName: '참석자' },
  { id: 'schedules', name: '일정 관리', shortName: '일정' },
  { id: 'actions', name: '액션 관리', shortName: '액션' },
  { id: 'surveys', name: '설문 관리', shortName: '설문' },
  { id: 'attributes', name: '속성 템플릿', shortName: '속성' },
  { id: 'board', name: '게시판 관리', shortName: '게시판' },
  { id: 'upload', name: '엑셀 업로드', shortName: '업로드' },
  { id: 'database', name: 'DB 관리', shortName: 'DB' },
]

onMounted(async () => {
  // 행사 정보 로드
  try {
    const response = await conventionAPI.getConvention(conventionId.value)
    convention.value = response.data
  } catch (error) {
    console.error('Failed to load convention:', error)
    alert('행사 정보를 불러오는데 실패했습니다.')
    router.push('/admin')
  }
})
</script>

<style scoped>
.fade-down-enter-active,
.fade-down-leave-active {
  transition: all 0.2s ease-out;
}
.fade-down-enter-from,
.fade-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
