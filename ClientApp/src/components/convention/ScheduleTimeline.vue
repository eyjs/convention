<template>
  <div>
    <!-- 날짜 선택 스크롤 -->
    <div class="bg-white border-b relative">
      <div ref="dateScrollContainer" class="overflow-x-auto scrollbar-hide" @scroll="handleDateScroll">
        <div class="flex px-4 py-3 space-x-2 min-w-max">
          <button
            :class="['flex-shrink-0 px-3 py-2 rounded-xl text-center transition-all', selectedDate === '' ? 'text-white shadow-lg scale-105' : 'bg-gray-100 text-gray-700 hover:bg-gray-200']"
            :style="selectedDate === '' ? { backgroundColor: props.brandColor } : {}"
            @click="selectedDate = ''"
          >
            <div class="text-sm font-bold leading-[2.125rem]">전체</div>
          </button>
          <button
            v-for="date in dates"
            :key="date.date"
            :class="['flex-shrink-0 px-3 py-2 rounded-xl text-center transition-all', selectedDate === date.date ? 'text-white shadow-lg scale-105' : 'bg-gray-100 text-gray-700 hover:bg-gray-200']"
            :style="selectedDate === date.date ? { backgroundColor: props.brandColor } : {}"
            @click="selectedDate = date.date"
          >
            <div class="text-xs font-medium mb-0.5">{{ date.day }}</div>
            <div class="text-sm font-bold">{{ date.month }}/{{ date.dayNum }}</div>
          </button>
        </div>
      </div>
      <div v-if="showLeftScroll" class="absolute left-0 top-0 bottom-0 flex items-center bg-gradient-to-r from-white to-transparent pr-4 pointer-events-none">
        <button class="p-1 bg-white rounded-full shadow-md pointer-events-auto hover:bg-gray-50 transition-colors" @click="scrollDateLeft">
          <svg class="w-4 h-4 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" /></svg>
        </button>
      </div>
      <div v-if="showRightScroll" class="absolute right-0 top-0 bottom-0 flex items-center bg-gradient-to-l from-white to-transparent pl-4 pointer-events-none">
        <button class="p-1 bg-white rounded-full shadow-md pointer-events-auto hover:bg-gray-50 transition-colors" @click="scrollDateRight">
          <svg class="w-4 h-4 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" /></svg>
        </button>
      </div>
    </div>

    <!-- SCHEDULE_CONTENT_TOP 동적 액션 -->
    <div v-if="contentTopActions.length > 0" class="px-4 pt-4">
      <DynamicActionRenderer :features="contentTopActions" />
    </div>

    <!-- 타임라인 일정 리스트 -->
    <div v-if="!showCalendarView" class="px-4 py-6 space-y-4">
      <div v-if="groupedSchedules.length === 0" class="text-center py-12">
        <div class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-gray-100 mb-4">
          <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
        </div>
        <p class="text-gray-500 font-medium">등록된 일정이 없습니다</p>
        <p class="text-gray-400 text-sm mt-2">새로운 일정이 추가되면 여기에 표시됩니다</p>
      </div>

      <div v-for="dateGroup in groupedSchedules" :key="dateGroup.date">
        <!-- 날짜 구분선 -->
        <div class="text-xs font-medium text-gray-400 mb-1.5 px-0.5">{{ formatDateHeader(dateGroup.date) }}</div>
        <!-- 카드 래퍼 -->
        <div class="bg-white rounded-xl border border-black/[0.07] overflow-hidden mb-3">
          <div
            v-for="(schedule, idx) in dateGroup.schedules"
            :key="schedule.id"
            :ref="(el) => { if (currentSchedule?.id === schedule.id) currentScheduleRef = el }"
            class="flex items-stretch cursor-pointer transition-colors active:brightness-[0.96]"
            :class="[
              idx < dateGroup.schedules.length - 1 ? 'border-b border-black/[0.05]' : '',
              isPastSchedule(schedule) ? 'opacity-30' : '',
              currentSchedule?.id === schedule.id ? 'bg-emerald-50/50' : '',
            ]"
            @click="emit('schedule-click', schedule)"
          >
            <!-- 시간 컬럼 -->
            <div class="w-[42px] flex-shrink-0 pt-2.5 pl-3">
              <span
                class="text-[11px]"
                :class="currentSchedule?.id === schedule.id ? 'font-medium' : 'text-gray-400'"
                :style="currentSchedule?.id === schedule.id ? { color: '#0F6E56' } : {}"
              >{{ schedule.startTime }}</span>
            </div>
            <!-- 도트 + 세로라인 -->
            <div class="w-4 flex-shrink-0 flex flex-col items-center pt-1">
              <div
                class="w-2 h-2 rounded-full flex-shrink-0 mt-1.5"
                :class="[
                  currentSchedule?.id === schedule.id ? '' : isPastSchedule(schedule) ? '' : '',
                ]"
                :style="{
                  backgroundColor: currentSchedule?.id === schedule.id ? '#1D9E75'
                    : isPastSchedule(schedule) ? '#b8dac8' : '#d4d2ca'
                }"
              ></div>
              <div
                v-if="idx < dateGroup.schedules.length - 1"
                class="w-[1.5px] flex-1 mt-1"
                :style="{
                  backgroundColor: isPastSchedule(schedule) ? '#b8dac8' : 'rgba(0,0,0,0.07)'
                }"
              ></div>
            </div>
            <!-- 콘텐츠 -->
            <div class="flex-1 py-2.5 pr-3 min-w-0">
              <div
                class="text-[13px] font-medium leading-tight"
                :class="currentSchedule?.id === schedule.id ? '' : 'text-gray-900'"
                :style="currentSchedule?.id === schedule.id ? { color: '#085041' } : {}"
              >{{ schedule.title }}</div>
              <div v-if="schedule.location" class="text-[11px] text-gray-400 mt-0.5">
                <span class="text-emerald-600">{{ schedule.location }}</span>
              </div>
              <!-- 배정 뱃지 -->
              <AssignmentBadges
                v-if="getScheduleBadges(schedule).length > 0"
                :attributes="getScheduleBadges(schedule)"
                class="mt-1"
              />
              <!-- 옵션투어 뱃지 -->
              <span
                v-if="schedule.isOptionTour"
                class="inline-block mt-1 px-1.5 py-0.5 rounded text-[10px] font-medium text-white"
                :style="{ backgroundColor: props.brandColor }"
              >옵션투어</span>
            </div>
            <!-- 화살표 -->
            <div class="flex items-center pr-3 opacity-20 flex-shrink-0">
              <svg width="9" height="9" viewBox="0 0 10 10" fill="none"><path d="M3.5 2l3 3-3 3" stroke="#1a1a1a" stroke-width="1.3" stroke-linecap="round" stroke-linejoin="round"/></svg>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 캘린더 뷰 -->
    <div v-if="showCalendarView" class="px-4 py-6">
      <div class="bg-white rounded-xl shadow-sm p-4">
        <div class="flex items-center justify-between mb-6">
          <button class="p-2 hover:bg-gray-100 rounded-lg transition-all" @click="changeMonth(-1)">
            <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" /></svg>
          </button>
          <h3 class="text-lg font-bold text-gray-900">{{ currentCalendarYear }}년 {{ currentCalendarMonth + 1 }}월</h3>
          <button class="p-2 hover:bg-gray-100 rounded-lg transition-all" @click="changeMonth(1)">
            <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" /></svg>
          </button>
        </div>
        <div class="grid grid-cols-7 gap-2 mb-4">
          <div v-for="day in ['일', '월', '화', '수', '목', '금', '토']" :key="day" class="text-center text-xs font-bold text-gray-500 py-2">{{ day }}</div>
        </div>
        <div class="grid grid-cols-7 gap-2">
          <div
            v-for="day in calendarDays"
            :key="day.date"
            :class="['aspect-square flex flex-col items-center justify-center rounded-lg cursor-pointer transition-all', day.isToday ? 'border-2' : '', day.hasSchedule && !day.isToday ? 'bg-blue-50' : day.isToday ? '' : 'hover:bg-gray-50', !day.isCurrentMonth ? 'opacity-30' : '']"
            :style="day.isToday ? { backgroundColor: props.brandColor + '20', borderColor: props.brandColor } : {}"
            @click="selectCalendarDay(day)"
          >
            <span :class="['text-sm font-medium', day.isToday ? 'font-bold' : 'text-gray-700']" :style="day.isToday ? { color: props.brandColor } : {}">{{ day.day }}</span>
            <div v-if="day.scheduleCount > 0" class="flex items-center justify-center mt-1">
              <div class="w-1 h-1 rounded-full" :style="{ backgroundColor: props.brandColor }"></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 전체 이미지 보기 -->
    <div v-if="fullImageUrl" class="fixed inset-0 z-[100] bg-black/90 flex items-center justify-center" @click="fullImageUrl = null">
      <img :src="fullImageUrl" class="max-w-full max-h-full object-contain" @click.stop />
      <button class="absolute top-4 right-4 w-10 h-10 bg-white/20 text-white rounded-full flex items-center justify-center text-xl hover:bg-white/30" @click="fullImageUrl = null">&times;</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import dayjs from 'dayjs'
import apiClient from '@/services/api'
import DynamicActionRenderer from '@/dynamic-features/DynamicActionRenderer.vue'
import AssignmentBadges from '@/components/convention/AssignmentBadges.vue'
import { useAuthStore } from '@/stores/auth'

const props = defineProps({
  brandColor: { type: String, default: '#10b981' },
  isAdmin: { type: Boolean, default: false },
  conventionId: { type: [Number, String], required: true },
  attributes: { type: Array, default: () => [] },
  highlightCurrent: { type: Boolean, default: true },
  dimPastSchedules: { type: Boolean, default: false },
})

const emit = defineEmits(['schedule-click'])

const authStore = useAuthStore()

// 내부 상태
const selectedDate = ref('')
const dateScrollContainer = ref(null)
const showLeftScroll = ref(false)
const showRightScroll = ref(false)
const showCalendarView = ref(false)
const fullImageUrl = ref(null)
const currentScheduleRef = ref(null)
const allSchedules = ref([])
const allOptionTours = ref([])
const allActions = ref([])
const currentCalendarYear = ref(new Date().getFullYear())
const currentCalendarMonth = ref(new Date().getMonth())

defineExpose({ showCalendarView })

// 일정 + 옵션투어 통합
const mergedSchedules = computed(() => {
  const regular = allSchedules.value.map((s) => ({ ...s, isOptionTour: false }))
  const tours = allOptionTours.value.map((ot) => ({
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
  return [...regular, ...tours].sort((a, b) => {
    if (a.date !== b.date) return a.date.localeCompare(b.date)
    return a.startTime.localeCompare(b.startTime)
  })
})

const schedules = computed(() =>
  selectedDate.value ? mergedSchedules.value.filter((s) => s.date === selectedDate.value) : mergedSchedules.value,
)

const currentSchedule = computed(() => {
  if (!props.highlightCurrent) return null
  const today = dayjs().format('YYYY-MM-DD')
  if (selectedDate.value && selectedDate.value !== today) return null
  const now = dayjs()
  const todaySchedules = schedules.value.filter((s) => s.date === today).sort((a, b) => a.startTime.localeCompare(b.startTime))
  if (todaySchedules.length === 0) return null
  let current = null
  for (const schedule of todaySchedules) {
    const dt = dayjs(`${schedule.date} ${schedule.startTime}`)
    if (dt.isBefore(now) || dt.isSame(now)) current = schedule
    else break
  }
  return current || todaySchedules[0]
})

const contentTopActions = computed(() => allActions.value.filter((a) => a.targetLocation === 'SCHEDULE_CONTENT_TOP'))

const dates = computed(() => {
  if (mergedSchedules.value.length === 0) return []
  const uniqueDates = [...new Set(mergedSchedules.value.map((s) => s.date))].sort()
  return uniqueDates.map((dateStr) => {
    const date = parseLocalDate(dateStr)
    const days = ['일', '월', '화', '수', '목', '금', '토']
    return { date: dateStr, day: days[date.getDay()], dayNum: String(date.getDate()), month: `${date.getMonth() + 1}` }
  })
})

const groupedSchedules = computed(() => {
  const grouped = {}
  schedules.value.forEach((s) => {
    if (!grouped[s.date]) grouped[s.date] = []
    grouped[s.date].push(s)
  })
  return Object.keys(grouped).map((date) => ({ date, schedules: grouped[date].sort((a, b) => a.startTime.localeCompare(b.startTime)) }))
})

const calendarDays = computed(() => {
  const days = []
  const today = new Date()
  const todayStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`
  const y = currentCalendarYear.value
  const m = currentCalendarMonth.value
  const firstDay = new Date(y, m, 1)
  const lastDay = new Date(y, m + 1, 0)
  const startDay = firstDay.getDay()

  for (let i = startDay - 1; i >= 0; i--) {
    const d = new Date(y, m, -i)
    const ds = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
    const cnt = mergedSchedules.value.filter((s) => s.date === ds).length
    days.push({ date: ds, day: d.getDate(), isCurrentMonth: false, isToday: ds === todayStr, hasSchedule: cnt > 0, scheduleCount: cnt })
  }
  for (let i = 1; i <= lastDay.getDate(); i++) {
    const d = new Date(y, m, i)
    const ds = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
    const cnt = mergedSchedules.value.filter((s) => s.date === ds).length
    days.push({ date: ds, day: i, isCurrentMonth: true, isToday: ds === todayStr, hasSchedule: cnt > 0, scheduleCount: cnt })
  }
  return days
})

function parseLocalDate(dateStr) {
  const [year, month, day] = dateStr.split('-').map(Number)
  return new Date(year, month - 1, day)
}

function formatDateHeader(dateStr) {
  const date = parseLocalDate(dateStr)
  const days = ['일', '월', '화', '수', '목', '금', '토']
  return `${date.getMonth() + 1}월 ${date.getDate()}일 (${days[date.getDay()]})`
}

function isPastSchedule(schedule) {
  if (!props.dimPastSchedules || currentSchedule.value?.id === schedule.id) return false
  return dayjs(`${schedule.date} ${schedule.startTime}`).isBefore(dayjs())
}

function handleDateScroll() {
  if (!dateScrollContainer.value) return
  const el = dateScrollContainer.value
  showLeftScroll.value = el.scrollLeft > 0
  showRightScroll.value = el.scrollLeft < el.scrollWidth - el.clientWidth - 1
}

function scrollDateLeft() { dateScrollContainer.value?.scrollBy({ left: -200, behavior: 'smooth' }) }
function scrollDateRight() { dateScrollContainer.value?.scrollBy({ left: 200, behavior: 'smooth' }) }

function stripHtml(html) {
  if (!html) return ''
  return html.replace(/<br\s*\/?>/gi, '\n').replace(/<\/p>/gi, '\n').replace(/<\/div>/gi, '\n').replace(/<\/li>/gi, '\n').replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').replace(/\n{3,}/g, '\n\n').trim()
}

function openFullImage(url) { fullImageUrl.value = url }

function getScheduleBadges(schedule) {
  if (!schedule.visibleAttributes || !props.attributes?.length) return []
  const keys = schedule.visibleAttributes.split(',').map((k) => k.trim()).filter(Boolean)
  return props.attributes.filter((a) => keys.includes(a.key))
}
function selectCalendarDay(day) { selectedDate.value = day.date; showCalendarView.value = false }

function changeMonth(direction) {
  currentCalendarMonth.value += direction
  if (currentCalendarMonth.value < 0) { currentCalendarMonth.value = 11; currentCalendarYear.value -= 1 }
  else if (currentCalendarMonth.value > 11) { currentCalendarMonth.value = 0; currentCalendarYear.value += 1 }
}

watch(currentSchedule, async (newSchedule) => {
  if (newSchedule && !showCalendarView.value) {
    await nextTick()
    currentScheduleRef.value?.scrollIntoView({ behavior: 'smooth', block: 'center' })
  }
}, { immediate: true })

async function loadSchedules() {
  const userId = authStore.user?.id
  if (!userId || !props.conventionId) return
  const response = await apiClient.get(`/user-schedules/${userId}/${props.conventionId}`)
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
  const toursRes = await apiClient.get(`/user-schedules/${userId}/${props.conventionId}/option-tours`)
  allOptionTours.value = toursRes.data || []
  if (mergedSchedules.value.length > 0) {
    selectedDate.value = ''
    const firstDate = parseLocalDate(mergedSchedules.value[0].date)
    currentCalendarYear.value = firstDate.getFullYear()
    currentCalendarMonth.value = firstDate.getMonth()
  }
  nextTick(() => handleDateScroll())
}

async function loadDynamicActions() {
  try {
    const response = await apiClient.get(`/conventions/${props.conventionId}/actions/all`, { params: { targetLocation: 'SCHEDULE_CONTENT_TOP', isActive: true } })
    allActions.value = response.data || []
  } catch (error) {
    console.error('Failed to load dynamic actions:', error)
    allActions.value = []
  }
}

onMounted(async () => {
  try {
    await loadSchedules()
  } catch (error) {
    console.error('Failed to load schedules:', error)
  }
  await loadDynamicActions()
})
</script>

<style scoped>
.scrollbar-hide::-webkit-scrollbar { display: none; }
.scrollbar-hide { -ms-overflow-style: none; scrollbar-width: none; }
</style>
