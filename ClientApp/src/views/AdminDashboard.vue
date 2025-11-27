<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <AdminHeader 
      title="행사 관리" 
      :subtitle="convention ? convention.title : ''"
      show-back-button 
      back-path="/admin"
      back-button-title="행사 목록으로"
    />

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
          <!-- Form Builder Tab -->
          <button
            @click="activeTab = 'formbuilder'"
            :class="[
              'pb-3 px-3 sm:px-4 border-b-2 font-medium text-sm whitespace-nowrap transition-colors',
              activeTab === 'formbuilder'
                ? 'border-primary-600 text-primary-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
            ]"
          >
            <span class="hidden sm:inline">폼 빌더</span>
            <span class="sm:hidden">폼 빌더</span>
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

      <!-- 폼 빌더 -->
      <div v-if="activeTab === 'formbuilder'">
        <FormBuilderManagement :convention-id="conventionId" />
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
    
    <ConventionFormModal
      v-if="showEditModal"
      :convention="convention"
      @close="showEditModal = false"
      @save="handleSaveConvention"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { conventionAPI } from '@/services/api'
import AdminHeader from '@/components/admin/AdminHeader.vue'
import DashboardOverview from '@/components/admin/DashboardOverview.vue'
import GuestManagement from '@/components/admin/GuestManagement.vue'
import DatabaseMigration from '@/components/admin/DatabaseMigration.vue'
import ScheduleManagement from '@/components/admin/ScheduleManagement.vue'
import ActionManagement from '@/components/admin/ActionManagement.vue'
import BulkUpload from '@/components/admin/BulkUpload.vue'
import AttributeTemplateManagement from '@/components/admin/AttributeTemplateManagement.vue'
import BoardManagement from '@/components/admin/BoardManagement.vue'
import SurveyManagement from '@/components/admin/SurveyManagement.vue'
import FormBuilderManagement from '@/components/admin/FormBuilderManagement.vue'

const router = useRouter()
const route = useRoute()
const conventionId = computed(() => {
  const id = parseInt(route.params.id)
  return isNaN(id) ? null : id
})
const convention = ref(null)
const activeTab = ref('dashboard')
const showMenu = ref(false)
const loading = ref(true)

const showGuestFromDashboard = (guestId) => {
  activeTab.value = 'guests'
  // GuestManagement 컴포넌트에서 상세 표시하도록 이벤트 전달 필요
}

const tabs = [
    { id: 'dashboard', name: '대시보드', shortName: '대시보드' },
    { id: 'guests', name: '참석자 관리', shortName: '참석자' },
    { id: 'board', name: '게시판 관리', shortName: '게시판' },
    { id: 'schedules', name: '일정 관리', shortName: '일정' },
    { id: 'attributes', name: '속성 템플릿', shortName: '속성' },
    { id: 'actions', name: '액션 관리', shortName: '액션' },
    { id: 'surveys', name: '설문 관리', shortName: '설문' },
    { id: 'upload', name: '엑셀 업로드', shortName: '업로드' },
  /*{ id: 'database', name: 'DB 관리', shortName: 'DB' },*/
]

onMounted(async () => {
  // conventionId가 유효한지 확인
  if (!conventionId.value) {
    alert('행사 ID가 유효하지 않습니다.')
    router.push('/admin')
    return
  }

  // 행사 정보 로드
  try {
    const response = await conventionAPI.getConvention(conventionId.value)
    convention.value = response.data
  } catch (error) {
    console.error('Failed to load convention:', error)
    alert('행사 정보를 불러오는데 실패했습니다.')
    router.push('/admin')
  } finally {
    loading.value = false
  }
})
</script>
