<template>
  <div class="p-3 bg-white border-l h-full overflow-y-auto text-sm">
    <!-- 선택된 핀 -->
    <div v-if="selectedPin" class="space-y-3 mb-4">
      <h3 class="font-semibold text-gray-800">핀 정보</h3>
      <div>
        <span class="text-xs text-gray-500">배정</span>
        <p v-if="selectedPin.userName" class="font-medium">
          {{ selectedPin.userName }}
          <button class="text-red-400 text-xs ml-2" @click="$emit('clear-user', selectedPin.id)">해제</button>
        </p>
        <button v-else class="text-blue-500 text-xs" @click="$emit('assign', selectedPin)">참석자 배정</button>
      </div>
      <div>
        <span class="text-xs text-gray-500">그룹 (테이블)</span>
        <input
          :value="selectedPin.group || ''"
          type="text"
          placeholder="예: 1번 테이블"
          class="w-full border rounded px-2 py-1 text-xs mt-0.5"
          @input="$emit('set-group', { pinId: selectedPin.id, group: $event.target.value })"
        />
      </div>
      <button class="w-full bg-red-500 text-white py-1.5 rounded text-xs hover:bg-red-600" @click="$emit('delete')">
        핀 삭제
      </button>
    </div>

    <div v-else class="text-gray-400 text-center mt-4 text-xs mb-4">
      핀을 선택하거나<br />핀 모드(📌)로 클릭하여 추가
    </div>

    <!-- 배정 현황 -->
    <div class="border-t pt-3">
      <div class="flex items-center justify-between mb-2">
        <h4 class="font-medium text-gray-700 text-xs">배정 현황</h4>
        <span class="text-xs" :class="assignedCount === totalGuests && totalGuests > 0 ? 'text-green-600 font-bold' : 'text-gray-500'">
          {{ assignedCount }}/{{ totalGuests }}명
        </span>
      </div>
      <div class="h-1.5 bg-gray-100 rounded-full overflow-hidden mb-3">
        <div class="h-full bg-blue-500 rounded-full transition-all" :style="{ width: totalGuests > 0 ? `${(assignedCount / totalGuests) * 100}%` : '0%' }"></div>
      </div>
    </div>

    <!-- 미배정 참석자 -->
    <div class="border-t pt-3">
      <h4 class="font-medium text-gray-700 text-xs mb-2">미배정 ({{ unassigned.length }}명)</h4>
      <input v-model="search" type="text" placeholder="검색..." class="w-full border rounded px-2 py-1 text-xs mb-2" />
      <div class="max-h-60 overflow-y-auto space-y-0.5">
        <div
          v-for="g in filtered"
          :key="g.id"
          class="text-xs text-gray-600 py-1 px-1.5 hover:bg-blue-50 rounded cursor-pointer"
          @click="$emit('quick-assign', g)"
        >
          <span class="font-medium">{{ g.name }}</span>
          <span class="text-gray-400 ml-1">{{ g.corpPart || g.affiliation || '' }}</span>
        </div>
        <div v-if="filtered.length === 0" class="text-xs text-gray-400 text-center py-4">
          {{ search ? '검색 결과 없음' : '참석자 없음' }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

const props = defineProps({
  selectedPin: { type: Object, default: null },
  guests: { type: Array, default: () => [] },
  assignedUserIds: { type: Set, default: () => new Set() },
})
defineEmits(['assign', 'clear-user', 'set-group', 'delete', 'quick-assign'])

const search = ref('')
const assignedCount = computed(() => props.assignedUserIds.size)
const totalGuests = computed(() => props.guests.length)
const unassigned = computed(() => props.guests.filter((g) => !props.assignedUserIds.has(g.id)))
const filtered = computed(() => {
  const q = search.value.trim().toLowerCase()
  const list = unassigned.value
  if (!q) return list.slice(0, 100)
  return list.filter((g) => g.name?.toLowerCase().includes(q) || g.corpPart?.toLowerCase().includes(q)).slice(0, 100)
})
</script>
