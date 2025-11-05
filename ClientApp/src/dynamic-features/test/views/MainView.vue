<template>
  <div class="test-page p-6">
    <div class="bg-white rounded-lg shadow-lg p-8">
      <h2 class="text-2xl font-bold text-gray-900 mb-4">✅ 테스트 기능</h2>
      <p class="text-gray-600 mb-6">동적 기능이 정상적으로 로드되었습니다!</p>

      <div class="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
        <h3 class="font-semibold text-blue-900 mb-2">기능 정보</h3>
        <div v-if="featureMetadata" class="text-sm text-blue-800">
          <p><strong>메뉴명:</strong> {{ featureMetadata.menuName }}</p>
          <p><strong>URL:</strong> {{ featureMetadata.menuUrl }}</p>
          <p>
            <strong>활성화:</strong>
            {{ featureMetadata.isActive ? '예' : '아니오' }}
          </p>
        </div>
      </div>

      <div class="space-y-4">
        <button
          @click="count++"
          class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
        >
          클릭 횟수: {{ count }}
        </button>

        <div class="text-sm text-gray-500">
          <p>이 페이지는 다음 경로에서 로드되었습니다:</p>
          <code class="block mt-2 p-2 bg-gray-100 rounded">
            @/dynamic-features/TestFeature/views/TestPage.vue
          </code>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  featureMetadata: {
    type: Object,
    default: () => ({}),
  },
})

const count = ref(0)

onMounted(() => {
  console.log('TestPage mounted', props.featureMetadata)
})

onUnmounted(() => {
  console.log('TestPage unmounted')
})
</script>

<style scoped>
.test-page {
  max-width: 800px;
  margin: 0 auto;
}
</style>
