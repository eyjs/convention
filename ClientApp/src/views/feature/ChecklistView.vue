<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <MainHeader title="체크리스트" :show-back="true" />

    <!-- 로딩 -->
    <div v-if="isLoading" class="flex items-center justify-center py-12">
      <div class="text-center">
        <div
          class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"
        ></div>
        <p class="mt-3 text-sm text-gray-500">불러오는 중...</p>
      </div>
    </div>

    <div v-else class="px-4 py-5 space-y-4">
      <!-- 진행률 요약 -->
      <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-5">
        <div class="flex items-center justify-between mb-3">
          <div>
            <h3 class="text-base font-bold text-gray-900">진행 현황</h3>
            <p class="text-sm text-gray-500 mt-0.5">
              {{ checklist.completedItems }} / {{ checklist.totalItems }} 완료
            </p>
          </div>
          <div
            class="text-2xl font-bold"
            :class="
              checklist.progressPercentage === 100
                ? 'text-emerald-500'
                : 'text-blue-600'
            "
          >
            {{ checklist.progressPercentage }}%
          </div>
        </div>
        <div class="h-2.5 bg-gray-100 rounded-full overflow-hidden">
          <div
            class="h-full rounded-full transition-all duration-500 ease-out"
            :class="
              checklist.progressPercentage === 100
                ? 'bg-emerald-500'
                : 'bg-blue-500'
            "
            :style="{ width: `${checklist.progressPercentage}%` }"
          ></div>
        </div>
      </div>

      <!-- 체크리스트 아이템 -->
      <div
        v-if="checklist.items.length > 0"
        class="bg-white rounded-2xl shadow-sm border border-gray-100 divide-y divide-gray-100"
      >
        <div
          v-for="item in checklist.items"
          :key="item.actionId"
          class="flex items-center gap-3 px-4 py-3.5 cursor-pointer active:bg-gray-50 transition-colors"
          :class="{ 'opacity-50': isExpired(item.deadline) && !item.isComplete }"
          @click="toggleItem(item)"
        >
          <!-- 체크 아이콘 -->
          <div
            class="w-6 h-6 rounded-full flex items-center justify-center flex-shrink-0 transition-colors"
            :class="
              item.isComplete
                ? 'bg-emerald-500'
                : 'border-2 border-gray-300'
            "
          >
            <Check
              v-if="item.isComplete"
              class="w-3.5 h-3.5 text-white"
            />
          </div>

          <!-- 제목 + 마감일 -->
          <div class="flex-1 min-w-0">
            <p
              class="text-sm font-medium"
              :class="
                item.isComplete
                  ? 'text-gray-400 line-through'
                  : 'text-gray-900'
              "
            >
              {{ item.title }}
            </p>
            <p
              v-if="item.deadline"
              class="text-xs mt-0.5"
              :class="
                isExpired(item.deadline) && !item.isComplete
                  ? 'text-red-400'
                  : 'text-gray-400'
              "
            >
              {{ formatDeadline(item.deadline) }}
              <span v-if="isExpired(item.deadline) && !item.isComplete"> (마감)</span>
            </p>
          </div>

          <!-- 토글 중 스피너 -->
          <div
            v-if="togglingId === item.actionId"
            class="w-5 h-5 animate-spin rounded-full border-2 border-gray-300 border-t-blue-500"
          ></div>
        </div>
      </div>

      <!-- 빈 상태 -->
      <div
        v-else
        class="text-center py-12"
      >
        <ClipboardList class="w-10 h-10 text-gray-300 mx-auto mb-3" />
        <p class="text-sm text-gray-400">등록된 체크리스트가 없습니다.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { Check, ClipboardList } from 'lucide-vue-next'
import apiClient from '@/services/api'
import MainHeader from '@/components/common/MainHeader.vue'

const route = useRoute()
const isLoading = ref(false)
const togglingId = ref(null)

const checklist = reactive({
  totalItems: 0,
  completedItems: 0,
  progressPercentage: 0,
  items: [],
})

async function fetchChecklist() {
  const conventionId = route.params.conventionId
  isLoading.value = true
  try {
    const res = await apiClient.get(
      `/conventions/${conventionId}/actions/checklist-status`,
    )
    Object.assign(checklist, res.data)
  } catch (error) {
    console.error('Failed to load checklist:', error)
  } finally {
    isLoading.value = false
  }
}

async function toggleItem(item) {
  if (togglingId.value) return
  if (isExpired(item.deadline) && !item.isComplete) return

  const conventionId = route.params.conventionId
  togglingId.value = item.actionId

  try {
    await apiClient.post(
      `/conventions/${conventionId}/actions/${item.actionId}/toggle`,
      { isComplete: !item.isComplete },
    )
    item.isComplete = !item.isComplete

    // 진행률 재계산
    checklist.completedItems = checklist.items.filter((i) => i.isComplete).length
    checklist.progressPercentage =
      checklist.totalItems > 0
        ? Math.round((checklist.completedItems * 100) / checklist.totalItems)
        : 0
  } catch (error) {
    console.error('Failed to toggle checklist item:', error)
  } finally {
    togglingId.value = null
  }
}

function isExpired(deadline) {
  if (!deadline) return false
  return new Date(deadline).getTime() <= Date.now()
}

function formatDeadline(deadline) {
  if (!deadline) return ''
  const d = new Date(deadline)
  return `${d.getMonth() + 1}/${d.getDate()} ${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}까지`
}

onMounted(fetchChecklist)
</script>
