<template>
  <div
    v-if="loading"
    class="min-h-screen min-h-dvh flex items-center justify-center"
  >
    <div
      class="inline-block w-8 h-8 border-4 border-t-transparent rounded-full animate-spin"
      :style="{ borderColor: brandColor, borderTopColor: 'transparent' }"
    ></div>
  </div>
  <div
    v-else-if="convention"
    class="min-h-screen min-h-dvh bg-gradient-to-br from-gray-50 to-gray-100"
  >
    <!-- 헤더 배너 -->
    <div class="relative h-48 overflow-hidden" :style="headerGradientStyle">
      <!-- 배경 패턴 -->
      <div class="absolute inset-0 opacity-10">
        <div
          class="absolute top-0 left-0 w-64 h-64 bg-white rounded-full -translate-x-32 -translate-y-32"
        ></div>
        <div
          class="absolute bottom-0 right-0 w-96 h-96 bg-white rounded-full translate-x-48 translate-y-48"
        ></div>
      </div>

      <!-- 공통 헤더 사용 (오버레이) -->
      <div class="absolute top-0 left-0 right-0 z-20">
        <MainHeader title="" :transparent="true" />
      </div>

      <div class="relative h-full flex flex-col justify-center px-6 text-white">
        <h1 class="text-2xl font-bold mb-2">{{ convention.title }}</h1>
        <div class="flex items-center space-x-2 text-sm text-white/90">
          <svg
            class="w-4 h-4"
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
          <span>{{ formattedConventionPeriod }}</span>
        </div>
        <div class="flex items-center space-x-2 text-sm text-white/90 mt-1">
          <svg
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
            />
          </svg>
          <span>{{ convention.location }}</span>
        </div>

        <!-- D-Day 뱃지 -->
        <div
          class="mt-4 inline-flex items-center px-3 py-1.5 bg-white/20 backdrop-blur-sm rounded-full text-sm font-bold"
        >
          <svg
            class="w-4 h-4 mr-1.5"
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
          D-{{ dDay }}
        </div>
      </div>
    </div>

    <!-- HOME_SUB_HEADER 위치: 헤더 배너 바로 아래 -->
    <div v-if="subHeaderActions.length > 0" class="px-4 pt-4">
      <DynamicActionRenderer :features="subHeaderActions" />
    </div>

    <!-- 메인 컨텐츠 -->
    <div class="px-4 pt-10 space-y-6 -mt-8">
      <!-- HOME_CONTENT_TOP 위치: 컨텐츠 영역 상단 -->
      <DynamicActionRenderer
        v-if="contentTopActions.length > 0"
        :features="contentTopActions"
      />

      <!-- 체크리스트 -->
      <ChecklistProgress
        v-if="checklistStatus && checklistStatus.totalItems > 0"
        :checklist="checklistStatus"
      />

      <!-- 공지사항 -->
      <div class="bg-white rounded-2xl shadow-lg p-5">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-bold text-gray-900">공지사항</h2>
          <button
            @click="navigateTo('/notices')"
            class="text-sm font-medium flex items-center"
            :style="{ color: brandColor }"
          >
            더보기
            <svg
              class="w-4 h-4 ml-0.5"
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
          </button>
        </div>

        <div v-if="recentNotices.length > 0" class="space-y-3">
          <div
            v-for="notice in recentNotices"
            :key="notice.id"
            @click="openNotice(notice)"
            class="bg-gradient-to-br from-white to-gray-50 rounded-xl shadow-md hover:shadow-lg transition-all p-4 cursor-pointer border border-gray-100"
          >
            <div class="flex items-start justify-between mb-2">
              <h3 class="font-bold text-gray-900 text-base flex-1 line-clamp-2">
                {{ notice.title }}
              </h3>
              <div class="flex items-center space-x-1 ml-2 flex-shrink-0">
                <span
                  v-if="notice.isPinned"
                  class="px-2 py-0.5 bg-red-100 text-red-700 text-xs font-bold rounded"
                  >필독</span
                >
                <span
                  class="px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-medium rounded"
                  >공지</span
                >
              </div>
            </div>
            <p class="text-sm text-gray-600 line-clamp-2 mb-3">
              {{ notice.content }}
            </p>
            <div
              class="flex items-center justify-between text-xs text-gray-500"
            >
              <span>{{ formatDate(notice.createdAt) }}</span>
              <div class="flex items-center space-x-3">
                <span class="flex items-center space-x-1">
                  <svg
                    class="w-4 h-4"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                    />
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                    />
                  </svg>
                  <span>{{ notice.viewCount || 0 }}</span>
                </span>
              </div>
            </div>
          </div>
        </div>

        <div v-else class="text-center py-8 text-gray-500">
          <svg
            class="w-12 h-12 mx-auto mb-2 opacity-50"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z"
            />
          </svg>
          <p class="text-sm">등록된 공지사항이 없습니다</p>
        </div>
      </div>

      <!-- 나의 일정 -->
      <div class="bg-white rounded-2xl shadow-lg p-5">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-bold text-gray-900">나의 일정</h2>
          <button
            @click="navigateTo('/my-schedule')"
            class="text-sm text-primary-600 font-medium flex items-center"
          >
            전체보기
            <svg
              class="w-4 h-4 ml-0.5"
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
          </button>
        </div>

        <div v-if="upcomingSchedules.length > 0" class="space-y-3">
          <div
            v-for="schedule in upcomingSchedules"
            :key="schedule.id"
            class="flex items-start space-x-3 p-3 bg-gray-50 rounded-xl"
          >
            <div
              class="w-12 h-12 bg-primary-100 rounded-full flex items-center justify-center flex-shrink-0"
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
              <div class="flex items-center space-x-2 mb-1">
                <span
                  class="px-2 py-0.5 bg-primary-600 text-white text-xs font-bold rounded"
                  >{{ schedule.date }}</span
                >
                <span
                  class="px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-medium rounded"
                  >{{ schedule.time }}</span
                >
              </div>
              <h3 class="font-semibold text-gray-900 text-sm">
                {{ schedule.title }}
              </h3>
              <p v-if="schedule.location" class="text-xs text-gray-500 mt-1">
                {{ schedule.location }}
              </p>
            </div>
          </div>
        </div>

        <div v-else class="text-center py-8 text-gray-500">
          <svg
            class="w-12 h-12 mx-auto mb-2 opacity-50"
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
          <p class="text-sm">예정된 일정이 없습니다</p>
        </div>
      </div>
    </div>
  </div>
  <div v-else class="min-h-screen min-h-dvh flex items-center justify-center">
    <div class="text-center">
      <p class="text-gray-600">행사 정보를 불러오지 못했습니다.</p>
      <button
        @click="handleLogout"
        class="mt-4 px-4 py-2 bg-primary-600 text-white rounded-lg"
      >
        로그아웃
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import apiClient from '@/services/api'
import DeadlineCountdown from '@/components/common/DeadlineCountdown.vue'
import ChecklistProgress from '@/components/common/ChecklistProgress.vue'
import DynamicActionRenderer from '@/dynamic-features/DynamicActionRenderer.vue'
import MainHeader from '@/components/common/MainHeader.vue'

const router = useRouter()
const authStore = useAuthStore()
const conventionStore = useConventionStore()

const loading = ref(true)
const convention = computed(() => conventionStore.currentConvention)
const checklistStatus = ref(null) // authStore가 아니라 ref로 변경
const allActions = ref([]) // 전체 동적 액션 저장

// 브랜드 컬러 가져오기
const brandColor = computed(() => {
  return conventionStore.currentConvention?.brandColor || '#10b981' // 기본값: primary-600 green
})

// 헤더 그라데이션 스타일 계산
const headerGradientStyle = computed(() => {
  const color = brandColor.value
  // 16진수 색상을 RGB로 변환
  const r = parseInt(color.slice(1, 3), 16)
  const g = parseInt(color.slice(3, 5), 16)
  const b = parseInt(color.slice(5, 7), 16)

  // 약간 더 어두운 색 계산 (0.8배)
  const darkerR = Math.floor(r * 0.8)
  const darkerG = Math.floor(g * 0.8)
  const darkerB = Math.floor(b * 0.8)

  return {
    background: `linear-gradient(to bottom right, rgb(${r}, ${g}, ${b}), rgb(${darkerR}, ${darkerG}, ${darkerB}))`,
  }
})

// 타겟 위치별 액션 필터링
const subHeaderActions = computed(() =>
  allActions.value.filter(
    (action) => action.targetLocation === 'HOME_SUB_HEADER',
  ),
)
const contentTopActions = computed(() =>
  allActions.value.filter(
    (action) => action.targetLocation === 'HOME_CONTENT_TOP',
  ),
)

const upcomingSchedules = ref([])
const recentNotices = ref([])

const dDay = computed(() => {
  if (!convention.value || !convention.value.startDate) return 0
  const today = new Date()
  const start = new Date(convention.value.startDate)
  const diff = Math.ceil((start - today) / (1000 * 60 * 60 * 24))
  return diff > 0 ? diff : 0
})

function navigateTo(route) {
  router.push(route)
}

function openNotice(notice) {
  // 공지사항 목록 페이지로 이동하면서 noticeId를 state로 전달
  router.push({
    path: '/notices',
    state: { selectedNoticeId: notice.id },
  })
}

function formatDate(dateStr) {
  const date = new Date(dateStr)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

// 날짜를 '2025-11-10(월)' 형식으로 변환
function formatDateWithDay(dateStr) {
  if (!dateStr) return ''

  const date = new Date(dateStr)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  const days = ['일', '월', '화', '수', '목', '금', '토']
  const dayOfWeek = days[date.getDay()]

  return `${year}-${month}-${day}(${dayOfWeek})`
}

// 행사 기간 포맷
const formattedConventionPeriod = computed(() => {
  if (!convention.value) return ''

  const start = formatDateWithDay(convention.value.startDate)
  const end = formatDateWithDay(convention.value.endDate)

  return `${start} ~ ${end}`
})

async function loadTodaySchedules() {
  try {
    const userId = authStore.user?.id
    const conventionId = conventionStore.currentConvention?.id

    if (!userId || !conventionId) {
      console.warn(
        '[loadTodaySchedules] userId or conventionId not available:',
        { userId, conventionId },
      )
      return
    }

    const response = await apiClient.get(
      `/user-schedules/${userId}/${conventionId}`,
    )
    const allSchedules = response.data
    const now = new Date()
    const today = now.toISOString().split('T')[0]

    // 현재 날짜 이후의 일정을 필터링합니다.
    let upcoming = allSchedules.filter((item) => {
      const scheduleDate = item.scheduleDate.split('T')[0]
      return scheduleDate >= today
    })

    // 다가오는 일정이 없으면(행사가 종료된 경우) 전체 일정을 표시합니다.
    if (upcoming.length === 0 && allSchedules.length > 0) {
      upcoming = allSchedules
    }

    upcomingSchedules.value = upcoming
      .sort((a, b) => {
        // 날짜와 시간으로 정렬
        const dateCompare = a.scheduleDate.localeCompare(b.scheduleDate)
        if (dateCompare !== 0) return dateCompare
        return a.startTime.localeCompare(b.startTime)
      })
      .slice(0, 3) // 최대 3개만 표시
      .map((item) => {
        const date = new Date(item.scheduleDate)
        const month = date.getMonth() + 1
        const day = date.getDate()
        return {
          id: item.id,
          date: `${month}/${day}`,
          time: item.startTime,
          title: item.title,
          location: item.location || '장소 미정',
        }
      })
  } catch (error) {
    console.error('Failed to load today schedules:', error)
  }
}

async function loadRecentNotices() {
  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) return

    const response = await apiClient.get('/notices', {
      params: {
        conventionId: conventionId,
        page: 1,
        pageSize: 2, // 최대 2개만
      },
    })

    recentNotices.value = response.data.items || []
  } catch (error) {
    console.error('Failed to load notices:', error)
  }
}

async function loadDynamicActions() {
  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) return

    const response = await apiClient.get(
      `/conventions/${conventionId}/actions/all`,
      {
        params: {
          targetLocation: 'HOME_SUB_HEADER,HOME_CONTENT_TOP',
          isActive: true,
        },
      },
    )

    allActions.value = response.data || []
  } catch (error) {
    console.error('Failed to load dynamic actions:', error)
    allActions.value = []
  }
}

onMounted(async () => {
  loading.value = true

  // 1. 행사 정보 먼저 로드 (필수)
  const selectedConventionId = localStorage.getItem('selectedConventionId')
  if (selectedConventionId && !conventionStore.currentConvention) {
    await conventionStore.setCurrentConvention(parseInt(selectedConventionId))
  }

  // 2. 사용자 정보 로드 (필수)
  if (!authStore.user) {
    await authStore.fetchCurrentUser()
  }

  // 3. 사용자/행사 정보가 모두 로드된 후에 병렬 실행
  await Promise.all([
    loadTodaySchedules(), // ← 이제 userId, conventionId가 준비됨
    loadRecentNotices(),
    loadDynamicActions(),
    loadChecklist(), // 체크리스트도 함수로 분리
  ])

  loading.value = false
})

// 체크리스트 로드 함수 분리
async function loadChecklist() {
  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) {
      console.warn('[loadChecklist] conventionId not available')
      return
    }

    const actionsResponse = await apiClient.get(
      `/conventions/${conventionId}/actions/urgent`,
    )
    const statusesResponse = await apiClient.get(
      `/conventions/${conventionId}/actions/statuses`,
    )
    const actions = actionsResponse.data || []
    const statuses = statusesResponse.data || []

    if (actions.length > 0) {
      const statusMap = new Map(statuses.map((s) => [s.conventionActionId, s]))
      const completedCount = actions.filter(
        (a) => statusMap.get(a.id)?.isComplete,
      ).length

      checklistStatus.value = {
        totalItems: actions.length,
        completedItems: completedCount,
        progressPercentage: Math.round((completedCount / actions.length) * 100),
        items: actions.map((action) => ({
          actionId: action.id,
          title: action.title,
          deadline: action.deadline,
          navigateTo: action.mapsTo,
          isComplete: statusMap.get(action.id)?.isComplete || false,
        })),
      }
    }
  } catch (error) {
    console.error('Failed to load checklist:', error)
    checklistStatus.value = null
  }
}
</script>

<style scoped>
.fade-down-enter-active,
.fade-down-leave-active {
  transition: all 0.2s ease-out;
}
.fade-down-enter-from,
.fade-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
