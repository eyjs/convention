<template>
  <div>
    <!-- 목록 뷰 -->
    <div v-if="currentView === 'list'" class="bg-white dark:bg-gray-800 p-4 sm:p-6 rounded-lg shadow-md">
      <div class="flex justify-between items-center mb-4">
        <h2 class="text-xl font-bold text-gray-800 dark:text-gray-200">설문 관리</h2>
        <button @click="openCreateModal" class="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 text-sm font-medium">
          새 설문 생성
        </button>
      </div>

      <div v-if="loading" class="text-center py-10">
        <p class="text-gray-500 dark:text-gray-400">설문 목록을 불러오는 중...</p>
      </div>
      <div v-else-if="error" class="text-center py-10 text-red-500">
        <p>{{ error }}</p>
      </div>
      <div v-else class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead class="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">제목</th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">상태</th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">생성일</th>
              <th scope="col" class="relative px-6 py-3"><span class="sr-only">Actions</span></th>
            </tr>
          </thead>
          <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
            <tr v-for="survey in surveys" :key="survey.id" class="hover:bg-gray-50 dark:hover:bg-gray-700/50">
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-gray-200">{{ survey.title }}</td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span :class="['px-2 inline-flex text-xs leading-5 font-semibold rounded-full', survey.isActive ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100' : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-100']">
                  {{ survey.isActive ? '활성' : '비활성' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-gray-300">{{ new Date(survey.createdAt).toLocaleDateString() }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <button @click="copySurveyUrl(survey.id)" class="text-blue-600 hover:text-blue-900 dark:text-blue-400 dark:hover:text-blue-300 mr-4">URL 복사</button>
                <button @click="showEditView(survey.id)" class="text-indigo-600 hover:text-indigo-900 dark:text-indigo-400 dark:hover:text-indigo-300 mr-4">수정</button>
                <button @click="showStatsView(survey.id)" class="text-green-600 hover:text-green-900 dark:text-green-400 dark:hover:text-green-300">통계</button>
              </td>
            </tr>
            <tr v-if="surveys.length === 0">
              <td colspan="4" class="px-6 py-4 text-center text-sm text-gray-500 dark:text-gray-400">이 행사에 해당하는 설문이 없습니다.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- 수정 뷰 -->
    <div v-else-if="currentView === 'edit'">
      <SurveyForm :survey-id="selectedSurveyId" :convention-id="conventionId" @cancel="goBackToList" @saved="handleSurveySaved" />
    </div>

    <!-- 통계 뷰 -->
    <div v-else-if="currentView === 'stats'">
      <SurveyStats :survey-id="selectedSurveyId" @back="goBackToList" />
    </div>

    <!-- 생성 모달 (항상 존재하지만 isOpen prop으로 표시 여부 제어) -->
    <BaseModal :is-open="isCreateModalVisible" @close="isCreateModalVisible = false">
        <template #header>새 설문 생성</template>
        <template #body>
            <SurveyForm :convention-id="conventionId" @cancel="isCreateModalVisible = false" @saved="handleSurveySaved" />
        </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import api from '@/api';
import BaseModal from '@/components/common/BaseModal.vue';
import SurveyForm from '@/views/admin/survey/SurveyForm.vue'; // Re-using the form view
import SurveyStats from '@/views/admin/survey/SurveyStats.vue'; // Re-using the stats view

const props = defineProps({
  conventionId: {
    type: Number,
    required: true,
  },
});

const surveys = ref([]);
const loading = ref(true);
const error = ref(null);

// View management
const currentView = ref('list'); // 'list', 'edit', 'stats'
const selectedSurveyId = ref(null);
const isCreateModalVisible = ref(false);

async function fetchSurveys() {
  loading.value = true;
  error.value = null;
  try {
    const response = await api.get('/surveys');
    surveys.value = response.data.filter(s => s.conventionId === props.conventionId);
  } catch (err) {
    error.value = "설문 목록을 불러오는데 실패했습니다.";
    console.error(err);
  } finally {
    loading.value = false;
  }
}

function showEditView(id) {
  selectedSurveyId.value = id;
  currentView.value = 'edit';
}

function showStatsView(id) {
  selectedSurveyId.value = id;
  currentView.value = 'stats';
}

function goBackToList() {
  currentView.value = 'list';
  selectedSurveyId.value = null;
  fetchSurveys(); // Refresh list after edit/stats
}

function openCreateModal() {
  selectedSurveyId.value = null; // Ensure it's a create operation
  isCreateModalVisible.value = true;
}

function handleSurveySaved() {
    isCreateModalVisible.value = false;
    goBackToList();
}

async function copySurveyUrl(surveyId) {
  const surveyPath = `/surveys/${surveyId}`;
  try {
    await navigator.clipboard.writeText(surveyPath);
    alert('설문 경로가 클립보드에 복사되었습니다!');
  } catch (err) {
    console.error('URL 복사 실패:', err);
    alert('URL 복사에 실패했습니다. 수동으로 복사해주세요: ' + surveyPath);
  }
}

onMounted(() => {
  fetchSurveys();
});

watch(() => props.conventionId, () => {
    fetchSurveys();
});

</script>