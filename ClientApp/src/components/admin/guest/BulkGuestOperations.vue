<template>
  <!-- 대량 일정 배정 모달 -->
  <BaseModal
    :is-open="showScheduleModal"
    max-width="2xl"
    @close="closeScheduleModal"
  >
    <template #header>
      <h2 class="text-xl font-semibold">일정 일괄 배정</h2>
    </template>
    <template #body>
      <div class="mb-4 p-4 bg-primary-50 rounded-lg">
        <p class="text-sm font-medium text-primary-900">
          선택된 참석자: {{ selectedGuests.length }}명
        </p>
        <div class="flex flex-wrap gap-2 mt-2">
          <span
            v-for="id in selectedGuests.slice(0, 5)"
            :key="id"
            class="px-2 py-1 bg-white text-sm rounded"
          >
            {{ getGuestNameById(id) }}
          </span>
          <span
            v-if="selectedGuests.length > 5"
            class="px-2 py-1 bg-white text-sm rounded"
          >
            +{{ selectedGuests.length - 5 }}명
          </span>
        </div>
      </div>

      <div class="mb-4">
        <label class="block text-sm font-medium mb-2">배정할 일정 선택</label>
        <div class="space-y-2 max-h-96 overflow-y-auto border rounded-lg p-3">
          <label
            v-for="template in availableTemplates"
            :key="template.id"
            class="flex items-start gap-2 p-3 border rounded hover:bg-gray-50 cursor-pointer"
          >
            <input
              v-model="bulkAssignTemplateIds"
              type="checkbox"
              :value="template.id"
              class="rounded mt-1"
            />
            <div class="flex-1">
              <div class="font-medium">{{ template.courseName }}</div>
              <div v-if="template.description" class="text-sm text-gray-600">
                {{ template.description }}
              </div>
              <div class="text-xs text-gray-500 mt-1">
                일정 {{ template.scheduleItems?.length || 0 }}개 • 참석자
                {{ template.guestCount || 0 }}명
              </div>
            </div>
          </label>
        </div>
      </div>

      <div class="mb-4">
        <label class="flex items-center gap-2">
          <input v-model="bulkAssignReplace" type="checkbox" class="rounded" />
          <span class="text-sm text-gray-700"
            >기존 일정을 대체 (체크 시 기존 일정 삭제 후 새 일정 배정)</span
          >
        </label>
      </div>
    </template>
    <template #footer>
      <button
        class="px-4 py-2 border rounded-lg hover:bg-gray-50"
        @click="closeScheduleModal"
      >
        취소
      </button>
      <button
        :disabled="bulkAssignTemplateIds.length === 0"
        class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:bg-gray-300 disabled:cursor-not-allowed whitespace-nowrap overflow-hidden text-ellipsis"
        @click="executeBulkAssign"
      >
        {{ selectedGuests.length }}명에게 일정 배정
      </button>
    </template>
  </BaseModal>

  <!-- 속성 일괄 매핑 모달 -->
  <BaseModal
    :is-open="showAttributeModal"
    max-width="2xl"
    @close="closeAttributeModal"
  >
    <template #header>
      <h2 class="text-xl font-semibold">
        속성 일괄 매핑 ({{ selectedGuests.length }}명)
      </h2>
    </template>
    <template #body>
      <div class="mb-4 p-3 bg-primary-50 rounded-lg">
        <p class="text-sm text-primary-800">
          선택한 {{ selectedGuests.length }}명의 참석자에게 동일한 속성을
          설정합니다.
        </p>
      </div>

      <div class="space-y-4">
        <div
          v-for="template in attributeTemplates"
          :key="template.id"
          class="space-y-2"
        >
          <label class="block text-sm font-medium text-gray-700">
            {{ template.attributeKey }}
          </label>

          <!-- 선택 값이 있는 경우 드롭다운 -->
          <select
            v-if="template.attributeValues"
            v-model="bulkAttributeForm[template.attributeKey]"
            class="w-full px-3 py-2 border rounded-lg"
          >
            <option value="">선택하세요</option>
            <option
              v-for="value in parseAttributeValues(template.attributeValues)"
              :key="value"
              :value="value"
            >
              {{ value }}
            </option>
          </select>

          <!-- 선택 값이 없는 경우 텍스트 입력 -->
          <input
            v-else
            v-model="bulkAttributeForm[template.attributeKey]"
            type="text"
            class="w-full px-3 py-2 border rounded-lg"
            :placeholder="`${template.attributeKey} 입력`"
          />
        </div>

        <!-- 미리보기 -->
        <div class="mt-6 p-4 bg-gray-50 rounded-lg">
          <h4 class="text-sm font-semibold mb-2">미리보기</h4>
          <div class="space-y-2 max-h-40 overflow-y-auto">
            <div
              v-for="guestId in selectedGuests.slice(0, 5)"
              :key="guestId"
              class="text-sm"
            >
              <span class="font-medium">{{ getGuestNameById(guestId) }}</span>
              <span class="text-gray-600 ml-2">
                →
                <span
                  v-for="(value, key) in bulkAttributeForm"
                  :key="key"
                  class="ml-1"
                >
                  <template v-if="value"> {{ key }}: {{ value }} </template>
                </span>
              </span>
            </div>
            <div v-if="selectedGuests.length > 5" class="text-xs text-gray-500">
              ... 외 {{ selectedGuests.length - 5 }}명
            </div>
          </div>
        </div>
      </div>
    </template>
    <template #footer>
      <button
        class="px-4 py-2 border rounded-lg hover:bg-gray-50"
        @click="closeAttributeModal"
      >
        취소
      </button>
      <button
        :disabled="submittingAttribute"
        class="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:opacity-50 whitespace-nowrap overflow-hidden text-ellipsis"
        @click="submitBulkAttribute"
      >
        {{ submittingAttribute ? '저장 중...' : '일괄 저장' }}
      </button>
    </template>
  </BaseModal>
</template>

<script setup>
import { ref } from 'vue'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
  selectedGuests: { type: Array, required: true },
  guests: { type: Array, required: true },
  availableTemplates: { type: Array, required: true },
  attributeTemplates: { type: Array, required: true },
  showScheduleModal: { type: Boolean, required: true },
  showAttributeModal: { type: Boolean, required: true },
})

const emit = defineEmits(['close-schedule', 'close-attribute', 'completed'])

const bulkAssignTemplateIds = ref([])
const bulkAssignReplace = ref(false)
const bulkAttributeForm = ref({})
const submittingAttribute = ref(false)

const parseAttributeValues = (valuesStr) => {
  if (!valuesStr) return []
  try {
    return JSON.parse(valuesStr)
  } catch {
    return []
  }
}

const getGuestNameById = (guestId) => {
  const guest = props.guests.find((g) => g.id === guestId)
  return guest ? guest.guestName : ''
}

const closeScheduleModal = () => {
  bulkAssignTemplateIds.value = []
  bulkAssignReplace.value = false
  emit('close-schedule')
}

const closeAttributeModal = () => {
  bulkAttributeForm.value = {}
  emit('close-attribute')
}

const executeBulkAssign = async () => {
  if (bulkAssignTemplateIds.value.length === 0) {
    alert('배정할 일정을 선택해주세요.')
    return
  }

  const confirmMessage = bulkAssignReplace.value
    ? `${props.selectedGuests.length}명의 기존 일정을 모두 삭제하고 새 일정을 배정하시겠습니까?`
    : `${props.selectedGuests.length}명에게 일정을 추가로 배정하시겠습니까?`

  if (!confirm(confirmMessage)) return

  try {
    let successCount = 0
    let failCount = 0

    for (const guestId of props.selectedGuests) {
      try {
        if (bulkAssignReplace.value) {
          await apiClient.post(
            `/admin/conventions/${props.conventionId}/guests/${guestId}/schedules`,
            {
              scheduleTemplateIds: bulkAssignTemplateIds.value,
            },
          )
        } else {
          const guest = props.guests.find((g) => g.id === guestId)
          const existingIds =
            guest?.scheduleTemplates.map((st) => st.scheduleTemplateId) || []
          const mergedIds = [
            ...new Set([...existingIds, ...bulkAssignTemplateIds.value]),
          ]

          await apiClient.post(
            `/admin/conventions/${props.conventionId}/guests/${guestId}/schedules`,
            {
              scheduleTemplateIds: mergedIds,
            },
          )
        }
        successCount++
      } catch (error) {
        console.error(`Failed to assign schedules to guest ${guestId}:`, error)
        failCount++
      }
    }

    closeScheduleModal()
    emit('completed')
    alert(`완료! 성공: ${successCount}명, 실패: ${failCount}명`)
  } catch (error) {
    console.error('Bulk assign failed:', error)
    alert('일괄 배정 실패: ' + (error.response?.data?.message || error.message))
  }
}

const submitBulkAttribute = async () => {
  // 빈 값 제거
  const filteredAttributes = Object.entries(bulkAttributeForm.value)
    .filter(([_, value]) => value !== '')
    .reduce((acc, [key, value]) => {
      acc[key] = value
      return acc
    }, {})

  if (Object.keys(filteredAttributes).length === 0) {
    alert('최소 하나의 속성을 입력해주세요')
    return
  }

  if (
    !confirm(
      `${props.selectedGuests.length}명의 참석자에게 속성을 일괄 매핑하시겠습니까?`,
    )
  ) {
    return
  }

  submittingAttribute.value = true

  try {
    const payload = {
      conventionId: props.conventionId,
      guestMappings: props.selectedGuests.map((guestId) => ({
        guestId,
        attributes: filteredAttributes,
      })),
    }

    const response = await apiClient.post(
      '/guest/bulk-assign-attributes',
      payload,
    )

    if (response.data.success) {
      alert(response.data.message)
      closeAttributeModal()
      emit('completed')
    } else {
      alert(response.data.message)
    }
  } catch (error) {
    console.error('Failed to bulk assign attributes:', error)
    alert(
      '속성 할당에 실패했습니다: ' +
        (error.response?.data?.message || error.message),
    )
  } finally {
    submittingAttribute.value = false
  }
}
</script>
