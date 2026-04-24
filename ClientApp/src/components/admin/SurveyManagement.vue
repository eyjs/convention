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
        :description="`전체 ${filteredSurveys.length}개`"
      >
        <AdminButton :icon="Plus" @click="showCreateView"
          >새 설문 생성</AdminButton
        >
      </AdminPageHeader>

      <!-- 요약 통계 -->
      <div class="mt-4 grid grid-cols-1 sm:grid-cols-3 gap-3">
        <div
          class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-3 flex items-center gap-3"
        >
          <div
            class="w-9 h-9 rounded-lg bg-primary-50 dark:bg-primary-900/30 flex items-center justify-center"
          >
            <ClipboardList
              :size="18"
              class="text-primary-600 dark:text-primary-400"
            />
          </div>
          <div>
            <p class="text-xs font-medium text-gray-500 dark:text-gray-400">
              전체 설문
            </p>
            <p class="text-lg font-bold text-gray-900 dark:text-gray-100">
              {{ surveys.length }}
            </p>
          </div>
        </div>
        <div
          class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-3 flex items-center gap-3"
        >
          <div
            class="w-9 h-9 rounded-lg bg-emerald-50 dark:bg-emerald-900/30 flex items-center justify-center"
          >
            <Eye :size="18" class="text-emerald-600 dark:text-emerald-400" />
          </div>
          <div>
            <p class="text-xs font-medium text-gray-500 dark:text-gray-400">
              활성 설문
            </p>
            <p class="text-lg font-bold text-emerald-600 dark:text-emerald-400">
              {{ activeSurveyCount }}
            </p>
          </div>
        </div>
        <div
          class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-3 flex items-center gap-3"
        >
          <div
            class="w-9 h-9 rounded-lg bg-blue-50 dark:bg-blue-900/30 flex items-center justify-center"
          >
            <BarChart3 :size="18" class="text-blue-600 dark:text-blue-400" />
          </div>
          <div>
            <p class="text-xs font-medium text-gray-500 dark:text-gray-400">
              총 응답
            </p>
            <p class="text-lg font-bold text-blue-600 dark:text-blue-400">
              {{ totalResponses }}
            </p>
          </div>
        </div>
      </div>

      <!-- 검색/필터 -->
      <div class="mt-4 flex flex-col sm:flex-row gap-3">
        <div class="flex-1">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="설문 제목 검색..."
            class="w-full px-3 py-2.5 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 text-sm focus:ring-primary-500 focus:border-primary-500"
          />
        </div>
        <select
          v-model="statusFilter"
          class="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 text-sm"
        >
          <option value="all">전체 상태</option>
          <option value="active">활성</option>
          <option value="inactive">비활성</option>
          <option value="expired">기간만료</option>
        </select>
      </div>

      <div class="mt-6">
        <div v-if="loading" class="text-center py-10">
          <p class="text-gray-500">설문 목록을 불러오는 중...</p>
        </div>
        <div v-else-if="error" class="text-center py-10 text-red-500">
          <p>{{ error }}</p>
        </div>
        <AdminEmptyState
          v-else-if="filteredSurveys.length === 0"
          :icon="ClipboardList"
          title="등록된 설문이 없습니다"
          :description="
            searchQuery || statusFilter !== 'all'
              ? '검색 조건에 맞는 설문이 없습니다'
              : '새 설문을 생성하여 참석자 의견을 수집하세요'
          "
        >
          <AdminButton
            v-if="!searchQuery && statusFilter === 'all'"
            :icon="Plus"
            @click="showCreateView"
            >새 설문 생성</AdminButton
          >
        </AdminEmptyState>
        <!-- 모바일 카드 -->
        <div v-else class="md:hidden space-y-2">
          <div
            v-for="survey in filteredSurveys"
            :key="'m-' + survey.id"
            class="bg-white rounded-lg shadow-sm overflow-hidden"
          >
            <div class="p-3 active:bg-gray-50" @click="showEditView(survey)">
              <div class="flex items-center justify-between mb-1">
                <span class="font-semibold text-gray-900 truncate">{{
                  survey.title
                }}</span>
                <AdminBadge
                  :variant="getSurveyStatusVariant(survey)"
                  class="flex-shrink-0 ml-2"
                  >{{ getSurveyStatusLabel(survey) }}</AdminBadge
                >
              </div>
              <p class="text-xs text-gray-500">
                응답 {{ survey.responseCount ?? 0 }}명 ·
                {{ survey.questionCount ?? 0 }}문항
              </p>
              <p
                v-if="survey.startDate || survey.endDate"
                class="text-xs text-gray-400 mt-0.5"
              >
                {{ survey.startDate?.split('T')[0] || '' }} ~
                {{ survey.endDate?.split('T')[0] || '' }}
              </p>
            </div>
            <div class="flex border-t divide-x">
              <button
                class="flex-1 py-2.5 text-xs font-medium text-primary-600 active:bg-primary-50"
                @click="showStatsView(survey)"
              >
                통계
              </button>
              <button
                class="flex-1 py-2.5 text-xs font-medium text-gray-600 active:bg-gray-50"
                @click="showEditView(survey)"
              >
                수정
              </button>
              <button
                class="flex-1 py-2.5 text-xs font-medium text-red-600 active:bg-red-50"
                @click="deleteSurvey(survey.id)"
              >
                삭제
              </button>
            </div>
          </div>
        </div>

        <!-- PC 테이블 -->
        <AdminTable
          v-if="filteredSurveys.length > 0"
          class="hidden md:block"
          :columns="tableColumns"
          :loading="loading"
          :empty="filteredSurveys.length === 0"
        >
          <tr
            v-for="survey in filteredSurveys"
            :key="survey.id"
            class="hover:bg-gray-50 dark:hover:bg-gray-700/50"
          >
            <td
              class="px-4 py-3 text-sm font-medium text-gray-900 dark:text-gray-100"
            >
              <span :title="survey.description || ''" class="cursor-default">
                {{ survey.title }}
                <span
                  v-if="survey.description"
                  class="block text-xs text-gray-400 dark:text-gray-500 font-normal truncate max-w-xs"
                >
                  {{ survey.description }}
                </span>
              </span>
            </td>
            <td class="px-4 py-3 whitespace-nowrap">
              <AdminBadge :variant="getSurveyStatusVariant(survey)">
                {{ getSurveyStatusLabel(survey) }}
              </AdminBadge>
            </td>
            <td
              class="px-4 py-3 whitespace-nowrap text-sm text-gray-900 dark:text-gray-100"
            >
              {{ survey.responseCount ?? 0 }}명
            </td>
            <td
              class="px-4 py-3 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400"
            >
              {{ formatDateRange(survey) }}
            </td>
            <td
              class="px-4 py-3 whitespace-nowrap text-sm text-gray-900 dark:text-gray-100"
            >
              {{ formatDate(survey.createdAt) }}
            </td>
            <td class="px-4 py-3 whitespace-nowrap text-right">
              <div class="inline-flex items-center gap-1">
                <!-- prettier-ignore -->
                <button
                  title="미리보기 (URL 복사 + 새 탭)"
                  class="p-1.5 rounded-md text-gray-400 hover:text-primary-600 hover:bg-primary-50 dark:hover:bg-primary-900/30 transition-colors"
                  @click="previewSurvey(survey.id)"
                >
                  <Eye :size="16" />
                </button>
                <!-- prettier-ignore -->
                <button
                  title="URL 복사"
                  class="p-1.5 rounded-md text-gray-400 hover:text-primary-600 hover:bg-primary-50 dark:hover:bg-primary-900/30 transition-colors"
                  @click="copySurveyUrl(survey.id)"
                >
                  <Copy :size="16" />
                </button>
                <!-- prettier-ignore -->
                <button
                  title="수정"
                  class="p-1.5 rounded-md text-gray-400 hover:text-primary-600 hover:bg-primary-50 dark:hover:bg-primary-900/30 transition-colors"
                  @click="showEditView(survey.id)"
                >
                  <Pencil :size="16" />
                </button>
                <!-- prettier-ignore -->
                <button
                  title="통계"
                  class="p-1.5 rounded-md text-gray-400 hover:text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/30 transition-colors"
                  @click="showStatsView(survey.id)"
                >
                  <BarChart3 :size="16" />
                </button>
                <!-- prettier-ignore -->
                <button
                  title="삭제"
                  class="p-1.5 rounded-md text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/30 transition-colors"
                  @click="confirmDeleteSurvey(survey)"
                >
                  <Trash2 :size="16" />
                </button>
              </div>
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
          <span class="font-semibold text-gray-900 dark:text-gray-100">{{
            deletingTitle
          }}</span>
          설문을 삭제하시겠습니까?
        </p>
        <p class="text-sm text-red-500 mt-2">
          연관된 응답 데이터와 액션도 함께 삭제됩니다. 이 작업은 되돌릴 수
          없습니다.
        </p>
        <div class="flex justify-end gap-3 mt-6">
          <AdminButton
            variant="secondary"
            @click="isDeleteModalVisible = false"
          >
            취소
          </AdminButton>
          <AdminButton
            variant="danger"
            :icon="Trash2"
            :loading="isDeleting"
            :disabled="isDeleting"
            @click="deleteSurvey"
          >
            {{ isDeleting ? '삭제 중...' : '삭제' }}
          </AdminButton>
        </div>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import {
  Plus,
  ClipboardList,
  Pencil,
  BarChart3,
  Trash2,
  Copy,
  Eye,
} from 'lucide-vue-next'
import { formatDate } from '@/utils/date'
import { useSurveyManagement } from '@/composables/useSurveyManagement'
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

const {
  surveys,
  loading,
  error,
  currentView,
  selectedSurveyId,
  searchQuery,
  statusFilter,
  toastMessage,
  toastType,
  showToast,
  isDeleteModalVisible,
  deletingTitle,
  isDeleting,
  filteredSurveys,
  activeSurveyCount,
  totalResponses,
  getSurveyStatusVariant,
  getSurveyStatusLabel,
  formatDateRange,
  showCreateView,
  showEditView,
  showStatsView,
  goBackToList,
  handleSurveySaved,
  confirmDeleteSurvey,
  deleteSurvey,
} = useSurveyManagement(() => props.conventionId, 'GENERAL')

const tableColumns = [
  { key: 'title', label: '제목' },
  { key: 'status', label: '상태', width: '80px' },
  { key: 'responseCount', label: '응답', width: '80px' },
  { key: 'period', label: '기간' },
  { key: 'createdAt', label: '생성일', width: '100px' },
  { key: 'actions', label: '', align: 'right', width: '180px' },
]

function getSurveyUrl(surveyId) {
  return `${location.origin}/conventions/${props.conventionId}/surveys/${surveyId}`
}

async function copySurveyUrl(surveyId) {
  const surveyUrl = getSurveyUrl(surveyId)
  try {
    await navigator.clipboard.writeText(surveyUrl)
    showToast('설문 URL이 클립보드에 복사되었습니다.')
  } catch {
    showToast('URL 복사에 실패했습니다.', 'error')
  }
}

async function previewSurvey(surveyId) {
  const surveyUrl = getSurveyUrl(surveyId)
  try {
    await navigator.clipboard.writeText(surveyUrl)
    showToast('URL이 복사되었습니다. 새 탭에서 미리보기를 엽니다.')
  } catch {
    // URL 복사 실패해도 새 탭은 열기
  }
  window.open(surveyUrl, '_blank')
}
</script>
