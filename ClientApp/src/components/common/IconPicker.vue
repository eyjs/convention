<template>
  <div>
    <div class="flex items-center gap-2 mb-2">
      <input
        v-model="search"
        type="text"
        class="flex-1 px-3 py-1.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500"
        placeholder="아이콘 검색 (영어)..."
      />
      <button
        v-if="modelValue"
        type="button"
        class="text-xs text-red-500 hover:underline flex-shrink-0"
        @click="emit('update:modelValue', '')"
      >
        초기화
      </button>
    </div>

    <!-- 선택된 아이콘 미리보기 -->
    <div
      v-if="modelValue"
      class="flex items-center gap-2 mb-2 px-2 py-1.5 bg-primary-50 rounded-lg"
    >
      <component
        :is="getIconComponent(modelValue)"
        class="w-5 h-5 text-primary-600"
      />
      <span class="text-sm text-primary-700 font-medium">{{ modelValue }}</span>
    </div>

    <!-- 카테고리 탭 -->
    <div class="flex flex-wrap gap-1 mb-2">
      <button
        v-for="cat in iconCategories"
        :key="cat.name"
        type="button"
        class="px-2 py-0.5 text-xs rounded-full border transition-colors"
        :class="
          selectedCategory === cat.name
            ? 'bg-primary-500 text-white border-primary-500'
            : 'bg-white text-gray-500 border-gray-200 hover:bg-gray-50'
        "
        @click="
          selectedCategory = selectedCategory === cat.name ? '' : cat.name
        "
      >
        {{ cat.label }}
      </button>
    </div>

    <!-- 아이콘 그리드 -->
    <div class="max-h-48 overflow-y-auto border rounded-lg p-2">
      <div
        v-if="filteredIcons.length === 0"
        class="text-center py-4 text-xs text-gray-400"
      >
        검색 결과 없음
      </div>
      <div class="grid grid-cols-8 gap-1">
        <button
          v-for="icon in filteredIcons.slice(0, 120)"
          :key="icon"
          type="button"
          class="w-8 h-8 rounded-lg hover:bg-gray-100 transition-colors flex items-center justify-center"
          :class="
            modelValue === icon ? 'bg-primary-100 ring-2 ring-primary-500' : ''
          "
          :title="icon"
          @click="emit('update:modelValue', modelValue === icon ? '' : icon)"
        >
          <component :is="getIconComponent(icon)" class="w-4 h-4" />
        </button>
      </div>
      <p
        v-if="filteredIcons.length > 120"
        class="text-xs text-gray-400 text-center mt-2"
      >
        {{ filteredIcons.length - 120 }}개 더 있음 — 검색어를 입력하세요
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import * as lucideIcons from 'lucide-vue-next'

defineProps({
  modelValue: { type: String, default: '' },
})

const emit = defineEmits(['update:modelValue'])

const search = ref('')
const selectedCategory = ref('')

// 카테고리별 주요 아이콘 분류
const iconCategories = [
  {
    name: 'common',
    label: '일반',
    icons: [
      'Bell',
      'BellRing',
      'Bookmark',
      'Calendar',
      'Check',
      'CheckCircle',
      'Clock',
      'Eye',
      'Flag',
      'Heart',
      'Home',
      'Info',
      'Link',
      'List',
      'Mail',
      'MessageCircle',
      'Phone',
      'Pin',
      'Search',
      'Settings',
      'Star',
      'Tag',
      'ThumbsUp',
      'User',
      'Users',
      'Zap',
    ],
  },
  {
    name: 'travel',
    label: '여행',
    icons: [
      'Plane',
      'PlaneTakeoff',
      'PlaneLanding',
      'Map',
      'MapPin',
      'Navigation',
      'Compass',
      'Globe',
      'Hotel',
      'Bed',
      'Car',
      'Bus',
      'Train',
      'Ship',
      'Luggage',
      'Ticket',
      'Camera',
      'Palmtree',
      'Mountain',
      'Sun',
      'Umbrella',
      'Waves',
    ],
  },
  {
    name: 'food',
    label: '식사',
    icons: [
      'UtensilsCrossed',
      'Coffee',
      'Wine',
      'Beer',
      'CookingPot',
      'Salad',
      'Pizza',
      'Sandwich',
      'Cake',
      'IceCream',
      'Apple',
      'Cherry',
      'Grape',
      'Banana',
    ],
  },
  {
    name: 'event',
    label: '행사',
    icons: [
      'PartyPopper',
      'Gift',
      'Trophy',
      'Award',
      'Medal',
      'Crown',
      'Sparkles',
      'Music',
      'Mic',
      'Video',
      'Presentation',
      'GraduationCap',
      'Handshake',
      'Megaphone',
      'Target',
    ],
  },
  {
    name: 'file',
    label: '문서',
    icons: [
      'FileText',
      'File',
      'Folder',
      'ClipboardList',
      'ClipboardCheck',
      'Notebook',
      'BookOpen',
      'Pencil',
      'Edit',
      'Download',
      'Upload',
      'Paperclip',
      'Printer',
      'QrCode',
    ],
  },
  {
    name: 'alert',
    label: '알림',
    icons: [
      'AlertCircle',
      'AlertTriangle',
      'CircleAlert',
      'ShieldAlert',
      'ShieldCheck',
      'Lock',
      'Unlock',
      'Key',
      'Ban',
      'CircleX',
      'CircleCheck',
      'BadgeCheck',
      'Siren',
    ],
  },
]

// 모든 lucide 아이콘 이름 (Icon 접미사 제거)
const allIconNames = Object.keys(lucideIcons)
  .filter((k) => k.endsWith('Icon') && k !== 'default')
  .map((k) => k.replace(/Icon$/, ''))
  .sort()

const filteredIcons = computed(() => {
  let icons = allIconNames

  // 카테고리 필터
  if (selectedCategory.value) {
    const cat = iconCategories.find((c) => c.name === selectedCategory.value)
    if (cat) {
      icons = cat.icons.filter((name) => allIconNames.includes(name))
    }
  }

  // 검색 필터
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    icons = icons.filter((name) => name.toLowerCase().includes(q))
  }

  return icons
})

function getIconComponent(name) {
  return lucideIcons[name + 'Icon'] || lucideIcons[name] || null
}

defineExpose({ getIconComponent })
</script>
