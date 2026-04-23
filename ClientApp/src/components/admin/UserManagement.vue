<template>
  <div class="space-y-4 md:space-y-6">
    <AdminPageHeader title="회원 관리" :description="`전체 ${totalCount}명`" />

    <!-- 검색 -->
    <div>
      <input
        v-model.lazy="searchTerm"
        type="text"
        placeholder="이름, 아이디, 전화번호 검색..."
        class="block w-full px-3 py-2.5 bg-white border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-primary-500 focus:border-primary-500 text-sm"
      />
    </div>

    <!-- 로딩 -->
    <div v-if="loading" class="text-center py-12 text-gray-400">로딩 중...</div>

    <!-- 빈 상태 -->
    <div v-else-if="users.length === 0" class="text-center py-12 text-gray-400">
      <Users class="w-10 h-10 mx-auto mb-2 text-gray-300" />
      <p>사용자가 없습니다.</p>
    </div>

    <template v-else>
      <!-- 모바일: 카드 리스트 -->
      <div class="space-y-3 md:hidden">
        <div
          v-for="user in users"
          :key="user.id"
          class="bg-white rounded-xl border shadow-sm p-4"
        >
          <div class="flex items-start justify-between mb-2">
            <div>
              <p class="font-semibold text-gray-900">{{ user.name }}</p>
              <p class="text-xs text-gray-500">
                {{ user.loginId || '게스트' }} · {{ user.phone || '-' }}
              </p>
            </div>
            <div class="flex items-center gap-1.5">
              <span
                class="px-2 py-0.5 text-xs rounded-full font-medium"
                :class="
                  user.role === 'Admin'
                    ? 'bg-blue-100 text-blue-700'
                    : user.role === 'Guest'
                      ? 'bg-gray-100 text-gray-600'
                      : 'bg-green-100 text-green-700'
                "
              >
                {{ user.role }}
              </span>
              <span
                class="px-2 py-0.5 text-xs rounded-full font-medium"
                :class="
                  user.isActive
                    ? 'bg-green-100 text-green-700'
                    : 'bg-red-100 text-red-700'
                "
              >
                {{ user.isActive ? '활성' : '비활성' }}
              </span>
            </div>
          </div>
          <div class="flex items-center justify-between">
            <p class="text-xs text-gray-400">
              {{ formatDate(user.createdAt) }}
            </p>
            <div class="flex gap-3">
              <button
                class="text-sm text-primary-600 font-medium"
                @click="openRoleModal(user)"
              >
                권한
              </button>
              <button
                class="text-sm text-red-600 font-medium"
                @click="openStatusModal(user)"
              >
                상태
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- PC: 테이블 -->
      <div class="hidden md:block">
        <AdminTable :columns="tableColumns" :loading="false" :empty="false">
          <tr v-for="user in users" :key="user.id" class="hover:bg-gray-50">
            <td class="px-6 py-4 text-sm text-gray-500">{{ user.id }}</td>
            <td
              class="px-6 py-4 text-sm font-medium text-gray-900 whitespace-nowrap"
            >
              {{ user.name }}
            </td>
            <td class="px-6 py-4 text-sm text-gray-500">
              {{ user.loginId || '-' }}
            </td>
            <td class="px-6 py-4 text-sm text-gray-500">{{ user.phone }}</td>
            <td class="px-6 py-4">
              <AdminBadge
                :variant="
                  user.role === 'Admin'
                    ? 'info'
                    : user.role === 'Guest'
                      ? 'neutral'
                      : 'success'
                "
              >
                {{ user.role }}
              </AdminBadge>
            </td>
            <td class="px-6 py-4">
              <AdminBadge :variant="user.isActive ? 'success' : 'danger'">
                {{ user.isActive ? '활성' : '비활성' }}
              </AdminBadge>
            </td>
            <td class="px-6 py-4 text-sm text-gray-500">
              {{ formatDate(user.createdAt) }}
            </td>
            <td class="px-6 py-4 text-sm text-gray-500">
              {{ formatDate(user.updatedAt) }}
            </td>
            <td class="px-6 py-4 text-right">
              <div class="flex justify-end space-x-2">
                <button
                  class="text-sm text-primary-600 hover:underline"
                  @click="openRoleModal(user)"
                >
                  권한
                </button>
                <button
                  class="text-sm text-red-600 hover:underline"
                  @click="openStatusModal(user)"
                >
                  상태
                </button>
              </div>
            </td>
          </tr>
        </AdminTable>
      </div>
    </template>

    <!-- Pagination -->
    <nav
      v-if="totalPages > 1"
      class="flex flex-col sm:flex-row items-center justify-between gap-3 pt-2"
    >
      <span class="text-sm text-gray-500">
        총 <span class="font-semibold text-gray-900">{{ totalCount }}</span
        >명 중
        <span class="font-semibold text-gray-900"
          >{{ (page - 1) * pageSize + 1 }} -
          {{ Math.min(page * pageSize, totalCount) }}</span
        >
      </span>
      <div class="flex -space-x-px text-sm">
        <button
          :disabled="page === 1"
          class="px-3 h-9 border border-gray-300 bg-white text-gray-500 hover:bg-gray-100 rounded-l-lg disabled:opacity-50"
          @click="changePage(page - 1)"
        >
          이전
        </button>
        <button
          v-for="p in paginationRange"
          :key="p"
          class="px-3 h-9 border border-gray-300"
          :class="
            p === page
              ? 'bg-primary-50 text-primary-600 border-primary-300 z-10'
              : 'bg-white text-gray-500 hover:bg-gray-100'
          "
          @click="changePage(p)"
        >
          {{ p }}
        </button>
        <button
          :disabled="page === totalPages"
          class="px-3 h-9 border border-gray-300 bg-white text-gray-500 hover:bg-gray-100 rounded-r-lg disabled:opacity-50"
          @click="changePage(page + 1)"
        >
          다음
        </button>
      </div>
    </nav>

    <!-- Role Modal -->
    <BaseModal :is-open="showRoleModal" max-width="md" @close="closeModal">
      <template #header>
        <h3 class="text-lg font-semibold">
          역할 변경: {{ selectedUser?.name }}
        </h3>
      </template>
      <template #body>
        <p class="mb-4 text-sm text-gray-600">
          현재 역할: {{ selectedUser?.role }}
        </p>
        <select
          v-model="newRole"
          class="w-full px-3 py-2.5 border rounded-lg text-sm"
        >
          <option value="Admin">Admin</option>
          <option value="User">User</option>
        </select>
      </template>
      <template #footer>
        <button
          class="px-4 py-2 text-sm border rounded-lg hover:bg-gray-50"
          @click="closeModal"
        >
          취소
        </button>
        <button
          class="px-4 py-2 text-sm bg-primary-600 text-white rounded-lg hover:bg-primary-700"
          @click="updateRole"
        >
          변경
        </button>
      </template>
    </BaseModal>

    <!-- Status Modal -->
    <BaseModal :is-open="showStatusModal" max-width="md" @close="closeModal">
      <template #header>
        <h3 class="text-lg font-semibold">
          상태 변경: {{ selectedUser?.name }}
        </h3>
      </template>
      <template #body>
        <p class="text-sm text-gray-600">
          정말로 이 사용자를
          <span class="font-bold">{{
            selectedUser?.isActive ? '비활성' : '활성'
          }}</span>
          상태로 변경하시겠습니까?
        </p>
      </template>
      <template #footer>
        <button
          class="px-4 py-2 text-sm border rounded-lg hover:bg-gray-50"
          @click="closeModal"
        >
          취소
        </button>
        <button
          class="px-4 py-2 text-sm bg-red-600 text-white rounded-lg hover:bg-red-700"
          @click="updateStatus"
        >
          {{ selectedUser?.isActive ? '비활성화' : '활성화' }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, computed } from 'vue'
import apiClient from '@/services/api'
import dayjs from 'dayjs'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminTable from '@/components/admin/ui/AdminTable.vue'
import AdminBadge from '@/components/admin/ui/AdminBadge.vue'
import BaseModal from '@/components/common/BaseModal.vue'
import { Users } from 'lucide-vue-next'

const tableColumns = [
  { key: 'id', label: 'ID', width: '60px' },
  { key: 'name', label: '이름' },
  { key: 'loginId', label: '로그인 ID' },
  { key: 'phone', label: '전화번호' },
  { key: 'role', label: '역할' },
  { key: 'status', label: '상태' },
  { key: 'createdAt', label: '생성일' },
  { key: 'updatedAt', label: '수정일' },
  { key: 'actions', label: '작업', align: 'right' },
]

const users = ref([])
const loading = ref(false)
const searchTerm = ref('')
const page = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)
const totalPages = ref(1)

const selectedUser = ref(null)
const showRoleModal = ref(false)
const showStatusModal = ref(false)
const newRole = ref('')

const fetchUsers = async () => {
  loading.value = true
  try {
    const response = await apiClient.get('/admin/users', {
      params: {
        searchTerm: searchTerm.value,
        page: page.value,
        pageSize: pageSize.value,
      },
    })
    users.value = response.data.items
    totalCount.value = response.data.totalCount
    totalPages.value = response.data.totalPages
  } catch (error) {
    console.error('Failed to fetch users:', error)
  } finally {
    loading.value = false
  }
}

watch(searchTerm, () => {
  page.value = 1
  fetchUsers()
})

const changePage = (newPage) => {
  if (newPage > 0 && newPage <= totalPages.value) {
    page.value = newPage
    fetchUsers()
  }
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  return dayjs(dateString).format('YYYY-MM-DD HH:mm')
}

const paginationRange = computed(() => {
  const range = []
  const total = totalPages.value
  const current = page.value
  const delta = 2
  for (let i = 1; i <= total; i++) {
    if (
      i === 1 ||
      i === total ||
      (i >= current - delta && i <= current + delta)
    )
      range.push(i)
  }
  const result = []
  let last
  for (const i of range) {
    if (last && i - last > 1) result.push(last + 1 === i ? last + 1 : -1)
    result.push(i)
    last = i
  }
  return result.filter((p) => p > 0)
})

const closeModal = () => {
  showRoleModal.value = false
  showStatusModal.value = false
  selectedUser.value = null
}
const openRoleModal = (user) => {
  selectedUser.value = { ...user }
  newRole.value = user.role
  showRoleModal.value = true
}
const openStatusModal = (user) => {
  selectedUser.value = { ...user }
  showStatusModal.value = true
}

const updateRole = async () => {
  if (!selectedUser.value) return
  try {
    await apiClient.put(`/admin/users/${selectedUser.value.id}/role`, {
      role: newRole.value,
    })
    alert('역할이 변경되었습니다.')
    closeModal()
    fetchUsers()
  } catch {
    alert('역할 변경 실패')
  }
}

const updateStatus = async () => {
  if (!selectedUser.value) return
  try {
    await apiClient.put(`/admin/users/${selectedUser.value.id}/status`, {
      isActive: !selectedUser.value.isActive,
    })
    alert('상태가 변경되었습니다.')
    closeModal()
    fetchUsers()
  } catch {
    alert('상태 변경 실패')
  }
}

onMounted(fetchUsers)
</script>
