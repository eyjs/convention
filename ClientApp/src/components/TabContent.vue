<template>
  <div class="px-4 py-2">
    <!-- 나의일정 탭 -->
    <MyScheduleTab 
      v-if="activeTab === '나의일정'" 
    />
    
    <!-- 공지사항 탭 -->
    <NoticeTab 
      v-else-if="activeTab === '공지사항'" 
    />
    
    <!-- 투어정보 탭 -->
    <TourInfoTab 
      v-else-if="activeTab === '투어정보'" 
    />
    
    <!-- 사진첩 탭 -->
    <PhotoTab 
      v-else-if="activeTab === '사진첩'" 
    />
  </div>
</template>

<script setup>
import { useConventionStore } from '@/stores/convention'
import { storeToRefs } from 'pinia'
import MyScheduleTab from '@/components/tabs/MyScheduleTab.vue'
import NoticeTab from '@/components/tabs/NoticeTab.vue'
import TourInfoTab from '@/components/tabs/TourInfoTab.vue'
import PhotoTab from '@/components/tabs/PhotoTab.vue'

const conventionStore = useConventionStore()
const { activeTab } = storeToRefs(conventionStore)
</script>
