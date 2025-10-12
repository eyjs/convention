<template>
  <div class="min-h-screen bg-gradient-to-br from-primary-500 to-primary-700 flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl w-full max-w-md p-8">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900 mb-2">참석자 로그인</h1>
        <p class="text-gray-600">이름과 연락처로 로그인하세요</p>
      </div>

      <form @submit.prevent="handleLogin" class="space-y-6">
        <!-- 행사 선택 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">행사 선택 *</label>
          <select
            v-model="form.conventionId"
            required
            class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          >
            <option value="">행사를 선택하세요</option>
            <option v-for="convention in conventions" :key="convention.id" :value="convention.id">
              {{ convention.title }}
            </option>
          </select>
        </div>

        <!-- 이름 -->
        <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">이름 *</label>
            <template>
                <div class="min-h-screen bg-gradient-to-br from-primary-500 to-primary-700 flex items-center justify-center p-4">
                    <div class="bg-white rounded-2xl shadow-2xl w-full max-w-md p-8">
                        <div class="text-center mb-8">
                            <h1 class="text-3xl font-bold text-gray-900 mb-2">참석자 로그인</h1>
                            <p class="text-gray-600">이름과 연락처로 로그인하세요</p>
                        </div>

                        <form @submit.prevent="handleLogin" class="space-y-6">
                            <div>
                                <label class="block text-sm font-medium text-gray-700 mb-2">행사 선택 *</label>
                                <select v-model="form.conventionId"
                                        required
                                        class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent">
                                    <option value="">행사를 선택하세요</option>
                                    <option v-for="convention in conventions" :key="convention.id" :value="convention.id">
                                        {{ convention.title }}
                                    </option>
                                </select>
                            </div>

                            <div>
                                <label class="block text-sm font-medium text-gray-700 mb-2">이름 *</label>
                                <input v-model="form.name"
                                       type="text"
                                       required
                                       placeholder="홍길동"
                                       class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" />
                            </div>

                            <div>
                                <label class="block text-sm font-medium text-gray-700 mb-2">연락처 *</label>
                                <input v-model="form.phone"
                                       type="tel"
                                       required
                                       placeholder="010-1234-5678 또는 01012345678"
                                       class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" />
                                <p class="text-xs text-gray-500 mt-1">'-' 기호는 있어도 되고 없어도 됩니다</p>
                            </div>

                            <div v-if="errorMessage" class="bg-red-50 border border-red-200 rounded-lg p-4">
                                <p class="text-sm text-red-600">{{ errorMessage }}</p>
                            </div>

                            <button type="submit"
                                    :disabled="authStore.loading"
                                    class="w-full py-3 bg-primary-600 text-white rounded-lg font-semibold hover:bg-primary-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors">
                                <span v-if="authStore.loading">로그인 중...</span>
                                <span v-else>로그인</span>
                            </button>

                            <div class="text-center pt-4 border-t">
                                <p class="text-sm text-gray-600">
                                    회원이신가요?
                                    <router-link to="/login" class="text-primary-600 hover:text-primary-700 font-semibold">
                                        일반 로그인
                                    </router-link>
                                </p>
                            </div>
                        </form>
                    </div>
                </div>
            </template>

            <script setup>
                import { ref, onMounted } from 'vue'
                import { useRouter } from 'vue-router'
                import axios from 'axios'
                import { useAuthStore } from '@/stores/auth' // 👈 authStore 임포트

                const router = useRouter()
                const authStore = useAuthStore() // 👈 authStore 사용

                const form = ref({
                    conventionId: '',
                    name: '',
                    phone: ''
                })

                const conventions = ref([])
                const errorMessage = ref('')

                const loadConventions = async () => {
                    try {
                        const response = await axios.get('/api/conventions') // 여기는 토큰이 필요 없으므로 axios 직접 사용
                        conventions.value = response.data.filter(c => c.deleteYn === 'N' && c.completeYn === 'N')
                    } catch (error) {
                        console.error('Failed to load conventions:', error)
                    }
                }

                const handleLogin = async () => {
                    errorMessage.value = ''

                    const { conventionId, name, phone } = form.value
                    const result = await authStore.guestLogin(parseInt(conventionId), name, phone)

                    if (result.success) {
                        router.push('/') // 로그인 성공 시 홈으로 이동
                    } else {
                        errorMessage.value = result.error
                        if (result.error.includes('회원으로 전환된 계정')) {
                            setTimeout(() => {
                                router.push('/login')
                            }, 2000)
                        }
                    }
                }

                onMounted(() => {
                    loadConventions()
                })
            </script>
            <input v-model="form.name"
                   type="text"
                   required
                   placeholder="홍길동"
                   class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent" />
        </div>

        <!-- 연락처 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">연락처 *</label>
          <input
            v-model="form.phone"
            type="tel"
            required
            placeholder="010-1234-5678 또는 01012345678"
            class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          />
          <p class="text-xs text-gray-500 mt-1">'-' 기호는 있어도 되고 없어도 됩니다</p>
        </div>

        <!-- 에러 메시지 -->
        <div v-if="errorMessage" class="bg-red-50 border border-red-200 rounded-lg p-4">
          <p class="text-sm text-red-600">{{ errorMessage }}</p>
        </div>

        <!-- 로그인 버튼 -->
        <button
          type="submit"
          :disabled="loading"
          class="w-full py-3 bg-primary-600 text-white rounded-lg font-semibold hover:bg-primary-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
        >
          <span v-if="loading">로그인 중...</span>
          <span v-else>로그인</span>
        </button>

        <!-- 일반 로그인 링크 -->
        <div class="text-center pt-4 border-t">
          <p class="text-sm text-gray-600">
            회원이신가요?
            <router-link to="/login" class="text-primary-600 hover:text-primary-700 font-semibold">
              일반 로그인
            </router-link>
          </p>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

const router = useRouter()

const form = ref({
  conventionId: '',
  name: '',
  phone: ''
})

const conventions = ref([])
const loading = ref(false)
const errorMessage = ref('')

const loadConventions = async () => {
  try {
    const response = await axios.get('/api/conventions')
    conventions.value = response.data.filter(c => c.deleteYn === 'N' && c.completeYn === 'N')
  } catch (error) {
    console.error('Failed to load conventions:', error)
  }
}

const handleLogin = async () => {
  loading.value = true
  errorMessage.value = ''

  try {
    const response = await axios.post('/api/auth/guest-login', {
      name: form.value.name,
      phone: form.value.phone,
      conventionId: parseInt(form.value.conventionId)
    })

    // 토큰 저장
    localStorage.setItem('accessToken', response.data.accessToken)
    localStorage.setItem('isGuest', 'true')
    localStorage.setItem('guestInfo', JSON.stringify(response.data.guest))
    localStorage.setItem('convention', JSON.stringify(response.data.convention))

    // 참석자 메인 화면으로 이동
    router.push(`/guest/${response.data.convention.id}`)
  } catch (error) {
    if (error.response?.status === 400 && error.response.data.loginId) {
      // 회원으로 전환된 경우
      errorMessage.value = error.response.data.message
      setTimeout(() => {
        router.push('/login')
      }, 2000)
    } else {
      errorMessage.value = error.response?.data?.message || '로그인에 실패했습니다. 정보를 확인해주세요.'
    }
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadConventions()
})
</script>
