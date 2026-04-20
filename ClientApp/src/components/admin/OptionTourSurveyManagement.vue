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
        title="옵션투어 설문 관리"
        :description="`전체 ${filteredSurveys.length}개`"
      >
        <AdminButton :icon="Plus" @click="showCreateView"
          >새 옵션투어 설문</AdminButton
        >
      </AdminPageHeader>

      <!-- 검색/필터 -->
      <div class="mt-4 flex flex-col sm:flex-row gap-3">
        <div class="flex-1">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="설문 제목 검색..."
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 text-sm focus:ring-indigo-500 focus:border-indigo-500"
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
          title="등록된 옵션투어 설문이 없습니다"
          :description="
            searchQuery || statusFilter !== 'all'
              ? '검색 조건에 맞는 설문이 없습니다'
              : '새 설문을 생성하여 옵션투어 선택을 받으세요'
          "
        >
          <AdminButton
            v-if="!searchQuery && statusFilter === 'all'"
            :icon="Plus"
            @click="showCreateView"
            >새 옵션투어 설문</AdminButton
          >
        </AdminEmptyState>
        <!-- 목록 (모바일 카드 + 데스크탑 테이블) -->
        <div v-else>
          <!-- 모바일 카드 뷰 (sm 미만) -->
          <div class="sm:hidden space-y-3">
            <div
              v-for="survey in filteredSurveys"
              :key="survey.id"
              class="bg-white border border-gray-200 rounded-lg p-4 shadow-sm"
            >
              <div class="flex items-start justify-between gap-2 mb-2">
                <span class="font-medium text-gray-900 text-sm leading-snug">{{
                  survey.title
                }}</span>
                <AdminBadge
                  :variant="getSurveyStatusVariant(survey)"
                  class="shrink-0"
                >
                  {{ getSurveyStatusLabel(survey) }}
                </AdminBadge>
              </div>
              <div class="text-xs text-gray-500 space-y-1 mb-3">
                <p>{{ formatDateRange(survey) }}</p>
                <p>참석자 {{ survey.responseCount ?? 0 }}명</p>
              </div>
              <div class="flex gap-3 text-sm font-medium">
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
              </div>
            </div>
          </div>

          <!-- 데스크탑 테이블 뷰 (sm 이상) -->
          <div class="hidden sm:block">
            <AdminTable
              :columns="tableColumns"
              :loading="loading"
              :empty="filteredSurveys.length === 0"
            >
              <tr
                v-for="survey in filteredSurveys"
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
                  {{ formatDate(survey.createdAt) }}
                </td>
                <td
                  class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-3"
                >
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
      </div>
    </div>

    <!-- 생성 뷰 -->
    <div v-else-if="currentView === 'create'">
      <OptionTourSurveyForm
        :convention-id="conventionId"
        @cancel="goBackToList"
        @saved="handleSurveySaved"
      />
    </div>

    <!-- 수정 뷰 -->
    <div v-else-if="currentView === 'edit'">
      <OptionTourSurveyForm
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
import { Plus, ClipboardList } from 'lucide-vue-next'
import { formatDate } from '@/utils/date'
import { useSurveyManagement } from '@/composables/useSurveyManagement'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminButton from '@/components/admin/ui/AdminButton.vue'
import AdminTable from '@/components/admin/ui/AdminTable.vue'
import AdminBadge from '@/components/admin/ui/AdminBadge.vue'
import AdminEmptyState from '@/components/admin/ui/AdminEmptyState.vue'
import BaseModal from '@/components/common/BaseModal.vue'
import OptionTourSurveyForm from '@/views/admin/survey/OptionTourSurveyForm.vue'
import SurveyStats from '@/views/admin/survey/SurveyStats.vue'

const props = defineProps({
  conventionId: {
    type: Number,
    required: true,
  },
})

const {
  loading,
  error,
  currentView,
  selectedSurveyId,
  searchQuery,
  statusFilter,
  toastMessage,
  toastType,
  isDeleteModalVisible,
  deletingTitle,
  isDeleting,
  filteredSurveys,
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
} = useSurveyManagement(() => props.conventionId, 'OPTION_TOUR')

const tableColumns = [
  { key: 'title', label: '제목' },
  { key: 'status', label: '상태' },
  { key: 'responseCount', label: '응답 수' },
  { key: 'period', label: '기간' },
  { key: 'createdAt', label: '생성일' },
  { key: 'actions', label: '', align: 'right' },
]
</script>
