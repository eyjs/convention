<template>
  <div class="min-h-screen min-h-dvh bg-gray-50 p-6">
    <div class="max-w-7xl mx-auto">
      <!-- 헤더 -->
      <div class="mb-6 flex items-center justify-between">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">공지사항 관리</h1>
          <p class="mt-2 text-gray-600">
            공지사항을 등록하고 관리할 수 있습니다
          </p>
        </div>
        <button
          class="px-6 py-3 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300 transition-colors flex items-center gap-2"
          @click="showCategoryModal = true"
        >
          <span>카테고리 관리</span>
        </button>
        <button
          class="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
          @click="openCreateModal"
        >
          <span>+</span>
          <span>새 공지사항</span>
        </button>
      </div>

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
            class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            @click="handleSearch"
          >
            검색
          </button>
        </div>
      </div>

      <!-- 로딩 상태 -->
      <div v-if="loading" class="text-center py-12">
        <div
          class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"
        ></div>
        <p class="mt-4 text-gray-600">로딩 중...</p>
      </div>

      <!-- 공지사항 테이블 -->
      <div v-else class="bg-white rounded-lg shadow-sm overflow-hidden">
        <table class="w-full">
          <thead class="bg-gray-50 border-b">
            <tr>
              <th
                class="px-6 py-4 text-left text-sm font-semibold text-gray-700 w-20"
              >
                번호
              </th>
              <th
                class="px-6 py-4 text-left text-sm font-semibold text-gray-700 w-24"
              >
                고정
              </th>
              <th
                class="px-6 py-4 text-left text-sm font-semibold text-gray-700 w-40"
              >
                카테고리
              </th>
              <th
                class="px-6 py-4 text-left text-sm font-semibold text-gray-700"
              >
                제목
              </th>
              <th
                class="px-6 py-4 text-center text-sm font-semibold text-gray-700 w-32"
              >
                작성자
              </th>
              <th
                class="px-6 py-4 text-center text-sm font-semibold text-gray-700 w-40"
              >
                작성일
              </th>
              <th
                class="px-6 py-4 text-center text-sm font-semibold text-gray-700 w-24"
              >
                조회
              </th>
              <th
                class="px-6 py-4 text-center text-sm font-semibold text-gray-700 w-40"
              >
                관리
              </th>
            </tr>
          </thead>
          <tbody class="divide-y">
            <tr
              v-for="notice in notices"
              :key="notice.id"
              :class="[
                'hover:bg-gray-50',
                'transition-colors',
                { 'bg-yellow-50': notice.isPinned },
              ]"
            >
              <td class="px-6 py-4 text-sm text-gray-900">
                {{ notice.displayNumber }}
              </td>
              <td class="px-6 py-4">
                <button
                  :class="[
                    'flex',
                    'items-center',
                    'gap-1',
                    'px-3',
                    'py-1',
                    'rounded-full',
                    'text-xs',
                    'font-semibold',
                    'transition-colors',
                    notice.isPinned
                      ? 'bg-red-100 text-red-800 hover:bg-red-200'
                      : 'bg-gray-100 text-gray-600 hover:bg-gray-200',
                  ]"
                  @click="togglePin(notice.id)"
                >
                  <span>{{ notice.isPinned ? '📌' : '📍' }}</span>
                  <span>{{ notice.isPinned ? '고정됨' : '고정' }}</span>
                </button>
              </td>
              <td class="px-6 py-4 text-sm text-gray-900">
                {{ notice.categoryName }}
              </td>
              <td class="px-6 py-4">
                <div
                  class="flex items-center gap-2 cursor-pointer"
                  style="background-color: rgba(255, 0, 0, 0.1)"
                  @click="
                    () => {
                      alert('클릭됨!')
                      openDetailModal(notice.id)
                    }
                  "
                >
                  <span
                    class="text-sm text-gray-900 font-medium hover:text-blue-600 transition-colors"
                    >{{ notice.title }}</span
                  >
                  <span v-if="notice.hasAttachment" class="text-gray-400"
                    >📎</span
                  >
                </div>
              </td>
              <td class="px-6 py-4 text-center text-sm text-gray-600">
                {{ notice.authorName }}
              </td>
              <td class="px-6 py-4 text-center text-sm text-gray-600">
                {{ formatDate(notice.createdAt) }}
              </td>
              <td class="px-6 py-4 text-center text-sm text-gray-600">
                {{ notice.viewCount }}
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center justify-center gap-2">
                  <button
                    class="px-3 py-1 bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition-colors text-sm"
                    @click="openEditModal(notice)"
                  >
                    수정
                  </button>
                  <button
                    class="px-3 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200 transition-colors text-sm"
                    @click="confirmDelete(notice.id)"
                  >
                    삭제
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- 데이터 없음 -->
        <div
          v-if="notices.length === 0"
          class="px-6 py-12 text-center text-gray-500"
        >
          <p class="text-lg">등록된 공지사항이 없습니다.</p>
        </div>
      </div>

      <!-- 페이지네이션 -->
      <div v-if="totalPages > 1" class="mt-6 flex justify-center">
        <nav class="flex items-center gap-2">
          <button
            :disabled="currentPage === 1 ? true : null"
            class="px-3 py-2 rounded-lg border hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            @click="goToPage(currentPage - 1)"
          >
            이전
          </button>

          <button
            v-for="page in visiblePages"
            :key="page"
            :class="[
              'px-4 py-2 rounded-lg border',
              currentPage === page
                ? 'bg-blue-600 text-white border-blue-600'
                : 'hover:bg-gray-50',
            ]"
            @click="goToPage(page)"
          >
            {{ page }}
          </button>

          <button
            :disabled="currentPage === totalPages ? true : null"
            class="px-3 py-2 rounded-lg border hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            @click="goToPage(currentPage + 1)"
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
      :convention-id="conventionId"
      @close="closeModal"
      @saved="handleSaved"
    />

    <CategoryManagementModal
      v-if="showCategoryModal"
      @close="showCategoryModal = false"
      @categories-updated="fetchNotices"
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
import NoticeFormModal from '@/components/notice/NoticeFormModal.vue'
import NoticeDetailModal from '@/components/notice/NoticeDetailModal.vue'
import CategoryManagementModal from '@/components/admin/CategoryManagementModal.vue'
import dayjs from 'dayjs'

export default {
  name: 'AdminNoticeManagement',
  components: {
    NoticeFormModal,
    NoticeDetailModal,
    CategoryManagementModal,
  },
  setup() {
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
    const showCategoryModal = ref(false)
    const categories = ref([])
    const conventionId = ref(1) // TODO: get from store or props

    // 계산된 속성
    const totalPages = computed(() =>
      Math.ceil(totalCount.value / pageSize.value),
    )

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

    const isPrevButtonDisabled = computed(() => currentPage.value === 1)
    const isNextButtonDisabled = computed(
      () => currentPage.value === totalPages.value,
    )

    // 메서드
    const fetchCategories = async () => {
      try {
        const response = await categoryAPI.getNoticeCategories(
          conventionId.value,
        )
        categories.value = response.data
      } catch (error) {
        console.error('Failed to fetch categories:', error)
        alert('카테고리를 불러오는데 실패했습니다.')
      }
    }

    const fetchNotices = async () => {
      loading.value = true
      try {
        const response = await noticeAPI.getNotices({
          conventionId: conventionId.value,
          page: currentPage.value,
          pageSize: pageSize.value,
          searchType: searchKeyword.value ? searchType.value : undefined,
          searchKeyword: searchKeyword.value || undefined,
        })

        notices.value = response.data.items.map((item, index) => ({
          ...item,
          displayNumber:
            totalCount.value - (currentPage.value - 1) * pageSize.value - index,
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

    const openCreateModal = () => {
      selectedNotice.value = null
      showModal.value = true
    }

    const openEditModal = (notice) => {
      selectedNotice.value = notice
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

    const togglePin = async (id) => {
      try {
        await noticeAPI.togglePin(id)
        fetchNotices()
      } catch (error) {
        console.error('Failed to toggle pin:', error)
        alert('고정 상태를 변경하는데 실패했습니다.')
      }
    }

    const confirmDelete = (id) => {
      if (confirm('정말 삭제하시겠습니까?')) {
        deleteNotice(id)
      }
    }

    const deleteNotice = async (id) => {
      try {
        await noticeAPI.deleteNotice(id)
        alert('삭제되었습니다.')
        fetchNotices()
      } catch (error) {
        console.error('Failed to delete notice:', error)
        alert('삭제에 실패했습니다.')
      }
    }

    const formatDate = (dateString) => {
      return dayjs(dateString).format('YYYY.MM.DD')
    }

    const openDetailModal = (id) => {
      alert(`상세보기 클릭됨! ID: ${id}`)
      console.log('[NoticeManagement] openDetailModal 호출됨, id:', id)
      console.log(
        '[NoticeManagement] 현재 showDetailModal:',
        showDetailModal.value,
      )
      console.log(
        '[NoticeManagement] 현재 selectedNoticeId:',
        selectedNoticeId.value,
      )

      selectedNoticeId.value = id
      showDetailModal.value = true

      console.log(
        '[NoticeManagement] 변경 후 showDetailModal:',
        showDetailModal.value,
      )
      console.log(
        '[NoticeManagement] 변경 후 selectedNoticeId:',
        selectedNoticeId.value,
      )

      // 다음 틱에서 확인
      setTimeout(() => {
        console.log(
          '[NoticeManagement] setTimeout 후 showDetailModal:',
          showDetailModal.value,
        )
        console.log(
          '[NoticeManagement] setTimeout 후 selectedNoticeId:',
          selectedNoticeId.value,
        )
      }, 100)
    }

    const closeDetailModal = () => {
      console.log('[NoticeManagement] closeDetailModal 호출됨')
      showDetailModal.value = false
      selectedNoticeId.value = null
    }

    // 생명주기
    onMounted(() => {
      fetchCategories()
      fetchNotices()
    })

    return {
      loading,
      notices,
      currentPage,
      pageSize,
      totalCount,
      searchType,
      searchKeyword,
      showModal,
      showDetailModal,
      selectedNotice,
      selectedNoticeId,
      showCategoryModal,
      categories,
      conventionId,
      totalPages,
      visiblePages,
      fetchNotices,
      handleSearch,
      goToPage,
      openCreateModal,
      openEditModal,
      closeModal,
      handleSaved,
      togglePin,
      confirmDelete,
      formatDate,
      openDetailModal,
      closeDetailModal,
    }
  },
}
</script>
