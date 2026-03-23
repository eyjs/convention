<template>
  <div class="bg-white rounded-lg shadow overflow-hidden">
    <div class="overflow-x-auto">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th
              v-for="col in columns"
              :key="col.key"
              :class="[
                'px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider',
                col.align === 'center'
                  ? 'text-center'
                  : col.align === 'right'
                    ? 'text-right'
                    : 'text-left',
              ]"
              :style="col.width ? { width: col.width } : {}"
            >
              {{ col.label }}
            </th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <slot />
        </tbody>
      </table>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <div
        class="w-8 h-8 border-2 border-primary-600 border-t-transparent rounded-full animate-spin"
      />
    </div>

    <!-- Empty -->
    <div v-else-if="empty" class="py-12 text-center">
      <component
        :is="emptyIcon"
        v-if="emptyIcon"
        :size="40"
        class="mx-auto text-gray-300 mb-3"
      />
      <p class="text-gray-500">{{ emptyText }}</p>
    </div>
  </div>
</template>

<script setup>
defineProps({
  columns: {
    type: Array,
    required: true,
  },
  loading: {
    type: Boolean,
    default: false,
  },
  empty: {
    type: Boolean,
    default: false,
  },
  emptyText: {
    type: String,
    default: '데이터가 없습니다',
  },
  emptyIcon: {
    type: [Object, Function],
    default: null,
  },
})
</script>
