<template>
  <div class="space-y-6">
    <!-- 2. Today's Summary (여행 중에만 표시) -->
    <section v-if="isDuringTrip" class="bg-white rounded-2xl shadow-md p-6">
      <h2 class="text-xl font-bold text-gray-900 mb-4 flex items-center gap-2">
        <svg class="w-6 h-6 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        오늘의 일정 (Day {{ currentDayNumber }})
      </h2>
      <div v-if="todayItineraries.length > 0" class="space-y-3">
        <div v-for="item in todayItineraries" :key="item.id" class="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
          <div class="flex-shrink-0 w-12 text-center">
            <p class="text-sm font-semibold text-primary-600">{{ item.startTime || '--:--' }}</p>
          </div>
          <div class="flex-1">
            <p class="font-medium text-gray-900">{{ item.locationName }}</p>
            <p class="text-sm text-gray-500">{{ item.category }}</p>
          </div>
        </div>
      </div>
      <p v-else class="text-gray-500 text-center py-4">오늘 등록된 일정이 없습니다.</p>
    </section>

    <!-- 3. Category Cards (4개) -->
    <section class="grid grid-cols-2 gap-4">
      <!-- 일정 카드 -->
      <div @click="navigateTo('itinerary')" class="bg-white rounded-xl shadow-md p-5 cursor-pointer hover:shadow-lg transition-shadow">
        <div class="flex items-center justify-between mb-3">
          <div class="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
            </svg>
          </div>
        </div>
        <h3 class="font-bold text-gray-900 mb-1">일정</h3>
        <p class="text-2xl font-bold text-blue-600">{{ itineraryCount }}</p>
        <p class="text-xs text-gray-500 mt-1">등록된 일정</p>
      </div>

      <!-- 숙소 카드 -->
      <div @click="openAccommodationManagementModal" class="bg-white rounded-xl shadow-md p-5 cursor-pointer hover:shadow-lg transition-shadow">
        <div class="flex items-center justify-between mb-3">
          <div class="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            </svg>
          </div>
        </div>
        <h3 class="font-bold text-gray-900 mb-1">숙소</h3>
        <p class="text-2xl font-bold text-green-600">{{ accommodationCount }}</p>
        <p class="text-xs text-gray-500 mt-1">예약된 숙소</p>
      </div>

      <!-- 교통 카드 -->
      <div @click="navigateTo('flight')" class="bg-white rounded-xl shadow-md p-5 cursor-pointer hover:shadow-lg transition-shadow">
        <div class="flex items-center justify-between mb-3">
          <div class="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
            </svg>
          </div>
        </div>
        <h3 class="font-bold text-gray-900 mb-1">교통</h3>
        <p class="text-2xl font-bold text-purple-600">{{ flightCount }}</p>
        <p class="text-xs text-gray-500 mt-1">등록된 교통편</p>
      </div>

      <!-- 가계부 카드 -->
      <div @click="navigateTo('expenses')" class="bg-white rounded-xl shadow-md p-5 cursor-pointer hover:shadow-lg transition-shadow">
        <div class="flex items-center justify-between mb-3">
          <div class="w-12 h-12 bg-orange-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
        </div>
        <h3 class="font-bold text-gray-900 mb-1">지출</h3>
        <p class="text-2xl font-bold text-orange-600">{{ formatCurrency(totalExpenses) }}</p>
        <p class="text-xs text-gray-500 mt-1">총 지출 금액</p>
      </div>
    </section>

    <!-- 4. 지출 요약 -->
    <section v-if="trip.budget > 0" class="bg-white rounded-2xl shadow-md p-6">
      <h2 class="text-xl font-bold text-gray-900 mb-4 flex items-center gap-2">
        <svg class="w-6 h-6 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
        </svg>
        예산 대비 지출
      </h2>

      <!-- 예산 vs 지출 -->
      <div class="mb-4">
        <div class="flex justify-between text-sm text-gray-600 mb-2">
          <span>예산: {{ formatCurrency(trip.budget) }}</span>
          <span>지출: {{ formatCurrency(totalExpenses) }} ({{ budgetPercentage }}%)</span>
        </div>
        <div class="w-full bg-gray-200 rounded-full h-4 overflow-hidden">
          <div
            class="h-full transition-all duration-500 rounded-full"
            :class="budgetPercentage > 100 ? 'bg-red-500' : budgetPercentage > 80 ? 'bg-orange-500' : 'bg-green-500'"
            :style="{ width: Math.min(budgetPercentage, 100) + '%' }"
          ></div>
        </div>
        <p v-if="budgetPercentage > 100" class="text-sm text-red-600 mt-2 font-medium">
          ⚠️ 예산을 {{ formatCurrency(totalExpenses - trip.budget) }} 초과했습니다.
        </p>
        <p v-else class="text-sm text-gray-600 mt-2">
          남은 예산: {{ formatCurrency(trip.budget - totalExpenses) }}
        </p>
      </div>

      <!-- 카테고리별 지출 -->
      <div class="space-y-3">
        <h3 class="font-semibold text-gray-700 text-sm">카테고리별 지출</h3>
        <div v-for="expense in expensesByCategory" :key="expense.category" class="flex items-center gap-3">
          <div class="flex-1">
            <div class="flex justify-between text-sm mb-1">
              <span class="text-gray-700">{{ expense.category }}</span>
              <span class="font-medium text-gray-900">{{ formatCurrency(expense.amount) }}</span>
            </div>
            <div class="w-full bg-gray-100 rounded-full h-2">
              <div
                class="bg-primary-500 h-full rounded-full transition-all duration-300"
                :style="{ width: (expense.amount / totalExpenses * 100) + '%' }"
              ></div>
            </div>
          </div>
        </div>
        <p v-if="expensesByCategory.length === 0" class="text-gray-500 text-center py-4">등록된 지출이 없습니다.</p>
      </div>
    </section>

    <!-- 5. 알림 & 리마인더 -->
    <section class="bg-white rounded-2xl shadow-md p-6">
      <h2 class="text-xl font-bold text-gray-900 mb-4 flex items-center gap-2">
        <svg class="w-6 h-6 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
        </svg>
        알림 & 리마인더
      </h2>

      <div class="space-y-3">
        <!-- 다가오는 일정 -->
        <div v-if="upcomingReminders.length > 0">
          <div v-for="reminder in upcomingReminders" :key="reminder.id" class="flex items-start gap-3 p-3 bg-blue-50 border border-blue-200 rounded-lg">
            <div class="flex-shrink-0 w-10 h-10 bg-blue-500 text-white rounded-full flex items-center justify-center text-sm font-bold">
              {{ reminder.daysUntil }}
            </div>
            <div class="flex-1">
              <p class="font-medium text-gray-900">{{ reminder.title }}</p>
              <p class="text-sm text-gray-600">{{ reminder.description }}</p>
              <p class="text-xs text-blue-600 mt-1">{{ reminder.dateText }}</p>
            </div>
          </div>
        </div>

        <!-- 예산 경고 -->
        <div v-if="trip.budget > 0 && budgetPercentage > 80" class="flex items-start gap-3 p-3 bg-orange-50 border border-orange-200 rounded-lg">
          <div class="flex-shrink-0">
            <svg class="w-6 h-6 text-orange-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
          <div>
            <p class="font-medium text-gray-900">예산 경고</p>
            <p class="text-sm text-gray-600">
              {{ budgetPercentage > 100 ? '예산을 초과했습니다!' : '예산의 80% 이상을 사용했습니다.' }}
            </p>
          </div>
        </div>

        <p v-if="upcomingReminders.length === 0 && (!trip.budget || trip.budget === 0 || budgetPercentage <= 80)" class="text-gray-500 text-center py-4">
          현재 알림이 없습니다.
        </p>
      </div>
    </section>
  </div>
</template>

<script setup>
import { computed, defineEmits } from 'vue'
import { useRouter } from 'vue-router'
import dayjs from 'dayjs'

const props = defineProps({
  trip: {
    type: Object,
    required: true
  }
})

const emit = defineEmits(['open-accommodation-modal'])

const router = useRouter()
const tripId = computed(() => props.trip.id)

// D-day 계산
const today = dayjs()

const isDuringTrip = computed(() => {
  if (!props.trip.startDate || !props.trip.endDate) return false
  const start = dayjs(props.trip.startDate)
  const end = dayjs(props.trip.endDate)
  return !today.isBefore(start, 'day') && !today.isAfter(end, 'day')
})

const currentDayNumber = computed(() => {
  if (!isDuringTrip.value) return 0
  const start = dayjs(props.trip.startDate)
  return today.diff(start, 'day') + 1
})

// 오늘의 일정
const todayItineraries = computed(() => {
  if (!isDuringTrip.value || !props.trip.itineraryItems) return []
  return props.trip.itineraryItems
    .filter(item => item.dayNumber === currentDayNumber.value)
    .sort((a, b) => {
      if (!a.startTime) return 1
      if (!b.startTime) return -1
      return a.startTime.localeCompare(b.startTime)
    })
})

// 카운트
const itineraryCount = computed(() => props.trip.itineraryItems?.length || 0)
const accommodationCount = computed(() => props.trip.accommodations?.length || 0)
const flightCount = computed(() => props.trip.flights?.length || 0)

// 지출 계산
const totalExpenses = computed(() => {
  let total = 0
  if (props.trip.itineraryItems) {
    total += props.trip.itineraryItems.reduce((sum, item) => sum + (item.expenseAmount || 0), 0)
  }
  if (props.trip.accommodations) {
    total += props.trip.accommodations.reduce((sum, acc) => sum + (acc.expenseAmount || 0), 0)
  }
  if (props.trip.flights) {
    total += props.trip.flights.reduce((sum, flight) => {
      return sum + (flight.amount || 0) + (flight.tollFee || 0) +
             (flight.fuelCost || 0) + (flight.parkingFee || 0) + (flight.rentalCost || 0)
    }, 0)
  }
  return total
})

const budgetPercentage = computed(() => {
  if (!props.trip.budget || props.trip.budget === 0) return 0
  return Math.round((totalExpenses.value / props.trip.budget) * 100)
})

// 카테고리별 지출
const expensesByCategory = computed(() => {
  const categories = {}
  if (props.trip.itineraryItems) {
    props.trip.itineraryItems.forEach(item => {
      if (item.expenseAmount && item.expenseAmount > 0) {
        const cat = item.category || '기타'
        categories[cat] = (categories[cat] || 0) + item.expenseAmount
      }
    })
  }
  if (props.trip.accommodations) {
    const total = props.trip.accommodations.reduce((sum, acc) => sum + (acc.expenseAmount || 0), 0)
    if (total > 0) categories['숙소'] = total
  }
  if (props.trip.flights) {
    const total = props.trip.flights.reduce((sum, flight) => {
      return sum + (flight.amount || 0) + (flight.tollFee || 0) +
             (flight.fuelCost || 0) + (flight.parkingFee || 0) + (flight.rentalCost || 0)
    }, 0)
    if (total > 0) categories['교통'] = total
  }
  return Object.entries(categories)
    .map(([category, amount]) => ({ category, amount }))
    .sort((a, b) => b.amount - a.amount)
})

// 다가오는 리마인더
const upcomingReminders = computed(() => {
  const reminders = []
  if (!props.trip.startDate) return reminders
  const start = dayjs(props.trip.startDate)
  const daysUntilTrip = start.diff(today, 'day')

  if (daysUntilTrip > 0 && daysUntilTrip <= 7) {
    reminders.push({
      id: 'trip-start',
      title: '여행 시작 임박',
      description: '여행 준비를 확인하세요!',
      dateText: props.trip.startDate,
      daysUntil: daysUntilTrip
    })
  }

  if (props.trip.accommodations) {
    props.trip.accommodations.forEach(acc => {
      if (acc.checkInTime) {
        const checkIn = dayjs(acc.checkInTime)
        const diff = checkIn.diff(today, 'day')
        if (diff >= 0 && diff <= 1) {
          reminders.push({
            id: `checkin-${acc.id}`,
            title: diff === 0 ? '오늘 체크인' : '내일 체크인',
            description: acc.name,
            dateText: checkIn.format('YYYY-MM-DD HH:mm'),
            daysUntil: diff
          })
        }
      }
    })
  }

  if (props.trip.flights) {
    props.trip.flights.forEach(flight => {
      if (flight.departureTime) {
        const departure = dayjs(flight.departureTime)
        const diff = departure.diff(today, 'day')
        if (diff >= 0 && diff <= 1) {
          reminders.push({
            id: `flight-${flight.id}`,
            title: diff === 0 ? '오늘 출발' : '내일 출발',
            description: `${flight.airline || ''} ${flight.flightNumber || ''} - ${flight.departureLocation} → ${flight.arrivalLocation}`,
            dateText: departure.format('YYYY-MM-DD HH:mm'),
            daysUntil: diff
          })
        }
      }
    })
  }

  return reminders.sort((a, b) => a.daysUntil - b.daysUntil)
})

// 유틸리티
function formatCurrency(amount) {
  if (!amount) return '₩0'
  return `₩${amount.toLocaleString()}`
}

function openAccommodationManagementModal() {
  emit('open-accommodation-modal');
}

function navigateTo(section) {
  router.push(`/trips/${tripId.value}/${section}`)
}
</script>