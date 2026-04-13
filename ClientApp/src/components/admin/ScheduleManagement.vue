<template>
  <div>
    <!-- 탭 -->
    <div class="flex gap-1 mb-6 border-b">
      <button
        :class="[
          'px-4 py-2.5 text-sm font-medium border-b-2 transition-colors -mb-px',
          activeTab === 'schedule'
            ? 'border-primary-600 text-primary-600'
            : 'border-transparent text-gray-500 hover:text-gray-700',
        ]"
        @click="activeTab = 'schedule'"
      >
        일정 코스
      </button>
      <button
        :class="[
          'px-4 py-2.5 text-sm font-medium border-b-2 transition-colors -mb-px',
          activeTab === 'optionTour'
            ? 'border-primary-600 text-primary-600'
            : 'border-transparent text-gray-500 hover:text-gray-700',
        ]"
        @click="activeTab = 'optionTour'"
      >
        옵션투어
      </button>
    </div>

    <!-- 옵션투어 탭 -->
    <OptionTourManagement
      v-if="activeTab === 'optionTour'"
      :convention-id="conventionId"
    />

    <!-- 일정 코스 탭 -->
    <div v-if="activeTab === 'schedule'">
      <AdminPageHeader title="일정 관리">
        <AdminButton :icon="Plus" @click="showCreateModal = true"
          >일정 코스 추가</AdminButton
        >
      </AdminPageHeader>

      <!-- 템플릿 필터 버튼 -->
      <div class="mb-4 overflow-x-auto scrollbar-hide -mx-1 px-1">
        <div class="flex gap-2 pb-2">
          <button
            :class="[
              'flex-shrink-0 px-3 sm:px-4 py-2 rounded-full text-sm font-medium transition-all whitespace-nowrap',
              selectedTemplateId === 'all'
                ? 'bg-primary-600 text-white'
                : 'bg-white text-gray-700 hover:bg-gray-100 border',
            ]"
            @click="selectedTemplateId = 'all'"
          >
            전체
          </button>
          <button
            v-for="template in templates"
            :key="template.id"
            :class="[
              'flex-shrink-0 px-3 sm:px-4 py-2 rounded-full text-sm font-medium transition-all whitespace-nowrap',
              selectedTemplateId === template.id
                ? 'bg-primary-600 text-white'
                : 'bg-white text-gray-700 hover:bg-gray-100 border',
            ]"
            @click="selectedTemplateId = template.id"
          >
            {{ template.courseName }}
          </button>
        </div>
      </div>

      <!-- 템플릿 목록 -->
      <div class="space-y-4">
        <div
          v-for="template in filteredTemplates"
          :key="template.id"
          class="bg-white rounded-lg shadow overflow-hidden"
        >
          <div
            class="p-4 sm:p-6 border-b bg-gray-50 flex items-center justify-between"
          >
            <div class="flex-1">
              <h3 class="font-semibold text-lg">{{ template.courseName }}</h3>
              <p class="text-sm text-gray-600">{{ template.description }}</p>
              <p class="text-xs text-gray-500 mt-1">
                할당된 참석자: {{ template.guestCount || 0 }}명 | 일정 항목:
                {{ template.scheduleItems?.length || 0 }}개
              </p>
            </div>
            <div class="flex gap-2">
              <button
                class="px-3 py-1.5 text-sm bg-primary-50 text-primary-600 rounded hover:bg-primary-100"
                @click="viewAssignedGuests(template)"
              >
                참석자 보기
              </button>
              <button
                class="px-3 py-1.5 text-sm bg-white border rounded hover:bg-gray-50"
                @click="editTemplate(template)"
              >
                수정
              </button>
              <button
                class="px-3 py-1.5 text-sm bg-red-50 text-red-600 rounded hover:bg-red-100"
                @click="deleteTemplate(template.id)"
              >
                삭제
              </button>
            </div>
          </div>

          <div class="p-4 sm:p-6">
            <div class="space-y-3">
              <div
                v-for="item in template.scheduleItems"
                :key="item.id"
                class="flex flex-col sm:flex-row sm:items-start gap-2 sm:gap-3 p-3 bg-gray-50 rounded-lg"
              >
                <div class="flex-shrink-0 w-20 sm:w-28 text-sm">
                  <div class="font-medium text-gray-600">
                    {{ formatDate(item.scheduleDate) }}
                  </div>
                  <div class="text-primary-600 font-semibold mt-0.5">
                    {{ item.startTime }}
                    <span v-if="item.endTime" class="text-gray-500">
                      ~ {{ item.endTime }}
                    </span>
                  </div>
                </div>
                <div class="flex-1 min-w-0">
                  <p class="font-medium text-gray-900">{{ item.title }}</p>
                  <p v-if="item.location" class="text-sm text-gray-500 mt-1">
                    📍 {{ item.location }}
                  </p>
                  <p
                    v-if="item.content"
                    class="text-sm text-gray-600 mt-1 line-clamp-2"
                  >
                    {{ stripHtml(item.content) }}
                  </p>
                  <div v-if="item.images?.length" class="flex gap-1 mt-1">
                    <img
                      v-for="img in item.images.slice(0, 3)"
                      :key="img.id"
                      :src="img.imageUrl"
                      class="w-10 h-10 object-cover rounded"
                    />
                    <span
                      v-if="item.images.length > 3"
                      class="w-10 h-10 bg-gray-200 rounded flex items-center justify-center text-xs text-gray-600"
                    >
                      +{{ item.images.length - 3 }}
                    </span>
                  </div>
                </div>
                <div class="flex gap-1">
                  <button
                    class="p-2 hover:bg-gray-200 rounded min-w-[36px] min-h-[36px] flex items-center justify-center"
                    title="수정"
                    @click="editScheduleItem(template, item)"
                  >
                    <Pencil class="w-4 h-4" />
                  </button>
                  <button
                    class="p-1.5 hover:bg-red-100 text-red-600 rounded"
                    title="삭제"
                    @click="deleteScheduleItem(item.id)"
                  >
                    <Trash2 class="w-4 h-4" />
                  </button>
                </div>
              </div>

              <div class="flex gap-2">
                <button
                  class="flex-1 py-2 text-sm text-primary-600 border-2 border-dashed border-primary-300 rounded-lg hover:bg-primary-50"
                  @click="addScheduleItem(template)"
                >
                  + 수기 등록
                </button>
                <button
                  class="flex-1 py-2 text-sm text-green-600 border-2 border-dashed border-green-300 rounded-lg hover:bg-green-50 flex items-center justify-center gap-1"
                  @click="showCopyItemsModal(template)"
                >
                  <Copy class="w-4 h-4" />
                  기존 일정 복사
                </button>
              </div>
            </div>
          </div>
        </div>

        <div
          v-if="templates.length === 0"
          class="text-center py-12 text-gray-500 bg-white rounded-lg shadow"
        >
          등록된 일정 코스가 없습니다. 일정 코스를 추가해주세요.
        </div>
      </div>

      <!-- 템플릿 생성/수정 모달 -->
      <BaseModal
        :is-open="showCreateModal || !!editingTemplate"
        max-width="md"
        @close="closeTemplateModal"
      >
        <template #header>
          <h2 class="text-xl font-semibold">
            {{ editingTemplate ? '일정 코스 수정' : '일정 코스 추가' }}
          </h2>
        </template>
        <template #body>
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium mb-1">코스명 *</label>
              <input
                v-model="templateForm.courseName"
                type="text"
                class="w-full px-3 py-2 border rounded-lg"
                placeholder="예: A코스"
              />
            </div>

            <div>
              <label class="block text-sm font-medium mb-1">설명</label>
              <textarea
                v-model="templateForm.description"
                class="w-full px-3 py-2 border rounded-lg"
                rows="2"
                placeholder="이 일정 코스에 대한 설명"
              ></textarea>
            </div>
          </div>
        </template>
        <template #footer>
          <button
            class="px-4 py-2 border rounded-lg hover:bg-gray-50"
            @click="closeTemplateModal"
          >
            취소
          </button>
          <button
            class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
            @click="saveTemplate"
          >
            저장
          </button>
        </template>
      </BaseModal>

      <!-- 일정 항목 생성/수정 모달 -->
      <BaseModal
        :is-open="showItemModal"
        max-width="lg"
        @close="closeItemModal"
      >
        <template #header>
          <h2 class="text-xl font-semibold">
            {{ editingItem ? '일정 수정' : '일정 추가' }}
          </h2>
        </template>
        <template #body>
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium mb-1">날짜 *</label>
              <input
                v-model="itemForm.scheduleDate"
                type="date"
                class="w-full px-3 py-2 border rounded-lg"
              />
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div>
                <label class="block text-sm font-medium mb-1"
                  >시작 시간 *</label
                >
                <input
                  v-model="itemForm.startTime"
                  type="time"
                  class="w-full px-3 py-2 border rounded-lg"
                />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">종료 시간</label>
                <input
                  v-model="itemForm.endTime"
                  type="time"
                  class="w-full px-3 py-2 border rounded-lg"
                />
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium mb-1">일정명 *</label>
              <input
                v-model="itemForm.title"
                type="text"
                class="w-full px-3 py-2 border rounded-lg"
                placeholder="예: 객실에 집결"
              />
            </div>

            <div>
              <label class="block text-sm font-medium mb-1">장소</label>
              <input
                v-model="itemForm.location"
                type="text"
                class="w-full px-3 py-2 border rounded-lg"
                placeholder="예: 호텔 로비"
              />
            </div>

            <div v-if="seatingLayouts.length > 0">
              <label class="block text-sm font-medium mb-1">좌석 배치도 연결</label>
              <select
                v-model="itemForm.seatingLayoutId"
                class="w-full px-3 py-2 border rounded-lg text-sm"
              >
                <option :value="null">연결 없음</option>
                <option
                  v-for="sl in seatingLayouts"
                  :key="sl.id"
                  :value="sl.id"
                >
                  {{ sl.name }}
                </option>
              </select>
              <p class="text-xs text-gray-500 mt-1">
                연결하면 사용자 일정에서 "내 자리 보기" 버튼이 표시됩니다
              </p>
            </div>

            <div>
              <label class="block text-sm font-medium mb-1">상세 내용</label>
              <RichTextEditor
                v-model="itemForm.content"
                height="200px"
                placeholder="일정에 대한 상세 설명을 입력하세요"
              />
            </div>

            <!-- 이미지 갤러리 -->
            <div>
              <label class="block text-sm font-medium mb-2"
                >이미지 갤러리</label
              >
              <div
                v-if="itemImages.length > 0 || pendingFiles.length > 0"
                class="grid grid-cols-2 sm:grid-cols-3 gap-2 mb-2"
              >
                <!-- 저장된 이미지 -->
                <div
                  v-for="img in itemImages"
                  :key="img.id"
                  class="relative group"
                >
                  <img
                    :src="img.imageUrl"
                    class="w-full h-24 object-cover rounded-lg"
                  />
                  <button
                    class="absolute top-1 right-1 w-5 h-5 bg-red-500 text-white rounded-full text-xs flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity"
                    @click="removeItemImage(img.id)"
                  >
                    &times;
                  </button>
                </div>
                <!-- 대기 중 이미지 (신규 등록 시 미리보기) -->
                <div
                  v-for="(pf, idx) in pendingFiles"
                  :key="'pending-' + idx"
                  class="relative group"
                >
                  <img
                    :src="pf.preview"
                    class="w-full h-24 object-cover rounded-lg opacity-70"
                  />
                  <button
                    class="absolute top-1 right-1 w-5 h-5 bg-red-500 text-white rounded-full text-xs flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity"
                    @click="removePendingFile(idx)"
                  >
                    &times;
                  </button>
                </div>
              </div>
              <label
                class="inline-flex items-center gap-1 px-3 py-1.5 text-sm border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:bg-gray-50"
              >
                <Plus class="w-4 h-4" />
                이미지 추가
                <input
                  type="file"
                  accept="image/*"
                  multiple
                  class="hidden"
                  @change="handleImageSelect($event)"
                />
              </label>
            </div>
          </div>
        </template>
        <template #footer>
          <button
            class="px-4 py-2 border rounded-lg hover:bg-gray-50"
            @click="closeItemModal"
          >
            취소
          </button>
          <button
            class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
            @click="saveScheduleItem"
          >
            저장
          </button>
        </template>
      </BaseModal>

      <!-- 할당된 참석자 목록 모달 -->
      <BaseModal
        :is-open="showGuestsModal"
        max-width="2xl"
        @close="closeGuestsModal"
      >
        <template #header>
          <h2 class="text-xl font-semibold">
            {{ selectedTemplate?.courseName }} - 할당된 참석자
          </h2>
        </template>
        <template #body>
          <div
            v-if="assignedGuests.length === 0"
            class="text-center py-8 text-gray-500"
          >
            이 일정에 할당된 참석자가 없습니다.
          </div>

          <div v-else class="space-y-2">
            <div
              v-for="guest in assignedGuests"
              :key="guest.id"
              class="p-4 border rounded-lg hover:bg-gray-50"
            >
              <div class="flex justify-between items-start">
                <div>
                  <p class="font-medium">{{ guest.name }}</p>
                  <p class="text-sm text-gray-600">{{ guest.telephone }}</p>
                  <p v-if="guest.corpPart" class="text-sm text-gray-500">
                    {{ guest.corpPart }}
                  </p>
                </div>
                <button
                  class="px-3 py-1 text-sm text-red-600 hover:bg-red-50 rounded"
                  @click="removeGuestFromSchedule(guest.id)"
                >
                  제거
                </button>
              </div>
            </div>
          </div>
        </template>
      </BaseModal>

      <!-- 기존 일정 복사 모달 -->
      <BaseModal
        :is-open="showCopyModal"
        max-width="4xl"
        @close="closeCopyModal"
      >
        <template #header>
          <h2 class="text-xl font-semibold">
            {{ targetTemplate?.courseName }} - 기존 일정 복사
          </h2>
        </template>
        <template #body>
          <div class="mb-4 p-4 bg-primary-50 rounded-lg">
            <p class="text-sm text-primary-900">
              다른 템플릿의 일정을 선택하면 현재 템플릿에 복사됩니다.
            </p>
            <p class="text-xs text-primary-700 mt-1">
              복사 후 개별적으로 수정할 수 있습니다.
            </p>
          </div>

          <div class="space-y-4">
            <div
              v-for="template in otherTemplates"
              :key="template.id"
              class="border rounded-lg overflow-hidden"
            >
              <div class="p-4 bg-gray-50 border-b">
                <div class="flex justify-between items-center">
                  <div>
                    <h3 class="font-semibold">{{ template.courseName }}</h3>
                    <p class="text-sm text-gray-600">
                      {{ template.description }}
                    </p>
                    <p class="text-xs text-gray-500 mt-1">
                      일정 {{ template.scheduleItems.length }}개
                    </p>
                  </div>
                  <button
                    class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
                    @click="copyAllItemsFromTemplate(template)"
                  >
                    전체 복사
                  </button>
                </div>
              </div>

              <div class="p-4">
                <div class="space-y-2">
                  <div
                    v-for="item in template.scheduleItems"
                    :key="item.id"
                    class="flex items-start gap-3 p-3 bg-white border rounded-lg hover:bg-gray-50"
                  >
                    <div class="flex-shrink-0">
                      <input
                        v-model="selectedItemsToCopy"
                        type="checkbox"
                        :value="item.id"
                        class="rounded mt-1"
                      />
                    </div>
                    <div class="flex-shrink-0 w-24 text-sm">
                      <div class="font-medium text-gray-600">
                        {{ formatDate(item.scheduleDate) }}
                      </div>
                      <div class="text-primary-600">
                        {{ item.startTime }}
                        <span v-if="item.endTime" class="text-gray-500">
                          ~ {{ item.endTime }}
                        </span>
                      </div>
                    </div>
                    <div class="flex-1">
                      <p class="font-medium">{{ item.title }}</p>
                      <p v-if="item.location" class="text-sm text-gray-500">
                        📍 {{ item.location }}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>
        <template #footer>
          <div
            v-if="selectedItemsToCopy.length > 0"
            class="w-full flex justify-between items-center"
          >
            <span class="text-sm font-medium"
              >선택: {{ selectedItemsToCopy.length }}개 일정</span
            >
            <div class="flex gap-2">
              <button
                class="px-4 py-2 border rounded-lg hover:bg-gray-50"
                @click="selectedItemsToCopy = []"
              >
                선택 취소
              </button>
              <button
                class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
                @click="copySelectedItems"
              >
                선택한 일정 복사
              </button>
            </div>
          </div>
        </template>
      </BaseModal>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { Pencil, Trash2, Copy, Plus } from 'lucide-vue-next'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'
import RichTextEditor from '@/components/common/RichTextEditor.vue'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminButton from '@/components/admin/ui/AdminButton.vue'
import OptionTourManagement from '@/components/admin/OptionTourManagement.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const activeTab = ref('schedule')
const selectedTemplateId = ref('all')

const filteredTemplates = computed(() => {
  if (selectedTemplateId.value === 'all') {
    return templates.value
  }
  return templates.value.filter((t) => t.id === selectedTemplateId.value)
})

const templates = ref([])
const showCreateModal = ref(false)
const showItemModal = ref(false)
const showGuestsModal = ref(false)
const showCopyModal = ref(false)
const editingTemplate = ref(null)
const editingItem = ref(null)
const itemImages = ref([])
const pendingFiles = ref([])
const currentTemplate = ref(null)
const selectedTemplate = ref(null)
const targetTemplate = ref(null)
const assignedGuests = ref([])
const otherTemplates = ref([])
const selectedItemsToCopy = ref([])

const templateForm = ref({
  courseName: '',
  description: '',
})

const itemForm = ref({
  scheduleDate: '',
  startTime: '',
  endTime: '',
  title: '',
  location: '',
  content: '',
  seatingLayoutId: null,
})
const seatingLayouts = ref([])

const formatDate = (dateStr) => {
  const date = new Date(dateStr)
  return (
    date.toLocaleDateString('ko-KR', {
      month: 'numeric',
      day: 'numeric',
    }) + '일'
  )
}

const stripHtml = (html) => {
  if (!html) return ''
  return html
    .replace(/<[^>]*>/g, '')
    .replace(/&nbsp;/g, ' ')
    .trim()
}

const loadTemplates = async () => {
  try {
    const [templatesRes, layoutsRes] = await Promise.all([
      apiClient.get(
        `/admin/conventions/${props.conventionId}/schedule-templates`,
      ),
      apiClient
        .get(`/admin/conventions/${props.conventionId}/seating-layouts`)
        .catch(() => ({ data: [] })),
    ])
    templates.value = templatesRes.data
    seatingLayouts.value = layoutsRes.data || []
  } catch (error) {
    console.error('Failed to load templates:', error)
  }
}

const editTemplate = (template) => {
  editingTemplate.value = template
  templateForm.value = {
    courseName: template.courseName,
    description: template.description,
  }
  showCreateModal.value = true
}

const closeTemplateModal = () => {
  showCreateModal.value = false
  editingTemplate.value = null
  templateForm.value = { courseName: '', description: '' }
}

const saveTemplate = async () => {
  try {
    if (!templateForm.value.courseName.trim()) {
      alert('코스명을 입력해주세요.')
      return
    }

    if (editingTemplate.value) {
      await apiClient.put(
        `/admin/schedule-templates/${editingTemplate.value.id}`,
        templateForm.value,
      )
    } else {
      await apiClient.post(
        `/admin/conventions/${props.conventionId}/schedule-templates`,
        templateForm.value,
      )
    }
    await loadTemplates()
    closeTemplateModal()
  } catch (error) {
    console.error('Failed to save template:', error)
    alert(
      '템플릿 저장 실패: ' + (error.response?.data?.message || error.message),
    )
  }
}

const deleteTemplate = async (id) => {
  if (
    !confirm(
      '템플릿을 삭제하면 모든 일정 항목도 함께 삭제됩니다. 계속하시겠습니까?',
    )
  )
    return

  try {
    await apiClient.delete(`/admin/schedule-templates/${id}`)
    await loadTemplates()
  } catch (error) {
    console.error('Failed to delete template:', error)
    alert(
      '템플릿 삭제 실패: ' + (error.response?.data?.message || error.message),
    )
  }
}

const addScheduleItem = (template) => {
  currentTemplate.value = template
  editingItem.value = null
  itemImages.value = []
  pendingFiles.value = []
  itemForm.value = {
    scheduleDate: '',
    startTime: '',
    endTime: '',
    title: '',
    location: '',
    content: '',
    seatingLayoutId: null,
  }
  showItemModal.value = true
}

const editScheduleItem = (template, item) => {
  currentTemplate.value = template
  editingItem.value = item
  itemImages.value = item.images || []
  itemForm.value = {
    scheduleDate: item.scheduleDate?.split('T')[0] || '',
    startTime: item.startTime,
    endTime: item.endTime || '',
    title: item.title,
    location: item.location || '',
    content: item.content || '',
    seatingLayoutId: item.seatingLayoutId || null,
  }
  showItemModal.value = true
}

const closeItemModal = () => {
  showItemModal.value = false
  editingItem.value = null
  itemImages.value = []
  pendingFiles.value = []
  currentTemplate.value = null
}

const saveScheduleItem = async () => {
  try {
    if (
      !itemForm.value.scheduleDate ||
      !itemForm.value.startTime ||
      !itemForm.value.title
    ) {
      alert('날짜, 시간, 일정명은 필수입니다.')
      return
    }

    const data = {
      ...itemForm.value,
      scheduleTemplateId: currentTemplate.value.id,
    }

    let itemId
    if (editingItem.value) {
      await apiClient.put(`/admin/schedule-items/${editingItem.value.id}`, data)
      itemId = editingItem.value.id
    } else {
      const res = await apiClient.post('/admin/schedule-items', data)
      itemId = res.data.id
    }

    // 대기 중 이미지 업로드
    if (pendingFiles.value.length > 0) {
      await uploadPendingImages(itemId)
    }

    await loadTemplates()
    closeItemModal()
  } catch (error) {
    console.error('Failed to save schedule item:', error)
    alert(
      '일정 항목 저장 실패: ' +
        (error.response?.data?.message || error.message),
    )
  }
}

const deleteScheduleItem = async (id) => {
  if (!confirm('이 일정을 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/admin/schedule-items/${id}`)
    await loadTemplates()
  } catch (error) {
    console.error('Failed to delete schedule item:', error)
    alert(
      '일정 항목 삭제 실패: ' +
        (error.response?.data?.message || error.message),
    )
  }
}

const viewAssignedGuests = async (template) => {
  selectedTemplate.value = template
  try {
    const response = await apiClient.get(
      `/admin/schedule-templates/${template.id}/guests`,
    )
    assignedGuests.value = response.data
    showGuestsModal.value = true
  } catch (error) {
    console.error('Failed to load assigned guests:', error)
    alert(
      '참석자 목록 로드 실패: ' +
        (error.response?.data?.message || error.message),
    )
  }
}

const closeGuestsModal = () => {
  showGuestsModal.value = false
  selectedTemplate.value = null
  assignedGuests.value = []
}

const removeGuestFromSchedule = async (guestId) => {
  if (!confirm('이 참석자를 일정에서 제거하시겠습니까?')) return

  try {
    await apiClient.delete(
      `/admin/guests/${guestId}/schedules/${selectedTemplate.value.id}`,
    )
    await viewAssignedGuests(selectedTemplate.value)
    await loadTemplates()
  } catch (error) {
    console.error('Failed to remove guest from schedule:', error)
    alert(
      '참석자 제거 실패: ' + (error.response?.data?.message || error.message),
    )
  }
}

const showCopyItemsModal = (template) => {
  targetTemplate.value = template
  otherTemplates.value = templates.value.filter((t) => t.id !== template.id)
  selectedItemsToCopy.value = []
  showCopyModal.value = true
}

const closeCopyModal = () => {
  showCopyModal.value = false
  targetTemplate.value = null
  otherTemplates.value = []
  selectedItemsToCopy.value = []
}

const copyAllItemsFromTemplate = async (sourceTemplate) => {
  if (
    !confirm(
      `${sourceTemplate.courseName}의 모든 일정(${sourceTemplate.scheduleItems.length}개)을 복사하시겠습니까?`,
    )
  )
    return

  try {
    const itemsToCopy = sourceTemplate.scheduleItems.map((item) => ({
      scheduleDate: item.scheduleDate,
      startTime: item.startTime,
      endTime: item.endTime,
      title: item.title,
      location: item.location,
      content: item.content,
      scheduleTemplateId: targetTemplate.value.id,
    }))

    await apiClient.post('/admin/schedule-items/bulk', { items: itemsToCopy })
    await loadTemplates()
    closeCopyModal()
    alert('일정 복사 완료')
  } catch (error) {
    console.error('Failed to copy items:', error)
    alert('일정 복사 실패: ' + (error.response?.data?.message || error.message))
  }
}

const copySelectedItems = async () => {
  if (selectedItemsToCopy.value.length === 0) {
    alert('복사할 일정을 선택해주세요.')
    return
  }

  try {
    const itemsToCopy = []
    otherTemplates.value.forEach((template) => {
      template.scheduleItems.forEach((item) => {
        if (selectedItemsToCopy.value.includes(item.id)) {
          itemsToCopy.push({
            scheduleDate: item.scheduleDate,
            startTime: item.startTime,
            endTime: item.endTime,
            title: item.title,
            location: item.location,
            content: item.content,
            scheduleTemplateId: targetTemplate.value.id,
          })
        }
      })
    })

    await apiClient.post('/admin/schedule-items/bulk', { items: itemsToCopy })
    await loadTemplates()
    closeCopyModal()
    alert(`${itemsToCopy.length}개 일정 복사 완료`)
  } catch (error) {
    console.error('Failed to copy items:', error)
    alert('일정 복사 실패: ' + (error.response?.data?.message || error.message))
  }
}

const handleImageSelect = async (event) => {
  const files = Array.from(event.target.files)
  if (!files.length) return

  if (editingItem.value) {
    // 수정 모드: 즉시 업로드
    for (const file of files) {
      try {
        const formData = new FormData()
        formData.append('file', file)
        const uploadRes = await apiClient.post('/file/upload/image', formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
        })
        const res = await apiClient.post(
          `/admin/schedule-items/${editingItem.value.id}/images`,
          { imageUrl: uploadRes.data.url },
        )
        itemImages.value.push(res.data)
      } catch (error) {
        console.error('Image upload failed:', error)
        alert('이미지 업로드 실패')
      }
    }
    await loadTemplates()
  } else {
    // 신규 등록: 미리보기로 대기
    for (const file of files) {
      pendingFiles.value.push({
        file,
        preview: URL.createObjectURL(file),
      })
    }
  }
  event.target.value = ''
}

const removePendingFile = (idx) => {
  URL.revokeObjectURL(pendingFiles.value[idx].preview)
  pendingFiles.value.splice(idx, 1)
}

const uploadPendingImages = async (itemId) => {
  let failCount = 0
  for (const pf of pendingFiles.value) {
    try {
      const formData = new FormData()
      formData.append('file', pf.file)
      const uploadRes = await apiClient.post('/file/upload/image', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      await apiClient.post(`/admin/schedule-items/${itemId}/images`, {
        imageUrl: uploadRes.data.url,
      })
    } catch (error) {
      console.error('Image upload failed:', error)
      failCount++
    }
    URL.revokeObjectURL(pf.preview)
  }
  pendingFiles.value = []
  if (failCount > 0) {
    alert(`${failCount}개 이미지 업로드에 실패했습니다.`)
  }
}

const removeItemImage = async (imageId) => {
  try {
    await apiClient.delete(`/admin/schedule-images/${imageId}`)
    itemImages.value = itemImages.value.filter((img) => img.id !== imageId)
    await loadTemplates()
  } catch (error) {
    console.error('Image remove failed:', error)
    alert('이미지 삭제 실패')
  }
}

onMounted(() => {
  loadTemplates()
})
</script>
