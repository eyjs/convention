<template>
  <!-- Mobile overlay -->
  <Transition name="fade">
    <div
      v-if="open"
      class="fixed inset-0 bg-black/50 z-40 md:hidden"
      @click="$emit('close')"
    />
  </Transition>

  <!-- Sidebar -->
  <aside
    :class="[
      'fixed top-0 bottom-0 left-0 z-40 w-64 bg-white border-r border-gray-200 flex flex-col transition-transform duration-200 pt-16',
      open ? 'translate-x-0' : '-translate-x-full',
      'md:translate-x-0',
    ]"
  >
    <nav class="flex-1 py-4 overflow-y-auto">
      <ul class="space-y-1 px-2">
        <li v-for="item in navItems" :key="item.key">
          <router-link
            :to="getPath(conventionId, item)"
            :class="[
              'flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors',
              activeKey === item.key
                ? 'bg-primary-50 text-primary-700'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900',
            ]"
            @click="$emit('close')"
          >
            <component :is="item.icon" :size="20" class="flex-shrink-0" />
            <span class="truncate">{{ item.label }}</span>
          </router-link>
        </li>
      </ul>
    </nav>
  </aside>
</template>

<script setup>
import { useAdminNav } from '@/composables/useAdminNav'

defineProps({
  conventionId: {
    type: [Number, String],
    default: '',
  },
  open: {
    type: Boolean,
    default: false,
  },
})

defineEmits(['close'])

const { navItems, activeKey, getPath } = useAdminNav()
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
