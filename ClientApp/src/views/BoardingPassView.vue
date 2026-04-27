<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 헤더 -->
    <div class="bg-white border-b px-4 py-3 flex items-center gap-3">
      <button
        class="p-2 -ml-2 rounded-lg active:bg-gray-100"
        @click="$router.back()"
      >
        <svg
          class="w-5 h-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M15 19l-7-7 7-7"
          />
        </svg>
      </button>
      <h1 class="text-lg font-semibold">내 탑승권</h1>
    </div>

    <!-- 로딩 -->
    <div v-if="loading" class="flex items-center justify-center py-20">
      <div
        class="w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"
      ></div>
    </div>

    <!-- 탑승권 없음 -->
    <div
      v-else-if="!boardingPassUrl"
      class="flex flex-col items-center justify-center py-20 px-4 text-center"
    >
      <svg
        class="w-16 h-16 text-gray-300 mb-4"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="1.5"
          d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
        />
      </svg>
      <p class="text-gray-500 text-lg font-medium">등록된 탑승권이 없습니다</p>
      <p class="text-gray-400 text-sm mt-1">
        관리자가 탑승권을 등록하면 이곳에서 확인할 수 있습니다
      </p>
    </div>

    <!-- PDF 뷰어 -->
    <div v-else class="p-4">
      <div class="bg-white rounded-lg shadow overflow-hidden">
        <iframe
          :src="boardingPassUrl"
          class="w-full border-0"
          style="height: calc(100vh - 140px)"
          title="탑승권 PDF"
        ></iframe>
      </div>
      <div class="mt-3 flex gap-2">
        <a
          :href="boardingPassUrl"
          target="_blank"
          class="flex-1 py-3 bg-primary-600 text-white rounded-lg text-center text-sm font-medium active:bg-primary-700"
        >
          새 탭에서 열기
        </a>
        <a
          :href="boardingPassUrl"
          download
          class="flex-1 py-3 bg-white border border-gray-300 text-gray-700 rounded-lg text-center text-sm font-medium active:bg-gray-50"
        >
          다운로드
        </a>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useConventionStore } from '@/stores/convention'
import apiClient from '@/services/api'

const conventionStore = useConventionStore()
const loading = ref(true)
const boardingPassUrl = ref(null)

onMounted(async () => {
  const conventionId = conventionStore.selectedConventionId
  if (!conventionId) {
    loading.value = false
    return
  }
  try {
    const res = await apiClient.get(
      `/conventions/${conventionId}/my-boarding-pass`,
    )
    boardingPassUrl.value = res.data.url
  } catch {
    boardingPassUrl.value = null
  } finally {
    loading.value = false
  }
})
</script>
