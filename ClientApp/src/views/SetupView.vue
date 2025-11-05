<template>
  <div
    class="min-h-screen min-h-dvh bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4"
  >
    <div class="max-w-md w-full">
      <div class="bg-white rounded-2xl shadow-xl p-8">
        <div class="text-center mb-8">
          <div
            class="inline-flex items-center justify-center w-16 h-16 bg-indigo-100 rounded-full mb-4"
          >
            <svg
              class="w-8 h-8 text-indigo-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4"
              ></path>
            </svg>
          </div>
          <h1 class="text-3xl font-bold text-gray-900 mb-2">초기 설정</h1>
          <p class="text-gray-600">관리자 계정을 생성합니다</p>
        </div>

        <div v-if="!created" class="space-y-6">
          <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <div class="flex items-start">
              <svg
                class="w-5 h-5 text-blue-600 mt-0.5"
                fill="currentColor"
                viewBox="0 0 20 20"
              >
                <path
                  fill-rule="evenodd"
                  d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                  clip-rule="evenodd"
                ></path>
              </svg>
              <div class="ml-3">
                <h3 class="text-sm font-medium text-blue-800">
                  생성될 계정 정보
                </h3>
                <div class="mt-2 text-sm text-blue-700">
                  <p><strong>아이디:</strong> admin</p>
                  <p><strong>비밀번호:</strong> admin123</p>
                </div>
              </div>
            </div>
          </div>

          <button
            @click="createAdmin"
            :disabled="loading"
            class="w-full bg-indigo-600 hover:bg-indigo-700 disabled:bg-gray-400 text-white font-semibold py-3 px-4 rounded-lg transition duration-200 flex items-center justify-center"
          >
            <svg
              v-if="loading"
              class="animate-spin -ml-1 mr-3 h-5 w-5 text-white"
              fill="none"
              viewBox="0 0 24 24"
            >
              <circle
                class="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                stroke-width="4"
              ></circle>
              <path
                class="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
              ></path>
            </svg>
            {{ loading ? '생성 중...' : '관리자 계정 생성' }}
          </button>

          <div
            v-if="error"
            class="bg-red-50 border border-red-200 rounded-lg p-4"
          >
            <div class="flex items-start">
              <svg
                class="w-5 h-5 text-red-600 mt-0.5"
                fill="currentColor"
                viewBox="0 0 20 20"
              >
                <path
                  fill-rule="evenodd"
                  d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                  clip-rule="evenodd"
                ></path>
              </svg>
              <p class="ml-3 text-sm text-red-700">{{ error }}</p>
            </div>
          </div>
        </div>

        <div v-else class="space-y-6">
          <div class="bg-green-50 border border-green-200 rounded-lg p-6">
            <div class="flex items-center justify-center mb-4">
              <div
                class="w-12 h-12 bg-green-100 rounded-full flex items-center justify-center"
              >
                <svg
                  class="w-6 h-6 text-green-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M5 13l4 4L19 7"
                  ></path>
                </svg>
              </div>
            </div>
            <h3 class="text-center text-lg font-semibold text-green-900 mb-2">
              계정 생성 완료!
            </h3>
            <div class="bg-white rounded-lg p-4 mb-4">
              <p class="text-sm text-gray-600 mb-2">로그인 정보:</p>
              <div class="space-y-1 font-mono text-sm">
                <p>
                  <span class="text-gray-500">아이디:</span>
                  <strong>{{ adminInfo.loginId }}</strong>
                </p>
                <p>
                  <span class="text-gray-500">비밀번호:</span>
                  <strong>{{ adminInfo.password }}</strong>
                </p>
              </div>
            </div>
            <p class="text-xs text-center text-gray-600">
              이 정보를 안전한 곳에 보관하세요
            </p>
          </div>

          <router-link
            to="/login"
            class="block w-full bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-3 px-4 rounded-lg transition duration-200 text-center"
          >
            로그인 페이지로 이동
          </router-link>
        </div>
      </div>

      <p class="text-center text-sm text-gray-600 mt-6">
        이미 계정이 있으신가요?
        <router-link
          to="/login"
          class="text-indigo-600 hover:text-indigo-700 font-medium"
          >로그인</router-link
        >
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'

const loading = ref(false)
const created = ref(false)
const error = ref('')
const adminInfo = ref({
  loginId: '',
  password: '',
})

const createAdmin = async () => {
  loading.value = true
  error.value = ''

  try {
    const response = await axios.post('/api/setup/create-admin')

    adminInfo.value = {
      loginId: response.data.loginId,
      password: response.data.password,
    }

    created.value = true
  } catch (err) {
    error.value =
      err.response?.data?.message || '계정 생성 중 오류가 발생했습니다.'
  } finally {
    loading.value = false
  }
}
</script>
