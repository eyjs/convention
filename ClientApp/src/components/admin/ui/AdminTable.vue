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
                col.sortable
                  ? 'cursor-pointer hover:bg-gray-100 select-none'
                  : '',
              ]"
              :style="col.width ? { width: col.width } : {}"
              @click="col.sortable && $emit('sort', col.key)"
            >
              <div
                class="flex items-center gap-1"
                :class="{
                  'justify-center': col.align === 'center',
                  'justify-end': col.align === 'right',
                }"
              >
                <span>{{ col.label }}</span>
                <span v-if="col.sortable" class="text-gray-400">
                  <svg
                    v-if="sortKey === col.key && sortDir === 'asc'"
                    class="w-3 h-3 text-primary-500"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                  >
                    <path d="M5 12l5-5 5 5H5z" />
                  </svg>
                  <svg
                    v-else-if="sortKey === col.key && sortDir === 'desc'"
                    class="w-3 h-3 text-primary-500"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                  >
                    <path d="M5 8l5 5 5-5H5z" />
                  </svg>
                  <svg
                    v-else
                    class="w-3 h-3"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                  >
                    <path d="M5 8l5-5 5 5H5zm0 4l5 5 5-5H5z" opacity="0.4" />
                  </svg>
                </span>
              </div>
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
  sortKey: {
    type: String,
    default: null,
  },
  sortDir: {
    type: String,
    default: 'asc',
  },
})

defineEmits(['sort'])
</script>
