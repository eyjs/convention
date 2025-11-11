<template>
  <div class="p-4 sm:p-6 bg-white dark:bg-gray-800 rounded-b-lg">
    <form @submit.prevent="saveSurvey">
      <div class="mb-6">
        <label
          for="title"
          class="block text-sm font-medium text-gray-700 dark:text-gray-300"
          >설문 제목</label
        >
        <input
          type="text"
          v-model="survey.title"
          id="title"
          class="mt-1 block w-full rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
        />
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

      <div class="mb-6">
        <label class="flex items-center">
          <input
            type="checkbox"
            v-model="survey.isActive"
            class="rounded text-indigo-600 shadow-sm"
          />
          <span class="ml-2 text-gray-700 dark:text-gray-300">활성 상태</span>
        </label>
      </div>

      <div v-if="isEditing">
        <hr class="my-6 border-gray-200 dark:border-gray-700" />

        <div class="flex justify-between items-center mb-4">
          <h3 class="text-xl font-semibold text-gray-800 dark:text-gray-200">
            질문
          </h3>
          <button
            type="button"
            @click="addQuestion"
            class="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 text-sm font-medium"
          >
            질문 추가
          </button>
        </div>

        <div
          v-for="(question, qIndex) in survey.questions"
          :key="question.id || qIndex"
          class="p-4 border dark:border-gray-700 rounded-lg mb-4 bg-gray-50 dark:bg-gray-800/50"
        >
          <div class="flex justify-between items-center mb-4">
            <h4 class="font-bold text-gray-700 dark:text-gray-300">
              질문 {{ qIndex + 1 }}
            </h4>
            <button
              type="button"
              @click="removeQuestion(qIndex)"
              class="text-red-500 hover:text-red-700 font-semibold"
            >
              삭제
            </button>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label
                class="block text-sm font-medium text-gray-700 dark:text-gray-300"
                >질문 내용</label
              >
              <input
                type="text"
                v-model="question.questionText"
                class="mt-1 block w-full rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 shadow-sm"
              />
            </div>
            <div>
              <label
                class="block text-sm font-medium text-gray-700 dark:text-gray-300"
                >질문 유형</label
              >
              <select
                v-model="question.type"
                @change="onQuestionTypeChange(question)"
                class="mt-1 block w-full rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 shadow-sm"
              >
                <option value="SHORT_TEXT">단답형</option>
                <option value="LONG_TEXT">장문형</option>
                <option value="SINGLE_CHOICE">단일 선택</option>
                <option value="MULTIPLE_CHOICE">다중 선택</option>
              </select>
            </div>
          </div>

          <div class="mt-4">
            <label class="flex items-center">
              <input
                type="checkbox"
                v-model="question.isRequired"
                class="rounded text-indigo-600"
              />
              <span class="ml-2 text-sm text-gray-700 dark:text-gray-300"
                >필수 응답</span
              >
            </label>
          </div>

          <div
            v-if="
              question.type === 'SINGLE_CHOICE' ||
              question.type === 'MULTIPLE_CHOICE'
            "
            class="mt-4 pl-4 border-l-2 border-gray-200 dark:border-gray-600"
          >
            <div class="flex justify-between items-center mb-2">
              <h5 class="font-semibold text-gray-700 dark:text-gray-300">
                선택지
              </h5>
              <button
                type="button"
                @click="addOption(qIndex)"
                class="text-sm text-indigo-600 hover:underline"
              >
                + 선택지 추가
              </button>
            </div>
            <div
              v-for="(option, oIndex) in question.options"
              :key="option.id || oIndex"
              class="flex items-center mb-2"
            >
              <span class="mr-2 text-gray-600 dark:text-gray-400"
                >({{ oIndex + 1 }})</span
              >
              <input
                type="text"
                v-model="option.optionText"
                class="flex-grow rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 shadow-sm text-sm"
              />
              <button
                type="button"
                @click="removeOption(qIndex, oIndex)"
                class="ml-2 text-red-500 hover:text-red-700"
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
      </div>

      <div class="mt-8 flex justify-end space-x-4">
        <button
          type="button"
          @click="emit('cancel')"
          class="px-6 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300 font-semibold"
        >
          취소
        </button>
        <button
          type="submit"
          class="px-6 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 font-semibold"
        >
          저장
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import api from '@/api'

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
  id: props.surveyId, // Add id for update operations
  title: '',
  description: '',
  isActive: true,
  conventionId: props.conventionId,
  questions: [],
})

const isEditing = computed(() => !!props.surveyId)

async function fetchSurveyData() {
  if (!isEditing.value) {
    survey.value.questions = [] // Ensure no questions are pre-filled for new surveys
    return
  }
  try {
    const response = await api.get(`/surveys/${props.surveyId}`)
    survey.value = response.data
  } catch (error) {
    console.error('설문 정보를 불러오는데 실패했습니다:', error)
    alert('설문 정보를 불러오는데 실패했습니다.')
  }
}

onMounted(() => {
  fetchSurveyData()
})

watch(() => props.surveyId, fetchSurveyData)

function addQuestion() {
  survey.value.questions.push({
    id: 0, // New questions have ID 0
    questionText: '',
    type: 'SHORT_TEXT',
    isRequired: false,
    orderIndex: survey.value.questions.length,
    options: [],
  })
}

function removeQuestion(index) {
  survey.value.questions.splice(index, 1)
}

function addOption(questionIndex) {
  survey.value.questions[questionIndex].options.push({
    id: 0, // New options have ID 0
    optionText: '',
    orderIndex: survey.value.questions[questionIndex].options.length,
  })
}

function removeOption(questionIndex, optionIndex) {
  survey.value.questions[questionIndex].options.splice(optionIndex, 1)
}

function onQuestionTypeChange(question) {
  if (question.type === 'SHORT_TEXT' || question.type === 'LONG_TEXT') {
    question.options = []
  }
}

async function saveSurvey() {
  // Re-index questions and options before saving
  survey.value.questions.forEach((q, qIndex) => {
    q.orderIndex = qIndex
    if (q.options) {
      q.options.forEach((o, oIndex) => {
        o.orderIndex = oIndex
      })
    }
  })

  try {
    if (isEditing.value) {
      await api.put(`/surveys/${survey.value.id}`, survey.value)
      alert('설문이 성공적으로 수정되었습니다!')
    } else {
      await api.post('/surveys', survey.value)
      alert('설문이 성공적으로 생성되었습니다!')
    }
    emit('saved')
  } catch (error) {
    console.error('설문 저장에 실패했습니다:', error)
    alert('설문 저장에 실패했습니다.')
  }
}
</script>
