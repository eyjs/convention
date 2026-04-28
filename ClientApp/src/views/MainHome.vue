<template>
  <div class="app-frame">
    <div class="safe-top bg-gray-50"></div>
    <div class="safe-content relative bg-gray-50">
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
              loading="lazy"
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
          <!-- 캐러셀 배너 -->
          <HomeCarousel
            v-if="banners.length > 0"
            :banners="banners"
            class="mb-4"
          />

          <!-- 여권 정보 카드 (해외 행사만, 승인 완료 시 숨김) -->
          <div
            v-if="showPassportCard"
            class="bg-white rounded-xl shadow-sm mb-4 px-4 py-3 cursor-pointer hover:shadow-md transition-shadow"
            :class="passportCardBorder"
            @click="router.push('/my-profile')"
          >
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2">
                <span class="text-sm font-semibold text-gray-800"
                  >여권 정보</span
                >
                <span
                  class="px-2 py-0.5 text-xs font-medium rounded"
                  :class="passportStatusBadgeClass"
                >
                  {{ passportStatusLabel }}
                </span>
              </div>
              <span class="text-xs text-gray-400">입력하러 가기 ›</span>
            </div>
            <p class="text-xs text-gray-600 mt-1.5">
              {{ passportStatusMessage }}
            </p>
            <ul
              v-if="missingPassportFields.length > 0"
              class="mt-2 space-y-0.5"
            >
              <li
                v-for="item in missingPassportFields"
                :key="item"
                class="text-xs text-gray-500 flex items-center gap-1"
              >
                <span class="text-red-400">•</span>
                {{ item }}
              </li>
            </ul>
          </div>

          <!-- 진행중인 스타투어 -->
          <div v-if="!isLoading" class="mb-6">
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
                  class="flex-shrink-0 w-[240px] bg-white rounded-xl shadow-sm hover:shadow-md transition-all cursor-pointer overflow-hidden group"
                  @click="goToConvention(convention)"
                >
                  <div class="relative h-[130px] overflow-hidden">
                    <div
                      v-if="convention.conventionImg"
                      class="absolute inset-0 bg-cover bg-center group-hover:scale-105 transition-transform duration-500"
                      :style="{
                        backgroundImage: `url(${convention.conventionImg})`,
                      }"
                    ></div>
                    <div
                      v-else
                      class="absolute inset-0"
                      :style="getGradientStyle(convention.brandColor)"
                    ></div>
                    <div
                      v-if="getDDay(convention.startDate) > 0"
                      class="absolute top-2 right-2 px-2 py-0.5 bg-black/50 backdrop-blur-sm text-white text-xs font-bold rounded-full"
                    >
                      D-{{ getDDay(convention.startDate) }}
                    </div>
                    <div
                      class="absolute top-2 left-2 px-1.5 py-0.5 text-xs font-semibold rounded-full shadow"
                      :class="
                        isConventionOverseas(convention)
                          ? 'bg-sky-500 text-white'
                          : 'bg-emerald-500 text-white'
                      "
                    >
                      {{ isConventionOverseas(convention) ? '해외' : '국내' }}
                    </div>
                  </div>
                  <div class="p-3">
                    <h4
                      class="font-semibold text-gray-900 text-sm mb-0.5 line-clamp-1 group-hover:text-rose-600 transition-colors"
                    >
                      {{ convention.title }}
                    </h4>
                    <div
                      class="flex items-center gap-1.5 text-xs text-gray-500"
                    >
                      <svg
                        class="w-3.5 h-3.5 flex-shrink-0"
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
                    <div class="absolute top-2 left-2 flex items-center gap-1">
                      <span
                        class="px-1.5 py-0.5 bg-black/50 text-white text-xs rounded backdrop-blur-sm"
                      >
                        종료
                      </span>
                      <span
                        class="px-1.5 py-0.5 text-xs font-semibold rounded backdrop-blur-sm"
                        :class="
                          isConventionOverseas(convention)
                            ? 'bg-sky-500/90 text-white'
                            : 'bg-emerald-500/90 text-white'
                        "
                      >
                        {{ isConventionOverseas(convention) ? '해외' : '국내' }}
                      </span>
                    </div>
                  </div>
                  <div class="p-3">
                    <h4
                      class="font-semibold text-gray-600 text-sm line-clamp-1"
                    >
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
    <div class="safe-bottom bg-gray-50"></div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import SidebarMenu from '@/components/common/SidebarMenu.vue'
import HomeCarousel from '@/components/common/HomeCarousel.vue'
import apiClient from '@/services/api'

const router = useRouter()
const authStore = useAuthStore()

const userName = computed(() => authStore.user?.name || '사용자')

const conventions = ref([])
const dashboard = ref({})
const banners = ref([])
const isSidebarOpen = ref(false)
const isLoading = ref(true)

// 국내/해외 판정 — conventionType SSOT
function isConventionOverseas(c) {
  return c?.conventionType === 'OVERSEAS'
}

// 행사 종료 판정: completeYn='Y' 또는 endDate가 오늘 이전
function isConventionEnded(c) {
  if (c.completeYn === 'Y') return true
  if (!c.endDate) return false
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const end = new Date(c.endDate)
  end.setHours(0, 0, 0, 0)
  return end < today
}

const activeConventions = computed(() =>
  conventions.value
    .filter((c) => !isConventionEnded(c))
    .sort((a, b) => new Date(a.startDate) - new Date(b.startDate)),
)
const completedConventions = computed(() =>
  conventions.value.filter((c) => isConventionEnded(c)),
)

// 여권 정보 점검 — 국내 행사 제외, 승인 완료 시 숨김
const hasOverseasConvention = computed(() =>
  activeConventions.value.some((c) => c.conventionType === 'OVERSEAS'),
)

const passportFieldChecks = computed(() => {
  const p = dashboard.value.passport || {}
  return [
    { key: 'passportNumber', label: '여권번호 미입력', ok: !!p.passportNumber },
    {
      key: 'passportName',
      label: '영문명(First/Last) 미입력',
      ok: !!(p.firstName && p.lastName),
    },
    {
      key: 'passportExpiry',
      label: '여권 만료일 미입력',
      ok: !!p.passportExpiryDate,
    },
    {
      key: 'passportImage',
      label: '여권 사본 미업로드',
      ok: !!p.passportImageUrl,
    },
  ]
})

const missingPassportFields = computed(() =>
  passportFieldChecks.value.filter((c) => !c.ok).map((c) => c.label),
)

const isPassportVerified = computed(
  () => !!(dashboard.value.passport && dashboard.value.passport.verified),
)

const allPassportFieldsFilled = computed(
  () => missingPassportFields.value.length === 0,
)

// 카드 표시 여부: 해외 행사 있음 && 승인 완료 아님
const showPassportCard = computed(
  () => hasOverseasConvention.value && !isPassportVerified.value,
)

// 상태별 메시지
const passportStatus = computed(() => {
  if (!allPassportFieldsFilled.value) {
    const total = passportFieldChecks.value.length
    const filled = total - missingPassportFields.value.length
    if (filled === 0) {
      return {
        label: '미등록',
        message: '여권 정보가 등록되지 않았습니다. 등록해 주세요.',
        badgeClass: 'bg-red-50 text-red-600',
        borderClass: 'border-l-4 border-red-400',
      }
    }
    return {
      label: '입력 필요',
      message: '여권 정보 입력이 필요합니다.',
      badgeClass: 'bg-amber-50 text-amber-700',
      borderClass: 'border-l-4 border-amber-400',
    }
  }
  return {
    label: '승인 대기',
    message: '여권 정보 승인 대기 중입니다.',
    badgeClass: 'bg-blue-50 text-blue-600',
    borderClass: 'border-l-4 border-blue-400',
  }
})

const passportStatusLabel = computed(() => passportStatus.value.label)
const passportStatusMessage = computed(() => passportStatus.value.message)
const passportStatusBadgeClass = computed(() => passportStatus.value.badgeClass)
const passportCardBorder = computed(() => passportStatus.value.borderClass)

function getGradientStyle(color) {
  const c = color || '#6366f1'
  const r = parseInt(c.slice(1, 3), 16)
  const g = parseInt(c.slice(3, 5), 16)
  const b = parseInt(c.slice(5, 7), 16)
  const dr = Math.floor(r * 0.7)
  const dg = Math.floor(g * 0.7)
  const db = Math.floor(b * 0.7)
  return {
    background: `linear-gradient(135deg, rgb(${r},${g},${b}), rgb(${dr},${dg},${db}))`,
  }
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

async function loadConventions() {
  try {
    const res = await apiClient.get('/users/conventions')
    conventions.value = res.data

    // 진행중 행사가 1개뿐이면 바로 이동 (최초 로그인 시에만)
    const active = conventions.value.filter((c) => !isConventionEnded(c))
    if (active.length === 1 && sessionStorage.getItem('justLoggedIn')) {
      sessionStorage.removeItem('justLoggedIn')
      router.replace(`/conventions/${active[0].id}`)
      return
    }
    sessionStorage.removeItem('justLoggedIn')
  } catch {
    conventions.value = []
  } finally {
    isLoading.value = false
  }
}

async function loadBanners() {
  try {
    const res = await apiClient.get('/home-banners')
    banners.value = res.data || []
  } catch {
    banners.value = []
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
  loadBanners()
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

.app-frame {
  display: flex;
  flex-direction: column;
  height: 100vh;
  height: 100dvh;
  overflow: hidden;
}
.safe-top {
  flex-shrink: 0;
  height: env(safe-area-inset-top, 0px);
}
.safe-content {
  flex: 1;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
}
.safe-bottom {
  height: env(safe-area-inset-bottom, 0px);
  flex-shrink: 0;
}
:global(.capacitor-app) .safe-top {
  height: max(env(safe-area-inset-top, 0px), 2rem);
}
:global(.capacitor-app) .safe-bottom {
  height: max(env(safe-area-inset-bottom, 0px), 3rem);
}
</style>
