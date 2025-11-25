<template>
  <div class="min-h-screen bg-gray-50 font-sans text-gray-800 pb-20">
    
    <!-- 헤더 -->
    <div class="sticky top-0 bg-white z-20 px-5 pt-12 pb-4 shadow-sm">
      <div class="flex justify-between items-center mb-4 max-w-md mx-auto">
        <div>
          <h1 class="text-2xl font-black text-gray-900 tracking-tight">나의 항공편</h1>
          <p class="text-xs text-gray-500 font-medium">스타투어 플래너</p>
        </div>
        <div class="w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center border border-gray-200 shadow-sm">
           <Share2 class="w-4 h-4 text-gray-600" />
        </div>
      </div>

      <!-- 검색창 -->
      <div class="relative w-full max-w-md mx-auto">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-4 h-4" />
          <input 
            type="text" 
            placeholder="항공편, 공항, 항공사 검색..." 
            class="w-full pl-9 pr-4 py-2.5 bg-gray-100 border-none rounded-xl text-sm font-medium focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all placeholder:text-gray-400"
            v-model="searchTerm"
          />
      </div>

      <!-- 필터 탭 (가로 스크롤) -->
      <div class="flex gap-2 mt-4 overflow-x-auto no-scrollbar pb-1 max-w-md mx-auto">
          <button
            v-for="tab in TABS"
            :key="tab.id"
            @click="activeTab = tab.id"
            :class="[
              'px-3.5 py-1.5 rounded-full text-xs font-bold whitespace-nowrap transition-colors border',
              activeTab === tab.id 
                ? 'bg-gray-900 text-white border-gray-900' 
                : 'bg-white text-gray-500 border-gray-200 hover:bg-gray-50'
            ]"
          >
            {{ tab.label }}
          </button>
      </div>
    </div>

    <!-- 컨텐츠 영역 -->
    <div class="px-5 pt-6 max-w-md mx-auto">
      <!-- 날짜 헤더 -->
      <div class="flex items-center gap-2 mb-4">
          <div class="h-5 w-1 bg-indigo-500 rounded-full"></div>
          <h2 class="text-lg font-bold text-gray-800">오늘의 일정</h2>
      </div>

      <div class="space-y-4">
        <template v-if="filteredFlights.length > 0">
          <FlightCard v-for="(flight, index) in filteredFlights" :key="index" :data="flight" />
        </template>
        <div v-else class="flex flex-col items-center justify-center py-12 text-center">
           <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mb-4">
              <Plane class="w-6 h-6 text-gray-400" />
           </div>
           <p class="text-gray-500 font-bold">검색된 항공편이 없습니다</p>
           <p class="text-xs text-gray-400 mt-1">검색어나 필터를 확인해주세요</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { Search, Share2, Plane } from 'lucide-vue-next';
import FlightCard from '@/components/personalTrip/FlightCard.vue';

const searchTerm = ref('');
const activeTab = ref('All');

// API 호출 로직으로 대체될 샘플 데이터
const flights = ref([
  {
    Type: "Departure",
    FlightId: "ST-701",
    Airline: "대한항공",
    Airport: "뉴욕 (JFK)",
    AirportCode: "JFK",
    ScheduleDate: "2024-05-20",
    ScheduleTime: "10:30",
    EstimatedTime: "10:30",
    Terminal: "T2",
    Gate: "234",
    CheckInCounter: "A12-15",
    Status: "Boarding",
    MasterFlightId: "KE081"
  },
  {
    Type: "Departure",
    FlightId: "ST-304",
    Airline: "아시아나항공",
    Airport: "오사카 (KIX)",
    AirportCode: "KIX",
    ScheduleDate: "2024-05-20",
    ScheduleTime: "13:15",
    EstimatedTime: "14:00",
    Terminal: "T1",
    Gate: "112",
    CheckInCounter: "C01-05",
    Status: "Delayed",
    MasterFlightId: "OZ112"
  },
  {
    Type: "Departure",
    FlightId: "ST-992",
    Airline: "델타항공",
    Airport: "시애틀 (SEA)",
    AirportCode: "SEA",
    ScheduleDate: "2024-05-20",
    ScheduleTime: "16:45",
    EstimatedTime: "16:45",
    Terminal: "T2",
    Gate: "250",
    CheckInCounter: "F01-10",
    Status: "On Time",
    MasterFlightId: "DL198"
  },
  {
    Type: "Departure",
    FlightId: "ST-505",
    Airline: "에어프랑스",
    Airport: "파리 (CDG)",
    AirportCode: "CDG",
    ScheduleDate: "2024-05-21",
    ScheduleTime: "09:00",
    EstimatedTime: "08:50",
    Terminal: "T2",
    Gate: "210",
    CheckInCounter: "B10-12",
    Status: "Scheduled",
    MasterFlightId: "AF267"
  },
  {
    Type: "Departure",
    FlightId: "ST-101",
    Airline: "진에어",
    Airport: "제주 (CJU)",
    AirportCode: "CJU",
    ScheduleDate: "2024-05-21",
    ScheduleTime: "07:30",
    EstimatedTime: "07:30",
    Terminal: "T1",
    Gate: "15",
    CheckInCounter: "G01-04",
    Status: "Cancelled",
    MasterFlightId: ""
  }
]);

const filteredFlights = computed(() => {
  return flights.value.filter(flight => {
    const term = searchTerm.value.toLowerCase();
    const matchesSearch = 
      flight.FlightId.toLowerCase().includes(term) ||
      flight.Airline.toLowerCase().includes(term) ||
      flight.AirportCode.toLowerCase().includes(term);
    
    const matchesStatus = activeTab.value === 'All' || flight.Status === activeTab.value;

    return matchesSearch && matchesStatus;
  });
});

const TABS = [
  { id: 'All', label: '전체' },
  { id: 'On Time', label: '정시' },
  { id: 'Delayed', label: '지연' },
  { id: 'Boarding', label: '탑승중' },
  { id: 'Scheduled', label: '예정' },
  { id: 'Cancelled', label: '결항' },
];

// TODO: onMounted 시점에 API를 호출하여 flights.value를 실제 데이터로 채우는 로직 추가
</script>

<style>
/* For Webkit-based browsers (Chrome, Safari) */
.no-scrollbar::-webkit-scrollbar {
    display: none;
}

/* For IE, Edge */
.no-scrollbar {
    -ms-overflow-style: none;  
/* For Firefox */
    scrollbar-width: none;  
}
</style>
