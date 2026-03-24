<template>
  <div class="container mx-auto p-4 sm:p-6">
    <div v-if="loading" class="text-center py-10">
      <p class="text-gray-500 dark:text-gray-400">설문을 불러오는 중...</p>
    </div>
    <div v-else-if="dateBlocked" class="max-w-2xl mx-auto">
      <div
        class="bg-white dark:bg-gray-800 p-6 sm:p-8 rounded-lg shadow-md text-center"
      >
        <div
          class="w-16 h-16 mx-auto mb-4 rounded-full bg-yellow-100 dark:bg-yellow-900/30 flex items-center justify-center"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            class="h-8 w-8 text-yellow-500"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4.5c-.77-.833-2.694-.833-3.464 0L3.34 16.5c-.77.833.192 2.5 1.732 2.5z"
            />
          </svg>
        </div>
        <h2 class="text-xl font-bold text-gray-800 dark:text-gray-200 mb-2">
          {{ dateBlockedMessage }}
        </h2>
        <p
          v-if="dateBlockedDetail"
          class="text-sm text-gray-500 dark:text-gray-400 mb-6"
        >
          {{ dateBlockedDetail }}
        </p>
        <button
          class="px-6 py-2 bg-gray-300 text-gray-800 rounded-md hover:bg-gray-400 font-semibold"
          @click="router.back()"
        >
          돌아가기
        </button>
      </div>
    </div>
    <div v-else-if="error" class="text-center py-10 text-red-500">
      <p>{{ error }}</p>
    </div>
    <div v-else-if="submitted" class="max-w-2xl mx-auto">
      <div
        class="bg-white dark:bg-gray-800 p-6 sm:p-8 rounded-lg shadow-md text-center"
      >
        <div
          class="w-16 h-16 mx-auto mb-4 rounded-full bg-green-100 dark:bg-green-900/30 flex items-center justify-center"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            class="h-8 w-8 text-green-500"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M5 13l4 4L19 7"
            />
          </svg>
        </div>
        <h2 class="text-xl font-bold text-gray-800 dark:text-gray-200 mb-2">
          설문이 제출되었습니다
        </h2>
        <p class="text-gray-500 dark:text-gray-400 mb-6">
          소중한 의견 감사합니다.
        </p>
        <button
          class="px-6 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 font-semibold"
          @click="goBack"
        >
          돌아가기
        </button>
      </div>
    </div>
    <div
      v-else-if="survey"
      class="max-w-2xl mx-auto bg-white dark:bg-gray-800 p-6 sm:p-8 rounded-lg shadow-md"
    >
      <h1
        class="text-2xl sm:text-3xl font-bold text-gray-800 dark:text-gray-200 mb-2"
      >
        {{ survey.title }}
      </h1>
      <p class="text-gray-600 dark:text-gray-400 mb-4">
        {{ survey.description }}
      </p>

      <!-- 기간 표시 -->
      <div
        v-if="survey.startDate || survey.endDate"
        class="text-sm text-gray-500 dark:text-gray-400 mb-4"
      >
        응답 기간:
        <span v-if="survey.startDate">{{ formatDate(survey.startDate) }}</span>
        <span v-if="survey.startDate && survey.endDate"> ~ </span>
        <span v-if="survey.endDate">{{ formatDate(survey.endDate) }}</span>
      </div>

      <!-- 진행률 -->
      <div class="text-sm text-gray-500 dark:text-gray-400 mb-6">
        질문 {{ answeredCount }} / {{ survey.questions.length }}
      </div>

      <!-- 제출 에러 메시지 -->
      <div
        v-if="submitError"
        class="mb-6 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md text-sm text-red-600 dark:text-red-400"
      >
        {{ submitError }}
      </div>

      <form @submit.prevent="submitSurvey">
        <div
          v-for="(question, index) in survey.questions"
          :key="question.id"
          class="mb-8"
        >
          <label class="block font-medium text-gray-700 dark:text-gray-300">
            {{ index + 1 }}. {{ question.questionText }}
            <span v-if="question.isRequired" class="text-red-500 ml-1">*</span>
          </label>

          <div v-if="question.type === 'SHORT_TEXT'" class="mt-3">
            <input
              v-model="responses[question.id]"
              type="text"
              class="w-full p-2 border rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 focus:ring-indigo-500 focus:border-indigo-500"
              :class="
                validationErrors[question.id]
                  ? 'border-red-500'
                  : 'border-gray-300 dark:border-gray-600'
              "
            />
          </div>

          <div v-if="question.type === 'LONG_TEXT'" class="mt-3">
            <textarea
              v-model="responses[question.id]"
              rows="4"
              class="w-full p-2 border rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 focus:ring-indigo-500 focus:border-indigo-500"
              :class="
                validationErrors[question.id]
                  ? 'border-red-500'
                  : 'border-gray-300 dark:border-gray-600'
              "
            ></textarea>
          </div>

          <div v-if="question.type === 'SINGLE_CHOICE'" class="mt-3 space-y-3">
            <div
              v-for="option in question.options"
              :key="option.id"
              class="flex items-center p-3 rounded-md border hover:bg-gray-50 dark:hover:bg-gray-700/50"
              :class="
                validationErrors[question.id]
                  ? 'border-red-300 dark:border-red-700'
                  : 'border-gray-200 dark:border-gray-700'
              "
            >
              <input
                :id="'option-' + option.id"
                v-model="responses[question.id]"
                type="radio"
                :name="'question-' + question.id"
                :value="option.id"
                class="h-4 w-4 text-indigo-600 border-gray-300 focus:ring-indigo-500"
              />
              <label
                :for="'option-' + option.id"
                class="ml-3 block text-sm font-medium text-gray-700 dark:text-gray-300"
                >{{ option.optionText }}</label
              >
            </div>
          </div>

          <div
            v-if="question.type === 'MULTIPLE_CHOICE'"
            class="mt-3 space-y-3"
          >
            <div
              v-for="option in question.options"
              :key="option.id"
              class="flex items-center p-3 rounded-md border hover:bg-gray-50 dark:hover:bg-gray-700/50"
              :class="
                validationErrors[question.id]
                  ? 'border-red-300 dark:border-red-700'
                  : 'border-gray-200 dark:border-gray-700'
              "
            >
              <input
                :id="'option-' + option.id"
                v-model="responses[question.id]"
                type="checkbox"
                :value="option.id"
                class="h-4 w-4 text-indigo-600 border-gray-300 rounded"
              />
              <label
                :for="'option-' + option.id"
                class="ml-3 block text-sm font-medium text-gray-700 dark:text-gray-300"
                >{{ option.optionText }}</label
              >
            </div>
          </div>

          <!-- 인라인 에러 메시지 -->
          <p
            v-if="validationErrors[question.id]"
            class="mt-1 text-sm text-red-500"
          >
            이 질문은 필수 응답입니다.
          </p>
        </div>

        <div class="mt-10 flex justify-between space-x-4">
          <button
            type="button"
            class="w-1/2 bg-gray-300 text-gray-800 py-3 px-4 rounded-md hover:bg-gray-400 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 font-semibold"
            @click="router.back()"
          >
            뒤로가기
          </button>
          <button
            type="submit"
            :disabled="isSubmitting"
            class="w-1/2 bg-indigo-600 text-white py-3 px-4 rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:bg-indigo-400 disabled:cursor-not-allowed font-semibold"
          >
            {{ isSubmitting ? '제출 중...' : '제출' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, reactive } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import api from '@/services/api'

const props = defineProps({
  id: String,
})

const router = useRouter()
const route = useRoute()
const survey = ref(null)
const loading = ref(true)
const error = ref(null)
const responses = reactive({})
const isSubmitting = ref(false)
const submitted = ref(false)
const submitError = ref('')
const validationErrors = reactive({})
const dateBlocked = ref(false)
const dateBlockedMessage = ref('')
const dateBlockedDetail = ref('')

const surveyId = computed(() => props.id || null)

const answeredCount = computed(() => {
  if (!survey.value) return 0
  return survey.value.questions.filter((q) => {
    const answer = responses[q.id]
    if (Array.isArray(answer)) return answer.length > 0
    return answer !== null && answer !== '' && answer !== undefined
  }).length
})

function formatDate(dateStr) {
  return new Date(dateStr).toLocaleString('ko-KR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

async function loadSurvey() {
  if (!surveyId.value) {
    error.value = '설문 ID를 찾을 수 없습니다.'
    loading.value = false
    return
  }

  loading.value = true
  error.value = null
  dateBlocked.value = false

  try {
    const response = await api.get(`/surveys/${surveyId.value}`)
    survey.value = response.data

    const userResponse = await api
      .get(`/surveys/${surveyId.value}/responses/me`)
      .catch(() => null)

    survey.value.questions.forEach((q) => {
      const previousAnswerDetail = userResponse?.data?.answers?.find(
        (a) => a.questionId === q.id,
      )

      if (q.type === 'MULTIPLE_CHOICE') {
        const allSelectedOptionIds =
          userResponse?.data?.answers
            .filter((a) => a.questionId === q.id && a.selectedOptionId !== null)
            .map((a) => a.selectedOptionId) || []
        responses[q.id] = allSelectedOptionIds
      } else if (q.type === 'SINGLE_CHOICE') {
        responses[q.id] = previousAnswerDetail?.selectedOptionId || null
      } else {
        responses[q.id] = previousAnswerDetail?.answerText || null
      }
    })
  } catch (err) {
    if (err.response?.status === 403) {
      dateBlocked.value = true
      dateBlockedMessage.value =
        err.response?.data?.message || '설문에 접근할 수 없습니다.'
      if (err.response?.data?.startDate) {
        dateBlockedDetail.value = `시작일: ${formatDate(err.response.data.startDate)}`
      } else if (err.response?.data?.endDate) {
        dateBlockedDetail.value = `종료일: ${formatDate(err.response.data.endDate)}`
      }
    } else {
      error.value = '설문을 불러오는데 실패했습니다.'
    }
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadSurvey()
})

watch(
  () => props.id,
  (newId, oldId) => {
    if (newId && newId !== oldId) {
      submitted.value = false
      loadSurvey()
    }
  },
)

function goBack() {
  if (window.history.length > 1) {
    router.go(-1)
  } else {
    router.push(`/conventions/${route.params.conventionId}`)
  }
}

async function submitSurvey() {
  isSubmitting.value = true
  submitError.value = ''

  // 유효성 검사 초기화
  Object.keys(validationErrors).forEach((key) => {
    validationErrors[key] = false
  })

  let hasError = false
  for (const question of survey.value.questions) {
    if (question.isRequired) {
      const answer = responses[question.id]
      if (
        answer === null ||
        answer === '' ||
        answer === undefined ||
        (Array.isArray(answer) && answer.length === 0)
      ) {
        validationErrors[question.id] = true
        hasError = true
      }
    }
  }

  if (hasError) {
    isSubmitting.value = false
    submitError.value = '필수 질문에 응답해주세요.'
    return
  }

  const submissionData = {
    SurveyId: survey.value.id,
    Answers: Object.keys(responses).map((questionId) => {
      const answer = responses[questionId]
      const question = survey.value.questions.find((q) => q.id == questionId)

      if (question.type === 'MULTIPLE_CHOICE') {
        return {
          QuestionId: parseInt(questionId),
          SelectedOptionIds: answer,
        }
      } else if (question.type === 'SINGLE_CHOICE') {
        return {
          QuestionId: parseInt(questionId),
          SelectedOptionIds: answer ? [answer] : [],
        }
      } else {
        return {
          QuestionId: parseInt(questionId),
          AnswerText: answer,
        }
      }
    }),
  }

  try {
    await api.post(`/surveys/${survey.value.id}/submit`, submissionData)
    submitted.value = true
  } catch (err) {
    submitError.value =
      err.response?.data?.message ||
      '설문 제출 중 오류가 발생했습니다. 다시 시도해주세요.'
  } finally {
    isSubmitting.value = false
  }
}
</script>
