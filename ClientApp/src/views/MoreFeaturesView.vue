<template>
  <div class="more-features-view max-w-6xl mx-auto p-4">
    <div class="page-header mb-8">
      <h1 class="text-3xl font-bold text-gray-900 mb-2">추가 기능</h1>
      <p class="text-gray-600">이 행사에서 사용 가능한 추가 기능들입니다</p>
    </div>

    <div v-if="isLoading" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
      <div class="w-12 h-12 border-4 border-gray-200 border-t-blue-600 rounded-full animate-spin"></div>
      <p class="text-gray-600">기능 목록을 불러오는 중...</p>
    </div>

    <div v-else-if="activeFeatures.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <div
        v-for="feature in activeFeatures"
        :key="feature.id"
        class="feature-card bg-white border border-gray-200 rounded-xl p-6 cursor-pointer hover:shadow-lg hover:-translate-y-1 transition-all"
        :class="{ 'opacity-50 cursor-not-allowed': !feature.isActive }"
        @click="navigateToFeature(feature)"
      >
        <div class="flex items-start gap-4">
          <div class="flex-shrink-0">
            <img
              v-if="feature.iconUrl"
              :src="feature.iconUrl"
              :alt="feature.menuName"
              class="w-12 h-12 object-contain"
            />
            <div v-else class="w-12 h-12 bg-gray-100 rounded-lg flex items-center justify-center text-2xl">
              📦
            </div>
          </div>

          <div class="flex-1">
            <h3 class="text-lg font-semibold text-gray-900 mb-1">{{ feature.menuName }}</h3>
            <span v-if="!feature.isActive" class="text-sm text-red-600">비활성화됨</span>
          </div>

          <div class="text-2xl text-gray-400">→</div>
        </div>
      </div>
    </div>

    <div v-else class="flex flex-col items-center justify-center min-h-[300px] gap-4 text-center">
      <p class="text-lg text-gray-600">현재 사용 가능한 추가 기능이 없습니다.</p>
      <button @click="router.push('/')" class="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
        홈으로 돌아가기
      </button>
    </div>
  </div>
</template>

<script setup>
import { onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useFeatureStore } from '@/stores/feature'

const router = useRouter()
const featureStore = useFeatureStore()

onMounted(async () => {
  const conventionId = localStorage.getItem('selectedConventionId')
  if (conventionId) {
    await featureStore.fetchActiveFeatures(conventionId)
  }
})

const activeFeatures = computed(() => featureStore.activeFeatures)
const isLoading = computed(() => featureStore.loading)

const navigateToFeature = (feature) => {
  if (!feature.isActive) return
  
  router.push({
    name: 'DynamicFeature',
    params: { featureName: feature.menuUrl }
  })
}
</script>
