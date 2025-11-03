<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-xl font-semibold">일정 관리</h2>
      <button
        @click="showCreateModal = true"
        class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
      >
        + 일정 코스 추가
      </button>
    </div>

    <!-- 템플릿 필터 버튼 -->
    <div class="mb-4 overflow-x-auto scrollbar-hide">
      <div class="flex gap-2 min-w-max pb-2">
        <button
          @click="selectedTemplateId = 'all'"
          :class="['flex-shrink-0 px-4 py-2 rounded-full text-sm font-medium transition-all', selectedTemplateId === 'all' ? 'bg-primary-600 text-white' : 'bg-white text-gray-700 hover:bg-gray-100 border']"
        >
          전체
        </button>
        <button
          v-for="template in templates"
          :key="template.id"
          @click="selectedTemplateId = template.id"
          :class="['flex-shrink-0 px-4 py-2 rounded-full text-sm font-medium transition-all', selectedTemplateId === template.id ? 'bg-primary-600 text-white' : 'bg-white text-gray-700 hover:bg-gray-100 border']"
        >
          {{ template.courseName }}
        </button>
      </div>
    </div>

    <!-- 템플릿 목록 -->
    <div class="space-y-4">
      <div
        v-for="template in filteredTemplates"
        :key="template.id"
        class="bg-white rounded-lg shadow overflow-hidden"
      >
        <div class="p-4 sm:p-6 border-b bg-gray-50 flex items-center justify-between">
          <div class="flex-1">
            <h3 class="font-semibold text-lg">{{ template.courseName }}</h3>
            <p class="text-sm text-gray-600">{{ template.description }}</p>
            <p class="text-xs text-gray-500 mt-1">
              할당된 참석자: {{ template.guestCount || 0 }}명 | 일정 항목: {{ template.scheduleItems?.length || 0 }}개
            </p>
          </div>
          <div class="flex gap-2">
            <button
              @click="viewAssignedGuests(template)"
              class="px-3 py-1.5 text-sm bg-blue-50 text-blue-600 rounded hover:bg-blue-100"
            >
              참석자 보기
            </button>
            <button
              @click="editTemplate(template)"
              class="px-3 py-1.5 text-sm bg-white border rounded hover:bg-gray-50"
            >
              수정
            </button>
            <button
              @click="deleteTemplate(template.id)"
              class="px-3 py-1.5 text-sm bg-red-50 text-red-600 rounded hover:bg-red-100"
            >
              삭제
            </button>
          </div>
        </div>

        <div class="p-4 sm:p-6">
          <div class="space-y-3">
            <div
              v-for="item in template.scheduleItems"
              :key="item.id"
              class="flex items-start gap-3 p-3 bg-gray-50 rounded-lg"
            >
              <div class="flex-shrink-0 w-28 text-sm">
                <div class="font-medium text-gray-600">{{ formatDate(item.scheduleDate) }}</div>
                <div class="text-primary-600 font-semibold mt-0.5">{{ item.startTime }}</div>
              </div>
              <div class="flex-1 min-w-0">
                <p class="font-medium text-gray-900">{{ item.title }}</p>
                <p v-if="item.location" class="text-sm text-gray-500 mt-1">📍 {{ item.location }}</p>
                <p v-if="item.content" class="text-sm text-gray-600 mt-1 whitespace-pre-wrap">{{ item.content }}</p>
              </div>
              <div class="flex gap-1">
                <button
                  @click="editScheduleItem(template, item)"
                  class="p-1.5 hover:bg-gray-200 rounded"
                  title="수정"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2-5a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"/>
                  </svg>
                </button>
                <button
                  @click="deleteScheduleItem(item.id)"
                  class="p-1.5 hover:bg-red-100 text-red-600 rounded"
                  title="삭제"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                  </svg>
                </button>
              </div>
            </div>
            
            <div class="flex gap-2">
              <button
                @click="addScheduleItem(template)"
                class="flex-1 py-2 text-sm text-primary-600 border-2 border-dashed border-primary-300 rounded-lg hover:bg-primary-50"
              >
                + 수기 등록
              </button>
              <button
                @click="showCopyItemsModal(template)"
                class="flex-1 py-2 text-sm text-green-600 border-2 border-dashed border-green-300 rounded-lg hover:bg-green-50 flex items-center justify-center gap-1"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/>
                </svg>
                기존 일정 복사
              </button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="templates.length === 0" class="text-center py-12 text-gray-500 bg-white rounded-lg shadow">
        등록된 일정 코스가 없습니다. 일정 코스를 추가해주세요.
      </div>
    </div>

    <!-- 템플릿 생성/수정 모달 -->
    <div v-if="showCreateModal || editingTemplate" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="isTouchDevice && closeTemplateModal()">
      <div class="bg-white rounded-lg w-full max-w-md">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            {{ editingTemplate ? '일정 코스 수정' : '일정 코스 추가' }}
          </h2>
          
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium mb-1">코스명 *</label>
              <input
                v-model="templateForm.courseName"
                type="text"
                class="w-full px-3 py-2 border rounded-lg"
                placeholder="예: A코스"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-1">설명</label>
              <textarea
                v-model="templateForm.description"
                class="w-full px-3 py-2 border rounded-lg"
                rows="2"
                placeholder="이 일정 코스에 대한 설명"
              ></textarea>
            </div>
          </div>

          <div class="flex justify-end space-x-3 mt-6">
            <button
              @click="closeTemplateModal"
              class="px-4 py-2 border rounded-lg hover:bg-gray-50"
            >
              취소
            </button>
            <button
              @click="saveTemplate"
              class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
            >
              저장
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 일정 항목 생성/수정 모달 -->
    <div v-if="showItemModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="isTouchDevice && closeItemModal()">
      <div class="bg-white rounded-lg w-full max-w-lg max-h-[90vh] overflow-y-auto">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            {{ editingItem ? '일정 수정' : '일정 추가' }}
          </h2>
          
          <div class="space-y-4">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-sm font-medium mb-1">날짜 *</label>
                <input
                  v-model="itemForm.scheduleDate"
                  type="date"
                  class="w-full px-3 py-2 border rounded-lg"
                />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">시작 시간 *</label>
                <input
                  v-model="itemForm.startTime"
                  type="time"
                  class="w-full px-3 py-2 border rounded-lg"
                />
              </div>
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-1">일정명 *</label>
              <input
                v-model="itemForm.title"
                type="text"
                class="w-full px-3 py-2 border rounded-lg"
                placeholder="예: 객실에 집결"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-1">장소</label>
              <input
                v-model="itemForm.location"
                type="text"
                class="w-full px-3 py-2 border rounded-lg"
                placeholder="예: 호텔 로비"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-1">상세 내용</label>
              <textarea
                v-model="itemForm.content"
                class="w-full px-3 py-2 border rounded-lg"
                rows="5"
                placeholder="일정에 대한 상세 설명을 입력하세요"
              ></textarea>
            </div>
          </div>

          <div class="flex justify-end space-x-3 mt-6">
            <button
              @click="closeItemModal"
              class="px-4 py-2 border rounded-lg hover:bg-gray-50"
            >
              취소
            </button>
            <button
              @click="saveScheduleItem"
              class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
            >
              저장
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 할당된 참석자 목록 모달 -->
    <div v-if="showGuestsModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="isTouchDevice && closeGuestsModal()">
      <div class="bg-white rounded-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div class="p-6">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-xl font-semibold">
              {{ selectedTemplate?.courseName }} - 할당된 참석자
            </h2>
            <button
              @click="closeGuestsModal"
              class="p-2 hover:bg-gray-100 rounded"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </button>
          </div>

          <div v-if="assignedGuests.length === 0" class="text-center py-8 text-gray-500">
            이 일정에 할당된 참석자가 없습니다.
          </div>

          <div v-else class="space-y-2">
            <div
              v-for="guest in assignedGuests"
              :key="guest.id"
              class="p-4 border rounded-lg hover:bg-gray-50"
            >
              <div class="flex justify-between items-start">
                <div>
                  <p class="font-medium">{{ guest.guestName }}</p>
                  <p class="text-sm text-gray-600">{{ guest.telephone }}</p>
                  <p v-if="guest.corpPart" class="text-sm text-gray-500">{{ guest.corpPart }}</p>
                </div>
                <button
                  @click="removeGuestFromSchedule(guest.id)"
                  class="px-3 py-1 text-sm text-red-600 hover:bg-red-50 rounded"
                >
                  제거
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 기존 일정 복사 모달 -->
    <div v-if="showCopyModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="isTouchDevice && closeCopyModal()">
      <div class="bg-white rounded-lg w-full max-w-4xl max-h-[90vh] overflow-y-auto">
        <div class="p-6">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-xl font-semibold">
              {{ targetTemplate?.courseName }} - 기존 일정 복사
            </h2>
            <button
              @click="closeCopyModal"
              class="p-2 hover:bg-gray-100 rounded"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </button>
          </div>

          <div class="mb-4 p-4 bg-blue-50 rounded-lg">
            <p class="text-sm text-blue-900">다른 템플릿의 일정을 선택하면 현재 템플릿에 복사됩니다.</p>
            <p class="text-xs text-blue-700 mt-1">복사 후 개별적으로 수정할 수 있습니다.</p>
          </div>

          <div class="space-y-4">
            <div
              v-for="template in otherTemplates"
              :key="template.id"
              class="border rounded-lg overflow-hidden"
            >
              <div class="p-4 bg-gray-50 border-b">
                <div class="flex justify-between items-center">
                  <div>
                    <h3 class="font-semibold">{{ template.courseName }}</h3>
                    <p class="text-sm text-gray-600">{{ template.description }}</p>
                    <p class="text-xs text-gray-500 mt-1">일정 {{ template.scheduleItems.length }}개</p>
                  </div>
                  <button
                    @click="copyAllItemsFromTemplate(template)"
                    class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
                  >
                    전체 복사
                  </button>
                </div>
              </div>

              <div class="p-4">
                <div class="space-y-2">
                  <div
                    v-for="item in template.scheduleItems"
                    :key="item.id"
                    class="flex items-start gap-3 p-3 bg-white border rounded-lg hover:bg-gray-50"
                  >
                    <div class="flex-shrink-0">
                      <input
                        type="checkbox"
                        :value="item.id"
                        v-model="selectedItemsToCopy"
                        class="rounded mt-1"
                      />
                    </div>
                    <div class="flex-shrink-0 w-24 text-sm">
                      <div class="font-medium text-gray-600">{{ formatDate(item.scheduleDate) }}</div>
                      <div class="text-primary-600">{{ item.startTime }}</div>
                    </div>
                    <div class="flex-1">
                      <p class="font-medium">{{ item.title }}</p>
                      <p v-if="item.location" class="text-sm text-gray-500">📍 {{ item.location }}</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-if="selectedItemsToCopy.length > 0" class="sticky bottom-0 mt-4 p-4 bg-white border-t">
            <div class="flex justify-between items-center">
              <span class="text-sm font-medium">선택: {{ selectedItemsToCopy.length }}개 일정</span>
              <div class="flex gap-2">
                <button
                  @click="selectedItemsToCopy = []"
                  class="px-4 py-2 border rounded-lg hover:bg-gray-50"
                >
                  선택 취소
                </button>
                <button
                  @click="copySelectedItems"
                  class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
                >
                  선택한 일정 복사
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import apiClient from '@/services/api'
import { useDevice } from '@/composables/useDevice'

const props = defineProps({
  conventionId: { type: Number, required: true }
})

const { isTouchDevice } = useDevice()

const selectedTemplateId = ref('all')

const filteredTemplates = computed(() => {
  if (selectedTemplateId.value === 'all') {
    return templates.value
  }
  return templates.value.filter(t => t.id === selectedTemplateId.value)
})

const templates = ref([])
const showCreateModal = ref(false)
const showItemModal = ref(false)
const showGuestsModal = ref(false)
const showCopyModal = ref(false)
const editingTemplate = ref(null)
const editingItem = ref(null)
const currentTemplate = ref(null)
const selectedTemplate = ref(null)
const targetTemplate = ref(null)
const assignedGuests = ref([])
const otherTemplates = ref([])
const selectedItemsToCopy = ref([])

const templateForm = ref({
  courseName: '',
  description: ''
})

const itemForm = ref({
  scheduleDate: '',
  startTime: '',
  title: '',
  location: '',
  content: ''
})

const formatDate = (dateStr) => {
  const date = new Date(dateStr)
  return date.toLocaleDateString('ko-KR', {
    month: 'numeric',
    day: 'numeric'
  }) + '일'
}

const loadTemplates = async () => {
  try {
    const response = await apiClient.get(`/admin/conventions/${props.conventionId}/schedule-templates`)
    templates.value = response.data
    console.log('✅ Templates loaded:', response.data)
  } catch (error) {
    console.error('❌ Failed to load templates:', error)
  }
}

const editTemplate = (template) => {
  editingTemplate.value = template
  templateForm.value = {
    courseName: template.courseName,
    description: template.description
  }
}

const closeTemplateModal = () => {
  showCreateModal.value = false
  editingTemplate.value = null
  templateForm.value = { courseName: '', description: '' }
}

const saveTemplate = async () => {
  try {
    if (!templateForm.value.courseName.trim()) {
      alert('코스명을 입력해주세요.')
      return
    }

    if (editingTemplate.value) {
      await apiClient.put(`/admin/schedule-templates/${editingTemplate.value.id}`, templateForm.value)
    } else {
      await apiClient.post(`/admin/conventions/${props.conventionId}/schedule-templates`, templateForm.value)
    }
    await loadTemplates()
    closeTemplateModal()
  } catch (error) {
    console.error('Failed to save template:', error)
    alert('템플릿 저장 실패: ' + (error.response?.data?.message || error.message))
  }
}

const deleteTemplate = async (id) => {
  if (!confirm('템플릿을 삭제하면 모든 일정 항목도 함께 삭제됩니다. 계속하시겠습니까?')) return

  try {
    await apiClient.delete(`/admin/schedule-templates/${id}`)
    await loadTemplates()
  } catch (error) {
    console.error('Failed to delete template:', error)
    alert('템플릿 삭제 실패: ' + (error.response?.data?.message || error.message))
  }
}

const addScheduleItem = (template) => {
  currentTemplate.value = template
  editingItem.value = null
  itemForm.value = {
    scheduleDate: '',
    startTime: '',
    title: '',
    location: '',
    content: ''
  }
  showItemModal.value = true
}

const editScheduleItem = (template, item) => {
  currentTemplate.value = template
  editingItem.value = item
  itemForm.value = {
    scheduleDate: item.scheduleDate?.split('T')[0] || '',
    startTime: item.startTime,
    title: item.title,
    location: item.location || '',
    content: item.content || ''
  }
  showItemModal.value = true
}

const closeItemModal = () => {
  showItemModal.value = false
  editingItem.value = null
  currentTemplate.value = null
}

const saveScheduleItem = async () => {
  try {
    if (!itemForm.value.scheduleDate || !itemForm.value.startTime || !itemForm.value.title) {
      alert('날짜, 시간, 일정명은 필수입니다.')
      return
    }

    const data = {
      ...itemForm.value,
      scheduleTemplateId: currentTemplate.value.id
    }

    if (editingItem.value) {
      await apiClient.put(`/admin/schedule-items/${editingItem.value.id}`, data)
    } else {
      await apiClient.post('/admin/schedule-items', data)
    }
    await loadTemplates()
    closeItemModal()
  } catch (error) {
    console.error('Failed to save schedule item:', error)
    alert('일정 항목 저장 실패: ' + (error.response?.data?.message || error.message))
  }
}

const deleteScheduleItem = async (id) => {
  if (!confirm('이 일정을 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/admin/schedule-items/${id}`)
    await loadTemplates()
  } catch (error) {
    console.error('Failed to delete schedule item:', error)
    alert('일정 항목 삭제 실패: ' + (error.response?.data?.message || error.message))
  }
}

const viewAssignedGuests = async (template) => {
  selectedTemplate.value = template
  try {
    const response = await apiClient.get(`/admin/schedule-templates/${template.id}/guests`)
    assignedGuests.value = response.data
    showGuestsModal.value = true
  } catch (error) {
    console.error('Failed to load assigned guests:', error)
    alert('참석자 목록 로드 실패: ' + (error.response?.data?.message || error.message))
  }
}

const closeGuestsModal = () => {
  showGuestsModal.value = false
  selectedTemplate.value = null
  assignedGuests.value = []
}

const removeGuestFromSchedule = async (guestId) => {
  if (!confirm('이 참석자를 일정에서 제거하시겠습니까?')) return

  try {
    await apiClient.delete(`/admin/guests/${guestId}/schedules/${selectedTemplate.value.id}`)
    await viewAssignedGuests(selectedTemplate.value)
    await loadTemplates()
  } catch (error) {
    console.error('Failed to remove guest from schedule:', error)
    alert('참석자 제거 실패: ' + (error.response?.data?.message || error.message))
  }
}

const showCopyItemsModal = (template) => {
  targetTemplate.value = template
  otherTemplates.value = templates.value.filter(t => t.id !== template.id)
  selectedItemsToCopy.value = []
  showCopyModal.value = true
}

const closeCopyModal = () => {
  showCopyModal.value = false
  targetTemplate.value = null
  otherTemplates.value = []
  selectedItemsToCopy.value = []
}

const copyAllItemsFromTemplate = async (sourceTemplate) => {
  if (!confirm(`${sourceTemplate.courseName}의 모든 일정(${sourceTemplate.scheduleItems.length}개)을 복사하시겠습니까?`)) return

  try {
    const itemsToCopy = sourceTemplate.scheduleItems.map(item => ({
      scheduleDate: item.scheduleDate,
      startTime: item.startTime,
      title: item.title,
      location: item.location,
      content: item.content,
      scheduleTemplateId: targetTemplate.value.id
    }))

    await apiClient.post('/admin/schedule-items/bulk', { items: itemsToCopy })
    await loadTemplates()
    closeCopyModal()
    alert('일정 복사 완료')
  } catch (error) {
    console.error('Failed to copy items:', error)
    alert('일정 복사 실패: ' + (error.response?.data?.message || error.message))
  }
}

const copySelectedItems = async () => {
  if (selectedItemsToCopy.value.length === 0) {
    alert('복사할 일정을 선택해주세요.')
    return
  }

  try {
    const itemsToCopy = []
    otherTemplates.value.forEach(template => {
      template.scheduleItems.forEach(item => {
        if (selectedItemsToCopy.value.includes(item.id)) {
          itemsToCopy.push({
            scheduleDate: item.scheduleDate,
            startTime: item.startTime,
            title: item.title,
            location: item.location,
            content: item.content,
            scheduleTemplateId: targetTemplate.value.id
          })
        }
      })
    })

    await apiClient.post('/admin/schedule-items/bulk', { items: itemsToCopy })
    await loadTemplates()
    closeCopyModal()
    alert(`${itemsToCopy.length}개 일정 복사 완료`)
  } catch (error) {
    console.error('Failed to copy items:', error)
    alert('일정 복사 실패: ' + (error.response?.data?.message || error.message))
  }
}

onMounted(() => {
  loadTemplates()
})
</script>
