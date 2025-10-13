<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-xl font-semibold">게시판 관리</h2>
      <button
        @click="showCreateModal = true"
        class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 flex items-center gap-2"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
        </svg>
        게시글 작성
      </button>
    </div>

    <!-- 필터 & 검색 -->
    <div class="bg-white rounded-lg shadow-sm p-4 mb-6">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <select v-model="filter.category" class="px-3 py-2 border rounded-lg">
          <option value="">전체 카테고리</option>
          <option value="공지">공지</option>
          <option value="일정">일정</option>
          <option value="안내">안내</option>
        </select>

        <select v-model="filter.isImportant" class="px-3 py-2 border rounded-lg">
          <option value="">전체</option>
          <option value="true">중요 공지</option>
          <option value="false">일반</option>
        </select>

        <input
          v-model="filter.search"
          type="text"
          placeholder="제목 또는 내용 검색..."
          class="px-3 py-2 border rounded-lg col-span-2"
        />
      </div>
    </div>

    <!-- 통계 카드 -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
      <div class="bg-white rounded-lg shadow-sm p-4">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">전체 게시글</p>
            <p class="text-2xl font-bold text-gray-900">{{ stats.total }}</p>
          </div>
          <div class="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm p-4">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">중요 공지</p>
            <p class="text-2xl font-bold text-red-600">{{ stats.important }}</p>
          </div>
          <div class="w-12 h-12 bg-red-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm p-4">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">총 조회수</p>
            <p class="text-2xl font-bold text-green-600">{{ stats.totalViews }}</p>
          </div>
          <div class="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
            </svg>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm p-4">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">총 댓글</p>
            <p class="text-2xl font-bold text-purple-600">{{ stats.totalComments }}</p>
          </div>
          <div class="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
            </svg>
          </div>
        </div>
      </div>
    </div>

    <!-- 게시글 목록 테이블 -->
    <div class="bg-white rounded-lg shadow overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase w-12">
                <input type="checkbox" @change="toggleSelectAll" :checked="selectedPosts.length === filteredPosts.length && filteredPosts.length > 0" class="rounded" />
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">상태</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">카테고리</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">제목</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">작성자</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">작성일</th>
              <th class="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase">조회</th>
              <th class="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase">댓글</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">작업</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="post in filteredPosts" :key="post.id" class="hover:bg-gray-50">
              <td class="px-6 py-4" @click.stop>
                <input type="checkbox" :value="post.id" v-model="selectedPosts" class="rounded" />
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span v-if="post.isImportant" class="px-2 py-1 bg-red-100 text-red-800 rounded text-xs font-bold">필독</span>
                <span v-else class="px-2 py-1 bg-gray-100 text-gray-600 rounded text-xs">일반</span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span class="px-2 py-1 bg-blue-100 text-blue-800 rounded text-xs font-medium">{{ post.category }}</span>
              </td>
              <td class="px-6 py-4">
                <button @click="viewPost(post)" class="font-medium text-gray-900 hover:text-primary-600 text-left">
                  {{ post.title }}
                </button>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ post.author }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ formatDate(post.createdAt) }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-center text-sm text-gray-500">{{ post.views }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-center text-sm text-gray-500">{{ post.commentCount }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium" @click.stop>
                <button @click="editPost(post)" class="text-blue-600 hover:text-blue-900 mr-3">수정</button>
                <button @click="deletePost(post.id)" class="text-red-600 hover:text-red-900">삭제</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="selectedPosts.length > 0" class="bg-gray-50 px-6 py-3 flex items-center justify-between border-t">
        <span class="text-sm text-gray-700">{{ selectedPosts.length }}개 선택됨</span>
        <div class="flex gap-2">
          <button @click="bulkDelete" class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 text-sm">선택 삭제</button>
          <button @click="selectedPosts = []" class="px-4 py-2 border rounded-lg hover:bg-gray-100 text-sm">선택 해제</button>
        </div>
      </div>
    </div>

    <!-- 작성/수정 모달 -->
    <div v-if="showCreateModal || editingPost" @click.self="closeModal" class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
      <div class="bg-white rounded-lg w-full max-w-3xl max-h-[90vh] overflow-y-auto">
        <div class="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between">
          <h2 class="text-lg font-bold">{{ editingPost ? '게시글 수정' : '게시글 작성' }}</h2>
          <button @click="closeModal" class="p-2 hover:bg-gray-100 rounded-lg">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <div class="p-6 space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">카테고리 *</label>
              <select v-model="postForm.category" class="w-full px-3 py-2 border rounded-lg">
                <option value="공지">공지</option>
                <option value="일정">일정</option>
                <option value="안내">안내</option>
              </select>
            </div>
            <div class="flex items-end">
              <label class="flex items-center space-x-2">
                <input type="checkbox" v-model="postForm.isImportant" class="rounded" />
                <span class="text-sm font-medium text-gray-700">중요 공지</span>
              </label>
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">제목 *</label>
            <input v-model="postForm.title" type="text" placeholder="제목을 입력하세요" class="w-full px-3 py-2 border rounded-lg" maxlength="200" />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">내용 *</label>
            <textarea v-model="postForm.content" rows="12" placeholder="내용을 입력하세요" class="w-full px-3 py-2 border rounded-lg resize-none"></textarea>
          </div>

          <div class="flex justify-end space-x-3 pt-4">
            <button @click="closeModal" class="px-4 py-2 border rounded-lg hover:bg-gray-50">취소</button>
            <button @click="savePost" :disabled="!postForm.title || !postForm.content" class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:bg-gray-300">
              {{ editingPost ? '수정' : '등록' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 상세보기 모달 -->
    <div v-if="viewingPost" @click.self="closeViewModal" class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
      <div class="bg-white rounded-lg w-full max-w-3xl max-h-[90vh] overflow-y-auto">
        <div class="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between">
          <h2 class="text-lg font-bold">게시글 상세</h2>
          <button @click="closeViewModal" class="p-2 hover:bg-gray-100 rounded-lg">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <div class="p-6">
          <div class="flex items-center space-x-2 mb-3">
            <span v-if="viewingPost.isImportant" class="px-2 py-0.5 bg-red-100 text-red-700 rounded text-xs font-bold">필독</span>
            <span class="px-2 py-0.5 bg-blue-100 text-blue-700 rounded text-xs font-medium">{{ viewingPost.category }}</span>
          </div>
          <h1 class="text-2xl font-bold text-gray-900 mb-4">{{ viewingPost.title }}</h1>
          <div class="prose max-w-none mb-6">
            <p class="text-gray-700 whitespace-pre-wrap">{{ viewingPost.content }}</p>
          </div>
          <div class="flex justify-end space-x-3 pt-6 border-t">
            <button @click="editPost(viewingPost); closeViewModal()" class="px-4 py-2 border rounded-lg hover:bg-gray-50">수정</button>
            <button @click="deletePost(viewingPost.id); closeViewModal()" class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700">삭제</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  conventionId: { type: Number, required: true }
})

const showCreateModal = ref(false)
const editingPost = ref(null)
const viewingPost = ref(null)
const selectedPosts = ref([])

const filter = ref({
  category: '',
  isImportant: '',
  search: ''
})

const postForm = ref({
  category: '공지',
  isImportant: false,
  title: '',
  content: ''
})

const posts = ref([
  { id: 1, category: '공지', title: '행사 일정 변경 안내', content: '날씨 관계로 일부 일정이 변경되었습니다.', author: '관리자', createdAt: '2025-03-10T10:00:00', views: 234, commentCount: 12, isImportant: true },
  { id: 2, category: '안내', title: '호텔 체크인 정보', content: '호텔 체크인 시간 및 주의사항', author: '관리자', createdAt: '2025-03-09T14:30:00', views: 189, commentCount: 5, isImportant: true },
  { id: 3, category: '일정', title: '조별 일정 안내', content: 'A조와 B조의 관광 일정', author: '관리자', createdAt: '2025-03-08T09:00:00', views: 156, commentCount: 8, isImportant: false }
])

const stats = computed(() => ({
  total: posts.value.length,
  important: posts.value.filter(p => p.isImportant).length,
  totalViews: posts.value.reduce((sum, p) => sum + p.views, 0),
  totalComments: posts.value.reduce((sum, p) => sum + p.commentCount, 0)
}))

const filteredPosts = computed(() => {
  let filtered = posts.value
  if (filter.value.category) filtered = filtered.filter(p => p.category === filter.value.category)
  if (filter.value.isImportant) filtered = filtered.filter(p => p.isImportant === (filter.value.isImportant === 'true'))
  if (filter.value.search) {
    const search = filter.value.search.toLowerCase()
    filtered = filtered.filter(p => p.title.toLowerCase().includes(search) || p.content.toLowerCase().includes(search))
  }
  return filtered.sort((a, b) => {
    if (a.isImportant !== b.isImportant) return b.isImportant ? 1 : -1
    return new Date(b.createdAt) - new Date(a.createdAt)
  })
})

function formatDate(dateStr) {
  const date = new Date(dateStr)
  return `${date.getFullYear()}.${String(date.getMonth() + 1).padStart(2, '0')}.${String(date.getDate()).padStart(2, '0')}`
}

function toggleSelectAll(e) {
  selectedPosts.value = e.target.checked ? filteredPosts.value.map(p => p.id) : []
}

function viewPost(post) {
  viewingPost.value = post
}

function closeViewModal() {
  viewingPost.value = null
}

function editPost(post) {
  editingPost.value = post
  postForm.value = { category: post.category, isImportant: post.isImportant, title: post.title, content: post.content }
}

function closeModal() {
  showCreateModal.value = false
  editingPost.value = null
  postForm.value = { category: '공지', isImportant: false, title: '', content: '' }
}

function savePost() {
  if (!postForm.value.title || !postForm.value.content) return
  if (editingPost.value) {
    const index = posts.value.findIndex(p => p.id === editingPost.value.id)
    posts.value[index] = { ...posts.value[index], ...postForm.value }
  } else {
    posts.value.unshift({ id: Date.now(), ...postForm.value, author: '관리자', createdAt: new Date().toISOString(), views: 0, commentCount: 0 })
  }
  closeModal()
}

function deletePost(id) {
  if (confirm('삭제하시겠습니까?')) {
    posts.value = posts.value.filter(p => p.id !== id)
  }
}

function bulkDelete() {
  if (confirm(`${selectedPosts.value.length}개 삭제하시겠습니까?`)) {
    posts.value = posts.value.filter(p => !selectedPosts.value.includes(p.id))
    selectedPosts.value = []
  }
}
</script>
