<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 로딩 상태 -->
    <div v-if="loading" class="flex items-center justify-center min-h-screen">
      <div class="text-center">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600">로딩 중...</p>
      </div>
    </div>

    <!-- 컨텐츠 -->
    <div v-else class="max-w-5xl mx-auto px-4 py-8">
      <!-- 상단 버튼 -->
      <div class="mb-6">
        <button
          @click="goBack"
          class="flex items-center gap-2 text-gray-600 hover:text-gray-900 transition-colors"
        >
          <span>←</span>
          <span>목록으로</span>
        </button>
      </div>

      <!-- 공지사항 카드 -->
      <div class="bg-white rounded-lg shadow-sm overflow-hidden">
        <!-- 헤더 -->
        <div class="border-b bg-gray-50 px-8 py-6">
          <div class="flex items-start gap-3 mb-4">
            <span
              v-if="notice.isPinned"
              class="px-3 py-1 bg-red-100 text-red-800 text-sm font-semibold rounded-full"
            >
              📌 공지
            </span>
            <h1 class="text-2xl font-bold text-gray-900 flex-1">{{ notice.title }}</h1>
          </div>

          <div class="flex items-center justify-between text-sm text-gray-600">
            <div class="flex items-center gap-6">
              <span class="flex items-center gap-2">
                <span class="font-semibold">작성자:</span>
                <span>{{ notice.authorName }}</span>
              </span>
              <span class="flex items-center gap-2">
                <span class="font-semibold">작성일:</span>
                <span>{{ formatDate(notice.createdAt) }}</span>
              </span>
              <span class="flex items-center gap-2">
                <span class="font-semibold">조회:</span>
                <span>{{ notice.viewCount }}</span>
              </span>
            </div>
          </div>
        </div>

        <!-- 본문 내용 (Quill 에디터 읽기 전용) -->
        <div class="px-8 py-8">
          <div 
            class="prose max-w-none ql-editor-readonly"
            v-html="notice.content"
          ></div>
        </div>

        <!-- 첨부파일 -->
        <div v-if="notice.attachments && notice.attachments.length > 0" class="border-t px-8 py-6 bg-gray-50">
          <h3 class="text-lg font-semibold mb-4 text-gray-900">첨부파일</h3>
          <div class="space-y-2">
            <a
              v-for="file in notice.attachments"
              :key="file.id"
              :href="file.url"
              :download="file.originalName"
              class="flex items-center gap-3 p-3 bg-white rounded-lg border hover:border-blue-500 hover:bg-blue-50 transition-colors"
            >
              <span class="text-2xl">📎</span>
              <div class="flex-1">
                <p class="font-medium text-gray-900">{{ file.originalName }}</p>
                <p class="text-sm text-gray-500">{{ formatFileSize(file.size) }}</p>
              </div>
              <span class="text-blue-600 text-sm font-medium">다운로드</span>
            </a>
          </div>
        </div>
      </div>

      <!-- 이전글/다음글 네비게이션 -->
      <div class="mt-6 bg-white rounded-lg shadow-sm overflow-hidden divide-y">
        <div
          v-if="navigation.prev"
          @click="goToNotice(navigation.prev.id)"
          class="px-8 py-4 hover:bg-gray-50 cursor-pointer transition-colors"
        >
          <div class="flex items-center gap-4">
            <span class="text-gray-500 font-medium w-16">이전글</span>
            <span class="text-gray-900">{{ navigation.prev.title }}</span>
          </div>
        </div>
        <div
          v-if="navigation.next"
          @click="goToNotice(navigation.next.id)"
          class="px-8 py-4 hover:bg-gray-50 cursor-pointer transition-colors"
        >
          <div class="flex items-center gap-4">
            <span class="text-gray-500 font-medium w-16">다음글</span>
            <span class="text-gray-900">{{ navigation.next.title }}</span>
          </div>
        </div>
      </div>

      <!-- 하단 버튼 -->
      <div class="mt-6 flex justify-center">
        <button
          @click="goBack"
          class="px-8 py-3 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
        >
          목록으로
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { noticeAPI } from '@/services/noticeService'
import { formatFileSize } from '@/utils/fileUpload'
import dayjs from 'dayjs'

// Quill 스타일 import (읽기 전용)
import 'quill/dist/quill.snow.css'

export default {
  name: 'NoticeDetail',
  setup() {
    const router = useRouter()
    const route = useRoute()
    
    // 상태
    const loading = ref(false)
    const notice = ref({
      id: null,
      title: '',
      content: '',
      authorName: '',
      createdAt: '',
      viewCount: 0,
      isPinned: false,
      attachments: []
    })
    const navigation = ref({
      prev: null,
      next: null
    })

    // 메서드
    const fetchNotice = async () => {
      loading.value = true
      try {
        const id = route.params.id
        
        // 조회수 증가
        await noticeAPI.incrementViewCount(id)
        
        // 공지사항 상세 조회
        const response = await noticeAPI.getNotice(id)
        notice.value = response.data
        
        // 이전글/다음글 정보가 있다면 설정
        if (response.data.navigation) {
          navigation.value = response.data.navigation
        }
      } catch (error) {
        console.error('Failed to fetch notice:', error)
        alert('공지사항을 불러오는데 실패했습니다.')
        goBack()
      } finally {
        loading.value = false
      }
    }

    const goBack = () => {
      router.push('/notices')
    }

    const goToNotice = (id) => {
      router.push(`/notices/${id}`)
    }

    const formatDate = (dateString) => {
      return dayjs(dateString).format('YYYY년 MM월 DD일 HH:mm')
    }

    // 생명주기
    onMounted(() => {
      fetchNotice()
    })

    return {
      loading,
      notice,
      navigation,
      goBack,
      goToNotice,
      formatDate,
      formatFileSize
    }
  }
}
</script>

<style scoped>
/* Quill 에디터 읽기 전용 스타일 */
.ql-editor-readonly {
  padding: 0;
  line-height: 1.8;
  font-size: 16px;
}

.ql-editor-readonly :deep(h1) {
  font-size: 2em;
  font-weight: bold;
  margin-top: 1em;
  margin-bottom: 0.5em;
}

.ql-editor-readonly :deep(h2) {
  font-size: 1.5em;
  font-weight: bold;
  margin-top: 1em;
  margin-bottom: 0.5em;
}

.ql-editor-readonly :deep(h3) {
  font-size: 1.25em;
  font-weight: bold;
  margin-top: 1em;
  margin-bottom: 0.5em;
}

.ql-editor-readonly :deep(p) {
  margin-bottom: 1em;
}

.ql-editor-readonly :deep(ul),
.ql-editor-readonly :deep(ol) {
  margin-left: 1.5em;
  margin-bottom: 1em;
}

.ql-editor-readonly :deep(img) {
  max-width: 100%;
  height: auto;
  margin: 1em 0;
}

.ql-editor-readonly :deep(blockquote) {
  border-left: 4px solid #ccc;
  padding-left: 16px;
  margin: 1em 0;
  color: #666;
}

.ql-editor-readonly :deep(pre) {
  background-color: #f4f4f4;
  padding: 1em;
  border-radius: 4px;
  overflow-x: auto;
}

.ql-editor-readonly :deep(code) {
  background-color: #f4f4f4;
  padding: 2px 4px;
  border-radius: 3px;
  font-family: monospace;
}

.ql-editor-readonly :deep(a) {
  color: #3b82f6;
  text-decoration: underline;
}

.ql-editor-readonly :deep(a:hover) {
  color: #2563eb;
}
</style>
