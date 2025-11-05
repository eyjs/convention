<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <!-- 공통 헤더 사용 -->
    <MainHeader title="나의 일정" :show-back="true">
      <template #actions>
        <button
          @click="showCalendarView = !showCalendarView"
          class="p-2 hover:bg-gray-100 rounded-lg"
        >
          <svg
            v-if="!showCalendarView"
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
              d="M4 6h16M4 12h16M4 18h16"
            />
          </svg>
        </button>
      </template>
    </MainHeader>

    <!-- 날짜 선택 스크롤 -->
    <div v-if="!showCalendarView" class="bg-white border-b">
      <div class="overflow-x-auto scrollbar-hide">
        <div class="flex px-4 py-3 space-x-2 min-w-max">
          <button
            v-for="date in dates"
            :key="date.date"
            @click="selectedDate = date.date"
            :class="[
              'flex-shrink-0 px-3 py-2 rounded-xl text-center transition-all',
              selectedDate === date.date
                ? 'text-white shadow-lg scale-105'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200',
            ]"
            :style="
              selectedDate === date.date
                ? {
                    backgroundColor: brandColor,
                  }
                : {}
            "
          >
            <div class="text-xs font-medium mb-0.5">{{ date.day }}</div>
            <div class="text-sm font-bold">
              {{ date.month }}/{{ date.dayNum }}
            </div>
          </button>
        </div>
      </div>
    </div>

    <!-- SCHEDULE_CONTENT_TOP 위치: 날짜 선택 스크롤 아래 -->
    <div v-if="contentTopActions.length > 0" class="px-4 pt-4">
      <DynamicActionRenderer :features="contentTopActions" />
    </div>

    <!-- 일정 리스트 -->
    <div v-if="!showCalendarView" class="px-4 py-6 space-y-4">
      <!-- 날짜별 그룹 -->
      <div
        v-for="dateGroup in groupedSchedules"
        :key="dateGroup.date"
        class="space-y-3"
      >
        <div class="flex items-center justify-between px-2">
          <h2 class="text-sm font-bold text-gray-900">
            {{ formatDateHeader(dateGroup.date) }}
          </h2>
          <span class="text-xs text-gray-500"
            >{{ dateGroup.schedules.length }}개 일정</span
          >
        </div>

        <!-- 일정 카드 -->
        <div
          v-for="schedule in dateGroup.schedules"
          :key="schedule.id"
          :ref="
            (el) => {
              if (currentSchedule?.id === schedule.id) currentScheduleRef = el
            }
          "
          @click="openScheduleDetail(schedule)"
          :class="[
            'rounded-xl shadow-sm hover:shadow-md transition-all p-4 cursor-pointer border-l-4',
            currentSchedule?.id === schedule.id ? 'ring-2' : 'bg-white',
          ]"
          :style="
            currentSchedule?.id === schedule.id
              ? {
                  backgroundColor: brandColor + '10',
                  borderLeftColor: brandColor,
                  '--tw-ring-color': brandColor + '50',
                }
              : {
                  borderLeftColor: '#e5e7eb',
                }
          "
        >
          <!-- 시간 & 타이틀 -->
          <div class="flex items-start space-x-3">
            <div class="w-16 flex-shrink-0">
              <div
                class="px-2 py-1 rounded-lg text-center"
                :style="
                  currentSchedule?.id === schedule.id
                    ? {
                        backgroundColor: brandColor,
                        color: '#ffffff',
                      }
                    : {
                        backgroundColor: brandColor + '20',
                        color: brandColor,
                      }
                "
              >
                <div class="text-xs font-bold">{{ schedule.startTime }}</div>
                <div v-if="schedule.endTime" class="text-xs opacity-70">
                  {{ schedule.endTime }}
                </div>
              </div>
            </div>

            <div class="flex-1 min-w-0">
              <div class="flex items-start justify-between mb-2">
                <h3 class="font-bold text-gray-900 text-base">
                  {{ schedule.title }}
                </h3>
                <span
                  v-if="schedule.category"
                  class="px-2 py-1 bg-blue-100 text-blue-700 rounded text-xs font-medium ml-2 flex-shrink-0"
                >
                  {{ schedule.category }}
                </span>
              </div>

              <!-- 장소 -->
              <div
                v-if="schedule.location"
                class="flex items-center space-x-1 text-sm text-gray-600 mb-2"
              >
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
                    d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z"
                  />
                </svg>
                <span>{{ schedule.location }}</span>
              </div>

              <!-- 설명 -->
              <p
                v-if="schedule.description"
                class="text-sm text-gray-600 line-clamp-2"
              >
                {{ schedule.description }}
              </p>

              <!-- 참가자/그룹 -->
              <div
                v-if="schedule.group"
                class="flex items-center space-x-2 mt-3"
              >
                <div
                  class="flex items-center space-x-1 px-2 py-1 bg-gray-100 rounded-full text-xs"
                >
                  <svg
                    class="w-3 h-3 text-gray-600"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
                    />
                  </svg>
                  <span class="text-gray-700 font-medium">{{
                    schedule.group
                  }}</span>
                  <span v-if="schedule.participants" class="text-gray-500">
                    ({{ schedule.participants }}명)
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 일정 없음 -->
        <div
          v-if="dateGroup.schedules.length === 0"
          class="text-center py-8 text-gray-500"
        >
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
          <p class="text-sm">이 날은 예정된 일정이 없습니다</p>
        </div>
      </div>
    </div>

    <!-- 캘린더 뷰 -->
    <div v-else class="px-4 py-6">
      <div class="bg-white rounded-xl shadow-sm p-4">
        <!-- 캘린더 헤더: 월/년 표시 및 네비게이션 -->
        <div class="flex items-center justify-between mb-6">
          <button
            @click="changeMonth(-1)"
            class="p-2 hover:bg-gray-100 rounded-lg transition-all"
          >
            <svg
              class="w-5 h-5 text-gray-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M15 19l-7-7 7-7"
              />
            </svg>
          </button>

          <h3 class="text-lg font-bold text-gray-900">
            {{ currentCalendarYear }}년 {{ currentCalendarMonth + 1 }}월
          </h3>

          <button
            @click="changeMonth(1)"
            class="p-2 hover:bg-gray-100 rounded-lg transition-all"
          >
            <svg
              class="w-5 h-5 text-gray-600"
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

        <div class="grid grid-cols-7 gap-2 mb-4">
          <div
            v-for="day in ['일', '월', '화', '수', '목', '금', '토']"
            :key="day"
            class="text-center text-xs font-bold text-gray-500 py-2"
          >
            {{ day }}
          </div>
        </div>
        <div class="grid grid-cols-7 gap-2">
          <div
            v-for="day in calendarDays"
            :key="day.date"
            @click="selectCalendarDay(day)"
            :class="[
              'aspect-square flex flex-col items-center justify-center rounded-lg cursor-pointer transition-all',
              day.isToday ? 'border-2' : '',
              day.hasSchedule && !day.isToday
                ? 'bg-blue-50'
                : day.isToday
                  ? ''
                  : 'hover:bg-gray-50',
              !day.isCurrentMonth ? 'opacity-30' : '',
            ]"
            :style="
              day.isToday
                ? {
                    backgroundColor: brandColor + '20',
                    borderColor: brandColor,
                  }
                : {}
            "
          >
            <span
              :class="[
                'text-sm font-medium',
                day.isToday ? 'font-bold' : 'text-gray-700',
              ]"
              :style="day.isToday ? { color: brandColor } : {}"
            >
              {{ day.day }}
            </span>
            <div
              v-if="day.scheduleCount > 0"
              class="flex items-center justify-center mt-1"
            >
              <div
                class="w-1 h-1 rounded-full"
                :style="{ backgroundColor: brandColor }"
              ></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 참석자 목록 모달 -->
    <ParticipantList
      v-if="showParticipantsModal"
      :participants="participants"
      :title="selectedGroupName"
      :brand-color="brandColor"
      :show-phone="isAdmin"
      :search-placeholder="
        isAdmin ? '이름, 소속, 연락처로 검색...' : '이름, 소속으로 검색...'
      "
      z-index-class="z-[60]"
      @close="closeParticipantsModal"
    />

    <!-- 일정 상세 모달 -->
    <Transition name="modal-slide">
      <div
        v-if="selectedSchedule"
        @click="closeScheduleDetail"
        class="fixed inset-0 bg-black/50 z-50 flex items-end sm:items-center justify-center p-4"
      >
        <div
          @click.stop
          class="bg-white rounded-t-3xl sm:rounded-2xl w-full sm:max-w-lg max-h-[90vh] overflow-y-auto"
        >
          <!-- 모달 헤더 -->
          <div
            class="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between"
          >
            <h2 class="text-lg font-bold text-gray-900">일정 상세</h2>
            <button
              @click="closeScheduleDetail"
              class="p-2 hover:bg-gray-100 rounded-lg"
            >
              <svg
                class="w-5 h-5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>
          </div>

          <!-- 모달 내용 -->
          <div class="p-6 space-y-4">
            <!-- 카테고리 & 시간 -->
            <div class="flex items-center justify-between">
              <span
                v-if="selectedSchedule.category"
                class="px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-sm font-medium"
              >
                {{ selectedSchedule.category }}
              </span>
              <span
                class="px-3 py-1 rounded-full text-sm font-medium text-white"
                :style="{ backgroundColor: brandColor }"
              >
                {{ selectedSchedule.startTime }} ~
                {{ selectedSchedule.endTime || '' }}
              </span>
            </div>

            <!-- 타이틀 -->
            <h3 class="text-2xl font-bold text-gray-900">
              {{ selectedSchedule.title }}
            </h3>

            <!-- 날짜 정보 -->
            <div class="flex items-center space-x-2 text-sm text-gray-600">
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
              <span>{{ formatDate(selectedSchedule.date) }}</span>
            </div>

            <!-- 장소 정보 -->
            <div
              v-if="selectedSchedule.location"
              class="flex items-start space-x-3 p-4 bg-gray-50 rounded-xl"
            >
              <svg
                class="w-6 h-6 text-gray-600 flex-shrink-0 mt-0.5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z"
                />
              </svg>
              <div>
                <div class="font-semibold text-gray-900">장소</div>
                <div class="text-gray-600">{{ selectedSchedule.location }}</div>
              </div>
            </div>

            <!-- 상세 설명 (메인 컨텐츠) -->
            <div v-if="selectedSchedule.description" class="py-4">
              <h4 class="text-lg font-bold text-gray-900 mb-3">상세 설명</h4>
              <p
                class="text-gray-700 whitespace-pre-wrap leading-relaxed text-base"
              >
                {{ selectedSchedule.description }}
              </p>
            </div>

            <!-- 참여 그룹 & 참석자 보기 (편의 옵션) -->
            <div v-if="selectedSchedule.group" class="pt-4 border-t mt-6">
              <div class="p-4 bg-gray-50 rounded-xl">
                <div class="flex items-center space-x-2 mb-3">
                  <svg
                    class="w-5 h-5 text-gray-600"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
                    />
                  </svg>
                  <span class="text-sm font-medium text-gray-700">{{
                    selectedSchedule.group
                  }}</span>
                  <span class="text-sm text-gray-500"
                    >({{ selectedSchedule.participants }}명)</span
                  >
                </div>
                <button
                  @click="
                    loadParticipants(
                      selectedSchedule.scheduleTemplateId,
                      selectedSchedule.group,
                    )
                  "
                  class="w-full px-4 py-2.5 rounded-lg text-white font-medium transition-all hover:opacity-90 flex items-center justify-center space-x-2"
                  :style="{ backgroundColor: brandColor }"
                >
                  <svg
                    class="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
                    />
                  </svg>
                  <span>참석자 보기</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import apiClient from '@/services/api'
import DynamicActionRenderer from '@/dynamic-features/DynamicActionRenderer.vue'
import MainHeader from '@/components/common/MainHeader.vue'
import ParticipantList from '@/components/ParticipantList.vue'
import dayjs from 'dayjs'

const authStore = useAuthStore()
const conventionStore = useConventionStore()

const showCalendarView = ref(false)
const selectedDate = ref('')
const selectedSchedule = ref(null)
const allSchedules = ref([]) // 전체 일정 저장
const allActions = ref([]) // 전체 동적 액션 저장
const currentScheduleRef = ref(null) // 현재 일정 카드 ref
const currentScheduleId = ref(null) // 현재 진행 중인 일정 ID
const showParticipantsModal = ref(false) // 참석자 목록 모달
const participants = ref([]) // 참석자 목록
const selectedGroupName = ref('') // 선택된 그룹명

const schedules = computed(() => {
  if (!selectedDate.value) return allSchedules.value
  return allSchedules.value.filter((s) => s.date === selectedDate.value)
})

// 브랜드 컬러 가져오기
const brandColor = computed(() => {
  return conventionStore.currentConvention?.brandColor || '#10b981' // 기본값: primary-600 green
})

// 어드민 권한 확인
const isAdmin = computed(() => {
  return authStore.user?.role === 'Admin'
})

// 현재 시간 기준 진행 중인 일정 계산
const currentSchedule = computed(() => {
  const today = dayjs().format('YYYY-MM-DD')

  // 선택된 날짜가 오늘이 아니면 null
  if (selectedDate.value !== today) {
    return null
  }

  const now = dayjs()
  const todaySchedules = schedules.value
    .filter((s) => s.date === today)
    .sort((a, b) => a.startTime.localeCompare(b.startTime))

  if (todaySchedules.length === 0) return null

  // 현재 시간 이전의 가장 마지막 일정 찾기
  let current = null
  for (const schedule of todaySchedules) {
    const scheduleDateTime = dayjs(`${schedule.date} ${schedule.startTime}`)

    if (scheduleDateTime.isBefore(now) || scheduleDateTime.isSame(now)) {
      current = schedule
    } else {
      break
    }
  }

  // 없으면 첫 번째 일정
  return current || todaySchedules[0]
})

// SCHEDULE_CONTENT_TOP 위치 액션 필터링
const contentTopActions = computed(() =>
  allActions.value.filter(
    (action) => action.targetLocation === 'SCHEDULE_CONTENT_TOP',
  ),
)

// 날짜 목록 생성
const dates = computed(() => {
  if (allSchedules.value.length === 0) return []

  // 일정에서 고유 날짜 추출
  const uniqueDates = [...new Set(allSchedules.value.map((s) => s.date))].sort()

  return uniqueDates.map((dateStr) => {
    const date = parseLocalDate(dateStr)
    const days = ['일', '월', '화', '수', '목', '금', '토']
    return {
      date: dateStr,
      day: days[date.getDay()],
      dayNum: String(date.getDate()),
      month: `${date.getMonth() + 1}`,
    }
  })
})

// 날짜별 일정 그룹화
const groupedSchedules = computed(() => {
  const grouped = {}
  schedules.value.forEach((schedule) => {
    if (!grouped[schedule.date]) {
      grouped[schedule.date] = []
    }
    grouped[schedule.date].push(schedule)
  })

  return Object.keys(grouped).map((date) => ({
    date,
    schedules: grouped[date].sort((a, b) =>
      a.startTime.localeCompare(b.startTime),
    ),
  }))
})
// 캘린더 현재 월/년 상태
const currentCalendarYear = ref(new Date().getFullYear())
const currentCalendarMonth = ref(new Date().getMonth())

// 캘린더 날짜 생성
const calendarDays = computed(() => {
  const days = []
  const today = new Date()
  const todayStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`

  const currentMonth = currentCalendarMonth.value
  const currentYear = currentCalendarYear.value

  const firstDay = new Date(currentYear, currentMonth, 1)
  const lastDay = new Date(currentYear, currentMonth + 1, 0)
  const startDay = firstDay.getDay()

  // 이전 달 날짜
  for (let i = startDay - 1; i >= 0; i--) {
    const date = new Date(currentYear, currentMonth, -i)
    const dateStr = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
    const daySchedules = allSchedules.value.filter((s) => s.date === dateStr)
    days.push({
      date: dateStr,
      day: date.getDate(),
      isCurrentMonth: false,
      isToday: dateStr === todayStr,
      hasSchedule: daySchedules.length > 0,
      scheduleCount: daySchedules.length,
    })
  }

  // 현재 달 날짜
  for (let i = 1; i <= lastDay.getDate(); i++) {
    const date = new Date(currentYear, currentMonth, i)
    const dateStr = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
    const daySchedules = allSchedules.value.filter((s) => s.date === dateStr)

    days.push({
      date: dateStr,
      day: i,
      isCurrentMonth: true,
      isToday: dateStr === todayStr,
      hasSchedule: daySchedules.length > 0,
      scheduleCount: daySchedules.length,
    })
  }

  return days
})

// 날짜 문자열을 로컬 Date 객체로 변환 (타임존 이슈 방지)
function parseLocalDate(dateStr) {
  const [year, month, day] = dateStr.split('-').map(Number)
  return new Date(year, month - 1, day)
}

function formatDateHeader(dateStr) {
  const date = parseLocalDate(dateStr)
  const days = ['일', '월', '화', '수', '목', '금', '토']
  return `${date.getMonth() + 1}월 ${date.getDate()}일 (${days[date.getDay()]})`
}

function formatDate(dateStr) {
  const date = parseLocalDate(dateStr)
  const days = [
    '일요일',
    '월요일',
    '화요일',
    '수요일',
    '목요일',
    '금요일',
    '토요일',
  ]
  return `${date.getFullYear()}년 ${date.getMonth() + 1}월 ${date.getDate()}일 ${days[date.getDay()]}`
}

function openScheduleDetail(schedule) {
  selectedSchedule.value = schedule
}

function closeScheduleDetail() {
  selectedSchedule.value = null
}

// 참석자 목록 조회
async function loadParticipants(scheduleTemplateId, groupName) {
  selectedGroupName.value = groupName || '참여 그룹'
  showParticipantsModal.value = true

  try {
    const response = await apiClient.get(
      `/user-schedules/participants/${scheduleTemplateId}`,
    )
    participants.value = response.data.participants || []
  } catch (error) {
    console.error('Failed to load participants:', error)
    participants.value = []
  }
}

// 참석자 모달 닫기
function closeParticipantsModal() {
  showParticipantsModal.value = false
  participants.value = []
  selectedGroupName.value = ''
}

function selectCalendarDay(day) {
  selectedDate.value = day.date
  showCalendarView.value = false
}

function changeMonth(direction) {
  currentCalendarMonth.value += direction

  if (currentCalendarMonth.value < 0) {
    currentCalendarMonth.value = 11
    currentCalendarYear.value -= 1
  } else if (currentCalendarMonth.value > 11) {
    currentCalendarMonth.value = 0
    currentCalendarYear.value += 1
  }
}

// 현재 일정으로 자동 스크롤
watch(
  currentSchedule,
  async (newSchedule) => {
    if (newSchedule && !showCalendarView.value) {
      await nextTick()
      if (currentScheduleRef.value) {
        currentScheduleRef.value.scrollIntoView({
          behavior: 'smooth',
          block: 'center',
        })
      }
    }
  },
  { immediate: true },
)

async function loadDynamicActions() {
  try {
    const conventionId = conventionStore.currentConvention?.id

    if (!conventionId) return

    const response = await apiClient.get(
      `/conventions/${conventionId}/actions/all`,
      {
        params: {
          targetLocation: 'SCHEDULE_CONTENT_TOP',
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

// API에서 일정 불러오기
onMounted(async () => {
  try {
    // 1. Ensure stores are ready
    if (!authStore.user) {
      await authStore.fetchCurrentUser()
    }
    if (!conventionStore.currentConvention) {
      const selectedConventionId = localStorage.getItem('selectedConventionId')
      if (selectedConventionId) {
        await conventionStore.setCurrentConvention(
          parseInt(selectedConventionId),
        )
      }
    }

    // 2. Get IDs from stores
    const userId = authStore.user?.id
    const conventionId = conventionStore.currentConvention?.id

    if (!userId || !conventionId) {
      console.error('User or Convention not found, cannot fetch schedules.')
      return
    }

    // 3. Fetch data from the correct endpoint
    const response = await apiClient.get(
      `/user-schedules/${userId}/${conventionId}`,
    )

    // 4. Map data (using existing mapping logic)
    allSchedules.value = response.data.map((item) => ({
      id: item.id,
      scheduleTemplateId: item.scheduleTemplateId,
      date: item.scheduleDate.split('T')[0],
      startTime: item.startTime,
      endTime: item.endTime,
      title: item.title,
      location: item.location,
      description: item.content,
      category: '일정',
      group: item.courseName || '전체',
      participants: item.participantCount || 0,
    }))

    // 5. Set default date (using existing logic)
    if (allSchedules.value.length > 0) {
      const today = new Date().toISOString().split('T')[0]
      const futureDates = [...new Set(allSchedules.value.map((s) => s.date))]
        .filter((d) => d >= today)
        .sort()

      selectedDate.value =
        futureDates.length > 0 ? futureDates[0] : allSchedules.value[0].date

      const firstScheduleDate = parseLocalDate(allSchedules.value[0].date)
      currentCalendarYear.value = firstScheduleDate.getFullYear()
      currentCalendarMonth.value = firstScheduleDate.getMonth()
    }
  } catch (error) {
    console.error('Failed to load schedules:', error)
  }

  // Load dynamic actions separately
  await loadDynamicActions()
})
</script>

<style scoped>
.scrollbar-hide::-webkit-scrollbar {
  display: none;
}
.scrollbar-hide {
  -ms-overflow-style: none;
  scrollbar-width: none;
}

.modal-slide-enter-active,
.modal-slide-leave-active {
  transition: all 0.3s ease-in-out;
}
.modal-slide-enter-active .bg-white,
.modal-slide-leave-active .bg-white {
  transition: all 0.3s ease-in-out;
}

.modal-slide-enter-from,
.modal-slide-leave-to {
  background-color: rgba(0, 0, 0, 0);
}

@media (max-width: 639px) {
  .modal-slide-enter-from .bg-white,
  .modal-slide-leave-to .bg-white {
    transform: translateY(100%);
  }
}

@media (min-width: 640px) {
  .modal-slide-enter-from .bg-white,
  .modal-slide-leave-to .bg-white {
    transform: scale(0.95);
    opacity: 0;
  }
}
</style>
