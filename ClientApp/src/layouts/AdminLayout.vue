<template>
  <div class="app-frame">
    <div class="safe-top bg-gray-50"></div>

    <div
      class="safe-content bg-gray-50"
      :class="{ 'flex flex-col': route.meta.adminFullScreen }"
    >
      <AdminHeader
        v-if="!route.meta.hideAdminHeader"
        :title="headerTitle"
        :subtitle="headerSubtitle"
        @toggle-sidebar="sidebarOpen = !sidebarOpen"
      />

      <!-- Full-screen layout (e.g. FormBuilder editor) -->
      <template v-if="route.meta.adminFullScreen || route.meta.hideAdminHeader">
        <router-view />
      </template>

      <!-- Sidebar layout (default) -->
      <template v-else>
        <div class="flex flex-1">
          <AdminSidebar
            :convention-id="conventionId"
            :open="sidebarOpen"
            @close="sidebarOpen = false"
          />
          <main class="flex-1 md:ml-64 min-h-0">
            <div class="px-4 py-6 sm:px-6 lg:px-8">
              <router-view />
            </div>
          </main>
        </div>
        <ScrollToTopButton />
      </template>
    </div>

    <div class="safe-bottom bg-gray-50"></div>
  </div>
</template>

<script setup>
import { ref, computed, provide, watch } from 'vue'
import { useRoute } from 'vue-router'
import AdminHeader from '@/components/admin/AdminHeader.vue'
import AdminSidebar from '@/components/admin/AdminSidebar.vue'
import ScrollToTopButton from '@/components/common/ScrollToTopButton.vue'

const route = useRoute()
const sidebarOpen = ref(false)

const titleOverride = ref('')
const subtitleOverride = ref('')

// 라우트 변경 시 override 초기화 + 사이드바 닫기
watch(
  () => route.fullPath,
  () => {
    titleOverride.value = ''
    subtitleOverride.value = ''
    sidebarOpen.value = false
  },
)

const headerTitle = computed(
  () => titleOverride.value || route.meta.adminTitle || '관리자',
)
const headerSubtitle = computed(() => subtitleOverride.value || '')

const conventionId = computed(() => route.params.id || '')

// provide는 그대로
provide('adminHeader', {
  setTitle: (t) => {
    titleOverride.value = t
  },
  setSubtitle: (s) => {
    subtitleOverride.value = s
  },
})
</script>

<style scoped>
.app-frame {
  display: flex;
  flex-direction: column;
  height: 100vh;
  height: 100dvh;
  overflow: hidden;
}

.safe-top {
  flex-shrink: 0;
  height: env(safe-area-inset-top, 0px);
}

.safe-content {
  flex: 1;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
}

.safe-bottom {
  height: env(safe-area-inset-bottom, 0px);
  flex-shrink: 0;
}

:global(.capacitor-app) .safe-top {
  height: max(env(safe-area-inset-top, 0px), 2rem);
}

:global(.capacitor-app) .safe-bottom {
  height: max(env(safe-area-inset-bottom, 0px), 3rem);
}
</style>
