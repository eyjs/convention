<template>
  <!-- 그룹 단위 일정 배정 모달 -->
  <BaseModal
    :is-open="showAssignModal"
    max-width="2xl"
    @close="closeAssignModal"
  >
    <template #header>
      <h2 class="text-xl font-semibold">그룹별 일정 배정</h2>
    </template>
    <template #body>
      <div class="space-y-6">
        <!-- 그룹 선택 -->
        <div>
          <label class="block text-sm font-medium mb-2">그룹 선택 *</label>
          <div
            v-if="availableGroups.length === 0"
            class="text-sm text-gray-500 p-3 bg-gray-50 rounded"
          >
            등록된 그룹이 없습니다. 참석자 업로드 시 그룹 정보를 포함해주세요.
          </div>
          <div
            v-else
            class="space-y-2 max-h-48 overflow-y-auto border rounded-lg p-3"
          >
            <label
              v-for="group in availableGroups"
              :key="group"
              class="flex items-center gap-3 p-3 border rounded hover:bg-gray-50 cursor-pointer"
            >
              <input
                v-model="selectedGroupsForAssign"
                type="checkbox"
                :value="group"
                class="rounded"
              />
              <div class="flex-1">
                <div class="font-medium">{{ group }}</div>
                <div class="text-xs text-gray-500">
                  {{ getGuestCountByGroup(group) }}명
                </div>
              </div>
            </label>
          </div>
        </div>

        <!-- 선택된 그룹 요약 -->
        <div
          v-if="selectedGroupsForAssign.length > 0"
          class="p-4 bg-orange-50 rounded-lg"
        >
          <p class="text-sm font-medium text-orange-900">
            선택된 그룹: {{ selectedGroupsForAssign.length }}개 (총
            {{ totalGuestCountForAssign }}명)
          </p>
          <div class="flex flex-wrap gap-2 mt-2">
            <span
              v-for="group in selectedGroupsForAssign"
              :key="group"
              class="px-2 py-1 bg-white text-sm rounded"
            >
              {{ group }}
            </span>
          </div>
        </div>

        <!-- 일정 선택 -->
        <div>
          <label class="block text-sm font-medium mb-2"
            >배정할 일정 선택 *</label
          >
          <div
            v-if="availableTemplates.length === 0"
            class="text-sm text-gray-500 p-3 bg-gray-50 rounded"
          >
            일정 템플릿이 없습니다. 먼저 일정 관리에서 템플릿을 생성하세요.
          </div>
          <div
            v-else
            class="space-y-2 max-h-60 overflow-y-auto border rounded-lg p-3"
          >
            <label
              v-for="template in availableTemplates"
              :key="template.id"
              class="flex items-start gap-2 p-3 border rounded hover:bg-gray-50 cursor-pointer"
            >
              <input
                v-model="groupAssignTemplateIds"
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
                  일정 {{ template.scheduleItems?.length || 0 }}개
                </div>
              </div>
            </label>
          </div>
        </div>
      </div>
    </template>
    <template #footer>
      <button
        class="px-4 py-2 border rounded-lg hover:bg-gray-50"
        @click="closeAssignModal"
      >
        취소
      </button>
      <button
        :disabled="
          selectedGroupsForAssign.length === 0 ||
          groupAssignTemplateIds.length === 0 ||
          groupAssigning
        "
        class="px-4 py-2 bg-orange-600 text-white rounded-lg hover:bg-orange-700 disabled:bg-gray-300 disabled:cursor-not-allowed whitespace-nowrap overflow-hidden text-ellipsis"
        @click="executeGroupAssign"
      >
        {{ groupAssigning ? '배정 중...' : '그룹에 일정 배정' }}
      </button>
    </template>
  </BaseModal>

  <!-- 그룹 단위 일정 해제 모달 -->
  <BaseModal
    :is-open="showRemoveModal"
    max-width="2xl"
    @close="closeRemoveModal"
  >
    <template #header>
      <h2 class="text-xl font-semibold text-red-600">그룹별 일정 해제</h2>
    </template>
    <template #body>
      <div class="space-y-6">
        <!-- 그룹 선택 -->
        <div>
          <label class="block text-sm font-medium mb-2">그룹 선택 *</label>
          <div
            v-if="availableGroups.length === 0"
            class="text-sm text-gray-500 p-3 bg-gray-50 rounded"
          >
            등록된 그룹이 없습니다.
          </div>
          <div
            v-else
            class="space-y-2 max-h-48 overflow-y-auto border rounded-lg p-3"
          >
            <label
              v-for="group in availableGroups"
              :key="group"
              class="flex items-center gap-3 p-3 border rounded hover:bg-red-50 cursor-pointer"
            >
              <input
                v-model="selectedGroupsForRemove"
                type="checkbox"
                :value="group"
                class="rounded text-red-600"
              />
              <div class="flex-1">
                <div class="font-medium">{{ group }}</div>
                <div class="text-xs text-gray-500">
                  {{ getGuestCountByGroup(group) }}명
                </div>
              </div>
            </label>
          </div>
        </div>

        <!-- 선택된 그룹 요약 -->
        <div
          v-if="selectedGroupsForRemove.length > 0"
          class="p-4 bg-red-50 rounded-lg"
        >
          <p class="text-sm font-medium text-red-900">
            선택된 그룹: {{ selectedGroupsForRemove.length }}개 (총
            {{ totalGuestCountForRemove }}명)
          </p>
        </div>

        <!-- 해제할 일정 선택 -->
        <div>
          <label class="block text-sm font-medium mb-2"
            >해제할 일정 선택 *</label
          >
          <div
            v-if="availableTemplates.length === 0"
            class="text-sm text-gray-500 p-3 bg-gray-50 rounded"
          >
            일정 템플릿이 없습니다.
          </div>
          <div
            v-else
            class="space-y-2 max-h-60 overflow-y-auto border rounded-lg p-3"
          >
            <label
              v-for="template in availableTemplates"
              :key="template.id"
              class="flex items-start gap-2 p-3 border rounded hover:bg-red-50 cursor-pointer"
            >
              <input
                v-model="groupRemoveTemplateIds"
                type="checkbox"
                :value="template.id"
                class="rounded mt-1 text-red-600"
              />
              <div class="flex-1">
                <div class="font-medium">{{ template.courseName }}</div>
                <div v-if="template.description" class="text-sm text-gray-600">
                  {{ template.description }}
                </div>
              </div>
            </label>
          </div>
        </div>

        <div class="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
          <p class="text-sm text-yellow-800">
            선택한 그룹의 참석자들에게서 선택한 일정이 해제됩니다. 이 작업은
            되돌릴 수 없습니다.
          </p>
        </div>
      </div>
    </template>
    <template #footer>
      <button
        class="px-4 py-2 border rounded-lg hover:bg-gray-50"
        @click="closeRemoveModal"
      >
        취소
      </button>
      <button
        :disabled="
          selectedGroupsForRemove.length === 0 ||
          groupRemoveTemplateIds.length === 0 ||
          groupRemoving
        "
        class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:bg-gray-300 disabled:cursor-not-allowed"
        @click="executeGroupRemove"
      >
        {{ groupRemoving ? '해제 중...' : '일정 해제' }}
      </button>
    </template>
  </BaseModal>

  <!-- 그룹별 속성 초기화 모달 -->
  <BaseModal
    :is-open="showAttributeResetModal"
    max-width="2xl"
    @close="closeAttributeResetModal"
  >
    <template #header>
      <h2 class="text-xl font-semibold text-gray-700">그룹별 속성 초기화</h2>
    </template>
    <template #body>
      <div class="space-y-6">
        <!-- 그룹 선택 -->
        <div>
          <label class="block text-sm font-medium mb-2">그룹 선택 *</label>
          <div
            v-if="availableGroups.length === 0"
            class="text-sm text-gray-500 p-3 bg-gray-50 rounded"
          >
            등록된 그룹이 없습니다.
          </div>
          <div
            v-else
            class="space-y-2 max-h-48 overflow-y-auto border rounded-lg p-3"
          >
            <label
              v-for="group in availableGroups"
              :key="group"
              class="flex items-center gap-3 p-3 border rounded hover:bg-gray-100 cursor-pointer"
            >
              <input
                v-model="selectedGroupsForAttributeReset"
                type="checkbox"
                :value="group"
                class="rounded"
              />
              <div class="flex-1">
                <div class="font-medium">{{ group }}</div>
                <div class="text-xs text-gray-500">
                  {{ getGuestCountByGroup(group) }}명
                </div>
              </div>
            </label>
          </div>
        </div>

        <!-- 선택된 그룹 요약 -->
        <div
          v-if="selectedGroupsForAttributeReset.length > 0"
          class="p-4 bg-gray-100 rounded-lg"
        >
          <p class="text-sm font-medium text-gray-900">
            선택된 그룹: {{ selectedGroupsForAttributeReset.length }}개 (총
            {{ totalGuestCountForAttributeReset }}명)
          </p>
        </div>

        <!-- 삭제할 속성 선택 -->
        <div>
          <label class="block text-sm font-medium mb-2"
            >삭제할 속성 선택 *</label
          >
          <div
            v-if="availableAttributeKeys.length === 0"
            class="text-sm text-gray-500 p-3 bg-gray-50 rounded"
          >
            등록된 속성이 없습니다.
          </div>
          <div
            v-else
            class="space-y-2 max-h-60 overflow-y-auto border rounded-lg p-3"
          >
            <label
              v-for="attrKey in availableAttributeKeys"
              :key="attrKey"
              class="flex items-center gap-2 p-3 border rounded hover:bg-gray-100 cursor-pointer"
            >
              <input
                v-model="selectedAttributeKeysForReset"
                type="checkbox"
                :value="attrKey"
                class="rounded"
              />
              <span class="font-medium">{{ attrKey }}</span>
            </label>
          </div>
        </div>

        <div class="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
          <p class="text-sm text-yellow-800">
            선택한 그룹의 참석자들에게서 선택한 속성이 삭제됩니다. 이 작업은
            되돌릴 수 없습니다.
          </p>
        </div>
      </div>
    </template>
    <template #footer>
      <button
        class="px-4 py-2 border rounded-lg hover:bg-gray-50"
        @click="closeAttributeResetModal"
      >
        취소
      </button>
      <button
        :disabled="
          selectedGroupsForAttributeReset.length === 0 ||
          selectedAttributeKeysForReset.length === 0 ||
          attributeResetting
        "
        class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 disabled:bg-gray-300 disabled:cursor-not-allowed"
        @click="executeGroupAttributeReset"
      >
        {{ attributeResetting ? '삭제 중...' : '속성 삭제' }}
      </button>
    </template>
  </BaseModal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
  guests: { type: Array, required: true },
  availableTemplates: { type: Array, required: true },
  showAssignModal: { type: Boolean, required: true },
  showRemoveModal: { type: Boolean, required: true },
  showAttributeResetModal: { type: Boolean, required: true },
})

const emit = defineEmits([
  'close-assign',
  'close-remove',
  'close-attribute-reset',
  'completed',
])

// 그룹 목록 (guests에서 추출)
const availableGroups = ref([])
const availableAttributeKeys = ref([])

// 일정 배정
const selectedGroupsForAssign = ref([])
const groupAssignTemplateIds = ref([])
const groupAssigning = ref(false)

// 일정 해제
const selectedGroupsForRemove = ref([])
const groupRemoveTemplateIds = ref([])
const groupRemoving = ref(false)

// 속성 초기화
const selectedGroupsForAttributeReset = ref([])
const selectedAttributeKeysForReset = ref([])
const attributeResetting = ref(false)

const loadGroups = () => {
  const groups = [
    ...new Set(
      props.guests
        .filter((g) => g.groupName && g.groupName.trim() !== '')
        .map((g) => g.groupName),
    ),
  ].sort()
  availableGroups.value = groups
}

const loadAttributeKeys = () => {
  const keys = new Set()
  props.guests.forEach((guest) => {
    if (guest.attributes && Array.isArray(guest.attributes)) {
      guest.attributes.forEach((attr) => {
        if (attr.attributeKey) {
          keys.add(attr.attributeKey)
        }
      })
    }
  })
  availableAttributeKeys.value = [...keys].sort()
}

const getGuestCountByGroup = (groupName) => {
  return props.guests.filter((g) => g.groupName === groupName).length
}

// 모달이 열릴 때 그룹 목록 로드
watch(
  () => props.showAssignModal,
  (val) => {
    if (val) loadGroups()
  },
)
watch(
  () => props.showRemoveModal,
  (val) => {
    if (val) loadGroups()
  },
)
watch(
  () => props.showAttributeResetModal,
  (val) => {
    if (val) {
      loadGroups()
      loadAttributeKeys()
    }
  },
)

const totalGuestCountForAssign = computed(() => {
  return selectedGroupsForAssign.value.reduce((total, groupName) => {
    return total + getGuestCountByGroup(groupName)
  }, 0)
})

const totalGuestCountForRemove = computed(() => {
  return selectedGroupsForRemove.value.reduce((total, groupName) => {
    return total + getGuestCountByGroup(groupName)
  }, 0)
})

const totalGuestCountForAttributeReset = computed(() => {
  return selectedGroupsForAttributeReset.value.reduce((total, groupName) => {
    return total + getGuestCountByGroup(groupName)
  }, 0)
})

const closeAssignModal = () => {
  selectedGroupsForAssign.value = []
  groupAssignTemplateIds.value = []
  emit('close-assign')
}

const closeRemoveModal = () => {
  selectedGroupsForRemove.value = []
  groupRemoveTemplateIds.value = []
  emit('close-remove')
}

const closeAttributeResetModal = () => {
  selectedGroupsForAttributeReset.value = []
  selectedAttributeKeysForReset.value = []
  emit('close-attribute-reset')
}

const executeGroupAssign = async () => {
  if (selectedGroupsForAssign.value.length === 0) {
    alert('그룹을 선택해주세요.')
    return
  }
  if (groupAssignTemplateIds.value.length === 0) {
    alert('배정할 일정을 선택해주세요.')
    return
  }

  const totalGuests = totalGuestCountForAssign.value
  if (
    !confirm(
      `선택된 ${selectedGroupsForAssign.value.length}개 그룹 (총 ${totalGuests}명)에게 일정을 배정하시겠습니까?`,
    )
  ) {
    return
  }

  groupAssigning.value = true

  try {
    let successCount = 0
    let failCount = 0

    const guestsInSelectedGroups = props.guests.filter((g) =>
      selectedGroupsForAssign.value.includes(g.groupName),
    )

    for (const guest of guestsInSelectedGroups) {
      try {
        const existingIds =
          guest.scheduleTemplates?.map((st) => st.scheduleTemplateId) || []
        const mergedIds = [
          ...new Set([...existingIds, ...groupAssignTemplateIds.value]),
        ]

        await apiClient.post(
          `/admin/conventions/${props.conventionId}/guests/${guest.id}/schedules`,
          {
            scheduleTemplateIds: mergedIds,
          },
        )
        successCount++
      } catch (error) {
        console.error(`Failed to assign schedules to guest ${guest.id}:`, error)
        failCount++
      }
    }

    closeAssignModal()
    emit('completed')
    alert(`그룹 일정 배정 완료!\n성공: ${successCount}명\n실패: ${failCount}명`)
  } catch (error) {
    console.error('Group assign failed:', error)
    alert(
      '그룹 일정 배정 실패: ' +
        (error.response?.data?.message || error.message),
    )
  } finally {
    groupAssigning.value = false
  }
}

const executeGroupRemove = async () => {
  if (selectedGroupsForRemove.value.length === 0) {
    alert('그룹을 선택해주세요.')
    return
  }
  if (groupRemoveTemplateIds.value.length === 0) {
    alert('해제할 일정을 선택해주세요.')
    return
  }

  const totalGuests = totalGuestCountForRemove.value
  if (
    !confirm(
      `선택된 ${selectedGroupsForRemove.value.length}개 그룹 (총 ${totalGuests}명)의 일정을 해제하시겠습니까?`,
    )
  ) {
    return
  }

  groupRemoving.value = true

  try {
    let successCount = 0
    let failCount = 0

    const guestsInSelectedGroups = props.guests.filter((g) =>
      selectedGroupsForRemove.value.includes(g.groupName),
    )

    for (const guest of guestsInSelectedGroups) {
      try {
        const existingIds =
          guest.scheduleTemplates?.map((st) => st.scheduleTemplateId) || []
        const remainingIds = existingIds.filter(
          (id) => !groupRemoveTemplateIds.value.includes(id),
        )

        await apiClient.post(
          `/admin/conventions/${props.conventionId}/guests/${guest.id}/schedules`,
          {
            scheduleTemplateIds: remainingIds,
          },
        )
        successCount++
      } catch (error) {
        console.error(
          `Failed to remove schedules from guest ${guest.id}:`,
          error,
        )
        failCount++
      }
    }

    closeRemoveModal()
    emit('completed')
    alert(`그룹 일정 해제 완료!\n성공: ${successCount}명\n실패: ${failCount}명`)
  } catch (error) {
    console.error('Group remove failed:', error)
    alert(
      '그룹 일정 해제 실패: ' +
        (error.response?.data?.message || error.message),
    )
  } finally {
    groupRemoving.value = false
  }
}

const executeGroupAttributeReset = async () => {
  if (selectedGroupsForAttributeReset.value.length === 0) {
    alert('그룹을 선택해주세요.')
    return
  }
  if (selectedAttributeKeysForReset.value.length === 0) {
    alert('삭제할 속성을 선택해주세요.')
    return
  }

  const totalGuests = totalGuestCountForAttributeReset.value
  if (
    !confirm(
      `선택된 ${selectedGroupsForAttributeReset.value.length}개 그룹 (총 ${totalGuests}명)의 속성을 삭제하시겠습니까?\n\n삭제할 속성: ${selectedAttributeKeysForReset.value.join(', ')}`,
    )
  ) {
    return
  }

  attributeResetting.value = true

  try {
    let successCount = 0
    let failCount = 0

    const guestsInSelectedGroups = props.guests.filter((g) =>
      selectedGroupsForAttributeReset.value.includes(g.groupName),
    )

    for (const guest of guestsInSelectedGroups) {
      try {
        for (const attrKey of selectedAttributeKeysForReset.value) {
          await apiClient.delete(
            `/admin/guests/${guest.id}/attributes/${encodeURIComponent(attrKey)}`,
          )
        }
        successCount++
      } catch (error) {
        console.error(
          `Failed to reset attributes for guest ${guest.id}:`,
          error,
        )
        failCount++
      }
    }

    closeAttributeResetModal()
    emit('completed')
    alert(`그룹 속성 삭제 완료!\n성공: ${successCount}명\n실패: ${failCount}명`)
  } catch (error) {
    console.error('Group attribute reset failed:', error)
    alert(
      '그룹 속성 삭제 실패: ' +
        (error.response?.data?.message || error.message),
    )
  } finally {
    attributeResetting.value = false
  }
}
</script>
