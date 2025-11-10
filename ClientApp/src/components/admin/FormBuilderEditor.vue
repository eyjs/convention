<template>
  <div class="space-y-6">
    <!-- 헤더 -->
    <div class="flex justify-between items-center">
      <div>
        <button
          @click="$emit('cancel')"
          class="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-2"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          <span>목록으로 돌아가기</span>
        </button>
        <h2 class="text-2xl font-bold text-gray-900">
          {{ isEditing ? '폼 수정' : '새 폼 만들기' }}
        </h2>
        <p v-if="formDefinition" class="text-sm text-gray-600 mt-1">Form ID: {{ formDefinition.id }}</p>
      </div>
      <button
        @click="saveForm"
        :disabled="saving"
        class="px-6 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg transition-colors disabled:opacity-50"
      >
        {{ saving ? '저장 중...' : '저장' }}
      </button>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- 왼쪽: 필드 팔레트 -->
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
        <h3 class="font-bold text-gray-900 mb-4">필드 추가</h3>
        <div class="space-y-2">
          <button
            v-for="fieldType in fieldTypes"
            :key="fieldType.type"
            @click="addField(fieldType.type)"
            class="w-full flex items-center gap-3 px-4 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 hover:border-primary-500 transition-colors text-left"
          >
            <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="fieldType.icon" />
            </svg>
            <div>
              <div class="font-medium text-gray-900">{{ fieldType.label }}</div>
              <div class="text-xs text-gray-500">{{ fieldType.description }}</div>
            </div>
          </button>
        </div>
      </div>

      <!-- 중앙: 폼 프리뷰 -->
      <div class="lg:col-span-2 space-y-6">
        <!-- 폼 기본 정보 -->
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <h3 class="font-bold text-gray-900 mb-4">폼 기본 정보</h3>
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2">폼 이름 *</label>
              <input
                v-model="formDefinition.name"
                type="text"
                required
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>
            <div>
              <label class="block text-sm font-semibold text-gray-700 mb-2">설명</label>
              <textarea
                v-model="formDefinition.description"
                rows="2"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              ></textarea>
            </div>
          </div>
        </div>

        <!-- 필드 목록 -->
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <div class="flex justify-between items-center mb-4">
            <h3 class="font-bold text-gray-900">폼 필드 ({{ formDefinition.fields.length }}개)</h3>
            <button
              v-if="formDefinition.fields.length > 0"
              @click="clearAllFields"
              class="text-sm text-red-600 hover:text-red-700"
            >
              모두 삭제
            </button>
          </div>

          <!-- 필드가 없을 때 -->
          <div
            v-if="formDefinition.fields.length === 0"
            class="text-center py-12 border-2 border-dashed border-gray-300 rounded-lg"
          >
            <svg class="w-12 h-12 mx-auto text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            <p class="text-gray-600">왼쪽에서 필드를 추가해보세요</p>
          </div>

          <!-- 필드 목록 -->
          <draggable
            v-else
            v-model="formDefinition.fields"
            item-key="id"
            class="space-y-3"
            handle=".drag-handle"
          >
            <template #item="{ element, index }">
              <div class="border border-gray-300 rounded-lg p-4 bg-gray-50 hover:bg-gray-100 transition-colors">
                <div class="flex items-start gap-3">
                  <!-- 드래그 핸들 -->
                  <div class="drag-handle cursor-move flex-shrink-0 mt-1">
                    <svg class="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                      <path d="M7 2a2 2 0 1 0 .001 4.001A2 2 0 0 0 7 2zm0 6a2 2 0 1 0 .001 4.001A2 2 0 0 0 7 8zm0 6a2 2 0 1 0 .001 4.001A2 2 0 0 0 7 14zm6-8a2 2 0 1 0-.001-4.001A2 2 0 0 0 13 6zm0 2a2 2 0 1 0 .001 4.001A2 2 0 0 0 13 8zm0 6a2 2 0 1 0 .001 4.001A2 2 0 0 0 13 14z"></path>
                    </svg>
                  </div>

                  <!-- 필드 설정 -->
                  <div class="flex-1 space-y-3 min-w-0">
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                      <div>
                        <label class="block text-xs font-medium text-gray-700 mb-1">필드 이름 (key) *</label>
                        <input
                          v-model="element.key"
                          type="text"
                          required
                          placeholder="예: fullName"
                          class="w-full px-3 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-primary-500"
                        />
                      </div>
                      <div>
                        <label class="block text-xs font-medium text-gray-700 mb-1">라벨 (표시명) *</label>
                        <input
                          v-model="element.label"
                          type="text"
                          required
                          placeholder="예: 이름"
                          class="w-full px-3 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-primary-500"
                        />
                      </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                      <div>
                        <label class="block text-xs font-medium text-gray-700 mb-1">필드 타입</label>
                        <select
                          v-model="element.fieldType"
                          class="w-full px-3 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-primary-500"
                        >
                          <option v-for="type in fieldTypes" :key="type.type" :value="type.type">
                            {{ type.label }}
                          </option>
                        </select>
                      </div>
                      <div>
                        <label class="block text-xs font-medium text-gray-700 mb-1">플레이스홀더</label>
                        <input
                          v-model="element.placeholder"
                          type="text"
                          placeholder="예: 이름을 입력하세요"
                          class="w-full px-3 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-primary-500"
                        />
                      </div>
                    </div>

                    <!-- 옵션 (select, radio 타입) -->
                    <div v-if="element.fieldType === 'select' || element.fieldType === 'radio'">
                      <label class="block text-xs font-medium text-gray-700 mb-1">
                        옵션 (JSON 배열) *
                      </label>
                      <textarea
                        v-model="element.optionsJson"
                        rows="2"
                        placeholder='[{"value":"option1","label":"옵션 1"}]'
                        class="w-full px-3 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-primary-500 font-mono"
                      ></textarea>
                      <p class="text-xs text-gray-500 mt-1">
                        예시: [{"value":"yes","label":"예"},{"value":"no","label":"아니오"}]
                      </p>
                    </div>

                    <!-- 필수 여부 -->
                    <div class="flex items-center gap-2">
                      <input
                        v-model="element.isRequired"
                        type="checkbox"
                        :id="`required-${index}`"
                        class="w-4 h-4 text-primary-600 rounded"
                      />
                      <label :for="`required-${index}`" class="text-sm text-gray-700">
                        필수 입력 항목
                      </label>
                    </div>
                  </div>

                  <!-- 삭제 버튼 -->
                  <button
                    @click="removeField(index)"
                    class="flex-shrink-0 p-1 hover:bg-red-50 text-red-600 rounded transition-colors"
                    title="필드 삭제"
                  >
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              </div>
            </template>
          </draggable>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/services/api'
import draggable from 'vuedraggable'

const props = defineProps({
  formDefinitionId: {
    type: Number,
    default: null,
  },
  conventionId: {
    type: Number,
    required: true,
  },
})

const emit = defineEmits(['cancel', 'saved'])

const formDefinition = ref({
  id: null,
  name: '',
  description: '',
  conventionId: props.conventionId,
  fields: [],
})

const isEditing = ref(false)
const saving = ref(false)

const fieldTypes = [
  {
    type: 'text',
    label: '텍스트',
    description: '한 줄 입력',
    icon: 'M4 6h16M4 12h16M4 18h7',
  },
  {
    type: 'textarea',
    label: '긴 텍스트',
    description: '여러 줄 입력',
    icon: 'M4 6h16M4 10h16M4 14h16M4 18h16',
  },
  {
    type: 'email',
    label: '이메일',
    description: '이메일 주소',
    icon: 'M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z',
  },
  {
    type: 'tel',
    label: '전화번호',
    description: '전화번호',
    icon: 'M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z',
  },
  {
    type: 'number',
    label: '숫자',
    description: '숫자 입력',
    icon: 'M7 20l4-16m2 16l4-16M6 9h14M4 15h14',
  },
  {
    type: 'date',
    label: '날짜',
    description: '날짜 선택',
    icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
  },
  {
    type: 'select',
    label: '선택',
    description: '드롭다운',
    icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2',
  },
  {
    type: 'radio',
    label: '라디오',
    description: '단일 선택',
    icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
  },
  {
    type: 'checkbox',
    label: '체크박스',
    description: '확인/동의',
    icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
  },
  {
    type: 'file',
    label: '파일',
    description: '파일 업로드',
    icon: 'M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12',
  },
]

// 필드 추가
function addField(fieldType) {
  const newField = {
    id: Date.now(), // 임시 ID (저장 시 서버에서 재할당)
    key: '',
    label: '',
    fieldType: fieldType,
    orderIndex: formDefinition.value.fields.length,
    isRequired: false,
    placeholder: '',
    optionsJson: fieldType === 'select' || fieldType === 'radio' ? '[]' : null,
  }
  formDefinition.value.fields.push(newField)
}

// 필드 삭제
function removeField(index) {
  if (confirm('이 필드를 삭제하시겠습니까?')) {
    formDefinition.value.fields.splice(index, 1)
    // OrderIndex 재정렬
    formDefinition.value.fields.forEach((field, idx) => {
      field.orderIndex = idx
    })
  }
}

// 모든 필드 삭제
function clearAllFields() {
  if (confirm('모든 필드를 삭제하시겠습니까?')) {
    formDefinition.value.fields = []
  }
}

// 폼 저장
async function saveForm() {
  if (!formDefinition.value.name.trim()) {
    alert('폼 이름을 입력하세요.')
    return
  }

  if (formDefinition.value.fields.length === 0) {
    alert('최소 1개 이상의 필드를 추가하세요.')
    return
  }

  // 필드 검증
  for (let i = 0; i < formDefinition.value.fields.length; i++) {
    const field = formDefinition.value.fields[i]
    if (!field.key || !field.label) {
      alert(`${i + 1}번째 필드의 key와 label을 입력하세요.`)
      return
    }
  }

  // 저장용 데이터 준비 (순환 참조 방지 및 불필요한 필드 제거)
  const payload = {
    name: formDefinition.value.name,
    description: formDefinition.value.description || '',
    conventionId: props.conventionId,
    fields: formDefinition.value.fields.map((field, idx) => ({
      key: field.key,
      label: field.label,
      fieldType: field.fieldType,
      orderIndex: idx,
      isRequired: field.isRequired || false,
      placeholder: field.placeholder || '',
      optionsJson: field.optionsJson || null,
    })),
  }

  saving.value = true

  try {
    if (isEditing.value) {
      await apiClient.put(`/admin/conventions/${props.conventionId}/forms/${formDefinition.value.id}`, payload)
    } else {
      await apiClient.post(`/admin/conventions/${props.conventionId}/forms`, payload)
    }

    alert('폼이 저장되었습니다.')
    emit('saved')
  } catch (error) {
    console.error('폼 저장 실패:', error)
    if (error.response?.data) {
      console.error('서버 응답:', error.response.data)
    }
    alert('폼 저장에 실패했습니다.')
  } finally {
    saving.value = false
  }
}

// 폼 로드 (수정 모드)
async function loadForm() {
  if (!props.formDefinitionId) return

  try {
    const response = await apiClient.get(`/admin/conventions/${props.conventionId}/forms/${props.formDefinitionId}`)
    formDefinition.value = response.data
    isEditing.value = true
  } catch (error) {
    console.error('폼 로드 실패:', error)
    alert('폼을 불러오는데 실패했습니다.')
    emit('cancel')
  }
}

onMounted(() => {
  if (props.formDefinitionId) {
    loadForm()
  }
})
</script>
