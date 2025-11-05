<template>
  <div class="bg-white border-b border-gray-200 sticky top-[73px] z-40">
    <div class="px-4">
      <div class="flex space-x-0 -mb-px">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          :class="[
            'flex-1 py-4 px-2 text-sm font-medium border-b-2 transition-colors duration-200',
            activeTab === tab.id
              ? 'border-ifa-green text-ifa-green'
              : 'border-transparent text-gray-500 hover:text-gray-700',
          ]"
          @click="setActiveTab(tab.id)"
        >
          {{ tab.name }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useConventionStore } from '@/stores/convention'
import { storeToRefs } from 'pinia'

const conventionStore = useConventionStore()
const { activeTab } = storeToRefs(conventionStore)

const tabs = [
  { id: '나의일정', name: '나의일정' },
  { id: '공지사항', name: '공지사항' },
  { id: '투어정보', name: '투어정보' },
  { id: '사진첩', name: '사진첩' },
]

const setActiveTab = (tabId) => {
  conventionStore.setActiveTab(tabId)

  // 탭별 데이터 로드
  const conventionId = conventionStore.getCurrentConvention?.id
  if (!conventionId) return

  switch (tabId) {
    case '공지사항':
      conventionStore.fetchNotices(conventionId)
      break
    case '투어정보':
      conventionStore.fetchTourInfo(conventionId)
      break
    case '사진첩':
      conventionStore.fetchPhotos(conventionId)
      break
  }
}
</script>
