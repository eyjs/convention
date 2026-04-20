<template>
  <div v-if="visibleAttributes.length > 0" class="flex flex-wrap gap-1.5">
    <div
      v-for="(attr, index) in visibleAttributes"
      :key="attr.key"
      class="inline-flex items-center gap-1 rounded-md px-2 py-0.5"
      :style="{
        backgroundColor: palette(index).bg,
        color: palette(index).text,
      }"
    >
      <span class="text-[10px] opacity-75">{{ attr.key }}</span>
      <span class="text-xs font-medium">{{ attr.value }}</span>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  // 배정 정보 배열: [{ key: '룸번호', value: '312' }, { key: '룸메이트', value: '심현목' }]
  attributes: { type: Array, default: () => [] },
  // 표시할 속성 키 목록 (비어있으면 전체 표시)
  showKeys: { type: Array, default: () => [] },
})

// 5색 팔레트 순환
const COLOR_PALETTE = [
  { bg: '#EEEDFE', text: '#3C3489' }, // 보라
  { bg: '#E1F5EE', text: '#085041' }, // 초록
  { bg: '#FAEEDA', text: '#633806' }, // 황금
  { bg: '#E1F5EE', text: '#0F6E56' }, // 민트
  { bg: '#FCEBEB', text: '#A32D2D' }, // 빨강
]

function palette(index) {
  return COLOR_PALETTE[index % COLOR_PALETTE.length]
}

const visibleAttributes = computed(() => {
  if (!props.attributes || props.attributes.length === 0) return []
  if (props.showKeys.length === 0) return props.attributes
  return props.attributes.filter((attr) => props.showKeys.includes(attr.key))
})
</script>
