<template>
  <div class="px-4 py-3 flex items-center justify-between" :style="headerStyle">
    <!-- 좌측: 행사명 + 날짜장소 + D-Day -->
    <div class="flex-1 min-w-0 pr-3">
      <h1 class="text-lg font-bold text-white leading-tight truncate">
        {{ convention.title }}
      </h1>
      <p
        class="text-xs mt-0.5 truncate"
        style="color: rgba(255, 255, 255, 0.7)"
      >
        {{ formattedPeriod }}
        <span v-if="convention.location"> · {{ convention.location }}</span>
      </p>
      <!-- D-Day 뱃지 -->
      <div
        v-if="dDayLabel"
        class="mt-1.5 inline-flex items-center px-2.5 py-1 bg-white/20 backdrop-blur-sm rounded-full text-xs text-white font-bold"
      >
        {{ dDayLabel }}
      </div>
    </div>

    <!-- 우측: 알림벨 + 햄버거 메뉴 -->
    <div class="flex items-center gap-1 flex-shrink-0">
      <NotificationBell :convention-id="conventionId" text-class="text-white" />
      <button
        class="p-2 rounded-full hover:bg-white/20 transition-colors"
        aria-label="메뉴 열기"
        @click="emit('menu-click')"
      >
        <svg
          class="w-6 h-6 text-white"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          aria-hidden="true"
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
</template>

<script setup>
import { computed } from 'vue'
import NotificationBell from '@/components/common/NotificationBell.vue'

const props = defineProps({
  convention: { type: Object, required: true },
  // convention: { title, startDate, endDate, location, brandColor, conventionImg }
  conventionId: { type: Number, required: true },
  dDay: { type: Number, default: null },
})

const emit = defineEmits(['menu-click'])

// 브랜드컬러 기반 그라디언트 스타일 (ConventionHome의 headerGradientStyle 로직 재사용)
const headerStyle = computed(() => {
  const color = props.convention.brandColor || '#10b981'

  // 16진수 색상을 RGB로 변환
  const hex = color.startsWith('#') ? color.slice(1) : color
  const r = parseInt(hex.slice(0, 2), 16)
  const g = parseInt(hex.slice(2, 4), 16)
  const b = parseInt(hex.slice(4, 6), 16)

  if (isNaN(r) || isNaN(g) || isNaN(b)) {
    return { background: color }
  }

  // 약간 더 어두운 색 계산 (0.8배)
  const darkerR = Math.floor(r * 0.8)
  const darkerG = Math.floor(g * 0.8)
  const darkerB = Math.floor(b * 0.8)

  return {
    background: `linear-gradient(to bottom right, rgb(${r}, ${g}, ${b}), rgb(${darkerR}, ${darkerG}, ${darkerB}))`,
  }
})

// 날짜를 '2026-04-09(수)' 형식으로 변환
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

// "2026-04-09(수) ~ 04-10(목)" 형태로 컴팩트 기간 포맷
function formatCompactPeriod(startDate, endDate) {
  if (!startDate) return ''

  const start = formatDateWithDay(startDate)
  if (!endDate) return start

  const endDateObj = new Date(endDate)
  const endMonth = String(endDateObj.getMonth() + 1).padStart(2, '0')
  const endDay = String(endDateObj.getDate()).padStart(2, '0')
  const days = ['일', '월', '화', '수', '목', '금', '토']
  const endDayOfWeek = days[endDateObj.getDay()]
  const end = `${endMonth}-${endDay}(${endDayOfWeek})`

  return `${start} ~ ${end}`
}

const formattedPeriod = computed(() =>
  formatCompactPeriod(props.convention.startDate, props.convention.endDate),
)

// D-Day 레이블 계산
const dDayLabel = computed(() => {
  if (props.dDay === null) return null
  if (props.dDay === 0) return 'D-Day'
  if (props.dDay > 0) return `D-${props.dDay}`
  // 진행 중 (음수): null 반환하여 표시 안 함
  return null
})
</script>
