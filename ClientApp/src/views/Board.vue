<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 헤더 -->
    <div class="sticky top-0 z-40 bg-white shadow-sm">
      <div class="px-4 py-4">
        <div class="flex items-center justify-between">
          <div class="flex items-center space-x-3">
            <button @click="$router.back()" class="p-2 hover:bg-gray-100 rounded-lg">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
              </svg>
            </button>
            <h1 class="text-xl font-bold text-gray-900">공지사항</h1>
          </div>
          <button v-if="isAdmin" @click="showWriteModal = true" class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-medium">
            글쓰기
          </button>
        </div>
      </div>

      <!-- 카테고리 탭 -->
      <div class="border-t overflow-x-auto">
        <div class="flex px-4 space-x-1 min-w-max">
          <button
            v-for="category in categories"
            :key="category.id"
            @click="selectedCategory = category.id"
            :class="[
              'px-4 py-3 font-medium text-sm whitespace-nowrap border-b-2 transition-colors',
              selectedCategory === category.id
                ? 'text-primary-600 border-primary-600'
                : 'text-gray-500 border-transparent hover:text-gray-700'
            ]"
          >
            {{ category.name }}
            <span v-if="category.count" class="ml-1 text-xs opacity-70">({{ category.count }})</span>
          </button>
        </div>
      </div>
    </div>

    <!-- 게시글 목록 -->
    <div class="px-4 py-6 space-y-3">
      <!-- 중요 공지 -->
      <div v-for="notice in importantNotices" :key="notice.id" @click="openNotice(notice)" class="bg-white rounded-xl shadow-sm p-4 cursor-pointer hover:shadow-md transition-all border-l-4 border-red-500">
        <div class="flex items-start space-x-3">
          <div class="w-12 h-12 bg-red-100 rounded-lg flex items-center justify-center flex-shrink-0">
            <svg class="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
          <div class="flex-1 min-w-0">
            <div class="flex items-center space-x-2 mb-1">
              <span class="px-2 py-0.5 bg-red-100 text-red-700 rounded text-xs font-bold">필독</span>
              <span class="text-xs text-gray-500">{{ formatDate(notice.createdAt) }}</span>
            </div>
            <h3 class="font-bold text-gray-900 text-base mb-1">{{ notice.title }}</h3>
            <p class="text-sm text-gray-600 line-clamp-2">{{ notice.content }}</p>
            <div class="flex items-center space-x-4 mt-2 text-xs text-gray-500">
              <span class="flex items-center space-x-1">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
                <span>{{ notice.views }}</span>
              </span>
              <span v-if="notice.commentCount" class="flex items-center space-x-1">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                </svg>
                <span>{{ notice.commentCount }}</span>
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- 일반 공지 -->
      <div v-for="notice in filteredNotices" :key="notice.id" @click="openNotice(notice)" class="bg-white rounded-xl shadow-sm p-4 cursor-pointer hover:shadow-md transition-all">
        <div class="flex items-start space-x-3">
          <div class="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center flex-shrink-0">
            <svg class="w-6 h-6 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
            </svg>
          </div>
          <div class="flex-1 min-w-0">
            <div class="flex items-center space-x-2 mb-1">
              <span class="px-2 py-0.5 bg-blue-100 text-blue-700 rounded text-xs font-medium">{{ notice.category }}</span>
              <span class="text-xs text-gray-500">{{ formatDate(notice.createdAt) }}</span>
            </div>
            <h3 class="font-semibold text-gray-900 text-base mb-1">{{ notice.title }}</h3>
            <p class="text-sm text-gray-600 line-clamp-2">{{ notice.content }}</p>
            <div class="flex items-center space-x-4 mt-2 text-xs text-gray-500">
              <span class="flex items-center space-x-1">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
                <span>{{ notice.views }}</span>
              </span>
              <span v-if="notice.commentCount" class="flex items-center space-x-1">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                </svg>
                <span>{{ notice.commentCount }}</span>
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- 게시글 없음 -->
      <div v-if="filteredNotices.length === 0 && importantNotices.length === 0" class="text-center py-16">
        <svg class="w-16 h-16 mx-auto mb-4 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        <p class="text-gray-500">등록된 게시글이 없습니다</p>
      </div>
    </div>

    <!-- 공지 상세 모달 -->
    <div v-if="selectedNotice" @click="closeNotice" class="fixed inset-0 bg-black/50 z-50 flex items-end sm:items-center justify-center p-0 sm:p-4">
      <div @click.stop class="bg-white rounded-t-3xl sm:rounded-2xl w-full sm:max-w-2xl max-h-[90vh] overflow-y-auto">
        <!-- 헤더 -->
        <div class="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between">
          <h2 class="text-lg font-bold text-gray-900">공지사항</h2>
          <button @click="closeNotice" class="p-2 hover:bg-gray-100 rounded-lg">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- 내용 -->
        <div class="p-6">
          <!-- 제목 & 메타 -->
          <div class="mb-6">
            <div class="flex items-center space-x-2 mb-3">
              <span v-if="selectedNotice.isImportant" class="px-2 py-0.5 bg-red-100 text-red-700 rounded text-xs font-bold">필독</span>
              <span class="px-2 py-0.5 bg-blue-100 text-blue-700 rounded text-xs font-medium">{{ selectedNotice.category }}</span>
            </div>
            <h1 class="text-2xl font-bold text-gray-900 mb-3">{{ selectedNotice.title }}</h1>
            <div class="flex items-center space-x-4 text-sm text-gray-500">
              <span class="flex items-center space-x-1">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                <span>{{ selectedNotice.author }}</span>
              </span>
              <span>{{ formatDateTime(selectedNotice.createdAt) }}</span>
              <span class="flex items-center space-x-1">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
                <span>{{ selectedNotice.views }}</span>
              </span>
            </div>
          </div>

          <!-- 본문 -->
          <div class="prose max-w-none mb-6">
            <div class="text-gray-700 leading-relaxed whitespace-pre-wrap">{{ selectedNotice.content }}</div>
          </div>

          <!-- 첨부파일 -->
          <div v-if="selectedNotice.attachments && selectedNotice.attachments.length > 0" class="border-t pt-4 mb-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-3">첨부파일</h3>
            <div class="space-y-2">
              <a
                v-for="file in selectedNotice.attachments"
                :key="file.id"
                :href="file.url"
                target="_blank"
                class="flex items-center space-x-2 p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
              >
                <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                <span class="text-sm text-gray-700 flex-1">{{ file.name }}</span>
                <span class="text-xs text-gray-500">{{ file.size }}</span>
              </a>
            </div>
          </div>

          <!-- 댓글 섹션 -->
          <div class="border-t pt-6">
            <h3 class="font-semibold text-gray-900 mb-4">댓글 {{ selectedNotice.comments?.length || 0 }}</h3>
            
            <!-- 댓글 목록 -->
            <div class="space-y-4 mb-6">
              <div v-for="comment in selectedNotice.comments" :key="comment.id" class="flex space-x-3">
                <div class="w-8 h-8 bg-gray-200 rounded-full flex-shrink-0"></div>
                <div class="flex-1">
                  <div class="flex items-center space-x-2 mb-1">
                    <span class="text-sm font-semibold text-gray-900">{{ comment.author }}</span>
                    <span class="text-xs text-gray-500">{{ formatDateTime(comment.createdAt) }}</span>
                  </div>
                  <p class="text-sm text-gray-700">{{ comment.content }}</p>
                </div>
              </div>
            </div>

            <!-- 댓글 작성 -->
            <div class="flex space-x-3">
              <div class="w-8 h-8 bg-primary-200 rounded-full flex-shrink-0"></div>
              <div class="flex-1">
                <textarea
                  v-model="newComment"
                  placeholder="댓글을 입력하세요..."
                  rows="3"
                  class="w-full px-3 py-2 border rounded-lg resize-none focus:outline-none focus:ring-2 focus:ring-primary-500"
                ></textarea>
                <div class="flex justify-end mt-2">
                  <button
                    @click="submitComment"
                    :disabled="!newComment.trim()"
                    class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm font-medium disabled:bg-gray-300 disabled:cursor-not-allowed"
                  >
                    댓글 작성
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 글쓰기 모달 (관리자용) -->
    <div v-if="showWriteModal" @click="closeWriteModal" class="fixed inset-0 bg-black/50 z-50 flex items-end sm:items-center justify-center p-0 sm:p-4">
      <div @click.stop class="bg-white rounded-t-3xl sm:rounded-2xl w-full sm:max-w-2xl max-h-[90vh] overflow-y-auto">
        <div class="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between">
          <h2 class="text-lg font-bold text-gray-900">공지사항 작성</h2>
          <button @click="closeWriteModal" class="p-2 hover:bg-gray-100 rounded-lg">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <div class="p-6 space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">카테고리</label>
            <select v-model="newNotice.category" class="w-full px-3 py-2 border rounded-lg">
              <option value="공지">공지</option>
              <option value="일정">일정</option>
              <option value="안내">안내</option>
            </select>
          </div>

          <div>
            <label class="flex items-center space-x-2">
              <input type="checkbox" v-model="newNotice.isImportant" class="rounded" />
              <span class="text-sm font-medium text-gray-700">중요 공지로 등록</span>
            </label>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">제목</label>
            <input
              v-model="newNotice.title"
              type="text"
              placeholder="제목을 입력하세요"
              class="w-full px-3 py-2 border rounded-lg"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">내용</label>
            <textarea
              v-model="newNotice.content"
              rows="10"
              placeholder="내용을 입력하세요"
              class="w-full px-3 py-2 border rounded-lg resize-none"
            ></textarea>
          </div>

          <div class="flex justify-end space-x-3 pt-4">
            <button @click="closeWriteModal" class="px-4 py-2 border rounded-lg hover:bg-gray-50">
              취소
            </button>
            <button
              @click="submitNotice"
              :disabled="!newNotice.title || !newNotice.content"
              class="px-4 py-2 bg-primary-600 text-white rounded-lg disabled:bg-gray-300"
            >
              등록
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/api'

const authStore = useAuthStore()
const isAdmin = computed(() => authStore.user?.role === 'Admin')

const selectedCategory = ref('all')
const selectedNotice = ref(null)
const showWriteModal = ref(false)
const newComment = ref('')
const newNotice = ref({
  category: '공지',
  isImportant: false,
  title: '',
  content: ''
})

const categories = ref([
  { id: 'all', name: '전체', count: 15 },
  { id: '공지', name: '공지', count: 8 },
  { id: '일정', name: '일정', count: 4 },
  { id: '안내', name: '안내', count: 3 }
])

const notices = ref([
  {
    id: 1,
    category: '공지',
    title: '행사 일정 변경 안내',
    content: '날씨 관계로 일부 일정이 변경되었습니다.\n\n변경 사항:\n- 3월 13일 바티칸 투어 → 3월 14일로 연기\n- 3월 13일은 자유 시간으로 변경\n\n불편을 드려 죄송합니다.',
    author: '관리자',
    createdAt: '2025-03-10T10:00:00',
    views: 234,
    commentCount: 12,
    isImportant: true,
    comments: [
      { id: 1, author: '김철수', content: '확인했습니다!', createdAt: '2025-03-10T11:00:00' },
      { id: 2, author: '이영희', content: '감사합니다', createdAt: '2025-03-10T12:00:00' }
    ],
    attachments: []
  },
  {
    id: 2,
    category: '안내',
    title: '호텔 체크인 정보',
    content: '호텔 체크인 시간 및 주의사항 안내드립니다.\n\n체크인: 15:00\n체크아웃: 11:00\n\n객실 배정은 현지 도착 후 진행됩니다.',
    author: '관리자',
    createdAt: '2025-03-09T14:30:00',
    views: 189,
    commentCount: 5,
    isImportant: true,
    comments: [],
    attachments: []
  },
  {
    id: 3,
    category: '일정',
    title: '3월 13일 조별 일정 안내',
    content: 'A조와 B조의 관광 일정이 다릅니다. 각자 배정된 조를 확인해주세요.',
    author: '관리자',
    createdAt: '2025-03-08T09:00:00',
    views: 156,
    commentCount: 8,
    isImportant: false,
    comments: [],
    attachments: []
  },
  {
    id: 4,
    category: '공지',
    title: '로밍 및 와이파이 안내',
    content: '현지에서 사용 가능한 로밍 요금제와 포켓와이파이 대여 정보입니다.',
    author: '관리자',
    createdAt: '2025-03-07T16:00:00',
    views: 203,
    commentCount: 15,
    isImportant: false,
    comments: [],
    attachments: [
      { id: 1, name: '로밍_안내.pdf', size: '1.2MB', url: '#' }
    ]
  },
  {
    id: 5,
    category: '안내',
    title: '여행자 보험 안내',
    content: '모든 참가자는 여행자 보험에 자동 가입됩니다.',
    author: '관리자',
    createdAt: '2025-03-06T10:00:00',
    views: 145,
    commentCount: 3,
    isImportant: false,
    comments: [],
    attachments: []
  }
])

const importantNotices = computed(() => 
  notices.value.filter(n => n.isImportant)
)

const filteredNotices = computed(() => {
  let filtered = notices.value.filter(n => !n.isImportant)
  if (selectedCategory.value !== 'all') {
    filtered = filtered.filter(n => n.category === selectedCategory.value)
  }
  return filtered
})

function formatDate(dateStr) {
  const date = new Date(dateStr)
  const now = new Date()
  const diff = now - date
  const hours = Math.floor(diff / (1000 * 60 * 60))
  
  if (hours < 1) return '방금 전'
  if (hours < 24) return `${hours}시간 전`
  if (hours < 48) return '어제'
  
  return `${date.getMonth() + 1}/${date.getDate()}`
}

function formatDateTime(dateStr) {
  const date = new Date(dateStr)
  return `${date.getFullYear()}.${String(date.getMonth() + 1).padStart(2, '0')}.${String(date.getDate()).padStart(2, '0')} ${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`
}

function openNotice(notice) {
  selectedNotice.value = { ...notice }
  // 조회수 증가 API 호출
  // apiClient.post(`/board/${notice.id}/view`)
}

function closeNotice() {
  selectedNotice.value = null
  newComment.value = ''
}

function submitComment() {
  if (!newComment.value.trim()) return
  
  const comment = {
    id: Date.now(),
    author: authStore.user?.name || '사용자',
    content: newComment.value,
    createdAt: new Date().toISOString()
  }
  
  selectedNotice.value.comments.push(comment)
  newComment.value = ''
  
  // API 호출
  // apiClient.post(`/board/${selectedNotice.value.id}/comments`, { content: comment.content })
}

function closeWriteModal() {
  showWriteModal.value = false
  newNotice.value = {
    category: '공지',
    isImportant: false,
    title: '',
    content: ''
  }
}

async function submitNotice() {
  if (!newNotice.value.title || !newNotice.value.content) return

  const notice = {
    id: Date.now(),
    ...newNotice.value,
    author: '관리자',
    createdAt: new Date().toISOString(),
    views: 0,
    commentCount: 0,
    comments: [],
    attachments: []
  }

  if (newNotice.value.isImportant) {
    notices.value.unshift(notice)
  } else {
    notices.value.push(notice)
  }

  // API 호출
  // await apiClient.post('/board', newNotice.value)

  closeWriteModal()
}
</script>
