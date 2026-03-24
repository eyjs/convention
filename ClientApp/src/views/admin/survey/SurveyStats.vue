<template>
  <div class="p-4 sm:p-6 bg-white dark:bg-gray-800 rounded-lg shadow-md">
    <div class="flex justify-between items-center mb-6">
      <h1
        class="text-2xl sm:text-3xl font-bold text-gray-800 dark:text-gray-200"
      >
        설문 통계
      </h1>
      <button
        class="px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300 font-semibold"
        @click="emit('back')"
      >
        목록으로 돌아가기
      </button>
    </div>

    <div v-if="loading" class="text-center py-10">
      <p class="text-gray-500 dark:text-gray-400">통계를 불러오는 중...</p>
    </div>
    <div v-else-if="error" class="text-center py-10 text-red-500">
      <p>{{ error }}</p>
    </div>
    <div v-else-if="stats">
      <h2 class="text-xl font-bold text-gray-800 dark:text-gray-200 mb-4">
        {{ stats.surveyTitle }}
      </h2>

      <!-- 탭 -->
      <div class="border-b border-gray-200 dark:border-gray-700 mb-6">
        <nav class="flex space-x-8">
          <button
            class="pb-3 px-1 text-sm font-medium border-b-2 transition-colors"
            :class="
              activeTab === 'summary'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            "
            @click="activeTab = 'summary'"
          >
            통계 요약
          </button>
          <button
            class="pb-3 px-1 text-sm font-medium border-b-2 transition-colors"
            :class="
              activeTab === 'responses'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            "
            @click="switchToResponses"
          >
            개별 응답
          </button>
        </nav>
      </div>

      <!-- 통계 요약 탭 -->
      <div v-if="activeTab === 'summary'">
        <!-- 요약 카드 -->
        <div class="grid grid-cols-2 gap-4 mb-6">
          <div
            class="bg-indigo-50 dark:bg-indigo-900/20 rounded-lg p-4 text-center"
          >
            <p class="text-sm text-gray-500 dark:text-gray-400">총 응답 수</p>
            <p class="text-3xl font-bold text-indigo-600 dark:text-indigo-400">
              {{ stats.totalResponses }}
            </p>
          </div>
          <div
            class="bg-green-50 dark:bg-green-900/20 rounded-lg p-4 text-center"
          >
            <p class="text-sm text-gray-500 dark:text-gray-400">질문 수</p>
            <p class="text-3xl font-bold text-green-600 dark:text-green-400">
              {{ stats.questionStats.length }}
            </p>
          </div>
        </div>

        <!-- 질문별 통계 -->
        <div
          v-for="(qStats, qIndex) in stats.questionStats"
          :key="qStats.questionId"
          class="bg-gray-50 dark:bg-gray-800/50 p-6 rounded-lg shadow-sm mb-6"
        >
          <h3
            class="text-lg font-semibold text-gray-800 dark:text-gray-200 mb-4"
          >
            {{ qIndex + 1 }}. {{ qStats.questionText }}
            <span class="text-sm font-normal text-gray-500 dark:text-gray-400"
              >({{ getQuestionTypeLabel(qStats.questionType) }})</span
            >
          </h3>

          <div
            v-if="
              qStats.questionType === 'SINGLE_CHOICE' ||
              qStats.questionType === 'MULTIPLE_CHOICE'
            "
          >
            <ul class="space-y-3">
              <li
                v-for="answer in qStats.answers"
                :key="answer.answer"
                class="text-sm"
              >
                <div class="flex justify-between items-center mb-1">
                  <span class="text-gray-700 dark:text-gray-300">{{
                    answer.answer
                  }}</span>
                  <span class="font-medium text-gray-500 dark:text-gray-400"
                    >{{ answer.count }}명 ({{
                      getPercentage(qStats, answer.count)
                    }}%)</span
                  >
                </div>
                <div
                  class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2.5"
                >
                  <div
                    class="bg-indigo-600 h-2.5 rounded-full transition-all"
                    :style="{
                      width: getPercentage(qStats, answer.count) + '%',
                    }"
                  ></div>
                </div>
              </li>
            </ul>
          </div>
          <div
            v-else
            class="max-h-60 overflow-y-auto p-3 bg-gray-100 dark:bg-gray-900/50 rounded-md"
          >
            <ul class="divide-y divide-gray-200 dark:divide-gray-700">
              <li
                v-for="(answer, aIndex) in qStats.answers"
                :key="aIndex"
                class="py-2 text-sm text-gray-700 dark:text-gray-300"
              >
                "{{ answer.answer }}"
              </li>
              <li
                v-if="qStats.answers.length === 0"
                class="py-2 text-sm text-center text-gray-500 dark:text-gray-400"
              >
                제출된 텍스트 답변이 없습니다.
              </li>
            </ul>
          </div>
        </div>
      </div>

      <!-- 개별 응답 탭 -->
      <div v-if="activeTab === 'responses'">
        <div class="flex justify-between items-center mb-4">
          <p class="text-sm text-gray-500">
            총 {{ responses.length }}건의 응답
          </p>
          <button
            class="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 text-sm font-medium"
            :disabled="isExporting"
            @click="exportToExcel"
          >
            {{ isExporting ? '다운로드 중...' : '엑셀 다운로드' }}
          </button>
        </div>

        <div v-if="responsesLoading" class="text-center py-10">
          <p class="text-gray-500">응답 목록을 불러오는 중...</p>
        </div>
        <div
          v-else-if="responses.length === 0"
          class="text-center py-10 text-gray-400"
        >
          제출된 응답이 없습니다.
        </div>
        <div
          v-else
          class="border dark:border-gray-700 rounded-lg overflow-hidden"
        >
          <table
            class="min-w-full divide-y divide-gray-200 dark:divide-gray-700"
          >
            <thead class="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider"
                >
                  이름
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider"
                >
                  그룹
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider"
                >
                  제출일
                </th>
                <th
                  class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider"
                >
                  상세
                </th>
              </tr>
            </thead>
            <tbody
              class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700"
            >
              <template v-for="resp in responses" :key="resp.responseId">
                <tr class="hover:bg-gray-50 dark:hover:bg-gray-700/50">
                  <td
                    class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-gray-200"
                  >
                    {{ resp.userName }}
                  </td>
                  <td
                    class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400"
                  >
                    {{ resp.groupName || '-' }}
                  </td>
                  <td
                    class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400"
                  >
                    {{ formatDate(resp.submittedAt) }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-right text-sm">
                    <button
                      class="text-indigo-600 hover:text-indigo-900 font-medium"
                      @click="toggleExpand(resp.responseId)"
                    >
                      {{ expandedId === resp.responseId ? '접기' : '보기' }}
                    </button>
                  </td>
                </tr>
                <!-- 확장 영역 -->
                <tr v-if="expandedId === resp.responseId">
                  <td
                    colspan="4"
                    class="px-6 py-4 bg-gray-50 dark:bg-gray-700/30"
                  >
                    <div class="space-y-3">
                      <div
                        v-for="answer in resp.answers"
                        :key="answer.questionId"
                        class="border-l-2 border-indigo-300 pl-4"
                      >
                        <p
                          class="text-sm font-medium text-gray-700 dark:text-gray-300"
                        >
                          {{ answer.questionText }}
                          <span class="text-xs text-gray-400 ml-1"
                            >({{
                              getQuestionTypeLabel(answer.questionType)
                            }})</span
                          >
                        </p>
                        <p
                          class="text-sm text-gray-600 dark:text-gray-400 mt-1"
                        >
                          <template
                            v-if="
                              answer.selectedOptions &&
                              answer.selectedOptions.length > 0
                            "
                          >
                            {{ answer.selectedOptions.join(', ') }}
                          </template>
                          <template v-else-if="answer.answerText">
                            {{ answer.answerText }}
                          </template>
                          <span v-else class="text-gray-400 italic"
                            >미응답</span
                          >
                        </p>
                      </div>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>
      </div>
    </div>
    <div v-else class="text-center py-10">
      <p class="text-gray-500 dark:text-gray-400">
        이 설문에 대한 통계가 없습니다.
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import api from '@/services/api'

const props = defineProps({
  surveyId: {
    type: Number,
    required: true,
  },
})

const emit = defineEmits(['back'])

const stats = ref(null)
const loading = ref(true)
const error = ref(null)
const activeTab = ref('summary')

// 개별 응답
const responses = ref([])
const responsesLoading = ref(false)
const expandedId = ref(null)
const isExporting = ref(false)

async function fetchSurveyStats() {
  loading.value = true
  error.value = null
  try {
    const response = await api.get(`/surveys/${props.surveyId}/stats`)
    stats.value = response.data
  } catch {
    error.value = '설문 통계를 불러오는데 실패했습니다.'
  } finally {
    loading.value = false
  }
}

async function fetchResponses() {
  responsesLoading.value = true
  try {
    const response = await api.get(`/surveys/${props.surveyId}/responses`)
    responses.value = response.data
  } catch {
    error.value = '개별 응답을 불러오는데 실패했습니다.'
  } finally {
    responsesLoading.value = false
  }
}

function switchToResponses() {
  activeTab.value = 'responses'
  fetchResponses()
}

function toggleExpand(responseId) {
  expandedId.value = expandedId.value === responseId ? null : responseId
}

async function exportToExcel() {
  isExporting.value = true
  try {
    const response = await api.get(
      `/surveys/${props.surveyId}/responses/export`,
      { responseType: 'blob' },
    )
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `survey_${props.surveyId}_responses.xlsx`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  } catch {
    error.value = '엑셀 다운로드에 실패했습니다.'
  } finally {
    isExporting.value = false
  }
}

function formatDate(dateStr) {
  return new Date(dateStr).toLocaleString('ko-KR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

onMounted(() => {
  fetchSurveyStats()
})

watch(
  () => props.surveyId,
  () => {
    stats.value = null
    responses.value = []
    expandedId.value = null
    activeTab.value = 'summary'
    fetchSurveyStats()
  },
)

function getPercentage(questionStats, count) {
  if (stats.value.totalResponses === 0) {
    return 0
  }
  if (questionStats.questionType === 'MULTIPLE_CHOICE') {
    const totalVotes = questionStats.answers.reduce(
      (sum, ans) => sum + ans.count,
      0,
    )
    return totalVotes === 0 ? 0 : Math.round((count / totalVotes) * 100)
  }
  return Math.round((count / stats.value.totalResponses) * 100)
}

function getQuestionTypeLabel(type) {
  switch (type) {
    case 'SHORT_TEXT':
      return '단답형'
    case 'LONG_TEXT':
      return '장문형'
    case 'SINGLE_CHOICE':
      return '단일 선택'
    case 'MULTIPLE_CHOICE':
      return '다중 선택'
    default:
      return type
  }
}
</script>
