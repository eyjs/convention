<template>
  <div class="container mx-auto p-4 sm:p-6">
    <div v-if="loading" class="text-center py-10">
      <p class="text-gray-500 dark:text-gray-400">Loading survey...</p>
    </div>
    <div v-else-if="error" class="text-center py-10 text-red-500">
      <p>{{ error }}</p>
    </div>
    <div v-else-if="survey" class="max-w-2xl mx-auto bg-white dark:bg-gray-800 p-6 sm:p-8 rounded-lg shadow-md">
      <h1 class="text-2xl sm:text-3xl font-bold text-gray-800 dark:text-gray-200 mb-2">{{ survey.title }}</h1>
      <p class="text-gray-600 dark:text-gray-400 mb-8">{{ survey.description }}</p>
      
      <form @submit.prevent="submitSurvey">
        <div v-for="(question, index) in survey.questions" :key="question.id" class="mb-8">
          <label class="block font-medium text-gray-700 dark:text-gray-300">
            {{ index + 1 }}. {{ question.questionText }}
            <span v-if="question.isRequired" class="text-red-500 ml-1">*</span>
          </label>
          
          <div v-if="question.type === 'SHORT_TEXT'" class="mt-3">
            <input type="text" v-model="responses[question.id]" class="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 focus:ring-indigo-500 focus:border-indigo-500">
          </div>
          
          <div v-if="question.type === 'LONG_TEXT'" class="mt-3">
            <textarea v-model="responses[question.id]" rows="4" class="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 focus:ring-indigo-500 focus:border-indigo-500"></textarea>
          </div>
          
          <div v-if="question.type === 'SINGLE_CHOICE'" class="mt-3 space-y-3">
            <div v-for="option in question.options" :key="option.id" class="flex items-center p-3 rounded-md border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/50">
              <input type="radio" :id="'option-' + option.id" :name="'question-' + question.id" :value="option.id" v-model="responses[question.id]" class="h-4 w-4 text-indigo-600 border-gray-300 focus:ring-indigo-500">
              <label :for="'option-' + option.id" class="ml-3 block text-sm font-medium text-gray-700 dark:text-gray-300">{{ option.optionText }}</label>
            </div>
          </div>

          <div v-if="question.type === 'MULTIPLE_CHOICE'" class="mt-3 space-y-3">
            <div v-for="option in question.options" :key="option.id" class="flex items-center p-3 rounded-md border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/50">
              <input type="checkbox" :id="'option-' + option.id" :value="option.id" v-model="responses[question.id]" class="h-4 w-4 text-indigo-600 border-gray-300 rounded">
              <label :for="'option-' + option.id" class="ml-3 block text-sm font-medium text-gray-700 dark:text-gray-300">{{ option.optionText }}</label>
            </div>
          </div>
        </div>
        
        <div class="mt-10 flex justify-between space-x-4">
          <button type="button" @click="router.back()" class="w-1/2 bg-gray-300 text-gray-800 py-3 px-4 rounded-md hover:bg-gray-400 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 font-semibold">
            뒤로가기
          </button>
          <button type="submit" :disabled="isSubmitting" class="w-1/2 bg-indigo-600 text-white py-3 px-4 rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:bg-indigo-400 disabled:cursor-not-allowed font-semibold">
            {{ isSubmitting ? '제출 중...' : '제출' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, reactive } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import api from '@/api';

const route = useRoute();
const router = useRouter();
const survey = ref(null);
const loading = ref(true);
const error = ref(null);
const responses = reactive({});
const isSubmitting = ref(false);

onMounted(async () => {
  const surveyId = route.params.id;
  if (!surveyId) {
    error.value = "Survey ID not found.";
    loading.value = false;
    return;
  }

  try {
    const response = await api.get(`/surveys/${surveyId}`);
    survey.value = response.data;
    // Initialize responses object and fetch previous response if exists
    const userResponse = await api.get(`/surveys/${surveyId}/responses/me`).catch(() => null);

    survey.value.questions.forEach(q => {
      const previousAnswerDetail = userResponse?.data?.answers?.find(a => a.questionId === q.id);

      if (q.type === 'MULTIPLE_CHOICE') {
        const allSelectedOptionIds = userResponse?.data?.answers
          .filter(a => a.questionId === q.id && a.selectedOptionId !== null)
          .map(a => a.selectedOptionId) || [];
        responses[q.id] = allSelectedOptionIds;
        console.log(`Question ${q.id} (MULTIPLE_CHOICE): Options:`, q.options, `Previous selected:`, allSelectedOptionIds, `Assigned:`, responses[q.id]);
      } else if (q.type === 'SINGLE_CHOICE') {
        responses[q.id] = previousAnswerDetail?.selectedOptionId || null;
        console.log(`Question ${q.id} (SINGLE_CHOICE): Options:`, q.options, `Previous selected:`, previousAnswerDetail?.selectedOptionId, `Assigned:`, responses[q.id]);
      } else { // SHORT_TEXT or LONG_TEXT
        responses[q.id] = previousAnswerDetail?.answerText || null;
        console.log(`Question ${q.id} (TEXT): Previous answer:`, previousAnswerDetail?.answerText, `Assigned:`, responses[q.id]);
      }
    });
  } catch (err) {
    error.value = "Failed to load survey.";
    console.error(err);
  } finally {
    loading.value = false;
  }
});

async function submitSurvey() {
  isSubmitting.value = true;

  // Basic validation
  for (const question of survey.value.questions) {
    if (question.isRequired) {
      const answer = responses[question.id];
      if (answer === null || answer === '' || (Array.isArray(answer) && answer.length === 0)) {
        alert(`Question "${question.questionText}" is required.`);
        isSubmitting.value = false;
        return;
      }
    }
  }

  const submissionData = {
    SurveyId: survey.value.id, // PascalCase로 수정
    Answers: Object.keys(responses).map(questionId => { // PascalCase로 수정
      const answer = responses[questionId];
      const question = survey.value.questions.find(q => q.id == questionId);

      if (question.type === 'MULTIPLE_CHOICE') {
        return {
          QuestionId: parseInt(questionId), // PascalCase로 수정
          SelectedOptionIds: answer // PascalCase로 수정
        };
      } else if (question.type === 'SINGLE_CHOICE') {
        return {
          QuestionId: parseInt(questionId), // PascalCase로 수정
          SelectedOptionIds: answer ? [answer] : [] // PascalCase로 수정
        };
      } else {
        return {
          QuestionId: parseInt(questionId), // PascalCase로 수정
          AnswerText: answer // PascalCase로 수정
        };
      }
    })
  };

  try {
    await api.post(`/surveys/${survey.value.id}/submit`, submissionData);
    alert('Thank you for your feedback!');
    // Optionally, redirect or show a success message. For example, go back.
    if (window.history.length > 1) {
        router.go(-1);
    } else {
        router.push('/');
    }
  } catch (err) {
    console.error("Error submitting survey:", err);
    alert('There was an error submitting your response. Please try again.');
  } finally {
    isSubmitting.value = false;
  }
}
</script>