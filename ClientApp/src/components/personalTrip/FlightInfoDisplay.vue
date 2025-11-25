<template>
  <div class="space-y-3 text-sm">
    <div class="flex items-center justify-between">
        <p class="font-semibold text-gray-900">{{ flight.airline }} {{ flight.flightNumber }}</p>
        <button @click="emit('clear')" class="text-gray-500 hover:text-red-600 text-xs font-medium">변경</button>
    </div>
    <p class="text-gray-600">{{ flight.departureLocation }} → {{ flight.arrivalLocation }}</p>
    <div class="grid grid-cols-2 gap-x-4 gap-y-1 text-gray-500">
        <div v-if="flight.departureTime" class="flex items-center gap-1.5">
            <span class="font-semibold w-12 inline-block">출발시간:</span>
            <span>{{ formatDateTime(flight.departureTime) }}</span>
        </div>
        <div v-if="flight.arrivalTime" class="flex items-center gap-1.5">
            <span class="font-semibold w-12 inline-block">도착시간:</span>
            <span>{{ formatDateTime(flight.arrivalTime) }}</span>
        </div>
        <div v-if="flight.terminal" class="flex items-center gap-1.5">
            <span class="font-semibold w-12 inline-block">터미널:</span>
            <span>{{ flight.terminal }}</span>
        </div>
        <div v-if="flight.gate" class="flex items-center gap-1.5">
            <span class="font-semibold w-12 inline-block">게이트:</span>
            <span>{{ flight.gate }}</span>
        </div>
        <div v-if="flight.status" class="flex items-center gap-1.5">
            <span class="font-semibold w-12 inline-block">상태:</span>
            <span class="text-xs px-2 py-0.5 rounded-full" :class="getStatusClass(flight.status)">
                {{ flight.status }}
            </span>
        </div>
    </div>
  </div>
</template>

<script setup>
import dayjs from 'dayjs'

const props = defineProps({
  flight: {
    type: Object,
    required: true
  },
  type: {
    type: String,
    required: true,
    validator: (value) => ['departure', 'arrival'].includes(value)
  }
})

const emit = defineEmits(['clear'])

function formatDateTime(datetime) {
  if (!datetime) return ''
  return dayjs(datetime).format('YYYY-MM-DD HH:mm')
}

function getStatusClass(status) {
  if (!status) return 'bg-gray-100 text-gray-700';
  if (status.includes('출발') || status.includes('도착') || status.includes('탑승')) {
    return 'bg-green-100 text-green-700';
  } else if (status.includes('지연') || status.includes('결항')) {
    return 'bg-red-100 text-red-700';
  } else {
    return 'bg-gray-100 text-gray-700';
  }
}
</script>
