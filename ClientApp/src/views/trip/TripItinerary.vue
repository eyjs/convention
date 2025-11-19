<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader :title="'일정표'" :show-back="true" />
    <div class="max-w-2xl mx-auto px-4 py-4 pb-24">
      <div class="text-center py-20">
        <h1 class="text-2xl font-bold text-gray-800">일정표 페이지</h1>
        <p class="mt-2 text-gray-600">이 페이지는 현재 미정입니다.</p>
        <p class="mt-1 text-sm text-gray-500">Trip ID: {{ tripId }}</p>
      </div>
    </div>
    <BottomNavigationBar v-if="tripId" :trip-id="tripId" :show="!uiStore.isModalOpen" />
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import MainHeader from '@/components/common/MainHeader.vue';
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue';
import { useUIStore } from '@/stores/ui';

const uiStore = useUIStore();

const route = useRoute();
const tripId = computed(() => route.params.id);
</script>
