<template>
  <BaseModal :is-open="isOpen" max-width="6xl" @close="$emit('close')">
    <template #header>
      <h2 class="text-xl font-semibold">일정 업로드 미리보기</h2>
    </template>

    <template #body>
      <div v-if="preview" class="space-y-4">
        <!-- 전체 요약 통계 -->
        <div class="grid grid-cols-4 gap-3">
          <div class="bg-gray-50 rounded-lg p-3 text-center">
            <p class="text-xs text-gray-500">총 행</p>
            <p class="text-2xl font-bold text-gray-900">
              {{ preview.totalRows }}
            </p>
          </div>
          <div class="bg-green-50 rounded-lg p-3 text-center">
            <p class="text-xs text-green-600">유효</p>
            <p class="text-2xl font-bold text-green-700">
              {{ preview.validRows }}
            </p>
          </div>
          <div class="bg-yellow-50 rounded-lg p-3 text-center">
            <p class="text-xs text-yellow-600">경고</p>
            <p class="text-2xl font-bold text-yellow-700">
              {{ preview.warningRows }}
            </p>
          </div>
          <div class="bg-red-50 rounded-lg p-3 text-center">
            <p class="text-xs text-red-600">스킵</p>
            <p class="text-2xl font-bold text-red-700">
              {{ preview.skippedRows }}
            </p>
          </div>
        </div>

        <!-- 충돌 경고 -->
        <div
          v-if="preview.conflictRows > 0"
          class="bg-orange-50 border border-orange-200 rounded-lg p-3"
        >
          <p class="text-sm text-orange-800">
            ⚠️ 기존 일정과
            <strong>{{ preview.conflictRows }}건</strong>의 날짜·시간 충돌이
            있습니다. 저장은 가능하지만 확인이 필요합니다.
          </p>
        </div>

        <!-- 전역 에러 -->
        <div
          v-if="preview.errors && preview.errors.length > 0"
          class="bg-red-50 border border-red-200 rounded-lg p-3"
        >
          <p
            v-for="(err, i) in preview.errors"
            :key="i"
            class="text-sm text-red-700"
          >
            {{ err }}
          </p>
        </div>

        <!-- 멀티시트 모드 (시트 2개 이상) -->
        <template v-if="isMultiSheet">
          <!-- 시트 탭 -->
          <div class="border-b border-gray-200">
            <nav class="-mb-px flex space-x-4 overflow-x-auto">
              <button
                v-for="(sheet, idx) in preview.sheets"
                :key="idx"
                :class="[
                  'whitespace-nowrap py-2 px-3 border-b-2 font-medium text-sm',
                  activeSheetIdx === idx
                    ? 'border-purple-500 text-purple-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
                ]"
                @click="activeSheetIdx = idx"
              >
                {{ sheet.sheetName }}
                <span class="ml-1 text-xs text-gray-400"
                  >({{ sheet.items.length }}행)</span
                >
              </button>
            </nav>
          </div>

          <!-- 현재 시트 내용 -->
          <template v-if="activeSheet">
            <!-- 시트 요약 -->
            <div class="grid grid-cols-4 gap-2">
              <div class="bg-gray-50 rounded p-2 text-center">
                <p class="text-xs text-gray-500">총 행</p>
                <p class="font-bold text-gray-900">
                  {{ activeSheet.totalRows }}
                </p>
              </div>
              <div class="bg-green-50 rounded p-2 text-center">
                <p class="text-xs text-green-600">유효</p>
                <p class="font-bold text-green-700">
                  {{ activeSheet.validRows }}
                </p>
              </div>
              <div class="bg-yellow-50 rounded p-2 text-center">
                <p class="text-xs text-yellow-600">경고</p>
                <p class="font-bold text-yellow-700">
                  {{ activeSheet.warningRows }}
                </p>
              </div>
              <div class="bg-red-50 rounded p-2 text-center">
                <p class="text-xs text-red-600">스킵</p>
                <p class="font-bold text-red-700">
                  {{ activeSheet.skippedRows }}
                </p>
              </div>
            </div>

            <!-- 코스명 입력 (시트별) -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                코스명 <span class="text-red-500">*</span>
                <span class="text-xs text-gray-400 ml-1"
                  >(시트: {{ activeSheet.sheetName }})</span
                >
              </label>
              <input
                v-model="sheetCourseNames[activeSheetIdx]"
                type="text"
                class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                :placeholder="`예: ${activeSheet.sheetName}`"
              />
            </div>

            <!-- 시트 아이템 테이블 -->
            <SheetPreviewTable :items="activeSheet.items" />
          </template>
        </template>

        <!-- 단일 시트 모드 -->
        <template v-else>
          <!-- 코스명 입력 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              코스명 <span class="text-red-500">*</span>
            </label>
            <input
              v-model="singleCourseName"
              type="text"
              class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              placeholder="예: A조 일정, 관광 코스"
            />
          </div>

          <!-- 단일 시트 아이템 테이블 -->
          <SheetPreviewTable :items="preview.items" />
        </template>

        <!-- 저장 결과 -->
        <div
          v-if="saveResult"
          class="p-3 rounded-lg"
          :class="
            saveResult.success
              ? 'bg-green-50 text-green-700'
              : 'bg-red-50 text-red-700'
          "
        >
          <p class="text-sm font-medium">
            {{
              saveResult.success
                ? `저장 완료! ${saveResult.templatesCreated}개 코스 / ${saveResult.itemsCreated}개 일정이 추가되었습니다.`
                : saveResult.errors?.join(', ') || '저장 실패'
            }}
          </p>
          <ul
            v-if="saveResult.warnings?.length"
            class="mt-1 text-xs space-y-0.5"
          >
            <li v-for="(w, i) in saveResult.warnings" :key="i">{{ w }}</li>
          </ul>
        </div>
      </div>

      <div v-else class="text-center py-8 text-gray-400">
        파일을 먼저 업로드하세요
      </div>
    </template>

    <template #footer>
      <button
        class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200"
        @click="$emit('close')"
      >
        {{ saveResult?.success ? '닫기' : '취소' }}
      </button>
      <button
        v-if="!saveResult?.success"
        class="px-4 py-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600 disabled:bg-gray-300"
        :disabled="!canSave || saving"
        @click="confirmSave"
      >
        {{ saving ? '저장 중...' : `${totalSaveCount}개 일정 저장` }}
      </button>
    </template>
  </BaseModal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'

// 인라인 서브컴포넌트: 아이템 테이블 (단일/멀티 공용)
const SheetPreviewTable = {
  name: 'SheetPreviewTable',
  props: { items: { type: Array, default: () => [] } },
  template: `
    <div class="border rounded-lg overflow-hidden">
      <div class="max-h-[40vh] overflow-y-auto">
        <table class="w-full text-sm">
          <thead class="bg-gray-50 sticky top-0 z-10">
            <tr>
              <th class="px-2 py-2 text-left w-10">행</th>
              <th class="px-2 py-2 text-left w-14">상태</th>
              <th class="px-2 py-2 text-left w-24">날짜</th>
              <th class="px-2 py-2 text-left w-16">시작</th>
              <th class="px-2 py-2 text-left w-16">종료</th>
              <th class="px-2 py-2 text-left w-28">장소</th>
              <th class="px-2 py-2 text-left">제목</th>
              <th class="px-2 py-2 text-left w-24">노출속성</th>
              <th class="px-2 py-2 text-left w-10">충돌</th>
            </tr>
          </thead>
          <tbody class="divide-y">
            <tr
              v-for="item in items"
              :key="item.row"
              :class="rowClass(item)"
            >
              <td class="px-2 py-2 text-gray-500">{{ item.row }}</td>
              <td class="px-2 py-2">
                <span class="px-1.5 py-0.5 rounded text-xs font-medium" :class="statusClass(item.status)">
                  {{ statusLabel(item.status) }}
                </span>
              </td>
              <td class="px-2 py-2">{{ item.date || '-' }}</td>
              <td class="px-2 py-2">{{ item.startTime || '-' }}</td>
              <td class="px-2 py-2">{{ item.endTime || '-' }}</td>
              <td class="px-2 py-2 truncate max-w-[120px]">{{ item.location || '-' }}</td>
              <td class="px-2 py-2">
                <div class="truncate max-w-[200px]">{{ item.title || '-' }}</div>
                <p v-if="item.message" class="text-xs text-red-500 mt-0.5">{{ item.message }}</p>
              </td>
              <td class="px-2 py-2 text-xs text-purple-600 truncate max-w-[96px]">
                {{ item.visibleAttributes || '-' }}
              </td>
              <td class="px-2 py-2 text-center">
                <span v-if="item.hasConflict" class="text-orange-500" :title="item.conflictDetail">⚠️</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  methods: {
    rowClass(item) {
      if (item.hasConflict) return 'bg-orange-50'
      if (item.status === 'skipped') return 'bg-red-50 text-gray-400'
      if (item.status === 'warning') return 'bg-yellow-50'
      return ''
    },
    statusClass(status) {
      if (status === 'skipped') return 'bg-red-100 text-red-700'
      if (status === 'warning') return 'bg-yellow-100 text-yellow-700'
      return 'bg-green-100 text-green-700'
    },
    statusLabel(status) {
      if (status === 'skipped') return '스킵'
      if (status === 'warning') return '경고'
      return '유효'
    },
  },
}

const props = defineProps({
  isOpen: { type: Boolean, required: true },
  conventionId: { type: Number, required: true },
  preview: { type: Object, default: null },
})

const emit = defineEmits(['close', 'saved'])

// 멀티시트 여부
const isMultiSheet = computed(() => props.preview?.sheets?.length > 1)

// 멀티시트 상태
const activeSheetIdx = ref(0)
const activeSheet = computed(
  () => props.preview?.sheets?.[activeSheetIdx.value] ?? null,
)
const sheetCourseNames = ref([])

// 단일 시트 상태
const singleCourseName = ref('')

const saving = ref(false)
const saveResult = ref(null)

watch(
  () => props.isOpen,
  (open) => {
    if (!open) return
    saveResult.value = null
    activeSheetIdx.value = 0

    const today = new Date().toISOString().slice(0, 10)

    if (props.preview?.sheets?.length > 0) {
      // 시트명을 기본 코스명으로
      sheetCourseNames.value = props.preview.sheets.map(
        (s) => s.sheetName || `업로드_${today}`,
      )
    } else {
      sheetCourseNames.value = []
    }
    singleCourseName.value = `업로드_${today}`
  },
)

// 저장 가능 수량
const totalSaveCount = computed(() => {
  if (isMultiSheet.value) {
    return (
      props.preview?.sheets?.reduce(
        (sum, s) =>
          sum + (s.items?.filter((i) => i.status !== 'skipped').length || 0),
        0,
      ) || 0
    )
  }
  return props.preview?.items?.filter((i) => i.status !== 'skipped').length || 0
})

const canSave = computed(() => {
  if (isMultiSheet.value) {
    return (
      totalSaveCount.value > 0 &&
      sheetCourseNames.value.every((n) => n?.trim().length > 0)
    )
  }
  return singleCourseName.value.trim().length > 0 && totalSaveCount.value > 0
})

async function confirmSave() {
  saving.value = true
  try {
    let res
    if (isMultiSheet.value) {
      // 멀티시트 일괄 저장
      const sheets = props.preview.sheets.map((s, idx) => ({
        courseName: sheetCourseNames.value[idx]?.trim() || s.sheetName,
        description: '일정 엑셀 업로드',
        items: s.items,
      }))
      res = await apiClient.post(
        `/upload/conventions/${props.conventionId}/schedule-templates/confirm-multi`,
        { sheets },
      )
    } else {
      // 단일 시트 저장 (기존 호환)
      res = await apiClient.post(
        `/upload/conventions/${props.conventionId}/schedule-templates/confirm`,
        {
          courseName: singleCourseName.value.trim(),
          description: '일정 엑셀 업로드',
          items: props.preview.items,
        },
      )
    }
    saveResult.value = res.data
    if (res.data.success) {
      emit('saved', res.data)
    }
  } catch (e) {
    saveResult.value = {
      success: false,
      errors: [e.response?.data?.error || e.message],
    }
  } finally {
    saving.value = false
  }
}
</script>
