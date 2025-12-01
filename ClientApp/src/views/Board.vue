<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <!-- 공통 헤더 사용 -->
    <MainHeader title="게시판" :show-back="true" />

    <!-- 카테고리 탭 -->
    <div class="sticky top-[72px] z-30 bg-white">
      <div class="relative border-t">
        <div
          ref="categoryContainer"
          class="overflow-x-auto whitespace-nowrap no-scrollbar"
          @scroll="handleScroll"
        >
          <div class="flex px-4 space-x-1 min-w-max">
            <button
              v-for="category in categories"
              :key="category.id"
              @click="selectedCategory = category.id"
              :class="[
                'px-4 py-3 font-medium text-sm whitespace-nowrap border-b-2 transition-colors',
                selectedCategory === category.id
                  ? 'text-primary-600 border-primary-600'
                  : 'text-gray-500 border-transparent hover:text-gray-700',
              ]"
            >
              {{ category.name }}
              <span v-if="category.count" class="ml-1 text-xs opacity-70"
                >({{ category.count }})</span
              >
            </button>
          </div>
        </div>
        <div
          v-if="showLeftScroll"
          class="absolute left-0 top-0 bottom-0 flex items-center bg-gradient-to-r from-white to-transparent pr-4"
        >
          <button
            @click="scrollLeft"
            class="p-1 bg-white rounded-full shadow-md"
          >
            <svg
              class="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M15 19l-7-7 7-7"
              />
            </svg>
          </button>
        </div>
        <div
          v-if="showRightScroll"
          class="absolute right-0 top-0 bottom-0 flex items-center bg-gradient-to-l from-white to-transparent pl-4"
        >
          <button
            @click="scrollRight"
            class="p-1 bg-white rounded-full shadow-md"
          >
            <svg
              class="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 5l7 7-7 7"
              />
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- BOARD_CONTENT_TOP 위치: 카테고리 필터 아래 -->
    <div v-if="contentTopActions.length > 0" class="px-4 pt-4">
      <DynamicActionRenderer :features="contentTopActions" />
    </div>

    <!-- 게시글 목록 -->
    <div class="px-4 py-6 space-y-3">
      <!-- 중요 공지 -->
      <div
        v-for="notice in importantNotices"
        :key="notice.id"
        @click="openNotice(notice)"
        class="bg-white rounded-xl shadow-sm p-4 cursor-pointer hover:shadow-md transition-all border-l-4 border-red-500"
      >
        <div class="flex items-start space-x-3">
          <div
            class="w-12 h-12 bg-red-100 rounded-lg flex items-center justify-center flex-shrink-0"
          >
            <svg
              class="w-6 h-6 text-red-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
              />
            </svg>
          </div>
          <div class="flex-1 min-w-0">
            <div class="flex items-center space-x-2 mb-1">
              <span
                class="px-2 py-0.5 bg-red-100 text-red-700 rounded text-xs font-bold"
                >필독</span
              >
              <span class="text-xs text-gray-500">{{
                formatDate(notice.createdAt)
              }}</span>
            </div>
            <h3 class="font-bold text-gray-900 text-base mb-1">
              {{ notice.title }}
            </h3>
            <p class="text-sm text-gray-600 line-clamp-2">
              {{ notice.content }}
            </p>
            <div class="flex items-center space-x-4 mt-2 text-xs text-gray-500">
              <span class="flex items-center space-x-1">
                <svg
                  class="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                  />
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                  />
                </svg>
                <span>{{ notice.views }}</span>
              </span>
              <span
                v-if="notice.commentCount"
                class="flex items-center space-x-1"
              >
                <svg
                  class="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
                  />
                </svg>
                <span>{{ notice.commentCount }}</span>
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- 일반 공지 -->
      <div
        v-for="notice in filteredNotices"
        :key="notice.id"
        @click="openNotice(notice)"
        class="bg-white rounded-xl shadow-sm p-4 cursor-pointer hover:shadow-md transition-all"
      >
        <div class="flex items-start space-x-3">
          <div
            class="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center flex-shrink-0"
          >
            <svg
              class="w-6 h-6 text-primary-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
              />
            </svg>
          </div>
          <div class="flex-1 min-w-0">
            <div class="flex items-center space-x-2 mb-1">
              <span
                class="px-2 py-0.5 bg-blue-100 text-blue-700 rounded text-xs font-medium"
                >{{ notice.category }}</span
              >
              <span class="text-xs text-gray-500">{{
                formatDate(notice.createdAt)
              }}</span>
            </div>
            <h3 class="font-semibold text-gray-900 text-base mb-1">
              {{ notice.title }}
            </h3>
            <p class="text-sm text-gray-600 line-clamp-2">
              {{ notice.content }}
            </p>
            <div class="flex items-center space-x-4 mt-2 text-xs text-gray-500">
              <span class="flex items-center space-x-1">
                <svg
                  class="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                  />
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                  />
                </svg>
                <span>{{ notice.views }}</span>
              </span>
              <span
                v-if="notice.commentCount"
                class="flex items-center space-x-1"
              >
                <svg
                  class="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
                  />
                </svg>
                <span>{{ notice.commentCount }}</span>
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- 게시글 없음 -->
      <div
        v-if="filteredNotices.length === 0 && importantNotices.length === 0"
        class="text-center py-16"
      >
        <svg
          class="w-16 h-16 mx-auto mb-4 text-gray-300"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
          />
        </svg>
        <p class="text-gray-500">등록된 게시글이 없습니다</p>
      </div>
    </div>

    <!-- 공지 상세 모달 -->
    <SlideUpModal
      :is-open="!!selectedNotice"
      @close="closeNotice"
      :disable-history-management="true"
    >
      <template #header-title>공지사항</template>
      <template #body>
        <div v-if="selectedNotice" class="p-6">
          <!-- 제목 & 메타 -->
          <div class="mb-6">
            <div class="flex items-center space-x-2 mb-3">
              <span
                v-if="selectedNotice.isPinned"
                class="px-2 py-0.5 bg-red-100 text-red-700 rounded text-xs font-bold"
                >필독</span
              >
              <span
                class="px-2 py-0.5 bg-blue-100 text-blue-700 rounded text-xs font-medium"
                >{{ selectedNotice.category }}</span
              >
            </div>
            <h1 class="text-2xl font-bold text-gray-900 mb-3">
              {{ selectedNotice.title }}
            </h1>
            <div class="flex items-center space-x-4 text-sm text-gray-500">
              <span class="flex items-center space-x-1">
                <svg
                  class="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                  />
                </svg>
                <span>{{ selectedNotice.author }}</span>
              </span>
              <span>{{ formatDateTime(selectedNotice.createdAt) }}</span>
              <span class="flex items-center space-x-1">
                <svg
                  class="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                  />
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                  />
                </svg>
                <span>{{ selectedNotice.views }}</span>
              </span>
            </div>
          </div>

          <!-- 본문 -->
          <div class="mb-6" v-viewer ref="viewer">
            <QuillViewer
              :content="selectedNotice.content"
              @image-clicked="openImageViewer"
            />
          </div>

          <!-- 첨부파일 -->
          <div
            v-if="
              selectedNotice.attachments &&
              selectedNotice.attachments.length > 0
            "
            class="border-t pt-4 mb-6"
          >
            <h3 class="text-sm font-semibold text-gray-900 mb-3">첨부파일</h3>
            <div class="space-y-2">
              <a
                v-for="file in selectedNotice.attachments"
                :key="file.id"
                :href="file.url"
                target="_blank"
                class="flex items-center space-x-2 p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
              >
                <svg
                  class="w-5 h-5 text-gray-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                  />
                </svg>
                <span class="text-sm text-gray-700 flex-1">{{
                  file.name
                }}</span>
                <span class="text-xs text-gray-500">{{ file.size }}</span>
              </a>
            </div>
          </div>

          <!-- 댓글 섹션 -->
          <div class="border-t pt-6">
            <h3 class="font-semibold text-gray-900 mb-4">
              댓글 {{ selectedNotice.comments?.length || 0 }}
            </h3>

            <!-- 댓글 목록 -->
            <div class="space-y-4 mb-6">
              <div
                v-for="comment in selectedNotice.comments"
                :key="comment.id"
                class="flex space-x-3"
              >
                <div
                  class="w-8 h-8 bg-gray-200 rounded-full flex-shrink-0"
                ></div>
                <div class="flex-1">
                  <div class="flex items-center space-x-2 mb-1">
                    <span class="text-sm font-semibold text-gray-900">{{
                      comment.authorName
                    }}</span>
                    <span class="text-xs text-gray-500">{{
                      formatDateTime(comment.createdAt)
                    }}</span>
                    <div
                      v-if="
                        comment.authorId === authStore.user?.id &&
                        !comment.isDeleted
                      "
                      class="flex-grow flex justify-end"
                    >
                      <button
                        @click.stop="deleteComment(comment.id)"
                        class="p-1 hover:bg-gray-100 rounded-full"
                      >
                        <svg
                          class="w-5 h-5 text-gray-500"
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            stroke-width="2"
                            d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                          ></path>
                        </svg>
                      </button>
                    </div>
                  </div>
                  <p class="text-sm text-gray-700">{{ comment.content }}</p>
                </div>
              </div>
            </div>

            <!-- 댓글 작성 -->
            <div class="flex space-x-3">
              <div
                class="w-8 h-8 bg-primary-200 rounded-full flex-shrink-0"
              ></div>
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
      </template>
    </SlideUpModal>

    <!-- 글쓰기 모달 (관리자용) -->
    <div
      v-if="showWriteModal"
      @click="closeWriteModal"
      class="fixed inset-0 bg-black/50 z-50 flex items-end sm:items-center justify-center p-0 sm:p-4"
    >
      <div
        @click.stop
        class="bg-white rounded-t-3xl sm:rounded-2xl w-full sm:max-w-2xl max-h-[90vh] overflow-y-auto"
      >
        <div
          class="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between"
        >
          <h2 class="text-lg font-bold text-gray-900">공지사항 작성</h2>
          <button
            @click="closeWriteModal"
            class="p-2 hover:bg-gray-100 rounded-lg"
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
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </div>

        <div class="p-6 space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2"
              >카테고리</label
            >
            <select
              v-model="newNotice.category"
              class="w-full px-3 py-2 border rounded-lg"
            >
              <option value="공지">공지</option>
              <option value="일정">일정</option>
              <option value="안내">안내</option>
            </select>
          </div>

          <div>
            <label class="flex items-center space-x-2">
              <input
                type="checkbox"
                v-model="newNotice.isPinned"
                class="rounded"
              />
              <span class="text-sm font-medium text-gray-700"
                >중요 공지로 등록</span
              >
            </label>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2"
              >제목</label
            >
            <input
              v-model="newNotice.title"
              type="text"
              placeholder="제목을 입력하세요"
              class="w-full px-3 py-2 border rounded-lg"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2"
              >내용</label
            >
            <!-- Quill 에디터 -->
            <div
              ref="editorRef"
              class="border rounded-lg"
              style="min-height: 300px"
            ></div>
          </div>

          <div class="flex justify-end space-x-3 pt-4">
            <button
              @click="closeWriteModal"
              class="px-4 py-2 border rounded-lg hover:bg-gray-50"
            >
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
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import { useNoticeNavigation } from '@/composables/useNoticeNavigation'
import apiClient from '@/services/api'
import { useQuillEditor } from '@/composables/useQuillEditor'
import QuillViewer from '@/components/common/QuillViewer.vue'
import DynamicActionRenderer from '@/dynamic-features/DynamicActionRenderer.vue'
import MainHeader from '@/components/common/MainHeader.vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'

const router = useRouter()
const route = useRoute()
const viewer = ref(null)
const authStore = useAuthStore()
const conventionStore = useConventionStore()
const { getPendingNotice } = useNoticeNavigation()
const isAdmin = computed(() => authStore.user?.role === 'Admin')

const selectedCategory = ref('all')
const selectedNotice = ref(null)
const showWriteModal = ref(false)
const newComment = ref('')
const allActions = ref([]) // 전체 동적 액션 저장

// BOARD_CONTENT_TOP 위치 액션 필터링
const contentTopActions = computed(() =>
  allActions.value.filter(
    (action) => action.targetLocation === 'BOARD_CONTENT_TOP',
  ),
)

const newNotice = ref({
  category: '공지',
  isPinned: false,
  title: '',
  content: '',
})

// Quill 에디터 초기화
const {
  editorRef,
  content: editorContent,
  setContent,
  getHTML,
} = useQuillEditor({
  placeholder: '내용을 입력하세요...',
  theme: 'snow',
})

const categories = ref([])

async function loadCategories() {
  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) return

    const response = await apiClient.get(
      `/conventions/${conventionId}/notice-categories`,
    )
    categories.value = [{ id: 'all', name: '전체' }, ...response.data]
  } catch (error) {
    console.error('Failed to load categories:', error)
    categories.value = [{ id: 'all', name: '전체' }]
  }
}

const notices = ref([])

const importantNotices = computed(() => notices.value.filter((n) => n.isPinned))

const filteredNotices = computed(() => {
  let filtered = notices.value.filter((n) => !n.isPinned)
  if (selectedCategory.value !== 'all') {
    filtered = filtered.filter(
      (n) => n.noticeCategoryId === selectedCategory.value,
    )
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

// 게시글 클릭 시 호출
async function openNotice(notice) {
  try {
    const response = await apiClient.get(`/notices/${notice.id}`)
    selectedNotice.value = response.data

    // 조회수 증가 API 호출
    apiClient.post(`/notices/${notice.id}/view`).catch((err) => {
      console.error('Failed to increment view count:', err)
    })

    // 댓글 불러오기
    await loadComments(notice.id)
  } catch (error) {
    console.error('Failed to open notice:', error)
  }
}

function closeNotice() {
  selectedNotice.value = null
  newComment.value = ''
}

async function loadComments(noticeId) {
  try {
    const response = await apiClient.get(`/notices/${noticeId}/comments`)
    if (selectedNotice.value) {
      selectedNotice.value.comments = response.data || []
    }
  } catch (error) {
    console.error('Failed to load comments:', error)
  }
}

async function submitComment() {
  if (!newComment.value.trim() || !selectedNotice.value) return

  try {
    const response = await apiClient.post(
      `/notices/${selectedNotice.value.id}/comments`,
      {
        content: newComment.value,
      },
    )

    // 댓글 목록에 추가
    if (!selectedNotice.value.comments) {
      selectedNotice.value.comments = []
    }
    selectedNotice.value.comments.push(response.data)
    newComment.value = ''
  } catch (error) {
    console.error('Failed to submit comment:', error)
    alert('댓글 작성에 실패했습니다.')
  }
}

async function deleteComment(commentId) {
  if (!confirm('댓글을 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/notices/comments/${commentId}`)
    const noticeId = selectedNotice.value.id
    await loadComments(noticeId)
  } catch (error) {
    console.error('Failed to delete comment:', error)
    alert('댓글 삭제에 실패했습니다.')
  }
}

function closeWriteModal() {
  showWriteModal.value = false
  newNotice.value = {
    category: '공지',
    isPinned: false,
    title: '',
    content: '',
  }
  // Quill 에디터 내용 초기화
  setContent('')
}

async function submitNotice() {
  // Quill 에디터에서 HTML 내용 가져오기
  const htmlContent = getHTML()

  if (!newNotice.value.title || !htmlContent || htmlContent === '<p><br></p>') {
    alert('제목과 내용을 모두 입력해주세요.')
    return
  }

  try {
    const conventionId = conventionStore.currentConvention?.id

    // Quill 에디터 내용을 newNotice에 할당
    await apiClient.post(
      '/notices',
      {
        ...newNotice.value,
        content: htmlContent,
      },
      {
        params: { conventionId },
      },
    )

    await loadNotices()
    closeWriteModal()
  } catch (error) {
    console.error('Failed to create notice:', error)
    alert('공지사항 등록에 실패했습니다.')
  }
}

async function loadNotices() {
  try {
    const conventionId = conventionStore.currentConvention?.id

    console.log('Board.vue - ConventionId:', conventionId)

    if (!conventionId) {
      console.error('ConventionId가 없습니다. 다시 로그인해주세요.')
      return
    }

    const response = await apiClient.get('/notices', {
      params: {
        conventionId: conventionId,
        page: 1,
        pageSize: 100,
      },
    })

    notices.value = (response.data.items || []).map((item) => ({
      id: item.id,
      category: item.categoryName,
      noticeCategoryId: item.noticeCategoryId,
      title: item.title,
      content: item.content,
      author: item.authorName || '관리자',
      createdAt: item.createdAt,
      views: item.viewCount || 0,
      commentCount: 0,
      isPinned: item.isPinned,
      comments: [], // 초기화
      attachments: [],
    }))
  } catch (error) {
    console.error('Failed to load notices:', error)
  }
}

const categoryContainer = ref(null)
const showLeftScroll = ref(false)
const showRightScroll = ref(false)

const handleScroll = () => {
  if (categoryContainer.value) {
    showLeftScroll.value = categoryContainer.value.scrollLeft > 0
    showRightScroll.value =
      categoryContainer.value.scrollWidth - categoryContainer.value.clientWidth
  }
}

const scrollLeft = () => {
  categoryContainer.value?.scrollBy({ left: -200, behavior: 'smooth' })
}

const scrollRight = () => {
  categoryContainer.value?.scrollBy({ left: 200, behavior: 'smooth' })
}

watch(categories, () => {
  setTimeout(() => {
    handleScroll()
  }, 100)
})

const openImageViewer = (src) => {
  const viewerInstance = viewer.value?.$viewer
  if (!viewerInstance) return

  const images = viewer.value.querySelectorAll('img')
  const urls = Array.from(images).map((img) => img.src)
  const index = urls.findIndex((url) => url === src)

  if (index !== -1) {
    viewerInstance.view(index)
  }
}

async function loadDynamicActions() {
  try {
    const conventionId = conventionStore.currentConvention?.id

    if (!conventionId) return

    const response = await apiClient.get(
      `/conventions/${conventionId}/actions/all`,
      {
        params: {
          targetLocation: 'BOARD_CONTENT_TOP',
          isActive: true,
        },
      },
    )

    allActions.value = response.data || []
  } catch (error) {
    console.error('Failed to load dynamic actions:', error)
    allActions.value = []
  }
}

onMounted(async () => {
  try {
    // 1. Ensure stores are ready
    if (!authStore.user) {
      await authStore.fetchCurrentUser()
    }
    if (!conventionStore.currentConvention) {
      const selectedConventionId = localStorage.getItem('selectedConventionId')
      if (selectedConventionId) {
        await conventionStore.setCurrentConvention(
          parseInt(selectedConventionId),
        )
      }
    }

    // 2. Load data
    await loadCategories()
    await loadNotices()

    await loadDynamicActions()

    // ConventionHome에서 composable로 전달된 noticeId 확인 (TripDetail 패턴)
    const pendingNoticeId = getPendingNotice()
    if (pendingNoticeId) {
      const notice = notices.value.find((n) => n.id === pendingNoticeId)
      if (notice) {
        await openNotice(notice)
      }
    }
  } catch (error) {
    console.error('Failed to initialize Board:', error)
  }
})

onUnmounted(() => {
})
</script>
<style>
.no-scrollbar::-webkit-scrollbar {
  display: none;
}
.no-scrollbar {
  -ms-overflow-style: none; /* IE and Edge */
  scrollbar-width: none; /* Firefox */
}

.modal-slide-enter-active,
.modal-slide-leave-active {
  transition: all 0.3s ease-in-out;
}
.modal-slide-enter-active .bg-white,
.modal-slide-leave-active .bg-white {
  transition: all 0.3s ease-in-out;
}

.modal-slide-enter-from,
.modal-slide-leave-to {
  background-color: rgba(0, 0, 0, 0);
}

@media (max-width: 639px) {
  .modal-slide-enter-from .bg-white,
  .modal-slide-leave-to .bg-white {
    transform: translateY(100%);
  }
}

@media (min-width: 640px) {
  .modal-slide-enter-from .bg-white,
  .modal-slide-leave-to .bg-white {
    transform: scale(0.95);
    opacity: 0;
  }
}
</style>