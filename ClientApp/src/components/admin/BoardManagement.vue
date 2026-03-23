<template>
  <div>
    <!-- 탭 네비게이션 -->
    <div class="mb-6 border-b">
      <nav class="flex space-x-4">
        <button
          :class="[
            'pb-3 px-4 border-b-2 font-medium text-sm transition-colors',
            activeSubTab === 'posts'
              ? 'border-primary-600 text-primary-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
          ]"
          @click="activeSubTab = 'posts'"
        >
          게시글 관리
        </button>
        <button
          :class="[
            'pb-3 px-4 border-b-2 font-medium text-sm transition-colors',
            activeSubTab === 'categories'
              ? 'border-primary-600 text-primary-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
          ]"
          @click="activeSubTab = 'categories'"
        >
          카테고리 관리
        </button>
      </nav>
    </div>

    <!-- 게시글 관리 탭 -->
    <div v-if="activeSubTab === 'posts'">
      <AdminPageHeader title="게시글 관리">
        <AdminButton :icon="Plus" @click="showCreateModal = true"
          >게시글 작성</AdminButton
        >
      </AdminPageHeader>

      <!-- 통계 카드 -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-6 mb-6">
        <AdminStatsCard
          label="전체 게시글"
          :value="totalCount"
          :icon="FileText"
          color="primary"
        />
        <AdminStatsCard
          label="고정 공지"
          :value="pinnedCount"
          :icon="Bookmark"
          color="red"
        />
        <AdminStatsCard
          label="총 조회수"
          :value="totalViews"
          :icon="Eye"
          color="green"
        />
      </div>

      <!-- 게시글 목록 테이블 -->
      <div class="bg-white rounded-lg shadow overflow-hidden">
        <div v-if="loading" class="p-8 text-center">로딩 중...</div>
        <div
          v-else-if="notices.length === 0"
          class="p-8 text-center text-gray-500"
        >
          등록된 게시글이 없습니다
        </div>
        <div v-else class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th
                  class="px-3 py-3 text-left text-xs font-medium text-gray-500 uppercase"
                >
                  <GripHorizontal class="w-4 h-4" />
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
                >
                  상태
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
                >
                  카테고리
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
                >
                  제목
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
                >
                  작성자
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
                >
                  작성일
                </th>
                <th
                  class="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase"
                >
                  조회
                </th>
                <th
                  class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase"
                >
                  작업
                </th>
              </tr>
            </thead>
            <VueDraggable
              v-model="notices"
              tag="tbody"
              item-key="id"
              handle=".drag-handle"
              class="bg-white divide-y divide-gray-200"
              @end="onDragEnd"
            >
              <template #item="{ element: notice }">
                <tr class="hover:bg-gray-50">
                  <td class="px-3 py-4 whitespace-nowrap">
                    <div class="drag-handle cursor-move">
                      <GripHorizontal
                        class="w-5 h-5 text-gray-400 hover:text-gray-600"
                      />
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <AdminBadge v-if="notice.isPinned" variant="danger"
                      >필독</AdminBadge
                    >
                    <AdminBadge v-else variant="neutral">일반</AdminBadge>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span
                      v-if="notice.categoryName"
                      class="px-2 py-1 rounded text-xs font-medium"
                      :style="{
                        backgroundColor: notice.categoryColor + '20',
                        color: notice.categoryColor || '#6B7280',
                      }"
                    >
                      {{ notice.categoryName }}
                    </span>
                    <span v-else class="text-gray-400 text-xs">미분류</span>
                  </td>
                  <td class="px-6 py-4">
                    <button
                      class="font-medium text-gray-900 hover:text-primary-600 text-left"
                      @click="viewNotice(notice)"
                    >
                      {{ notice.title }}
                    </button>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ notice.authorName }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ formatDate(notice.createdAt) }}
                  </td>
                  <td
                    class="px-6 py-4 whitespace-nowrap text-center text-sm text-gray-500"
                  >
                    {{ notice.viewCount }}
                  </td>
                  <td
                    class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium"
                  >
                    <button
                      class="text-yellow-600 hover:text-yellow-900 mr-3"
                      @click="togglePin(notice)"
                    >
                      {{ notice.isPinned ? '고정해제' : '고정' }}
                    </button>
                    <button
                      class="text-primary-600 hover:text-primary-900 mr-3"
                      @click="editNotice(notice)"
                    >
                      수정
                    </button>
                    <button
                      class="text-red-600 hover:text-red-900"
                      @click="deleteNotice(notice.id)"
                    >
                      삭제
                    </button>
                  </td>
                </tr>
              </template>
            </VueDraggable>
          </table>
        </div>
      </div>
    </div>

    <!-- 카테고리 관리 탭 -->
    <div v-if="activeSubTab === 'categories'">
      <AdminPageHeader title="카테고리 관리">
        <AdminButton :icon="Plus" @click="showCategoryModal = true"
          >카테고리 추가</AdminButton
        >
      </AdminPageHeader>

      <!-- 카테고리 목록 -->
      <div class="bg-white rounded-lg shadow overflow-hidden">
        <div v-if="loadingCategories" class="p-8 text-center">로딩 중...</div>
        <div
          v-else-if="categories.length === 0"
          class="p-8 text-center text-gray-500"
        >
          등록된 카테고리가 없습니다
        </div>
        <div v-else class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
                >
                  카테고리명
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase"
                >
                  설명
                </th>
                <th
                  class="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase"
                >
                  게시글 수
                </th>
                <th
                  class="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase"
                >
                  표시 순서
                </th>
                <th
                  class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase"
                >
                  작업
                </th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr
                v-for="category in categories"
                :key="category.id"
                class="hover:bg-gray-50"
              >
                <td
                  class="px-6 py-4 whitespace-nowrap font-medium text-gray-900"
                >
                  {{ category.name }}
                </td>
                <td class="px-6 py-4 text-sm text-gray-500">
                  {{ category.description || '-' }}
                </td>
                <td
                  class="px-6 py-4 whitespace-nowrap text-center text-sm text-gray-500"
                >
                  {{ category.noticeCount }}
                </td>
                <td
                  class="px-6 py-4 whitespace-nowrap text-center text-sm text-gray-500"
                >
                  {{ category.displayOrder }}
                </td>
                <td
                  class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium"
                >
                  <button
                    class="text-primary-600 hover:text-primary-900 mr-3"
                    @click="editCategory(category)"
                  >
                    수정
                  </button>
                  <button
                    class="text-red-600 hover:text-red-900"
                    @click="deleteCategory(category.id, category.noticeCount)"
                  >
                    삭제
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- 게시글 작성/수정 모달 -->
    <BaseModal
      :is-open="showCreateModal || !!editingNotice"
      max-width="3xl"
      @close="closeModal"
    >
      <template #header>
        <h2 class="text-lg font-bold">
          {{ editingNotice ? '게시글 수정' : '게시글 작성' }}
        </h2>
      </template>
      <template #body>
        <div class="space-y-4">
          <div class="flex items-center">
            <label class="flex items-center space-x-2">
              <input
                v-model="noticeForm.isPinned"
                type="checkbox"
                class="rounded"
              />
              <span class="text-sm font-medium text-gray-700"
                >필독 공지로 설정</span
              >
            </label>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2"
              >카테고리</label
            >
            <select
              v-model="noticeForm.noticeCategoryId"
              class="w-full px-3 py-2 border rounded-lg"
            >
              <option :value="null">카테고리 없음</option>
              <option
                v-for="category in categories"
                :key="category.id"
                :value="category.id"
              >
                {{ category.name }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2"
              >제목 *</label
            >
            <input
              v-model="noticeForm.title"
              type="text"
              placeholder="제목을 입력하세요"
              class="w-full px-3 py-2 border rounded-lg"
              maxlength="200"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2"
              >내용 *</label
            >
            <RichTextEditor v-model="noticeForm.content" />
          </div>
        </div>
      </template>
      <template #footer>
        <button
          class="px-4 py-2 border rounded-lg hover:bg-gray-50"
          @click="closeModal"
        >
          취소
        </button>
        <button
          :disabled="!noticeForm.title || !noticeForm.content ? true : null"
          class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:bg-gray-300"
          @click="saveNotice"
        >
          {{ editingNotice ? '수정' : '등록' }}
        </button>
      </template>
    </BaseModal>

    <!-- 카테고리 생성/수정 모달 -->
    <BaseModal
      :is-open="showCategoryModal || !!editingCategory"
      max-width="md"
      @close="closeCategoryModal"
    >
      <template #header>
        <h2 class="text-lg font-bold">
          {{ editingCategory ? '카테고리 수정' : '카테고리 추가' }}
        </h2>
      </template>
      <template #body>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2"
              >카테고리명 *</label
            >
            <input
              v-model="categoryForm.name"
              type="text"
              placeholder="카테고리명을 입력하세요"
              class="w-full px-3 py-2 border rounded-lg"
              maxlength="100"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2"
              >설명</label
            >
            <textarea
              v-model="categoryForm.description"
              rows="3"
              placeholder="카테고리 설명 (선택사항)"
              class="w-full px-3 py-2 border rounded-lg resize-none"
              maxlength="500"
            ></textarea>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2"
              >표시 순서</label
            >
            <input
              v-model.number="categoryForm.displayOrder"
              type="number"
              min="0"
              class="w-full px-3 py-2 border rounded-lg"
            />
          </div>
        </div>
      </template>
      <template #footer>
        <button
          class="px-4 py-2 border rounded-lg hover:bg-gray-50"
          @click="closeCategoryModal"
        >
          취소
        </button>
        <button
          :disabled="!categoryForm.name ? true : null"
          class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:bg-gray-300"
          @click="saveCategory"
        >
          {{ editingCategory ? '수정' : '추가' }}
        </button>
      </template>
    </BaseModal>

    <!-- 상세보기 모달 -->
    <BaseModal
      :is-open="!!viewingNotice"
      max-width="4xl"
      @close="closeViewModal"
    >
      <template #header>
        <h2 class="text-base md:text-lg font-bold">게시글 상세</h2>
      </template>
      <template #body>
        <div class="px-4 md:px-6 py-5">
          <div class="flex flex-wrap items-center gap-2 mb-3">
            <span
              v-if="viewingNotice.isPinned"
              class="px-2 py-0.5 bg-red-100 text-red-700 rounded text-xs font-bold"
              >필독</span
            >
            <span
              v-if="viewingNotice.categoryName"
              class="px-2 py-1 rounded text-xs font-medium"
              :style="{
                backgroundColor: viewingNotice.categoryColor + '20',
                color: viewingNotice.categoryColor || '#6B7280',
              }"
            >
              {{ viewingNotice.categoryName }}
            </span>
          </div>
          <h1 class="text-xl md:text-2xl font-bold text-gray-900 mb-4">
            {{ viewingNotice.title }}
          </h1>
          <div
            class="flex flex-wrap items-center gap-2 md:gap-4 text-xs md:text-sm text-gray-500 mb-6"
          >
            <span>작성자: {{ viewingNotice.authorName }}</span>
            <span class="hidden md:inline">|</span>
            <span>조회수: {{ viewingNotice.viewCount }}</span>
            <span class="hidden md:inline">|</span>
            <span>작성일: {{ formatDate(viewingNotice.createdAt) }}</span>
          </div>
          <div class="prose prose-sm md:prose-base max-w-none mb-6">
            <div v-html="viewingNotice.content"></div>
          </div>
        </div>
      </template>
      <template #footer>
        <!-- prettier-ignore -->
        <button
          class="px-3 md:px-4 py-2 text-sm md:text-base border rounded-lg hover:bg-gray-50"
          @click="editNotice(viewingNotice); closeViewModal()"
        >
          수정
        </button>
        <!-- prettier-ignore -->
        <button
          class="px-3 md:px-4 py-2 text-sm md:text-base bg-red-600 text-white rounded-lg hover:bg-red-700"
          @click="deleteNotice(viewingNotice.id); closeViewModal()"
        >
          삭제
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { Plus, FileText, Bookmark, Eye, GripHorizontal } from 'lucide-vue-next'
import apiClient from '@/services/api'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminButton from '@/components/admin/ui/AdminButton.vue'
import AdminStatsCard from '@/components/admin/ui/AdminStatsCard.vue'
import AdminBadge from '@/components/admin/ui/AdminBadge.vue'
import RichTextEditor from '@/components/common/RichTextEditor.vue'
import BaseModal from '@/components/common/BaseModal.vue'
import VueDraggable from 'vuedraggable'
import 'quill/dist/quill.snow.css'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const activeSubTab = ref('posts')
const loading = ref(true)
const loadingCategories = ref(true)
const notices = ref([])
const categories = ref([])
const showCreateModal = ref(false)
const showCategoryModal = ref(false)
const editingNotice = ref(null)
const editingCategory = ref(null)
const viewingNotice = ref(null)

const noticeForm = ref({
  isPinned: false,
  noticeCategoryId: null,
  title: '',
  content: '',
  attachments: [],
})

const categoryForm = ref({
  name: '',
  description: '',
  displayOrder: 0,
})

const totalCount = computed(() => notices.value.length)
const pinnedCount = computed(
  () => notices.value.filter((n) => n.isPinned).length,
)
const totalViews = computed(() =>
  notices.value.reduce((sum, n) => sum + n.viewCount, 0),
)

// 카테고리 목록 로드
async function loadCategories() {
  try {
    loadingCategories.value = true
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/notice-categories`,
    )
    categories.value = response.data
  } catch (error) {
    console.error('Failed to load categories:', error)
    alert('카테고리를 불러오는데 실패했습니다')
  } finally {
    loadingCategories.value = false
  }
}

// 게시글 목록 로드
async function loadNotices() {
  try {
    loading.value = true
    const response = await apiClient.get(`/admin/notices`, {
      params: { conventionId: props.conventionId },
    })
    notices.value = response.data.items
  } catch (error) {
    console.error('Failed to load notices:', error)
    alert('게시글을 불러오는데 실패했습니다')
  } finally {
    loading.value = false
  }
}

function formatDate(dateStr) {
  const date = new Date(dateStr)
  return `${date.getFullYear()}.${String(date.getMonth() + 1).padStart(2, '0')}.${String(date.getDate()).padStart(2, '0')}`
}

async function viewNotice(notice) {
  try {
    const response = await apiClient.get(`/notices/${notice.id}`)
    viewingNotice.value = response.data
  } catch (error) {
    console.error('Failed to fetch notice details:', error)
    alert('공지사항을 불러오는데 실패했습니다.')
  }
}

function closeViewModal() {
  viewingNotice.value = null
}

async function editNotice(notice) {
  try {
    const response = await apiClient.get(`/admin/notices/${notice.id}`)
    editingNotice.value = response.data
    noticeForm.value = {
      isPinned: response.data.isPinned,
      noticeCategoryId: response.data.noticeCategoryId,
      title: response.data.title,
      content: response.data.content,
      attachments: response.data.attachments || [],
    }
    showCreateModal.value = true
  } catch (error) {
    console.error('Failed to fetch notice details:', error)
    alert('공지사항 정보를 불러오는데 실패했습니다.')
  }
}

function closeModal() {
  showCreateModal.value = false
  editingNotice.value = null
  noticeForm.value = {
    isPinned: false,
    noticeCategoryId: null,
    title: '',
    content: '',
    attachments: [],
  }
}

async function saveNotice() {
  if (!noticeForm.value.title || !noticeForm.value.content) return

  try {
    const payload = {
      ...noticeForm.value,
      attachmentIds: noticeForm.value.attachments?.map((a) => a.id) || [],
    }
    console.log('Saving notice with payload:', payload)

    if (editingNotice.value) {
      await apiClient.put(`/admin/notices/${editingNotice.value.id}`, payload)
    } else {
      await apiClient.post(
        `/admin/notices?conventionId=${props.conventionId}`,
        payload,
      )
    }
    await loadNotices()
    closeModal()
  } catch (error) {
    console.error('Failed to save notice:', error.response || error)
    alert('저장에 실패했습니다')
  }
}

async function deleteNotice(id) {
  if (!confirm('삭제하시겠습니까?')) return

  try {
    console.log('Deleting notice with id:', id)
    await apiClient.delete(`/admin/notices/${id}`)
    await loadNotices()
  } catch (error) {
    console.error('Failed to delete notice:', error.response || error)
    alert('삭제에 실패했습니다')
  }
}

async function togglePin(notice) {
  try {
    await apiClient.post(`/admin/notices/${notice.id}/toggle-pin`)
    await loadNotices()
  } catch (error) {
    console.error('Failed to toggle pin:', error)
    alert('고정 상태 변경에 실패했습니다')
  }
}

async function onDragEnd() {
  try {
    // Update display order based on current array order
    const orders = notices.value.map((notice, index) => ({
      id: notice.id,
      displayOrder: index,
    }))

    await apiClient.put('/admin/notices/update-order', { orders })
    console.log('Notice order updated successfully')
  } catch (error) {
    console.error('Failed to update notice order:', error)
    alert('순서 변경에 실패했습니다')
    // Reload to revert to server state
    await loadNotices()
  }
}

// 카테고리 관련 함수들
function editCategory(category) {
  editingCategory.value = category
  categoryForm.value = {
    name: category.name,
    description: category.description || '',
    displayOrder: category.displayOrder,
  }
  showCategoryModal.value = true
}

function closeCategoryModal() {
  showCategoryModal.value = false
  editingCategory.value = null
  categoryForm.value = {
    name: '',
    description: '',
    displayOrder: 0,
  }
}

async function saveCategory() {
  if (!categoryForm.value.name) return

  try {
    const payload = { ...categoryForm.value, conventionId: props.conventionId }
    if (editingCategory.value) {
      await apiClient.put(
        `/admin/conventions/${props.conventionId}/notice-categories/${editingCategory.value.id}`,
        payload,
      )
    } else {
      await apiClient.post(
        `/admin/conventions/${props.conventionId}/notice-categories`,
        payload,
      )
    }
    await loadCategories()
    closeCategoryModal()
  } catch (error) {
    console.error('Failed to save category:', error)
    alert(error.response?.data?.message || '카테고리 저장에 실패했습니다')
  }
}

async function deleteCategory(id, noticeCount) {
  if (noticeCount > 0) {
    alert(
      `이 카테고리에 ${noticeCount}개의 게시글이 있어 삭제할 수 없습니다.\n게시글을 먼저 삭제하거나 다른 카테고리로 이동해주세요.`,
    )
    return
  }

  if (!confirm('카테고리를 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(
      `/admin/conventions/${props.conventionId}/notice-categories/${id}`,
    )
    await loadCategories()
  } catch (error) {
    console.error('Failed to delete category:', error)
    alert(error.response?.data?.message || '카테고리 삭제에 실패했습니다')
  }
}

// 탭 변경 시 데이터 로드
watch(activeSubTab, (newTab) => {
  if (newTab === 'categories' && categories.value.length === 0) {
    loadCategories()
  }
})

onMounted(async () => {
  await loadCategories() // 게시글 작성 시 카테고리 선택을 위해 먼저 로드
  await loadNotices()
})
</script>
