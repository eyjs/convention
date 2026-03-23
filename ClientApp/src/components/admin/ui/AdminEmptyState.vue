<template>
  <div class="py-12 text-center bg-white rounded-lg shadow">
    <component
      :is="icon"
      v-if="icon"
      :size="48"
      class="mx-auto text-gray-300 mb-4"
    />
    <h3 class="text-lg font-medium text-gray-900 mb-1">{{ title }}</h3>
    <p v-if="description" class="text-sm text-gray-500 mb-4">
      {{ description }}
    </p>
    <div class="flex justify-center gap-2">
      <slot />
    </div>
  </div>
</template>

<script setup>
defineProps({
  icon: {
    type: [Object, Function],
    default: null,
  },
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
