<template>
  <div class="flex flex-col h-screen">
    <!-- Header -->
    <header class="bg-white shadow-sm z-10">
      <div class="mx-auto py-4 px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-lg font-semibold leading-6 text-gray-900">
              {{ isNewForm ? '새 폼 만들기' : '폼 편집' }}
            </h1>
            <p class="mt-1 text-sm text-gray-500">
              {{ form.name || '폼의 세부 정보를 설정하세요.' }}
            </p>
          </div>
          <div class="flex items-center gap-4">
            <router-link
              to="/admin/form-builder"
              class="text-sm font-medium text-gray-600 hover:text-gray-900"
            >
              목록으로
            </router-link>
            <button
              @click="saveForm"
              :disabled="isSaving"
              class="inline-flex items-center justify-center rounded-md border border-transparent bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-primary-700 disabled:opacity-50"
            >
              {{ isSaving ? '저장 중...' : '저장' }}
            </button>
          </div>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="flex-1 flex overflow-hidden">
      <!-- Palette -->
      <aside
        class="w-64 bg-gray-50 border-r border-gray-200 p-4 overflow-y-auto"
      >
        <h2 class="text-sm font-medium text-gray-500 mb-4">필드 추가</h2>
        <div class="space-y-2">
          <button
            v-for="fieldType in fieldPalette"
            :key="fieldType.type"
            @click="addField(fieldType.type)"
            class="w-full flex items-center p-2 text-sm text-left text-gray-700 rounded-md hover:bg-gray-200"
          >
            {{ fieldType.label }}
          </button>
        </div>
      </aside>

      <!-- Form Preview -->
      <div class="flex-1 overflow-y-auto p-8">
        <div class="max-w-2xl mx-auto">
          <div class="space-y-4 mb-8">
            <div>
              <label class="block text-sm font-medium text-gray-700"
                >폼 이름</label
              >
              <input
                type="text"
                v-model="form.name"
                class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
                placeholder="예: 여권 정보 입력"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700"
                >폼 설명</label
              >
              <textarea
                v-model="form.description"
                rows="2"
                class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
                placeholder="사용자에게 표시될 폼에 대한 간단한 설명"
              ></textarea>
            </div>
          </div>

          <div v-if="loading" class="text-center py-10">
            <div
              class="w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin mx-auto"
            ></div>
          </div>
          <div
            v-else-if="fields.length === 0"
            class="text-center py-10 border-2 border-dashed border-gray-300 rounded-lg"
          >
            <p class="text-gray-500">필드가 없습니다.</p>
            <p class="mt-2 text-sm text-gray-500">
              왼쪽 팔레트에서 필드를 추가하세요.
            </p>
          </div>
          <div v-else class="space-y-4">
            <div
              v-for="(field, index) in fields"
              :key="field.tempId || field.id"
              @click="selectField(field)"
              :class="[
                'p-4 border rounded-lg cursor-pointer transition-colors',
                selectedField === field
                  ? 'border-primary-500 bg-primary-50'
                  : 'border-gray-300 hover:border-gray-400',
              ]"
            >
              <div class="flex justify-between items-start">
                <div>
                  <label class="block text-sm font-medium text-gray-900">
                    {{ field.label || '새 필드' }}
                    <span v-if="field.isRequired" class="text-red-500">*</span>
                  </label>
                  <p class="text-xs text-gray-500 mt-1">
                    Key: {{ field.fieldKey }} | Type: {{ field.fieldType }}
                  </p>
                </div>
                <div class="flex items-center gap-2">
                  <button
                    @click.stop="moveField(index, -1)"
                    :disabled="index === 0"
                    class="p-1 text-gray-400 hover:text-gray-700 disabled:opacity-50"
                  >
                    <svg
                      class="w-5 h-5"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M5 15l7-7 7 7"
                      />
                    </svg>
                  </button>
                  <button
                    @click.stop="moveField(index, 1)"
                    :disabled="index === fields.length - 1"
                    class="p-1 text-gray-400 hover:text-gray-700 disabled:opacity-50"
                  >
                    <svg
                      class="w-5 h-5"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M19 9l-7 7-7-7"
                      />
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Properties Editor -->
      <aside class="w-80 bg-white border-l border-gray-200 overflow-y-auto">
        <div v-if="selectedField" class="p-6">
          <h2 class="text-lg font-medium text-gray-900 mb-4">필드 속성</h2>
          <div class="space-y-6">
            <div>
              <label class="block text-sm font-medium text-gray-700"
                >필드 타입</label
              >
              <p
                class="mt-1 text-sm text-gray-900 font-mono p-2 bg-gray-100 rounded"
              >
                {{ selectedField.fieldType }}
              </p>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700"
                >필드 레이블</label
              >
              <input
                type="text"
                v-model="selectedField.label"
                class="mt-1 block w-full rounded-md border-gray-300 shadow-sm sm:text-sm"
              />
              <p class="mt-1 text-xs text-gray-500">
                사용자에게 보여질 필드의 이름입니다.
              </p>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700"
                >필드 키 (FieldKey)</label
              >
              <input
                type="text"
                v-model="selectedField.fieldKey"
                @input="sanitizeFieldKey"
                class="mt-1 block w-full rounded-md border-gray-300 shadow-sm sm:text-sm font-mono"
              />
              <p class="mt-1 text-xs text-gray-500">
                데이터 저장에 사용될 고유한 키입니다. (영문, 숫자, _ 만 가능)
              </p>
            </div>
            <div class="flex items-start">
              <div class="flex items-center h-5">
                <input
                  type="checkbox"
                  v-model="selectedField.isRequired"
                  class="h-4 w-4 text-primary-600 border-gray-300 rounded"
                />
              </div>
              <div class="ml-3 text-sm">
                <label class="font-medium text-gray-700">필수 항목</label>
                <p class="text-gray-500">
                  사용자가 반드시 입력해야 하는 필드입니다.
                </p>
              </div>
            </div>
            <div class="pt-6 border-t border-gray-200">
              <button
                @click="deleteSelectedField"
                type="button"
                class="w-full text-sm text-red-600 hover:text-red-900"
              >
                필드 삭제
              </button>
            </div>
          </div>
        </div>
        <div v-else class="p-6 text-center text-gray-500">
          <p>편집할 필드를 선택하세요.</p>
        </div>
      </aside>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import formBuilderService from '@/services/formBuilderService'

const route = useRoute()
const router = useRouter()

const form = ref({ id: null, name: '', description: '' })
const fields = ref([])
const selectedField = ref(null)
const loading = ref(false)
const isSaving = ref(false)

const formId = computed(() => route.params.id)
const isNewForm = computed(() => formId.value === 'new')

const fieldPalette = [
  { type: 'text', label: '텍스트' },
  { type: 'textarea', label: '여러 줄 텍스트' },
  { type: 'date', label: '날짜' },
  { type: 'number', label: '숫자' },
  { type: 'file', label: '파일' },
]

let tempIdCounter = 0

async function loadForm() {
  if (isNewForm.value) {
    form.value = { id: null, name: '', description: '' }
    fields.value = []
    return
  }

  loading.value = true
  try {
    const response = await formBuilderService.getFormDefinition(formId.value)
    form.value = {
      id: response.data.id,
      name: response.data.name,
      description: response.data.description,
    }
    fields.value = response.data.fields.sort((a, b) => a.order - b.order)
  } catch (error) {
    console.error('Failed to load form definition:', error)
    alert('폼 정보를 불러오는 데 실패했습니다.')
    router.push('/admin/form-builder')
  } finally {
    loading.value = false
  }
}

function addField(fieldType) {
  const newField = {
    tempId: `temp-${tempIdCounter++}`,
    label: '새 필드',
    fieldKey: `field_${tempIdCounter}`,
    fieldType: fieldType,
    isRequired: false,
    order: fields.value.length + 1,
  }
  fields.value.push(newField)
  selectField(newField)
}

function selectField(field) {
  selectedField.value = field
}

function deleteSelectedField() {
  if (!selectedField.value) return
  if (confirm(`'${selectedField.value.label}' 필드를 삭제하시겠습니까?`)) {
    const index = fields.value.findIndex(
      (f) =>
        (f.tempId && f.tempId === selectedField.value.tempId) ||
        (f.id && f.id === selectedField.value.id),
    )
    if (index > -1) {
      fields.value.splice(index, 1)
      selectedField.value = null
    }
  }
}

function moveField(index, direction) {
  if (
    (direction === -1 && index === 0) ||
    (direction === 1 && index === fields.value.length - 1)
  ) {
    return
  }
  const newIndex = index + direction
  const element = fields.value.splice(index, 1)[0]
  fields.value.splice(newIndex, 0, element)
}

function sanitizeFieldKey() {
  if (selectedField.value) {
    selectedField.value.fieldKey = selectedField.value.fieldKey
      .replace(/[^a-zA-Z0-9_]/g, '')
      .toLowerCase()
  }
}

async function saveForm() {
  isSaving.value = true

  // Update order before saving
  fields.value.forEach((field, index) => {
    field.order = index + 1
  })

  const payload = {
    name: form.value.name,
    description: form.value.description,
    fields: fields.value,
  }

  try {
    if (isNewForm.value) {
      const response = await formBuilderService.createFormDefinition(payload)
      alert('폼이 성공적으로 생성되었습니다.')
      router.push(`/admin/form-builder/${response.data.id}`)
    } else {
      await formBuilderService.updateFormDefinition(formId.value, payload)
      alert('폼이 성공적으로 업데이트되었습니다.')
      await loadForm() // Reload to get updated data (like new field IDs)
    }
  } catch (error) {
    console.error('Failed to save form:', error)
    alert(
      '폼 저장에 실패했습니다: ' +
        (error.response?.data?.message || error.message),
    )
  } finally {
    isSaving.value = false
  }
}

onMounted(() => {
  loadForm()
})
</script>
