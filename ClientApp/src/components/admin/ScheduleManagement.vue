<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-xl font-semibold">일정 템플릿</h2>
      <button
        @click="showCreateModal = true"
        class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
      >
        + 템플릿 추가
      </button>
    </div>

    <!-- 템플릿 목록 -->
    <div class="space-y-4">
      <div
        v-for="template in templates"
        :key="template.id"
        class="bg-white rounded-lg shadow overflow-hidden"
      >
        <div class="p-4 sm:p-6 border-b bg-gray-50 flex items-center justify-between">
          <div class="flex-1">
            <h3 class="font-semibold text-lg">{{ template.courseName }}</h3>
            <p class="text-sm text-gray-600">{{ template.description }}</p>
          </div>
          <div class="flex gap-2">
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
              <div class="flex-shrink-0 w-16 text-sm font-medium text-gray-600">
                {{ item.startTime }}
              </div>
              <div class="flex-1 min-w-0">
                <p class="font-medium">{{ item.title }}</p>
                <p v-if="item.location" class="text-sm text-gray-500">📍 {{ item.location }}</p>
                <p v-if="item.content" class="text-sm text-gray-600 mt-1 whitespace-pre-wrap">{{ item.content }}</p>
              </div>
              <div class="flex gap-1">
                <button
                  @click="editScheduleItem(template, item)"
                  class="p-1.5 hover:bg-gray-200 rounded"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2-5a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"/>
                  </svg>
                </button>
                <button
                  @click="deleteScheduleItem(item.id)"
                  class="p-1.5 hover:bg-red-100 text-red-600 rounded"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                  </svg>
                </button>
              </div>
            </div>
            
            <button
              @click="addScheduleItem(template)"
              class="w-full py-2 text-sm text-primary-600 border-2 border-dashed border-primary-300 rounded-lg hover:bg-primary-50"
            >
              + 일정 항목 추가
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 템플릿 생성/수정 모달 -->
    <div v-if="showCreateModal || editingTemplate" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-lg w-full max-w-md">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            {{ editingTemplate ? '템플릿 수정' : '템플릿 추가' }}
          </h2>
          
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium mb-1">코스명 *</label>
              <input
                v-model="templateForm.courseName"
                type="text"
                class="w-full px-3 py-2 border rounded-lg"
                placeholder="예: 11월 3일"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-1">설명</label>
              <textarea
                v-model="templateForm.description"
                class="w-full px-3 py-2 border rounded-lg"
                rows="2"
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
    <div v-if="showItemModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
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
              <label class="block text-sm font-medium mb-1">제목 *</label>
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
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/services/api'

const props = defineProps({
  conventionId: { type: Number, required: true }
})

const templates = ref([])
const showCreateModal = ref(false)
const showItemModal = ref(false)
const editingTemplate = ref(null)
const editingItem = ref(null)
const currentTemplate = ref(null)

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

const loadTemplates = async () => {
  try {
    const response = await apiClient.get(`/admin/conventions/${props.conventionId}/schedule-templates`)
    templates.value = response.data
    console.log('✅ Templates loaded:', response.data)
  } catch (error) {
    console.error('❌ Failed to load templates:', error)
    console.error('Error details:', error.response?.data)
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

onMounted(() => {
  loadTemplates()
})
</script>
