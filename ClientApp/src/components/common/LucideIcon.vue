<template>
  <component :is="iconComponent" v-if="iconComponent" v-bind="$attrs" />
</template>

<script setup>
import { computed } from 'vue'
import * as lucideIcons from 'lucide-vue-next'

const props = defineProps({
  name: { type: String, required: true },
})

const iconComponent = computed(() => {
  if (!props.name || !/^[A-Z]/.test(props.name)) return null
  return lucideIcons[props.name + 'Icon'] || null
})
</script>
