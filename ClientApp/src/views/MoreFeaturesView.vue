<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <MainHeader title="추가 메뉴" :show-back="true" />

    <!-- 로딩 -->
    <div v-if="isLoading" class="flex items-center justify-center py-12">
      <div class="text-center">
        <div
          class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"
        ></div>
        <p class="mt-3 text-sm text-gray-500">메뉴를 불러오는 중...</p>
      </div>
    </div>

    <!-- 2x2 그리드 -->
    <div v-else-if="allActions.length > 0" class="px-4 py-5">
      <div class="grid grid-cols-2 gap-3">
        <div
          v-for="action in allActions"
          :key="action.id"
          class="bg-white rounded-xl border border-gray-100 shadow-sm p-4 cursor-pointer hover:shadow-md hover:border-blue-200 active:scale-[0.97] transition-all duration-150 flex flex-col items-center text-center gap-2"
          role="button"
          tabindex="0"
          @click="handleAction(action)"
          @keydown.enter="handleAction(action)"
        >
          <!-- 아이콘 -->
          <div
            class="w-12 h-12 rounded-xl flex items-center justify-center"
            :style="getIconStyle(action)"
          >
            <span v-if="getConfig(action).icon" class="text-2xl">{{
              getConfig(action).icon
            }}</span>
            <component :is="getDefaultIcon(action)" v-else class="w-6 h-6" />
          </div>

          <!-- 제목 -->
          <span class="text-sm font-semibold text-gray-800 leading-tight">
            {{ action.title }}
          </span>

          <!-- 상태 뱃지 -->
          <span
            v-if="action.isComplete !== undefined"
            :class="[
              'px-2 py-0.5 text-[10px] font-medium rounded-full',
              action.isComplete
                ? 'bg-emerald-50 text-emerald-600'
                : 'bg-orange-50 text-orange-600',
            ]"
          >
            {{ action.isComplete ? '완료' : '미완료' }}
          </span>
        </div>
      </div>
    </div>

    <!-- 빈 상태 -->
    <div v-else class="px-4 pt-12 pb-8">
      <div class="text-center">
        <LayoutGrid class="w-10 h-10 text-gray-300 mx-auto mb-3" />
        <p class="text-sm text-gray-400">등록된 메뉴가 없습니다.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ClipboardList, FileText, LayoutGrid, Zap } from 'lucide-vue-next'
import apiClient from '@/services/api'
import MainHeader from '@/components/common/MainHeader.vue'
import { useAction } from '@/composables/useAction'

const route = useRoute()
const { executeAction } = useAction()
const allActions = ref([])
const isLoading = ref(false)

// 액션 카테고리별 기본 아이콘
const CATEGORY_ICONS = {
  CARD: FileText,
  MENU: LayoutGrid,
  BUTTON: Zap,
  CHECKLIST_CARD: ClipboardList,
}

function getConfig(action) {
  try {
    if (!action.configJson) return {}
    return typeof action.configJson === 'string'
      ? JSON.parse(action.configJson)
      : action.configJson
  } catch {
    return {}
  }
}

function getIconStyle(action) {
  const config = getConfig(action)
  return {
    backgroundColor: config.bgColor || '#EEF2FF',
    color: config.iconColor || '#6366F1',
  }
}

function getDefaultIcon(action) {
  return CATEGORY_ICONS[action.actionCategory] || Zap
}

function handleAction(action) {
  executeAction(action)
}

onMounted(async () => {
  const conventionId = route.params.conventionId
  isLoading.value = true

  try {
    const [actionsRes, statusesRes] = await Promise.all([
      apiClient.get(`/conventions/${conventionId}/actions/menu`),
      apiClient.get(`/conventions/${conventionId}/actions/statuses`),
    ])

    const actions = actionsRes.data || []
    const statuses = statusesRes.data || []
    const statusMap = new Map(statuses.map((s) => [s.conventionActionId, s]))

    allActions.value = actions.map((action) => ({
      ...action,
      isComplete: statusMap.get(action.id)?.isComplete,
    }))
  } catch (error) {
    console.error('Failed to load menu actions:', error)
    allActions.value = []
  } finally {
    isLoading.value = false
  }
})
</script>
