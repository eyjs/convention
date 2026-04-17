<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 헤더 -->
    <div class="sticky top-0 z-40 bg-white shadow-sm">
      <div class="px-4 py-3 flex items-center gap-3">
        <button class="p-1 -ml-1 rounded-lg hover:bg-gray-100" @click="router.back()">
          <svg class="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <h1 class="text-lg font-bold text-gray-900 truncate">{{ banner?.title || '상세 보기' }}</h1>
      </div>
    </div>

    <!-- 로딩 -->
    <div v-if="loading" class="flex items-center justify-center py-20">
      <div class="w-8 h-8 border-3 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
    </div>

    <!-- 콘텐츠: 이미지 이어붙이기 -->
    <div v-else-if="detailImages.length > 0" class="max-w-2xl mx-auto">
      <img
        v-for="(img, idx) in detailImages"
        :key="idx"
        :src="img"
        class="w-full block"
        :alt="`상세 이미지 ${idx + 1}`"
        loading="lazy"
      />
    </div>

    <!-- 이미지 없음 -->
    <div v-else class="text-center py-20 text-gray-400">
      <p>상세 콘텐츠가 없습니다.</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import apiClient from '@/services/api'

const route = useRoute()
const router = useRouter()
const banner = ref(null)
const loading = ref(true)

const detailImages = computed(() => {
  if (!banner.value?.detailImagesJson) return []
  try {
    return JSON.parse(banner.value.detailImagesJson)
  } catch {
    return []
  }
})

onMounted(async () => {
  try {
    const res = await apiClient.get(`/home-banners/${route.params.id}`)
    banner.value = res.data
  } catch {
    banner.value = null
  } finally {
    loading.value = false
  }
})
</script>
