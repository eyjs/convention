<template>
  <div class="min-h-screen relative bg-gray-50">
    <!-- Decorative Background Elements -->
    <div class="fixed inset-0 z-0 overflow-hidden pointer-events-none">
      <!-- Large gradient blobs -->
      <div class="absolute -top-32 -left-32 w-80 h-80 bg-gradient-to-br from-rose-200/15 to-orange-200/15 rounded-full blur-3xl"></div>
      <div class="absolute top-1/2 -right-40 w-96 h-96 bg-gradient-to-br from-orange-200/12 to-amber-200/12 rounded-full blur-3xl"></div>
      <div class="absolute bottom-32 left-1/4 w-72 h-72 bg-gradient-to-br from-amber-200/12 to-rose-200/12 rounded-full blur-3xl"></div>

      <!-- Subtle pattern -->
      <div class="absolute inset-0 opacity-[0.02]" style="background-image: url('data:image/svg+xml,%3Csvg width=&quot;60&quot; height=&quot;60&quot; viewBox=&quot;0 0 60 60&quot; xmlns=&quot;http://www.w3.org/2000/svg&quot;%3E%3Cg fill=&quot;none&quot; fill-rule=&quot;evenodd&quot;%3E%3Cg fill=&quot;%239C92AC&quot; fill-opacity=&quot;1&quot;%3E%3Cpath d=&quot;M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z&quot;/%3E%3C/g%3E%3C/g%3E%3C/svg%3E');"></div>
    </div>

    <div class="relative z-10">
      <MainHeader title="스타투어" :show-back="false" />

      <div class="max-w-2xl mx-auto px-4 py-4">
        <!-- 로딩 상태 -->
        <div v-if="loading" class="text-center py-16">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-rose-600 mx-auto"></div>
          <p class="mt-4 text-gray-600 font-medium">로딩 중...</p>
        </div>

        <!-- 스타투어 목록 -->
        <template v-else>
          <!-- Empty State -->
          <div v-if="conventions.length === 0" class="text-center py-16">
            <div class="w-24 h-24 mx-auto mb-6 bg-rose-100 rounded-full flex items-center justify-center">
              <svg class="w-12 h-12 text-rose-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
            </div>
            <h3 class="text-xl font-bold text-gray-900 mb-2">참여 중인 스타투어가 없습니다</h3>
            <p class="text-gray-500 mb-6">스타투어에 참여해보세요!</p>
          </div>

          <!-- 스타투어 카드 목록 -->
          <div v-else class="space-y-5">
            <div
              v-for="convention in conventions"
              :key="convention.id"
              @click="goToConvention(convention)"
              class="group bg-white rounded-2xl shadow-md hover:shadow-2xl transition-shadow cursor-pointer overflow-hidden w-full">
              <!-- 카드 헤더 (이미지 영역) -->
              <div class="relative h-40 overflow-hidden">
                <div v-if="convention.conventionImg"
                     class="absolute inset-0 bg-cover bg-center pointer-events-none"
                     :style="{ backgroundImage: `url(${convention.conventionImg})` }">
                </div>
                <div v-else class="absolute inset-0 bg-gradient-to-br from-rose-500 via-orange-500 to-amber-500 pointer-events-none">
                  <div class="absolute inset-0 flex items-center justify-center">
                    <svg class="w-16 h-16 text-white/30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                    </svg>
                  </div>
                  <!-- Decorative elements -->
                  <div class="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -mr-16 -mt-16"></div>
                  <div class="absolute bottom-0 left-0 w-24 h-24 bg-white/10 rounded-full -ml-12 -mb-12"></div>
                </div>
              </div>

              <!-- 카드 본문 -->
              <div class="p-5">
                <h3 class="text-xl font-bold text-gray-900 mb-2 group-hover:text-rose-600 transition-colors">{{ convention.title }}</h3>

                <!-- 날짜 -->
                <div class="flex items-center gap-2 text-sm text-gray-500">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  <span class="font-medium">{{ formatDate(convention.startDate) }} ~ {{ formatDate(convention.endDate) }}</span>
                </div>
              </div>
            </div>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useConventionStore } from '@/stores/convention'
import MainHeader from '@/components/common/MainHeader.vue'
import apiClient from '@/services/api'

const router = useRouter()
const conventionStore = useConventionStore()
const loading = ref(true)
const conventions = ref([])

async function loadConventions() {
  loading.value = true
  try {
    const response = await apiClient.get(`/users/conventions?_=${new Date().getTime()}`)
    conventions.value = response.data
  } catch (error) {
    console.error('스타투어 목록 로드 실패:', error)
    alert('스타투어 목록을 불러올 수 없습니다.')
  } finally {
    loading.value = false
  }
}

async function goToConvention(convention) {
  // [방어] convention.id가 유효한지 확인
  if (!convention || !convention.id || convention.id === 'undefined') {
    console.warn('Invalid convention:', convention)
    return
  }
  await conventionStore.selectConvention(convention.id)
  router.push('/')
}

// 날짜 포맷
function formatDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('ko-KR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  })
}

onMounted(loadConventions)
</script>
