<template>
  <div class="min-h-screen relative bg-gray-50">
    <!-- Decorative Background Elements -->
    <div class="fixed inset-0 z-0 overflow-hidden pointer-events-none">
      <!-- Large gradient blobs -->
      <div
        class="absolute -top-40 -right-40 w-96 h-96 bg-gradient-to-br from-sky-200/15 to-blue-200/15 rounded-full blur-3xl"
      ></div>
      <div
        class="absolute top-1/3 -left-32 w-80 h-80 bg-gradient-to-br from-blue-200/12 to-cyan-200/12 rounded-full blur-3xl"
      ></div>
      <div
        class="absolute bottom-20 right-1/4 w-64 h-64 bg-gradient-to-br from-cyan-200/12 to-sky-200/12 rounded-full blur-3xl"
      ></div>

      <!-- Subtle pattern overlay -->
      <div
        class="absolute inset-0 opacity-[0.02]"
        style="
          background-image: url('data:image/svg+xml,%3Csvg width=&quot;60&quot; height=&quot;60&quot; viewBox=&quot;0 0 60 60&quot; xmlns=&quot;http://www.w3.org/2000/svg&quot;%3E%3Cg fill=&quot;none&quot; fill-rule=&quot;evenodd&quot;%3E%3Cg fill=&quot;%239C92AC&quot; fill-opacity=&quot;1&quot;%3E%3Cpath d=&quot;M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z&quot;/%3E%3C/g%3E%3C/g%3E%3C/svg%3E');
        "
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
        <!-- 환영 메시지 (Hero Section) -->
        <div
          class="relative overflow-hidden bg-gradient-to-br from-violet-600 via-purple-600 to-fuchsia-600 rounded-2xl shadow-xl p-8 mb-8 text-white"
        >
          <!-- Background decorative elements -->
          <div class="absolute inset-0 overflow-hidden">
            <div
              class="absolute top-1/4 right-1/4 w-64 h-64 bg-gradient-to-br from-pink-200/15 to-violet-200/15 rounded-full blur-3xl"
            ></div>
            <div
              class="absolute bottom-1/3 left-1/4 w-56 h-56 bg-gradient-to-br from-purple-200/12 to-fuchsia-200/12 rounded-full blur-3xl"
            ></div>

            <!-- Subtle dot pattern -->
            <div
              class="absolute inset-0 opacity-[0.03]"
              style="
                background-image: url('data:image/svg+xml,%3Csvg width=&quot;20&quot; height=&quot;20&quot; xmlns=&quot;http://www.w3.org/2000/svg&quot;%3E%3Cg fill=&quot;%23FFFFFF&quot; fill-opacity=&quot;1&quot;%3E%3Ccircle cx=&quot;2&quot; cy=&quot;2&quot; r=&quot;1&quot;/%3E%3C/g%3E%3C/svg%3E');
              "
            ></div>
          </div>

          <div class="relative z-10">
            <h2 class="text-3xl font-bold mb-2">
              안녕하세요,<br />{{ userName }}님!
            </h2>
            <p class="text-lg text-white/90">스타투어에 오신 것을 환영합니다</p>
          </div>

          <!-- Decorative circles -->
          <div
            class="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -mr-16 -mt-16"
          ></div>
          <div
            class="absolute bottom-0 right-8 w-24 h-24 bg-white/10 rounded-full -mb-12"
          ></div>
        </div>

        <!-- 다가오는 일정 D-day -->
        <div v-if="dashboard.preparations?.length > 0" class="mb-6">
          <div
            v-for="prep in dashboard.preparations"
            :key="prep.conventionId"
            class="bg-white rounded-2xl shadow-md overflow-hidden mb-3 cursor-pointer hover:shadow-lg transition-shadow"
            @click="router.push(`/conventions/${prep.conventionId}`)"
          >
            <div
              class="p-5"
              :style="{
                borderLeft: `4px solid ${prep.brandColor || '#6366f1'}`,
              }"
            >
              <div class="flex items-center justify-between mb-3">
                <div>
                  <h3 class="font-bold text-gray-900">{{ prep.title }}</h3>
                  <p v-if="prep.location" class="text-xs text-gray-500 mt-0.5">
                    {{ prep.location }}
                  </p>
                </div>
                <div
                  class="flex-shrink-0 px-3 py-1.5 rounded-full text-sm font-bold text-white"
                  :style="{
                    backgroundColor: prep.brandColor || '#6366f1',
                  }"
                >
                  D-{{ getDDay(prep.startDate) }}
                </div>
              </div>

              <!-- 준비 현황 프로그레스 -->
              <div class="flex gap-3">
                <!-- 여권 -->
                <div
                  class="flex-1 px-3 py-2 rounded-lg text-center text-xs"
                  :class="
                    prep.passport.verified
                      ? 'bg-green-50 text-green-700'
                      : prep.passport.hasNumber
                        ? 'bg-yellow-50 text-yellow-700'
                        : 'bg-red-50 text-red-600'
                  "
                >
                  <span class="block text-base mb-0.5">{{
                    prep.passport.verified
                      ? '✅'
                      : prep.passport.hasNumber
                        ? '⏳'
                        : '❌'
                  }}</span>
                  여권
                </div>
                <!-- 체크리스트 -->
                <div
                  class="flex-1 px-3 py-2 rounded-lg text-center text-xs"
                  :class="
                    prep.checklist.total === 0
                      ? 'bg-gray-50 text-gray-400'
                      : prep.checklist.completed === prep.checklist.total
                        ? 'bg-green-50 text-green-700'
                        : 'bg-yellow-50 text-yellow-700'
                  "
                >
                  <span class="block text-base mb-0.5">{{
                    prep.checklist.total === 0
                      ? '—'
                      : `${prep.checklist.completed}/${prep.checklist.total}`
                  }}</span>
                  체크리스트
                </div>
                <!-- 설문 -->
                <div
                  class="flex-1 px-3 py-2 rounded-lg text-center text-xs"
                  :class="
                    prep.pendingSurveys === 0
                      ? 'bg-green-50 text-green-700'
                      : 'bg-red-50 text-red-600'
                  "
                >
                  <span class="block text-base mb-0.5">{{
                    prep.pendingSurveys === 0 ? '✅' : prep.pendingSurveys
                  }}</span>
                  {{ prep.pendingSurveys === 0 ? '설문 완료' : '미완료 설문' }}
                </div>
                <!-- 공지 -->
                <div
                  v-if="prep.unreadNotices > 0"
                  class="flex-1 px-3 py-2 rounded-lg text-center text-xs bg-blue-50 text-blue-700"
                >
                  <span class="block text-base mb-0.5">{{
                    prep.unreadNotices
                  }}</span>
                  새 공지
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 다가오는 일정 미리보기 -->
        <div v-if="dashboard.upcomingSchedules?.length > 0" class="mb-6">
          <h3 class="text-lg font-bold text-gray-900 mb-3 px-1">
            다가오는 일정
          </h3>
          <div class="space-y-2">
            <div
              v-for="(schedule, i) in dashboard.upcomingSchedules"
              :key="i"
              class="bg-white rounded-xl shadow-sm p-4 flex items-start gap-3"
            >
              <div
                class="flex-shrink-0 w-12 h-12 bg-primary-100 rounded-full flex items-center justify-center"
              >
                <svg
                  class="w-6 h-6 text-primary-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 mb-1">
                  <span
                    class="px-2 py-0.5 bg-primary-600 text-white text-xs font-bold rounded"
                    >{{ formatScheduleDate(schedule.date) }}</span
                  >
                  <span
                    v-if="schedule.time"
                    class="px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-medium rounded"
                    >{{ schedule.time }}</span
                  >
                </div>
                <h4 class="font-semibold text-gray-900 text-sm">
                  {{ schedule.title }}
                </h4>
                <p
                  v-if="schedule.location"
                  class="text-xs text-gray-500 mt-0.5"
                >
                  {{ schedule.location }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- 최신 공지사항 -->
        <div v-if="dashboard.recentNotices?.length > 0" class="mb-6">
          <h3 class="text-lg font-bold text-gray-900 mb-3 px-1">최신 공지</h3>
          <div class="space-y-2">
            <div
              v-for="notice in dashboard.recentNotices"
              :key="notice.id"
              class="bg-white rounded-xl shadow-sm p-4 cursor-pointer hover:shadow-md transition-shadow"
              @click="
                router.push(
                  `/conventions/${notice.conventionId}/board/${notice.id}`,
                )
              "
            >
              <div class="flex items-start justify-between gap-2">
                <div class="flex-1 min-w-0">
                  <div class="flex items-center gap-2 mb-1">
                    <span
                      class="px-1.5 py-0.5 bg-gray-100 text-gray-600 text-xs rounded"
                      >{{ notice.conventionTitle }}</span
                    >
                    <span
                      v-if="notice.isPinned"
                      class="px-1.5 py-0.5 bg-red-100 text-red-700 text-xs font-bold rounded"
                      >필독</span
                    >
                  </div>
                  <h4 class="font-medium text-gray-900 text-sm line-clamp-1">
                    {{ notice.title }}
                  </h4>
                </div>
                <span class="text-xs text-gray-400 flex-shrink-0">{{
                  formatDate(notice.createdAt)
                }}</span>
              </div>
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
            class="bg-white rounded-2xl shadow-md p-8 text-center mx-1"
          >
            <div
              class="w-20 h-20 mx-auto mb-4 bg-gray-100 rounded-full flex items-center justify-center"
            >
              <svg
                class="w-10 h-10 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
                />
              </svg>
            </div>
            <p class="text-gray-600 font-medium">
              진행중인 스타투어가 없습니다
            </p>
          </div>

          <!-- 수평 스크롤 카드 -->
          <div v-else class="overflow-x-auto -mx-4 px-4 scrollbar-hide">
            <div class="flex gap-4 pb-2">
              <div
                v-for="convention in activeConventions.slice(0, 10)"
                :key="convention.id"
                onclick=""
                role="button"
                tabindex="0"
                class="flex-shrink-0 w-[280px] bg-white rounded-2xl shadow-md hover:shadow-xl transition-shadow cursor-pointer overflow-hidden group"
                @click="goToConvention(convention)"
              >
                <!-- 상단 이미지 영역 -->
                <div
                  class="relative h-[200px] overflow-hidden pointer-events-none"
                >
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
                  >
                    <div
                      class="absolute inset-0 flex items-center justify-center"
                    >
                      <svg
                        class="w-20 h-20 text-white/25"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
                        />
                      </svg>
                    </div>
                  </div>
                </div>

                <!-- 카드 정보 -->
                <div class="p-4">
                  <h4
                    class="font-bold text-gray-900 text-lg mb-2 line-clamp-1 group-hover:text-rose-600 transition-colors"
                  >
                    {{ convention.title }}
                  </h4>

                  <!-- 날짜 -->
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
                onclick=""
                role="button"
                tabindex="0"
                class="flex-shrink-0 w-[280px] bg-white rounded-2xl shadow-md hover:shadow-xl transition-shadow cursor-pointer overflow-hidden group"
                @click="goToConvention(convention)"
              >
                <!-- 상단 이미지 영역 -->
                <div
                  class="relative h-[200px] overflow-hidden pointer-events-none"
                >
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
                  >
                    <div
                      class="absolute inset-0 flex items-center justify-center"
                    >
                      <svg
                        class="w-20 h-20 text-white/25"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
                        />
                      </svg>
                    </div>
                  </div>
                  <!-- 종료 뱃지 -->
                  <div
                    class="absolute top-3 left-3 px-2 py-1 bg-black/50 text-white text-xs font-medium rounded-md backdrop-blur-sm"
                  >
                    종료됨
                  </div>
                </div>

                <!-- 카드 정보 -->
                <div class="p-4">
                  <h4
                    class="font-bold text-gray-700 text-lg mb-2 line-clamp-1 group-hover:text-gray-900 transition-colors"
                  >
                    {{ convention.title }}
                  </h4>

                  <!-- 날짜 -->
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

const activeConventions = computed(() =>
  conventions.value.filter((c) => c.completeYn !== 'Y'),
)

const completedConventions = computed(() =>
  conventions.value.filter((c) => c.completeYn === 'Y'),
)

// 행사 목록 로드
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

// 네비게이션
function goToConvention(convention) {
  if (!convention?.id) {
    console.warn('goToConvention: invalid convention', convention)
    return
  }
  router.push(`/conventions/${convention.id}`)
}

// 날짜 포맷
function formatDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('ko-KR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  })
}

function formatScheduleDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  return `${date.getMonth() + 1}/${date.getDate()}`
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

async function loadDashboard() {
  try {
    const res = await apiClient.get('/users/home-dashboard')
    dashboard.value = res.data
  } catch (e) {
    console.error('대시보드 로드 실패:', e)
  }
}

onMounted(() => {
  loadConventions()
  loadDashboard()
})
</script>

<style scoped>
/* 수평 스크롤바 숨기기 */
.scrollbar-hide {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
.scrollbar-hide::-webkit-scrollbar {
  display: none;
}
</style>
