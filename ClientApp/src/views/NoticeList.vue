<template>
  <div class="min-h-screen min-h-dvh bg-gray-50">
    <!-- 헤더 -->
    <div class="bg-white border-b">
      <div class="max-w-7xl mx-auto px-4 py-6">
        <h1 class="text-3xl font-bold text-gray-900">공지사항</h1>
        <p class="mt-2 text-gray-600">행사에 관한 중요한 공지사항을 확인하세요</p>
      </div>
    </div>

    <!-- 메인 컨텐츠 -->
    <div class="max-w-7xl mx-auto px-4 py-8">
      <!-- 검색 영역 -->
      <div class="bg-white rounded-lg shadow-sm p-6 mb-6">
        <div class="flex gap-4">
          <select
            v-model="searchType"
            class="px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="title">제목</option>
            <option value="content">내용</option>
            <option value="all">제목+내용</option>
          </select>

          <input
            v-model="searchKeyword"
            type="text"
            placeholder="검색어를 입력하세요"
            class="flex-1 px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            @keyup.enter="handleSearch"
          />

          <button
            @click="handleSearch"
            class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            검색
          </button>
        </div>
      </div>

      <!-- 새 글 작성 버튼 -->
      <div v-if="authStore.isAuthenticated" class="mb-6 flex justify-end">
        <button
          @click="openCreateModal"
          class="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
        >
          <span>+</span>
          <span>새 글 작성</span>
        </button>
      </div>

      <!-- 로딩 상태 -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        <p class="mt-4 text-gray-600">로딩 중...</p>
      </div>

      <!-- 공지사항 목록 -->
      <div v-else class="bg-white rounded-lg shadow-sm overflow-hidden">
        <!-- 테이블 헤더 -->
        <div class="bg-gray-50 border-b px-6 py-4">
          <div class="grid grid-cols-12 gap-4 font-semibold text-gray-700">
            <div class="col-span-1 text-center">번호</div>
            <div class="col-span-6">제목</div>
            <div class="col-span-2 text-center">작성자</div>
            <div class="col-span-2 text-center">작성일</div>
            <div class="col-span-1 text-center">조회</div>
          </div>
        </div>

        <!-- 공지사항 리스트 -->
        <div v-if="notices.length > 0">
          <!-- 고정 공지사항 -->
          <div
            v-for="notice in pinnedNotices"
            :key="`pinned-${notice.id}`"
            @click="openDetailModal(notice.id)"
            class="border-b px-6 py-4 hover:bg-blue-50 cursor-pointer transition-colors bg-yellow-50"
          >
            <div class="grid grid-cols-12 gap-4 items-center">
              <div class="col-span-1 text-center">
                <span class="inline-flex items-center px-2 py-1 rounded-full text-xs font-semibold bg-red-100 text-red-800">
                  📌 공지
                </span>
              </div>
              <div class="col-span-6">
                <div class="flex items-center gap-2">
                  <span class="font-semibold text-gray-900">{{ notice.title }}</span>
                  <span v-if="isNew(notice.createdAt)" class="px-2 py-0.5 bg-red-500 text-white text-xs rounded">NEW</span>
                </div>
              </div>
              <div class="col-span-2 text-center text-gray-600">{{ notice.authorName }}</div>
              <div class="col-span-2 text-center text-gray-600">{{ formatDate(notice.createdAt) }}</div>
              <div class="col-span-1 text-center text-gray-600">{{ notice.viewCount }}</div>
            </div>
          </div>

          <!-- 일반 공지사항 -->
          <div
            v-for="notice in regularNotices"
            :key="notice.id"
            @click="openDetailModal(notice.id)"
            class="border-b px-6 py-4 hover:bg-gray-50 cursor-pointer transition-colors"
          >
            <div class="grid grid-cols-12 gap-4 items-center">
              <div class="col-span-1 text-center text-gray-600">{{ notice.displayNumber }}</div>
              <div class="col-span-6">
                <div class="flex items-center gap-2">
                  <span class="text-gray-900">{{ notice.title }}</span>
                  <span v-if="isNew(notice.createdAt)" class="px-2 py-0.5 bg-red-500 text-white text-xs rounded">NEW</span>
                  <span v-if="notice.hasAttachment" class="text-gray-400">📎</span>
                </div>
              </div>
              <div class="col-span-2 text-center text-gray-600">{{ notice.authorName }}</div>
              <div class="col-span-2 text-center text-gray-600">{{ formatDate(notice.createdAt) }}</div>
              <div class="col-span-1 text-center text-gray-600">{{ notice.viewCount }}</div>
            </div>
          </div>
        </div>

        <!-- 데이터 없음 -->
        <div v-else class="px-6 py-12 text-center text-gray-500">
          <p class="text-lg">등록된 공지사항이 없습니다.</p>
        </div>
      </div>

      <!-- 페이지네이션 -->
      <div v-if="totalPages > 1" class="mt-6 flex justify-center">
        <nav class="flex items-center gap-2">
          <button
            @click="goToPage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="px-3 py-2 rounded-lg border hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            이전
          </button>

          <button
            v-for="page in visiblePages"
            :key="page"
            @click="goToPage(page)"
            :class="[
              'px-4 py-2 rounded-lg border',
              currentPage === page
                ? 'bg-blue-600 text-white border-blue-600'
                : 'hover:bg-gray-50'
            ]"
          >
            {{ page }}
          </button>

          <button
            @click="goToPage(currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="px-3 py-2 rounded-lg border hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            다음
          </button>
        </nav>
      </div>
    </div>

    <!-- 공지사항 작성/수정 모달 -->
    <NoticeFormModal
      v-if="showModal"
      :notice="selectedNotice"
      :categories="categories"
      :default-category-id="categories[0]?.id"
      :convention-id="conventionStore.currentConvention?.id || 0"
      @close="closeModal"
      @saved="handleSaved"
    />

    <!-- 공지사항 상세보기 모달 -->
    <NoticeDetailModal
      v-if="showDetailModal"
      :notice-id="selectedNoticeId"
      @close="closeDetailModal"
    />
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import { noticeAPI } from '@/services/noticeService'
import { categoryAPI } from '@/services/categoryService'
import { useConventionStore } from '@/stores/convention'
import { useAuthStore } from '@/stores/auth'
import NoticeFormModal from '@/components/notice/NoticeFormModal.vue'
import NoticeDetailModal from '@/components/notice/NoticeDetailModal.vue'
import dayjs from 'dayjs'

export default {
  name: 'NoticeList',
  components: {
    NoticeFormModal,
    NoticeDetailModal
  },
  setup() {
    const conventionStore = useConventionStore()
    const authStore = useAuthStore()
    
    // 상태
    const loading = ref(false)
    const notices = ref([])
    const currentPage = ref(1)
    const pageSize = ref(20)
    const totalCount = ref(0)
    const searchType = ref('title')
    const searchKeyword = ref('')
    const showModal = ref(false)
    const selectedNotice = ref(null)
    const showDetailModal = ref(false)
    const selectedNoticeId = ref(null)
    const categories = ref([])

    // 계산된 속성
    const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))
    
    const pinnedNotices = computed(() => notices.value.filter(n => n.isPinned))
    const regularNotices = computed(() => notices.value.filter(n => !n.isPinned))
    
    const visiblePages = computed(() => {
      const pages = []
      const maxVisible = 5
      let start = Math.max(1, currentPage.value - Math.floor(maxVisible / 2))
      let end = Math.min(totalPages.value, start + maxVisible - 1)
      
      if (end - start + 1 < maxVisible) {
        start = Math.max(1, end - maxVisible + 1)
      }
      
      for (let i = start; i <= end; i++) {
        pages.push(i)
      }
      
      return pages
    })

    // 메서드
    const fetchCategories = async () => {
      try {
        const conventionId = conventionStore.currentConvention?.id
        if (!conventionId) return

        const response = await categoryAPI.getNoticeCategories(conventionId)
        categories.value = response.data
      } catch (error) {
        console.error('Failed to fetch categories:', error)
      }
    }

    const fetchNotices = async () => {
      loading.value = true
      try {
        const conventionId = conventionStore.currentConvention?.id
        if (!conventionId) {
          alert('현재 행사 정보를 찾을 수 없습니다. 홈으로 돌아가 다시 시도해주세요.')
          loading.value = false
          return
        }

        const response = await noticeAPI.getNotices({
          conventionId: conventionId,
          page: currentPage.value,
          pageSize: pageSize.value,
          searchType: searchKeyword.value ? searchType.value : undefined,
          searchKeyword: searchKeyword.value || undefined
        })

        notices.value = response.data.items.map((item, index) => ({
          ...item,
          displayNumber: totalCount.value - ((currentPage.value - 1) * pageSize.value) - index
        }))
        totalCount.value = response.data.totalCount
      } catch (error) {
        console.error('Failed to fetch notices:', error)
        alert('공지사항을 불러오는데 실패했습니다.')
      } finally {
        loading.value = false
      }
    }

    const handleSearch = () => {
      currentPage.value = 1
      fetchNotices()
    }

    const goToPage = (page) => {
      if (page < 1 || page > totalPages.value) return
      currentPage.value = page
      fetchNotices()
      window.scrollTo({ top: 0, behavior: 'smooth' })
    }

    const openDetailModal = (id) => {
      selectedNoticeId.value = id
      showDetailModal.value = true
    }

    const closeDetailModal = () => {
      showDetailModal.value = false
      selectedNoticeId.value = null
      // 모달 닫을 때 목록 새로고침 (조회수 업데이트 반영)
      fetchNotices()
    }

    const formatDate = (dateString) => {
      return dayjs(dateString).format('YYYY.MM.DD')
    }

    const isNew = (dateString) => {
      const daysDiff = dayjs().diff(dayjs(dateString), 'day')
      return daysDiff <= 3
    }

    const openCreateModal = () => {
      selectedNotice.value = null
      showModal.value = true
    }

    const closeModal = () => {
      showModal.value = false
      selectedNotice.value = null
    }

    const handleSaved = () => {
      closeModal()
      fetchNotices()
    }

    // 생명주기
    onMounted(() => {
      fetchCategories()
      fetchNotices()
    })

    return {
      authStore,
      conventionStore,
      loading,
      notices,
      currentPage,
      pageSize,
      totalCount,
      searchType,
      searchKeyword,
      showModal,
      selectedNotice,
      showDetailModal,
      selectedNoticeId,
      categories,
      totalPages,
      pinnedNotices,
      regularNotices,
      visiblePages,
      fetchNotices,
      handleSearch,
      goToPage,
      openDetailModal,
      closeDetailModal,
      formatDate,
      isNew,
      openCreateModal,
      closeModal,
      handleSaved
    }
  }
}
</script>
