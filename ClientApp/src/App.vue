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

const route = useRoute()
const authStore = useAuthStore()

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
