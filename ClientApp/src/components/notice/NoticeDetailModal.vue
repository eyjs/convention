<template>
  <BaseModal :is-open="true" @close="closeModal" max-width="5xl">
    <template #header>
      <h2 class="text-lg md:text-xl font-bold text-gray-900">
        공지사항 상세보기
      </h2>
    </template>
    <template #body>
      <!-- 로딩 상태 -->
      <div v-if="loading" class="flex-1 flex items-center justify-center py-12">
        <div class="text-center">
          <div
            class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"
          ></div>
          <p class="mt-4 text-gray-600">로딩 중...</p>
        </div>
      </div>

      <!-- 본문 -->
      <div v-else class="flex-1 overflow-y-auto">
        <!-- 제목 및 메타 정보 -->
        <div class="border-b bg-gray-50 px-4 md:px-6 py-4 md:py-5">
          <div class="flex items-start gap-2 md:gap-3 mb-3">
            <span
              v-if="notice.isPinned"
              class="px-2 md:px-3 py-1 bg-red-100 text-red-800 text-xs md:text-sm font-semibold rounded-full flex-shrink-0"
            >
              📌 공지
            </span>
            <h1 class="text-xl md:text-2xl font-bold text-gray-900 flex-1">
              {{ notice.title }}
            </h1>
          </div>

          <div
            class="flex flex-wrap items-center gap-2 md:gap-4 text-xs md:text-sm text-gray-600"
          >
            <span class="flex items-center gap-1 md:gap-2">
              <span class="font-semibold">카테고리:</span>
              <span>{{ notice.categoryName }}</span>
            </span>
            <span class="flex items-center gap-1 md:gap-2">
              <span class="font-semibold">작성자:</span>
              <span>{{ notice.authorName }}</span>
            </span>
            <span class="flex items-center gap-1 md:gap-2">
              <span class="font-semibold">작성일:</span>
              <span>{{ formatDate(notice.createdAt) }}</span>
            </span>
            <span class="flex items-center gap-1 md:gap-2">
              <span class="font-semibold">조회:</span>
              <span>{{ notice.viewCount }}</span>
            </span>
          </div>
        </div>

        <!-- 본문 내용 -->
        <div class="px-4 md:px-6 py-6">
          <div
            v-if="notice.content"
            class="prose prose-sm md:prose-base max-w-none"
            v-html="notice.content"
          ></div>
          <div v-else class="text-gray-500 italic">본문 내용이 없습니다.</div>
        </div>

        <!-- 첨부파일 -->
        <div
          v-if="notice.attachments && notice.attachments.length > 0"
          class="border-t px-4 md:px-6 py-4 md:py-5 bg-gray-50"
        >
          <h3 class="text-base md:text-lg font-semibold mb-3 text-gray-900">
            첨부파일
          </h3>
          <div class="space-y-2">
            <a
              v-for="file in notice.attachments"
              :key="file.id"
              :href="file.url"
              :download="file.originalName"
              class="flex items-center gap-2 md:gap-3 p-2 md:p-3 bg-white rounded-lg border hover:border-blue-500 hover:bg-blue-50 transition-colors"
            >
              <span class="text-xl md:text-2xl">📎</span>
              <div class="flex-1 min-w-0">
                <p
                  class="font-medium text-gray-900 truncate text-sm md:text-base"
                >
                  {{ file.originalName }}
                </p>
                <p class="text-xs md:text-sm text-gray-500">
                  {{ formatFileSize(file.size) }}
                </p>
              </div>
              <span
                class="text-blue-600 text-xs md:text-sm font-medium flex-shrink-0"
                >다운로드</span
              >
            </a>
          </div>
        </div>
      </div>
    </template>
    <template #footer>
      <button
        type="button"
        @click="closeModal"
        class="px-4 md:px-6 py-2 text-sm md:text-base bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
      >
        닫기
      </button>
    </template>
  </BaseModal>
</template>

<script>
import { ref, onMounted } from 'vue'
import { noticeAPI } from '@/services/noticeService'
import { formatFileSize } from '@/utils/fileUpload'
import dayjs from 'dayjs'
import BaseModal from '@/components/common/BaseModal.vue'

// Quill 스타일 import (읽기 전용)
import 'quill/dist/quill.snow.css'

export default {
  name: 'NoticeDetailModal',
  components: {
    BaseModal,
  },
  props: {
    noticeId: {
      type: Number,
      required: true,
    },
  },
  emits: ['close'],
  setup(props, { emit }) {
    // 상태
    const loading = ref(false)
    const notice = ref({
      id: null,
      title: '',
      content: '',
      authorName: '',
      categoryName: '',
      createdAt: '',
      viewCount: 0,
      isPinned: false,
      attachments: [],
    })

    // 메서드
    const closeModal = () => {
      emit('close')
    }

    const fetchNotice = async () => {
      console.log('[NoticeDetailModal] fetchNotice 호출됨')
      console.log('[NoticeDetailModal] props.noticeId:', props.noticeId)

      if (!props.noticeId) {
        console.error('[NoticeDetailModal] noticeId가 없습니다!')
        alert('게시글 ID가 없습니다.')
        closeModal()
        return
      }

      loading.value = true
      try {
        console.log('[NoticeDetailModal] 조회수 증가 API 호출 시작')
        // 조회수 증가
        await noticeAPI.incrementViewCount(props.noticeId)

        console.log('[NoticeDetailModal] 상세 조회 API 호출 시작')
        // 공지사항 상세 조회
        const response = await noticeAPI.getNotice(props.noticeId)
        console.log('[NoticeDetailModal] API Response:', response)
        console.log('[NoticeDetailModal] API Response Data:', response.data)
        notice.value = response.data
        console.log('[NoticeDetailModal] Notice Value:', notice.value)
        console.log('[NoticeDetailModal] Content:', notice.value.content)
      } catch (error) {
        console.error('[NoticeDetailModal] Failed to fetch notice:', error)
        alert('공지사항을 불러오는데 실패했습니다.')
        closeModal()
      } finally {
        loading.value = false
      }
    }

    const formatDate = (dateString) => {
      return dayjs(dateString).format('YYYY년 MM월 DD일 HH:mm')
    }

    // 생명주기
    onMounted(() => {
      console.log('[NoticeDetailModal] onMounted 호출됨')
      console.log('[NoticeDetailModal] props:', props)
      fetchNotice()
    })

    return {
      noticeId: props.noticeId,
      loading,
      notice,
      closeModal,
      formatDate,
      formatFileSize,
    }
  },
}
</script>
