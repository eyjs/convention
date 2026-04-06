<template>
  <div class="min-h-screen relative bg-gray-50">
    <div class="relative z-10">
      <!-- 상단 로고 + 메뉴 -->
      <div class="sticky top-0 z-40 bg-white shadow-sm">
        <div class="px-4 py-3">
          <div class="flex items-center justify-between">
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
      </div>

      <SidebarMenu :is-open="isSidebarOpen" @close="isSidebarOpen = false" />

      <div class="max-w-2xl mx-auto px-4 py-4">
        <!-- 내 정보 점검 -->
        <div
          class="bg-white rounded-xl shadow-sm mb-4 overflow-hidden cursor-pointer hover:shadow-md transition-shadow"
          @click="router.push('/my-profile')"
        >
          <div
            class="px-4 py-2.5 flex items-center justify-between"
            :class="allChecksPassed ? 'bg-green-50' : 'bg-amber-50'"
          >
            <span
              class="text-sm font-semibold"
              :class="allChecksPassed ? 'text-green-700' : 'text-amber-700'"
            >
              {{
                allChecksPassed
                  ? '✅ 정보 등록 완료'
                  : '⚠️ 등록이 필요한 정보가 있습니다'
              }}
            </span>
            <svg
              class="w-4 h-4 flex-shrink-0"
              :class="allChecksPassed ? 'text-green-400' : 'text-amber-400'"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 5l7 7-7 7"
              />
            </svg>
          </div>
          <div v-if="!allChecksPassed" class="px-4 py-3 space-y-1.5">
            <div
              v-for="check in failedChecks"
              :key="check.key"
              class="flex items-center gap-2 text-sm"
            >
              <span class="text-red-400">✕</span>
              <span class="text-gray-600">{{ check.label }}</span>
            </div>
          </div>
        </div>

        <!-- 내 일정 타임테이블 -->
        <div v-if="dashboard.mySchedules?.length > 0" class="mb-6">
          <h3 class="text-lg font-bold text-gray-900 mb-3 px-1">내 일정</h3>
          <div class="bg-white rounded-2xl shadow-md overflow-hidden">
            <div
              v-for="(schedule, i) in dashboard.mySchedules"
              :key="schedule.id || i"
              class="flex items-stretch border-b last:border-b-0"
            >
              <!-- 시간 -->
              <div
                class="flex-shrink-0 w-20 py-3 px-3 flex flex-col items-center justify-center"
                :class="isToday(schedule.date) ? 'bg-primary-50' : 'bg-gray-50'"
              >
                <span
                  class="text-xs font-medium"
                  :class="
                    isToday(schedule.date)
                      ? 'text-primary-600'
                      : 'text-gray-500'
                  "
                  >{{ formatScheduleDate(schedule.date) }}</span
                >
                <span
                  class="text-sm font-bold"
                  :class="
                    isToday(schedule.date)
                      ? 'text-primary-700'
                      : 'text-gray-700'
                  "
                  >{{ schedule.time || '—' }}</span
                >
              </div>
              <!-- 내용 -->
              <div
                class="flex-1 py-3 px-4 min-w-0 cursor-pointer hover:bg-gray-50 transition-colors"
                @click="
                  schedule.conventionId &&
                  router.push(`/conventions/${schedule.conventionId}/schedule`)
                "
              >
                <p class="font-medium text-gray-900 text-sm truncate">
                  {{ schedule.title }}
                </p>
                <p
                  v-if="schedule.location"
                  class="text-xs text-gray-500 truncate mt-0.5"
                >
                  {{ schedule.location }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- 진행중인 스타투어 (가로 스크롤) -->
        <div class="mb-6">
          <div class="flex justify-between items-center mb-3 px-1">
            <h3 class="text-lg font-bold text-gray-900">진행중인 스타투어</h3>
          </div>

          <div
            v-if="conventionsLoading"
            class="text-center py-12 text-gray-500"
          >
            <div
              class="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600 mx-auto"
            ></div>
          </div>

          <div
            v-else-if="activeConventions.length === 0"
            class="bg-white rounded-2xl shadow-md p-8 text-center"
          >
            <p class="text-gray-500">진행중인 스타투어가 없습니다</p>
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

        <!-- 종료된 스타투어 -->
        <div v-if="completedConventions.length > 0" class="mb-8">
          <h3 class="text-lg font-bold text-gray-500 mb-3 px-1">
            종료된 스타투어
          </h3>
          <div class="overflow-x-auto -mx-4 px-4 scrollbar-hide">
            <div class="flex gap-3 pb-2">
              <div
                v-for="convention in completedConventions.slice(0, 10)"
                :key="convention.id"
                class="flex-shrink-0 w-[200px] bg-white rounded-xl shadow-sm hover:shadow-md transition-shadow cursor-pointer overflow-hidden opacity-75"
                @click="goToConvention(convention)"
              >
                <div class="relative h-[100px] overflow-hidden">
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

const conventionsLoading = ref(true)
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
    { key: 'passportNumber', label: '여권번호 등록', ok: !!p.passportNumber },
    {
      key: 'passportName',
      label: '영문 이름 등록',
      ok: !!(p.firstName && p.lastName),
    },
    {
      key: 'passportExpiry',
      label: '여권 만료일 등록',
      ok: !!p.passportExpiryDate,
    },
    {
      key: 'passportImage',
      label: '여권 사본 업로드',
      ok: !!p.passportImageUrl,
    },
    { key: 'passportVerified', label: '여권 승인', ok: !!p.verified },
  ]
})
const failedChecks = computed(() => infoChecks.value.filter((c) => !c.ok))
const allChecksPassed = computed(() => failedChecks.value.length === 0)

const passportExpiryDays = computed(() => {
  const expiry = dashboard.value.passport?.passportExpiryDate
  if (!expiry) return null
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const exp = new Date(expiry)
  exp.setHours(0, 0, 0, 0)
  return Math.ceil((exp - today) / (1000 * 60 * 60 * 24))
})

function isToday(dateStr) {
  if (!dateStr) return false
  const d = new Date(dateStr)
  const today = new Date()
  return (
    d.getFullYear() === today.getFullYear() &&
    d.getMonth() === today.getMonth() &&
    d.getDate() === today.getDate()
  )
}

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

function formatScheduleDate(dateStr) {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  const days = ['일', '월', '화', '수', '목', '금', '토']
  return `${d.getMonth() + 1}/${d.getDate()}(${days[d.getDay()]})`
}

async function loadConventions() {
  conventionsLoading.value = true
  try {
    const res = await apiClient.get('/users/conventions')
    conventions.value = res.data
  } catch (e) {
    conventions.value = []
  } finally {
    conventionsLoading.value = false
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
