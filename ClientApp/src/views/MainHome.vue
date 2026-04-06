<template>
  <div class="min-h-screen relative bg-gray-50">
    <!-- Decorative Background -->
    <div class="fixed inset-0 z-0 overflow-hidden pointer-events-none">
      <div
        class="absolute -top-40 -right-40 w-96 h-96 bg-gradient-to-br from-sky-200/15 to-blue-200/15 rounded-full blur-3xl"
      ></div>
      <div
        class="absolute top-1/3 -left-32 w-80 h-80 bg-gradient-to-br from-blue-200/12 to-cyan-200/12 rounded-full blur-3xl"
      ></div>
    </div>

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
        <!-- 환영 배너 -->
        <div
          class="relative overflow-hidden bg-gradient-to-br from-violet-600 via-purple-600 to-fuchsia-600 rounded-2xl shadow-xl p-6 mb-6 text-white"
        >
          <div class="absolute inset-0 overflow-hidden">
            <div
              class="absolute top-1/4 right-1/4 w-64 h-64 bg-gradient-to-br from-pink-200/15 to-violet-200/15 rounded-full blur-3xl"
            ></div>
          </div>
          <div class="relative z-10">
            <h2 class="text-2xl font-bold mb-1">
              {{ userName }}님, 안녕하세요!
            </h2>
            <p class="text-sm text-white/80">iFA STARTOUR</p>
          </div>
          <div
            class="absolute top-0 right-0 w-28 h-28 bg-white/10 rounded-full -mr-14 -mt-14"
          ></div>
        </div>

        <!-- 내 여권 정보 -->
        <div class="mb-6">
          <h3 class="text-lg font-bold text-gray-900 mb-3 px-1">내 여권</h3>
          <div
            class="bg-white rounded-2xl shadow-md p-5 cursor-pointer hover:shadow-lg transition-shadow"
            @click="router.push('/my-profile')"
          >
            <div v-if="dashboard.passport?.passportNumber" class="space-y-3">
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-xs text-gray-500">여권번호</p>
                  <p class="text-lg font-bold text-gray-900 tracking-wider">
                    {{ dashboard.passport.passportNumber }}
                  </p>
                </div>
                <span
                  class="px-2.5 py-1 rounded-full text-xs font-semibold"
                  :class="
                    dashboard.passport.verified
                      ? 'bg-green-100 text-green-700'
                      : 'bg-yellow-100 text-yellow-700'
                  "
                >
                  {{ dashboard.passport.verified ? '승인완료' : '승인대기' }}
                </span>
              </div>
              <div class="flex gap-4 text-sm">
                <div>
                  <p class="text-xs text-gray-400">영문명</p>
                  <p class="font-medium text-gray-700">
                    {{
                      dashboard.passport.firstName ||
                      dashboard.passport.lastName
                        ? `${dashboard.passport.firstName || ''} ${dashboard.passport.lastName || ''}`
                        : '-'
                    }}
                  </p>
                </div>
                <div>
                  <p class="text-xs text-gray-400">만료일</p>
                  <p class="font-medium" :class="passportExpiryClass">
                    {{
                      dashboard.passport.passportExpiryDate
                        ? dashboard.passport.passportExpiryDate
                        : '-'
                    }}
                    <span v-if="passportExpiryDays !== null" class="text-xs">
                      ({{
                        passportExpiryDays > 0
                          ? `${passportExpiryDays}일 남음`
                          : '만료됨'
                      }})
                    </span>
                  </p>
                </div>
              </div>
            </div>

            <!-- 미등록 상태 -->
            <div v-else class="text-center py-4">
              <div
                class="w-14 h-14 mx-auto mb-3 bg-gray-100 rounded-full flex items-center justify-center"
              >
                <svg
                  class="w-7 h-7 text-gray-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                  />
                </svg>
              </div>
              <p class="text-gray-600 font-medium mb-1">
                여권 정보를 등록해주세요
              </p>
              <p class="text-xs text-gray-400">
                해외 행사 참여를 위해 여권 정보가 필요합니다
              </p>
            </div>
          </div>
        </div>

        <!-- 나의 여행 이력 -->
        <div v-if="dashboard.travelHistory?.totalTrips > 0" class="mb-6">
          <h3 class="text-lg font-bold text-gray-900 mb-3 px-1">
            나의 여행 이력
          </h3>
          <div class="bg-white rounded-2xl shadow-md p-5">
            <div class="flex items-center gap-6 mb-4">
              <div class="text-center">
                <p class="text-3xl font-bold text-primary-600">
                  {{ dashboard.travelHistory.totalTrips }}
                </p>
                <p class="text-xs text-gray-500">총 참여</p>
              </div>
              <div class="text-center">
                <p class="text-3xl font-bold text-green-600">
                  {{ dashboard.travelHistory.completedTrips }}
                </p>
                <p class="text-xs text-gray-500">완료</p>
              </div>
              <div class="text-center">
                <p class="text-3xl font-bold text-blue-600">
                  {{ dashboard.travelHistory.visitedCountries?.length || 0 }}
                </p>
                <p class="text-xs text-gray-500">방문 국가</p>
              </div>
            </div>
            <!-- 방문 도시 태그 -->
            <div
              v-if="dashboard.travelHistory.visitedCities?.length > 0"
              class="flex flex-wrap gap-2"
            >
              <span
                v-for="city in dashboard.travelHistory.visitedCities"
                :key="city"
                class="px-2.5 py-1 bg-gray-100 text-gray-600 text-xs rounded-full"
              >
                {{ city }}
              </span>
            </div>
          </div>
        </div>

        <!-- 인천공항 현황 -->
        <div class="mb-6">
          <h3 class="text-lg font-bold text-gray-900 mb-3 px-1">
            인천공항 현황
          </h3>
          <div class="bg-white rounded-2xl shadow-md p-5">
            <div v-if="airportLoading" class="text-center py-4 text-gray-400">
              <div
                class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-2"
              ></div>
              <p class="text-sm">인천공항 정보 조회 중...</p>
            </div>
            <div v-else-if="airportFlights.length > 0">
              <div class="flex items-center justify-between mb-3">
                <p class="text-sm text-gray-500">
                  오늘 출발편
                  <span class="font-semibold text-gray-900"
                    >{{ airportFlights.length }}건</span
                  >
                </p>
                <span class="text-xs text-gray-400">{{
                  new Date().toLocaleDateString('ko-KR')
                }}</span>
              </div>
              <div class="space-y-2">
                <div
                  v-for="flight in airportFlights.slice(0, 5)"
                  :key="flight.flightNum || flight.flightId"
                  class="flex items-center justify-between p-2.5 bg-gray-50 rounded-lg text-sm"
                >
                  <div class="flex items-center gap-3">
                    <span class="font-bold text-gray-900">{{
                      flight.flightNum || flight.flightId
                    }}</span>
                    <span class="text-gray-500">{{ flight.airport }}</span>
                  </div>
                  <div class="flex items-center gap-2">
                    <span class="text-gray-700">{{
                      flight.scheduleTime || flight.scheduleDateTime
                    }}</span>
                    <span
                      class="px-1.5 py-0.5 rounded text-xs font-medium"
                      :class="
                        getFlightStatusClass(flight.status || flight.remark)
                      "
                    >
                      {{ flight.status || flight.remark || '정상' }}
                    </span>
                  </div>
                </div>
              </div>
              <p class="text-xs text-gray-400 mt-3 text-center">
                인천국제공항 출발편 기준 (최근 5편)
              </p>
            </div>
            <div v-else class="text-center py-4 text-gray-400">
              <p class="text-sm">현재 조회 가능한 항공편이 없습니다</p>
            </div>
          </div>
        </div>

        <!-- 진행중인 스타투어 -->
        <div class="mb-8">
          <div class="flex justify-between items-center mb-4 px-1">
            <h3 class="text-xl font-bold text-gray-900">진행중인 스타투어</h3>
            <button
              v-if="activeConventions.length > 0"
              class="text-sm font-semibold text-blue-600 hover:text-blue-700 transition-colors"
              @click="router.push('/conventions')"
            >
              더보기
            </button>
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
            <p class="text-gray-600 font-medium">
              진행중인 스타투어가 없습니다
            </p>
          </div>

          <div v-else class="overflow-x-auto -mx-4 px-4 scrollbar-hide">
            <div class="flex gap-4 pb-2">
              <div
                v-for="convention in activeConventions.slice(0, 10)"
                :key="convention.id"
                class="flex-shrink-0 w-[280px] bg-white rounded-2xl shadow-md hover:shadow-xl transition-shadow cursor-pointer overflow-hidden group"
                @click="goToConvention(convention)"
              >
                <div class="relative h-[200px] overflow-hidden">
                  <div
                    v-if="convention.conventionImg"
                    class="absolute inset-0 bg-cover bg-center"
                    :style="{
                      backgroundImage: `url(${convention.conventionImg})`,
                    }"
                  ></div>
                  <div
                    v-else
                    class="absolute inset-0 bg-gradient-to-br from-rose-500 via-orange-500 to-amber-500"
                  ></div>
                </div>
                <div class="p-4">
                  <h4
                    class="font-bold text-gray-900 text-lg mb-2 line-clamp-1 group-hover:text-rose-600 transition-colors"
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
          <div class="flex justify-between items-center mb-4 px-1">
            <h3 class="text-xl font-bold text-gray-900">종료된 스타투어</h3>
          </div>
          <div class="overflow-x-auto -mx-4 px-4 scrollbar-hide">
            <div class="flex gap-4 pb-2">
              <div
                v-for="convention in completedConventions.slice(0, 10)"
                :key="convention.id"
                class="flex-shrink-0 w-[280px] bg-white rounded-2xl shadow-md hover:shadow-xl transition-shadow cursor-pointer overflow-hidden group"
                @click="goToConvention(convention)"
              >
                <div class="relative h-[200px] overflow-hidden">
                  <div
                    v-if="convention.conventionImg"
                    class="absolute inset-0 bg-cover bg-center"
                    :style="{
                      backgroundImage: `url(${convention.conventionImg})`,
                    }"
                  ></div>
                  <div
                    v-else
                    class="absolute inset-0 bg-gradient-to-br from-gray-400 via-gray-500 to-gray-600"
                  ></div>
                  <div
                    class="absolute top-3 left-3 px-2 py-1 bg-black/50 text-white text-xs font-medium rounded-md backdrop-blur-sm"
                  >
                    종료됨
                  </div>
                </div>
                <div class="p-4">
                  <h4
                    class="font-bold text-gray-700 text-lg mb-2 line-clamp-1 group-hover:text-gray-900 transition-colors"
                  >
                    {{ convention.title }}
                  </h4>
                  <div class="flex items-center gap-2 text-sm text-gray-400">
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

const conventionsLoading = ref(true)
const conventions = ref([])
const dashboard = ref({})
const isSidebarOpen = ref(false)
const airportFlights = ref([])
const airportLoading = ref(false)

const activeConventions = computed(() =>
  conventions.value.filter((c) => c.completeYn !== 'Y'),
)

const completedConventions = computed(() =>
  conventions.value.filter((c) => c.completeYn === 'Y'),
)

// 여권 만료일 D-day
const passportExpiryDays = computed(() => {
  const expiry = dashboard.value.passport?.passportExpiryDate
  if (!expiry) return null
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const exp = new Date(expiry)
  exp.setHours(0, 0, 0, 0)
  return Math.ceil((exp - today) / (1000 * 60 * 60 * 24))
})

const passportExpiryClass = computed(() => {
  if (passportExpiryDays.value === null) return 'text-gray-700'
  if (passportExpiryDays.value <= 0) return 'text-red-600 font-semibold'
  if (passportExpiryDays.value <= 180) return 'text-yellow-600 font-semibold'
  return 'text-gray-700'
})

async function loadConventions() {
  conventionsLoading.value = true
  try {
    const response = await apiClient.get('/users/conventions')
    conventions.value = response.data
  } catch (error) {
    console.error('행사 목록 로드 실패:', error)
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

async function loadAirportStatus() {
  airportLoading.value = true
  try {
    const today = new Date().toISOString().split('T')[0]
    const now = new Date()
    const fromTime = `${String(now.getHours()).padStart(2, '0')}:00`
    const toTime = `${String(Math.min(now.getHours() + 3, 23)).padStart(2, '0')}:59`

    const res = await apiClient.get('/flight/incheon', {
      params: {
        date: today,
        flightType: 'departure',
        fromTime,
        toTime,
      },
    })
    airportFlights.value = Array.isArray(res.data) ? res.data : []
  } catch (e) {
    console.error('인천공항 조회 실패:', e)
    airportFlights.value = []
  } finally {
    airportLoading.value = false
  }
}

function goToConvention(convention) {
  if (!convention?.id) return
  router.push(`/conventions/${convention.id}`)
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

function getFlightStatusClass(status) {
  if (!status || status === '정상' || status === '출발')
    return 'bg-green-100 text-green-700'
  if (status.includes('지연')) return 'bg-red-100 text-red-700'
  if (status.includes('결항')) return 'bg-gray-100 text-gray-700'
  return 'bg-blue-100 text-blue-700'
}

onMounted(() => {
  loadConventions()
  loadDashboard()
  loadAirportStatus()
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
