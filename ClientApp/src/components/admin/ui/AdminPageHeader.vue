<template>
  <div
    class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 sm:gap-4"
  >
    <div>
      <h2 class="text-xl font-semibold text-gray-900">{{ title }}</h2>
      <p v-if="description" class="text-sm text-gray-500 mt-1">
        {{ description }}
      </p>
    </div>
    <div class="flex flex-wrap items-center gap-2">
      <slot />
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    required: true,
  },
  description: {
    type: String,
    default: '',
  },
})
</script>
