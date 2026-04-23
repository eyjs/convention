<template>
  <!-- 페이지 초기화/데이터 로딩 중: 렌더링 과정 은닉 -->
  <div
    v-if="!pageReady"
    class="min-h-screen min-h-dvh flex items-center justify-center bg-gray-50"
  >
    <div
      class="inline-block w-8 h-8 border-4 border-t-transparent rounded-full animate-spin"
      :style="{ borderColor: brandColor, borderTopColor: 'transparent' }"
    ></div>
  </div>
  <div v-else class="min-h-screen min-h-dvh bg-gray-50">
    <!-- 공통 헤더 -->
    <MainHeader title="나의 일정" :show-back="true">
      <template #actions>
        <button
          class="p-2 hover:bg-gray-100 rounded-lg"
          @click="toggleCalendarView"
        >
          <svg
            v-if="!isCalendarViewActive"
            class="w-6 h-6 text-gray-600"
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
          <svg
            v-else
            class="w-6 h-6 text-gray-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M20 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 14H4V6h16v12zM6 8h4v2H6V8z"
            />
          </svg>
        </button>
      </template>
    </MainHeader>

    <!-- 타임라인 컴포넌트 -->
    <ScheduleTimeline
      ref="timelineRef"
      :brand-color="brandColor"
      :is-admin="isAdmin"
      :convention-id="conventionId"
      :highlight-current="true"
      :dim-past-schedules="false"
      @schedule-click="openScheduleDetail"
    />

    <!-- 일정 상세 모달 -->
    <ScheduleDetailModal
      :schedule="selectedSchedule"
      :brand-color="brandColor"
      :is-admin="isAdmin"
      @close="closeScheduleDetail"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import MainHeader from '@/components/common/MainHeader.vue'
import ScheduleTimeline from '@/components/convention/ScheduleTimeline.vue'
import ScheduleDetailModal from '@/components/convention/ScheduleDetailModal.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const conventionStore = useConventionStore()

const pageReady = ref(false)
const selectedSchedule = ref(null)
const timelineRef = ref(null)

// 캘린더 뷰 상태 (헤더 아이콘 연동을 위해 미러링)
const isCalendarViewActive = computed(
  () => timelineRef.value?.showCalendarView ?? false,
)

const conventionId = computed(() => conventionStore.currentConvention?.id)

const brandColor = computed(
  () => conventionStore.currentConvention?.brandColor || '#10b981',
)

const isAdmin = computed(() => authStore.user?.role === 'Admin')

function toggleCalendarView() {
  if (timelineRef.value) {
    timelineRef.value.showCalendarView = !timelineRef.value.showCalendarView
  }
}

function openScheduleDetail(schedule) {
  selectedSchedule.value = schedule
}

function closeScheduleDetail() {
  selectedSchedule.value = null
}


onMounted(async () => {
  if (!authStore.user) {
    await authStore.fetchCurrentUser()
  }
  if (!conventionStore.currentConvention) {
    await conventionStore.selectConvention(parseInt(route.params.conventionId))
  }
  pageReady.value = true
})
</script>
