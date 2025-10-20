<template>
  <div class="dynamic-feature-loader">
    <component 
      v-if="!loadError"
      :is="featureComponent" 
      :feature-metadata="featureMetadata"
    />
    
    <div v-else class="feature-error flex flex-col items-center justify-center min-h-[400px] gap-4 p-8 text-center">
      <h3 class="text-xl font-semibold text-red-600">⚠️ {{ loadError }}</h3>
      <p class="text-gray-600 max-w-md">요청하신 기능을 찾을 수 없거나 로드하는 중 문제가 발생했습니다.</p>
      <router-link to="/features" class="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
        기능 목록으로 돌아가기
      </router-link>
    </div>
  </div>
</template>

<script setup>
import { computed, defineAsyncComponent, ref } from 'vue'
import { useFeatureStore } from '@/stores/feature'

const props = defineProps({
  featureName: {
    type: String,
    required: true
  }
})

const featureStore = useFeatureStore()
const loadError = ref(null)

const formatFeatureName = (name) => {
  return name
    .split('-')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join('')
}

const featureComponent = computed(() => {
  const formattedName = formatFeatureName(props.featureName)
  const folderName = `${formattedName}Feature`
  const fileName = `${formattedName}Page`
  
  return defineAsyncComponent({
    loader: () => 
      import(`@/dynamic-features/${folderName}/views/${fileName}.vue`)
        .catch(err => {
          console.error(`Failed to load feature: ${props.featureName}`, err)
          loadError.value = `기능을 불러올 수 없습니다: ${props.featureName}`
          throw err
        }),
    
    loadingComponent: {
      template: `
        <div class="flex flex-col items-center justify-center min-h-[400px] gap-4">
          <div class="w-12 h-12 border-4 border-gray-200 border-t-blue-600 rounded-full animate-spin"></div>
          <p class="text-gray-600">기능을 불러오는 중...</p>
        </div>
      `
    },
    
    errorComponent: {
      template: `
        <div class="flex flex-col items-center justify-center min-h-[400px] gap-4 p-8 text-center">
          <h3 class="text-xl font-semibold text-red-600">⚠️ 기능을 불러올 수 없습니다</h3>
          <p class="text-gray-600">요청하신 기능을 찾을 수 없습니다.</p>
        </div>
      `
    },
    
    delay: 200,
    timeout: 10000
  })
})

const featureMetadata = computed(() => {
  return featureStore.getFeatureByUrl(props.featureName)
})
</script>
