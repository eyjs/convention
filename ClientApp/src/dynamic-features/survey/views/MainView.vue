<template>
  <div class="survey-page max-w-4xl mx-auto">
    <div v-if="loading" class="flex justify-center items-center min-h-[400px]">
      <div
        class="w-12 h-12 border-4 border-gray-200 border-t-blue-600 rounded-full animate-spin"
      ></div>
    </div>

    <div v-else-if="!selectedSurvey" class="space-y-4">
      <div
        v-for="survey in surveys"
        :key="survey.id"
        @click="selectSurvey(survey)"
        class="bg-white rounded-lg shadow-md p-6 cursor-pointer hover:shadow-lg transition-shadow"
      >
        <h3 class="text-xl font-bold text-gray-900 mb-2">{{ survey.title }}</h3>
        <p class="text-gray-600 mb-4">{{ survey.description }}</p>
        <div class="flex items-center gap-4 text-sm text-gray-500">
          <span>📝 질문 {{ survey.questionCount }}개</span>
          <span>⏱️ 약 {{ survey.estimatedMinutes }}분</span>
        </div>
      </div>

      <div v-if="surveys.length === 0" class="text-center py-12 text-gray-500">
        현재 진행 중인 설문이 없습니다.
      </div>
    </div>

    <div v-else class="bg-white rounded-lg shadow-md p-6">
      <button
        @click="selectedSurvey = null"
        class="mb-4 text-blue-600 hover:text-blue-800"
      >
        ← 목록으로
      </button>

      <h2 class="text-2xl font-bold text-gray-900 mb-2">
        {{ selectedSurvey.title }}
      </h2>
      <p class="text-gray-600 mb-6">{{ selectedSurvey.description }}</p>

      <div class="space-y-6">
        <div
          v-for="(question, index) in selectedSurvey.questions"
          :key="question.id"
          class="border-b pb-6"
        >
          <label class="block font-semibold text-gray-900 mb-3">
            {{ index + 1 }}. {{ question.text }}
          </label>

          <div v-if="question.type === 'multiple-choice'" class="space-y-2">
            <label
              v-for="option in question.options"
              :key="option.id"
              class="flex items-center gap-3 p-3 border rounded-lg hover:bg-gray-50 cursor-pointer"
            >
              <input
                type="radio"
                :name="`q${question.id}`"
                :value="option.id"
                v-model="userResponse[question.id]"
                class="w-4 h-4"
              />
              <span>{{ option.text }}</span>
            </label>
          </div>

          <textarea
            v-else-if="question.type === 'text'"
            v-model="userResponse[question.id]"
            rows="3"
            placeholder="답변을 입력하세요"
            class="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500"
          ></textarea>
        </div>
      </div>

      <button
        @click="submitResponse"
        class="w-full mt-6 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-semibold"
      >
        제출하기
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import axios from 'axios'

const props = defineProps({
  featureMetadata: { type: Object, default: () => ({}) },
})

const conventionId = localStorage.getItem('selectedConventionId')
const surveys = ref([
  {
    id: 1,
    title: '행사 만족도 조사',
    description: '이번 행사에 대한 여러분의 의견을 들려주세요',
    questionCount: 5,
    estimatedMinutes: 3,
    questions: [
      {
        id: 1,
        text: '이번 행사의 전반적인 만족도는 어떠셨나요?',
        type: 'multiple-choice',
        options: [
          { id: 1, text: '매우 만족' },
          { id: 2, text: '만족' },
          { id: 3, text: '보통' },
          { id: 4, text: '불만족' },
          { id: 5, text: '매우 불만족' },
        ],
      },
      {
        id: 2,
        text: '가장 인상 깊었던 프로그램은 무엇이었나요?',
        type: 'text',
      },
    ],
  },
])
const loading = ref(false)
const selectedSurvey = ref(null)
const userResponse = ref({})

onMounted(() => {
  console.log('SurveyPage mounted', props.featureMetadata)
})

onUnmounted(() => {
  console.log('SurveyPage unmounted')
})

function selectSurvey(survey) {
  selectedSurvey.value = survey
  userResponse.value = {}
}

async function submitResponse() {
  const unanswered = selectedSurvey.value.questions.filter(
    (q) => !userResponse.value[q.id],
  )
  if (unanswered.length > 0) {
    alert('모든 질문에 답변해주세요.')
    return
  }

  try {
    await axios.post(
      `/api/surveys/${selectedSurvey.value.id}/responses`,
      userResponse.value,
    )
    alert('설문 응답이 제출되었습니다!')
    selectedSurvey.value = null
    userResponse.value = {}
  } catch (error) {
    console.error('Failed to submit:', error)
    alert('제출 중 오류가 발생했습니다.')
  }
}
</script>

<style scoped>
.survey-page {
  padding: 1rem;
}
</style>
