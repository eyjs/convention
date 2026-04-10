<template>
  <div class="p-3 bg-white border-l h-full overflow-y-auto text-base">
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
        <span class="text-xs text-gray-500">그룹</span>
        <input
          :value="selectedPin.group || ''"
          type="text"
          placeholder="예: A조, 1번, VIP"
          class="w-full border rounded px-2 py-1.5 text-sm mt-0.5"
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

    <!-- 참석자 목록 (클릭 → 핀 자동 생성 + 배정) -->
    <div class="border-t pt-3">
      <div class="flex items-center justify-between mb-1">
        <h4 class="font-medium text-gray-700 text-xs">미배정 ({{ unassigned.length }}명)</h4>
        <span class="text-xs text-gray-400">이름 클릭 → 핀 자동 생성</span>
      </div>
      <input
        v-model="search"
        type="text"
        placeholder="이름 검색..."
        class="w-full border rounded px-2 py-1.5 text-xs mb-1.5 sticky top-0 bg-white z-10"
      />

      <!-- 필터: 소속 -->
      <div class="mb-1">
        <select
          v-model="filterAffiliation"
          class="w-full border rounded px-2 py-1.5 text-sm text-gray-700"
        >
          <option value="">소속 전체</option>
          <option v-for="a in affiliations" :key="a" :value="a">{{ a }}</option>
        </select>
      </div>

      <!-- 필터: 성별 -->
      <div class="flex gap-1 mb-1">
        <button
          v-for="g in genderOptions"
          :key="g.value"
          class="px-2 py-1 rounded text-sm"
          :class="filterGender === g.value ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600'"
          @click="filterGender = filterGender === g.value ? '' : g.value"
        >
          {{ g.label }}
        </button>
      </div>

      <!-- 필터: 일정(코스)/그룹 -->
      <div class="flex gap-1 mb-1 overflow-x-auto pb-0.5">
        <button
          class="px-2 py-1 rounded text-sm whitespace-nowrap"
          :class="filterGroup === '' ? 'bg-green-600 text-white' : 'bg-gray-100 text-gray-600'"
          @click="filterGroup = ''"
        >
          전체
        </button>
        <button
          v-for="group in groupNames"
          :key="group"
          class="px-2 py-1 rounded text-sm whitespace-nowrap"
          :class="filterGroup === group ? 'bg-green-600 text-white' : 'bg-gray-100 text-gray-600'"
          @click="filterGroup = filterGroup === group ? '' : group"
        >
          {{ group }}
        </button>
      </div>

      <div class="overflow-y-auto space-y-0.5" style="max-height: calc(100vh - 480px)">
        <button
          v-for="g in filtered"
          :key="g.id"
          class="w-full text-left text-xs py-2 px-3 hover:bg-blue-50 rounded flex items-center gap-1.5 transition-colors"
          @click="$emit('create-pin-for', g)"
        >
          <span class="w-7 h-7 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center text-xs font-bold flex-shrink-0">
            {{ g.name?.charAt(0) }}
          </span>
          <span class="font-medium truncate">{{ g.name }}</span>
          <span class="text-gray-400 truncate text-xs">{{ g.corpPart || g.groupName || '' }}</span>
        </button>
        <div v-if="filtered.length === 0" class="text-xs text-gray-400 text-center py-4">
          {{ search ? '검색 결과 없음' : '모두 배정 완료' }}
        </div>
      </div>
    </div>

    <!-- 배정 완료 목록 (접이식) -->
    <details class="border-t pt-2 mt-2">
      <summary class="text-xs text-gray-500 cursor-pointer">배정 완료 ({{ assignedCount }}명)</summary>
      <div class="mt-1 space-y-0.5 max-h-40 overflow-y-auto">
        <div
          v-for="g in assignedGuests"
          :key="g.id"
          class="text-xs text-gray-400 py-0.5 px-2 flex items-center gap-1"
        >
          <span class="w-4 h-4 rounded-full bg-green-100 text-green-600 flex items-center justify-center text-xs flex-shrink-0">✓</span>
          <span class="truncate">{{ g.name }}</span>
        </div>
      </div>
    </details>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

const props = defineProps({
  selectedPin: { type: Object, default: null },
  guests: { type: Array, default: () => [] },
  assignedUserIds: { type: Set, default: () => new Set() },
})
defineEmits(['assign', 'clear-user', 'set-group', 'delete', 'quick-assign', 'create-pin-for'])

const search = ref('')
const filterGroup = ref('')
const filterAffiliation = ref('')
const filterGender = ref('')

const genderOptions = [
  { label: '전체', value: '' },
  { label: '남', value: 'M' },
  { label: '여', value: 'F' },
]

const assignedCount = computed(() => props.assignedUserIds.size)
const totalGuests = computed(() => props.guests.length)
const unassigned = computed(() => props.guests.filter((g) => !props.assignedUserIds.has(g.id)))
const assignedGuests = computed(() => props.guests.filter((g) => props.assignedUserIds.has(g.id)))

const groupNames = computed(() => {
  const names = new Set()
  props.guests.forEach((g) => { if (g.groupName) names.add(g.groupName) })
  return [...names].sort()
})

const affiliations = computed(() => {
  const names = new Set()
  props.guests.forEach((g) => {
    const aff = g.corpPart || g.affiliation
    if (aff) names.add(aff)
  })
  return [...names].sort()
})

function getGender(g) {
  // 주민번호 뒷자리 첫번째로 성별 판단 (1,3=남, 2,4=여)
  if (!g.residentNumber) return ''
  const parts = g.residentNumber.replace(/[^0-9]/g, '')
  if (parts.length < 7) return ''
  const code = parts.charAt(6)
  if (code === '1' || code === '3') return 'M'
  if (code === '2' || code === '4') return 'F'
  return ''
}

const filtered = computed(() => {
  let list = unassigned.value

  if (filterGroup.value) list = list.filter((g) => g.groupName === filterGroup.value)
  if (filterAffiliation.value) list = list.filter((g) => (g.corpPart || g.affiliation) === filterAffiliation.value)
  if (filterGender.value) list = list.filter((g) => getGender(g) === filterGender.value)

  const q = search.value.trim().toLowerCase()
  if (q) list = list.filter((g) => g.name?.toLowerCase().includes(q))

  return list.slice(0, 200)
})
</script>
