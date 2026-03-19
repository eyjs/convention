<template>
  <!-- 참석자 생성/수정 모달 -->
  <BaseModal
    :is-open="isOpen"
    :max-width="activeTab === 'existing' && !editingGuest ? '5xl' : '2xl'"
    @close="closeGuestModal"
  >
    <template #header>
      <h2 class="text-xl font-semibold">
        {{ editingGuest ? '참석자 수정' : '참석자 추가' }}
      </h2>
    </template>
    <template #body>
      <!-- 탭 선택 (새 참석자 생성 시에만 표시) -->
      <div v-if="!editingGuest" class="flex border-b mb-4">
        <button
          class="flex-1 py-2.5 text-sm font-medium text-center transition-colors"
          :class="
            activeTab === 'manual'
              ? 'text-primary-600 border-b-2 border-primary-600'
              : 'text-gray-500 hover:text-gray-700'
          "
          @click="activeTab = 'manual'"
        >
          새 참석자 수기 입력
        </button>
        <button
          class="flex-1 py-2.5 text-sm font-medium text-center transition-colors"
          :class="
            activeTab === 'existing'
              ? 'text-primary-600 border-b-2 border-primary-600'
              : 'text-gray-500 hover:text-gray-700'
          "
          @click="switchToExistingTab"
        >
          기존 사용자 추가
        </button>
      </div>

      <!-- 기존 사용자 추가 탭: 좌우 2패널 -->
      <div
        v-if="activeTab === 'existing' && !editingGuest"
        class="flex flex-col h-[60vh]"
      >
        <!-- 그룹명 입력 -->
        <div class="mb-3 flex-shrink-0">
          <label class="block text-sm font-medium mb-1">그룹명 (선택)</label>
          <input
            v-model="linkGroupName"
            type="text"
            placeholder="그룹명 입력 (선택사항)"
            class="w-full px-3 py-2 border rounded-lg"
          />
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 flex-1 min-h-0">
          <!-- 왼쪽: 전체 명단 탐색기 -->
          <div class="border rounded-lg flex flex-col min-h-0">
            <div class="p-3 bg-gray-50 border-b rounded-t-lg flex-shrink-0">
              <h3 class="text-sm font-semibold text-gray-700 mb-2">
                전체 사용자 명단
              </h3>
              <input
                v-model="userSearchTerm"
                type="text"
                placeholder="이름, 전화번호, 로그인ID 검색..."
                class="w-full px-3 py-2 border rounded-lg text-sm"
                @input="onSearchInput"
              />
            </div>

            <div class="flex-1 overflow-y-auto min-h-0">
              <!-- 로딩 -->
              <div
                v-if="userSearchLoading"
                class="flex items-center justify-center py-8 text-gray-500 text-sm"
              >
                검색 중...
              </div>

              <!-- 초기 상태 -->
              <div
                v-else-if="
                  !userSearchTerm && filteredSearchResults.length === 0
                "
                class="flex items-center justify-center py-8 text-gray-400 text-sm"
              >
                검색어를 입력하면 사용자 목록이 표시됩니다
              </div>

              <!-- 결과 없음 -->
              <div
                v-else-if="filteredSearchResults.length === 0"
                class="flex items-center justify-center py-8 text-gray-400 text-sm"
              >
                검색 결과가 없습니다 (이미 등록된 사용자 제외)
              </div>

              <!-- 검색 결과 목록 -->
              <div v-else>
                <div
                  v-for="user in filteredSearchResults"
                  :key="user.id"
                  class="flex items-center gap-3 px-3 py-2.5 border-b last:border-b-0 hover:bg-gray-50 cursor-pointer transition-colors"
                  :class="{
                    'bg-primary-50': isUserSelected(user.id),
                  }"
                  @click="toggleUserSelection(user)"
                >
                  <input
                    type="checkbox"
                    :checked="isUserSelected(user.id)"
                    class="rounded flex-shrink-0"
                    @click.stop
                    @change="toggleUserSelection(user)"
                  />
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-gray-900 truncate">
                      {{ user.name }}
                    </p>
                    <p class="text-xs text-gray-500 truncate">
                      {{ user.phone || '-' }} /
                      {{ user.loginId || '비회원' }}
                    </p>
                  </div>
                  <span
                    class="px-2 py-0.5 text-xs rounded-full flex-shrink-0"
                    :class="
                      user.role === 'Admin'
                        ? 'bg-red-100 text-red-800'
                        : user.role === 'User'
                          ? 'bg-blue-100 text-blue-800'
                          : 'bg-gray-100 text-gray-800'
                    "
                  >
                    {{ user.role }}
                  </span>
                </div>
              </div>
            </div>

            <!-- 페이지네이션 -->
            <div
              v-if="userSearchTotalPages > 1"
              class="p-2 border-t bg-gray-50 flex items-center justify-between text-xs flex-shrink-0"
            >
              <button
                class="px-2 py-1 rounded hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
                :disabled="userSearchPage <= 1"
                @click="goToPage(userSearchPage - 1)"
              >
                이전
              </button>
              <span class="text-gray-600">
                {{ userSearchPage }} / {{ userSearchTotalPages }} 페이지 ({{
                  userSearchTotalCount
                }}명)
              </span>
              <button
                class="px-2 py-1 rounded hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
                :disabled="userSearchPage >= userSearchTotalPages"
                @click="goToPage(userSearchPage + 1)"
              >
                다음
              </button>
            </div>
          </div>

          <!-- 오른쪽: 선택된 명단 -->
          <div class="border rounded-lg flex flex-col min-h-0">
            <div
              class="p-3 bg-primary-50 border-b rounded-t-lg flex items-center justify-between flex-shrink-0"
            >
              <h3 class="text-sm font-semibold text-primary-700">
                추가할 참석자 ({{ selectedUsers.length }}명)
              </h3>
              <button
                v-if="selectedUsers.length > 0"
                class="text-xs text-red-500 hover:text-red-700"
                @click="selectedUsers = []"
              >
                전체 해제
              </button>
            </div>

            <div class="flex-1 overflow-y-auto min-h-0">
              <!-- 비어있는 상태 -->
              <div
                v-if="selectedUsers.length === 0"
                class="flex items-center justify-center h-full text-gray-400 text-sm"
              >
                왼쪽 목록에서 사용자를 선택하세요
              </div>

              <!-- 선택된 사용자 목록 -->
              <div v-else>
                <div
                  v-for="user in selectedUsers"
                  :key="user.id"
                  class="flex items-center gap-3 px-3 py-2.5 border-b last:border-b-0 group"
                >
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-gray-900 truncate">
                      {{ user.name }}
                    </p>
                    <p class="text-xs text-gray-500 truncate">
                      {{ user.phone || '-' }} /
                      {{ user.loginId || '비회원' }}
                    </p>
                  </div>
                  <button
                    class="p-1 text-gray-400 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-opacity flex-shrink-0"
                    @click="removeSelectedUser(user.id)"
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
                        d="M6 18L18 6M6 6l12 12"
                      />
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 수기 입력 탭 (기존 폼) -->
      <div v-if="activeTab === 'manual' || editingGuest" class="space-y-4">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium mb-1">이름 *</label>
            <input
              v-model="guestForm.guestName"
              type="text"
              class="w-full px-3 py-2 border rounded-lg"
            />
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">전화번호 *</label>
            <input
              v-model="guestForm.telephone"
              type="text"
              class="w-full px-3 py-2 border rounded-lg"
            />
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium mb-1">부서</label>
            <input
              v-model="guestForm.corpPart"
              type="text"
              class="w-full px-3 py-2 border rounded-lg"
            />
          </div>
          <div>
            <label class="block text-sm font-medium mb-1">주민등록번호</label>
            <input
              v-model="guestForm.residentNumber"
              type="text"
              placeholder="000000-0000000"
              class="w-full px-3 py-2 border rounded-lg"
            />
          </div>
        </div>

        <div>
          <label class="block text-sm font-medium mb-1">소속</label>
          <input
            v-model="guestForm.affiliation"
            type="text"
            class="w-full px-3 py-2 border rounded-lg"
          />
        </div>

        <div>
          <label class="block text-sm font-medium mb-1"
            >초기 비밀번호 (선택)</label
          >
          <input
            v-model="guestForm.password"
            type="password"
            placeholder="미입력 시 주민등록번호 앞 6자리 자동 설정"
            class="w-full px-3 py-2 border rounded-lg"
          />
          <p class="text-xs text-gray-500 mt-1">
            * 주민등록번호가 없거나 비밀번호를 지정하지 않으면 기본 비밀번호
            "123456"이 설정됩니다.
          </p>
        </div>

        <div>
          <label class="block text-sm font-medium mb-2">속성 정보</label>
          <div class="space-y-3">
            <!-- 템플릿 기반 속성 -->
            <div
              v-for="template in attributeTemplates"
              :key="template.id"
              class="space-y-2"
            >
              <label class="block text-sm font-medium text-gray-700">{{
                template.attributeKey
              }}</label>
              <select
                v-if="template.attributeValues"
                v-model="guestForm.templateAttributes[template.attributeKey]"
                class="w-full px-3 py-2 border rounded-lg"
              >
                <option value="">선택하세요</option>
                <option
                  v-for="value in parseAttributeValues(
                    template.attributeValues,
                  )"
                  :key="value"
                  :value="value"
                >
                  {{ value }}
                </option>
              </select>
              <input
                v-else
                v-model="guestForm.templateAttributes[template.attributeKey]"
                type="text"
                class="w-full px-3 py-2 border rounded-lg"
                :placeholder="`${template.attributeKey} 입력`"
              />
            </div>

            <!-- 추가 속성 (수기) -->
            <div class="pt-3 border-t">
              <p class="text-sm text-gray-600 mb-2">추가 속성 (수기 입력)</p>
              <div
                v-for="(attr, idx) in guestForm.customAttributes"
                :key="idx"
                class="flex flex-col sm:flex-row gap-2 mb-2"
              >
                <input
                  v-model="attr.key"
                  placeholder="키 (예: 호차)"
                  class="flex-1 px-3 py-2 border rounded-lg"
                />
                <input
                  v-model="attr.value"
                  placeholder="값"
                  class="flex-1 px-3 py-2 border rounded-lg"
                />
                <button
                  class="px-3 py-2 text-red-600 hover:bg-red-50 rounded-lg whitespace-nowrap overflow-hidden text-ellipsis"
                  @click="guestForm.customAttributes.splice(idx, 1)"
                >
                  삭제
                </button>
              </div>
              <button
                class="w-full py-2 border-2 border-dashed rounded-lg text-sm text-gray-600 hover:bg-gray-50 whitespace-nowrap overflow-hidden text-ellipsis"
                @click="guestForm.customAttributes.push({ key: '', value: '' })"
              >
                + 속성 추가
              </button>
            </div>
          </div>
        </div>

        <div>
          <label class="block text-sm font-medium mb-2">일정 배정</label>
          <div
            v-if="availableTemplates.length === 0"
            class="text-sm text-gray-500 p-3 bg-gray-50 rounded"
          >
            일정 템플릿이 없습니다. 먼저 일정 관리에서 템플릿을 생성하세요.
          </div>
          <div v-else>
            <!-- 다른 참석자 일정 복사버튼 -->
            <div class="flex justify-between items-center mb-2">
              <span class="text-sm text-gray-600"
                >선택: {{ guestForm.scheduleTemplateIds.length }}개</span
              >
              <button
                class="text-sm text-blue-600 hover:text-blue-800 flex items-center gap-1 whitespace-nowrap overflow-hidden text-ellipsis"
                @click="showCopyScheduleModal = true"
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
                    d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"
                  />
                </svg>
                다른 참석자 일정 복사
              </button>
            </div>
            <div class="space-y-2 max-h-60 overflow-y-auto border rounded p-2">
              <label
                v-for="template in availableTemplates"
                :key="template.id"
                class="flex items-start gap-2 p-2 border rounded hover:bg-gray-50 cursor-pointer"
              >
                <input
                  v-model="guestForm.scheduleTemplateIds"
                  type="checkbox"
                  :value="template.id"
                  class="rounded mt-1"
                />
                <div class="flex-1">
                  <div class="font-medium">{{ template.courseName }}</div>
                  <div
                    v-if="template.description"
                    class="text-xs text-gray-500"
                  >
                    {{ template.description }}
                  </div>
                  <div class="text-xs text-gray-400 mt-1">
                    일정 {{ template.scheduleItems?.length || 0 }}개
                  </div>
                </div>
              </label>
            </div>
          </div>
        </div>
      </div>
    </template>
    <template #footer>
      <button
        class="px-4 py-2 border rounded-lg hover:bg-gray-50"
        @click="closeGuestModal"
      >
        취소
      </button>
      <button
        v-if="activeTab === 'existing' && !editingGuest"
        class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed"
        :disabled="selectedUsers.length === 0 || linkLoading"
        @click="linkExistingUsers"
      >
        {{ linkLoading ? '추가 중...' : `${selectedUsers.length}명 일괄 추가` }}
      </button>
      <button
        v-else
        class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
        @click="saveGuest"
      >
        저장
      </button>
    </template>
  </BaseModal>

  <!-- 일정 복사 모달 -->
  <BaseModal
    :is-open="showCopyScheduleModal"
    max-width="lg"
    @close="closeCopyScheduleModal"
  >
    <template #header>
      <h2 class="text-xl font-semibold">다른 참석자 일정 복사</h2>
    </template>
    <template #body>
      <div class="mb-4">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="참석자 이름 검색..."
          class="w-full px-3 py-2 border rounded-lg"
        />
      </div>

      <div class="space-y-2 max-h-96 overflow-y-auto">
        <div
          v-for="guest in filteredGuestsForCopy"
          :key="guest.id"
          class="p-3 border rounded-lg hover:bg-gray-50 cursor-pointer"
          @click="copyScheduleFromGuest(guest)"
        >
          <div class="flex justify-between items-start">
            <div>
              <p class="font-medium">{{ guest.guestName }}</p>
              <p class="text-sm text-gray-500">{{ guest.telephone }}</p>
              <div
                v-if="guest.scheduleTemplates.length > 0"
                class="flex flex-wrap gap-1 mt-2"
              >
                <span
                  v-for="st in guest.scheduleTemplates"
                  :key="st.scheduleTemplateId"
                  class="px-2 py-0.5 bg-blue-100 text-blue-800 rounded text-xs"
                >
                  {{ st.courseName }}
                </span>
              </div>
              <p v-else class="text-xs text-gray-400 mt-1">배정된 일정 없음</p>
            </div>
            <button
              v-if="guest.scheduleTemplates.length > 0"
              class="px-3 py-1 text-sm bg-blue-600 text-white rounded hover:bg-blue-700 whitespace-nowrap overflow-hidden text-ellipsis"
            >
              복사
            </button>
          </div>
        </div>
      </div>
    </template>
    <template #footer>
      <button
        class="px-4 py-2 border rounded-lg hover:bg-gray-50"
        @click="closeCopyScheduleModal"
      >
        취소
      </button>
    </template>
  </BaseModal>
</template>

<script setup>
import { ref, computed, watch, onUnmounted } from 'vue'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({
  isOpen: { type: Boolean, required: true },
  editingGuest: { type: Object, default: null },
  availableTemplates: { type: Array, required: true },
  attributeTemplates: { type: Array, required: true },
  guests: { type: Array, required: true },
  conventionId: { type: Number, required: true },
})

const emit = defineEmits(['close', 'saved'])

// 탭 상태
const activeTab = ref('manual')

// 기존 사용자 검색 상태
const userSearchTerm = ref('')
const userSearchResults = ref([])
const userSearchLoading = ref(false)
const userSearchPage = ref(1)
const userSearchTotalPages = ref(0)
const userSearchTotalCount = ref(0)
const selectedUsers = ref([])
const linkGroupName = ref('')
const linkLoading = ref(false)
let searchTimeout = null

onUnmounted(() => {
  if (searchTimeout) clearTimeout(searchTimeout)
})

// 수기 입력 상태
const showCopyScheduleModal = ref(false)
const searchQuery = ref('')

const guestForm = ref({
  guestName: '',
  telephone: '',
  corpPart: '',
  residentNumber: '',
  affiliation: '',
  password: '',
  scheduleTemplateIds: [],
  templateAttributes: {},
  customAttributes: [],
})

// 이미 해당 행사에 등록된 사용자 ID Set
const registeredUserIds = computed(() => {
  return new Set(props.guests.map((g) => g.id))
})

// 검색 결과에서 이미 등록된 사용자 제외
const filteredSearchResults = computed(() => {
  return userSearchResults.value.filter(
    (u) => !registeredUserIds.value.has(u.id),
  )
})

const filteredGuestsForCopy = computed(() => {
  if (!searchQuery.value) return props.guests
  return props.guests.filter((g) =>
    g.guestName.toLowerCase().includes(searchQuery.value.toLowerCase()),
  )
})

const parseAttributeValues = (valuesStr) => {
  if (!valuesStr) return []
  try {
    return JSON.parse(valuesStr)
  } catch {
    return []
  }
}

// 기존 사용자 탭으로 전환 시 전체 목록 로드
const switchToExistingTab = () => {
  activeTab.value = 'existing'
  if (userSearchResults.value.length === 0 && !userSearchTerm.value) {
    loadAllUsers()
  }
}

const loadAllUsers = async () => {
  userSearchLoading.value = true
  try {
    const response = await apiClient.get('/admin/users', {
      params: { page: 1, pageSize: 30 },
    })
    userSearchResults.value = response.data.items || []
    userSearchTotalPages.value = response.data.totalPages || 0
    userSearchTotalCount.value = response.data.totalCount || 0
    userSearchPage.value = 1
  } catch (error) {
    console.error('Failed to load users:', error)
  } finally {
    userSearchLoading.value = false
  }
}

// 사용자 검색 (debounce)
const onSearchInput = () => {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    userSearchPage.value = 1
    searchUsers()
  }, 300)
}

const searchUsers = async () => {
  const term = userSearchTerm.value.trim()

  userSearchLoading.value = true
  try {
    const params = { page: userSearchPage.value, pageSize: 30 }
    if (term) params.searchTerm = term

    const response = await apiClient.get('/admin/users', { params })
    userSearchResults.value = response.data.items || []
    userSearchTotalPages.value = response.data.totalPages || 0
    userSearchTotalCount.value = response.data.totalCount || 0
  } catch (error) {
    console.error('Failed to search users:', error)
    userSearchResults.value = []
  } finally {
    userSearchLoading.value = false
  }
}

const goToPage = (page) => {
  userSearchPage.value = page
  searchUsers()
}

const isUserSelected = (userId) => {
  return selectedUsers.value.some((u) => u.id === userId)
}

const toggleUserSelection = (user) => {
  const idx = selectedUsers.value.findIndex((u) => u.id === user.id)
  if (idx >= 0) {
    selectedUsers.value.splice(idx, 1)
  } else {
    selectedUsers.value.push({ ...user })
  }
}

const removeSelectedUser = (userId) => {
  selectedUsers.value = selectedUsers.value.filter((u) => u.id !== userId)
}

const linkExistingUsers = async () => {
  if (selectedUsers.value.length === 0) return

  linkLoading.value = true
  try {
    const response = await apiClient.post(
      `/admin/conventions/${props.conventionId}/guests/link`,
      {
        userIds: selectedUsers.value.map((u) => u.id),
        groupName: linkGroupName.value || null,
      },
    )
    alert(response.data.message)
    resetExistingUserForm()
    emit('saved')
  } catch (error) {
    console.error('Failed to link users:', error)
    alert('추가 실패: ' + (error.response?.data?.message || error.message))
  } finally {
    linkLoading.value = false
  }
}

const resetExistingUserForm = () => {
  userSearchTerm.value = ''
  userSearchResults.value = []
  userSearchPage.value = 1
  userSearchTotalPages.value = 0
  userSearchTotalCount.value = 0
  selectedUsers.value = []
  linkGroupName.value = ''
}

// editingGuest 변경 시 폼 초기화
watch(
  () => props.editingGuest,
  (guest) => {
    if (guest) {
      activeTab.value = 'manual'
      const templateAttrs = {}
      const customAttrs = []
      const templateKeys = props.attributeTemplates.map((t) => t.attributeKey)

      guest.attributes.forEach((attr) => {
        if (templateKeys.includes(attr.attributeKey)) {
          templateAttrs[attr.attributeKey] = attr.attributeValue
        } else {
          customAttrs.push({
            key: attr.attributeKey,
            value: attr.attributeValue,
          })
        }
      })

      guestForm.value = {
        guestName: guest.guestName,
        telephone: guest.telephone,
        corpPart: guest.corpPart || '',
        residentNumber: guest.residentNumber || '',
        affiliation: guest.affiliation || '',
        password: '',
        scheduleTemplateIds: guest.scheduleTemplates.map(
          (st) => st.scheduleTemplateId,
        ),
        templateAttributes: templateAttrs,
        customAttributes: customAttrs,
      }
    }
  },
)

// 모달이 열릴 때 새 참석자인 경우 폼 초기화
watch(
  () => props.isOpen,
  (isOpen) => {
    if (isOpen && !props.editingGuest) {
      resetForm()
      resetExistingUserForm()
      activeTab.value = 'manual'
    }
  },
)

const resetForm = () => {
  guestForm.value = {
    guestName: '',
    telephone: '',
    corpPart: '',
    residentNumber: '',
    affiliation: '',
    password: '',
    scheduleTemplateIds: [],
    templateAttributes: {},
    customAttributes: [],
  }
}

const closeGuestModal = () => {
  resetForm()
  resetExistingUserForm()
  emit('close')
}

const saveGuest = async () => {
  try {
    // 템플릿 + 커스텀 속성 병합
    const attributes = {}

    // 템플릿 속성
    Object.entries(guestForm.value.templateAttributes).forEach(
      ([key, value]) => {
        if (value) {
          attributes[key] = value
        }
      },
    )

    // 커스텀 속성
    guestForm.value.customAttributes.forEach((attr) => {
      if (attr.key && attr.value) {
        attributes[attr.key] = attr.value
      }
    })

    const data = {
      name: guestForm.value.guestName,
      phone: guestForm.value.telephone,
      corpPart: guestForm.value.corpPart,
      residentNumber: guestForm.value.residentNumber,
      affiliation: guestForm.value.affiliation,
      password: guestForm.value.password,
      attributes: Object.keys(attributes).length > 0 ? attributes : null,
    }

    let guestId
    if (props.editingGuest) {
      await apiClient.put(`/admin/guests/${props.editingGuest.id}`, data)
      guestId = props.editingGuest.id
    } else {
      const response = await apiClient.post(
        `/admin/conventions/${props.conventionId}/guests`,
        data,
      )
      guestId = response.data.id
    }

    // 일정 배정
    await apiClient.post(
      `/admin/conventions/${props.conventionId}/guests/${guestId}/schedules`,
      {
        scheduleTemplateIds: guestForm.value.scheduleTemplateIds,
      },
    )

    resetForm()
    emit('saved')
  } catch (error) {
    console.error('Failed to save guest:', error)
    alert('저장 실패: ' + (error.response?.data?.message || error.message))
  }
}

const copyScheduleFromGuest = (guest) => {
  if (guest.scheduleTemplates.length === 0) {
    alert('이 참석자는 배정된 일정이 없습니다.')
    return
  }

  guestForm.value.scheduleTemplateIds = guest.scheduleTemplates.map(
    (st) => st.scheduleTemplateId,
  )
  showCopyScheduleModal.value = false
  searchQuery.value = ''
  alert(
    `${guest.guestName}님의 일정 ${guest.scheduleTemplates.length}개를 복사했습니다.`,
  )
}

const closeCopyScheduleModal = () => {
  showCopyScheduleModal.value = false
  searchQuery.value = ''
}
</script>
