<template>
  <select
    :value="modelValue"
    class="px-3 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent min-w-[120px]"
    @change="$emit('update:modelValue', $event.target.value)"
  >
    <slot></slot>
  </select>
</template>

<script setup>
defineProps({
  modelValue: { type: [String, Number], default: '' },
})

defineEmits(['update:modelValue'])
</script>
