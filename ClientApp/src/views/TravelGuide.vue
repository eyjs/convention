<template>
  <div class="min-h-screen bg-gray-50 pb-24">
    <MainHeader title="여행 가이드" :show-back="true" />

    <div class="max-w-2xl mx-auto px-4 py-4 space-y-4">
      <!-- 날씨 -->
      <div v-if="weather.available" class="bg-white rounded-2xl shadow-md p-5">
        <h3 class="text-sm font-semibold text-gray-500 mb-3">현지 날씨</h3>
        <div class="flex items-center gap-4">
          <img
            v-if="weather.icon"
            :src="`https://openweathermap.org/img/wn/${weather.icon}@2x.png`"
            class="w-16 h-16"
            alt="날씨"
          />
          <div>
            <p class="text-3xl font-bold text-gray-900">{{ weather.temp }}°C</p>
            <p class="text-sm text-gray-600">
              {{ weather.description }}
            </p>
            <p class="text-xs text-gray-400">
              {{ travelInfo.destinationCity }} · 체감 {{ weather.feelsLike }}°C
              · 습도 {{ weather.humidity }}%
            </p>
          </div>
        </div>
      </div>

      <!-- 환율 -->
      <div v-if="exchange.available" class="bg-white rounded-2xl shadow-md p-5">
        <h3 class="text-sm font-semibold text-gray-500 mb-3">환율 정보</h3>
        <div class="flex items-center justify-between">
          <div>
            <p class="text-2xl font-bold text-gray-900">
              {{ exchange.description }}
            </p>
            <p class="text-xs text-gray-400">
              {{ exchange.currencyName }} ({{ exchange.currencyCode }})
            </p>
          </div>
          <!-- 간이 환율 계산 -->
          <div class="text-right">
            <input
              v-model.number="exchangeInput"
              type="number"
              class="w-24 px-2 py-1 border rounded text-right text-sm"
              placeholder="KRW"
            />
            <p
              v-if="exchangeInput"
              class="text-sm font-medium text-primary-600 mt-1"
            >
              ≈ {{ convertedAmount }} {{ exchange.currencyCode }}
            </p>
          </div>
        </div>
      </div>

      <!-- 긴급연락처 -->
      <div
        v-if="emergencyContacts.length > 0"
        class="bg-white rounded-2xl shadow-md p-5"
      >
        <h3 class="text-sm font-semibold text-gray-500 mb-3">긴급 연락처</h3>
        <div class="space-y-3">
          <div
            v-for="(contact, i) in emergencyContacts"
            :key="i"
            class="flex items-center justify-between"
          >
            <div>
              <p class="font-medium text-gray-900">{{ contact.name }}</p>
              <p class="text-xs text-gray-500">{{ contact.role }}</p>
            </div>
            <a
              :href="`tel:${contact.phone}`"
              class="flex items-center gap-1 px-3 py-1.5 bg-green-50 text-green-700 rounded-lg text-sm font-medium"
            >
              <svg
                class="w-4 h-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"
                />
              </svg>
              {{ contact.phone }}
            </a>
          </div>
        </div>
      </div>

      <!-- 미팅 장소 / 공항 안내 -->
      <div
        v-if="travelInfo.meetingPointInfo"
        class="bg-white rounded-2xl shadow-md p-5"
      >
        <h3 class="text-sm font-semibold text-gray-500 mb-3">집결지 안내</h3>
        <div
          class="prose prose-sm max-w-none text-gray-700"
          v-html="travelInfo.meetingPointInfo"
        ></div>
      </div>

      <!-- 캘린더 내보내기 -->
      <div class="bg-white rounded-2xl shadow-md p-5">
        <h3 class="text-sm font-semibold text-gray-500 mb-3">일정 내보내기</h3>
        <a
          :href="`/api/conventions/${conventionId}/travel-guide/calendar.ics`"
          class="flex items-center gap-2 px-4 py-3 bg-primary-50 text-primary-700 rounded-xl font-medium text-sm"
          download
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
              d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
            />
          </svg>
          캘린더에 일정 추가 (.ics)
        </a>
      </div>

      <!-- 정보 없음 -->
      <div
        v-if="
          !weather.available &&
          !exchange.available &&
          emergencyContacts.length === 0 &&
          !travelInfo.meetingPointInfo
        "
        class="text-center py-12 text-gray-400"
      >
        <p>아직 등록된 여행 정보가 없습니다</p>
        <p class="text-xs mt-1">관리자가 행사 정보를 업데이트하면 표시됩니다</p>
      </div>
    </div>

    <BottomNavigationBar />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import MainHeader from '@/components/common/MainHeader.vue'
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue'
import apiClient from '@/services/api'

const route = useRoute()
const conventionId = route.params.conventionId

const travelInfo = ref({})
const weather = ref({ available: false })
const exchange = ref({ available: false })
const exchangeInput = ref(null)

const emergencyContacts = computed(() => {
  try {
    if (!travelInfo.value.emergencyContacts) return []
    const parsed =
      typeof travelInfo.value.emergencyContacts === 'string'
        ? JSON.parse(travelInfo.value.emergencyContacts)
        : travelInfo.value.emergencyContacts
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
})

const convertedAmount = computed(() => {
  if (!exchangeInput.value || !exchange.value.rate) return ''
  return (exchangeInput.value * exchange.value.rate).toFixed(2)
})

async function loadTravelInfo() {
  try {
    const res = await apiClient.get(`/conventions/${conventionId}/travel-guide`)
    travelInfo.value = res.data
  } catch (e) {
    console.error('여행 정보 로드 실패:', e)
  }
}

async function loadWeather() {
  try {
    const res = await apiClient.get(
      `/conventions/${conventionId}/travel-guide/weather`,
    )
    if (res.data.available === false) {
      weather.value = { available: false }
      return
    }
    const d = res.data
    weather.value = {
      available: true,
      temp: Math.round(d.main?.temp),
      feelsLike: Math.round(d.main?.feels_like),
      humidity: d.main?.humidity,
      description: d.weather?.[0]?.description,
      icon: d.weather?.[0]?.icon,
    }
  } catch {
    weather.value = { available: false }
  }
}

async function loadExchangeRate() {
  try {
    const res = await apiClient.get(
      `/conventions/${conventionId}/travel-guide/exchange-rate`,
    )
    exchange.value = res.data
  } catch {
    exchange.value = { available: false }
  }
}

onMounted(() => {
  loadTravelInfo()
  loadWeather()
  loadExchangeRate()
})
</script>
