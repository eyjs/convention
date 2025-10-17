<template>
  <div class="min-h-screen min-h-dvh bg-gradient-to-br from-primary-500 to-primary-700 flex items-center justify-center p-4">
    <!-- 배경 패턴 -->
    <div class="absolute inset-0 opacity-10">
      <div class="absolute top-0 left-0 w-96 h-96 bg-white rounded-full -translate-x-48 -translate-y-48"></div>
      <div class="absolute bottom-0 right-0 w-96 h-96 bg-white rounded-full translate-x-48 translate-y-48"></div>
    </div>

    <div class="relative w-full max-w-md">
      <!-- 로고 -->
      <div class="text-center mb-8">
        <div class="w-20 h-20 bg-white rounded-2xl flex items-center justify-center mx-auto mb-4 shadow-xl">
          <span class="text-3xl font-bold text-primary-600">iFA</span>
        </div>
        <h1 class="text-3xl font-bold text-white mb-2">Convention</h1>
        <p class="text-white/80 text-sm">행사 관리 시스템</p>
      </div>

      <!-- 로그인/회원가입 카드 -->
      <div class="bg-white rounded-2xl shadow-2xl p-8">
        <!-- 탭 -->
        <div class="flex space-x-2 mb-6 bg-gray-100 p-1 rounded-xl">
          <button
            @click="activeTab = 'login'"
            :class="[
              'flex-1 py-2 px-4 rounded-lg font-medium transition-all',
              activeTab === 'login'
                ? 'bg-white text-primary-600 shadow-sm'
                : 'text-gray-600 hover:text-gray-900'
            ]"
          >
            로그인
          </button>
          <button
            @click="activeTab = 'register'"
            :class="[
              'flex-1 py-2 px-4 rounded-lg font-medium transition-all',
              activeTab === 'register'
                ? 'bg-white text-primary-600 shadow-sm'
                : 'text-gray-600 hover:text-gray-900'
            ]"
          >
            회원가입
          </button>
        </div>

        <!-- 에러 메시지 -->
        <div v-if="errorMessage" class="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg">
          <p class="text-sm text-red-600">{{ errorMessage }}</p>
        </div>

        <!-- 로그인 폼 -->
        <form v-if="activeTab === 'login'" @submit.prevent="handleLogin" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">아이디</label>
            <input
              v-model="loginForm.loginId"
              type="text"
              required
              class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              placeholder="아이디를 입력하세요"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">비밀번호</label>
            <input
              v-model="loginForm.password"
              type="password"
              required
              class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              placeholder="비밀번호를 입력하세요"
            />
          </div>

          <button
            type="submit"
            :disabled="authStore.loading"
            class="w-full py-3 bg-primary-600 hover:bg-primary-700 text-white font-semibold rounded-xl transition-all disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ authStore.loading ? '로그인 중...' : '로그인' }}
          </button>

          <!-- 아이디/비밀번호 찾기 -->
          <div class="flex justify-center gap-4 text-sm">
            <router-link to="/find-id" class="text-gray-600 hover:text-primary-600 transition-colors">
              아이디 찾기
            </router-link>
            <span class="text-gray-300">|</span>
            <router-link to="/find-password" class="text-gray-600 hover:text-primary-600 transition-colors">
              비밀번호 찾기
            </router-link>
          </div>
        </form>

        <!-- 회원가입 폼 -->
        <form v-if="activeTab === 'register'" @submit.prevent="handleRegister" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">아이디 *</label>
            <input
              v-model="registerForm.loginId"
              type="text"
              required
              class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              placeholder="아이디"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">비밀번호 *</label>
            <input
              v-model="registerForm.password"
              type="password"
              required
              minlength="6"
              class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              placeholder="6자 이상"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">비밀번호 확인 *</label>
            <input
              v-model="registerForm.passwordConfirm"
              type="password"
              required
              class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              placeholder="비밀번호 재입력"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">이름 *</label>
            <input
              v-model="registerForm.name"
              type="text"
              required
              class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              placeholder="이름"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">이메일</label>
            <input
              v-model="registerForm.email"
              type="email"
              class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              placeholder="example@email.com"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">전화번호</label>
            <input
              v-model="registerForm.phone"
              type="tel"
              class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              placeholder="010-1234-5678"
            />
          </div>

          <button
            type="submit"
            :disabled="authStore.loading"
            class="w-full py-3 bg-primary-600 hover:bg-primary-700 text-white font-semibold rounded-xl transition-all disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ authStore.loading ? '처리 중...' : '회원가입' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const activeTab = ref('login')
const errorMessage = ref('')

const loginForm = ref({
  loginId: '',
  password: ''
})

const registerForm = ref({
  loginId: '',
  password: '',
  passwordConfirm: '',
  name: '',
  email: '',
  phone: ''
})

async function handleLogin() {
  errorMessage.value = ''

  const result = await authStore.login(loginForm.value.loginId, loginForm.value.password)

  if (result.success) {
    router.push('/')
  } else {
    errorMessage.value = result.error
  }
}

async function handleRegister() {
  errorMessage.value = ''

  // 비밀번호 확인
  if (registerForm.value.password !== registerForm.value.passwordConfirm) {
    errorMessage.value = '비밀번호가 일치하지 않습니다.'
    return
  }

  const { passwordConfirm, ...data } = registerForm.value
  const result = await authStore.register(data)

  if (result.success) {
    alert('회원가입이 완료되었습니다. 로그인해주세요.')
    activeTab.value = 'login'
    registerForm.value = {
      loginId: '',
      password: '',
      passwordConfirm: '',
      name: '',
      email: '',
      phone: ''
    }
  } else {
    errorMessage.value = result.error
  }
}
</script>
