<template>
  <div class="min-h-screen min-h-dvh bg-gray-50 pb-8">
    <!-- 로딩 -->
    <div v-if="isLoading" class="flex items-center justify-center py-12">
      <div class="text-center">
        <div
          class="inline-block animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"
        ></div>
        <p class="mt-3 text-sm text-gray-500">불러오는 중...</p>
      </div>
    </div>

    <div v-else class="space-y-3 pt-2">
      <!-- 섹션 1: 내 정보 -->
      <section class="mx-3">
        <p class="text-xs font-medium text-gray-400 mb-2 px-1">내 정보</p>
        <div
          class="bg-white rounded-xl border border-black/[0.07] overflow-hidden shadow-sm"
        >
          <!-- 이름 + 역할 -->
          <div class="flex items-center gap-3 px-4 py-3 border-b border-black/[0.05]">
            <div
              class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0"
              style="background: #eeedfe"
            >
              <UserIcon class="w-4 h-4" style="color: #7f77dd" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-sm font-medium text-gray-900 truncate">
                {{ myInfo.name || '-' }}
              </div>
              <div v-if="myInfo.role" class="text-xs text-gray-400">
                {{ myInfo.role }}
              </div>
            </div>
          </div>

          <!-- 코스 -->
          <div
            v-if="myInfo.scheduleCourses && myInfo.scheduleCourses.length > 0"
            class="flex items-center gap-3 px-4 py-3 border-b border-black/[0.05]"
          >
            <div
              class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0"
              style="background: #e1f5ee"
            >
              <CalendarIcon class="w-4 h-4" style="color: #1d9e75" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-sm font-medium text-gray-900">코스</div>
              <div class="text-xs text-gray-400 truncate">
                {{ myInfo.scheduleCourses.map(c => c.courseName || c).join(' · ') }}
              </div>
            </div>
          </div>

          <!-- 그룹 -->
          <div
            v-if="myInfo.groupName"
            class="flex items-center gap-3 px-4 py-3 border-b border-black/[0.05]"
          >
            <div
              class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0"
              style="background: #faeeda"
            >
              <UsersIcon class="w-4 h-4" style="color: #ba7517" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-sm font-medium text-gray-900">그룹</div>
              <div class="text-xs text-gray-400">{{ myInfo.groupName }}</div>
            </div>
          </div>

          <!-- 동반자 토글 -->
          <div
            v-if="myInfo.companions && myInfo.companions.length > 0"
            class="border-b border-black/[0.05] last:border-b-0"
          >
            <button
              class="w-full flex items-center gap-3 px-4 py-3 text-left hover:bg-gray-50 transition-colors"
              :aria-expanded="isCompanionsOpen"
              aria-controls="companions-list"
              @click="isCompanionsOpen = !isCompanionsOpen"
            >
              <div
                class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0"
                style="background: #e1f5ee"
              >
                <UsersIcon class="w-4 h-4" style="color: #1d9e75" />
              </div>
              <span class="flex-1 text-sm font-medium text-gray-900"
                >동반자 보기</span
              >
              <ChevronDownIcon
                class="w-4 h-4 text-gray-400 transition-transform duration-200"
                :class="{ 'rotate-180': isCompanionsOpen }"
              />
            </button>

            <!-- 동반자 목록 -->
            <div
              v-if="isCompanionsOpen"
              id="companions-list"
              class="px-4 pb-3 space-y-2"
            >
              <div
                v-for="companion in myInfo.companions"
                :key="companion.userId || companion.name"
                class="flex items-center gap-2 py-1.5"
              >
                <div
                  class="w-7 h-7 rounded-full bg-gray-100 flex items-center justify-center text-xs font-medium text-gray-600 flex-shrink-0"
                >
                  {{ (companion.name || '?').charAt(0) }}
                </div>
                <div class="flex-1 min-w-0">
                  <div class="text-sm text-gray-800">{{ companion.name }}</div>
                  <div v-if="companion.relationType" class="text-xs text-gray-400">
                    {{ companion.relationType }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- 섹션 2: 내 배정 정보 -->
      <section v-if="hasAttributes" class="mx-3">
        <p class="text-xs font-medium text-gray-400 mb-2 px-1">내 배정 정보</p>
        <div
          class="bg-white rounded-xl border border-black/[0.07] overflow-hidden shadow-sm"
        >
          <div class="grid grid-cols-2">
            <div
              v-for="(attr, index) in myInfo.attributes"
              :key="attr.key"
              class="px-4 py-3 border-b border-black/[0.05]"
              :class="[
                index % 2 === 0 ? 'border-r border-black/[0.05]' : '',
              ]"
            >
              <div
                class="text-[11px] mb-1"
                :style="{ color: palette(index).text }"
              >
                {{ attr.key }}
              </div>
              <div
                v-if="attr.value"
                class="text-sm font-semibold text-gray-900"
              >
                {{ attr.value }}
              </div>
              <div v-else class="text-sm text-gray-400">미배정</div>
            </div>
          </div>
        </div>
      </section>

      <!-- 섹션 3: 메뉴 -->
      <section v-if="hasMenuItems" class="mx-3">
        <p class="text-xs font-medium text-gray-400 mb-2 px-1">메뉴</p>
        <div
          class="bg-white rounded-xl border border-black/[0.07] overflow-hidden shadow-sm"
        >
          <!-- 설문조사 -->
          <button
            v-if="hasSurveys"
            class="w-full flex items-center gap-3 px-4 py-3 border-b border-black/[0.05] hover:bg-gray-50 transition-colors text-left"
            aria-label="설문조사로 이동"
            @click="goToSurveys"
          >
            <div
              class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0"
              style="background: #eef2ff"
            >
              <ClipboardListIcon class="w-4 h-4" style="color: #6366f1" />
            </div>
            <span class="flex-1 text-sm font-medium text-gray-900">설문조사</span>
            <span
              v-if="incompleteSurveyCount > 0"
              class="px-2 py-0.5 text-[10px] font-medium rounded-full bg-orange-50 text-orange-600"
            >
              {{ incompleteSurveyCount }}개 미완료
            </span>
            <ChevronRightIcon class="w-4 h-4 text-gray-300 flex-shrink-0" />
          </button>

          <!-- 체크리스트 -->
          <button
            v-if="checklistMeta"
            class="w-full flex items-center gap-3 px-4 py-3 border-b border-black/[0.05] hover:bg-gray-50 transition-colors text-left"
            aria-label="체크리스트로 이동"
            @click="goToChecklist"
          >
            <div
              class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0"
              style="background: #e1f5ee"
            >
              <CheckSquareIcon class="w-4 h-4" style="color: #1d9e75" />
            </div>
            <span class="flex-1 text-sm font-medium text-gray-900">체크리스트</span>
            <span
              class="px-2 py-0.5 text-[10px] font-medium rounded-full"
              :class="
                checklistMeta.completed === checklistMeta.total
                  ? 'bg-emerald-50 text-emerald-600'
                  : 'bg-orange-50 text-orange-600'
              "
            >
              {{ checklistMeta.completed }}/{{ checklistMeta.total }}
            </span>
            <ChevronRightIcon class="w-4 h-4 text-gray-300 flex-shrink-0" />
          </button>

          <!-- Dynamic Action MENU 타입 -->
          <button
            v-for="(item, idx) in dynamicMenuItems"
            :key="item.id"
            class="w-full flex items-center gap-3 px-4 py-3 hover:bg-gray-50 transition-colors text-left"
            :class="idx < dynamicMenuItems.length - 1 ? 'border-b border-black/[0.05]' : ''"
            :aria-label="item.title"
            @click="executeAction(item)"
          >
            <div
              class="w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0"
              :style="getDynamicIconStyle(item)"
            >
              <span v-if="getConfig(item).icon" class="text-base leading-none">
                {{ getConfig(item).icon }}
              </span>
              <ZapIcon v-else class="w-4 h-4" />
            </div>
            <span class="flex-1 text-sm font-medium text-gray-900">{{ item.title }}</span>
            <span
              v-if="item.badge"
              class="px-2 py-0.5 text-[10px] font-medium rounded-full"
              :class="item.badgeClass"
            >
              {{ item.badge }}
            </span>
            <ChevronRightIcon class="w-4 h-4 text-gray-300 flex-shrink-0" />
          </button>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  User as UserIcon,
  Users as UsersIcon,
  Calendar as CalendarIcon,
  ChevronDown as ChevronDownIcon,
  ChevronRight as ChevronRightIcon,
  ClipboardList as ClipboardListIcon,
  CheckSquare as CheckSquareIcon,
  Zap as ZapIcon,
} from 'lucide-vue-next'
import apiClient from '@/services/api'
import { useAction } from '@/composables/useAction'

const route = useRoute()
const router = useRouter()
const { executeAction } = useAction()

const isLoading = ref(false)
const isCompanionsOpen = ref(false)
const myInfo = ref({})
const hasSurveys = ref(false)
const incompleteSurveyCount = ref(0)
const checklistMeta = ref(null)
const dynamicMenuItems = ref([])

// AssignmentBadges.vue와 동일한 5색 팔레트
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

const hasAttributes = computed(
  () => myInfo.value.attributes && myInfo.value.attributes.length > 0,
)

const hasMenuItems = computed(
  () => hasSurveys.value || checklistMeta.value || dynamicMenuItems.value.length > 0,
)

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

function getDynamicIconStyle(action) {
  const config = getConfig(action)
  return {
    backgroundColor: config.bgColor || '#EEF2FF',
    color: config.iconColor || '#6366F1',
  }
}

function goToSurveys() {
  router.push({
    name: 'SurveyList',
    params: { conventionId: route.params.conventionId },
  })
}

function goToChecklist() {
  router.push({
    name: 'Checklist',
    params: { conventionId: route.params.conventionId },
  })
}

onMounted(async () => {
  const conventionId = route.params.conventionId
  isLoading.value = true

  try {
    const [myInfoRes, actionsRes, statusesRes, surveysRes, checklistRes] =
      await Promise.all([
        apiClient.get(`/users/my-convention-info/${conventionId}`),
        apiClient.get(`/conventions/${conventionId}/actions/menu`),
        apiClient.get(`/conventions/${conventionId}/actions/statuses`),
        apiClient
          .get(`/surveys/convention/${conventionId}`)
          .catch(() => ({ data: [] })),
        apiClient
          .get(`/conventions/${conventionId}/actions/checklist-status`)
          .catch(() => ({ data: null })),
      ])

    myInfo.value = myInfoRes.data || {}

    const surveys = Array.isArray(surveysRes.data) ? surveysRes.data : []
    hasSurveys.value = surveys.length > 0
    incompleteSurveyCount.value = surveys.filter((s) => !s.isCompleted).length

    const actions = actionsRes.data || []
    const statuses = statusesRes.data || []
    const statusMap = new Map(statuses.map((s) => [s.conventionActionId, s]))

    const checklistItems = []
    const menuItems = []

    for (const action of actions) {
      if (action.actionCategory === 'CHECKLIST_CARD') {
        checklistItems.push(action)
      } else {
        const status = statusMap.get(action.id)
        menuItems.push({
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

    if (checklistItems.length > 0) {
      const completed = checklistItems.filter(
        (c) => statusMap.get(c.id)?.isComplete,
      ).length
      checklistMeta.value = { completed, total: checklistItems.length }
    } else if (checklistRes.data) {
      checklistMeta.value = checklistRes.data
    }

    dynamicMenuItems.value = menuItems
  } catch (error) {
    console.error('Failed to load more features:', error)
  } finally {
    isLoading.value = false
  }
})
</script>
