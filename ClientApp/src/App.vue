<template>
  <div id="app" class="bg-gray-50">
    <router-view />
    <GlobalPopup />
  </div>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useUIStore } from '@/stores/ui'
import { useKeyboardAdjust } from '@/composables/useKeyboardAdjust'
import GlobalPopup from '@/components/common/GlobalPopup.vue'

const authStore = useAuthStore()
const uiStore = useUIStore()

useKeyboardAdjust({
  offset: 20,
  duration: 300,
  enabled: false,
})

const onKeyDown = (e) => {
  if (e.key === 'Escape') {
    uiStore.closeTopModal()
  }
}

onMounted(() => {
  authStore.ensureInitialized()
  window.addEventListener('keydown', onKeyDown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', onKeyDown)
})
</script>

<style>
#app {
  width: 100%;
  min-height: 100vh;
  min-height: 100dvh;
}
</style>
