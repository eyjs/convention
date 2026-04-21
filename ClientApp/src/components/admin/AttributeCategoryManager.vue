<template>
  <div>
    <AdminPageHeader
      title="속성 카테고리 관리"
      description="배정 정보 속성을 카테고리별로 묶어 더보기 화면에서 그룹화하여 표시합니다"
    >
      <AdminButton :icon="Plus" @click="openCreateForm">카테고리 추가</AdminButton>
    </AdminPageHeader>

    <div v-if="loading" class="text-center py-12 mt-6">
      <div
        class="inline-block w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"
      ></div>
    </div>

    <div v-else class="mt-6 space-y-4">
      <div
        v-if="attributeKeys.length > 0"
        class="bg-blue-50 border border-blue-100 rounded-lg px-4 py-3 text-sm text-blue-700"
      >
        <span class="font-medium">사용 가능한 속성 키:</span>
        <span class="ml-2">{{ attributeKeys.join(', ') }}</span>
      </div>

      <div v-if="categories.length > 0" class="grid gap-4">
        <div
          v-for="category in categories"
          :key="category.id"
          class="bg-white rounded-lg shadow-sm border border-gray-200 p-4 hover:shadow-md transition-shadow"
        >
          <div class="flex items-start justify-between gap-4">
            <div class="flex items-center gap-3 flex-1 min-w-0">
              <span class="text-2xl leading-none flex-shrink-0" aria-hidden="true">
                {{ category.icon || '📋' }}
              </span>
              <div class="flex-1 min-w-0">
                <div class="font-semibold text-gray-900">{{ category.name }}</div>
                <div class="text-xs text-gray-400 mt-1">
                  속성 {{ category.attributeKeys?.length || 0 }}개
                  <span v-if="category.attributeKeys?.length > 0">
                    · {{ category.attributeKeys.join(', ') }}
                  </span>
                </div>
              </div>
            </div>
            <div class="flex items-center gap-2 flex-shrink-0">
              <button
                class="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
                aria-label="카테고리 수정"
                @click="openEditForm(category)"
              >
                <PencilIcon class="w-4 h-4" />
              </button>
              <button
                class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                aria-label="카테고리 삭제"
                @click="deleteCategory(category)"
              >
                <TrashIcon class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>
      </div>

      <div
        v-else
        class="bg-white rounded-lg border border-gray-200 py-12 text-center text-sm text-gray-400"
      >
        등록된 카테고리가 없습니다. 카테고리를 추가해보세요.
      </div>
    </div>

    <BaseModal :show="showForm" @close="closeForm">
      <template #header>
        <h2 class="text-lg font-semibold">
          {{ editTarget ? '카테고리 수정' : '카테고리 추가' }}
        </h2>
      </template>

      <div class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1" for="cat-name">
            카테고리 이름 <span class="text-red-500">*</span>
          </label>
          <input
            id="cat-name"
            v-model="form.name"
            type="text"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            placeholder="예: 호텔 정보, 교통 정보"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1" for="cat-icon">
            아이콘 (이모지)
          </label>
          <input
            id="cat-icon"
            v-model="form.icon"
            type="text"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            placeholder="예: 🏨 🚌 📋"
          />
          <p class="text-xs text-gray-400 mt-1">이모지를 직접 입력하세요</p>
        </div>

        <div v-if="editTarget">
          <label class="block text-sm font-medium text-gray-700 mb-2">
            포함할 속성 키
          </label>
          <div v-if="attributeKeys.length > 0" class="flex flex-wrap gap-2">
            <!-- prettier-ignore -->
            <label
              v-for="key in attributeKeys"
              :key="key"
              class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg border cursor-pointer text-sm transition-colors"
              :class="form.attributeKeys.includes(key) ? 'bg-primary-50 border-primary-300 text-primary-700' : 'bg-white border-gray-200 text-gray-600 hover:border-gray-300'"
            >
              <input type="checkbox" :value="key" v-model="form.attributeKeys" class="sr-only" />
              {{ key }}
            </label>
          </div>
          <p v-else class="text-xs text-gray-400">
            이 행사에 등록된 속성 키가 없습니다.
          </p>
        </div>

        <p v-if="formError" class="text-sm text-red-600">{{ formError }}</p>
      </div>

      <template #footer>
        <button
          class="px-4 py-2 bg-white text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 text-sm"
          @click="closeForm"
        >
          취소
        </button>
        <AdminButton :loading="saving" @click="saveCategory">
          {{ editTarget ? '저장' : '추가' }}
        </AdminButton>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { Plus, Pencil as PencilIcon, Trash2 as TrashIcon } from 'lucide-vue-next'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminButton from '@/components/admin/ui/AdminButton.vue'
import BaseModal from '@/components/common/BaseModal.vue'
import apiClient from '@/services/api'

const route = useRoute()
const conventionId = route.params.conventionId

const loading = ref(false)
const saving = ref(false)
const categories = ref([])
const attributeKeys = ref([])
const showForm = ref(false)
const editTarget = ref(null)
const formError = ref('')
const form = ref({ name: '', icon: '', attributeKeys: [] })

async function loadData() {
  loading.value = true
  try {
    const [catRes, keysRes] = await Promise.all([
      apiClient.get(`/admin/conventions/${conventionId}/attribute-categories`),
      apiClient.get(`/admin/conventions/${conventionId}/attribute-keys`),
    ])
    categories.value = catRes.data || []
    attributeKeys.value = keysRes.data || []
  } catch (error) {
    console.error('속성 카테고리 로드 실패:', error)
  } finally {
    loading.value = false
  }
}

function openCreateForm() {
  editTarget.value = null
  form.value = { name: '', icon: '', attributeKeys: [] }
  formError.value = ''
  showForm.value = true
}

function openEditForm(category) {
  editTarget.value = category
  form.value = {
    name: category.name,
    icon: category.icon || '',
    attributeKeys: [...(category.attributeKeys || [])],
  }
  formError.value = ''
  showForm.value = true
}

function closeForm() {
  showForm.value = false
  editTarget.value = null
  formError.value = ''
}

async function saveCategory() {
  if (!form.value.name.trim()) {
    formError.value = '카테고리 이름은 필수입니다.'
    return
  }
  saving.value = true
  formError.value = ''
  try {
    if (editTarget.value) {
      await apiClient.put(`/admin/attribute-categories/${editTarget.value.id}`, {
        name: form.value.name.trim(),
        icon: form.value.icon.trim() || null,
        attributeKeys: form.value.attributeKeys,
      })
    } else {
      await apiClient.post(`/admin/conventions/${conventionId}/attribute-categories`, {
        name: form.value.name.trim(),
        icon: form.value.icon.trim() || null,
      })
    }
    closeForm()
    await loadData()
  } catch (error) {
    console.error('카테고리 저장 실패:', error)
    formError.value = error.response?.data?.message || '저장에 실패했습니다.'
  } finally {
    saving.value = false
  }
}

async function deleteCategory(category) {
  // eslint-disable-next-line no-alert
  if (!confirm(category.name + ' 카테고리를 삭제하시겠습니까?')) return
  try {
    await apiClient.delete(`/admin/attribute-categories/${category.id}`)
    await loadData()
  } catch (error) {
    console.error('카테고리 삭제 실패:', error)
    // eslint-disable-next-line no-alert
    alert(error.response?.data?.message || '삭제에 실패했습니다.')
  }
}

onMounted(() => {
  loadData()
})
</script>
