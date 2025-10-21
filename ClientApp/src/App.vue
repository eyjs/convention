<template>
  <div id="app">
    <component v-if="currentLayout" :is="currentLayout">
      <router-view />
    </component>
    <router-view v-else />
  </div>
</template>

<script setup>
import { computed, defineAsyncComponent, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useKeyboardAdjust } from '@/composables/useKeyboardAdjust'

const route = useRoute()
const authStore = useAuthStore()

// 전역 키보드 대응 활성화
const { isKeyboardVisible } = useKeyboardAdjust({
  offset: 20,        // 키보드 위 여백 (px)
  duration: 300,     // 스크롤 애니메이션 시간 (ms)
  enabled: true      // 항상 활성화
})

onMounted(() => {
  authStore.initAuth()
  if (authStore.isAuthenticated) {
    authStore.fetchCurrentUser()
  }
})

const currentLayout = computed(() => {
  const layoutName = route.meta.layout
  
  if (!layoutName) {
    return null
  }
  
  return defineAsyncComponent(() => 
    import(`@/layouts/${layoutName}.vue`)
  )
})
</script>

<style>
#app {
  width: 100%;
  min-height: 100vh;
}
</style>
