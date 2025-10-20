<template>
  <div class="relative rounded-2xl overflow-hidden shadow-xl">
    <!-- 블러 배경 -->
    <div class="absolute inset-0 bg-gradient-to-br from-red-600 via-red-700 to-red-900"></div>
    <div class="absolute inset-0 backdrop-blur-3xl bg-black/30"></div>
    
    <!-- 컨텐츠 -->
    <div class="relative px-6 py-8">
      <div class="flex items-center justify-between mb-4">
        <div class="flex items-center space-x-3">
          <div class="w-10 h-10 bg-white/20 rounded-full flex items-center justify-center backdrop-blur-sm">
            <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div>
            <p class="text-white/80 text-sm font-medium">마감까지</p>
            <p class="text-white text-xs">{{ formattedDeadline }}</p>
          </div>
        </div>
        <div class="text-white/60 text-sm">
          {{ urgencyText }}
        </div>
      </div>

      <!-- 타이머 -->
      <div class="grid grid-cols-4 gap-3">
        <div v-for="unit in timeUnits" :key="unit.label" 
             class="bg-white/10 backdrop-blur-sm rounded-xl p-4 text-center">
          <div class="text-3xl font-bold text-white mb-1 tabular-nums">
            {{ unit.value }}
          </div>
          <div class="text-white/70 text-xs font-medium uppercase tracking-wider">
            {{ unit.label }}
          </div>
        </div>
      </div>

      <!-- 진행바 -->
      <div class="mt-6">
        <div class="flex justify-between text-xs text-white/60 mb-2">
          <span>시작</span>
          <span>{{ Math.round(timeProgress) }}% 경과</span>
          <span>마감</span>
        </div>
        <div class="h-2 bg-white/10 rounded-full overflow-hidden backdrop-blur-sm">
          <div 
            class="h-full bg-gradient-to-r from-yellow-400 to-red-500 transition-all duration-1000 ease-out"
            :style="{ width: `${timeProgress}%` }"
          ></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  deadline: {
    type: String,
    required: true
  }
})

const now = ref(Date.now())
let intervalId = null

const deadlineDate = computed(() => new Date(props.deadline))

const timeRemaining = computed(() => {
  const diff = deadlineDate.value.getTime() - now.value
  if (diff <= 0) return { days: 0, hours: 0, minutes: 0, seconds: 0, total: 0 }

  return {
    days: Math.floor(diff / (1000 * 60 * 60 * 24)),
    hours: Math.floor((diff / (1000 * 60 * 60)) % 24),
    minutes: Math.floor((diff / (1000 * 60)) % 60),
    seconds: Math.floor((diff / 1000) % 60),
    total: diff
  }
})

const timeUnits = computed(() => [
  { label: 'Days', value: String(timeRemaining.value.days).padStart(2, '0') },
  { label: 'Hours', value: String(timeRemaining.value.hours).padStart(2, '0') },
  { label: 'Mins', value: String(timeRemaining.value.minutes).padStart(2, '0') },
  { label: 'Secs', value: String(timeRemaining.value.seconds).padStart(2, '0') }
])

const urgencyText = computed(() => {
  const days = timeRemaining.value.days
  if (days > 7) return '여유있음'
  if (days > 3) return '주의'
  if (days > 0) return '긴급'
  return '마감임박!'
})

const formattedDeadline = computed(() => {
  return deadlineDate.value.toLocaleDateString('ko-KR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
})

const timeProgress = computed(() => {
  const total = deadlineDate.value.getTime() - (deadlineDate.value.getTime() - 30 * 24 * 60 * 60 * 1000) // 30일 기준
  const elapsed = now.value - (deadlineDate.value.getTime() - 30 * 24 * 60 * 60 * 1000)
  return Math.min(100, Math.max(0, (elapsed / total) * 100))
})

onMounted(() => {
  intervalId = setInterval(() => {
    now.value = Date.now()
  }, 1000)
})

onUnmounted(() => {
  if (intervalId) {
    clearInterval(intervalId)
  }
})
</script>

<style scoped>
.tabular-nums {
  font-variant-numeric: tabular-nums;
}
</style>
