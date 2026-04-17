<template>
  <div>
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-lg font-bold text-gray-900">홈 배너 관리</h2>
      <button
        class="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors"
        @click="openCreateModal"
      >
        + 배너 추가
      </button>
    </div>

    <p class="text-sm text-gray-500 mb-4">
      메인 홈 상단에 표시되는 캐러셀 배너입니다. 드래그하여 순서를 변경할 수 있습니다.
    </p>

    <div v-if="loading" class="text-center py-8 text-gray-400">로딩 중...</div>

    <div v-else-if="banners.length === 0" class="text-center py-12 text-gray-400">
      <p class="text-lg mb-2">등록된 배너가 없습니다</p>
      <p class="text-sm">배너를 추가하면 메인 홈 상단에 캐러셀로 표시됩니다.</p>
    </div>

    <draggable
      v-else
      v-model="banners"
      item-key="id"
      handle=".drag-handle"
      class="space-y-3"
      @end="onReorder"
    >
      <template #item="{ element }">
        <div class="bg-white rounded-xl shadow-sm border overflow-hidden">
          <!-- 모바일: 세로 레이아웃 / PC: 가로 레이아웃 -->
          <div class="flex flex-col sm:flex-row sm:items-stretch">
            <!-- 썸네일 + 드래그 핸들 -->
            <div class="relative flex-shrink-0">
              <img :src="element.imageUrl" class="w-full h-32 sm:w-32 sm:h-20 object-cover" alt="" />
              <div class="drag-handle absolute top-2 left-2 p-1 bg-black/40 rounded cursor-grab active:cursor-grabbing text-white/80">
                <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M8 6h2v2H8zm6 0h2v2h-2zM8 11h2v2H8zm6 0h2v2h-2zM8 16h2v2H8zm6 0h2v2h-2z" />
                </svg>
              </div>
            </div>

            <!-- 정보 + 액션 -->
            <div class="flex-1 min-w-0 p-3 flex items-center justify-between gap-2">
              <div class="min-w-0">
                <div class="flex flex-wrap items-center gap-1.5 mb-1">
                  <span
                    class="px-2 py-0.5 text-xs rounded-full font-medium"
                    :class="element.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'"
                  >
                    {{ element.isActive ? '활성' : '비활성' }}
                  </span>
                  <span v-if="element.detailImagesJson" class="text-xs text-purple-600">상세페이지</span>
                  <span v-if="element.title" class="text-xs text-gray-700 font-medium">{{ element.title }}</span>
                </div>
                <p v-if="element.linkUrl" class="text-xs text-gray-400 truncate">{{ element.linkUrl }}</p>
                <p v-else-if="!element.detailImagesJson" class="text-xs text-gray-400">링크 없음</p>
              </div>

              <!-- 액션 버튼 -->
              <div class="flex items-center gap-0.5 flex-shrink-0">
                <button
                  class="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-blue-600 transition-colors"
                  @click="openEditModal(element)"
                >
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                  </svg>
                </button>
                <button
                  class="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-red-600 transition-colors"
                  @click="deleteBanner(element)"
                >
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </template>
    </draggable>

    <!-- 추가/수정 모달 -->
    <BaseModal :is-open="isModalOpen" max-width="md" @close="isModalOpen = false">
      <template #header>
        <h3 class="text-lg font-bold">{{ editingBanner ? '배너 수정' : '배너 추가' }}</h3>
      </template>
      <template #body>
        <div class="space-y-4">
          <!-- 이미지 업로드 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">배너 이미지</label>
            <div v-if="form.imageUrl" class="mb-2">
              <div class="relative inline-block w-full">
                <img :src="form.imageUrl" class="w-full h-40 object-cover rounded-lg" alt="" />
                <button
                  class="absolute top-2 right-2 w-7 h-7 flex items-center justify-center bg-black/60 text-white rounded-full hover:bg-black/80 transition-colors"
                  @click="form.imageUrl = ''"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>
            <label
              class="flex items-center justify-center gap-2 px-4 py-3 border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:border-blue-400 hover:bg-blue-50 transition-colors"
            >
              <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <span class="text-sm text-gray-500">이미지 선택</span>
              <input type="file" class="hidden" accept="image/*" @change="onFileSelect" />
            </label>
          </div>

          <!-- 제목 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">배너 제목 (선택)</label>
            <input
              v-model="form.title"
              type="text"
              class="w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="2026 스타투어 일정 안내"
            />
          </div>

          <!-- 상세 페이지 이미지 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              상세 페이지 이미지
              <span class="text-xs text-gray-400 font-normal ml-1">클릭 시 이미지가 이어붙여진 상세 페이지로 이동</span>
            </label>
            <div v-if="form.detailImages.length > 0" class="space-y-2 mb-2">
              <draggable v-model="form.detailImages" item-key="idx" handle=".detail-drag" class="space-y-2">
                <template #item="{ element, index }">
                  <div class="flex items-center gap-2 bg-gray-50 rounded-lg p-2">
                    <div class="detail-drag cursor-grab text-gray-300 hover:text-gray-500">
                      <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                        <path d="M8 6h2v2H8zm6 0h2v2h-2zM8 11h2v2H8zm6 0h2v2h-2zM8 16h2v2H8zm6 0h2v2h-2z" />
                      </svg>
                    </div>
                    <img :src="element" class="w-16 h-10 object-cover rounded" alt="" />
                    <span class="flex-1 text-xs text-gray-500 truncate">{{ index + 1 }}번</span>
                    <button class="p-1 text-gray-400 hover:text-red-500" @click="removeDetailImage(index)">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>
                </template>
              </draggable>
            </div>
            <label
              class="flex items-center justify-center gap-2 px-4 py-2 border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:border-blue-400 hover:bg-blue-50 transition-colors"
            >
              <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              <span class="text-sm text-gray-500">상세 이미지 추가</span>
              <input type="file" class="hidden" accept="image/*" multiple @change="onDetailFileSelect" />
            </label>
          </div>

          <!-- 구분선 -->
          <div class="border-t pt-4">
            <p class="text-xs text-gray-400 mb-3">상세 이미지가 없으면 아래 링크로 이동합니다.</p>
          </div>

          <!-- 링크 URL -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">링크 URL (선택)</label>
            <input
              v-model="form.linkUrl"
              type="text"
              class="w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="https://... 또는 /conventions/1"
            />
          </div>

          <!-- 버튼 텍스트 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">버튼 텍스트 (선택)</label>
            <input
              v-model="form.linkLabel"
              type="text"
              class="w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="자세히 보기"
            />
            <p class="text-xs text-gray-400 mt-1">비워두면 버튼 없이 이미지만 표시됩니다.</p>
          </div>

          <!-- 활성 토글 -->
          <div class="flex items-center justify-between">
            <div>
              <span class="text-sm font-medium text-gray-700">활성 상태</span>
              <p class="text-xs text-gray-400">{{ form.isActive ? '메인 홈에 표시됨' : '표시 안 됨' }}</p>
            </div>
            <button
              type="button"
              role="switch"
              :aria-checked="form.isActive"
              class="relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200"
              :class="form.isActive ? 'bg-blue-600' : 'bg-gray-200'"
              @click="form.isActive = !form.isActive"
            >
              <span
                class="pointer-events-none inline-block h-5 w-5 rounded-full bg-white shadow ring-0 transition-transform duration-200"
                :class="form.isActive ? 'translate-x-5' : 'translate-x-0'"
              />
            </button>
          </div>
        </div>
      </template>
      <template #footer>
        <button
          class="px-4 py-2 text-sm text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
          @click="isModalOpen = false"
        >
          취소
        </button>
        <button
          class="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
          :disabled="!form.imageUrl || saving"
          @click="saveBanner"
        >
          {{ saving ? '저장 중...' : (editingBanner ? '수정' : '추가') }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import draggable from 'vuedraggable'
import BaseModal from '@/components/common/BaseModal.vue'
import apiClient from '@/services/api'

const banners = ref([])
const loading = ref(true)
const isModalOpen = ref(false)
const editingBanner = ref(null)
const saving = ref(false)

const form = ref({
  imageUrl: '',
  linkUrl: '',
  linkLabel: '',
  title: '',
  detailImages: [],
  isActive: true,
})

async function loadBanners() {
  loading.value = true
  try {
    const res = await apiClient.get('/admin/home-banners')
    banners.value = res.data || []
  } catch {
    banners.value = []
  } finally {
    loading.value = false
  }
}

function openCreateModal() {
  editingBanner.value = null
  form.value = { imageUrl: '', linkUrl: '', linkLabel: '', title: '', detailImages: [], isActive: true }
  isModalOpen.value = true
}

function openEditModal(banner) {
  editingBanner.value = banner
  let detailImages = []
  if (banner.detailImagesJson) {
    try { detailImages = JSON.parse(banner.detailImagesJson) } catch { detailImages = [] }
  }
  form.value = {
    imageUrl: banner.imageUrl,
    linkUrl: banner.linkUrl || '',
    linkLabel: banner.linkLabel || '',
    title: banner.title || '',
    detailImages,
    isActive: banner.isActive,
  }
  isModalOpen.value = true
}

async function onFileSelect(e) {
  const file = e.target.files?.[0]
  if (!file) return
  try {
    const fd = new FormData()
    fd.append('file', file)
    const res = await apiClient.post('/file/upload/image?dateFolder=banners', fd, { headers: { 'Content-Type': 'multipart/form-data' } })
    form.value.imageUrl = res.data.url
  } catch (err) {
    alert('이미지 업로드 실패')
    console.error(err)
  }
}

async function onDetailFileSelect(e) {
  const files = Array.from(e.target.files || [])
  for (const file of files) {
    try {
      const fd = new FormData()
      fd.append('file', file)
      const res = await apiClient.post('/file/upload/image?dateFolder=banners', fd, { headers: { 'Content-Type': 'multipart/form-data' } })
      form.value.detailImages.push(res.data.url)
    } catch (err) {
      alert(`이미지 업로드 실패: ${file.name}`)
      console.error(err)
    }
  }
  e.target.value = ''
}

function removeDetailImage(index) {
  form.value.detailImages.splice(index, 1)
}

async function saveBanner() {
  if (!form.value.imageUrl) return
  saving.value = true
  try {
    const payload = {
      imageUrl: form.value.imageUrl,
      linkUrl: form.value.linkUrl || null,
      linkLabel: form.value.linkLabel || null,
      title: form.value.title || null,
      detailImagesJson: form.value.detailImages.length > 0 ? JSON.stringify(form.value.detailImages) : null,
      isActive: form.value.isActive,
    }
    if (editingBanner.value) {
      await apiClient.put(`/admin/home-banners/${editingBanner.value.id}`, payload)
    } else {
      await apiClient.post('/admin/home-banners', payload)
    }
    isModalOpen.value = false
    await loadBanners()
  } catch (err) {
    alert('저장 실패')
    console.error(err)
  } finally {
    saving.value = false
  }
}

async function deleteBanner(banner) {
  if (!confirm('이 배너를 삭제하시겠습니까?')) return
  try {
    await apiClient.delete(`/admin/home-banners/${banner.id}`)
    await loadBanners()
  } catch (err) {
    alert('삭제 실패')
    console.error(err)
  }
}

async function onReorder() {
  const ids = banners.value.map((b) => b.id)
  try {
    await apiClient.put('/admin/home-banners/reorder', ids)
  } catch (err) {
    console.error('순서 변경 실패:', err)
  }
}

onMounted(() => loadBanners())
</script>
