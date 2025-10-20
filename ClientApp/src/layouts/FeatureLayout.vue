<template>
  <div class="feature-layout min-h-screen bg-white">
    <header class="feature-header sticky top-0 z-10 bg-white border-b border-gray-200">
      <div class="flex items-center px-4 py-3">
        <button @click="goBack" class="back-button flex items-center gap-2 px-3 py-2 text-gray-600 hover:text-gray-900">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
          <span>뒤로</span>
        </button>
        <div class="flex-1 text-center font-semibold text-lg">
          <slot name="header"></slot>
        </div>
        <div class="w-20"></div>
      </div>
    </header>
    
    <main class="feature-content p-4">
      <slot />
    </main>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'

const router = useRouter()

const goBack = () => {
  router.push('/features')
}
</script>

<style scoped>
.feature-layout {
  display: flex;
  flex-direction: column;
}

.feature-content {
  flex: 1;
  overflow-y: auto;
}
</style>
