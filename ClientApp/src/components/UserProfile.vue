<template>
  <div class="bg-white border-b border-gray-200">
    <div class="px-4 py-6">
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-lg font-bold text-gray-800">
            {{ currentUser?.guestName || '정유진' }} 님
          </h2>
          <p class="text-sm text-gray-500 mt-1">
            {{ currentConvention?.title || '컨벤션 정보를 불러오는 중...' }}
          </p>
        </div>
        
        <button 
          class="text-sm text-ifa-green font-medium touch-feedback px-3 py-1 rounded"
          @click="showProfile"
        >
          로그아웃
        </button>
      </div>
      
      <!-- 컨벤션 기간 정보 -->
      <div 
        v-if="currentConvention" 
        class="mt-3 text-xs text-gray-400"
      >
        {{ formatConventionPeriod }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useConventionStore } from '@/stores/convention'
import dayjs from 'dayjs'

const conventionStore = useConventionStore()
const currentConvention = computed(() => conventionStore.getCurrentConvention)

// 임시 사용자 정보 (실제로는 auth store에서 가져옴)
const currentUser = computed(() => ({
  guestName: '정유진',
  corpPart: '개발팀'
}))

const formatConventionPeriod = computed(() => {
  if (!currentConvention.value) return ''
  
  const start = dayjs(currentConvention.value.startDate)
  const end = dayjs(currentConvention.value.endDate)
  
  return `${start.format('YYYY.MM.DD')} - ${end.format('YYYY.MM.DD')}`
})

const showProfile = () => {
  // 프로필 모달 표시 또는 로그아웃 처리
  if (confirm('로그아웃 하시겠습니까?')) {
    // 로그아웃 처리
    localStorage.removeItem('auth_token')
    window.location.reload()
  }
}
</script>
