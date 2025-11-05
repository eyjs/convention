<template>
  <div
    class="min-h-screen min-h-dvh flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 px-4 py-8"
  >
    <div class="max-w-md w-full">
      <!-- 로고/제목 -->
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900 mb-2">비밀번호 찾기</h1>
        <p class="text-gray-600">아이디와 가입 정보를 입력해주세요</p>
      </div>

      <!-- 카드 -->
      <div class="bg-white rounded-2xl shadow-xl p-8">
        <!-- Step 1: 정보 입력 및 인증번호 발송 -->
        <div v-if="step === 1">
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2"
                >아이디</label
              >
              <input
                v-model="form.loginId"
                type="text"
                class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="아이디를 입력하세요"
              />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2"
                >이름</label
              >
              <input
                v-model="form.name"
                type="text"
                class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="홍길동"
              />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2"
                >전화번호</label
              >
              <input
                v-model="form.phoneNumber"
                type="tel"
                class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="01012345678"
                maxlength="11"
              />
            </div>
          </div>

          <button
            @click="sendCode"
            :disabled="
              loading || !form.loginId || !form.name || !form.phoneNumber
            "
            class="w-full mt-6 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed font-medium transition-colors"
          >
            {{ loading ? '전송 중...' : '인증번호 발송' }}
          </button>
        </div>

        <!-- Step 2: 인증번호 입력 -->
        <div v-if="step === 2">
          <div class="mb-6 p-4 bg-blue-50 rounded-lg">
            <p class="text-sm text-blue-900">
              <strong>{{ form.phoneNumber }}</strong
              >로 인증번호를 발송했습니다.
            </p>
            <p class="text-xs text-blue-700 mt-1">5분 내에 입력해주세요.</p>
          </div>

          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2"
                >인증번호</label
              >
              <input
                v-model="form.code"
                type="text"
                class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent text-center text-2xl tracking-widest"
                placeholder="000000"
                maxlength="6"
              />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2"
                >새 비밀번호</label
              >
              <input
                v-model="form.newPassword"
                type="password"
                class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="최소 6자 이상"
              />
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2"
                >새 비밀번호 확인</label
              >
              <input
                v-model="form.confirmPassword"
                type="password"
                class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="비밀번호 재입력"
              />
            </div>
          </div>

          <div
            v-if="
              form.newPassword &&
              form.confirmPassword &&
              form.newPassword !== form.confirmPassword
            "
            class="mt-2 text-sm text-red-600"
          >
            비밀번호가 일치하지 않습니다.
          </div>

          <div class="flex gap-3 mt-6">
            <button
              @click="step = 1"
              class="flex-1 px-6 py-3 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 font-medium transition-colors"
            >
              이전
            </button>
            <button
              @click="verifyAndReset"
              :disabled="
                loading ||
                form.code.length !== 6 ||
                !form.newPassword ||
                form.newPassword !== form.confirmPassword
              "
              class="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed font-medium transition-colors"
            >
              {{ loading ? '처리 중...' : '비밀번호 재설정' }}
            </button>
          </div>
        </div>

        <!-- Step 3: 완료 -->
        <div v-if="step === 3">
          <div class="text-center mb-6">
            <div
              class="inline-flex items-center justify-center w-16 h-16 bg-green-100 rounded-full mb-4"
            >
              <svg
                class="w-8 h-8 text-green-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M5 13l4 4L19 7"
                />
              </svg>
            </div>
            <h3 class="text-xl font-bold text-gray-900 mb-2">
              비밀번호가 재설정되었습니다
            </h3>
            <p class="text-gray-600">새로운 비밀번호로 로그인해주세요.</p>
          </div>

          <button
            @click="$router.push('/login')"
            class="w-full px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium transition-colors"
          >
            로그인하기
          </button>
        </div>

        <!-- 에러 메시지 -->
        <div v-if="error" class="mt-4 p-4 bg-red-50 rounded-lg">
          <p class="text-sm text-red-800">{{ error }}</p>
        </div>
      </div>

      <!-- 하단 링크 -->
      <div class="mt-6 text-center space-y-2">
        <router-link
          to="/login"
          class="text-sm text-gray-600 hover:text-gray-900"
        >
          로그인으로 돌아가기
        </router-link>
        <span class="text-gray-400 mx-2">|</span>
        <router-link
          to="/find-id"
          class="text-sm text-gray-600 hover:text-gray-900"
        >
          아이디 찾기
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/services/api'

const router = useRouter()

const step = ref(1)
const loading = ref(false)
const error = ref('')

const form = ref({
  loginId: '',
  name: '',
  phoneNumber: '',
  code: '',
  newPassword: '',
  confirmPassword: '',
})

const sendCode = async () => {
  loading.value = true
  error.value = ''

  try {
    await apiClient.post('/accountrecovery/reset-password/send-code', {
      loginId: form.value.loginId,
      name: form.value.name,
      phoneNumber: form.value.phoneNumber,
    })

    step.value = 2
  } catch (err) {
    error.value = err.response?.data?.message || '인증번호 발송에 실패했습니다.'
  } finally {
    loading.value = false
  }
}

const verifyAndReset = async () => {
  if (form.value.newPassword.length < 6) {
    error.value = '비밀번호는 최소 6자 이상이어야 합니다.'
    return
  }

  if (form.value.newPassword !== form.value.confirmPassword) {
    error.value = '비밀번호가 일치하지 않습니다.'
    return
  }

  loading.value = true
  error.value = ''

  try {
    await apiClient.post('/accountrecovery/reset-password/verify', {
      loginId: form.value.loginId,
      name: form.value.name,
      phoneNumber: form.value.phoneNumber,
      code: form.value.code,
      newPassword: form.value.newPassword,
    })

    step.value = 3
  } catch (err) {
    error.value =
      err.response?.data?.message || '비밀번호 재설정에 실패했습니다.'
  } finally {
    loading.value = false
  }
}
</script>
