<template>
  <div v-if="loading" class="min-h-screen min-h-dvh flex items-center justify-center">
    <div
      class="inline-block w-8 h-8 border-4 border-t-transparent rounded-full animate-spin"
      :style="{ borderColor: brandColor, borderTopColor: 'transparent' }"
    ></div>
  </div>

  <div v-else-if="convention" class="min-h-screen min-h-dvh bg-gray-50">
    <!-- 컴팩트 헤더 -->
    <ConventionHeader
      :convention="convention"
      :convention-id="conventionId"
      :d-day="dDay"
      @menu-click="isSidebarOpen = true"
    />

    <!-- GLOBAL_ROOT_POPUP: 팝업 공지 (화면에 보이지 않고 팝업만 트리거) -->
    <DynamicActionRenderer
      v-if="globalPopupActions.length > 0"
      :features="globalPopupActions"
    />

    <!-- 여행 가이드 링크 -->
    <div v-if="hasTravelGuide" class="px-4 pt-4">
      <router-link
        :to="`/conventions/${conventionId}/travel-guide`"
        class="flex items-center gap-3 bg-white rounded-xl shadow-sm p-3 hover:shadow-md transition-shadow"
      >
        <span class="text-lg">🧳</span>
        <span class="text-sm font-medium text-gray-700">여행 가이드</span>
        <span class="text-xs text-gray-400">긴급연락처 · 집결지 · 캘린더</span>
        <span class="ml-auto text-xs font-medium" :style="{ color: brandColor }">→</span>
      </router-link>
    </div>

    <!-- HOME_SUB_HEADER 동적 액션 -->
    <div v-if="subHeaderActions.length > 0" class="px-4 pt-4">
      <DynamicActionRenderer :features="subHeaderActions" />
    </div>

    <!-- HOME_CONTENT_TOP 동적 액션 -->
    <div v-if="contentTopActions.length > 0" class="px-4 pt-4">
      <DynamicActionRenderer :features="contentTopActions" />
    </div>

    <!-- 타임라인 일정 (메인 콘텐츠) -->
    <ScheduleTimeline
      :brand-color="brandColor"
      :is-admin="isAdmin"
      :convention-id="conventionId"
      :attributes="myAttributes"
      :highlight-current="true"
      :dim-past-schedules="true"
      @schedule-click="onScheduleClick"
    />

    <!-- 일정 상세 모달 -->
    <ScheduleDetailModal
      :schedule="selectedSchedule"
      :brand-color="brandColor"
      :is-admin="isAdmin"
      :attributes="myAttributes"
      @close="selectedSchedule = null"
    />

    <!-- 사이드바 메뉴 -->
    <SidebarMenu :is-open="isSidebarOpen" @close="isSidebarOpen = false" />
  </div>

  <div v-else class="min-h-screen min-h-dvh flex items-center justify-center">
    <div class="text-center">
      <p class="text-gray-600">행사 정보를 불러오지 못했습니다.</p>
      <button
        class="mt-4 px-4 py-2 bg-primary-600 text-white rounded-lg"
        @click="handleLogout"
      >
        로그아웃
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import apiClient from '@/services/api'
import ConventionHeader from '@/components/convention/ConventionHeader.vue'
import ScheduleTimeline from '@/components/convention/ScheduleTimeline.vue'
import ScheduleDetailModal from '@/components/convention/ScheduleDetailModal.vue'
import DynamicActionRenderer from '@/dynamic-features/DynamicActionRenderer.vue'
import SidebarMenu from '@/components/common/SidebarMenu.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const conventionStore = useConventionStore()

const loading = ref(true)
const isSidebarOpen = ref(false)
const myInfo = ref(null)
const allActions = ref([])
const hasTravelGuide = ref(false)
const selectedSchedule = ref(null)

const convention = computed(() => conventionStore.currentConvention)
const conventionId = computed(() => {
  const id = route.params.conventionId
  return id ? parseInt(id) : null
})

const brandColor = computed(() => convention.value?.brandColor || '#10b981')

const isAdmin = computed(() => authStore.user?.role === 'Admin')

const dDay = computed(() => {
  if (!convention.value?.startDate) return null
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const start = new Date(convention.value.startDate)
  start.setHours(0, 0, 0, 0)
  const end = convention.value.endDate ? new Date(convention.value.endDate) : start
  end.setHours(23, 59, 59, 999)
  // 행사 종료 후 → null (표시 안 함)
  if (today > end) return null
  // 행사 진행 중 → 0 (D-Day)
  if (today >= start && today <= end) return 0
  // 행사 전 → D-n
  return Math.ceil((start - today) / (1000 * 60 * 60 * 24))
})

const myAttributes = computed(() => myInfo.value?.attributes || [])

// 타겟 위치별 액션 필터링
const subHeaderActions = computed(() =>
  allActions.value.filter((a) => a.targetLocation === 'HOME_SUB_HEADER'),
)
const contentTopActions = computed(() =>
  allActions.value.filter((a) => a.targetLocation === 'HOME_CONTENT_TOP'),
)
const globalPopupActions = computed(() =>
  allActions.value.filter((a) => a.targetLocation === 'GLOBAL_ROOT_POPUP'),
)

function onScheduleClick(schedule) {
  selectedSchedule.value = schedule
}

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
  }
}

async function loadMyInfo() {
  try {
    const id = conventionStore.currentConvention?.id
    if (!id) return
    const response = await apiClient.get(`/users/my-convention-info/${id}`)
    myInfo.value = response.data
  } catch (error) {
    console.error('Failed to load my info:', error)
    myInfo.value = null
  }
}

async function loadDynamicActions() {
  try {
    const id = conventionStore.currentConvention?.id
    if (!id) return

    const [actionsResponse, statusesResponse] = await Promise.all([
      apiClient.get(`/conventions/${id}/actions/all`, {
        params: {
          targetLocation: 'HOME_SUB_HEADER,HOME_CONTENT_TOP,GLOBAL_ROOT_POPUP',
          isActive: true,
        },
      }),
      apiClient.get(`/conventions/${id}/actions/statuses`),
    ])

    const actions = actionsResponse.data || []
    const statuses = statusesResponse.data || []
    const statusMap = new Map(statuses.map((s) => [s.conventionActionId, s]))

    allActions.value = actions.map((action) => ({
      ...action,
      isComplete: statusMap.get(action.id)?.isComplete || false,
    }))
  } catch (error) {
    console.error('Failed to load dynamic actions:', error)
    allActions.value = []
  }
}

async function checkTravelGuide() {
  try {
    const id = conventionStore.currentConvention?.id
    if (!id) return
    const res = await apiClient.get(`/conventions/${id}/travel-guide`)
    const d = res.data
    hasTravelGuide.value = !!(d.emergencyContacts || d.meetingPointInfo || d.location)
  } catch {
    hasTravelGuide.value = false
  }
}

onMounted(async () => {
  loading.value = true

  if (!authStore.user) {
    await authStore.fetchCurrentUser()
  }

  await Promise.all([loadDynamicActions(), loadMyInfo(), checkTravelGuide()])

  loading.value = false
})
</script>
