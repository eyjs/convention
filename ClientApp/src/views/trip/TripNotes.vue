<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader :title="'노트'" :show-back="true" />
    <div class="max-w-2xl mx-auto px-4 py-4 pb-24">
      <!-- Tab Navigation -->
      <div class="mb-4 border-b border-gray-200">
        <nav class="flex space-x-4" aria-label="Tabs">
          <button @click="activeTab = 'photos'"
                  :class="['px-3 py-2 font-medium text-sm rounded-t-lg', activeTab === 'photos' ? 'border-b-2 border-primary-500 text-primary-600' : 'text-gray-500 hover:text-gray-700']">
            사진첩
          </button>
          <button @click="activeTab = 'checklist'"
                  :class="['px-3 py-2 font-medium text-sm rounded-t-lg', activeTab === 'checklist' ? 'border-b-2 border-primary-500 text-primary-600' : 'text-gray-500 hover:text-gray-700']">
            체크리스트
          </button>
        </nav>
      </div>

      <!-- Tab Content -->
      <div v-if="activeTab === 'photos'">
        <div class="text-center py-16">
          <h2 class="text-xl font-bold text-gray-800">사진첩</h2>
          <p class="mt-2 text-gray-600">일자별로 업로드된 이미지를 표시합니다.</p>
        </div>
      </div>
      <div v-if="activeTab === 'checklist'">
        <div class="text-center py-16">
          <h2 class="text-xl font-bold text-gray-800">체크리스트</h2>
          <p class="mt-2 text-gray-600">여행 준비물 등의 체크리스트를 관리합니다.</p>
        </div>
      </div>
    </div>
    <BottomNavigationBar v-if="tripId" :trip-id="tripId" :show="!uiStore.isModalOpen" />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useRoute } from 'vue-router';
import MainHeader from '@/components/common/MainHeader.vue';
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue';
import { useUIStore } from '@/stores/ui';

const uiStore = useUIStore();

const route = useRoute();
const tripId = computed(() => route.params.id);
const activeTab = ref('photos'); // 'photos' or 'checklist'
</script>
