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
    <div v-else-if="menuItems.length > 0" class="px-4 py-5">
      <div class="grid grid-cols-2 gap-3">
        <div
          v-for="item in menuItems"
          :key="item.id"
          class="bg-white rounded-xl border border-gray-100 shadow-sm p-4 cursor-pointer hover:shadow-md hover:border-blue-200 active:scale-[0.97] transition-all duration-150 flex flex-col items-center text-center gap-2"
          role="button"
          tabindex="0"
          @click="handleAction(item)"
          @keydown.enter="handleAction(item)"
        >
          <!-- 아이콘 -->
          <div
            class="w-12 h-12 rounded-xl flex items-center justify-center"
            :style="getIconStyle(item)"
          >
            <span v-if="getConfig(item).icon" class="text-2xl">{{
              getConfig(item).icon
            }}</span>
            <component :is="getDefaultIcon(item)" v-else class="w-6 h-6" />
          </div>

          <!-- 제목 -->
          <span class="text-sm font-semibold text-gray-800 leading-tight">
            {{ item.title }}
          </span>

          <!-- 상태 뱃지 -->
          <span
            v-if="item.badge"
            :class="[
              'px-2 py-0.5 text-[10px] font-medium rounded-full',
              item.badgeClass,
            ]"
          >
            {{ item.badge }}
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
import { useRoute, useRouter } from 'vue-router'
import { ClipboardList, FileText, LayoutGrid, Zap } from 'lucide-vue-next'
import apiClient from '@/services/api'
import MainHeader from '@/components/common/MainHeader.vue'
import { useAction } from '@/composables/useAction'

const route = useRoute()
const router = useRouter()
const { executeAction } = useAction()
const menuItems = ref([])
const isLoading = ref(false)

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

function handleAction(item) {
  if (item._isChecklistGroup) {
    router.push({
      name: 'Checklist',
      params: { conventionId: route.params.conventionId },
    })
    return
  }
  executeAction(item)
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

    const items = []
    const checklistItems = []

    for (const action of actions) {
      if (action.actionCategory === 'CHECKLIST_CARD') {
        checklistItems.push(action)
      } else {
        const status = statusMap.get(action.id)
        items.push({
          ...action,
          isComplete: status?.isComplete,
          badge:
            status?.isComplete !== undefined
              ? status.isComplete
                ? '완료'
                : '미완료'
              : undefined,
          badgeClass: status?.isComplete
            ? 'bg-emerald-50 text-emerald-600'
            : 'bg-orange-50 text-orange-600',
        })
      }
    }

    // 체크리스트 항목이 있으면 하나의 그룹 카드로 합침
    if (checklistItems.length > 0) {
      const completedCount = checklistItems.filter(
        (c) => statusMap.get(c.id)?.isComplete,
      ).length
      const total = checklistItems.length
      const allDone = completedCount === total

      items.unshift({
        id: 'checklist-group',
        title: '체크리스트',
        actionCategory: 'CHECKLIST_CARD',
        _isChecklistGroup: true,
        badge: `${completedCount}/${total}`,
        badgeClass: allDone
          ? 'bg-emerald-50 text-emerald-600'
          : 'bg-orange-50 text-orange-600',
      })
    }

    menuItems.value = items
  } catch (error) {
    console.error('Failed to load menu actions:', error)
    menuItems.value = []
  } finally {
    isLoading.value = false
  }
})
</script>
