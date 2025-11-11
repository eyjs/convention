<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader title="장소 안내" :show-back="true" />
    <div class="max-w-2xl mx-auto px-4 py-6">
      <p class="text-gray-600">장소 안내 페이지입니다.</p>
    </div>
  </div>
</template>

<script setup>
import MainHeader from '@/components/common/MainHeader.vue'
</script>
