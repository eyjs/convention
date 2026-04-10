<template>
  <BaseModal v-if="isOpen" :is-open="isOpen" max-width="md" @close="$emit('close')">
    <template #header>
      <h3 class="text-lg font-bold">참석자 배정</h3>
    </template>
    <template #body>
      <div class="mb-3">
        <input
          v-model="search"
          type="text"
          placeholder="이름/소속 검색..."
          class="w-full border rounded px-3 py-2"
        />
      </div>
      <p class="text-xs text-gray-500 mb-2">
        미배정 참석자 {{ unassigned.length }}명
      </p>
      <div class="max-h-80 overflow-y-auto space-y-1">
        <button
          v-for="g in filtered"
          :key="g.id"
          class="w-full text-left px-3 py-2 border rounded hover:bg-blue-50 hover:border-blue-400 text-sm"
          @click="$emit('select', g)"
        >
          <div class="font-medium">{{ g.name }}</div>
          <div class="text-xs text-gray-500">
            {{ g.corpPart || g.affiliation || '' }} · {{ g.phone || '' }}
          </div>
        </button>
        <div v-if="filtered.length === 0" class="text-center text-gray-400 py-8 text-sm">
          참석자가 없습니다
        </div>
      </div>
    </template>
  </BaseModal>
</template>

<script setup>
import { ref, computed } from 'vue'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({
  isOpen: Boolean,
  guests: { type: Array, default: () => [] },
  assignedUserIds: { type: Set, default: () => new Set() },
})
defineEmits(['close', 'select'])

const search = ref('')

const unassigned = computed(() =>
  props.guests.filter((g) => !props.assignedUserIds.has(g.id)),
)

const filtered = computed(() => {
  const q = search.value.trim().toLowerCase()
  if (!q) return unassigned.value
  return unassigned.value.filter(
    (g) =>
      g.name?.toLowerCase().includes(q) ||
      g.corpPart?.toLowerCase().includes(q) ||
      g.affiliation?.toLowerCase().includes(q),
  )
})
</script>
