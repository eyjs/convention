<template>
  <div class="min-h-screen flex flex-col justify-center items-center font-sans text-white bg-[url('https://images.unsplash.com/photo-1526778548025-fa2f459cd5c1')] bg-cover bg-center relative overflow-hidden">
    <div class="absolute inset-0 bg-black/50"></div>

    <header class="flex justify-center py-8">
      <img src="/images/logo_w.png" alt="iFA Logo" class="w-28 drop-shadow-lg" />
    </header>

    <!-- Hero Section -->
    <section class="relative z-10 text-center space-y-4 animate-fade-in">
      <h1 class="text-4xl md:text-5xl font-bold drop-shadow-lg">iFA StarTour</h1>
      <p class="text-sm text-white/70 max-w-md mx-auto">당신의 여행이 시작됩니다. 감동과 여유가 있는 순간을 지금 만나보세요.</p>

      <button @click="openLoginModal" class="mt-6 px-8 py-3 bg-white/90 text-gray-800 font-semibold rounded-full shadow-md hover:bg-white transition">로그인하기</button>
    </section>

    <!-- 로그인 모달 -->
    <div v-if="isModalOpen" class="fixed inset-0 bg-black/70 flex justify-center items-center z-50" @click.self="closeLoginModal">
        <div class="bg-white rounded-3xl shadow-2xl p-8 w-full max-w-md text-center animate-fade-up relative">
            <button @click="closeLoginModal" class="absolute top-4 right-4 text-gray-500 hover:text-gray-700">✕</button>
            <h2 class="text-2xl font-bold text-gray-800 mb-2">로그인</h2>
            <p class="text-sm text-gray-600 mb-6">참가자 전용 로그인</p>

            <!-- 탭 -->
            <div class="flex space-x-2 mb-6 bg-gray-100 p-1 rounded-xl">
                <button @click="activeTab = 'login'"
                        :class="[
              'flex-1 py-2 px-4 rounded-lg font-medium transition-all',
              activeTab === 'login'
                ? 'bg-white text-[#17B185] shadow-sm'
                : 'text-gray-600 hover:text-gray-900'
            ]">
                    로그인
                </button>
                <button @click="activeTab = 'register'"
                        :class="[
              'flex-1 py-2 px-4 rounded-lg font-medium transition-all',
              activeTab === 'register'
                ? 'bg-white text-[#17B185] shadow-sm'
                : 'text-gray-600 hover:text-gray-900'
            ]">
                    회원가입
                </button>
            </div>

            <!-- 에러 메시지 -->
            <div v-if="errorMessage" class="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg">
                <p class="text-sm text-red-600">{{ errorMessage }}</p>
            </div>

            <!-- 로그인 폼 -->
            <form v-if="activeTab === 'login'" @submit.prevent="handleLogin" class="space-y-5">
                <div class="text-left">
                    <label for="email" class="text-sm text-gray-700 font-medium">아이디</label>
                    <input id="email" v-model="loginForm.loginId" type="text" placeholder="이메일 입력" required class="w-full mt-1 px-3 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#17B185] focus:border-[#17B185] outline-none text-gray-800" />
                </div>
                <div class="text-left">
                    <label for="password" class="text-sm text-gray-700 font-medium">비밀번호</label>
                    <input id="password" v-model="loginForm.password" type="password" placeholder="비밀번호 입력" required class="w-full mt-1 px-3 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#17B185] focus:border-[#17B185] outline-none text-gray-800" />
                </div>
                <button type="submit" :disabled="authStore.loading" class="w-full py-3 bg-[#17B185] text-white rounded-xl font-semibold shadow-md hover:shadow-lg transition">
                    {{ authStore.loading ? '로그인 중...' : '로그인' }}
                </button>
            </form>

            <!-- 회원가입 폼 -->
            <form v-if="activeTab === 'register'" @submit.prevent="handleRegister" class="space-y-4">
                <div class="text-left">
                    <label class="block text-sm font-medium text-gray-700 mb-1">아이디 *</label>
                    <input v-model="registerForm.loginId" type="text" required class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#17B185] focus:border-[#17B185] outline-none text-gray-800" placeholder="아이디" />
                </div>
                <div class="text-left">
                    <label class="block text-sm font-medium text-gray-700 mb-1">비밀번호 *</label>
                    <input v-model="registerForm.password" type="password" required minlength="6" class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#17B185] focus:border-[#17B185] outline-none text-gray-800" placeholder="6자 이상" />
                </div>
                <div class="text-left">
                    <label class="block text-sm font-medium text-gray-700 mb-1">비밀번호 확인 *</label>
                    <input v-model="registerForm.passwordConfirm" type="password" required class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#17B185] focus:border-[#17B185] outline-none text-gray-800" placeholder="비밀번호 재입력" />
                </div>
                <div class="text-left">
                    <label class="block text-sm font-medium text-gray-700 mb-1">이름 *</label>
                    <input v-model="registerForm.name" type="text" required class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#17B185] focus:border-[#17B185] outline-none text-gray-800" placeholder="이름" />
                </div>
                <div class="text-left">
                    <label class="block text-sm font-medium text-gray-700 mb-1">이메일</label>
                    <input v-model="registerForm.email" type="email" class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#17B185] focus:border-[#17B185] outline-none text-gray-800" placeholder="example@email.com" />
                </div>
                <div class="text-left">
                    <label class="block text-sm font-medium text-gray-700 mb-1">전화번호</label>
                    <input v-model="registerForm.phone" type="tel" class="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#17B185] focus:border-[#17B185] outline-none text-gray-800" placeholder="010-1234-5678" />
                </div>
                <button type="submit" :disabled="authStore.loading" class="w-full py-3 bg-[#17B185] text-white rounded-xl font-semibold shadow-md hover:shadow-lg transition">
                    {{ authStore.loading ? '처리 중...' : '회원가입' }}
                </button>
            </form>
        </div>
    </div>

    <footer class="absolute bottom-6 text-xs text-white/80 z-10">© 2025 iFA StarTour. All rights reserved.</footer>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const isModalOpen = ref(false)
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

const openLoginModal = () => {
  isModalOpen.value = true
}

const closeLoginModal = () => {
  isModalOpen.value = false
}

async function handleLogin() {
  errorMessage.value = ''
  const result = await authStore.login(loginForm.value.loginId, loginForm.value.password)
  if (result.success) {
    if (authStore.isAdmin) {
      router.push('/admin')
    } else {
      router.push('/my-conventions')
    }
  } else {
    errorMessage.value = result.error
  }
}

async function handleRegister() {
  errorMessage.value = ''
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

<style>
@keyframes fade-in {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
@keyframes fade-up {
  from { opacity: 0; transform: translateY(50px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in { animation: fade-in 1.2s ease-out forwards; }
.animate-fade-up { animation: fade-up 0.6s ease-out forwards; }
</style>
