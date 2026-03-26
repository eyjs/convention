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
        <div class="flex flex-col gap-3">
          <button
            v-if="hasRemainingSurveys"
            class="px-6 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 font-semibold"
            @click="
              router.push(`/conventions/${route.params.conventionId}/surveys`)
            "
          >
            다른 설문 확인하기
          </button>
          <button
            class="px-6 py-2 font-semibold rounded-md"
            :class="
              hasRemainingSurveys
                ? 'bg-gray-300 text-gray-800 hover:bg-gray-400'
                : 'bg-indigo-600 text-white hover:bg-indigo-700'
            "
            @click="goBack"
          >
            돌아가기
          </button>
        </div>
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
        <span v-if="survey.startDate">{{
          formatDateTime(survey.startDate)
        }}</span>
        <span v-if="survey.startDate && survey.endDate"> ~ </span>
        <span v-if="survey.endDate">{{ formatDateTime(survey.endDate) }}</span>
      </div>

      <!-- 진행률 -->
      <div class="text-sm text-gray-500 dark:text-gray-400 mb-6">
        질문 {{ answeredCount }} / {{ visibleQuestions.length }}
      </div>

      <!-- 제출 에러 메시지 -->
      <div
        v-if="submitError"
        role="alert"
        class="mb-6 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md text-sm text-red-600 dark:text-red-400"
      >
        {{ submitError }}
      </div>

      <form @submit.prevent="submitSurvey">
        <template
          v-for="(question, index) in visibleQuestions"
          :key="question.id"
        >
          <div
            :ref="
              (el) => {
                if (el) questionRefs[question.id] = el
              }
            "
            class="mb-8"
            :class="
              question.parentOptionId
                ? 'ml-6 pl-4 border-l-2 border-indigo-200 dark:border-indigo-800'
                : ''
            "
          >
            <label
              class="block font-medium text-gray-700 dark:text-gray-300"
              :aria-required="question.isRequired ? 'true' : undefined"
            >
              <span
                v-if="question.parentOptionId"
                class="text-indigo-500 text-xs font-normal block mb-1"
              >
                꼬리질문
              </span>
              {{ getVisibleNumber(question, index) }}.
              {{ question.questionText }}
              <span v-if="question.isRequired" class="text-red-500 ml-1"
                >*</span
              >
            </label>

            <div v-if="question.type === 'SHORT_TEXT'" class="mt-3">
              <input
                v-model="responses[question.id]"
                type="text"
                :aria-invalid="
                  validationErrors[question.id] ? 'true' : undefined
                "
                class="w-full p-2 border rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
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
                :aria-invalid="
                  validationErrors[question.id] ? 'true' : undefined
                "
                class="w-full p-2 border rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                :class="
                  validationErrors[question.id]
                    ? 'border-red-500'
                    : 'border-gray-300 dark:border-gray-600'
                "
              ></textarea>
            </div>

            <div
              v-if="question.type === 'SINGLE_CHOICE'"
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
                  type="radio"
                  :name="'question-' + question.id"
                  :value="option.id"
                  class="h-4 w-4 text-indigo-600 border-gray-300 focus:ring-2 focus:ring-indigo-500"
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
                  class="h-4 w-4 text-indigo-600 border-gray-300 rounded focus:ring-2 focus:ring-indigo-500"
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
              role="alert"
              :aria-invalid="true"
              class="mt-1 text-sm text-red-500"
            >
              이 질문은 필수 응답입니다.
            </p>
          </div>
        </template>

        <!-- 설문 종료 안내 -->
        <div
          v-if="terminatingAfterIndex >= 0"
          class="mb-6 p-4 bg-gray-50 dark:bg-gray-700/50 border border-gray-200 dark:border-gray-600 rounded-lg text-center"
        >
          <p class="text-sm text-gray-500 dark:text-gray-400">
            선택하신 응답에 따라 설문이 종료됩니다. 아래 제출 버튼을 눌러주세요.
          </p>
        </div>

        <div class="mt-10 flex flex-col sm:flex-row justify-between gap-4">
          <button
            type="button"
            class="w-full sm:w-1/2 bg-gray-300 text-gray-800 py-3 px-4 rounded-md hover:bg-gray-400 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 font-semibold"
            @click="router.back()"
          >
            뒤로가기
          </button>
          <button
            type="submit"
            :disabled="isSubmitting"
            class="w-full sm:w-1/2 bg-indigo-600 text-white py-3 px-4 rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:bg-indigo-400 disabled:cursor-not-allowed font-semibold"
          >
            {{ isSubmitting ? '제출 중...' : '제출' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, reactive } from 'vue'
import { useRouter, useRoute, onBeforeRouteLeave } from 'vue-router'
import api from '@/services/api'
import { formatDateTime } from '@/utils/date'

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
const isDirty = ref(false)
const questionRefs = reactive({})
const hasRemainingSurveys = ref(false)

const surveyId = computed(() => props.id || null)

// 선택된 옵션 ID 집합
const selectedOptionIds = computed(() => {
  const ids = new Set()
  if (!survey.value) return ids

  for (const q of survey.value.questions) {
    const answer = responses[q.id]
    if (Array.isArray(answer)) {
      answer.forEach((id) => ids.add(id))
    } else if (answer != null && answer !== '') {
      ids.add(answer)
    }
  }
  return ids
})

// 종료 옵션이 선택된 질문의 인덱스 (이후 질문 숨김)
const terminatingAfterIndex = computed(() => {
  if (!survey.value) return -1
  const questions = survey.value.questions
  for (let i = 0; i < questions.length; i++) {
    const q = questions[i]
    if (q.parentOptionId) continue // 꼬리질문은 건너뜀
    const answer = responses[q.id]
    if (answer == null || answer === '') continue
    const selectedIds = Array.isArray(answer) ? answer : [answer]
    const hasTerminating = (q.options || []).some(
      (o) => o.isTerminating && selectedIds.includes(o.id),
    )
    if (hasTerminating) return i
  }
  return -1
})

// 보이는 질문: 최상위 또는 부모 옵션이 선택된 꼬리질문 + 종료 옵션 이후 숨김
const visibleQuestions = computed(() => {
  if (!survey.value) return []
  let topLevelIdx = -1
  return survey.value.questions.filter((q) => {
    if (!q.parentOptionId) topLevelIdx++
    // 종료 옵션이 선택된 질문 이후의 최상위 질문은 숨김
    if (
      terminatingAfterIndex.value >= 0 &&
      !q.parentOptionId &&
      topLevelIdx > terminatingAfterIndex.value
    ) {
      return false
    }
    if (!q.parentOptionId) return true
    return selectedOptionIds.value.has(q.parentOptionId)
  })
})

function getVisibleNumber(question, index) {
  if (question.parentOptionId) {
    // 꼬리질문은 부모 번호 기반 서브넘버링 (단순히 index+1 사용)
    return index + 1
  }
  // 최상위 질문은 최상위 기준 번호
  let topNum = 0
  for (const q of visibleQuestions.value) {
    if (!q.parentOptionId) topNum++
    if (q === question) return topNum
  }
  return index + 1
}

const answeredCount = computed(() => {
  return visibleQuestions.value.filter((q) => {
    const answer = responses[q.id]
    if (Array.isArray(answer)) return answer.length > 0
    return answer !== null && answer !== '' && answer !== undefined
  }).length
})

// 미저장 경고
function onBeforeUnload(e) {
  if (isDirty.value && !submitted.value) {
    e.preventDefault()
    e.returnValue = ''
  }
}

onMounted(() => {
  window.addEventListener('beforeunload', onBeforeUnload)
  loadSurvey()
})

onUnmounted(() => {
  window.removeEventListener('beforeunload', onBeforeUnload)
})

onBeforeRouteLeave(() => {
  if (isDirty.value && !submitted.value) {
    return window.confirm('작성 중인 내용이 있습니다. 페이지를 떠나시겠습니까?')
  }
})

// 변경 감지
watch(
  responses,
  () => {
    isDirty.value = true
  },
  { deep: true },
)

watch(
  () => props.id,
  (newId, oldId) => {
    if (newId && newId !== oldId) {
      submitted.value = false
      loadSurvey()
    }
  },
)

function prefillResponses(questions, userResponse) {
  if (!userResponse?.data?.answers) return

  const previousAnswers = userResponse.data.answers
  questions.forEach((q) => {
    if (q.type === 'MULTIPLE_CHOICE') {
      responses[q.id] =
        previousAnswers
          ?.filter((a) => a.questionId === q.id && a.selectedOptionId != null)
          .map((a) => a.selectedOptionId) || []
    } else if (q.type === 'SINGLE_CHOICE') {
      const detail = previousAnswers?.find((a) => a.questionId === q.id)
      responses[q.id] = detail?.selectedOptionId || null
    } else {
      const detail = previousAnswers?.find((a) => a.questionId === q.id)
      responses[q.id] = detail?.answerText || null
    }
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

    prefillResponses(survey.value.questions, userResponse)
    isDirty.value = false
  } catch (err) {
    if (err.response?.status === 403) {
      dateBlocked.value = true
      dateBlockedMessage.value =
        err.response?.data?.message || '설문에 접근할 수 없습니다.'
      if (err.response?.data?.startDate) {
        dateBlockedDetail.value = `시작일: ${formatDateTime(err.response.data.startDate)}`
      } else if (err.response?.data?.endDate) {
        dateBlockedDetail.value = `종료일: ${formatDateTime(err.response.data.endDate)}`
      }
    } else {
      error.value = '설문을 불러오는데 실패했습니다.'
    }
  } finally {
    loading.value = false
  }
}

function goBack() {
  if (window.history.length > 1) {
    router.go(-1)
  } else {
    router.push(`/conventions/${route.params.conventionId}`)
  }
}

function scrollToFirstError() {
  const firstErrorId = Object.keys(validationErrors).find(
    (key) => validationErrors[key],
  )
  if (firstErrorId && questionRefs[firstErrorId]) {
    questionRefs[firstErrorId].scrollIntoView({
      behavior: 'smooth',
      block: 'center',
    })
  }
}

async function submitSurvey() {
  isSubmitting.value = true
  submitError.value = ''

  Object.keys(validationErrors).forEach((key) => {
    validationErrors[key] = false
  })

  // 보이는 질문만 필수 검증
  let hasError = false
  const visibleIds = new Set(visibleQuestions.value.map((q) => q.id))

  for (const question of survey.value.questions) {
    if (!visibleIds.has(question.id)) continue
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
    scrollToFirstError()
    return
  }

  // 보이는 질문의 응답만 전송
  const submissionData = {
    SurveyId: survey.value.id,
    Answers: visibleQuestions.value.map((question) => {
      const answer = responses[question.id]

      if (question.type === 'MULTIPLE_CHOICE') {
        return {
          QuestionId: question.id,
          SelectedOptionIds: answer || [],
        }
      } else if (question.type === 'SINGLE_CHOICE') {
        return {
          QuestionId: question.id,
          SelectedOptionIds: answer ? [answer] : [],
        }
      } else {
        return {
          QuestionId: question.id,
          AnswerText: answer,
        }
      }
    }),
  }

  try {
    await api.post(`/surveys/${survey.value.id}/submit`, submissionData)
    isDirty.value = false
    submitted.value = true

    // 남은 미완료 설문 확인
    try {
      const convId = route.params.conventionId
      const listRes = await api.get(`/surveys/convention/${convId}`)
      const pending = (listRes.data || []).filter((s) => !s.isCompleted)
      hasRemainingSurveys.value = pending.length > 0
    } catch {
      hasRemainingSurveys.value = false
    }
  } catch (err) {
    submitError.value =
      err.response?.data?.message ||
      '설문 제출 중 오류가 발생했습니다. 다시 시도해주세요.'
  } finally {
    isSubmitting.value = false
  }
}
</script>
