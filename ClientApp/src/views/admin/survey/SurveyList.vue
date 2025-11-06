<template>
  <div class="p-4 sm:p-6 bg-gray-50 dark:bg-gray-900 min-h-screen">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-2xl sm:text-3xl font-bold text-gray-800 dark:text-gray-200">Survey List</h1>
      <router-link to="/admin/surveys/new" class="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 text-sm font-medium">
        Create New Survey
      </router-link>
    </div>

    <div v-if="loading" class="text-center py-10">
      <p class="text-gray-500 dark:text-gray-400">Loading surveys...</p>
    </div>
    <div v-else-if="error" class="text-center py-10 text-red-500">
      <p>{{ error }}</p>
    </div>
    <div v-else class="bg-white dark:bg-gray-800 p-4 sm:p-6 rounded-lg shadow-md">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead class="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Title
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Status
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Convention ID
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase tracking-wider">
                Created At
              </th>
              <th scope="col" class="relative px-6 py-3">
                <span class="sr-only">Actions</span>
              </th>
            </tr>
          </thead>
          <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
            <tr v-for="survey in surveys" :key="survey.id" class="hover:bg-gray-50 dark:hover:bg-gray-700/50">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm font-medium text-gray-900 dark:text-gray-200">{{ survey.title }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span :class="[
                  'px-2 inline-flex text-xs leading-5 font-semibold rounded-full',
                  survey.isActive ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100' : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-100'
                ]">
                  {{ survey.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900 dark:text-gray-300">{{ survey.conventionId || 'N/A' }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900 dark:text-gray-300">{{ new Date(survey.createdAt).toLocaleDateString() }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <router-link :to="`/admin/surveys/${survey.id}/edit`" class="text-indigo-600 hover:text-indigo-900 dark:text-indigo-400 dark:hover:text-indigo-300 mr-4">Edit</router-link>
                <router-link :to="`/admin/surveys/${survey.id}/stats`" class="text-green-600 hover:text-green-900 dark:text-green-400 dark:hover:text-green-300">Stats</router-link>
              </td>
            </tr>
            <tr v-if="surveys.length === 0">
                <td colspan="5" class="px-6 py-4 text-center text-sm text-gray-500 dark:text-gray-400">
                    No surveys found.
                </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import api from '@/api'; // Assuming you have an axios instance configured at @/api

const surveys = ref([]);
const loading = ref(true);
const error = ref(null);

onMounted(async () => {
  try {
    const response = await api.get('/surveys');
    surveys.value = response.data;
  } catch (err) {
    error.value = "Failed to load surveys.";
    console.error(err);
  } finally {
    loading.value = false;
  }
});
</script>
