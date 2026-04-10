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
    <!-- 공통 헤더 사용 -->
    <MainHeader title="나의 일정" :show-back="true">
      <template #actions>
        <button
          class="p-2 hover:bg-gray-100 rounded-lg"
          @click="showCalendarView = !showCalendarView"
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
              d="M20 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 14H4V6h16v12zM6 8h4v2H6V8z"
            />
          </svg>
        </button>
      </template>
    </MainHeader>

    <!-- 날짜 선택 스크롤 -->
    <div v-if="!showCalendarView" class="bg-white border-b relative">
      <div
        ref="dateScrollContainer"
        class="overflow-x-auto scrollbar-hide"
        @scroll="handleDateScroll"
      >
        <div class="flex px-4 py-3 space-x-2 min-w-max">
          <!-- 전체 탭 -->
          <button
            :class="[
              'flex-shrink-0 px-3 py-2 rounded-xl text-center transition-all',
              selectedDate === ''
                ? 'text-white shadow-lg scale-105'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200',
            ]"
            :style="selectedDate === '' ? { backgroundColor: brandColor } : {}"
            @click="selectedDate = ''"
          >
            <div class="text-sm font-bold leading-[2.125rem]">전체</div>
          </button>
          <button
            v-for="date in dates"
            :key="date.date"
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
            @click="selectedDate = date.date"
          >
            <div class="text-xs font-medium mb-0.5">{{ date.day }}</div>
            <div class="text-sm font-bold">
              {{ date.month }}/{{ date.dayNum }}
            </div>
          </button>
        </div>
      </div>
      <!-- 왼쪽 스크롤 표시 -->
      <div
        v-if="showLeftScroll"
        class="absolute left-0 top-0 bottom-0 flex items-center bg-gradient-to-r from-white to-transparent pr-4 pointer-events-none"
      >
        <button
          class="p-1 bg-white rounded-full shadow-md pointer-events-auto hover:bg-gray-50 transition-colors"
          @click="scrollDateLeft"
        >
          <svg
            class="w-4 h-4 text-gray-600"
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
      </div>
      <!-- 오른쪽 스크롤 표시 -->
      <div
        v-if="showRightScroll"
        class="absolute right-0 top-0 bottom-0 flex items-center bg-gradient-to-l from-white to-transparent pl-4 pointer-events-none"
      >
        <button
          class="p-1 bg-white rounded-full shadow-md pointer-events-auto hover:bg-gray-50 transition-colors"
          @click="scrollDateRight"
        >
          <svg
            class="w-4 h-4 text-gray-600"
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
    </div>

    <!-- SCHEDULE_CONTENT_TOP 위치: 날짜 선택 스크롤 아래 -->
    <div v-if="contentTopActions.length > 0" class="px-4 pt-4">
      <DynamicActionRenderer :features="contentTopActions" />
    </div>

    <!-- 일정 리스트 -->
    <div v-if="!showCalendarView" class="px-4 py-6 space-y-4">
      <!-- 빈 상태 -->
      <div v-if="groupedSchedules.length === 0" class="text-center py-12">
        <div
          class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-gray-100 mb-4"
        >
          <svg
            class="w-8 h-8 text-gray-400"
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
        </div>
        <p class="text-gray-500 font-medium">등록된 일정이 없습니다</p>
        <p class="text-gray-400 text-sm mt-2">
          새로운 일정이 추가되면 여기에 표시됩니다
        </p>
      </div>

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

        <!-- 타임라인 일정 -->
        <div class="relative">
          <!-- 세로 연결선: bullet 중심 (5px) -->
          <div
            class="absolute top-0 bottom-0 w-px bg-gray-200"
            style="left: 5px"
          ></div>

          <div
            v-for="(schedule, idx) in dateGroup.schedules"
            :key="schedule.id"
            :ref="
              (el) => {
                if (currentSchedule?.id === schedule.id) currentScheduleRef = el
              }
            "
            class="flex items-center gap-3 cursor-pointer"
            :class="idx < dateGroup.schedules.length - 1 ? 'mb-3' : ''"
            @click="openScheduleDetail(schedule)"
          >
            <!-- bullet -->
            <div
              class="w-2.5 h-2.5 rounded-full border-2 flex-shrink-0 z-10"
              :style="{
                borderColor: brandColor,
                backgroundColor:
                  currentSchedule?.id === schedule.id ? brandColor : '#fff',
              }"
            ></div>

            <!-- 카드 -->
            <div
              class="flex-1 min-w-0"
              :class="[
                'rounded-xl transition-all overflow-hidden',
                currentSchedule?.id === schedule.id
                  ? 'shadow-md ring-1'
                  : 'bg-white shadow-sm hover:shadow-md',
              ]"
              :style="
                currentSchedule?.id === schedule.id
                  ? {
                      backgroundColor: brandColor + '08',
                      '--tw-ring-color': brandColor + '30',
                    }
                  : {}
              "
            >
              <div class="p-3.5">
                <!-- 시간 (카드 최상단) -->
                <div class="flex items-center gap-1 mb-1.5">
                  <span
                    class="text-xs font-bold"
                    :style="{ color: brandColor }"
                  >
                    {{ schedule.startTime }}
                  </span>
                  <span v-if="schedule.endTime" class="text-xs text-gray-400">
                    — {{ schedule.endTime }}
                  </span>
                </div>
                <div class="flex items-center justify-between gap-2 mb-1">
                  <h3 class="font-bold text-gray-900 text-sm truncate flex-1">
                    {{ schedule.title }}
                  </h3>
                  <span
                    v-if="schedule.isOptionTour"
                    class="px-2 py-0.5 rounded-md text-xs font-medium text-white flex-shrink-0"
                    :style="{ backgroundColor: brandColor }"
                  >
                    옵션투어
                  </span>
                </div>
                <p
                  v-if="schedule.description"
                  class="text-xs text-gray-500 line-clamp-2 leading-relaxed whitespace-pre-line"
                >
                  {{ stripHtml(schedule.description) }}
                </p>
                <div
                  v-if="schedule.location"
                  class="flex items-center gap-1 text-xs text-gray-400 mt-1.5"
                >
                  <svg
                    class="w-3 h-3 flex-shrink-0"
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
              </div>
              <!-- 이미지 갤러리 (카드 하단, 최대 3장) -->
              <!-- 이미지 갤러리 (카드 하단, 최대 3장) -->
              <div
                v-if="schedule.images?.length"
                class="px-3.5 pb-3 pt-2.5 mt-1 border-t border-gray-100 grid grid-cols-3 gap-1.5"
              >
                <!-- prettier-ignore -->
                <img
                  v-for="img in schedule.images.slice(0, 3)"
                  :key="img.id"
                  :src="img.imageUrl"
                  class="w-full h-16 object-cover rounded border border-gray-200 cursor-pointer hover:opacity-80 transition-opacity"
                  @click.stop="openFullImage(img.imageUrl)"
                />
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
            class="p-2 hover:bg-gray-100 rounded-lg transition-all"
            @click="changeMonth(-1)"
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
            class="p-2 hover:bg-gray-100 rounded-lg transition-all"
            @click="changeMonth(1)"
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
            @click="selectCalendarDay(day)"
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
    <SlideUpModal
      :is-open="showParticipantsModal"
      z-index-class="z-[60]"
      @close="closeParticipantsModal"
    >
      <template #header-title>{{ selectedGroupName }}</template>
      <template #body>
        <ParticipantList
          :participants="participants"
          :show-phone="isAdmin"
          :search-placeholder="
            isAdmin ? '이름, 소속, 연락처로 검색...' : '이름, 소속으로 검색...'
          "
        />
      </template>
    </SlideUpModal>

    <!-- 전체 이미지 보기 -->
    <div
      v-if="fullImageUrl"
      class="fixed inset-0 z-[100] bg-black/90 flex items-center justify-center"
      @click="fullImageUrl = null"
    >
      <img
        :src="fullImageUrl"
        class="max-w-full max-h-full object-contain"
        @click.stop
      />
      <button
        class="absolute top-4 right-4 w-10 h-10 bg-white/20 text-white rounded-full flex items-center justify-center text-xl hover:bg-white/30"
        @click="fullImageUrl = null"
      >
        &times;
      </button>
    </div>

    <!-- 일정 상세 모달 -->
    <SlideUpModal :is-open="!!selectedSchedule" @close="closeScheduleDetail">
      <template #header-title>
        <span
          v-if="selectedSchedule?.isOptionTour"
          class="flex items-center space-x-2"
        >
          <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path
              d="M10.894 2.553a1 1 0 00-1.788 0l-7 14a1 1 0 001.169 1.409l5-1.429A1 1 0 009 15.571V11a1 1 0 112 0v4.571a1 1 0 00.725.962l5 1.428a1 1 0 001.17-1.408l-7-14z"
            />
          </svg>
          <span>옵션투어 상세</span>
        </span>
        <span v-else>일정 상세</span>
      </template>
      <template #body>
        <div class="space-y-4">
          <!-- 상단 메타: 날짜 · 시간 · 장소 (작게) -->
          <div
            class="flex flex-wrap items-center gap-x-2 gap-y-1 text-xs text-gray-500"
          >
            <span>{{ formatDate(selectedSchedule.date) }}</span>
            <span class="text-gray-300">·</span>
            <span class="font-medium" :style="{ color: brandColor }">
              {{ selectedSchedule.startTime }}
              <template v-if="selectedSchedule.endTime">
                — {{ selectedSchedule.endTime }}
              </template>
            </span>
            <template v-if="selectedSchedule.location">
              <span class="text-gray-300">·</span>
              <span class="flex items-center gap-0.5">
                <svg
                  class="w-3 h-3"
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
                {{ selectedSchedule.location }}
              </span>
            </template>
            <span
              v-if="selectedSchedule.isOptionTour"
              class="px-1.5 py-0.5 rounded-md text-xs font-medium text-white"
              :style="{ backgroundColor: brandColor }"
            >
              옵션투어
            </span>
          </div>

          <!-- 타이틀 -->
          <h3 class="text-xl font-bold text-gray-900">
            {{ selectedSchedule.title }}
          </h3>

          <!-- 메인 컨텐츠 영역 -->
          <div
            v-if="
              selectedSchedule.description || selectedSchedule.images?.length
            "
            class="bg-gray-50 rounded-xl p-4 -mx-1"
          >
            <div v-if="selectedSchedule.description">
              <QuillViewer
                :content="selectedSchedule.description"
                @image-clicked="openFullImage"
              />
            </div>

            <!-- 이미지 갤러리 -->
            <div
              v-if="selectedSchedule.images?.length"
              :class="
                selectedSchedule.description
                  ? 'mt-4 pt-4 border-t border-gray-200'
                  : ''
              "
            >
              <div
                :class="[
                  'grid gap-2',
                  selectedSchedule.images.length === 1
                    ? 'grid-cols-1'
                    : 'grid-cols-2',
                ]"
              >
                <img
                  v-for="img in selectedSchedule.images"
                  :key="img.id"
                  :src="img.imageUrl"
                  class="w-full rounded-lg object-cover cursor-pointer hover:opacity-90 transition-opacity"
                  :class="
                    selectedSchedule.images.length === 1 ? 'max-h-64' : 'h-40'
                  "
                  @click="openFullImage(img.imageUrl)"
                />
              </div>
            </div>
          </div>

          <!-- 내 자리 보기 버튼 -->
          <button
            v-if="selectedSchedule.seatingLayoutId"
            class="w-full py-3 bg-gradient-to-r from-amber-500 to-orange-500 text-white rounded-xl font-semibold text-sm shadow hover:shadow-md active:scale-[0.98] transition-all flex items-center justify-center gap-2"
            @click="goToMySeat(selectedSchedule.seatingLayoutId)"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7" />
            </svg>
            내 자리 보기
          </button>

          <!-- 참여 그룹 & 참석자 보기 (관리자만) - 옵션투어는 제외 -->
          <div
            v-if="
              isAdmin &&
              selectedSchedule.group &&
              !selectedSchedule.isOptionTour
            "
            class="pt-4 border-t mt-6"
          >
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
                class="w-full px-4 py-2.5 rounded-lg text-white font-medium transition-all hover:opacity-90 flex items-center justify-center space-x-2"
                :style="{ backgroundColor: brandColor }"
                @click="
                  loadParticipants(
                    selectedSchedule.scheduleTemplateId,
                    selectedSchedule.group,
                  )
                "
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
      </template>
    </SlideUpModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useUIStore } from '@/stores/ui'
import { useConventionStore } from '@/stores/convention'
import apiClient from '@/services/api'
import DynamicActionRenderer from '@/dynamic-features/DynamicActionRenderer.vue'
import MainHeader from '@/components/common/MainHeader.vue'
import ParticipantList from '@/components/ParticipantList.vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import QuillViewer from '@/components/common/QuillViewer.vue'
import dayjs from 'dayjs'

const route = useRoute()
const authStore = useAuthStore()
const conventionStore = useConventionStore()
const uiStore = useUIStore()

const showCalendarView = ref(false)
const selectedDate = ref('')
const dateScrollContainer = ref(null)
const showLeftScroll = ref(false)
const showRightScroll = ref(false)
const selectedSchedule = ref(null)
const pageReady = ref(false)
const allSchedules = ref([]) // 전체 일정 저장
const allOptionTours = ref([]) // 전체 옵션투어 저장
const allActions = ref([]) // 전체 동적 액션 저장
const currentScheduleRef = ref(null) // 현재 일정 카드 ref
const currentScheduleId = ref(null) // 현재 진행 중인 일정 ID
const showParticipantsModal = ref(false) // 참석자 목록 모달
const participants = ref([]) // 참석자 목록
const selectedGroupName = ref('') // 선택된 그룹명
const fullImageUrl = ref(null) // 전체 이미지 보기

// 일정과 옵션투어를 합친 통합 스케줄
const mergedSchedules = computed(() => {
  // 일반 일정
  const regularSchedules = allSchedules.value.map((s) => ({
    ...s,
    isOptionTour: false,
  }))

  // 옵션투어를 일정 형식으로 변환
  const optionTourSchedules = allOptionTours.value.map((ot) => ({
    id: `option-${ot.id}`,
    scheduleTemplateId: null,
    date: ot.date,
    startTime: ot.startTime,
    endTime: ot.endTime,
    title: ot.name,
    location: '',
    description: ot.content,
    category: '옵션투어',
    group: '',
    participants: 0,
    isOptionTour: true,
    customOptionId: ot.customOptionId,
    images: ot.images || [],
  }))

  // 두 배열을 합치고 날짜/시간순으로 정렬
  return [...regularSchedules, ...optionTourSchedules].sort((a, b) => {
    if (a.date !== b.date) return a.date.localeCompare(b.date)
    return a.startTime.localeCompare(b.startTime)
  })
})

const schedules = computed(() => {
  if (!selectedDate.value) return mergedSchedules.value
  return mergedSchedules.value.filter((s) => s.date === selectedDate.value)
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
  if (mergedSchedules.value.length === 0) return []

  // 일정에서 고유 날짜 추출
  const uniqueDates = [
    ...new Set(mergedSchedules.value.map((s) => s.date)),
  ].sort()

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
    const daySchedules = mergedSchedules.value.filter((s) => s.date === dateStr)
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
    const daySchedules = mergedSchedules.value.filter((s) => s.date === dateStr)

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

function handleDateScroll() {
  if (!dateScrollContainer.value) return
  const el = dateScrollContainer.value
  showLeftScroll.value = el.scrollLeft > 0
  showRightScroll.value = el.scrollLeft < el.scrollWidth - el.clientWidth - 1
}

function scrollDateLeft() {
  dateScrollContainer.value?.scrollBy({ left: -200, behavior: 'smooth' })
}

function scrollDateRight() {
  dateScrollContainer.value?.scrollBy({ left: 200, behavior: 'smooth' })
}

function stripHtml(html) {
  if (!html) return ''
  return html
    .replace(/<br\s*\/?>/gi, '\n')
    .replace(/<\/p>/gi, '\n')
    .replace(/<\/div>/gi, '\n')
    .replace(/<\/li>/gi, '\n')
    .replace(/<[^>]*>/g, '')
    .replace(/&nbsp;/g, ' ')
    .replace(/\n{3,}/g, '\n\n')
    .trim()
}

function openFullImage(url) {
  fullImageUrl.value = url
}

const router = useRouter()

function goToMySeat(layoutId) {
  // 모달 닫고 이동
  selectedSchedule.value = null
  const conventionId = route.params.conventionId
  setTimeout(() => {
    router.push(`/conventions/${conventionId}/my-seat?layout=${layoutId}`)
  }, 100)
}

function openScheduleDetail(schedule) {
  selectedSchedule.value = schedule
}

function closeScheduleDetail() {
  selectedSchedule.value = null
  closeParticipantsModal() // 자식 모달도 함께 닫기
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
      await conventionStore.selectConvention(
        parseInt(route.params.conventionId),
      )
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
      images: item.images || [],
      seatingLayoutId: item.seatingLayoutId || null,
    }))

    // 5. Fetch option tours
    const optionToursResponse = await apiClient.get(
      `/user-schedules/${userId}/${conventionId}/option-tours`,
    )

    allOptionTours.value = optionToursResponse.data || []

    // 6. Set default date — 전체 보기
    if (mergedSchedules.value.length > 0) {
      selectedDate.value = ''

      const firstScheduleDate = parseLocalDate(mergedSchedules.value[0].date)
      currentCalendarYear.value = firstScheduleDate.getFullYear()
      currentCalendarMonth.value = firstScheduleDate.getMonth()
    }

    nextTick(() => handleDateScroll())
  } catch (error) {
    console.error('Failed to load schedules:', error)
  }

  // Load dynamic actions separately
  await loadDynamicActions()

  // 모든 초기 로딩 완료 → 페이지 렌더링
  pageReady.value = true
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
</style>
