<template>
  <div class="p-4 sm:p-6 bg-white dark:bg-gray-800 rounded-b-lg">
    <!-- 토스트 메시지 -->
    <div
      v-if="toastMessage"
      class="fixed top-4 right-4 z-50 px-4 py-3 rounded-lg shadow-lg text-white text-sm"
      :class="toastType === 'success' ? 'bg-green-500' : 'bg-red-500'"
    >
      {{ toastMessage }}
    </div>

    <form @submit.prevent="saveSurvey">
      <div class="mb-6">
        <label
          for="title"
          class="block text-sm font-medium text-gray-700 dark:text-gray-300"
          >설문 제목 <span class="text-red-500">*</span></label
        >
        <input
          id="title"
          v-model="survey.title"
          type="text"
          class="mt-1 block w-full rounded-md shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
          :class="
            errors.title
              ? 'border-red-500 dark:border-red-500'
              : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200'
          "
        />
        <p v-if="errors.title" class="mt-1 text-sm text-red-500">
          {{ errors.title }}
        </p>
      </div>

      <div class="mb-6">
        <label
          for="description"
          class="block text-sm font-medium text-gray-700 dark:text-gray-300"
          >설명</label
        >
        <textarea
          id="description"
          v-model="survey.description"
          rows="3"
          class="mt-1 block w-full rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
        ></textarea>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
        <div>
          <label
            for="startDate"
            class="block text-sm font-medium text-gray-700 dark:text-gray-300"
            >시작일 <span class="text-red-500">*</span></label
          >
          <input
            id="startDate"
            v-model="survey.startDate"
            type="datetime-local"
            class="mt-1 block w-full rounded-md shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            :class="
              errors.startDate || errors.dateRange
                ? 'border-red-500 dark:border-red-500'
                : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200'
            "
          />
          <p v-if="errors.startDate" class="mt-1 text-sm text-red-500">
            {{ errors.startDate }}
          </p>
        </div>
        <div>
          <label
            for="endDate"
            class="block text-sm font-medium text-gray-700 dark:text-gray-300"
            >종료일 <span class="text-red-500">*</span></label
          >
          <input
            id="endDate"
            v-model="survey.endDate"
            type="datetime-local"
            class="mt-1 block w-full rounded-md shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            :class="
              errors.endDate || errors.dateRange
                ? 'border-red-500 dark:border-red-500'
                : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200'
            "
          />
          <p v-if="errors.endDate" class="mt-1 text-sm text-red-500">
            {{ errors.endDate }}
          </p>
        </div>
        <p
          v-if="errors.dateRange"
          class="col-span-full text-sm text-red-500 -mt-2"
        >
          {{ errors.dateRange }}
        </p>
      </div>

      <div class="mb-6">
        <label class="flex items-center">
          <input
            v-model="survey.isActive"
            type="checkbox"
            class="rounded text-indigo-600 shadow-sm"
          />
          <span class="ml-2 text-gray-700 dark:text-gray-300">활성 상태</span>
        </label>
      </div>

      <hr class="my-6 border-gray-200 dark:border-gray-700" />

      <div class="flex justify-between items-center mb-4">
        <h3 class="text-xl font-semibold text-gray-800 dark:text-gray-200">
          질문
        </h3>
        <button
          type="button"
          class="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 text-sm font-medium"
          @click="addQuestion"
        >
          질문 추가
        </button>
      </div>

      <p v-if="errors.questions" class="text-sm text-red-500 mb-4">
        {{ errors.questions }}
      </p>

      <div
        v-if="survey.questions.length === 0"
        class="text-center py-8 text-gray-400"
      >
        질문을 추가하여 설문을 구성하세요.
      </div>

      <div
        v-for="(question, qIndex) in survey.questions"
        :key="question.id || qIndex"
        class="p-4 border rounded-lg mb-4 bg-gray-50 dark:bg-gray-800/50"
        :class="
          errors[`question_${qIndex}`]
            ? 'border-red-300 dark:border-red-700'
            : 'dark:border-gray-700'
        "
      >
        <div class="flex justify-between items-center mb-4">
          <h4 class="font-bold text-gray-700 dark:text-gray-300">
            질문 {{ qIndex + 1 }}
          </h4>
          <div class="flex items-center space-x-2">
            <button
              type="button"
              class="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30"
              :disabled="qIndex === 0"
              title="위로 이동"
              @click="moveQuestion(qIndex, -1)"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="h-5 w-5"
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fill-rule="evenodd"
                  d="M14.707 12.707a1 1 0 01-1.414 0L10 9.414l-3.293 3.293a1 1 0 01-1.414-1.414l4-4a1 1 0 011.414 0l4 4a1 1 0 010 1.414z"
                  clip-rule="evenodd"
                />
              </svg>
            </button>
            <button
              type="button"
              class="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30"
              :disabled="qIndex === survey.questions.length - 1"
              title="아래로 이동"
              @click="moveQuestion(qIndex, 1)"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="h-5 w-5"
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fill-rule="evenodd"
                  d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z"
                  clip-rule="evenodd"
                />
              </svg>
            </button>
            <button
              type="button"
              class="text-red-500 hover:text-red-700 font-semibold"
              @click="removeQuestion(qIndex)"
            >
              삭제
            </button>
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label
              class="block text-sm font-medium text-gray-700 dark:text-gray-300"
              >질문 내용 <span class="text-red-500">*</span></label
            >
            <input
              v-model="question.questionText"
              type="text"
              class="mt-1 block w-full rounded-md shadow-sm"
              :class="
                errors[`question_${qIndex}`]
                  ? 'border-red-500 dark:border-red-500'
                  : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200'
              "
            />
            <p
              v-if="errors[`question_${qIndex}`]"
              class="mt-1 text-sm text-red-500"
            >
              {{ errors[`question_${qIndex}`] }}
            </p>
          </div>
          <div>
            <label
              class="block text-sm font-medium text-gray-700 dark:text-gray-300"
              >질문 유형</label
            >
            <select
              v-model="question.type"
              class="mt-1 block w-full rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 shadow-sm"
              @change="onQuestionTypeChange(question)"
            >
              <option
                v-for="(label, value) in QUESTION_TYPE_LABELS"
                :key="value"
                :value="value"
              >
                {{ label }}
              </option>
            </select>
          </div>
        </div>

        <div class="mt-4">
          <label class="flex items-center">
            <input
              v-model="question.isRequired"
              type="checkbox"
              class="rounded text-indigo-600"
            />
            <span class="ml-2 text-sm text-gray-700 dark:text-gray-300"
              >필수 응답</span
            >
          </label>
        </div>

        <div
          v-if="isChoiceType(question.type)"
          class="mt-4 pl-4 border-l-2 border-gray-200 dark:border-gray-600"
        >
          <div class="flex justify-between items-center mb-2">
            <h5 class="font-semibold text-gray-700 dark:text-gray-300">
              선택지
            </h5>
            <button
              type="button"
              class="text-sm text-indigo-600 hover:underline"
              @click="addOption(qIndex)"
            >
              + 선택지 추가
            </button>
          </div>
          <p
            v-if="errors[`question_${qIndex}_options`]"
            class="text-sm text-red-500 mb-2"
          >
            {{ errors[`question_${qIndex}_options`] }}
          </p>
          <div
            v-for="(option, oIndex) in question.options"
            :key="option.id || oIndex"
            class="flex items-center gap-2 mb-2"
          >
            <span class="text-gray-600 dark:text-gray-400 text-sm"
              >({{ oIndex + 1 }})</span
            >
            <input
              v-model="option.optionText"
              type="text"
              class="flex-grow rounded-md shadow-sm text-sm"
              :class="
                errors[`question_${qIndex}_option_${oIndex}`]
                  ? 'border-red-500 dark:border-red-500'
                  : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200'
              "
              placeholder="선택지 텍스트"
            />
            <select
              v-model="option.optionTourId"
              class="w-48 rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 text-sm shadow-sm"
            >
              <option :value="null">옵션투어 연결</option>
              <option v-for="ot in optionTours" :key="ot.id" :value="ot.id">
                {{ ot.name }} ({{ formatTourDate(ot) }})
              </option>
            </select>
            <button
              type="button"
              class="text-red-500 hover:text-red-700 flex-shrink-0"
              @click="removeOption(qIndex, oIndex)"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="h-5 w-5"
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fill-rule="evenodd"
                  d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                  clip-rule="evenodd"
                />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <div class="mt-8 flex justify-end space-x-4">
        <button
          type="button"
          class="px-6 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300 font-semibold"
          @click="emit('cancel')"
        >
          취소
        </button>
        <button
          type="submit"
          :disabled="isSaving"
          class="px-6 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 font-semibold disabled:bg-green-400 disabled:cursor-not-allowed"
        >
          {{ isSaving ? '저장 중...' : '저장' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch, onUnmounted, reactive } from 'vue'
import api from '@/services/api'
import { formatDateTimeForInput, formatDate } from '@/utils/date'
import { useToast } from '@/composables/useToast'
import { QUESTION_TYPE_LABELS, isChoiceType } from '@/constants/survey'

const props = defineProps({
  surveyId: {
    type: Number,
    default: null,
  },
  conventionId: {
    type: Number,
    required: true,
  },
})

const emit = defineEmits(['cancel', 'saved'])

const survey = ref({
  id: props.surveyId,
  title: '',
  description: '',
  isActive: true,
  startDate: null,
  endDate: null,
  conventionId: props.conventionId,
  surveyType: 'OPTION_TOUR',
  questions: [],
})

const optionTours = ref([])
const isEditing = computed(() => !!props.surveyId)
const isSaving = ref(false)
const isDirty = ref(false)
const errors = reactive({})

const { toastMessage, toastType, showToast } = useToast()

function onBeforeUnload(e) {
  if (isDirty.value) {
    e.preventDefault()
    e.returnValue = ''
  }
}

onMounted(() => {
  window.addEventListener('beforeunload', onBeforeUnload)
  fetchOptionTours()
  fetchSurveyData()
})

onUnmounted(() => {
  window.removeEventListener('beforeunload', onBeforeUnload)
})

watch(
  survey,
  () => {
    isDirty.value = true
  },
  { deep: true },
)

watch(() => props.surveyId, fetchSurveyData)

async function fetchOptionTours() {
  try {
    const response = await api.get(
      `/admin/conventions/${props.conventionId}/option-tours`,
    )
    optionTours.value = response.data || []
  } catch {
    optionTours.value = []
  }
}

function formatTourDate(ot) {
  const date = ot.date ? formatDate(ot.date) : ''
  return `${date} ${ot.startTime || ''}`
}

async function fetchSurveyData() {
  if (!isEditing.value) {
    survey.value.questions = []
    isDirty.value = false
    return
  }
  try {
    const response = await api.get(`/surveys/${props.surveyId}`)
    survey.value = response.data
    survey.value.startDate = formatDateTimeForInput(response.data.startDate)
    survey.value.endDate = formatDateTimeForInput(response.data.endDate)
    isDirty.value = false
  } catch {
    showToast('설문 정보를 불러오는데 실패했습니다.', 'error')
  }
}

function addQuestion() {
  survey.value.questions.push({
    id: 0,
    questionText: '',
    type: 'SINGLE_CHOICE',
    isRequired: false,
    orderIndex: survey.value.questions.length,
    options: [{ id: 0, optionText: '', orderIndex: 0, optionTourId: null }],
  })
}

function removeQuestion(index) {
  survey.value.questions.splice(index, 1)
}

function moveQuestion(index, direction) {
  const newIndex = index + direction
  if (newIndex < 0 || newIndex >= survey.value.questions.length) return
  const questions = survey.value.questions
  const temp = questions[index]
  questions[index] = questions[newIndex]
  questions[newIndex] = temp
  survey.value.questions = [...questions]
}

function addOption(questionIndex) {
  survey.value.questions[questionIndex].options.push({
    id: 0,
    optionText: '',
    orderIndex: survey.value.questions[questionIndex].options.length,
    optionTourId: null,
  })
}

function removeOption(questionIndex, optionIndex) {
  survey.value.questions[questionIndex].options.splice(optionIndex, 1)
}

function onQuestionTypeChange(question) {
  if (!isChoiceType(question.type)) {
    question.options = []
  }
}

function clearErrors() {
  Object.keys(errors).forEach((key) => delete errors[key])
}

function validateForm() {
  clearErrors()
  let valid = true

  if (!survey.value.title?.trim()) {
    errors.title = '설문 제목을 입력해주세요.'
    valid = false
  }

  if (!survey.value.startDate) {
    errors.startDate = '시작일을 입력해주세요.'
    valid = false
  }

  if (!survey.value.endDate) {
    errors.endDate = '종료일을 입력해주세요.'
    valid = false
  }

  if (
    survey.value.startDate &&
    survey.value.endDate &&
    new Date(survey.value.startDate) > new Date(survey.value.endDate)
  ) {
    errors.dateRange = '시작일이 종료일보다 늦을 수 없습니다.'
    valid = false
  }

  if (survey.value.questions.length === 0) {
    errors.questions = '질문을 1개 이상 추가해주세요.'
    valid = false
  }

  survey.value.questions.forEach((q, qIndex) => {
    if (!q.questionText?.trim()) {
      errors[`question_${qIndex}`] = '질문 내용을 입력해주세요.'
      valid = false
    }

    if (isChoiceType(q.type)) {
      if (!q.options || q.options.length === 0) {
        errors[`question_${qIndex}_options`] = '선택지를 1개 이상 추가해주세요.'
        valid = false
      } else {
        q.options.forEach((o, oIndex) => {
          if (!o.optionText?.trim()) {
            errors[`question_${qIndex}_option_${oIndex}`] = true
            valid = false
          }
        })
      }
    }
  })

  return valid
}

async function saveSurvey() {
  if (!validateForm()) {
    showToast('입력값을 확인해주세요.', 'error')
    return
  }

  isSaving.value = true

  survey.value.questions.forEach((q, qIndex) => {
    q.orderIndex = qIndex
    if (q.options) {
      q.options.forEach((o, oIndex) => {
        o.orderIndex = oIndex
      })
    }
  })

  const payload = {
    ...survey.value,
    surveyType: 'OPTION_TOUR',
    startDate: survey.value.startDate || null,
    endDate: survey.value.endDate || null,
  }

  try {
    if (isEditing.value) {
      await api.put(`/surveys/${survey.value.id}`, payload)
      showToast('설문이 수정되었습니다.')
    } else {
      await api.post('/surveys', payload)
      showToast('설문이 생성되었습니다.')
    }
    isDirty.value = false
    emit('saved')
  } catch (err) {
    const msg = err.response?.data?.message || '설문 저장에 실패했습니다.'
    showToast(msg, 'error')
  } finally {
    isSaving.value = false
  }
}
</script>
