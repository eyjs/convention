<template>
  <div>
    <!-- 토스트 메시지 -->
    <div
      v-if="toastMessage"
      class="fixed top-4 right-4 z-50 px-4 py-3 rounded-lg shadow-lg text-white text-sm transition-all"
      :class="toastType === 'success' ? 'bg-green-500' : 'bg-red-500'"
    >
      {{ toastMessage }}
    </div>

    <!-- 목록 뷰 -->
    <div v-if="currentView === 'list'">
      <AdminPageHeader
        title="설문 관리"
        :description="`전체 ${surveys.length}개`"
      >
        <AdminButton :icon="Plus" @click="showCreateView"
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
          <AdminButton :icon="Plus" @click="showCreateView"
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
              <AdminBadge :variant="getSurveyStatusVariant(survey)">
                {{ getSurveyStatusLabel(survey) }}
              </AdminBadge>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
              {{ survey.responseCount ?? 0 }}명
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
              {{ formatDateRange(survey) }}
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
              <button
                class="text-red-600 hover:text-red-900"
                @click="confirmDeleteSurvey(survey)"
              >
                삭제
              </button>
            </td>
          </tr>
        </AdminTable>
      </div>
    </div>

    <!-- 생성 뷰 -->
    <div v-else-if="currentView === 'create'">
      <SurveyForm
        :convention-id="conventionId"
        @cancel="goBackToList"
        @saved="handleSurveySaved"
      />
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

    <!-- 삭제 확인 모달 -->
    <BaseModal
      :is-open="isDeleteModalVisible"
      @close="isDeleteModalVisible = false"
    >
      <template #header>설문 삭제 확인</template>
      <template #body>
        <p class="text-gray-700 dark:text-gray-300">
          <span class="font-semibold">{{ deletingTitle }}</span> 설문을
          삭제하시겠습니까?
        </p>
        <p class="text-sm text-red-500 mt-2">
          연관된 응답 데이터와 액션도 함께 삭제됩니다. 이 작업은 되돌릴 수
          없습니다.
        </p>
        <div class="flex justify-end space-x-3 mt-6">
          <button
            class="px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300 text-sm font-medium"
            @click="isDeleteModalVisible = false"
          >
            취소
          </button>
          <button
            class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 text-sm font-medium"
            :disabled="isDeleting"
            @click="deleteSurvey"
          >
            {{ isDeleting ? '삭제 중...' : '삭제' }}
          </button>
        </div>
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
import SurveyForm from '@/views/admin/survey/SurveyForm.vue'
import SurveyStats from '@/views/admin/survey/SurveyStats.vue'

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
const currentView = ref('list')
const selectedSurveyId = ref(null)

// Toast
const toastMessage = ref('')
const toastType = ref('success')
let toastTimer = null

// Delete
const isDeleteModalVisible = ref(false)
const deletingSurveyId = ref(null)
const deletingTitle = ref('')
const isDeleting = ref(false)

const tableColumns = [
  { key: 'title', label: '제목' },
  { key: 'status', label: '상태' },
  { key: 'responseCount', label: '응답 수' },
  { key: 'period', label: '기간' },
  { key: 'createdAt', label: '생성일' },
  { key: 'actions', label: '', align: 'right' },
]

function showToast(message, type = 'success') {
  toastMessage.value = message
  toastType.value = type
  if (toastTimer) clearTimeout(toastTimer)
  toastTimer = setTimeout(() => {
    toastMessage.value = ''
  }, 3000)
}

function getSurveyStatusVariant(survey) {
  if (!survey.isActive) return 'danger'
  if (survey.endDate && new Date(survey.endDate) < new Date()) return 'warning'
  return 'success'
}

function getSurveyStatusLabel(survey) {
  if (!survey.isActive) return '비활성'
  if (survey.endDate && new Date(survey.endDate) < new Date()) return '기간만료'
  return '활성'
}

function formatDateRange(survey) {
  if (!survey.startDate && !survey.endDate) return '-'
  const fmt = (d) => new Date(d).toLocaleDateString()
  if (survey.startDate && survey.endDate)
    return `${fmt(survey.startDate)} ~ ${fmt(survey.endDate)}`
  if (survey.startDate) return `${fmt(survey.startDate)} ~`
  return `~ ${fmt(survey.endDate)}`
}

async function fetchSurveys() {
  loading.value = true
  error.value = null
  try {
    const response = await api.get('/surveys')
    surveys.value = response.data.filter(
      (s) => s.conventionId === props.conventionId,
    )
  } catch {
    error.value = '설문 목록을 불러오는데 실패했습니다.'
  } finally {
    loading.value = false
  }
}

function showCreateView() {
  selectedSurveyId.value = null
  currentView.value = 'create'
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
  fetchSurveys()
}

function handleSurveySaved() {
  showToast('설문이 저장되었습니다.')
  goBackToList()
}

async function copySurveyUrl(surveyId) {
  const surveyUrl = `${location.origin}/conventions/${props.conventionId}/surveys/${surveyId}`
  try {
    await navigator.clipboard.writeText(surveyUrl)
    showToast('설문 URL이 클립보드에 복사되었습니다.')
  } catch {
    showToast('URL 복사에 실패했습니다.', 'error')
  }
}

function confirmDeleteSurvey(survey) {
  deletingSurveyId.value = survey.id
  deletingTitle.value = survey.title
  isDeleteModalVisible.value = true
}

async function deleteSurvey() {
  isDeleting.value = true
  try {
    await api.delete(`/surveys/${deletingSurveyId.value}`)
    isDeleteModalVisible.value = false
    showToast('설문이 삭제되었습니다.')
    fetchSurveys()
  } catch {
    showToast('설문 삭제에 실패했습니다.', 'error')
  } finally {
    isDeleting.value = false
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
