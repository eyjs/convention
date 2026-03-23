<template>
  <div>
    <!-- 목록 뷰 -->
    <div v-if="currentView === 'list'">
      <AdminPageHeader
        title="설문 관리"
        :description="`전체 ${surveys.length}개`"
      >
        <AdminButton :icon="Plus" @click="openCreateModal"
          >새 설문 생성</AdminButton
        >
      </AdminPageHeader>

      <div class="mt-6">
        <div v-if="loading" class="text-center py-10">
          <p class="text-gray-500">설문 목록을 불러오는 중...</p>
        </div>
        <div v-else-if="error" class="text-center py-10 text-red-500">
          <p>{{ error }}</p>
        </div>
        <AdminEmptyState
          v-else-if="surveys.length === 0"
          :icon="ClipboardList"
          title="등록된 설문이 없습니다"
          description="새 설문을 생성하여 참석자 의견을 수집하세요"
        >
          <AdminButton :icon="Plus" @click="openCreateModal"
            >새 설문 생성</AdminButton
          >
        </AdminEmptyState>
        <AdminTable
          v-else
          :columns="tableColumns"
          :loading="loading"
          :empty="surveys.length === 0"
        >
          <tr
            v-for="survey in surveys"
            :key="survey.id"
            class="hover:bg-gray-50"
          >
            <td
              class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900"
            >
              {{ survey.title }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap">
              <AdminBadge :variant="survey.isActive ? 'success' : 'danger'">
                {{ survey.isActive ? '활성' : '비활성' }}
              </AdminBadge>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
              {{ new Date(survey.createdAt).toLocaleDateString() }}
            </td>
            <td
              class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-3"
            >
              <button
                class="text-primary-600 hover:text-primary-900"
                @click="copySurveyUrl(survey.id)"
              >
                URL 복사
              </button>
              <button
                class="text-primary-600 hover:text-primary-900"
                @click="showEditView(survey.id)"
              >
                수정
              </button>
              <button
                class="text-green-600 hover:text-green-900"
                @click="showStatsView(survey.id)"
              >
                통계
              </button>
            </td>
          </tr>
        </AdminTable>
      </div>
    </div>

    <!-- 수정 뷰 -->
    <div v-else-if="currentView === 'edit'">
      <SurveyForm
        :survey-id="selectedSurveyId"
        :convention-id="conventionId"
        @cancel="goBackToList"
        @saved="handleSurveySaved"
      />
    </div>

    <!-- 통계 뷰 -->
    <div v-else-if="currentView === 'stats'">
      <SurveyStats :survey-id="selectedSurveyId" @back="goBackToList" />
    </div>

    <!-- 생성 모달 (항상 존재하지만 isOpen prop으로 표시 여부 제어) -->
    <BaseModal
      :is-open="isCreateModalVisible"
      @close="isCreateModalVisible = false"
    >
      <template #header>새 설문 생성</template>
      <template #body>
        <SurveyForm
          :convention-id="conventionId"
          @cancel="isCreateModalVisible = false"
          @saved="handleSurveySaved"
        />
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { Plus, ClipboardList } from 'lucide-vue-next'
import api from '@/services/api'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminButton from '@/components/admin/ui/AdminButton.vue'
import AdminTable from '@/components/admin/ui/AdminTable.vue'
import AdminBadge from '@/components/admin/ui/AdminBadge.vue'
import AdminEmptyState from '@/components/admin/ui/AdminEmptyState.vue'
import BaseModal from '@/components/common/BaseModal.vue'
import SurveyForm from '@/views/admin/survey/SurveyForm.vue' // Re-using the form view
import SurveyStats from '@/views/admin/survey/SurveyStats.vue' // Re-using the stats view

const props = defineProps({
  conventionId: {
    type: Number,
    required: true,
  },
})

const surveys = ref([])
const loading = ref(true)
const error = ref(null)

// View management
const currentView = ref('list') // 'list', 'edit', 'stats'
const selectedSurveyId = ref(null)
const isCreateModalVisible = ref(false)

const tableColumns = [
  { key: 'title', label: '제목' },
  { key: 'status', label: '상태' },
  { key: 'createdAt', label: '생성일' },
  { key: 'actions', label: '', align: 'right' },
]

async function fetchSurveys() {
  loading.value = true
  error.value = null
  try {
    const response = await api.get('/surveys')
    surveys.value = response.data.filter(
      (s) => s.conventionId === props.conventionId,
    )
  } catch (err) {
    error.value = '설문 목록을 불러오는데 실패했습니다.'
    console.error(err)
  } finally {
    loading.value = false
  }
}

function showEditView(id) {
  selectedSurveyId.value = id
  currentView.value = 'edit'
}

function showStatsView(id) {
  selectedSurveyId.value = id
  currentView.value = 'stats'
}

function goBackToList() {
  currentView.value = 'list'
  selectedSurveyId.value = null
  fetchSurveys() // Refresh list after edit/stats
}

function openCreateModal() {
  selectedSurveyId.value = null // Ensure it's a create operation
  isCreateModalVisible.value = true
}

function handleSurveySaved() {
  isCreateModalVisible.value = false
  goBackToList()
}

async function copySurveyUrl(surveyId) {
  const surveyPath = `/surveys/${surveyId}`
  try {
    await navigator.clipboard.writeText(surveyPath)
    alert('설문 경로가 클립보드에 복사되었습니다!')
  } catch (err) {
    console.error('URL 복사 실패:', err)
    alert('URL 복사에 실패했습니다. 수동으로 복사해주세요: ' + surveyPath)
  }
}

onMounted(() => {
  fetchSurveys()
})

watch(
  () => props.conventionId,
  () => {
    fetchSurveys()
  },
)
</script>
