<template>
  <div class="bg-white rounded-lg shadow p-6">
    <div class="flex items-center justify-between">
      <div>
        <p class="text-sm text-gray-600">{{ label }}</p>
        <p class="text-2xl font-bold mt-1">{{ value }}</p>
      </div>
      <div
        :class="[
          'w-12 h-12 rounded-lg flex items-center justify-center',
          colorClass,
        ]"
      >
        <component :is="icon" v-if="icon" :size="24" :class="iconColorClass" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  label: {
    type: String,
    required: true,
  },
  value: {
    type: [String, Number],
    required: true,
  },
  icon: {
    type: [Object, Function],
    default: null,
  },
  color: {
    type: String,
    default: 'primary',
    validator: (v) => ['primary', 'green', 'orange', 'red'].includes(v),
  },
})

const colorClass = computed(() => {
  const map = {
    primary: 'bg-primary-100',
    green: 'bg-green-100',
    orange: 'bg-orange-100',
    red: 'bg-red-100',
  }
  return map[props.color]
})

const iconColorClass = computed(() => {
  const map = {
    primary: 'text-primary-600',
    green: 'text-green-600',
    orange: 'text-orange-600',
    red: 'text-red-600',
  }
  return map[props.color]
})
</script>
