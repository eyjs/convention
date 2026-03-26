<template>
  <div id="app" class="bg-gray-50">
    <router-view />
    <GlobalPopup />
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useKeyboardAdjust } from '@/composables/useKeyboardAdjust'
import GlobalPopup from '@/components/common/GlobalPopup.vue'

const authStore = useAuthStore()

useKeyboardAdjust({
  offset: 20,
  duration: 300,
  enabled: false,
})

onMounted(() => {
  authStore.ensureInitialized()
})
</script>

<style>
#app {
  width: 100%;
  min-height: 100vh;
  min-height: 100dvh;
}
</style>
