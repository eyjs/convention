<template>
  <div class="bg-white rounded-3xl shadow-[0_8px_30px_rgb(0,0,0,0.04)] overflow-hidden mb-5 border border-gray-100 relative group w-full max-w-md mx-auto">
    
    <!-- 워터마크 로고 영역 (Background Watermark) -->
    <div class="absolute inset-0 flex items-center justify-center pointer-events-none overflow-hidden z-0">
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="w-64 h-64 text-indigo-900 opacity-[0.03] transform -rotate-12">
        <path d="M2 12h20"/><path d="M13 2l9 10-9 10"/><path d="M2 12l5 5m-5-5l5-5"/>
      </svg>
    </div>

    <!-- 카드 상단: 상태 바 -->
    <div class="h-1.5 w-full relative z-10" :class="statusConfig.bg"></div>

    <div class="p-5 relative z-10">
      <!-- 헤더: 항공사 정보 및 상태 -->
      <div class="flex justify-between items-start mb-6">
        <div class="flex items-center gap-3">
          <!-- 항공사 로고 플레이스홀더 -->
          <div class="w-10 h-10 rounded-xl bg-gray-50 flex items-center justify-center text-xs font-bold text-gray-700 shadow-sm border border-gray-100">
            {{ airlineLogoText }}
          </div>
          <div>
            <h3 class="font-bold text-gray-900 text-sm tracking-tight">{{ data.Airline }}</h3>
            <div class="flex items-center gap-2 text-xs text-gray-500 mt-0.5">
              <span class="font-medium text-gray-600">{{ data.FlightId }}</span>
              <template v-if="data.MasterFlightId">
                <span class="w-1 h-1 rounded-full bg-gray-300"></span>
                <span class="text-gray-400">공동운항 {{ data.MasterFlightId }}</span>
              </template>
            </div>
          </div>
        </div>
        
        <!-- 상태 뱃지 -->
        <div class="px-2.5 py-1 rounded-full text-[11px] font-bold tracking-tight flex items-center gap-1.5 shadow-sm"
             :class="[statusConfig.badgeBg, statusConfig.text]">
          <!-- 아이콘 스위칭 -->
          <svg v-if="statusConfig.iconName === 'door-open'" xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M13 4h3a2 2 0 0 1 2 2v14"/><path d="M2 20h3"/><path d="M13 20h9"/><path d="M10 12v.01"/><path d="M13 4.562v16.157a1 1 0 0 1-1.242.97L5 20V5.562a2 2 0 0 1 1.515-1.94l4-1A2 2 0 0 1 13 4.561Z"/></svg>
          <svg v-else-if="statusConfig.iconName === 'alert-circle'" xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" x2="12" y1="8" y2="12"/><line x1="12" x2="12.01" y1="16" y2="16"/></svg>
          <svg v-else-if="statusConfig.iconName === 'check-circle'" xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
          <svg v-else xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
          
          {{ statusConfig.label }}
        </div>
      </div>

      <!-- 메인: 경로 및 시간 (Timeline) -->
      <div class="flex items-center justify-between mb-6 px-1">
        <!-- 출발지 -->
        <div class="text-left">
          <div class="text-3xl font-black text-gray-800 tracking-tight">ICN</div>
          <div class="text-xs font-bold text-gray-500 mt-1">10:30</div> 
        </div>

        <!-- 비행 아이콘 & 그래픽 -->
        <div class="flex-1 px-4 flex flex-col items-center">
           <div class="text-[10px] text-gray-400 font-medium mb-1">직항</div>
           <div class="w-full h-px bg-gray-200 relative flex items-center justify-center">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-indigo-400 rotate-90 bg-white p-0.5 absolute"><path d="M2 12h20"/><path d="M13 2l9 10-9 10"/><path d="M2 12l5 5m-5-5l5-5"/></svg>
              <div class="w-1.5 h-1.5 rounded-full bg-gray-300 absolute left-0"></div>
              <div class="w-1.5 h-1.5 rounded-full bg-gray-300 absolute right-0"></div>
           </div>
           <div class="text-[10px] text-gray-400 font-medium mt-1">비행 시간</div>
        </div>

        <!-- 도착지 -->
        <div class="text-right">
           <div class="text-3xl font-black tracking-tight" :class="isDelayed ? 'text-red-500' : 'text-indigo-600'">
             {{ data.AirportCode }}
           </div>
           <div class="flex flex-col items-end mt-1">
             <span class="text-xs font-bold" :class="isDelayed ? 'text-red-500 animate-pulse' : 'text-gray-800'">
                {{ isDelayed ? data.EstimatedTime : data.ScheduleTime }}
             </span>
             <span v-if="isDelayed" class="text-[10px] text-gray-400 line-through">{{ data.ScheduleTime }}</span>
           </div>
        </div>
      </div>

      <!-- 상세 정보 그리드 카드 -->
      <div class="bg-gray-50/80 backdrop-blur-[2px] rounded-2xl p-4 grid grid-cols-2 gap-y-4 gap-x-2 border border-gray-100/50">
          
          <!-- 날짜 -->
          <div class="flex flex-col">
            <span class="text-[10px] text-gray-400 font-bold mb-1 flex items-center gap-1">
              <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
              날짜
            </span>
            <span class="text-sm font-bold text-gray-700">{{ data.ScheduleDate }}</span>
          </div>

          <!-- 게이트 (강조) -->
          <div class="flex flex-col border-l border-gray-200 pl-4">
            <span class="text-[10px] text-gray-400 font-bold mb-1 flex items-center gap-1">
              <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M13 4h3a2 2 0 0 1 2 2v14"/><path d="M2 20h3"/><path d="M13 20h9"/><path d="M10 12v.01"/><path d="M13 4.562v16.157a1 1 0 0 1-1.242.97L5 20V5.562a2 2 0 0 1 1.515-1.94l4-1A2 2 0 0 1 13 4.561Z"/></svg>
              탑승구
            </span>
            <span class="text-sm font-bold text-indigo-600">{{ data.Gate || '미정' }}</span>
          </div>

          <!-- 터미널 -->
          <div class="flex flex-col">
            <span class="text-[10px] text-gray-400 font-bold mb-1 flex items-center gap-1">
              <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="4" y="2" width="16" height="20" rx="2" ry="2"/><path d="M9 22v-4h6v4"/><path d="M8 6h.01"/><path d="M16 6h.01"/><path d="M12 6h.01"/><path d="M12 10h.01"/><path d="M12 14h.01"/><path d="M16 10h.01"/><path d="M16 14h.01"/><path d="M8 10h.01"/><path d="M8 14h.01"/></svg>
              터미널
            </span>
            <span class="text-sm font-bold text-gray-700">{{ data.Terminal }}</span>
          </div>

          <!-- 체크인 카운터 -->
          <div class="flex flex-col border-l border-gray-200 pl-4">
            <span class="text-[10px] text-gray-400 font-bold mb-1 flex items-center gap-1">
              <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
              카운터
            </span>
            <span class="text-sm font-bold text-gray-700">{{ data.CheckInCounter }}</span>
          </div>
      </div>
      
      <!-- 하단 도시 이름 -->
      <div class="mt-4 flex items-center justify-center">
          <div class="text-xs text-gray-500 font-medium bg-white/90 px-3 py-1 rounded-full border border-gray-100 shadow-sm flex items-center gap-1 backdrop-blur-sm">
            <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-indigo-400"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
            {{ arrivalCity }} 도착
          </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  data: {
    type: Object,
    required: true,
    // 예상 데이터 구조:
    // {
    //   Type: "Departure",
    //   FlightId: "ST-701",
    //   Airline: "대한항공",
    //   Airport: "뉴욕 (JFK)",
    //   AirportCode: "JFK",
    //   ScheduleDate: "2024-05-20",
    //   ScheduleTime: "10:30",
    //   EstimatedTime: "10:30",
    //   Terminal: "T2",
    //   Gate: "234",
    //   CheckInCounter: "A12-15",
    //   Status: "Boarding",
    //   MasterFlightId: "KE081"
    // }
  }
});

// 상태값에 따른 스타일 및 라벨 설정
const statusConfig = computed(() => {
  const status = props.data.Status?.toLowerCase() || '';
  
  switch (status) {
    case 'boarding':
      return { 
        label: '탑승중', 
        bg: 'bg-green-500', 
        text: 'text-green-600', 
        badgeBg: 'bg-green-100',
        iconName: 'door-open'
      };
    case 'delayed':
      return { 
        label: '지연', 
        bg: 'bg-red-500', 
        text: 'text-red-600', 
        badgeBg: 'bg-red-100',
        iconName: 'alert-circle'
      };
    case 'on time':
      return { 
        label: '정시', 
        bg: 'bg-blue-500', 
        text: 'text-blue-600', 
        badgeBg: 'bg-blue-100',
        iconName: 'check-circle'
      };
    case 'cancelled':
      return { 
        label: '결항', 
        bg: 'bg-gray-500', 
        text: 'text-gray-600', 
        badgeBg: 'bg-gray-200',
        iconName: 'alert-circle'
      };
    case 'scheduled':
      return { 
        label: '예정', 
        bg: 'bg-indigo-500', 
        text: 'text-indigo-600', 
        badgeBg: 'bg-indigo-100',
        iconName: 'clock'
      };
    default:
      return { 
        label: props.data.Status, 
        bg: 'bg-indigo-500', 
        text: 'text-indigo-600', 
        badgeBg: 'bg-indigo-100',
        iconName: 'clock'
      };
  }
});

const isDelayed = computed(() => props.data.Status?.toLowerCase() === 'delayed');
const arrivalCity = computed(() => props.data.Airport ? props.data.Airport.split('(')[0].trim() : '');

// 항공사 로고 텍스트 (앞 2글자)
const airlineLogoText = computed(() => props.data.Airline ? props.data.Airline.substring(0, 2).toUpperCase() : '');
</script>