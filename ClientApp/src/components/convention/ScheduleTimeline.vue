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

      <div v-for="dateGroup in groupedSchedules" :key="dateGroup.date" class="space-y-3">
        <div class="flex items-center justify-between px-2">
          <h2 class="text-sm font-bold text-gray-900">{{ formatDateHeader(dateGroup.date) }}</h2>
          <span class="text-xs text-gray-500">{{ dateGroup.schedules.length }}개 일정</span>
        </div>
        <div class="relative">
          <div class="absolute top-0 bottom-0 w-px bg-gray-200" style="left: 5px"></div>
          <div
            v-for="(schedule, idx) in dateGroup.schedules"
            :key="schedule.id"
            :ref="(el) => { if (currentSchedule?.id === schedule.id) currentScheduleRef = el }"
            class="flex items-center gap-3 cursor-pointer"
            :class="[idx < dateGroup.schedules.length - 1 ? 'mb-3' : '', isPastSchedule(schedule) ? 'opacity-40' : '']"
            @click="emit('schedule-click', schedule)"
          >
            <!-- bullet -->
            <div class="w-2.5 h-2.5 rounded-full border-2 flex-shrink-0 z-10" :style="{ borderColor: props.brandColor, backgroundColor: currentSchedule?.id === schedule.id ? props.brandColor : '#fff' }"></div>
            <!-- 카드 -->
            <div
              class="flex-1 min-w-0 rounded-xl transition-all overflow-hidden"
              :class="currentSchedule?.id === schedule.id ? 'shadow-md ring-1' : 'bg-white shadow-sm hover:shadow-md'"
              :style="currentSchedule?.id === schedule.id ? { backgroundColor: props.brandColor + '08', '--tw-ring-color': props.brandColor + '30' } : {}"
            >
              <div class="p-3.5">
                <div class="flex items-center gap-1 mb-1.5">
                  <span class="text-xs font-bold" :style="{ color: props.brandColor }">{{ schedule.startTime }}</span>
                  <span v-if="schedule.endTime" class="text-xs text-gray-400"> — {{ schedule.endTime }}</span>
                </div>
                <div class="flex items-center justify-between gap-2 mb-1">
                  <h3 class="font-bold text-gray-900 text-sm truncate flex-1">{{ schedule.title }}</h3>
                  <span v-if="schedule.isOptionTour" class="px-2 py-0.5 rounded-md text-xs font-medium text-white flex-shrink-0" :style="{ backgroundColor: props.brandColor }">옵션투어</span>
                </div>
                <p v-if="schedule.description" class="text-xs text-gray-500 line-clamp-2 leading-relaxed whitespace-pre-line">{{ stripHtml(schedule.description) }}</p>
                <div v-if="schedule.location" class="flex items-center gap-1 text-xs text-gray-400 mt-1.5">
                  <svg class="w-3 h-3 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
                  <span>{{ schedule.location }}</span>
                </div>
              </div>
              <div v-if="schedule.images?.length" class="px-3.5 pb-3 pt-2.5 mt-1 border-t border-gray-100 grid grid-cols-3 gap-1.5">
                <!-- prettier-ignore -->
                <img v-for="img in schedule.images.slice(0, 3)" :key="img.id" :src="img.imageUrl" class="w-full h-16 object-cover rounded border border-gray-200 cursor-pointer hover:opacity-80 transition-opacity" @click.stop="openFullImage(img.imageUrl)" />
              </div>
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
