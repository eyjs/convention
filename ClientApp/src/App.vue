<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <AppHeader />
    
    <!-- Main Content -->
    <div class="pb-20">
      <UserProfile />
      <TabNavigation />
      <TabContent />
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useConventionStore } from '@/stores/convention'
import AppHeader from '@/components/AppHeader.vue'
import UserProfile from '@/components/UserProfile.vue'
import TabNavigation from '@/components/TabNavigation.vue'
import TabContent from '@/components/TabContent.vue'

const conventionStore = useConventionStore()

onMounted(async () => {
  await conventionStore.fetchConventions()
})
</script>

<style>
@import 'tailwindcss/base';
@import 'tailwindcss/components';
@import 'tailwindcss/utilities';

/* iOS Safari 전용 스타일 */
@supports (-webkit-touch-callout: none) {
  .safe-area-top {
    padding-top: env(safe-area-inset-top);
  }
  
  .safe-area-bottom {
    padding-bottom: env(safe-area-inset-bottom);
  }
}

/* 커스텀 스크롤바 */
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
  height: 4px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 2px;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #00B894;
  border-radius: 2px;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #00A085;
}

/* 터치 피드백 */
.touch-feedback:active {
  transform: scale(0.98);
  transition: transform 0.1s ease;
}

/* 타임라인 스타일 */
.timeline-dot {
  @apply w-3 h-3 bg-ifa-green rounded-full border-2 border-white shadow-md;
}

.timeline-line {
  @apply w-0.5 bg-ifa-light-gray absolute left-1/2 transform -translate-x-1/2;
}
</style>
