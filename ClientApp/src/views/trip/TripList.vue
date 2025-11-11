<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader title="내 여행" :show-back="false" />

    <div class="max-w-2xl mx-auto px-4 py-6">
      <!-- 로딩 상태 -->
      <div v-if="loading" class="text-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-4 text-gray-600">로딩 중...</p>
      </div>

      <!-- 여행 목록 -->
      <template v-else>
        <!-- 새 여행 만들기 버튼 -->
        <button @click="goToCreateTrip" class="w-full mb-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 flex items-center justify-center gap-2">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          <span>새 여행 만들기</span>
        </button>

        <!-- 여행 카드 목록 -->
        <div v-if="trips.length === 0" class="text-center py-12">
          <svg class="w-16 h-16 mx-auto text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <p class="mt-4 text-gray-500">아직 여행이 없습니다.</p>
          <p class="text-gray-400 text-sm">새 여행을 만들어보세요!</p>
        </div>

        <div v-else class="space-y-4">
          <div v-for="trip in trips" :key="trip.id" @click="goToTripDetail(trip.id)" class="bg-white rounded-lg shadow p-4 hover:shadow-md transition-shadow cursor-pointer">
            <h3 class="text-lg font-bold text-gray-900">{{ trip.title }}</h3>
            <p v-if="trip.description" class="text-sm text-gray-600 mt-1">{{ trip.description }}</p>

            <div class="mt-3 flex items-center gap-4 text-sm text-gray-500">
              <div class="flex items-center gap-1">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <span>{{ trip.startDate }} ~ {{ trip.endDate }}</span>
              </div>
            </div>

            <div v-if="trip.destination || trip.city" class="mt-2 flex items-center gap-1 text-sm text-gray-600">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <span>{{ [trip.destination, trip.city].filter(Boolean).join(', ') }}</span>
            </div>

            <div class="mt-3 flex gap-2 text-xs">
              <span v-if="trip.flights.length > 0" class="px-2 py-1 bg-blue-100 text-blue-700 rounded">
                항공 {{ trip.flights.length }}
              </span>
              <span v-if="trip.accommodations.length > 0" class="px-2 py-1 bg-green-100 text-green-700 rounded">
                숙소 {{ trip.accommodations.length }}
              </span>
            </div>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import MainHeader from '@/components/common/MainHeader.vue'
import apiClient from '@/services/api'

const router = useRouter()
const loading = ref(true)
const trips = ref([])

async function loadTrips() {
  loading.value = true
  try {
    const response = await apiClient.get('/personal-trips')
    trips.value = response.data
  } catch (error) {
    console.error('여행 목록 로드 실패:', error)
    alert('여행 목록을 불러올 수 없습니다.')
  } finally {
    loading.value = false
  }
}

function goToCreateTrip() {
  router.push('/trips/create')
}

function goToTripDetail(tripId) {
  router.push(`/trips/${tripId}`)
}

onMounted(loadTrips)
</script>
