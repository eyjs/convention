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
        <div 
          v-for="item in todayItineraries" 
          :key="item.id" 
          class="flex items-start gap-3 p-3 bg-gray-50 rounded-lg transition-all duration-300"
          :class="{
            'border-2 border-primary-500': item.isCurrent // Only border highlight
          }"
        >
          <div class="flex-shrink-0 w-12 text-center">
            <p class="text-sm font-semibold text-primary-600">
              {{ item.startTime || '--:--' }}
            </p>
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
      <div @click="$emit('go-to-itinerary')" class="bg-white rounded-xl shadow-md p-5 cursor-pointer hover:shadow-lg transition-shadow">
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
      <div @click="$emit('open-accommodation-modal')" class="bg-white rounded-xl shadow-md p-5 cursor-pointer hover:shadow-lg transition-shadow">
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
      <div @click="$emit('open-flight-modal')" class="bg-white rounded-xl shadow-md p-5 cursor-pointer hover:shadow-lg transition-shadow">
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
      <div @click="$emit('go-to-expenses')" class="bg-white rounded-xl shadow-md p-5 cursor-pointer hover:shadow-lg transition-shadow">
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

    <!-- Checklist Progress -->
    <section v-if="totalChecklistItems > 0 && checklistProgressPercentage < 100" class="bg-white rounded-2xl shadow-md p-6">
      <div @click="$emit('go-to-notes')" class="cursor-pointer">
        <div class="flex justify-between items-center mb-2">
          <h2 class="text-xl font-bold text-gray-900 flex items-center gap-2">
            <svg class="w-6 h-6 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" /></svg>
            체크리스트
          </h2>
          <span class="font-semibold text-gray-700">{{ completedChecklistItems }} / {{ totalChecklistItems }}</span>
        </div>
        <div class="w-full bg-gray-200 rounded-full h-2.5">
          <div class="bg-primary-500 h-2.5 rounded-full" :style="{ width: checklistProgressPercentage + '%' }"></div>
        </div>
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
  </div>
</template>

<script setup>
import { computed, defineEmits } from 'vue'
import { useRouter } from 'vue-router'
import dayjs from 'dayjs'
import isBetween from 'dayjs/plugin/isBetween' // Import the plugin

dayjs.extend(isBetween) // Extend dayjs with the plugin

const props = defineProps({
  trip: {
    type: Object,
    required: true
  },
  readonly: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['open-accommodation-modal', 'open-flight-modal', 'go-to-itinerary', 'go-to-expenses', 'go-to-notes'])

function formatCurrency(amount) {
  if (!amount) return '₩0'
  return `₩${amount.toLocaleString()}`
}

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

  const now = dayjs(); // Get current time
  const currentTripDate = dayjs(props.trip.startDate).add(currentDayNumber.value - 1, 'day');

  let filtered = props.trip.itineraryItems
    .filter(item => item.dayNumber === currentDayNumber.value)
    .filter(item => {
      // Filter out items whose end time has already passed
      if (item.endTime) {
        const itemEndTime = dayjs(`${currentTripDate.format('YYYY-MM-DD')}T${item.endTime}`);
        return itemEndTime.isAfter(now); // Only show items that are still active or upcoming
      }
      return true; // If no end time, assume it's always relevant
    })
    .sort((a, b) => {
      if (!a.startTime) return 1
      if (!b.startTime) return -1
      return a.startTime.localeCompare(b.startTime)
    });

  // Add isCurrent status
  let nextUpcomingFound = false;
  return filtered.map(item => {
    const itemStartTime = dayjs(`${currentTripDate.format('YYYY-MM-DD')}T${item.startTime}`);
    const itemEndTime = dayjs(`${currentTripDate.format('YYYY-MM-DD')}T${item.endTime}`);
    
    const isCurrent = now.isBetween(itemStartTime, itemEndTime, null, '[]'); // [] includes start and end
    
    // Highlight the very next upcoming item if no current item is found
    let isNextUpcoming = false;
    if (!isCurrent && !nextUpcomingFound && itemStartTime.isAfter(now)) {
        isNextUpcoming = true;
        nextUpcomingFound = true; // Mark that we found the next upcoming
    }

    return {
      ...item,
      isCurrent: isCurrent || isNextUpcoming, // Highlight current or next upcoming
    };
  });
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
      // If it's a rental car or personal car, flight.amount already includes all sub-costs
      if (flight.category === '렌트카' || flight.category === '자가용') {
        return sum + (flight.amount || 0);
      } else {
        // For other flight types, amount might be separate
        return sum + (flight.amount || 0) + (flight.tollFee || 0) +
               (flight.fuelCost || 0) + (flight.parkingFee || 0);
      }
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
      if (flight.category === '렌트카' || flight.category === '자가용') {
        return sum + (flight.amount || 0);
      } else {
        return sum + (flight.amount || 0) + (flight.tollFee || 0) +
               (flight.fuelCost || 0) + (flight.parkingFee || 0);
      }
    }, 0)
    if (total > 0) categories['교통'] = total
  }
  return Object.entries(categories)
    .map(([category, amount]) => ({ category, amount }))
    .sort((a, b) => b.amount - a.amount)
})

// Checklist Progress
const totalChecklistItems = computed(() => {
    return props.trip.checklistCategories?.reduce((sum, category) => sum + category.totalItemsCount, 0) || 0;
});
const completedChecklistItems = computed(() => {
    return props.trip.checklistCategories?.reduce((sum, category) => sum + category.completedItemsCount, 0) || 0;
});
const checklistProgressPercentage = computed(() => {
    if (totalChecklistItems.value === 0) return 0;
    return Math.round((completedChecklistItems.value / totalChecklistItems.value) * 100);
});

</script>