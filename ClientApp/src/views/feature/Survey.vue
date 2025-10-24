<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">Event Satisfaction Survey</h1>
    <div v-for="(question, index) in questions" :key="index" class="mb-4">
      <label class="block font-bold mb-2">{{ question.text }}</label>
      <input v-if="question.type === 'text'" type="text" v-model="answers[question.id]" class="w-full p-2 border rounded" :disabled="submitted">
      <div v-if="question.type === 'single-choice'">
        <div v-for="option in question.options" :key="option" class="flex items-center mb-2">
          <input type="radio" :name="question.id" :value="option" v-model="answers[question.id]" class="mr-2" :disabled="submitted">
          <label>{{ option }}</label>
        </div>
      </div>
    </div>
    <button @click="submitSurvey" class="bg-blue-500 text-white px-4 py-2 rounded" :disabled="submitted">Submit</button>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useAction } from '@/composables/useAction';
import apiClient from '@/services/api';
import { useRouter } from 'vue-router';

const route = useRoute();
const router = useRouter();
const { submitAction } = useAction();

const actionType = route.params.actionType;

const questions = ref([
  { id: 'q1', text: 'How satisfied are you with the event?', type: 'single-choice', options: ['Very Satisfied', 'Satisfied', 'Neutral', 'Unsatisfied', 'Very Unsatisfied'] },
  { id: 'q2', text: 'What was your favorite part of the event?', type: 'text' },
  { id: 'q3', text: 'Any suggestions for future events?', type: 'text' },
]);

const answers = ref({});
const submitted = ref(false);

onMounted(async () => {
  try {
    const response = await apiClient.get(`/surveys/${actionType}`);
    if (response.data) {
      const savedAnswers = {};
      for (const answer of response.data.answers) {
        const question = questions.value.find(q => q.text === answer.question);
        if (question) {
          savedAnswers[question.id] = answer.answer;
        }
      }
      answers.value = savedAnswers;
      submitted.value = true;
    }
  } catch (error) {
    if (error.response?.status !== 404) {
      console.error('Failed to fetch survey response:', error);
    }
  }
});

async function submitSurvey() {
  const responseData = {
    answers: Object.keys(answers.value).map(key => ({
      question: questions.value.find(q => q.id === key).text,
      answer: answers.value[key],
    })),
  };

  try {
    await apiClient.post(`/surveys/${actionType}`, responseData);
    await submitAction(actionType, responseData);
    alert('Survey submitted successfully!');
    router.push('/');
  } catch (error) {
    console.error('Failed to submit survey:', error);
    alert('Failed to submit survey.');
  }
}
</script>
