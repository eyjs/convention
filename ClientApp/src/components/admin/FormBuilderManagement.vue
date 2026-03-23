<template>
  <div>
    <!-- 목록 뷰 -->
    <div v-if="currentView === 'list'">
      <AdminPageHeader
        title="폼 빌더 관리"
        description="드래그 앤 드롭으로 폼을 만들고 액션에 연결하세요"
      >
        <AdminButton :icon="Plus" @click="openCreateModal"
          >새 폼 만들기</AdminButton
        >
      </AdminPageHeader>

      <!-- 로딩 -->
      <div v-if="loading" class="text-center py-12">
        <div
          class="inline-block w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"
        ></div>
      </div>

      <!-- 폼 목록 -->
      <div v-else-if="forms.length > 0" class="grid gap-4">
        <div
          v-for="form in forms"
          :key="form.id"
          class="bg-white rounded-lg shadow-sm border border-gray-200 p-5 hover:shadow-md transition-shadow"
        >
          <div
            class="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-4"
          >
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 mb-2">
                <h3 class="text-lg font-bold text-gray-900">{{ form.name }}</h3>
                <span
                  class="px-2 py-1 bg-primary-100 text-primary-700 text-xs font-medium rounded"
                >
                  ID: {{ form.id }}
                </span>
              </div>
              <p v-if="form.description" class="text-sm text-gray-600 mb-3">
                {{ form.description }}
              </p>
              <div class="flex items-center gap-4 text-sm text-gray-500">
                <span>필드: {{ form.fields?.length || 0 }}개</span>
                <span>생성일: {{ formatDate(form.createdAt) }}</span>
              </div>
            </div>

            <!-- 액션 버튼 -->
            <div class="flex items-center gap-2 flex-shrink-0">
              <button
                class="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                title="Form ID 복사"
                @click="copyFormId(form.id)"
              >
                <Copy class="w-5 h-5 text-gray-600" />
              </button>
              <button
                class="p-2 hover:bg-primary-50 text-primary-600 rounded-lg transition-colors"
                title="수정"
                @click="showEditView(form.id)"
              >
                <Pencil class="w-5 h-5" />
              </button>
              <button
                class="p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors"
                title="삭제"
                @click="deleteForm(form)"
              >
                <Trash2 class="w-5 h-5" />
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- 빈 상태 -->
      <AdminEmptyState
        v-else
        :icon="FormInput"
        title="등록된 폼이 없습니다"
        description="새로운 폼을 만들어보세요"
      >
        <AdminButton :icon="Plus" @click="openCreateModal"
          >첫 폼 만들기</AdminButton
        >
      </AdminEmptyState>
    </div>

    <!-- 수정/생성 뷰 -->
    <div v-else-if="currentView === 'edit'">
      <FormBuilderEditor
        :form-definition-id="selectedFormId"
        :convention-id="conventionId"
        @cancel="goBackToList"
        @saved="handleFormSaved"
      />
    </div>

    <!-- 생성 모달 -->
    <BaseModal
      :is-open="isCreateModalVisible"
      max-width="sm"
      @close="isCreateModalVisible = false"
    >
      <template #header>새 폼 만들기</template>
      <template #body>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >폼 이름 *</label
            >
            <input
              v-model="newFormName"
              type="text"
              required
              placeholder="예: 여행 정보 제출 폼"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
          </div>
          <div>
            <label class="block text-sm font-semibold text-gray-700 mb-2"
              >설명</label
            >
            <textarea
              v-model="newFormDescription"
              rows="3"
              placeholder="이 폼에 대한 간단한 설명"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            ></textarea>
          </div>
        </div>
      </template>
      <template #footer>
        <button
          type="button"
          class="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
          @click="isCreateModalVisible = false"
        >
          취소
        </button>
        <button
          :disabled="!newFormName.trim()"
          class="px-4 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          @click="createForm"
        >
          생성
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { Copy, Pencil, Trash2, Plus, FormInput } from 'lucide-vue-next'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'
import FormBuilderEditor from '@/components/admin/FormBuilderEditor.vue'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminButton from '@/components/admin/ui/AdminButton.vue'
import AdminEmptyState from '@/components/admin/ui/AdminEmptyState.vue'

const props = defineProps({
  conventionId: {
    type: Number,
    required: true,
  },
})

const forms = ref([])
const loading = ref(false)
const currentView = ref('list') // 'list', 'edit'
const selectedFormId = ref(null)
const isCreateModalVisible = ref(false)
const newFormName = ref('')
const newFormDescription = ref('')

// 폼 목록 조회
async function fetchForms() {
  loading.value = true
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/forms`,
    )
    forms.value = response.data
  } catch (error) {
    console.error('폼 목록 조회 실패:', error)
    alert('폼 목록을 불러오는데 실패했습니다.')
  } finally {
    loading.value = false
  }
}

// 폼 생성
async function createForm() {
  if (!newFormName.value.trim()) return

  try {
    const response = await apiClient.post(
      `/admin/conventions/${props.conventionId}/forms`,
      {
        name: newFormName.value,
        description: newFormDescription.value,
        conventionId: props.conventionId,
      },
    )

    isCreateModalVisible.value = false
    newFormName.value = ''
    newFormDescription.value = ''

    // 생성 후 바로 수정 화면으로
    showEditView(response.data.id)
  } catch (error) {
    console.error('폼 생성 실패:', error)
    alert('폼 생성에 실패했습니다.')
  }
}

// 폼 삭제
async function deleteForm(form) {
  if (!confirm(`"${form.name}" 폼을 삭제하시겠습니까?`)) return

  try {
    await apiClient.delete(
      `/admin/conventions/${props.conventionId}/forms/${form.id}`,
    )
    await fetchForms()
    alert('폼이 삭제되었습니다.')
  } catch (error) {
    console.error('폼 삭제 실패:', error)
    alert('폼 삭제에 실패했습니다.')
  }
}

// Form ID 복사
async function copyFormId(id) {
  try {
    await navigator.clipboard.writeText(id.toString())
    alert(
      `폼 ID(${id})가 클립보드에 복사되었습니다!\n액션 관리에서 FormBuilder 타입 생성 시 사용하세요.`,
    )
  } catch (error) {
    console.error('복사 실패:', error)
    alert(`폼 ID: ${id}`)
  }
}

// 날짜 포맷팅
function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('ko-KR')
}

// 뷰 전환
function showEditView(id) {
  selectedFormId.value = id
  currentView.value = 'edit'
}

function goBackToList() {
  currentView.value = 'list'
  selectedFormId.value = null
  fetchForms()
}

function openCreateModal() {
  newFormName.value = ''
  newFormDescription.value = ''
  isCreateModalVisible.value = true
}

function handleFormSaved() {
  goBackToList()
}

onMounted(() => {
  fetchForms()
})

watch(
  () => props.conventionId,
  () => {
    fetchForms()
  },
)
</script>
