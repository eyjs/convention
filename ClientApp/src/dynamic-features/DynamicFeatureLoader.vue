<template>
  <div v-if="isLoading" class="min-h-screen flex items-center justify-center">
    <div class="text-center">
      <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      <p class="mt-4 text-gray-600">로딩 중...</p>
    </div>
  </div>

  <div v-else-if="error" class="min-h-screen flex items-center justify-center">
    <div class="text-center px-4">
      <svg class="w-16 h-16 text-red-500 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
      <h2 class="text-xl font-bold text-gray-900 mb-2">기능을 불러올 수 없습니다</h2>
      <p class="text-gray-600 mb-6">{{ error }}</p>
      <button @click="goBack" class="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
        돌아가기
      </button>
    </div>
  </div>

  <component v-else-if="featureComponent" :is="featureComponent" />
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { loadFeature } from './registry'

const props = defineProps({
  featureName: {
    type: String,
    required: true
  }
})

const route = useRoute()
const router = useRouter()

const isLoading = ref(true)
const error = ref(null)
const featureComponent = ref(null)

async function loadFeatureModule() {
  isLoading.value = true
  error.value = null
  featureComponent.value = null

  try {
    console.log('Loading feature:', props.featureName)
    
    // registry에서 기능 모듈 로드
    const featureModule = await loadFeature(props.featureName)
    
    if (!featureModule || !featureModule.component) {
      throw new Error('기능 컴포넌트를 찾을 수 없습니다')
    }

    // 컴포넌트 로드
    const component = await featureModule.component()
    featureComponent.value = component.default || component
    
    console.log('Feature loaded successfully:', props.featureName)
  } catch (err) {
    console.error('Failed to load feature:', err)
    error.value = err.message || '기능을 불러오는데 실패했습니다'
  } finally {
    isLoading.value = false
  }
}

function goBack() {
  router.back()
}

onMounted(loadFeatureModule)

// featureName이 변경되면 다시 로드
watch(() => props.featureName, loadFeatureModule)
</script>
