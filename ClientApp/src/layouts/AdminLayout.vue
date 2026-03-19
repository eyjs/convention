<template>
  <div
    class="min-h-screen min-h-dvh bg-gray-50"
    :class="{ 'flex flex-col h-screen': route.meta.adminFullScreen }"
  >
    <AdminHeader
      v-if="!route.meta.hideAdminHeader"
      :title="headerTitle"
      :subtitle="headerSubtitle"
      :show-back-button="!!route.meta.adminBackPath"
      :back-path="route.meta.adminBackPath || '/admin'"
    >
      <div id="admin-header-slot"></div>
    </AdminHeader>
    <router-view />
  </div>
</template>

<script setup>
import { ref, computed, provide, watch } from 'vue'
import { useRoute } from 'vue-router'
import AdminHeader from '@/components/admin/AdminHeader.vue'

const route = useRoute()

const titleOverride = ref('')
const subtitleOverride = ref('')

// 라우트 변경 시 override 초기화 (파라미터 변경 포함)
watch(
  () => route.fullPath,
  () => {
    titleOverride.value = ''
    subtitleOverride.value = ''
  },
)

const headerTitle = computed(
  () => titleOverride.value || route.meta.adminTitle || '관리자',
)
const headerSubtitle = computed(() => subtitleOverride.value || '')

provide('adminHeader', {
  setTitle: (t) => {
    titleOverride.value = t
  },
  setSubtitle: (s) => {
    subtitleOverride.value = s
  },
})
</script>
