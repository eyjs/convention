<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-xl font-semibold">속성 템플릿 관리</h2>
      <button
        @click="showCreateModal = true"
        class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
      >
        + 속성 템플릿 추가
      </button>
    </div>

    <div class="bg-white rounded-lg shadow">
      <div v-if="templates.length === 0" class="text-center py-12 text-gray-500">
        등록된 속성 템플릿이 없습니다.
      </div>
      <div v-else class="divide-y">
        <div
          v-for="template in templates"
          :key="template.id"
          class="p-4 hover:bg-gray-50 flex items-center justify-between"
        >
          <div class="flex-1">
            <h3 class="font-semibold text-lg">{{ template.attributeKey }}</h3>
            <div v-if="template.attributeValues" class="mt-2 flex flex-wrap gap-2">
              <span
                v-for="(value, idx) in parseValues(template.attributeValues)"
                :key="idx"
                class="px-2 py-1 bg-gray-100 text-sm rounded"
              >
                {{ value }}
              </span>
            </div>
            <p v-else class="text-sm text-gray-500 mt-1">선택 값 없음 (수기 입력)</p>
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
      </div>
    </div>

    <!-- 속성 템플릿 생성/수정 모달 -->
    <div v-if="showCreateModal || editingTemplate" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="closeModal">
      <div class="bg-white rounded-lg w-full max-w-lg">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            {{ editingTemplate ? '속성 템플릿 수정' : '속성 템플릿 추가' }}
          </h2>
          
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium mb-1">속성명 *</label>
              <input
                v-model="form.attributeKey"
                type="text"
                class="w-full px-3 py-2 border rounded-lg"
                placeholder="예: 티셔츠 사이즈, 호차, 방 번호"
              />
            </div>
            
            <div>
              <label class="block text-sm font-medium mb-2">선택 값 (쉼표로 구분)</label>
              <textarea
                v-model="form.attributeValuesText"
                class="w-full px-3 py-2 border rounded-lg"
                rows="4"
                placeholder="예: 95, 100, 105, 110, 115&#10;비워두면 참석자 등록 시 수기 입력"
              ></textarea>
              <p class="text-xs text-gray-500 mt-1">
                쉼표(,)로 구분하여 입력하세요. 빈 값으로 두면 참석자가 직접 입력합니다.
              </p>
            </div>
          </div>

          <div class="flex justify-end space-x-3 mt-6">
            <button
              @click="closeModal"
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

    <!-- 토스트 알림 -->
    <div v-if="toast.show" :class="['fixed top-4 right-4 px-6 py-3 rounded-lg shadow-lg z-50 transition-all', toast.type === 'success' ? 'bg-green-600' : 'bg-red-600']">
      <p class="text-white font-medium">{{ toast.message }}</p>
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
const editingTemplate = ref(null)

const form = ref({
  attributeKey: '',
  attributeValuesText: ''
})

const toast = ref({
  show: false,
  message: '',
  type: 'success'
})

const parseValues = (valuesStr) => {
  if (!valuesStr) return []
  try {
    return JSON.parse(valuesStr)
  } catch {
    return []
  }
}

const loadTemplates = async () => {
  try {
    const response = await apiClient.get(`/attributetemplate/conventions/${props.conventionId}`)
    templates.value = response.data
  } catch (error) {
    console.error('Failed to load attribute templates:', error)
  }
}

const editTemplate = (template) => {
  editingTemplate.value = template
  form.value = {
    attributeKey: template.attributeKey,
    attributeValuesText: template.attributeValues 
      ? parseValues(template.attributeValues).join(', ')
      : ''
  }
}

const closeModal = () => {
  showCreateModal.value = false
  editingTemplate.value = null
  form.value = {
    attributeKey: '',
    attributeValuesText: ''
  }
}

const saveTemplate = async () => {
  try {
    if (!form.value.attributeKey.trim()) {
      alert('속성명을 입력해주세요.')
      return
    }

    // 쉼표로 구분된 값을 JSON 배열로 변환
    let attributeValues = null
    if (form.value.attributeValuesText.trim()) {
      const values = form.value.attributeValuesText
        .split(',')
        .map(v => v.trim())
        .filter(v => v)
      attributeValues = JSON.stringify(values)
    }

    const data = {
      attributeKey: form.value.attributeKey.trim(),
      attributeValues: attributeValues,
      orderNum: templates.value.length
    }

    if (editingTemplate.value) {
      await apiClient.put(`/attributetemplate/${editingTemplate.value.id}`, data)
    } else {
      await apiClient.post(`/attributetemplate/conventions/${props.conventionId}`, data)
    }

    await loadTemplates()
    closeModal()
    showToast('저장되었습니다', 'success')
  } catch (error) {
    console.error('Failed to save template:', error)
    showToast('저장 실패: ' + (error.response?.data?.message || error.message), 'error')
  }
}

const deleteTemplate = async (id) => {
  if (!confirm('이 속성 템플릿을 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/attributetemplate/${id}`)
    await loadTemplates()
    showToast('삭제되었습니다', 'success')
  } catch (error) {
    console.error('Failed to delete template:', error)
    showToast('삭제 실패', 'error')
  }
}

const showToast = (message, type = 'success') => {
  toast.value = { show: true, message, type }
  setTimeout(() => {
    toast.value.show = false
  }, 3000)
}

onMounted(() => {
  loadTemplates()
})
</script>
