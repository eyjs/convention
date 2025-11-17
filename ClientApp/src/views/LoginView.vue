<template>
  <div
    class="min-h-screen flex flex-col justify-center items-center font-sans text-white bg-black bg-cover bg-center relative overflow-hidden"
  >
    <video 
      ref="videoPlayer"
      src="/videos/travel_splash.mp4" 
      autoplay 
      loop 
      muted 
      playsinline
      class="absolute top-0 left-0 w-full h-full object-cover"
    ></video>
    <div class="absolute inset-0 bg-black/50"></div>

    <header class="flex justify-center py-8 relative z-10">
      <img
        src="/images/logo_w.png"
        alt="iFA Logo"
        class="w-28 drop-shadow-lg"
      />
    </header>

    <!-- Hero Section -->
    <section class="relative z-10 text-center space-y-4 animate-fade-in">
      <h1 class="text-4xl md:text-5xl font-bold drop-shadow-lg">StarTour</h1>
      <p class="text-sm text-white/70 max-w-md mx-auto">
        당신의 여행이 시작됩니다. 감동과 여유가 있는 순간을 지금 만나보세요.
      </p>

      <button
        @click="openLoginModal"
        class="mt-6 px-8 py-3 bg-white/90 text-gray-800 font-semibold rounded-full shadow-md hover:bg-white transition"
      >
        로그인하기
      </button>
    </section>

    <!-- 로그인 모달 -->
    <Transition name="modal-slide-bottom">
      <div
        v-if="isModalOpen"
        class="fixed inset-0 bg-black/60 flex justify-center items-end z-50"
        @click.self="closeLoginModal"
      >
        <div
          class="bg-white rounded-t-3xl shadow-2xl w-full max-w-md text-center relative max-h-[90vh] overflow-y-auto"
        >
          <div class="pt-8 pb-8 px-8">
          <button
            @click="closeLoginModal"
            class="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
          >
            ✕
          </button>
          <h2 class="text-2xl font-bold text-gray-800 mb-2">로그인</h2>
          <p class="text-sm text-gray-600 mb-4">참가자 전용 로그인</p>

          <!-- 탭 -->
          <div class="flex space-x-2 bg-gray-100 p-1 rounded-xl mb-6">
            <button
              @click="activeTab = 'login'"
              :class="[
                'flex-1 py-2 px-4 rounded-lg font-medium transition-all',
                activeTab === 'login'
                  ? 'bg-white text-[#17B185] shadow-sm'
                  : 'text-gray-600 hover:text-gray-900',
              ]"
            >
              로그인
            </button>
            <button
              @click="activeTab = 'register'"
              :class="[
                'flex-1 py-2 px-4 rounded-lg font-medium transition-all',
                activeTab === 'register'
                  ? 'bg-white text-[#17B185] shadow-sm'
                  : 'text-gray-600 hover:text-gray-900',
              ]"
            >
              회원가입
            </button>
          </div>

          <!-- 에러 메시지 -->
          <div
            v-if="errorMessage"
            class="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg"
          >
            <p class="text-sm text-red-600">{{ errorMessage }}</p>
          </div>

          <!-- 로그인 폼 -->
          <form
            v-if="activeTab === 'login'"
            @submit.prevent="handleLogin"
            class="space-y-5"
          >
            <div class="text-left">
              <label for="email" class="text-sm text-gray-700 font-medium"
                >아이디</label
              >
              <input
                id="email"
                v-model="loginForm.loginId"
                type="text"
                placeholder="아이디 입력"
                required
                class="w-full mt-1 px-3 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#17B185] focus:border-[#17B185] outline-none text-gray-800"
              />
            </div>
            <div class="text-left">
              <label for="password" class="text-sm text-gray-700 font-medium"
                >비밀번호</label
              >
              <input
                id="password"
                v-model="loginForm.password"
                type="password"
                placeholder="비밀번호 입력"
                required
                class="w-full mt-1 px-3 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#17B185] focus:border-[#17B185] outline-none text-gray-800"
              />
            </div>
            <button
              type="submit"
              :disabled="authStore.loading"
              class="w-full py-3 bg-[#17B185] text-white rounded-xl font-semibold shadow-md hover:shadow-lg transition"
            >
              {{ authStore.loading ? '로그인 중...' : '로그인' }}
            </button>
          </form>

          <!-- 회원가입 폼 -->
          <form
            v-if="activeTab === 'register'"
            @submit.prevent="handleRegister"
            class="space-y-4"
          >
            <div class="text-left">
              <label class="block text-sm font-medium text-gray-700 mb-1"
                >아이디 *</label
              >
              <input
                v-model="registerForm.loginId"
                @blur="validation.loginId.touched = true"
                type="text"
                required
                class="w-full px-4 py-3 border border-gray-300 rounded-xl outline-none transition-all text-gray-800"
                :class="{
                  'border-red-500 focus:border-red-500 focus:ring-red-200':
                    validation.loginId.touched && !validation.loginId.isValid,
                  'border-green-500 focus:border-green-500 focus:ring-green-200':
                    validation.loginId.touched && validation.loginId.isValid,
                }"
                placeholder="2자 이상"
              />
              <div
                v-if="validation.loginId.touched && validation.loginId.message"
                class="mt-1 text-xs flex items-center"
                :class="
                  validation.loginId.isValid ? 'text-green-600' : 'text-red-600'
                "
              >
                <svg
                  v-if="validation.loginId.isValid"
                  class="w-4 h-4 mr-1"
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
                <span>{{ validation.loginId.message }}</span>
              </div>
            </div>
            <div class="text-left">
              <label class="block text-sm font-medium text-gray-700 mb-1"
                >비밀번호 *</label
              >
              <input
                v-model="registerForm.password"
                @blur="validation.password.touched = true"
                type="password"
                required
                minlength="6"
                class="w-full px-4 py-3 border border-gray-300 rounded-xl outline-none transition-all text-gray-800"
                :class="{
                  'border-red-500 focus:border-red-500 focus:ring-red-200':
                    validation.password.touched && !validation.password.isValid,
                  'border-green-500 focus:border-green-500 focus:ring-green-200':
                    validation.password.touched && validation.password.isValid,
                }"
                placeholder="6자 이상"
              />
              <div
                v-if="
                  validation.password.touched && validation.password.message
                "
                class="mt-1 text-xs flex items-center"
                :class="
                  validation.password.isValid
                    ? 'text-green-600'
                    : 'text-red-600'
                "
              >
                <svg
                  v-if="validation.password.isValid"
                  class="w-4 h-4 mr-1"
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
                <span>{{ validation.password.message }}</span>
              </div>
            </div>
            <div class="text-left">
              <label class="block text-sm font-medium text-gray-700 mb-1"
                >비밀번호 확인 *</label
              >
              <input
                v-model="registerForm.passwordConfirm"
                @blur="validation.passwordConfirm.touched = true"
                type="password"
                required
                class="w-full px-4 py-3 border border-gray-300 rounded-xl outline-none transition-all text-gray-800"
                :class="{
                  'border-red-500 focus:border-red-500 focus:ring-red-200':
                    validation.passwordConfirm.touched &&
                    !validation.passwordConfirm.isValid,
                  'border-green-500 focus:border-green-500 focus:ring-green-200':
                    validation.passwordConfirm.touched &&
                    validation.passwordConfirm.isValid,
                }"
                placeholder="비밀번호 재입력"
              />
              <div
                v-if="
                  validation.passwordConfirm.touched &&
                  validation.passwordConfirm.message
                "
                class="mt-1 text-xs flex items-center"
                :class="
                  validation.passwordConfirm.isValid
                    ? 'text-green-600'
                    : 'text-red-600'
                "
              >
                <svg
                  v-if="validation.passwordConfirm.isValid"
                  class="w-4 h-4 mr-1"
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
                <span>{{ validation.passwordConfirm.message }}</span>
              </div>
            </div>
            <div class="text-left">
              <label class="block text-sm font-medium text-gray-700 mb-1"
                >이름 *</label
              >
              <input
                v-model="registerForm.name"
                @blur="validation.name.touched = true"
                type="text"
                required
                class="w-full px-4 py-3 border border-gray-300 rounded-xl outline-none transition-all text-gray-800"
                :class="{
                  'border-red-500 focus:border-red-500 focus:ring-red-200':
                    validation.name.touched && !validation.name.isValid,
                  'border-green-500 focus:border-green-500 focus:ring-green-200':
                    validation.name.touched && validation.name.isValid,
                }"
                placeholder="이름"
              />
              <div
                v-if="validation.name.touched && validation.name.message"
                class="mt-1 text-xs flex items-center"
                :class="
                  validation.name.isValid ? 'text-green-600' : 'text-red-600'
                "
              >
                <svg
                  v-if="validation.name.isValid"
                  class="w-4 h-4 mr-1"
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
                <span>{{ validation.name.message }}</span>
              </div>
            </div>
            <div class="text-left">
              <label class="block text-sm font-medium text-gray-700 mb-1"
                >이메일</label
              >
              <input
                v-model="registerForm.email"
                @blur="validation.email.touched = true"
                type="email"
                class="w-full px-4 py-3 border border-gray-300 rounded-xl outline-none transition-all text-gray-800"
                :class="{
                  'border-red-500 focus:border-red-500 focus:ring-red-200':
                    validation.email.touched && !validation.email.isValid,
                  'border-green-500 focus:border-green-500 focus:ring-green-200':
                    validation.email.touched &&
                    validation.email.isValid &&
                    registerForm.email,
                }"
                placeholder="example@email.com"
              />
              <div
                v-if="validation.email.touched && validation.email.message"
                class="mt-1 text-xs flex items-center"
                :class="
                  validation.email.isValid ? 'text-green-600' : 'text-red-600'
                "
              >
                <svg
                  v-if="validation.email.isValid && registerForm.email"
                  class="w-4 h-4 mr-1"
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
                <span>{{ validation.email.message }}</span>
              </div>
            </div>
            <div class="text-left">
              <label class="block text-sm font-medium text-gray-700 mb-1"
                >전화번호</label
              >
              <input
                v-model="registerForm.phone"
                @blur="validation.phone.touched = true"
                type="tel"
                class="w-full px-4 py-3 border border-gray-300 rounded-xl outline-none transition-all text-gray-800"
                :class="{
                  'border-red-500 focus:border-red-500 focus:ring-red-200':
                    validation.phone.touched && !validation.phone.isValid,
                  'border-green-500 focus:border-green-500 focus:ring-green-200':
                    validation.phone.touched &&
                    validation.phone.isValid &&
                    registerForm.phone,
                }"
                placeholder="전화번호 (- 없이 입력 가능)"
              />
              <div
                v-if="validation.phone.touched && validation.phone.message"
                class="mt-1 text-xs flex items-center"
                :class="
                  validation.phone.isValid ? 'text-green-600' : 'text-red-600'
                "
              >
                <svg
                  v-if="validation.phone.isValid && registerForm.phone"
                  class="w-4 h-4 mr-1"
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
                <span>{{ validation.phone.message }}</span>
              </div>
            </div>
            <button
              type="submit"
              :disabled="authStore.loading || !isRegistrationFormValid"
              class="w-full py-3 bg-[#17B185] text-white rounded-xl font-semibold shadow-md hover:shadow-lg transition disabled:bg-gray-400 disabled:cursor-not-allowed"
            >
              {{ authStore.loading ? '처리 중...' : '회원가입' }}
            </button>
          </form>
          </div>
        </div>
      </div>
    </Transition>

    <div class="relative z-10 text-center mb-2">
      <a
        href="#"
        @click.prevent="openPrivacyPolicyModal"
        class="text-white/80 text-xs underline hover:text-white"
        >개인정보처리 방침</a
      >
    </div>
    <footer class="absolute bottom-6 text-xs text-white/80 z-10">
      © 2025 iFA StarTour. All rights reserved.
    </footer>

    <PrivacyPolicyModal
      :is-open="showPrivacyPolicyModal"
      @close="closePrivacyPolicyModal"
    />
  </div>
</template>

<script setup>
import { ref, watch, computed, onMounted } from 'vue'
import PrivacyPolicyModal from '@/components/common/PrivacyPolicyModal.vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const isModalOpen = ref(false)
const activeTab = ref('login')
const errorMessage = ref('')
const showPrivacyPolicyModal = ref(false)

const loginForm = ref({
  loginId: '',
  password: '',
})

const registerForm = ref({
  loginId: '',
  password: '',
  passwordConfirm: '',
  name: '',
  email: '',
  phone: '',
})

const videoPlayer = ref(null)
onMounted(() => {
  if (videoPlayer.value) {
    videoPlayer.value.play().catch(error => {
      console.error("Video play failed:", error);
    });
  }
})

// --- Real-time Validation Logic ---
const validation = ref({
  loginId: { isValid: false, message: '', touched: false },
  password: { isValid: false, message: '', touched: false },
  passwordConfirm: { isValid: false, message: '', touched: false },
  name: { isValid: false, message: '', touched: false },
  email: { isValid: true, message: '', touched: false }, // Optional, so starts as valid
  phone: { isValid: true, message: '', touched: false }, // Optional, so starts as valid
})

const emailRegex =
  /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/

function formatPhoneNumber(phoneStr) {
  if (!phoneStr) return ''
  const digits = phoneStr.replace(/\D/g, '')
  if (digits.length === 11) {
    return digits.replace(/(\d{3})(\d{4})(\d{4})/, '$1-$2-$3')
  }
  if (digits.length === 10) {
    if (digits.startsWith('02')) {
      return digits.replace(/(\d{2})(\d{4})(\d{4})/, '$1-$2-$3')
    }
    return digits.replace(/(\d{3})(\d{3})(\d{4})/, '$1-$2-$3')
  }
  return phoneStr
}

watch(
  () => registerForm.value.loginId,
  (val) => {
    const field = validation.value.loginId
    field.touched = true
    if (val.length < 2) {
      field.isValid = false
      field.message = '아이디는 2자 이상이어야 합니다.'
    } else {
      field.isValid = true
      field.message = '사용 가능한 아이디입니다.'
    }
  },
)

watch(
  () => registerForm.value.password,
  (val) => {
    const field = validation.value.password
    field.touched = true
    if (val.length < 6) {
      field.isValid = false
      field.message = '비밀번호는 6자 이상이어야 합니다.'
    } else {
      field.isValid = true
      field.message = '안전한 비밀번호입니다.'
    }
    // Also re-validate password confirm
    if (validation.value.passwordConfirm.touched) {
      const confirmVal = registerForm.value.passwordConfirm
      const confirmField = validation.value.passwordConfirm
      if (val !== confirmVal) {
        confirmField.isValid = false
        confirmField.message = '비밀번호가 일치하지 않습니다.'
      } else {
        confirmField.isValid = true
        confirmField.message = '비밀번호가 일치합니다.'
      }
    }
  },
)

watch(
  () => registerForm.value.passwordConfirm,
  (val) => {
    const field = validation.value.passwordConfirm
    field.touched = true
    if (val !== registerForm.value.password) {
      field.isValid = false
      field.message = '비밀번호가 일치하지 않습니다.'
    } else {
      field.isValid = true
      field.message = '비밀번호가 일치합니다.'
    }
  },
)

watch(
  () => registerForm.value.name,
  (val) => {
    const field = validation.value.name
    field.touched = true
    if (!val) {
      field.isValid = false
      field.message = '이름을 입력해주세요.'
    } else {
      field.isValid = true
      field.message = '확인되었습니다.'
    }
  },
)

watch(
  () => registerForm.value.email,
  (val) => {
    const field = validation.value.email
    field.touched = true
    if (val && !emailRegex.test(val)) {
      field.isValid = false
      field.message = '올바른 이메일 형식이 아닙니다.'
    } else {
      field.isValid = true
      field.message = val ? '유효한 이메일입니다.' : ''
    }
  },
)

watch(
  () => registerForm.value.phone,
  (val) => {
    const field = validation.value.phone
    field.touched = true
    const digits = val.replace(/\D/g, '') // Remove all non-digits

    if (val && (digits.length < 10 || digits.length > 11)) {
      field.isValid = false
      field.message = '유효한 전화번호를 입력해주세요 (10-11자리).'
    } else {
      field.isValid = true
      field.message = val ? '유효한 전화번호입니다.' : ''
    }
  },
)

const isRegistrationFormValid = computed(() => {
  return Object.values(validation.value).every((field) => field.isValid)
})
// --- End Validation Logic ---

const openLoginModal = () => {
  isModalOpen.value = true
}

const closeLoginModal = () => {
  isModalOpen.value = false
}

const openPrivacyPolicyModal = () => {
  showPrivacyPolicyModal.value = true
}

const closePrivacyPolicyModal = () => {
  showPrivacyPolicyModal.value = false
}

async function handleLogin() {
  errorMessage.value = ''
  const result = await authStore.login(
    loginForm.value.loginId,
    loginForm.value.password,
  )
  if (result.success) {
    if (authStore.isAdmin) {
      router.push('/admin')
    } else {
      router.push('/home')
    }
  } else {
    errorMessage.value = result.error
  }
}

async function handleRegister() {
  errorMessage.value = ''
  // Mark all fields as touched to show errors
  for (const key in validation.value) {
    validation.value[key].touched = true
  }

  if (!isRegistrationFormValid.value) {
    errorMessage.value = '입력 정보를 다시 확인해주세요.'
    return
  }

  // Format phone number before submitting
  const formattedPhone = formatPhoneNumber(registerForm.value.phone)

  const { passwordConfirm, ...data } = {
    ...registerForm.value,
    phone: formattedPhone,
  }
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
      phone: '',
    }
    // Reset validation state
    Object.values(validation.value).forEach((field) => {
      field.touched = false
      field.message = ''
    })
  } else {
    errorMessage.value = result.error
  }
}
</script>

<style>
@keyframes fade-in {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
.animate-fade-in {
  animation: fade-in 1.2s ease-out forwards;
}

/* New Modal Slide Animation */
.modal-slide-bottom-enter-active,
.modal-slide-bottom-leave-active {
  transition: all 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
}
.modal-slide-bottom-enter-active .bg-white,
.modal-slide-bottom-leave-active .bg-white {
  transition: all 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.modal-slide-bottom-enter-from,
.modal-slide-bottom-leave-to {
  background-color: rgba(0, 0, 0, 0);
}

.modal-slide-bottom-enter-from .bg-white,
.modal-slide-bottom-leave-to .bg-white {
  transform: translateY(100%);
}
</style>
