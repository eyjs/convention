<template>
  <div class="min-h-screen relative bg-gray-50">
    <div class="fixed inset-0 z-0 overflow-hidden pointer-events-none">
      <div
        class="absolute -top-40 -right-40 w-96 h-96 bg-gradient-to-br from-sky-200/15 to-blue-200/15 rounded-full blur-3xl"
      ></div>
    </div>

    <div class="relative z-10">
      <!-- 상단 로고 + 메뉴 -->
      <div class="sticky top-0 z-40 bg-white shadow-sm">
        <div class="px-4 py-3 flex items-center justify-between">
          <img
            src="https://www.ifa.co.kr/Images/logo.png"
            alt="iFA"
            class="h-8 object-contain"
          />
          <button
            class="p-2 -mr-2 rounded-lg text-gray-500 hover:bg-gray-100"
            @click="isSidebarOpen = true"
          >
            <svg
              class="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M4 6h16M4 12h16M4 18h16"
              />
            </svg>
          </button>
        </div>
      </div>

      <SidebarMenu :is-open="isSidebarOpen" @close="isSidebarOpen = false" />

      <div class="max-w-2xl mx-auto px-4 py-4">
        <!-- Hero: 인사말 -->
        <div
          class="relative overflow-hidden bg-gradient-to-br from-violet-600 via-purple-600 to-fuchsia-600 rounded-2xl shadow-xl p-6 mb-4 text-white"
        >
          <div class="absolute inset-0 overflow-hidden pointer-events-none">
            <div
              class="absolute top-1/4 right-1/4 w-64 h-64 bg-gradient-to-br from-pink-200/15 to-violet-200/15 rounded-full blur-3xl"
            ></div>
          </div>
          <div
            class="absolute top-0 right-0 w-28 h-28 bg-white/10 rounded-full -mr-14 -mt-14"
          ></div>
          <div
            class="absolute bottom-0 right-8 w-20 h-20 bg-white/10 rounded-full -mb-10"
          ></div>

          <div class="relative z-10">
            <h2 class="text-2xl font-bold mb-1">안녕하세요 {{ userName }}님</h2>
            <p class="text-sm text-white/80">iFA STARTOUR</p>
          </div>
        </div>

        <!-- 내 정보 점검 -->
        <div
          v-if="!allChecksPassed"
          class="bg-white rounded-xl shadow-sm mb-4 px-4 py-3 cursor-pointer hover:shadow-md transition-shadow"
          @click="router.push('/my-profile')"
        >
          <div class="flex items-center justify-between mb-2">
            <span class="text-sm font-semibold text-gray-700">내 정보</span>
            <span class="text-xs font-medium text-amber-600">
              {{ passedCount }}/{{ infoChecks.length }}
            </span>
          </div>
          <div class="h-2 bg-gray-100 rounded-full overflow-hidden">
            <div
              class="h-full bg-amber-500 rounded-full transition-all duration-500"
              :style="{ width: `${(passedCount / infoChecks.length) * 100}%` }"
            ></div>
          </div>
          <div class="flex flex-wrap gap-1.5 mt-2">
            <span
              v-for="check in failedChecks"
              :key="check.key"
              class="px-2 py-0.5 bg-red-50 text-red-500 text-xs rounded"
            >
              {{ check.label }}
            </span>
          </div>
        </div>

        <!-- 진행중인 스타투어 -->
        <div class="mb-6">
          <h3 class="text-lg font-bold text-gray-900 mb-3 px-1">
            진행중인 스타투어
          </h3>

          <div
            v-if="activeConventions.length === 0"
            class="bg-white rounded-2xl shadow-sm p-8 text-center"
          >
            <p class="text-sm text-gray-500">진행중인 스타투어가 없습니다</p>
          </div>

          <div v-else class="overflow-x-auto -mx-4 px-4 scrollbar-hide">
            <div class="flex gap-4 pb-2">
              <div
                v-for="convention in activeConventions"
                :key="convention.id"
                class="flex-shrink-0 w-[300px] bg-white rounded-2xl shadow-md hover:shadow-xl transition-all cursor-pointer overflow-hidden group"
                @click="goToConvention(convention)"
              >
                <div class="relative h-[180px] overflow-hidden">
                  <div
                    v-if="convention.conventionImg"
                    class="absolute inset-0 bg-cover bg-center group-hover:scale-105 transition-transform duration-500"
                    :style="{
                      backgroundImage: `url(${convention.conventionImg})`,
                    }"
                  ></div>
                  <div
                    v-else
                    class="absolute inset-0 bg-gradient-to-br from-rose-500 via-orange-500 to-amber-500"
                  ></div>
                  <div
                    v-if="getDDay(convention.startDate) > 0"
                    class="absolute top-3 right-3 px-3 py-1 bg-black/50 backdrop-blur-sm text-white text-sm font-bold rounded-full"
                  >
                    D-{{ getDDay(convention.startDate) }}
                  </div>
                </div>
                <div class="p-4">
                  <h4
                    class="font-bold text-gray-900 text-lg mb-1 line-clamp-1 group-hover:text-rose-600 transition-colors"
                  >
                    {{ convention.title }}
                  </h4>
                  <div class="flex items-center gap-2 text-sm text-gray-500">
                    <svg
                      class="w-4 h-4 flex-shrink-0"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                      />
                    </svg>
                    <span class="truncate"
                      >{{ formatDate(convention.startDate) }} ~
                      {{ formatDate(convention.endDate) }}</span
                    >
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 지난 스타투어 -->
        <div v-if="completedConventions.length > 0" class="mb-8">
          <h3 class="text-lg font-bold text-gray-500 mb-3 px-1">
            지난 스타투어
          </h3>
          <div class="overflow-x-auto -mx-4 px-4 scrollbar-hide">
            <div class="flex gap-3 pb-2">
              <div
                v-for="convention in completedConventions"
                :key="convention.id"
                class="flex-shrink-0 w-[220px] bg-white rounded-xl shadow-sm hover:shadow-md transition-shadow cursor-pointer overflow-hidden opacity-80"
                @click="goToConvention(convention)"
              >
                <div class="relative h-[120px] overflow-hidden">
                  <div
                    v-if="convention.conventionImg"
                    class="absolute inset-0 bg-cover bg-center grayscale"
                    :style="{
                      backgroundImage: `url(${convention.conventionImg})`,
                    }"
                  ></div>
                  <div
                    v-else
                    class="absolute inset-0 bg-gradient-to-br from-gray-400 to-gray-500"
                  ></div>
                  <div
                    class="absolute top-2 left-2 px-1.5 py-0.5 bg-black/50 text-white text-xs rounded backdrop-blur-sm"
                  >
                    종료
                  </div>
                </div>
                <div class="p-3">
                  <h4 class="font-semibold text-gray-600 text-sm line-clamp-1">
                    {{ convention.title }}
                  </h4>
                  <p class="text-xs text-gray-400 mt-0.5">
                    {{ formatDate(convention.startDate) }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import SidebarMenu from '@/components/common/SidebarMenu.vue'
import apiClient from '@/services/api'

const router = useRouter()
const authStore = useAuthStore()

const userName = computed(() => authStore.user?.name || '사용자')

const conventions = ref([])
const dashboard = ref({})
const isSidebarOpen = ref(false)

const activeConventions = computed(() =>
  conventions.value.filter((c) => c.completeYn !== 'Y'),
)
const completedConventions = computed(() =>
  conventions.value.filter((c) => c.completeYn === 'Y'),
)

// 정보 점검
const infoChecks = computed(() => {
  const p = dashboard.value.passport || {}
  return [
    { key: 'passportNumber', label: '여권번호', ok: !!p.passportNumber },
    {
      key: 'passportName',
      label: '영문명',
      ok: !!(p.firstName && p.lastName),
    },
    {
      key: 'passportExpiry',
      label: '만료일',
      ok: !!p.passportExpiryDate,
    },
    {
      key: 'passportImage',
      label: '여권 사본',
      ok: !!p.passportImageUrl,
    },
    { key: 'passportVerified', label: '승인', ok: !!p.verified },
  ]
})
const failedChecks = computed(() => infoChecks.value.filter((c) => !c.ok))
const passedCount = computed(() => infoChecks.value.filter((c) => c.ok).length)
const allChecksPassed = computed(() => failedChecks.value.length === 0)

function getDDay(startDate) {
  if (!startDate) return 0
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const start = new Date(startDate)
  start.setHours(0, 0, 0, 0)
  const diff = Math.ceil((start - today) / (1000 * 60 * 60 * 24))
  return diff > 0 ? diff : 0
}

function formatDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('ko-KR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  })
}

async function loadConventions() {
  try {
    const res = await apiClient.get('/users/conventions')
    conventions.value = res.data
  } catch {
    conventions.value = []
  }
}

async function loadDashboard() {
  try {
    const res = await apiClient.get('/users/home-dashboard')
    dashboard.value = res.data
  } catch (e) {
    console.error('대시보드 로드 실패:', e)
  }
}

function goToConvention(convention) {
  if (!convention?.id) return
  router.push(`/conventions/${convention.id}`)
}

onMounted(() => {
  loadConventions()
  loadDashboard()
})
</script>

<style scoped>
.scrollbar-hide {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
.scrollbar-hide::-webkit-scrollbar {
  display: none;
}
</style>
