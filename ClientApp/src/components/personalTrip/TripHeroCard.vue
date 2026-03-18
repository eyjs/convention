<template>
  <section
    class="relative overflow-hidden bg-primary-500 rounded-2xl shadow-xl p-6 text-white"
  >
    <!-- Background Image -->
    <div
      v-if="trip.coverImageUrl"
      class="absolute inset-0 bg-cover bg-center"
      :style="{ backgroundImage: `url(${trip.coverImageUrl})` }"
    ></div>
    <!-- Overlay for text readability -->
    <div class="absolute inset-0 bg-black/30"></div>

    <div class="relative z-10">
      <div class="flex justify-between items-start mb-3">
        <h1 class="text-3xl font-bold">{{ trip.title }}</h1>
        <div v-if="!effectiveReadonly" class="flex gap-2">
          <button
            v-if="tripId"
            class="px-4 py-2 bg-white/20 backdrop-blur-sm rounded-lg text-sm font-semibold hover:bg-white/30 transition-colors flex items-center gap-1.5"
            @click="$emit('open-share')"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              stroke-width="1.5"
              stroke="currentColor"
              class="w-4 h-4"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M3 16.5v2.25A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75V16.5m-13.5-9L12 3m0 0 4.5 4.5M12 3v13.5"
              />
            </svg>
            공유
          </button>
          <button
            class="px-4 py-2 bg-white/20 backdrop-blur-sm rounded-lg text-sm font-semibold hover:bg-white/30 transition-colors"
            @click="$emit('open-edit')"
          >
            수정
          </button>
        </div>
      </div>
      <div class="flex items-center gap-2 text-white/90 mb-2">
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
            d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
          />
        </svg>
        <span class="font-medium"
          >{{ trip.startDate }} ~ {{ trip.endDate }}</span
        >
      </div>
      <div class="flex items-center gap-2 text-white/90 mb-4">
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
            d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
          />
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
          />
        </svg>
        <span class="font-medium">{{
          trip.destination || '목적지 미설정'
        }}</span>
      </div>
      <p
        v-if="trip.description"
        class="text-white/90 text-sm leading-relaxed mb-4"
      >
        {{ trip.description }}
      </p>

      <!-- D-day Display -->
      <div class="bg-white/20 backdrop-blur-sm rounded-lg p-4 text-center">
        <p class="text-sm text-white/80 mb-1">{{ tripStatus }}</p>
        <p class="text-3xl font-bold">{{ dDayText }}</p>
      </div>
    </div>
  </section>
</template>

<script setup>
import { computed } from 'vue'
import dayjs from 'dayjs'

const props = defineProps({
  trip: { type: Object, required: true },
  effectiveReadonly: { type: Boolean, default: false },
  tripId: { type: String, default: null },
})

defineEmits(['open-share', 'open-edit'])

const today = dayjs()

const tripStatus = computed(() => {
  if (!props.trip.startDate) return ''
  const start = dayjs(props.trip.startDate)
  const end = dayjs(props.trip.endDate)

  if (today.isBefore(start, 'day')) return '여행 시작까지'
  if (today.isAfter(end, 'day')) return '여행 종료 후'
  return '여행 중'
})

const dDayText = computed(() => {
  if (!props.trip.startDate) return 'D-?'
  const start = dayjs(props.trip.startDate)
  const end = dayjs(props.trip.endDate)

  if (today.isBefore(start, 'day')) {
    const diff = start.diff(today, 'day')
    return `D-${diff}`
  }
  if (today.isAfter(end, 'day')) {
    const diff = today.diff(end, 'day')
    return `D+${diff}`
  }
  // 여행 중
  const currentDay = today.diff(start, 'day') + 1
  const totalDays = end.diff(start, 'day') + 1
  return `${currentDay}일차 / ${totalDays}일`
})
</script>
