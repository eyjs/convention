<template>
  <div class="p-4 sm:p-6 bg-white dark:bg-gray-800 rounded-lg shadow-md">
    <div class="flex justify-between items-center mb-6">
      <h1
        class="text-2xl sm:text-3xl font-bold text-gray-800 dark:text-gray-200"
      >
        설문 통계
      </h1>
      <button
        @click="emit('back')"
        class="px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300 font-semibold"
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
      <h2 class="text-xl font-bold text-gray-800 dark:text-gray-200 mb-2">
        {{ stats.surveyTitle }}
      </h2>
      <p class="text-gray-600 dark:text-gray-400 mb-6">
        총 응답 수:
        <span class="font-bold text-gray-800 dark:text-gray-200">{{
          stats.totalResponses
        }}</span>
      </p>

      <div
        v-for="(qStats, qIndex) in stats.questionStats"
        :key="qStats.questionId"
        class="bg-gray-50 dark:bg-gray-800/50 p-6 rounded-lg shadow-sm mb-6"
      >
        <h3 class="text-lg font-semibold text-gray-800 dark:text-gray-200 mb-4">
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
                  >{{ answer.count }} ({{
                    getPercentage(qStats, answer.count)
                  }}%)</span
                >
              </div>
              <div
                class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2.5"
              >
                <div
                  class="bg-indigo-600 h-2.5 rounded-full"
                  :style="{ width: getPercentage(qStats, answer.count) + '%' }"
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
    <div v-else class="text-center py-10">
      <p class="text-gray-500 dark:text-gray-400">
        이 설문에 대한 통계가 없습니다.
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import api from '@/api'

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

async function fetchSurveyStats() {
  loading.value = true
  error.value = null
  try {
    const response = await api.get(`/surveys/${props.surveyId}/stats`)
    stats.value = response.data
  } catch (err) {
    error.value = '설문 통계를 불러오는데 실패했습니다.'
    console.error(err)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchSurveyStats()
})

watch(() => props.surveyId, fetchSurveyStats)

function getPercentage(questionStats, count) {
  if (stats.value.totalResponses === 0) {
    return 0
  }
  // For multiple choice, the total number of votes can be > totalResponses
  // So, calculate percentage based on total votes for that question, not total responses for the survey
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
