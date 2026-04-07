<template>
  <BaseModal :is-open="isOpen" max-width="6xl" @close="$emit('close')">
    <template #header>
      <h2 class="text-xl font-semibold">일정 업로드 미리보기</h2>
    </template>

    <template #body>
      <div v-if="preview" class="space-y-4">
        <!-- 요약 통계 -->
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

        <!-- 에러 (전체 실패) -->
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

        <!-- 코스명 입력 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1"
            >코스명 <span class="text-red-500">*</span></label
          >
          <input
            v-model="courseName"
            type="text"
            class="w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            placeholder="예: A조 일정, 관광 코스"
          />
        </div>

        <!-- 미리보기 테이블 -->
        <div class="border rounded-lg overflow-hidden">
          <div class="max-h-[50vh] overflow-y-auto">
            <table class="w-full text-sm">
              <thead class="bg-gray-50 sticky top-0 z-10">
                <tr>
                  <th class="px-2 py-2 text-left w-12">행</th>
                  <th class="px-2 py-2 text-left w-16">상태</th>
                  <th class="px-2 py-2 text-left w-28">날짜</th>
                  <th class="px-2 py-2 text-left w-20">시작</th>
                  <th class="px-2 py-2 text-left w-20">종료</th>
                  <th class="px-2 py-2 text-left w-32">장소</th>
                  <th class="px-2 py-2 text-left">제목</th>
                  <th class="px-2 py-2 text-left w-12">충돌</th>
                </tr>
              </thead>
              <tbody class="divide-y">
                <tr
                  v-for="item in preview.items"
                  :key="item.row"
                  :class="getRowClass(item)"
                >
                  <td class="px-2 py-2 text-gray-500">{{ item.row }}</td>
                  <td class="px-2 py-2">
                    <span
                      class="px-1.5 py-0.5 rounded text-xs font-medium"
                      :class="getStatusClass(item.status)"
                    >
                      {{ getStatusLabel(item.status) }}
                    </span>
                  </td>
                  <td class="px-2 py-2">{{ item.date || '-' }}</td>
                  <td class="px-2 py-2">{{ item.startTime || '-' }}</td>
                  <td class="px-2 py-2">{{ item.endTime || '-' }}</td>
                  <td class="px-2 py-2 truncate max-w-[150px]">
                    {{ item.location || '-' }}
                  </td>
                  <td class="px-2 py-2">
                    <div class="truncate max-w-[300px]">
                      {{ item.title || '-' }}
                    </div>
                    <p v-if="item.message" class="text-xs text-red-500 mt-0.5">
                      {{ item.message }}
                    </p>
                  </td>
                  <td class="px-2 py-2 text-center">
                    <span
                      v-if="item.hasConflict"
                      class="text-orange-500"
                      :title="item.conflictDetail"
                      >⚠️</span
                    >
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

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
                ? `✅ 저장 완료! ${saveResult.itemsCreated}개 일정이 추가되었습니다.`
                : saveResult.errors?.join(', ') || '저장 실패'
            }}
          </p>
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
        {{ saving ? '저장 중...' : `${validSaveCount}개 일정 저장` }}
      </button>
    </template>
  </BaseModal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({
  isOpen: { type: Boolean, required: true },
  conventionId: { type: Number, required: true },
  preview: { type: Object, default: null },
})

const emit = defineEmits(['close', 'saved'])

const courseName = ref('')
const saving = ref(false)
const saveResult = ref(null)

watch(
  () => props.isOpen,
  (open) => {
    if (open) {
      courseName.value = `업로드_${new Date().toISOString().slice(0, 10)}`
      saveResult.value = null
    }
  },
)

const validSaveCount = computed(
  () => props.preview?.items?.filter((i) => i.status !== 'skipped').length || 0,
)

const canSave = computed(
  () => courseName.value.trim().length > 0 && validSaveCount.value > 0,
)

function getRowClass(item) {
  if (item.hasConflict) return 'bg-orange-50'
  if (item.status === 'skipped') return 'bg-red-50 text-gray-400'
  if (item.status === 'warning') return 'bg-yellow-50'
  return ''
}

function getStatusClass(status) {
  if (status === 'skipped') return 'bg-red-100 text-red-700'
  if (status === 'warning') return 'bg-yellow-100 text-yellow-700'
  return 'bg-green-100 text-green-700'
}

function getStatusLabel(status) {
  if (status === 'skipped') return '스킵'
  if (status === 'warning') return '경고'
  return '유효'
}

async function confirmSave() {
  saving.value = true
  try {
    const res = await apiClient.post(
      `/upload/conventions/${props.conventionId}/schedule-templates/confirm`,
      {
        courseName: courseName.value.trim(),
        description: '일정 엑셀 업로드',
        items: props.preview.items,
      },
    )
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
